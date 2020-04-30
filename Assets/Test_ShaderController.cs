// Auto Generated code from ShaderControllerGenerator
using UnityEngine;

public class Test_ShaderController : BaseShaderController
{
	[Header("IntRange")]
	[Tooltip("This  my float")]
	[SerializeField] private float _MyFloat = 2.46f;

	[Tooltip("SrcBlend")]
	[SerializeField] private float _SrcBlend = 1f;

	[Space(50)]
	[Header("Other Props")]
	[Tooltip("Colored outline")]
	[SerializeField] private Color _CustomColor = new Color(1f, 1f, 1f, 1f);

	[Tooltip("Colored Gamma")]
	[ColorUsage(true, true)]
	[SerializeField] private Color _CustomGamma = new Color(1f, 1f, 1f, 1f);

	[Tooltip("That Vec")]
	[SerializeField] private Vector4  _Vector = new Vector4(1f, 3f, 2f, 4f);

	[Tooltip("Range2")]
	[Range(-255f, 255f)]
	[SerializeField] private int _Range2 = 0;

	[Tooltip("AS")]
	[Range(654f, 1024f)]
	[SerializeField] private float _Range = 750f;
	
	[Tooltip("TRUE F")]
	[Range(0f, 1f)]
	[SerializeField] private float _RangeFloat = 0.08f;
	


	protected override void Start()
	{
		shader = Shader.Find("Unlit/Test_Shader");

		base.Start();
	}

	public override void SetupInspectorValue()
	{
		_MyFloat = material.GetFloat("_MyFloat");
		_SrcBlend = material.GetFloat("_SrcBlend");
		_CustomColor = material.GetColor("_CustomColor");
		_CustomGamma = material.GetColor("_CustomGamma");
		_Vector = material.GetVector("_Vector");
		_Range2 = material.GetInt("_Range2");
		_Range = material.GetFloat("_Range");
		_RangeFloat = material.GetFloat("_RangeFloat");
	}

	private void Update()
	{
		material.SetFloat("_MyFloat", _MyFloat);
		material.SetFloat("_SrcBlend", _SrcBlend);
		material.SetColor("_CustomColor", _CustomColor);
		material.SetColor("_CustomGamma", _CustomGamma);
		material.SetVector("_Vector", _Vector);
		material.SetFloat("_Range2", _Range2);
		material.SetFloat("_Range", _Range);
		material.SetFloat("_RangeFloat", _RangeFloat);
	}
}