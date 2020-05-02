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

	BaseShaderController shaderController;
	bool useMaterialCopy;
	bool overrideMaterial;
	
	private void OnEnable()
	{
		shaderProperty = serializedObject.FindProperty("shader");
		materialProperty = serializedObject.FindProperty("material");
		isPostProcessControllerProperty = serializedObject.FindProperty("isPostProcessController");

		shaderController = target as BaseShaderController;
		shaderController.SetupInspectorValue();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();

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

		EditorGUILayout.Space();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(shaderProperty);
		GUI.enabled = true;

		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();

		overrideMaterial = EditorGUILayout.Toggle(new GUIContent("Override Material", ""), overrideMaterial);

		bool overrideMaterialHasChanged = EditorGUI.EndChangeCheck();
		bool materialHasChanged = false;
		bool useMaterialCopyHasChanged = false;


		if (overrideMaterial)
		{
			overrideMaterialHasChanged = false;

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(materialProperty);

			materialHasChanged = EditorGUI.EndChangeCheck();

			EditorGUI.BeginChangeCheck();

			Material material = materialProperty.objectReferenceValue as Material;

			if (material && ((material.hideFlags & HideFlags.DontSave) != HideFlags.DontSave))
				useMaterialCopy = EditorGUILayout.Toggle(new GUIContent("Use Material Copy", "When enabled, create a new material based on the one in the material field."), useMaterialCopy);

			useMaterialCopyHasChanged = EditorGUI.EndChangeCheck();
		}

		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();

		base.OnInspectorGUI();

		if (materialHasChanged)
		{
			shaderController.SetupMaterial();
		}
		else if (useMaterialCopyHasChanged)
		{
			SetupMaterialCopy();
		}
		else if (overrideMaterialHasChanged)
		{
			ResetMaterial();
		}

		serializedObject.ApplyModifiedProperties();
	}

	public void SetupMaterialCopy()
	{
		Material material = materialProperty.objectReferenceValue as Material;

		string originalName = material.name;
		material = new Material(material);
		material.name = originalName + " - Copy";
		material.hideFlags = HideFlags.DontSave;

		materialProperty.objectReferenceValue = material;

		useMaterialCopy = false;

		if (isPostProcessControllerProperty.boolValue == false)
			shaderController.GetComponent<Renderer>().material = material;
	}

	public void ResetMaterial()
	{
		Material material = materialProperty.objectReferenceValue as Material;
		Shader shader = shaderProperty.objectReferenceValue as Shader;

		material = new Material(material);
		string originalName = shader.name;
		material.name = originalName + " - Tmp";
		material.hideFlags = HideFlags.DontSave;

		materialProperty.objectReferenceValue = material;

		if (isPostProcessControllerProperty.boolValue == false)
			shaderController.GetComponent<Renderer>().material = material;
	}
}
