using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BaseShaderController), true)]
[CanEditMultipleObjects]
public class BaseShaderControllerEditor : Editor
{
	SerializedProperty shaderProperty;
	SerializedProperty materialProperty;
	SerializedProperty useMaterialCopyProperty;
	SerializedProperty overrideMaterialProperty;

	BaseShaderController shaderController;
	Shader shader;
	Material material;

	private void OnEnable()
	{
		shaderProperty = serializedObject.FindProperty("shader");
		materialProperty = serializedObject.FindProperty("material");
		useMaterialCopyProperty = serializedObject.FindProperty("useMaterialCopy");
		overrideMaterialProperty = serializedObject.FindProperty("overrideMaterial");

		shader = shaderProperty.objectReferenceValue as Shader;
		material = materialProperty.objectReferenceValue as Material;

		shaderController = target as BaseShaderController;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();

		if (GUILayout.Button("Update Controller"))
		{
			string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(shaderController));
			
			ShaderControllerGeneratorInterface.GenerateShaderController(shader, scriptPath);

			Debug.Log($"[Shader Controller] {Path.GetFileName(scriptPath)} updated");
		}

		EditorGUILayout.Space();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(shaderProperty);
		GUI.enabled = true;

		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(overrideMaterialProperty);

		bool overrideMaterialChanged = EditorGUI.EndChangeCheck();
		bool materialHasChanged = false;
		bool useMaterialCopyaterialHasChanged = false;


		if (overrideMaterialProperty.boolValue)
		{
			overrideMaterialChanged = false;

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(materialProperty);

			materialHasChanged = EditorGUI.EndChangeCheck();

			EditorGUI.BeginChangeCheck();

			if ((material.hideFlags & HideFlags.DontSave) != HideFlags.DontSave)
				EditorGUILayout.PropertyField(useMaterialCopyProperty);

			useMaterialCopyaterialHasChanged = EditorGUI.EndChangeCheck();
		}
		else
		{
			if (overrideMaterialChanged)
				materialProperty.objectReferenceValue = null;
		}

		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();

		//DrawDefaultInspector();
		base.OnInspectorGUI();

		if (materialHasChanged || useMaterialCopyaterialHasChanged || overrideMaterialChanged)
		{
			shaderController.SetupMaterial();
		}
	}
}
