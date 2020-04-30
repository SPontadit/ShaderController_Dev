using Assets.ShaderController.Editor.Templates;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Text.RegularExpressions;

public class PropertyInfo
{
	public ShaderPropertyType shaderType;
	public string name;
	public string description;
	public string descriptionAsName;
	public string attribute;
	public float[] values;
	public bool isDecorator = false;
}

public partial class ShaderControllerGeneratorInterface : ShaderControllerGenerator
{
	public static void OLD_GenerateShaderController()
	{
		Object selected = Selection.activeObject;

		Shader shader = selected as Shader;
		if (shader == null)
		{
			// LOG_WARNING USE ON SHADER ONLY
			return;
		}

		string shaderName = shader.name.Split('/').Last();

		Debug.Log("Shader Name: " + shaderName);

		string outputPath = EditorUtility.SaveFilePanelInProject("Save Location", shaderName + "Controller", "cs", "Where do you want to save the script");

		ShaderControllerGenerator generator = new ShaderControllerGenerator();

		generator.Session = new Dictionary<string, object>();

		generator.Session["ShaderName"] = shaderName;
		generator.Session["ShaderPathInternal"] = shader.name;

		int propertyCount = ShaderUtil.GetPropertyCount(shader);


		List<string> colorPropertyNames = new List<string>();
		List<string> colorPropertyDescripions = new List<string>();

		List<float> floatPropertyValues = new List<float>();
		List<string> floatPropertyNames = new List<string>();
		List<string> floatPropertyDescriptions = new List<string>();

		List<List<float>> vectorPropertyValues = new List<List<float>>();
		List<string> vectorPropertyNames = new List<string>();
		List<string> vectorPropertyDescriptions = new List<string>();

		string propertyName = "";
		string propertyDescription = "";

		for (int i = 0; i < propertyCount; i++)
		{
			if ((shader.GetPropertyFlags(i) & ShaderPropertyFlags.HideInInspector) == ShaderPropertyFlags.HideInInspector)
				continue;

			propertyName = shader.GetPropertyName(i);
			propertyDescription = shader.GetPropertyDescription(i);

			//string[] ss = shader.GetPropertyAttributes(i);
			//for (int j = 0; j < ss.Length; j++)
			//{
			//	Debug.Log(shader.GetPropertyType(i) + " " + propertyName + " " + ss[j]);
			//	Debug.Log(shader.GetPropertyDescription(i) + " " + propertyName + " " + ss[j]);
			//}

			switch (shader.GetPropertyType(i))
			{
				case ShaderPropertyType.Color:
					colorPropertyNames.Add(propertyName);
					colorPropertyDescripions.Add(propertyDescription);
					break;
				case ShaderPropertyType.Vector:
					vectorPropertyNames.Add(propertyName);
					vectorPropertyDescriptions.Add(propertyDescription);

					Vector4 value = shader.GetPropertyDefaultVectorValue(i);
					vectorPropertyValues.Add(new List<float>() { value.x, value.y, value.z, value.w });
					break;
				case ShaderPropertyType.Float:
					floatPropertyNames.Add(propertyName);
					floatPropertyDescriptions.Add(propertyDescription);
					floatPropertyValues.Add(shader.GetPropertyDefaultFloatValue(i));
					break;
				case ShaderPropertyType.Range:
					break;
				default:
					break;
			}
		}



		generator.Session["FloatPropertyNames"] = floatPropertyNames.ToArray();
		generator.Session["FloatPropertyDescriptions"] = floatPropertyDescriptions.ToArray();
		generator.Session["FloatPropertyValues"] = floatPropertyValues.ToArray();
		generator.Session["ColorPropertyNames"] = colorPropertyNames.ToArray();
		generator.Session["ColorPropertyDescriptions"] = colorPropertyDescripions.ToArray();
		generator.Session["VectorPropertyNames"] = vectorPropertyNames.ToArray();
		generator.Session["VectorPropertyDescriptions"] = vectorPropertyDescriptions.ToArray();
		generator.Session["VectorPropertyValues"] = vectorPropertyValues.Select(a => a.ToArray()).ToArray();

		generator.Initialize();

		string classDefinition = generator.TransformText();

		File.WriteAllText(outputPath, classDefinition);

		AssetDatabase.Refresh();
	}

	[MenuItem("Assets/Create Shader Controller")]
	public static void CreateShaderControllerFromShaderAsset()
	{
		Object selected = Selection.activeObject;

		Shader shader = selected as Shader;
		if (shader == null)
		{
			// LOG_WARNING USE ON SHADER ONLY
			return;
		}

		string shaderName = shader.name.Split('/').Last();

		string outputPath = EditorUtility.SaveFilePanelInProject("Save Location", shaderName + "Controller", "cs", "Where do you want to save the script");

		if (string.IsNullOrEmpty(outputPath))
		{
			return;
		}

		GenerateShaderController(shader, outputPath);
	}

	public static void GenerateShaderController(Shader shader, string outputPath)
	{
		string shaderName = shader.name.Split('/').Last();

		ShaderControllerGenerator generator = new ShaderControllerGenerator();

		generator.Session = new Dictionary<string, object>();

		generator.Session["ShaderName"] = shaderName;
		generator.Session["ShaderPathInternal"] = shader.name;

		int propertyCount = ShaderUtil.GetPropertyCount(shader);

		List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

		string propertyName = "";
		string propertyDescription = "";
		string[] propertyAttributes;

		for (int i = 0; i < propertyCount; i++)
		{

			if ((shader.GetPropertyFlags(i) & ShaderPropertyFlags.HideInInspector) == ShaderPropertyFlags.HideInInspector)
				continue;

			propertyName = shader.GetPropertyName(i);
			propertyDescription = shader.GetPropertyDescription(i);
			propertyAttributes = shader.GetPropertyAttributes(i);

			bool isIntRange = false;

			for (int j = 0; j < propertyAttributes.Length; j++)
			{
				if (isIntRange == false)
				{
					isIntRange = propertyAttributes[j].Equals("IntRange");
				}

				string decorator = ProcessDecorator(propertyAttributes[j], j);
				if (string.IsNullOrEmpty(decorator) == false)
				{
					PropertyInfo attributeInfo = new PropertyInfo();
					attributeInfo.attribute = decorator;
					attributeInfo.isDecorator = true;
					propertyInfos.Add(attributeInfo);
				}
			}

			PropertyInfo info = new PropertyInfo();
			info.name = propertyName;
			info.description = propertyDescription;
			string descriptionAsName = Regex.Replace(propertyDescription, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
			info.descriptionAsName = descriptionAsName.Replace(" ", string.Empty);
			info.shaderType = shader.GetPropertyType(i);
			switch (info.shaderType)
			{
				case ShaderPropertyType.Color:
					Vector4 color = shader.GetPropertyDefaultVectorValue(i);
					info.values = new float[4] { color.x, color.y, color.z, color.w };

					if((shader.GetPropertyFlags(i) & ShaderPropertyFlags.HDR) == ShaderPropertyFlags.HDR)
					{
						info.attribute = "[ColorUsage(true, true)]";
					}
					break;
				case ShaderPropertyType.Vector:
					Vector4 value = shader.GetPropertyDefaultVectorValue(i);
					info.values = new float[4] { value.x, value.y, value.z, value.w };
					break;
				case ShaderPropertyType.Float:
					info.values = new float[1] { shader.GetPropertyDefaultFloatValue(i) };
					break;
				case ShaderPropertyType.Range:
					Vector2 rangeLimits = shader.GetPropertyRangeLimits(i);
					info.values = new float[4] { shader.GetPropertyDefaultFloatValue(i), rangeLimits.x, rangeLimits.y, isIntRange ? 1.0f : 0.0f };
					break;
				default:
					break;
			}

			propertyInfos.Add(info);
		}


		generator.Session["Properties"] = propertyInfos.ToArray();


		generator.Initialize();

		string classDefinition = generator.TransformText();

		File.WriteAllText(outputPath, classDefinition);

		AssetDatabase.Refresh();
	}

	private static string ProcessDecorator(string attribute, int j)
	{
		if (attribute.StartsWith("Space"))
		{
			string spaceValue = new string(attribute.Where(System.Char.IsDigit).ToArray());
			return "[Space(" + spaceValue + ", order = " + j + ")]";
		}
		else if (attribute.StartsWith("Header"))
		{
			string headerValue = attribute.Split('(', ')')[1];
			return "[Header(\"" + headerValue + "\", order = " + j + ")]";
		}

		return "";
	}

	[MenuItem("ShaderController/Log Info")]
	public static void GetShaderInfo()
	{
		Object selected = Selection.activeObject;

		Shader shader = selected as Shader;
		if (shader == null)
		{
			// LOG_WARNING USE ON SHADER ONLY
			return;
		}

		int propertyCount = shader.GetPropertyCount();

		for (int i = 0; i < propertyCount; i++)
		{
			string[] attributes = shader.GetPropertyAttributes(i);
			string attributeStr = "";
			string name = shader.GetPropertyName(i);
			string description = shader.GetPropertyDescription(i);
			UnityEngine.Rendering.ShaderPropertyFlags flag = shader.GetPropertyFlags(i);



			//Debug.Log("Name: " + name);
			//Debug.Log("Description: " + description);
			//Debug.Log("Attributes: " + attributeStr);
			//Debug.Log("Flags: " + flag);

			switch (shader.GetPropertyType(i))
			{
				case ShaderPropertyType.Color:
					//Color v = shader.GetPropertyDefaultVectorValue(i);
					//Debug.Log("Color Value: " + v.r);
					//Debug.Log("Color Value: " + v.g);
					//Debug.Log("Color Value: " + v.b);
					//Debug.Log("Color Value: " + v.a);
					break;
				case ShaderPropertyType.Vector:
					break;
				case ShaderPropertyType.Float:
					for (int j = 0; j < attributes.Length; j++)
					{
						attributeStr += attributes[j];
						attributeStr += ", ";
					}
					Debug.Log("FLOAT Desc: " + shader.GetPropertyDescription(i));
					Debug.Log("FLOAT Value: " + shader.GetPropertyDefaultFloatValue(i));
					Debug.Log("Attributes: " + attributeStr);
					break;
				case ShaderPropertyType.Range:
					//Debug.Log("Range Value: " + shader.GetPropertyDefaultFloatValue(i));
					break;
				default:
					break;
			}

			//Debug.Log("------------------------");
		}
	}
}