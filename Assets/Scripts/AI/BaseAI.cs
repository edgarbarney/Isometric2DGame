using UnityEngine;
using UnityEngine.UIElements;

namespace Isometric2DGame.Characters.AI
{
	public enum AIState
	{
		Idle,		// Enemy is not moving or attacking.
		Follow,		// Enemy is following the target.
		Attack,     // Enemy is attacking the target.
		Patrol,     // Enemy is following a predefined path.
	}

	// What can the AI currently do?
	[System.Serializable]
	public struct AIAbilities
	{
		public bool canFollow;
		public bool canAttack;
		public bool canPatrol;

		public AIAbilities (bool follow, bool attack, bool patrol)
		{
			canFollow = follow;
			canAttack = attack;
			canPatrol = patrol;
		}
	}

	// Base class for AI components.
	// This class contains the basic functionality and properties that almost all AI components should have.
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Collider2D))]
	public class BaseAI : MonoBehaviour
	{
		protected Rigidbody2D myRigidbody;
		protected Collider2D myCollider;

		[SerializeField]
		private AIAbilities abilities = new(true, false, false); // By default, AI can follow, but not attack or patrol.
		public AIAbilities Abilities
		{
			get => abilities;
			protected set => abilities = value;
		}
		protected AIState currentState = AIState.Idle;

		[SerializeField]
		protected float moveSpeed = 2f;

		// Follow and attacking stuff
		private GameObject primaryTarget;       // The target the AI is currently interacting with, e.g., the player.
		public GameObject PrimaryTarget
		{
			get => primaryTarget;
			protected set => primaryTarget = value;
		}
		[SerializeField]
		protected GameObject[] possibleTargets; // All possible targets the AI can interact with.
		[SerializeField]
		protected float detectionDist = 5f;     // The distance at which the AI can detect targets.

		// Patrol stuff
		private Transform patrolTarget;         // The target the AI is currently patrolling towards.
		public Transform PatrolTarget
		{
			get => patrolTarget;
			protected set => patrolTarget = value;
		}
		[SerializeField]
		protected Transform[] patrolPoints;		// The points the AI will patrol between.
		[SerializeField]
		protected float patrolDelay = 2f;		// The time the AI will wait at each patrol point before moving to the next one.
		[SerializeField]
		protected float lastPatrolTime = 0f;	// The speed at which the AI will patrol.

		// Attack stuff
		[SerializeField]
		protected float attackDist = 1.5f;      // The distance at which the AI can attack the target.
		[SerializeField]
		protected float attackDelay = 1f;       // The delay between attacks.
		[SerializeField]
		protected float lastAttackTime = 0f;    // The time when the AI last attacked.
		[SerializeField]
		protected float attackDamage = 10f;     // The damage dealt by the AI when it attacks.


		protected virtual void Awake()
		{
			// Component caching
			myRigidbody = GetComponent<Rigidbody2D>();
			myCollider = GetComponent<Collider2D>();

			PrimaryTarget = null; // No initial target. Otherwise ai may try to follow a target prematurely.

			if (Abilities.canPatrol)
			{
				PatrolTarget = GetNextPatrolPoint();
			}
			else
			{
				PatrolTarget = null; // No patrol target if AI can't patrol.
			}
		}

		// Returns a random patrol point from the list of patrol points.
		// It will not be the same as the current patrol target.
		protected Transform GetNextPatrolPoint()
		{
			if (patrolPoints.Length == 0)
			{
				Debug.LogWarning("No patrol points set for AI. Please set patrol points.");
				return null;
			}

			Transform nextPoint;

			do
			{
				nextPoint = patrolPoints[Random.Range(0, patrolPoints.Length)];
			} while (nextPoint == PatrolTarget);

			return nextPoint;
		}

		private void Update()
		{
			CalculateState();
		}

		private void FixedUpdate()
		{
			Think();
		}

		// Called every frate to determine the AI's current state based on its abilities and targets.
		// Our base state machine logic.
		private void CalculateState()
		{
			currentState = AIState.Idle; // Default state is idle.

			if (PrimaryTarget != null)
			{
				if (Abilities.canAttack && GetAttackableHealth(PrimaryTarget) != null)
				{
					currentState = AIState.Attack;
					return;
				}
				if (Abilities.canFollow)
				{
					currentState = AIState.Follow;
					return;
				}
			}

			if (Abilities.canPatrol && PatrolTarget != null)
			{
				currentState = AIState.Patrol;
				return;
			}
		}

		// Called every deltaTime to process the AI's current state.
		// Physical movement and actions are handled here.
		private void Think()
		{
			switch(currentState)
			{
				default:
				case AIState.Idle:
					// TODO: Animations etc. for idle
					break;
				case AIState.Follow:
					FollowTarget(PrimaryTarget);
					break;
				case AIState.Attack:
					PerformAttack(PrimaryTarget);
					break;
				case AIState.Patrol:
					Patrol(PatrolTarget);
					break;
			}
		}

		// Tries to find a target for the AI to follow.
		// Returns the first target that is within the detection distance.
		// Uses raycasting to check for obstacles.
		protected GameObject FindTargetToFollow()
		{
			foreach (GameObject target in possibleTargets)
			{
				if (CanFollowObstacle(target)) 
					return target; // Found a valid target to follow.
			}
			return null; // No valid target found
		}

		// Follows the target, moving towards it at the AI's move speed.
		protected void FollowTarget(GameObject target)
		{
			if (!CanFollow(target))
				return;

			Vector2 direction = (target.transform.position - transform.position).normalized;
			myRigidbody.MovePosition(myRigidbody.position + direction * moveSpeed);
		}

		// Checks if the AI can follow the target based on distance.
		// Returns true if the target is within the detection distance, false otherwise.
		protected bool CanFollow(GameObject target)
		{
			if (target == null)
				return false;

			return Vector2.Distance(transform.position, target.transform.position) <= detectionDist;
		}

		// Checks if the AI can follow the target, considering distance AND obstacles in the way.
		// Uses raycasting to check if the target is visible and not blocked by obstacles.
		// Returns true if the target is visible and within detection distance, false otherwise.
		protected bool CanFollowObstacle(GameObject target)
		{
			// Check for distance first to avoid unnecessary raycasts.
			if (!CanFollow(target))
				return false;
			
			RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, detectionDist);
			return (hit.collider != null && hit.collider.gameObject == target);
		}

		// Returns the CharacterHealth component of the target if it can be attacked, null otherwise.
		// Alos checks if the AI can attack the target
		// Based on distance, attack delay and if the target has a health component.
		protected CharacterHealth GetAttackableHealth(GameObject target)
		{
			if (target == null)
				return null;

			if (Time.time < lastAttackTime + attackDelay)
				return null; // Can't attack yet, still in cooldown.

			if (Vector2.Distance(transform.position, target.transform.position) > attackDist)
				return null;

			return target.GetComponent<CharacterHealth>();
		}

		// Performs an attack on the target if the AI is in the attack state and the target is within range.
		// Also, target should have a health component.
		private void PerformAttack(GameObject target)
		{
			// TODO: Should we get rid of this check?
			// It's already done in CalculateState.
			CharacterHealth targetHealth = GetAttackableHealth(target);

			// Either in cooldown or target is not attackable.
			if (targetHealth == null) 
				return; 

			targetHealth.TakeDamage(attackDamage);
			lastAttackTime = Time.time; // Reset the attack timer.
		}

		private void Patrol(Transform target)
		{
			if (target == null)
				return;

			if (Time.time < lastPatrolTime + patrolDelay)
				return; // Cooldonw

			ProcessMovementTowards(target.position);

			// Did we reach the patrol target?
			if (Vector2.Distance(transform.position, target.position) < 0.1f)
			{
				PatrolTarget = GetNextPatrolPoint();
				lastPatrolTime = Time.time; // Reset the patrol timer.
			}

			if (Abilities.canFollow)
				FindTargetToFollow();
		}

		private void ProcessMovementTowards(Vector2 target)
		{
			Vector2 direction = (target - (Vector2)transform.position);
			myRigidbody.linearVelocity = direction.normalized * moveSpeed;
		}
	}
}
