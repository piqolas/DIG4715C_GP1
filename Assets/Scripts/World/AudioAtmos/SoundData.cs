using UnityEngine;
// using piqey.Utilities.Editor;

[CreateAssetMenu(fileName = "SoundData", menuName = "AudioAtmos/Create SoundData")]
public class SoundData : ScriptableObject
{
	public enum SoundType
	{
		OneOff = 0,
		Looping = 1,
	}

	public enum ImpetusType
	{
		Random = 0,
		Instant = 1,
	}

	public enum PositionType
	{
		Anywhere = 0,
		Hemisphere = 2,
		Everywhere = 1,
	}

	public AudioClip Sound;

	[Header("Engine"), Space]

	[Range(0, 256)]
	public int Priority = 128;

	[Header("Engine"), Space]

	public SoundType Type = SoundType.OneOff;
	public ImpetusType Impetus = ImpetusType.Random;
	public PositionType Positioning = PositionType.Anywhere;

	[Space]

	[Tooltip("Used only when Positioning is set to a value where it makes sense that it would be.")]
	public Vector2 Distance = new(8.0f, 8.0f);

	[Header("VOL/PIT"), Space]

	public Vector2 Volume = Vector2.one;
	public Vector2 Pitch = Vector2.one;

	[Header("Spatialization"), Space]

	public bool Spatialize = true;

	[Space]

	[Range(-1.0f, 1.0f)]
	public float StereoPan = 0.0f;
	[Range(0.0f, 1.0f)]
	public float SpatialBlend = 1.0f;
	[Range(0.0f, 1.0f)]
	public float ReverbZoneMix = 1.0f;
	[Range(0.0f, 5.0f)]
	public float DopplerLevel = 0.25f; // TODO: Change this(?)
	[Range(0.0f, 360.0f)]
	public float Spread = 0.0f;

	[Space]

	public AudioRolloffMode RolloffCurveType = AudioRolloffMode.Logarithmic;
	public Vector2 RolloffDistMinMax = new(1.0f, 36.0f);
	// public AnimationCurve rolloffCurve;
}
