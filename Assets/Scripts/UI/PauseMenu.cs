using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using piqey.Utilities.Editor;

// [RequireComponent(typeof(CharacterController))]
public class PauseMenu : MonoBehaviour
{
	private static bool _mainMenuOpenedPreviously = false;

	[Header("UI Menus"), Space]

	[SerializeField] private GameObject _pauseMenuUI;
	[SerializeField] private GameObject _mainMenuUI;
	[SerializeField] private GameObject _gameOverMenuUI;
	[SerializeField] private GameObject _winMenuUI;

	[Header("UI Objects"), Space]

	[SerializeField] private GameObject _firstSelected;

	[Header("UI States"), Space]

	[SerializeField, ReadOnly] private bool _isPaused = false;

	private void Awake()
	{
		if (!EventSystem.current)
			new GameObject(
				"EventSystem",
				typeof(EventSystem),
				typeof(StandaloneInputModule)
			);
	}

	private void Start()
	{
		if (!_pauseMenuUI)
			Debug.LogError($"No {nameof(_pauseMenuUI)} specified for {nameof(PauseMenu)}!");

		if (!_mainMenuOpenedPreviously)
			OpenMainMenu();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape)
			&& !(_mainMenuUI.activeSelf
					|| _gameOverMenuUI.activeSelf
					|| _winMenuUI.activeSelf))
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

	public void OpenMainMenu()
	{
		_pauseMenuUI.SetActive(false);
		_gameOverMenuUI.SetActive(false);
		_winMenuUI.SetActive(false);
		_mainMenuUI.SetActive(true);

		Time.timeScale = 0.0f;
		AudioListener.pause = true;

		LockCursor(false);

		_mainMenuOpenedPreviously = true;
	}

	public void OpenMenu()
	{
		_pauseMenuUI.SetActive(true);
		_gameOverMenuUI.SetActive(false);
		_winMenuUI.SetActive(false);
		_mainMenuUI.SetActive(false);

		Time.timeScale = 0.0f;
		AudioListener.pause = true; // TODO: Update to AudioMixer(?)

		LockCursor(false);

		EventSystem.current.SetSelectedGameObject(_firstSelected);
	}

	public void CloseMenu()
	{
		_pauseMenuUI.SetActive(false);
		_gameOverMenuUI.SetActive(false);
		_winMenuUI.SetActive(false);
		_mainMenuUI.SetActive(false);

		Time.timeScale = 1.0f;
		AudioListener.pause = false; // TODO: Update to AudioMixer(?)

		LockCursor(true);

		EventSystem.current.SetSelectedGameObject(null);
	}

	public void ReloadCurrentScene()
	{
		CloseMenu();

		int scene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(scene);
	}

	public void QuitGame() =>
		Application.Quit();

	public void DoGameOver()
	{
		_pauseMenuUI.SetActive(false);
		_gameOverMenuUI.SetActive(true);
		_winMenuUI.SetActive(false);
		_mainMenuUI.SetActive(false);

		Time.timeScale = 0.0f;
		AudioListener.pause = true;

		LockCursor(false);

		DestroyEscapeTexts();
	}

	public void DoWin()
	{
		_pauseMenuUI.SetActive(false);
		_gameOverMenuUI.SetActive(false);
		_winMenuUI.SetActive(true);
		_mainMenuUI.SetActive(false);

		Time.timeScale = 0.0f;
		AudioListener.pause = true;

		LockCursor(false);

		DestroyEscapeTexts();
	}

	private void DestroyEscapeTexts()
	{
		foreach (PlayerControllerEscapeText escapeText in FindObjectsByType<PlayerControllerEscapeText>(FindObjectsSortMode.None))
			Destroy(escapeText);
	}
}
