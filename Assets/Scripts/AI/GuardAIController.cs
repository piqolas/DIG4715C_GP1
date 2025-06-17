using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using piqey.Utilities;
using piqey.Utilities.Editor;

// [RequireComponent(typeof(CharacterController))]
public partial class GuardAIController : MonoBehaviour
{
	[Header("AI"), Space]

	public Transform SentryEyes;
	public LayerMask SentryViewconeMask;

	[Range(0.0f, 1.0f)]
	public float SentryViewconeWidth = 2.0f / 3.0f;
	[Range(0.0f, 128.0f)]
	public float SentryViewconeDist = 30.0f;

	[Range(0.1f, 10.0f)]
	public float SentryGiveUpChaseTime = 5.0f;
	[Range(0.1f, 10.0f)]
	public float SentryGiveUpSearchTime = 6.0f;

	[Range(0.01f, 12.0f)]
	public float SearchLocationRadius = 4.0f;
	[Range(0.01f, 1.0f)]
	public float SearchMoveSpeedMul = 0.5f;

	[Tooltip("Whether the AI Sentry should remain in its current state regardless of circumstantial fallbacks")]
	public bool SentryRemainInState = false;

	public Transform[] PatrolPoints;
	public Transform PatrolPoint => PatrolPoints[PatrolPointIndex];
	public int PatrolPointIndex
	{
		get => _patrolPointIndex;
		set
		{
			_patrolPointIndex = Mathf.Max(0, value % PatrolPoints.Length);
			_currentPatrolPoint = PatrolPoints[_patrolPointIndex];
		}
	}

	[Range(0.01f, 1.0f)]
	public float UpdateRate = 0.1f;

	[SerializeField] private GuardState _defaultState = GuardState.Patrol;

	public GuardState State
	{
		get => _state;
		set
		{
			OnStateChange?.Invoke(_state, value);
			_state = value;
		}
	}

	public delegate void StateChangeEvent(GuardState oldState, GuardState newState);
	public StateChangeEvent OnStateChange;

	[Header("Sound Settings"), Space]

	public AudioSource RockSlideSound;
	[Range(0.01f, 32.0f)]
	public float RockSlideSoundLerpRate = 20.0f;

	[Header("Debug Only"), Space]

	[SerializeField, ReadOnly] private float _rockSlideSoundInitialVolume = 0.0f;
	[SerializeField, ReadOnly] private float _rockSlideSoundPreVolume = 0.0f;

	[SerializeField, ReadOnly] private GameObject _player;
	[SerializeField, ReadOnly] private Transform _currentPatrolPoint;
	[SerializeField, ReadOnly] private int _patrolPointIndex;
	[SerializeField, ReadOnly] private NavMeshAgent _agent;

	[SerializeField, ReadOnly] private GuardState _state;

	private Coroutine _followCoroutine;
	private Coroutine _sentryCoroutine;

	private TimeSince _playerLastSpotted;
	private TimeSince _timeInState;

#if UNITY_EDITOR
	[SerializeField, ReadOnly] private float _debugRockSlideVolume;
	[SerializeField, ReadOnly] private float _sentryViewconeDot = 0.0f;
#endif

	private void Awake()
	{
		_rockSlideSoundInitialVolume = RockSlideSound.volume;

		_player = GameObject.FindGameObjectWithTag("Player");

		if (!_player)
			Debug.LogError($"Couldn't find a {nameof(_player)} for {nameof(GuardAIController)}!");

		_agent = GetComponent<NavMeshAgent>();

		OnStateChange += HandleStateChange;
	}

	private void Start()
	{
		OnStateChange?.Invoke(GuardState.Spawn, _defaultState);
		_sentryCoroutine = StartCoroutine(DoSentry());
	}

	private void Update()
	{
		// _agent.SetDestination(_destination.transform.position);
		DoRockSlideSound();
	}

	private void DoRockSlideSound()
	{
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

	private void OnDisable() =>
		_state = _defaultState;

	private IEnumerator DoSentry()
	{
		WaitForSeconds wait = new(UpdateRate);

		while (gameObject.activeSelf)
		{
			Vector3 plyPos = _player.transform.position;
			Vector3 rayDir = (plyPos - SentryEyes.position).normalized;
			float eyeDot = Vector3.Dot(SentryEyes.forward, rayDir);

		#if UNITY_EDITOR
			_sentryViewconeDot = eyeDot;
		#endif

			if (eyeDot >= SentryViewconeWidth
				&& Physics.Raycast(SentryEyes.position, rayDir, out RaycastHit hit, SentryViewconeDist, SentryViewconeMask, QueryTriggerInteraction.UseGlobal)
				&& hit.transform.gameObject == _player)
			{
				_playerLastSpotted = 0.0f;

				if (_state != GuardState.Chase)
					State = GuardState.Chase;
			}
			else if (!SentryRemainInState)
				if (_state == GuardState.Chase && _playerLastSpotted >= SentryGiveUpChaseTime)
					State = GuardState.Search;
				else if (_state == GuardState.Search && _timeInState >= SentryGiveUpSearchTime)
					State = GuardState.Patrol;

			yield return wait;
		}
	}

	private void HandleStateChange(GuardState oldState, GuardState newState)
	{
		if (oldState != newState)
		{
			_timeInState = 0.0f;

			if (_followCoroutine != null)
				StopCoroutine(_followCoroutine);

			if (oldState == GuardState.Search)
				_agent.speed /= SearchMoveSpeedMul;

			switch (newState)
			{
				case GuardState.Search:
					_followCoroutine = StartCoroutine(DoIdleMotion());
					break;

				case GuardState.Chase:
					_followCoroutine = StartCoroutine(FollowTarget());
					break;

				case GuardState.Patrol:
					_followCoroutine = StartCoroutine(FollowPatrolPath());
					break;
			}
		}
	}

	private IEnumerator DoIdleMotion()
	{
		WaitForSeconds wait = new(UpdateRate);

		_agent.speed *= SearchMoveSpeedMul;

		while (true)
		{
			if (!_agent.enabled || !_agent.isOnNavMesh)
				yield return wait;
			else if (_agent.remainingDistance <= _agent.stoppingDistance)
			{
				Vector2 point = Random.insideUnitCircle * SearchLocationRadius;

				if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(point.x, 0.0f, point.y), out NavMeshHit hit, 2.0f, _agent.areaMask))
					_agent.SetDestination(hit.position);
			}

			yield return wait;
		}
	}

	private IEnumerator FollowTarget()
	{
		WaitForSeconds wait = new(UpdateRate);

		while (gameObject.activeSelf)
		{
			if (_agent.enabled)
				_agent.SetDestination(_player.transform.position);

			yield return wait;
		}
	}

	private const float _patrolPathMaxNavMeshDistance = 1.0f;

	private IEnumerator FollowPatrolPath()
	{
		WaitForSeconds wait = new(UpdateRate);

		PatrolPointIndex = FindClosestPatrolPoint();

		while (true)
		{
			if (!_agent.enabled || !_agent.isOnNavMesh)
				yield return wait;
			else if (_agent.remainingDistance <= _agent.stoppingDistance)
			{
				// Wrapping around array bounds handled by property setter/accessor
				PatrolPointIndex += 1;

				if (NavMesh.SamplePosition(_currentPatrolPoint.position, out NavMeshHit hit, _patrolPathMaxNavMeshDistance, _agent.areaMask))
					_agent.SetDestination(hit.position);
			}

			yield return wait;
		}
	}

	private int FindClosestPatrolPoint() =>
		PatrolPoints.Select((obj, i) => (obj, i))
			.OrderBy(pt => (pt.obj.position - gameObject.transform.position).sqrMagnitude)
			.FirstOrDefault()
			.i;

#if UNITY_EDITOR
	private const float _patrolPointGizmoSphereRadius = 0.5f;
	private static Vector3 _patrolPointGizmoAddUp = Vector3.up * 1.5f;

	private void OnDrawGizmosSelected()
	{
		// Draw patrol path
		for (int i = 0; i < PatrolPoints.Length; i++)
		{
			Transform pointA = PatrolPoints[i];
			Transform pointB = PatrolPoints[(i + 1) % PatrolPoints.Length];

			Gizmos.DrawWireSphere(pointA.position, _patrolPointGizmoSphereRadius * 0.5f);
			Gizmos.DrawWireSphere(pointB.position, _patrolPointGizmoSphereRadius * 0.5f);

			Gizmos.DrawWireSphere(pointA.position + _patrolPointGizmoAddUp, _patrolPointGizmoSphereRadius);
			Gizmos.DrawWireSphere(pointB.position + _patrolPointGizmoAddUp, _patrolPointGizmoSphereRadius);

			Gizmos.DrawLine(pointA.position + _patrolPointGizmoAddUp, pointB.position + _patrolPointGizmoAddUp);
		}

		Gizmos.color = Color.cyan;

		// line up the frustum with our "eyes"
		Gizmos.matrix = Matrix4x4.TRS(
			SentryEyes.position,
			SentryEyes.rotation,
			Vector3.one
		);

		// convert dot‐threshold -> full vertical FOV in degrees
		float halfAngleRad = Mathf.Acos(SentryViewconeWidth);
		float fullFovDeg = halfAngleRad * 2.0f * Mathf.Rad2Deg;

		Gizmos.DrawFrustum(
			Vector3.zero,
			fullFovDeg,
			SentryViewconeDist,
			0.0f,
			1.0f
		);
	}
#endif
}
