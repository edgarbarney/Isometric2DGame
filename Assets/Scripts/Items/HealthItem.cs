using UnityEngine;
using Isometric2DGame.Characters;

namespace Isometric2DGame.Items
{
	[CreateAssetMenu(menuName = "Inventory/Health Item")]
	public class HealthItem : BaseItem
	{
		[SerializeField]
		private float healthAmount = 5f; // Amount of health to apply. Use negative values for damage.

		public override bool Use(GameObject user)
		{
			base.Use(user);

			CharacterHealth characterHealth = user.GetComponent<CharacterHealth>();
			if (characterHealth != null)
			{
				characterHealth.TakeDamage(-healthAmount); // Heal the character
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
