using UnityEngine;
using UnityEngine.SceneManagement;
// using piqey.Utilities.Editor;

// [RequireComponent(typeof(CharacterController))]
public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject _pauseMenuUI;
	[SerializeField] private bool _isPaused = false;

	private void Start()
	{
		if (!_pauseMenuUI)
			Debug.LogError($"No {nameof(_pauseMenuUI)} specified for {nameof(PauseMenu)}!");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			_isPaused = !_isPaused;

			if (_isPaused)
				OpenMenu();
			else
				CloseMenu();
		}
	}

	private void LockCursor(bool doLock = true)
	{
		Cursor.lockState = doLock ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !doLock;
	}

	public void OpenMenu()
	{
		_pauseMenuUI.SetActive(true);
		Time.timeScale = 0.0f;
		AudioListener.pause = true; // TODO: Update to AudioMixer(?)
		LockCursor(false);
	}

	public void CloseMenu()
	{
		_pauseMenuUI.SetActive(false);
		Time.timeScale = 1.0f;
		AudioListener.pause = false; // TODO: Update to AudioMixer(?)
		LockCursor(true);
	}

	public void ReloadCurrentScene()
	{
		CloseMenu();

		int scene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(scene);
	}

	public void QuitGame() =>
		Application.Quit();
}
