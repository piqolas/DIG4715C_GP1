using UnityEngine;
using UnityEngine.AI;
using piqey.Utilities.Editor;

// [RequireComponent(typeof(CharacterController))]
public class GuardAIController : MonoBehaviour
{
	public AudioSource RockSlideSound;
	[Range(0.01f, 32.0f)]
	public float RockSlideSoundLerpRate = 20.0f;

	[SerializeField, ReadOnly] private float _rockSlideSoundInitialVolume = 0.0f;
	[SerializeField, ReadOnly] private float _rockSlideSoundPreVolume = 0.0f;
	[SerializeField, ReadOnly] private GameObject _destination;
	[SerializeField, ReadOnly] private NavMeshAgent _agent;

#if UNITY_EDITOR
	[SerializeField, ReadOnly] private float _debugRockSlideVolume;
#endif

	private void Start()
	{
		_rockSlideSoundInitialVolume = RockSlideSound.volume;

		_destination = GameObject.FindGameObjectWithTag("Player");

		if (!_destination)
			Debug.LogError($"Couldn't find a {nameof(_destination)} for {nameof(GuardAIController)}!");

		if (TryGetComponent(out NavMeshAgent agent))
			_agent = agent;
		else
			Debug.LogError($"Couldn't find a {nameof(NavMeshAgent)} for {nameof(GuardAIController)}!");

		_agent.SetDestination(_destination.transform.position);
	}

	private void Update()
	{
		_agent.SetDestination(_destination.transform.position);

		if (RockSlideSound)
		{

			if (_agent.velocity.sqrMagnitude > 0.0f)
			{
				float mag = _agent.velocity.magnitude;
				float desiredMag = _agent.desiredVelocity.magnitude;

				float newVolume = Mathf.Approximately(desiredMag, 0.0f) ? 0.0f : mag / desiredMag;
				// Debug.Log(newVolume);

				_rockSlideSoundPreVolume = Mathf.Lerp(_rockSlideSoundPreVolume, newVolume, RockSlideSoundLerpRate * Time.deltaTime);
				RockSlideSound.volume = _rockSlideSoundInitialVolume * _rockSlideSoundPreVolume;

#if UNITY_EDITOR
				_debugRockSlideVolume = newVolume;
#endif

				if (!RockSlideSound.isPlaying)
					RockSlideSound.Play();
			}
			else if (RockSlideSound.isPlaying)
			{
				if (_rockSlideSoundPreVolume <= 1e-5f)
				{
					RockSlideSound.Stop();
					_rockSlideSoundPreVolume = 0.0f;
					RockSlideSound.volume = 0.0f;
				}
				else
				{
					// Still have to use `_rockSlideSoundPreVolume` here to keep things consistent if the guard goes back to moving before
					// the volume fully decays back to zero
					_rockSlideSoundPreVolume = Mathf.Lerp(_rockSlideSoundPreVolume, 0.0f, RockSlideSoundLerpRate * Time.deltaTime);
					RockSlideSound.volume = _rockSlideSoundInitialVolume * _rockSlideSoundPreVolume;
				}
			}
		}
	}
}
