using UnityEngine;

public class BasePostProcessController : MonoBehaviour
{
	[HideInInspector]
	[SerializeField] protected Shader shader;

	[HideInInspector]
	[SerializeField] protected Material material;
}
