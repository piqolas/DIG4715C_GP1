using UnityEngine;

// [RequireComponent(typeof(CharacterController))]
public class CharacterSetup : MonoBehaviour
{
	[SerializeField]
	private CharacterController cc;

	// [SerializeField]
	// private Camera cam;

	[Tooltip("Moves the capsule so that its bottom sphere sits at the pivot")]
	public bool AlignFeetToPivot = true;

	private void Awake()
	{
		if (cc == null)
			cc = GetComponent<CharacterController>();

		// if (cam == null)
			// cam = GetComponent<Camera>();

		ApplySettings();
	}

	private void OnValidate()
	{
		// Changes in the Inspector will thus take effect immediately
		if (cc == null)
			cc = GetComponent<CharacterController>();
		
		// if (cam == null)
			// cam = GetComponent<Camera>();

		ApplySettings();
	}

	private void ApplySettings()
	{
		if (!AlignFeetToPivot)
			return;

		cc.center = new Vector3(
			0.0f,
			cc.height * 0.5f - cc.radius,
			0.0f
		);
	}
}
