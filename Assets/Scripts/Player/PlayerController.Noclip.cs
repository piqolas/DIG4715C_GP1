#if UNITY_EDITOR
using UnityEngine;

public partial class PlayerController
{
	[Header("HAX!")]

	/// <summary>
	/// Is Noclip flying mode enabled? Toggled by pressing `V`;
	/// only exists in editor debugging mode.
	/// </summary>
	[SerializeField]
	private bool isNoclip = false;

	private bool ToggleNoclipRequested()
	{
		if (Input.GetKeyDown(KeyCode.V))
		{
			if (isNoclip)
				_velocity.y = 0.0f;

			isNoclip = !isNoclip;
			_controller.enabled = !isNoclip;
		}

		if (isNoclip)
		{
			HandleNoclip();
			return true;
		}

		return false;
	}

	private void HandleNoclip()
	{
		float y = Input.GetKey(KeyCode.Space) ? 1.0f : (Input.GetKey(KeyCode.LeftControl) ? -1.0f : 0.0f);

		Vector3 move = transform.right * _move.x + transform.forward * _move.y + transform.up * y;
		transform.position += MoveSpeed * Time.deltaTime * move;

		HandleMouseLook();
	}
}
#endif
