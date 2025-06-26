using UnityEngine;

namespace Isometric2DGame.Characters.AI
{
	public class EnemyAI : BaseAI
	{
		
		protected override void Awake()
		{
			base.Awake();

			if (followModule.possibleTargets == null || followModule.possibleTargets.Length == 0)
			{
				Debug.LogWarning("No possible targets set for EnemyAI. Finding players by tag.");
				followModule.possibleTargets = GameObject.FindGameObjectsWithTag("Player");
			}
		}
	}
}
