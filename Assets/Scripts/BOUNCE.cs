using UnityEngine;
using piqey.Utilities.Editor;

public class BouncingAnimation : MonoBehaviour
{
	public float FloatSpeedMul = 1.0f;
	public float SpeedMul = 0.5f;
	[Range(0.0f, 2.0f)]
	public float FloatHeight = 0.25f;

	[SerializeField, ReadOnly] private Vector3 _initialOffset;

	void Awake()
	{
		// Capture the editor-offset pivot
		_initialOffset = transform.position;
	}

	void Update()
	{
		// Rotate around world-aligned "up" axis
		Quaternion q = Quaternion.AngleAxis(Time.time * 360.0f * SpeedMul, Vector3.up);

		// Float up and down
		transform.position = _initialOffset + FloatHeight * Mathf.Sin(Time.time * Mathf.PI * FloatSpeedMul) * Vector3.up;
		transform.rotation = q;
	}
}
