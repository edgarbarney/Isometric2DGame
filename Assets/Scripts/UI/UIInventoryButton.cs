using System.Runtime.Serialization;
using UnityEngine;
using Isometric2DGame.Characters.Player;

namespace Isometric2DGame.UI
{
	public class UIInventoryButton : MonoBehaviour
	{
		public void OnClick_Use()
		{
			PlayerInventory.Instance.UseUISelectedItem();
		}

		public void OnClick_Drop()
		{
			PlayerInventory.Instance.DropUISelectedItem();
		}
	}
}
