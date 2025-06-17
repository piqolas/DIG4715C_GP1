using UnityEngine;
using piqey.Utilities.Editor;

public class LevelExitWinTrigger : MonoBehaviour
{
	[SerializeField] string _playerTag = "Player";
	[SerializeField] string _menuTag = "MainCanvas";

	[SerializeField, ReadOnly] PauseMenu _pauseMenu;

	private void Awake()
	{
		GameObject mainCanvas = GameObject.FindGameObjectWithTag(_menuTag);
		PauseMenu pauseMenu = mainCanvas.GetComponentInChildren<PauseMenu>();

		if (pauseMenu)
			_pauseMenu = pauseMenu;
		else
			Debug.LogError($"No {nameof(_pauseMenu)} specified for {nameof(LevelExitWinTrigger)}!");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag(_playerTag)
			&& PlayerController.PickupsCollected.Contains(PickupItem.PickupType.MacGuffin))
		{
			// Display "Win" screen, etc.
			_pauseMenu.DoWin();
			// Destroy this object
			Destroy(gameObject);
		}
	}
}
