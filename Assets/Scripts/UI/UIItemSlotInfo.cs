using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Isometric2DGame.Items;
using Isometric2DGame.Characters.Player;

namespace Isometric2DGame.UI
{
	public class UIItemSlotInfo : MonoBehaviour
	{
		public TextMeshProUGUI itemNameText;
		public TextMeshProUGUI itemDescText;

		public void SetItem(BaseItem item)
		{
			if (item == null)
			{
				itemNameText.text = "";
				itemDescText.text = "";
				return;
			}
			itemNameText.text = item.ItemName;
			itemDescText.text = item.ItemDescription;
		}

		private void Update()
		{
			if (Mouse.current != null && Mouse.current.position != null)
			{
				Vector2 mousePos = Mouse.current.position.ReadValue();
				transform.position = mousePos;
			}
		}
	}
}
