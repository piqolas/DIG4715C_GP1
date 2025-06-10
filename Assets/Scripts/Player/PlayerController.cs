using UnityEngine;
using piqey.Utilities.Editor;

// [RequireComponent(typeof(CharacterController))]
public partial class PlayerController : MonoBehaviour
{
	[SerializeField]
	private CharacterController controller;

	[Header("Movement")]

	[Range(0.0f, 10.0f)]
	public float MoveSpeed = 6.0f;
	[Range(1.0f / 3.0f, 3.0f)]
	public float JumpHeight = 1.5f;

	[Header("Camera Look")]

	public Vector2 MouseSensitivity = new(1600.0f, 1200.0f); // TODO: Vector2Int? Or round to nearest whole number, but cast back to floats for math?

	[Header("Physics")]

	public float Gravity = -9.80665f;

	[Header("Hidden")]

	[SerializeField, ReadOnly] private Transform camTransform;
	[SerializeField, ReadOnly] private Vector3 velocity;
	[SerializeField, ReadOnly] private float xRot = 0.0f;
	[SerializeField, ReadOnly] private Vector2 _greatestDelta = Vector2.zero;

	/// <summary>
	/// Used to swallow the first large delta after locking mouse input.
	/// </summary>
	private bool _cursorJustLocked = false;

	private void Awake()
	{
		if (controller == null)
			controller = GetComponent<CharacterController>();

		camTransform = Camera.main.transform;
		LockCursor();
	}

	private void Update()
	{
	// #if UNITY_EDITOR
		if (ToggleNoclipRequested())
			return; // noclip Update will run instead
	// #endif

	// #if UNITY_EDITOR
		HandleCursorLockState();
	// #endif
		HandleMovement();
		HandleMouseLook();
	}

// #if UNITY_EDITOR
	private void HandleCursorLockState()
	{
		// Press Esc to release
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		// Left-click to re-capture
		else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
			LockCursor();
	}
// #endif

	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		_cursorJustLocked = true;
	}

	private void HandleMovement()
	{
		// Common trick to keep you "snapped" to the ground without accumulating a
		// negative fall-velocity that can prevent `CharacterController.isGrounded`
		// from returning to `true`
		if (controller.isGrounded && velocity.y < 0.0f)
			velocity.y = -2.0f;

		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		Vector3 move = transform.right * x + transform.forward * z;
		controller.Move(MoveSpeed * Time.deltaTime * move);

//		if (Input.GetButtonDown("Jump") && controller.isGrounded)
//			velocity.y = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);

		velocity.y += Gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	private void HandleMouseLook()
	{
		// Don't rotate if the cursor's locked
		if (Cursor.lockState != CursorLockMode.Locked)
			return;

		// Swallow the first frame's input after locking to avoid jump
		if (_cursorJustLocked)
		{
			_cursorJustLocked = false;
			return;
		}

		Vector2 mouse = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		if (_greatestDelta.magnitude < mouse.magnitude)
			_greatestDelta = mouse;

		mouse *= MouseSensitivity;

		xRot = Mathf.Clamp(xRot - mouse.y, -90.0f, 90.0f);
		camTransform.localRotation = Quaternion.Euler(xRot, 0.0f, 0.0f);
		transform.Rotate(Vector3.up * mouse.x);
	}

	private void OnDisable()
	{
		if (controller != null)
			controller.enabled = true;
	}
}
