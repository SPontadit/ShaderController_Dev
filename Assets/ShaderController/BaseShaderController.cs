using UnityEngine;

[ExecuteInEditMode]
public abstract class BaseShaderController : MonoBehaviour
{

	[HideInInspector]
	[SerializeField] protected Shader shader;
	
	[HideInInspector]
	[SerializeField] protected Material material;

#if UNITY_EDITOR
	[HideInInspector]
	[SerializeField] protected bool allowMaterialOverride = false;

	[HideInInspector]
	[SerializeField] protected bool isPostProcessController = false;
#endif

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
			material.name = originalName + " - Virtual";
			material.hideFlags = HideFlags.DontSave;
		}

		SetupInspectorValue();
	}

	public abstract void SetupInspectorValue();
}