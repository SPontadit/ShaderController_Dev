using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseShaderController), true)]
[CanEditMultipleObjects]
public class BaseShaderControllerEditor : Editor
{
	SerializedProperty shaderProperty;
	SerializedProperty materialProperty;
	SerializedProperty useMaterialCopyProperty;

	BaseShaderController shaderController;
	Shader shader;
	Material material;

	private void OnEnable()
	{
		shaderProperty = serializedObject.FindProperty("shader");
		materialProperty = serializedObject.FindProperty("material");
		useMaterialCopyProperty = serializedObject.FindProperty("useMaterialCopy");

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

			// LOG: Update Shader Ctrl
		}

		EditorGUILayout.Space();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(shaderProperty);
		GUI.enabled = true;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(materialProperty);

		bool materialHasChanged = EditorGUI.EndChangeCheck();

		EditorGUI.BeginChangeCheck();


		if ((material.hideFlags & HideFlags.DontSave) != HideFlags.DontSave)
			EditorGUILayout.PropertyField(useMaterialCopyProperty);

		bool useMaterialCopyaterialHasChanged = EditorGUI.EndChangeCheck();

		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();

		//DrawDefaultInspector();
		base.OnInspectorGUI();

		if (materialHasChanged || useMaterialCopyaterialHasChanged)
		{
			shaderController.SetupMaterial();
		}
	}
}
