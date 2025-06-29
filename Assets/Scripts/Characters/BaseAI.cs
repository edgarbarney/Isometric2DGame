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

	[System.Serializable]
	public class AIModule
	{
		[SerializeField]
		private bool isEnabled = true;		// Whether this AI module is enabled or not.
		public bool IsEnabled
		{
			get => isEnabled;
			set => isEnabled = value;
		}
	}

	[System.Serializable]
	public class FollowModule : AIModule
	{
		private GameObject primaryTarget;	// The target the AI is currently interacting with, e.g., the player.
		public GameObject PrimaryTarget
		{
			get => primaryTarget;
			set => primaryTarget = value;
		}
		public GameObject[] possibleTargets; // All possible targets the AI can interact with.
		public float detectionDist = 5f;     // The distance at which the AI can detect targets.
	}

	[System.Serializable]
	public class PatrolModule : AIModule
	{
		private Transform patrolTarget;     // The target the AI is currently patrolling towards.
		public Transform PatrolTarget
		{
			get => patrolTarget;
			set => patrolTarget = value;
		}
		public Transform[] patrolPoints;	// The points the AI will patrol between.
		public float patrolDelay = 2f;		// The time the AI will wait at each patrol point before moving to the next one.
		public float lastPatrolTime = 0f;	// The speed at which the AI will patrol.
	}

	[System.Serializable]
	public class AttackModule : AIModule
	{
		[Header("Note: Attack Module depends on Follow Module to find the target.")]
		public float attackDist = 0.5f;		// The distance at which the AI can attack the target.
		public float attackDelay = 1f;		// The delay between attacks.
		public float lastAttackTime = 0f;	// The time when the AI last attacked.
		public float attackDamage = 10f;	// The damage dealt by the AI when it attacks.
	}

	// Base class for AI components.
	// This class contains the basic functionality and properties that almost all AI components should have.
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Collider2D))]
	public class BaseAI : MonoBehaviour
	{
		protected Rigidbody2D myRigidbody;
		protected Collider2D myCollider;
		protected SpriteRenderer mySpriteRenderer;
		public CharacterSpriteManager characterSpriteManager;

		protected AIState currentState = AIState.Idle;

		[SerializeField]
		protected float moveSpeed = 2f;

		[SerializeField]
		protected FollowModule followModule = new FollowModule();

		[SerializeField]
		protected PatrolModule patrolModule = new PatrolModule();

		[SerializeField]
		protected AttackModule attackModule = new AttackModule();

		protected virtual void Awake()
		{
			// Component caching
			myRigidbody = GetComponent<Rigidbody2D>();
			myCollider = GetComponent<Collider2D>();
			mySpriteRenderer = transform.GetComponentInChildren<SpriteRenderer>(); // First found SpriteRenderer in the children

			followModule.PrimaryTarget = null; // No initial target. Otherwise ai may try to follow a target prematurely.

			if (patrolModule.IsEnabled)
			{
				patrolModule.PatrolTarget = GetNextPatrolPoint();
			}
			else
			{
				patrolModule.PatrolTarget = null; // No patrol target if AI can't patrol.
			}
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

			if (followModule.PrimaryTarget != null)
			{
				if (attackModule.IsEnabled && GetAttackableHealth(followModule.PrimaryTarget) != null)
				{
					currentState = AIState.Attack;
					return;
				}
				if (followModule.IsEnabled)
				{
					currentState = AIState.Follow;
					return;
				}
			}

			if (patrolModule.IsEnabled && patrolModule.PatrolTarget != null)
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
					Follow();
					break;
				case AIState.Attack:
					Attack();
					break;
				case AIState.Patrol:
					Patrol();
					break;
			}
		}

		// Tries to find a target for the AI to follow.
		// Returns the first target that is within the detection distance.
		// Uses raycasting to check for obstacles.
		protected GameObject FindTargetToFollow()
		{
			foreach (GameObject target in followModule.possibleTargets)
			{
				if (CanFollowObstacle(target)) 
					return target; // Found a valid target to follow.
			}
			return null; // No valid target found
		}

		// Follows the primary target, moving towards it at the AI's move speed.
		protected virtual void Follow()
		{
			if (!FollowTarget(followModule.PrimaryTarget))
			{
				followModule.PrimaryTarget = null; // Clear the target if it's out of range.
				return;
			}
		}

		// Follows a known target.
		// Does not check for obstacles, only distance.
		// Because you won't forget the target if you already saw it.
		private bool FollowTarget(GameObject target)
		{
			if (!CanFollow(target))
				return false;

			ProcessMovementTowards(target.transform.position);
			return true;
		}

		// Checks if the AI can follow the target based on distance.
		// Returns true if the target is within the detection distance, false otherwise.
		protected bool CanFollow(GameObject target)
		{
			if (target == null)
				return false;

			return Vector2.Distance(transform.position, target.transform.position) <= followModule.detectionDist;
		}

		// Checks if the AI can follow the target, considering distance AND obstacles in the way.
		// Uses raycasting to check if the target is visible and not blocked by obstacles.
		// Returns true if the target is visible and within detection distance, false otherwise.
		protected bool CanFollowObstacle(GameObject target)
		{
			// Check for distance first to avoid unnecessary raycasts.
			if (!CanFollow(target))
				return false;
			
			RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, followModule.detectionDist);
			return (hit.collider != null && hit.collider.gameObject == target);
		}

		// Returns the CharacterHealth component of the target if it can be attacked, null otherwise.
		// Alos checks if the AI can attack the target
		// Based on distance, attack delay and if the target has a health component.
		protected CharacterHealth GetAttackableHealth(GameObject target)
		{
			if (target == null)
				return null;

			if (Time.time < attackModule.lastAttackTime + attackModule.attackDelay)
				return null; // Can't attack yet, still in cooldown.

			if (Vector2.Distance(transform.position, target.transform.position) > attackModule.attackDist)
				return null;

			return target.GetComponent<CharacterHealth>();
		}

		protected virtual void Attack()
		{
			AttackToTarget(followModule.PrimaryTarget);
		}

		// Performs an attack on the target if the AI is in the attack state and the target is within range.
		// Also, target should have a health component.
		private void AttackToTarget(GameObject target)
		{
			// TODO: Should we get rid of this check?
			// It's already done in CalculateState.
			CharacterHealth targetHealth = GetAttackableHealth(target);

			// Either in cooldown or target is not attackable.
			if (targetHealth == null) 
				return; 

			targetHealth.TakeDamage(attackModule.attackDamage);
			attackModule.lastAttackTime = Time.time; // Reset the attack timer.
		}

		protected virtual void Patrol()
		{
			PatrolToTarget(patrolModule.PatrolTarget);
		}

		private void PatrolToTarget(Transform target)
		{
			// We have to check if follow module is enabled, because it may override the patrol target.
			// If follow module is enabled, we will try to find a target to follow first.
			if (followModule.IsEnabled)
				followModule.PrimaryTarget = FindTargetToFollow();

			if (target == null)
				return;

			if (Time.time < patrolModule.lastPatrolTime + patrolModule.patrolDelay)
				return; // Cooldonw

			ProcessMovementTowards(target.position);

			// Did we reach the patrol target?
			if (Vector2.Distance(transform.position, target.position) < 0.1f)
			{
				patrolModule.PatrolTarget = GetNextPatrolPoint();
				patrolModule.lastPatrolTime = Time.time; // Reset the patrol timer.
			}
		}

		// Returns a random patrol point from the list of patrol points.
		// It will not be the same as the current patrol target.
		protected Transform GetNextPatrolPoint()
		{
			if (patrolModule.patrolPoints.Length == 0)
			{
				Debug.LogWarning("No patrol points set for AI. Please set patrol points.");
				return null;
			}

			Transform nextPoint;

			do
			{
				nextPoint = patrolModule.patrolPoints[Random.Range(0, patrolModule.patrolPoints.Length)];
			} while (nextPoint == patrolModule.PatrolTarget);

			return nextPoint;
		}

		private void ProcessMovementTowards(Vector2 target)
		{
			Vector2 direction = (target - (Vector2)transform.position);

			characterSpriteManager.SetSprite(WorldData.GetDirectionFromVector(direction.normalized));

			myRigidbody.linearVelocity = direction.normalized * moveSpeed;
		}

		// ==========================================================
		// Editor-only Stuff
		// ==========================================================

#if UNITY_EDITOR
		// Draws gizmos in the editor to visualize the AI's detection distance and patrol points.
		private void OnDrawGizmosSelected()
		{
			if (followModule.IsEnabled)
			{
				if (followModule.PrimaryTarget != null)
				{
					Gizmos.color = Color.magenta;
					Gizmos.DrawLine(transform.position, followModule.PrimaryTarget.transform.position);
					Gizmos.color = Color.red;
				}
				else
				{
					Gizmos.color = Color.yellow;
				}
				
				Gizmos.DrawWireSphere(transform.position, followModule.detectionDist);
			}

			if (attackModule.IsEnabled && followModule.PrimaryTarget != null)
			{
				if (currentState == AIState.Attack)
				{
					Gizmos.color = Color.yellow;
				}
				else
				{
					Gizmos.color = Color.red;
				}

				Gizmos.DrawWireSphere(transform.position, attackModule.attackDist);
			}

			if (patrolModule.IsEnabled && currentState == AIState.Patrol && patrolModule.patrolPoints.Length > 0)
			{
				foreach (Transform point in patrolModule.patrolPoints)
				{
					if (patrolModule.PatrolTarget == point)
					{
						Gizmos.color = Color.green;
					}
					else
					{
						Gizmos.color = Color.blue;
					}

					Gizmos.DrawWireSphere(point.position, 0.2f);
				}
			}
		}
#endif
	}
}
