using UnityEngine;

namespace Isometric2DGame.Characters.Player
{
	public class PlayerHealth : CharacterHealth
	{
		protected override void Die()
		{
			PlayerInventory.Instance.DropEveryItem();
			PlayerInventory.Instance.ToggleInventoryUI(false);

			SpawnCorpse();
			Destroy(gameObject);
		}
	}
}
