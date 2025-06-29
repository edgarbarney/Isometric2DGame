using UnityEngine;
using UnityEngine.InputSystem;

namespace Isometric2DGame.Characters.Player
{
	public class PlayerController : MonoBehaviour
	{
		public enum AccelerationState
		{
			Normal,  // Normal acceleration, smooth acceleration and deceleration.
			Instant, // Instant acceleration, immediate change in speed.
		}

		private static PlayerController instance;
		public static PlayerController Instance
		{
			get
			{
				if (instance == null)
				{
					instance = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
				}

				return instance;
			}
		}

		private Rigidbody2D myRigidbody;
		private PlayerInput myPlayerInput;
		public PlayerInput MyPlayerInput
		{
			get { return myPlayerInput; }
		}
		private Camera playerCamera;
		public Camera PlayerCamera
		{
			get { return playerCamera; }
		}
		private PlayerSpriteManager playerSpriteManager;
		public PlayerSpriteManager PlayerSpriteManager
		{
			get { return playerSpriteManager; }
		}

		[SerializeField]
		private float normalMoveSpeed = 5; // Speed at which the player moves, can be adjusted in the inspector.
		[SerializeField]
		private float instantMoveSpeed = 10; // Speed for instant acceleration, can be adjusted in the inspector.


		[SerializeField]
		private float moveSpeed; // Current speed of the player, can be modified during gameplay (e.g., for power-ups or slowdowns).
		public float MoveSpeed
		{
			get => moveSpeed;
			private set => moveSpeed = value;
		}

		private Vector2 moveInputVector; // Input direction for movement.

		public AccelerationState accelerationState = AccelerationState.Normal; // Determines how the player accelerates.

		private void Awake()
		{
			// Component caching
			myRigidbody = GetComponent<Rigidbody2D>();
			myPlayerInput = GetComponent<PlayerInput>();
			playerCamera = GameObject.Find("PlayerCameraSystem").GetComponentInChildren<Camera>();
			playerSpriteManager = GetComponentInChildren<PlayerSpriteManager>();
		}

		private void FixedUpdate()
		{
			ProcessMovement(moveInputVector);
		}

		private void ProcessMovement(Vector2 direction)
		{
			if (direction == Vector2.zero)
				return;

			// Player Sprite Stuff
			playerSpriteManager.SetPlayerSprite(WorldData.GetDirectionFromVector(direction));

			// Set the move speed based on the acceleration state.
			//
			// TODO: Move this to awake, so it only needs to be set once.
			// This is here for demonstration purposes.
			switch (accelerationState)
			{
				default:
				case AccelerationState.Normal:
					MoveSpeed = normalMoveSpeed; // Set the move speed to normal speed for smooth acceleration.
					myRigidbody.linearDamping = 10; // Disable damping for instant acceleration.
					break;
				case AccelerationState.Instant:
					MoveSpeed = instantMoveSpeed; // Set the move speed to instant speed for immediate changes.
					myRigidbody.linearDamping = 100; // Disable damping for instant acceleration.
					break;
			}

			myRigidbody.linearVelocity = direction.normalized * MoveSpeed;
		}

		// ==========================================================
		// Input System Callbacks
		// ==========================================================

		public void OnMove(InputAction.CallbackContext context)
		{
			moveInputVector = context.ReadValue<Vector2>();
		}

		public void OnInteract(InputAction.CallbackContext context)
		{
			if (context.performed)
				_ = PlayerInventory.Instance.PossiblePickupInteract();
		}

		public void OnInventory(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				PlayerInventory.Instance.ToggleInventoryUI(!PlayerInventory.Instance.IsInventoryUIOpen);
			}
		}
	}
}
