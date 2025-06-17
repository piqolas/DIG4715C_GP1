using UnityEngine;
using piqey.Utilities.Editor;

public class GameOverTrigger : MonoBehaviour
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
			Debug.LogError($"No {nameof(_pauseMenu)} specified for {nameof(GameOverTrigger)}!");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag(_playerTag))
		{
			// Display "Game Over" screen, etc.
			_pauseMenu.DoGameOver();
			// Destroy this object
			Destroy(gameObject);
		}
	}
}
