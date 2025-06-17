using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using piqey.Utilities.Editor;

// [RequireComponent(typeof(CharacterController))]
public partial class PlayerController : MonoBehaviour
{
	public static HashSet<PickupItem.PickupType> PickupsCollected = new();
	public static PlayerController current;

	[SerializeField]
	private CharacterController _controller;

	[Header("Movement"), Space]

	[Range(0.0f, 10.0f)]
	public float MoveSpeed = 6.0f;
//	[Range(1.0f / 3.0f, 3.0f)]
//	public float JumpHeight = 1.5f;

	[Header("Camera Look"), Space]

	public Vector2 MouseSensitivity = new(1.0f, 1.0f); // TODO: Vector2Int? Or round to nearest whole number, but cast back to floats for math?

	[Header("Physics"), Space]

	public float Gravity = -9.80665f;

	[Header("Debug Only"), Space]

	[SerializeField, ReadOnly] private Transform _camTransform;
	[SerializeField, ReadOnly] private Vector3 _velocity;
	[SerializeField, ReadOnly] private float _xRot = 0.0f;
	[SerializeField, ReadOnly] private Vector2 _greatestDelta = Vector2.zero;
	// [SerializeField, ReadOnly] private bool _doJump = false;

	[SerializeField, ReadOnly] private Vector2 _move = Vector2.zero;
	// [SerializeField, ReadOnly] private Vector2 _look = Vector2.zero;

#if UNITY_EDITOR
	[SerializeField, ReadOnly] private bool _isGrounded = false;
#endif

//	/// <summary>
	//	/// Used to swallow the first large delta after locking mouse input.
	//	/// </summary>
	//	private bool _cursorJustLocked = false;

	private void Start()
	{
		if (!_controller)
			_controller = GetComponent<CharacterController>();

		_camTransform = Camera.main.transform;

		current = this;
	}

	private void Awake()
	{
		LockCursor();
	}

	private void Update()
	{
	#if UNITY_EDITOR
		if (ToggleNoclipRequested())
			return; // noclip Update will run instead
	#endif

		// #if UNITY_EDITOR
		// HandleCursorLockState();
		// #endif

		UpdateInputs();

		HandleMouseLook(); // TODO: Move to UpdateInputs(?)
		HandleMovement();
	}

	private void UpdateInputs()
	{
//		if (Input.GetButtonDown("Jump"))
//			_doJump = true;
		// Don't accidentally set this back to false before it happens
		// in FixedUpdate/HandlePhysics
//		else if (_doJump == true && Input.GetButtonUp("Jump"))
//			_doJump = false;
	}

	private void FixedUpdate()
	{
		if (_controller.enabled)
			HandlePhysics();
	}

	private void HandlePhysics()
	{
		_controller.Move(Vector3.down * Time.fixedDeltaTime); // TODO: Why is this necessary, Unity?

		if (_controller.isGrounded)
		{
//			if (_doJump)
//			{
//				_velocity.y = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);
//				_doJump = false;
//			}
			/*else */if (_velocity.y < 0.0f)
				// This is supposed to help keep you snapped to the ground;
				// I don't know if it actually does
				_velocity.y = -2.0f;
		}
		else
			_velocity.y += Gravity * Time.fixedDeltaTime;

		// Debug.Log($"{nameof(_controller.isGrounded)}: {_controller.isGrounded}");

		_controller.Move(_velocity * Time.fixedDeltaTime);

	#if UNITY_EDITOR
		_isGrounded = _controller.isGrounded;
	#endif
	}

	// #if UNITY_EDITOR
	//	private void HandleCursorLockState()
	//	{
	// Press Esc to release
	//		if (Input.GetKeyDown(KeyCode.Escape))
	//		{
	//			Cursor.lockState = CursorLockMode.None;
	//			Cursor.visible = true;
	//		}
	// Left-click to re-capture
	//		else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
	//			LockCursor();
	//	}
	// #endif

	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		// _cursorJustLocked = true;
	}

	public void InputMove(InputAction.CallbackContext ctx)
	{
		Vector2 val = ctx.ReadValue<Vector2>();
		_move = Vector2.ClampMagnitude(val, 1.0f);;
	}

	// public void InputLook(InputAction.CallbackContext ctx) =>
	// 	_look = ctx.ReadValue<Vector2>() * Time.deltaTime;

	private void HandleMovement()
	{
		/*
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		Vector3 move = transform.right * x + transform.forward * z;
		move = Vector3.ClampMagnitude(move, 1.0f);
		*/

		Vector3 move = transform.right * _move.x + transform.forward * _move.y;
		_controller.Move(MoveSpeed * Time.deltaTime * move); // move

		// if (Input.GetButtonDown("Jump") && controller.isGrounded)
		// 	_velocity.y = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);
	}

	private void HandleMouseLook()
	{
		// Don't rotate if the cursor's locked
		if (Cursor.lockState != CursorLockMode.Locked)
			return;

		// Swallow the first frame's input after locking to avoid screen-jump
		// if (_cursorJustLocked)
		// {
		// 	_cursorJustLocked = false;
		// 	return;
		// }

		Vector2 mouse = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		if (_greatestDelta.sqrMagnitude < mouse.sqrMagnitude)
			_greatestDelta = mouse;

		mouse *= MouseSensitivity;

		_xRot = Mathf.Clamp(_xRot - mouse.y, -90.0f, 90.0f);
		_camTransform.localRotation = Quaternion.Euler(_xRot, 0.0f, 0.0f);
		transform.Rotate(Vector3.up * mouse.x);
	}

	private void OnEnable()
	{
		if (_controller)
			_controller.enabled = true;
	}

	private void OnDisable()
	{
		if (_controller)
			_controller.enabled = false;
	}
}
