using UnityEngine;

[ExecuteInEditMode]
public abstract class BaseShaderController : MonoBehaviour
{
	[HideInInspector]
	[SerializeField] protected bool isPostProcessController = false;

	[HideInInspector]
	[SerializeField] protected Shader shader;
	
	[HideInInspector]
	[SerializeField] protected Material material;


	protected virtual void Start()
	{
		SetupMaterial();
	}

	protected virtual void Reset()
	{
		Start();
	}

	public virtual void SetupMaterial()
	{
		if (material == null)
		{
			material = new Material(shader);
			string originalName = shader.name;
			material.name = originalName + " - Tmp";
			material.hideFlags = HideFlags.DontSave;
		}

		SetupInspectorValue();
	}

	public abstract void SetupInspectorValue();


	protected void OnValidate()
	{
		//Start();
	}
}