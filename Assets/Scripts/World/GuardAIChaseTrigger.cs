using UnityEngine;
using UnityEngine.AI;

public class GuardAIChaseTrigger : MonoBehaviour
{
	[Tooltip("Multiple by which to increase the guard's speed when triggered")]
	public float GuardSpeedModifier = 1.25f;
	public float GuardSoundPitchModifier = 0.225f;

	[SerializeField] GuardAIController _guard;
	[SerializeField] string _playerTag = "Player";

	private void Awake()
	{
		if (!_guard)
			Debug.LogError($"Couldn't find a {nameof(_guard)} for {nameof(GuardAIChaseTrigger)}!");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag(_playerTag))
		{
			// Set the guard's `State` field to chase the player
			_guard.State = GuardAIController.GuardState.Chase;
			// Ensure the guard remains in this state indefinitely
			_guard.SentryRemainInState = true;
			// Make the guard faster for the final stretch
			if (_guard.TryGetComponent(out NavMeshAgent agent))
				agent.speed += GuardSpeedModifier;
			// Make the guard's sound pitch for the final stretch
			if (_guard.TryGetComponent(out AudioSource asrc))
				asrc.pitch += GuardSoundPitchModifier;

			// Destroy this object
			Destroy(gameObject);
		}
	}
}
