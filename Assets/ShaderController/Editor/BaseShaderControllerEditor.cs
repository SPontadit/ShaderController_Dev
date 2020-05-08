using UnityEngine;
using UnityEditor;
using System.IO;


[CustomEditor(typeof(BaseShaderController), true)]
[CanEditMultipleObjects]
public class BaseShaderControllerEditor : Editor
{
	SerializedProperty shaderProperty;
	SerializedProperty materialProperty;
	SerializedProperty isPostProcessControllerProperty;
	SerializedProperty allowMaterialOverrideProperty;

	BaseShaderController shaderController;

	private void OnEnable()
	{
		shaderProperty = serializedObject.FindProperty("shader");
		materialProperty = serializedObject.FindProperty("material");
		isPostProcessControllerProperty = serializedObject.FindProperty("isPostProcessController");
		allowMaterialOverrideProperty = serializedObject.FindProperty("allowMaterialOverride");

		

		shaderController = target as BaseShaderController;
		shaderController.SetupInspectorValue();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();
		DrawUpdateControllerButton();

		GUI.enabled = false;
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(shaderProperty);
		GUI.enabled = true;

		EditorGUILayout.Space();

		if (allowMaterialOverrideProperty.boolValue == false)
			EditorGUILayout.HelpBox("Current material used is not saved. To be able to save or use a material, check the \"Allow Material Override\" toggle", MessageType.Info);

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(allowMaterialOverrideProperty);
		bool isVirtualWorkflowHasChanged = EditorGUI.EndChangeCheck();

		if (allowMaterialOverrideProperty.boolValue)
		{
			EditorGUILayout.Space();
			DrawSaveAndUseButton();
		}

		EditorGUI.BeginChangeCheck();
		GUI.enabled = allowMaterialOverrideProperty.boolValue;
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(materialProperty);
		GUI.enabled = true;
		bool materialHasChanged = EditorGUI.EndChangeCheck();


		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space();
		base.OnInspectorGUI();


		if (materialHasChanged)
		{
			shaderController.SetupMaterial();
		}
		else if (isVirtualWorkflowHasChanged && allowMaterialOverrideProperty.boolValue == false)
		{
			Material material = materialProperty.objectReferenceValue as Material;

			if (material && (material.hideFlags & HideFlags.DontSave) != HideFlags.DontSave)
				ResetMaterial();
		}
	}

	private void DrawUpdateControllerButton()
	{
		if (GUILayout.Button("Update Controller"))
		{
			string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(shaderController));

			Shader shader = shaderProperty.objectReferenceValue as Shader;

			if (isPostProcessControllerProperty.boolValue)
				ShaderControllerGeneratorInterface.GeneratePostProcessController(shader, scriptPath);
			else
				ShaderControllerGeneratorInterface.GenerateShaderController(shader, scriptPath);

			Debug.Log($"[Shader Controller] {Path.GetFileName(scriptPath)} updated");
		}

	}

	private void DrawSaveAndUseButton()
	{
		if (allowMaterialOverrideProperty.boolValue)
		{
			if (GUILayout.Button("Save and Use Material"))
			{
				Shader shader = shaderProperty.objectReferenceValue as Shader;
				Material material = materialProperty.objectReferenceValue as Material;
				bool isVirtualMaterial = (material.hideFlags & HideFlags.DontSave) == HideFlags.DontSave;

				string defaultName = string.Empty;
				string absPath = string.Empty;

				if (isVirtualMaterial)
				{
					string shaderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(shader));

					defaultName = shader.name.Substring(shader.name.LastIndexOf('/') + 1);
					absPath = EditorUtility.SaveFilePanel("Save material", shaderPath, defaultName, "mat");
				}
				else
				{
					string materialPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(material));

					defaultName = material.name + " - Copy";
					absPath = EditorUtility.SaveFilePanel("Save material", materialPath, defaultName, "mat");
				}

				if (string.IsNullOrEmpty(absPath) == false)
				{
					Material copy = new Material(materialProperty.objectReferenceValue as Material);
					string saveLocation = absPath.Substring(absPath.IndexOf("Assets/"));

					AssetDatabase.CreateAsset(copy, saveLocation);
					materialProperty.objectReferenceValue = copy;
				}
			}
		}
	}

	//TODO - Rename RestMaterial
	public void ResetMaterial()
	{
		Material material = materialProperty.objectReferenceValue as Material;
		Shader shader = shaderProperty.objectReferenceValue as Shader;

		material = new Material(material);
		string originalName = shader.name;
		material.name = originalName + " - Virtual";
		material.hideFlags = HideFlags.DontSave;

		materialProperty.objectReferenceValue = material;

		if (isPostProcessControllerProperty.boolValue == false)
			shaderController.GetComponent<Renderer>().material = material;

		serializedObject.ApplyModifiedProperties();
	}
}
