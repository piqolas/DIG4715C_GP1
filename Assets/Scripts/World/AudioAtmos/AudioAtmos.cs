using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using piqey.Utilities;
using piqey.Utilities.Extensions;
using piqey.Utilities.Editor;

public class AudioAtmos : MonoBehaviour
{
	public Vector2 Delay = Vector2.one;

	[SerializeField] private GameObject _soundPrefab;
	[SerializeField] private SoundData[] _ambientSounds;

	/*[SerializeField, ReadOnly]*/ private TimeSince _timeSinceLastPlay;
	[SerializeField, ReadOnly] private float _currentDelay;

	private readonly List<SoundData> _soundDataBucket = new();
#if UNITY_EDITOR
	private readonly List<GameObject> _soundObjects = new();
#endif

	// [SerializeField, ReadOnly] private GameObject _player;

	private void Awake()
	{
		foreach (SoundData soundData in _ambientSounds)
		{
			if (soundData.Impetus == SoundData.ImpetusType.Instant)
				SpawnSoundData(soundData);
			else
				_soundDataBucket.Add(soundData);
		}

		ResetDelay();
	}

	private void Update()
	{
		if (_timeSinceLastPlay >= _currentDelay)
		{
			SoundData soundData = SampleFromBucket();
			SpawnSoundData(soundData);

			ResetDelay();
		}
	}

	private float GetAdjustedSoundLength(AudioClip clip, float pitch) =>
		clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale) / ((pitch < 0.01f) ? 0.01f : pitch);

	private Vector3 GetSoundDataPosOffset(SoundData soundData)
	{
		return soundData.Positioning switch
		{
			SoundData.PositionType.Anywhere => Random.onUnitSphere * Random.Range(soundData.Distance.x, soundData.Distance.y),
			SoundData.PositionType.Hemisphere => Random.onUnitSphere.WithAbsY() * Random.Range(soundData.Distance.x, soundData.Distance.y),
			SoundData.PositionType.Everywhere => Vector3.zero,
			_ => throw new System.NotImplementedException()
		};
	}

	private GameObject SpawnSoundData(SoundData soundData)
	{
		Vector3 pos = Camera.main.transform.position + GetSoundDataPosOffset(soundData);
		Transform parent = (soundData.Positioning == SoundData.PositionType.Everywhere) ? Camera.main.transform : gameObject.transform;

		GameObject soundObj = Instantiate(_soundPrefab, pos, Quaternion.identity, parent.transform);
#if UNITY_EDITOR
		_soundObjects.Add(soundObj);
#endif

		if (soundObj.TryGetComponent(out AudioSource asrc))
		{
			asrc.clip = soundData.Sound;

			asrc.priority = soundData.Priority;

			float volume = Random.Range(soundData.Volume.x, soundData.Volume.y);
			float pitch = Random.Range(soundData.Pitch.x, soundData.Pitch.y);

			asrc.volume = volume;
			asrc.pitch = pitch;

			asrc.spatialize = true;

			asrc.panStereo = soundData.StereoPan;
			asrc.spatialBlend = soundData.SpatialBlend;
			asrc.reverbZoneMix = soundData.ReverbZoneMix;
			asrc.dopplerLevel = soundData.DopplerLevel;
			asrc.spread = soundData.Spread;

			asrc.rolloffMode = soundData.RolloffCurveType;
			asrc.minDistance = soundData.RolloffDistMinMax.x;
			asrc.maxDistance = soundData.RolloffDistMinMax.y;

			if (soundData.Type == SoundData.SoundType.Looping)
				asrc.loop = true;
			else
				Destroy(soundObj, GetAdjustedSoundLength(soundData.Sound, pitch));

			asrc.Play();
		}
		else
			Debug.LogError($"Couldn't find {nameof(AudioSource)} on {nameof(_soundPrefab)}!");

		return soundObj;
	}

	private void ResetDelay()
	{
		_timeSinceLastPlay = 0.0f;
		_currentDelay = Random.Range(Delay.x, Delay.y);
	}

	private void FillBucket() =>
		_soundDataBucket.AddRange(_ambientSounds.Where(d => d.Impetus != SoundData.ImpetusType.Instant));

	private SoundData SampleFromBucket()
	{
		if (_soundDataBucket.Count == 0)
			FillBucket();

		return _soundDataBucket.Random(true);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		_soundObjects.RemoveAll(obj => !obj);

		foreach (GameObject soundObj in _soundObjects)
			Gizmos.DrawLine(Camera.main.transform.position, soundObj.transform.position);
	}
#endif
}
