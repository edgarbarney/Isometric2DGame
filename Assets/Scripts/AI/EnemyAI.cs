using UnityEngine;

namespace Isometric2DGame.Characters.AI
{
	public class EnemyAI : BaseAI
	{
		
		protected override void Awake()
		{
			base.Awake();

			possibleTargets = GameObject.FindGameObjectsWithTag("Player");
		}
	}
}
