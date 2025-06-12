using Unity.VisualScripting;
using UnityEngine;

// [RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class PixelateBlit : MonoBehaviour
{
	/// <summary>
	/// Records whether a <see cref="PixelateBlit" /> component is
	/// already active somewhere in the scene (for optimization).
	/// </summary>
	private static bool _isActive = false;

	/// <summary>
	/// Render texture used for downsampling the framebuffer.
	/// Resolution specified in <see cref="DownSampleResolution" />.
	/// </summary>
	private static RenderTexture _lowRT;

	/// <remarks>
	/// Some PS1 framebuffer resolutions:
	/// <list type="bullet">
	///   <item>
	///     <term>Silent Hill</term>
	///     <description>319×223 px</description>
	///   </item>
	///   <item>
	///     <term>Spyro</term>
	///     <description>297×217 px</description>
	///   </item>
	///   <item>
	///     <term>Metal Gear Solid</term>
	///     <description>320×200 px</description>
	///   </item>
	/// </list>
	/// </remarks>
	public Vector2Int DownSampleResolution = new(319, 223);

	public Material PixelateMat;
	// public Shader BlitShader;
	// public Texture NoiseTexture;

	private void Awake()
	{
		if (_isActive)
		{
			Debug.LogWarning($"Another {nameof(PixelateBlit)} is already running. Disabling this one!");
			enabled = false;

			return;
		}
		else
			_isActive = true;

		// if (BlitShader == null)
		// 	Debug.LogError($"Assign a {nameof(BlitShader)}!");
		// else
		// 	PixelateMat = new Material(BlitShader);
	}

	private void OnEnable() =>
		CreateBuffer();

	private void OnValidate() =>
		CreateBuffer();

	private void OnDisable() =>
		ReleaseBuffer();

	private void OnDestroy()
	{
		if (_isActive)
			_isActive = false;
	}

	private void CreateBuffer()
	{
		if (_lowRT != null && _lowRT.width == DownSampleResolution.x && _lowRT.height == DownSampleResolution.y)
			return;

		ReleaseBuffer();

		_lowRT = new RenderTexture(DownSampleResolution.x, DownSampleResolution.y, 0)
		{
			autoGenerateMips = false,
			useMipMap = false,
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp,
			antiAliasing = 1,
			format = RenderTextureFormat.DefaultHDR
		};

		if (Camera.main != null)
		{
			Camera.main.allowMSAA = false;
			QualitySettings.antiAliasing = 0;
		}
	}

	private void ReleaseBuffer()
	{
		if (_lowRT != null)
		{
			_lowRT.Release();
			Destroy(_lowRT);
			_lowRT = null;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (!enabled) // `OnRenderImage` must write something into `dest`
		{
			Graphics.Blit(src, dest);
			return;
		}

		// ensure src samples with point filtering
		FilterMode oldFilter = src.filterMode;
		src.filterMode = FilterMode.Point;
		// ensure `_lowRT` is also point-sampled
		_lowRT.filterMode = FilterMode.Point;

		// downsample with point filtering
		Graphics.Blit(src, _lowRT);
		// restore original filter mode on `src`
		src.filterMode = oldFilter;

		// upsample to screen (will also be point when we set `_lowRT` above)
		//_pixelateMat.SetTexture("_NoiseTex", NoiseTexture);
		Graphics.Blit(_lowRT, dest, PixelateMat);
	}
}
