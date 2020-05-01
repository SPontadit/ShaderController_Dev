using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BasePostProcessController), true)]
[CanEditMultipleObjects]
public class BasePostProcessControllerEditor : Editor
{
	SerializedProperty shaderProperty;
	SerializedProperty materialProperty;

	BasePostProcessController postProcessController;
	Shader shader;
	Material material;

	private void OnEnable()
	{
		shaderProperty = serializedObject.FindProperty("shader");
		materialProperty = serializedObject.FindProperty("material");

		shader = shaderProperty.objectReferenceValue as Shader;
		material = materialProperty.objectReferenceValue as Material;

		postProcessController = target as BasePostProcessController;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();

		if (GUILayout.Button("Update Controller"))
		{
			string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(postProcessController));

			ShaderControllerGeneratorInterface.GeneratePostProcessController(shader, scriptPath);

			Debug.Log($"[Shader Controller] {Path.GetFileName(scriptPath)} updated");
		}

		EditorGUILayout.Space();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(shaderProperty);
		GUI.enabled = true;

		DrawDefaultInspector();
	}
}
