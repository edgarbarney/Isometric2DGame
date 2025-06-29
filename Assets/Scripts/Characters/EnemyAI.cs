using UnityEngine;

namespace Isometric2DGame.Characters.AI
{
	public class EnemyAI : BaseAI
	{
		public Color patrolColor = Color.blue;
		public Color followColor = Color.yellow;
		public Color attackColor = Color.red;

		protected override void Awake()
		{
			base.Awake();

			if (followModule.possibleTargets == null || followModule.possibleTargets.Length == 0)
			{
				Debug.LogWarning("No possible targets set for EnemyAI. Finding players by tag.");
				followModule.possibleTargets = GameObject.FindGameObjectsWithTag("Player");
			}
		}

		protected override void Follow()
		{
			base.Follow();

			mySpriteRenderer.material.color = followColor;
		}

		protected override void Patrol()
		{
			base.Patrol();

			mySpriteRenderer.material.color = patrolColor;
		}

		protected override void Attack()
		{
			base.Attack();

			mySpriteRenderer.material.color = attackColor;
		}
	}
}
