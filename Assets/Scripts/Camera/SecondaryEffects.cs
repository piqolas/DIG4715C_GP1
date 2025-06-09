using UnityEngine;

// [RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SecondaryCameraEffects : MonoBehaviour
{
	[SerializeField]
	private Camera cam;

	[Header("Fog")]

	public bool FogEnabled = true;

	public Color FogColor = Color.black;

	[Tooltip("How much padding to put after the camera near clipping plane before using it as the fog \"end\" plane")]
	public float FogNearPadding = 0.0f;
	[Tooltip("How much padding to put before the camera far clipping plane before using it as the fog \"end\" plane")]
	public float FogFarPadding = 16.0f;
	[Range(0.0f, 1.0f)]
	public float FogDensity = 1.0f;
	public FogMode FogMode = FogMode.Linear;

	private void Awake()
	{
		if (cam == null)
			cam = GetComponent<Camera>();
	}

	void Update()
	{
		if (FogEnabled && cam != null)
		{
			RenderSettings.fogStartDistance = cam.nearClipPlane + FogNearPadding;
			RenderSettings.fogEndDistance = cam.farClipPlane - FogFarPadding;
		}

		RenderSettings.fog = FogEnabled;
		RenderSettings.fogColor = FogColor;
		RenderSettings.fogDensity = FogDensity;
		RenderSettings.fogMode = FogMode;
	}
}
