using UnityEngine;
using piqey.Utilities;

public class PlayerControllerEscapeText : MonoBehaviour
{
	[Tooltip("The text to display (duh)")]
	public string DisplayText = "Find a way out!";

	[Tooltip("How long this text should persist on-screen")]
	public float Lifetime = 10.0f;

	private TimeSince _timeSinceDisplayed;

	private void Start() =>
		_timeSinceDisplayed = 0.0f;

	private void Update()
	{
		if (_timeSinceDisplayed >= Lifetime)
			Destroy(this);
	}

	private const float _textPadding = 16.0f;
	private const float _textWidth = 512.0f;
	private const float _textHeight = 48.0f;

	private void OnGUI() =>
		GUI.Label(
			new Rect(_textPadding, _textPadding, _textWidth, _textHeight),
			DisplayText
		);
}
