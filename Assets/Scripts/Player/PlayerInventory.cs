using UnityEngine;
using Isometric2DGame.Items;

namespace Isometric2DGame.Characters.Player
{
	public class PlayerInventory : MonoBehaviour
	{


		public bool AddItem(BaseItem item)
		{
			if (item == null)
				return false;

			if (IsFull())
				return false;

			Debug.Log($"Item {item.name} added to inventory.");

			return true;
		}

		public bool IsFull()
		{
			return false;
		}
	}
}
