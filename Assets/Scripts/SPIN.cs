using UnityEngine;

public class IrrationalSphericalOrbit : MonoBehaviour
{
	// Irrational revolutions per second
	public float SpeedX = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f; // phi = 1.618...
	public float SpeedY = Mathf.Sqrt(2.0f); // sqrt(2) = 1.414...
	public float SpeedZ = Mathf.Sqrt(3.0f); // sqrt(3) = 1.732...

	public float SpeedMul = 0.1f;

	private Vector3 _initialOffset;

	void Awake()
	{
		// Capture the editor-offset pivot
		_initialOffset = transform.localPosition;
	}

	void Update()
	{
		float t = Time.time;

		// Rotate around world-aligned local axes directly
		Quaternion q =
			Quaternion.AngleAxis(t * SpeedX * 360.0f * SpeedMul, Vector3.right) *
			Quaternion.AngleAxis(t * SpeedY * 360.0f * SpeedMul, Vector3.up) *
			Quaternion.AngleAxis(t * SpeedZ * 360.0f * SpeedMul, Vector3.forward);

		// Apply to the initial offset to sweep out a sphere
		transform.localPosition = q * _initialOffset;
	}
}
