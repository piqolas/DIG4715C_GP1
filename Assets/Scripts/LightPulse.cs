using UnityEngine;

public class LightPulse : MonoBehaviour
{
	[SerializeField]
	private new Light light;

	[Range(0.0f, 1.0f)]
	public float Blend = 1.0f;
	[Range(0.0f, 16.0f)]
	public float Rate = 2.5f;
	public Vector2 Offset = Vector2.zero;

	public bool OffsetByHashCode = true;
	[Range(0.0f, 128.0f)]
	public float OffsetStartDistanceMax = 32.0f;

	[SerializeField, piqey.Utilities.Editor.ReadOnly]
	private float _baseIntensity;
	[SerializeField, piqey.Utilities.Editor.ReadOnly]
	private float _baseOffset;

	private void Awake()
	{
		if (light == null)
			light = GetComponent<Light>();

		_baseIntensity = light.intensity;

		if (OffsetByHashCode)
		{
			uint hash = (uint)GetHashCode();

			// It is what it is man
			hash ^= hash << 13;
			hash ^= hash >> 17;
			hash ^= hash << 5;

			// low 16 bits -> X
			float x = (hash & 0xFFFFu) / 65535.0f;
			// high 16 bits -> Y
			float y = ((hash >> 16) & 0xFFFFu) / 65535.0f;

			Offset = new Vector2(x, y);
			_baseOffset = Random.value * OffsetStartDistanceMax;
		}
	}

	private void Update()
	{
		Vector2 scaledTime = Offset * Time.fixedTime;
		light.intensity = Mathf.Lerp(_baseIntensity, _baseIntensity * Mathf.PerlinNoise(_baseOffset + scaledTime.x, _baseOffset + scaledTime.y), Blend);
	}
}
