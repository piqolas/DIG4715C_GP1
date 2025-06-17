using UnityEngine;
using piqey.Utilities.Editor;

public partial class PickupItem : MonoBehaviour
{
	public GameObject LightObject;
	public AudioClip CollectSound;
	[Range(0.0f, 1.0f)]
	public float CollectSoundVolume = 1.0f;

	[SerializeField] private PickupType _pickupType = PickupType.MacGuffin;

	// [SerializeField, ReadOnly] private GameObject _player;
	[SerializeField, ReadOnly] private float _lightObjectDist = 0.0f;

	private void Start()
	{
		if (!LightObject)
			Debug.LogError($"No {nameof(LightObject)} specified for {nameof(PickupItem)}!");
		if (!CollectSound)
			Debug.LogError($"No {nameof(CollectSound)} specified for {nameof(PickupItem)}!");

		_lightObjectDist = (LightObject.transform.position - transform.position).magnitude;

		// _player = GameObject.FindGameObjectWithTag("Player");

		// if (!_player)
		// Debug.LogError($"Couldn't find a {nameof(_player)} for {nameof(PickupItem)}!");
	}

	private void Update()
	{
		Vector3 dir = (Camera.main.transform.position - transform.position).normalized;
		LightObject.transform.position = transform.position + dir * _lightObjectDist;
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject obj = other.gameObject;

		if (obj.CompareTag("Player"))
		{
			AudioSource asrc = obj.GetComponentInChildren<AudioSource>();

			// Add this PickupType to the PlayerController HashSet
			PlayerController.PickupsCollected.Add(_pickupType);

			if (_pickupType == PickupType.MacGuffin)
				obj.AddComponent<PlayerControllerEscapeText>();

			if (asrc)
				asrc.PlayOneShot(CollectSound, CollectSoundVolume);
			else
				Debug.LogWarning($"Failed to find {nameof(AudioSource)} on {nameof(obj)}!");

			Destroy(gameObject);
		}
	}
}
