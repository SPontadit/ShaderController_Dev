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

public sealed class ShaderControllerGeneratorInterface
{
	[MenuItem("Assets/Create Shader Controller")]
	public static void CreateShaderControllerFromShaderAsset()
	{
		Object selected = Selection.activeObject;
		Shader shader = selected as Shader;

		if (shader == null)
		{
			// LOG USE ON SHADER ONLY
			return;
		}

		string outputPath = GetOutputPath(shader);

		if (string.IsNullOrEmpty(outputPath))
		{
			// LOG Bad Path

			return;
		}

		GenerateShaderController(shader, outputPath);

		// LOG Shader Generation Success
	}

	[MenuItem("Assets/Create PostProcess Controller")]
	public static void CreatePostProcessControllerFromShaderAsset()
	{
		Object selected = Selection.activeObject;
		Shader shader = selected as Shader;
		
		if (shader == null)
		{
			// LOG USE ON SHADER ONLY
			return;
		}


		string outputPath = GetOutputPath(shader);
		
		if (string.IsNullOrEmpty(outputPath))
		{
			// LOG Bad Path

			return;
		}

		GeneratePostProcessController(shader, outputPath);

		// LOG PostProcess Generation Success
	}

	private static string GetOutputPath(Shader shader)
	{
		string shaderName = shader.name.Split('/').Last();
		string shaderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(shader));

		return EditorUtility.SaveFilePanel("Save Location", shaderPath, shaderName + "Controller", "cs");
	}

	public static void GenerateShaderController(Shader shader, string outputPath)
	{
		string shaderName = shader.name.Split('/').Last();

		ShaderControllerGenerator generator = new ShaderControllerGenerator();

		generator.Session = new Dictionary<string, object>();

		generator.Session["ShaderName"] = shaderName;
		generator.Session["ShaderPathInternal"] = shader.name;

		generator.Session["Properties"] = GetShaderPropertyInfos(shader);

		generator.Initialize();

		string classDefinition = generator.TransformText();

		File.WriteAllText(outputPath, classDefinition);

		AssetDatabase.Refresh();
	}

	public static void GeneratePostProcessController(Shader shader, string outputPath)
	{
		string shaderName = shader.name.Split('/').Last();

		PostProcessControllerGenerator generator = new PostProcessControllerGenerator();

		generator.Session = new Dictionary<string, object>();

		generator.Session["ShaderName"] = shaderName;
		generator.Session["ShaderPathInternal"] = shader.name;

		generator.Session["Properties"] = GetShaderPropertyInfos(shader);

		generator.Initialize();

		string classDefinition = generator.TransformText();

		File.WriteAllText(outputPath, classDefinition);

		AssetDatabase.Refresh();
	}

	private static PropertyInfo[] GetShaderPropertyInfos(Shader shader)
	{
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
			descriptionAsName = descriptionAsName.Replace(" ", string.Empty);
			info.descriptionAsName = System.Char.ToLower(descriptionAsName[0]) + descriptionAsName.Substring(1);
			info.shaderType = shader.GetPropertyType(i);
			switch (info.shaderType)
			{
				case ShaderPropertyType.Color:
					Vector4 color = shader.GetPropertyDefaultVectorValue(i);
					info.values = new float[4] { color.x, color.y, color.z, color.w };

					if ((shader.GetPropertyFlags(i) & ShaderPropertyFlags.HDR) == ShaderPropertyFlags.HDR)
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

		return propertyInfos.ToArray();
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