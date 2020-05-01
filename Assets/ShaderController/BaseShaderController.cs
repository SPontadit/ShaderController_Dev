using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteInEditMode]
public abstract class BaseShaderController : MonoBehaviour
{
	[HideInInspector]
	[SerializeField] protected Shader shader;
	
	[HideInInspector]
	[SerializeField] protected Material material;

	[HideInInspector]
	[Tooltip("When enabled, create a new material based on the one in the material field. Don't do anything if it's use with default material")]
	[SerializeField] protected bool useMaterialCopy;

	[HideInInspector]
	[SerializeField] protected bool overrideMaterial;


	protected virtual void Start()
	{
		SetupMaterial();
	}

	public void SetupMaterial()
	{
		if (material == null)
		{
			material = new Material(shader);
			string originalName = material.name;
			material.name = originalName + " - Tmp";
			material.hideFlags = HideFlags.DontSave;
		}
		else if (useMaterialCopy)
		{
			string originalName = material.name;
			material = new Material(material);
			material.name = originalName + " - Copy";
			material.hideFlags = HideFlags.DontSave;
		}

		useMaterialCopy = false;

		SetupInspectorValue();

		GetComponent<Renderer>().material = material;
	}

	public abstract void SetupInspectorValue();

	protected void OnValidate()
	{
		//Start();
	}
}