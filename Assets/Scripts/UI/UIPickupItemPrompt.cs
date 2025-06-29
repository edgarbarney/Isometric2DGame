using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Isometric2DGame.Characters.Player;
using Isometric2DGame.Items;

namespace Isometric2DGame.UI
{
	public class UIPickupItemPrompt : MonoBehaviour
	{
		public TextMeshProUGUI pickupKey;
		public TextMeshProUGUI itemNameText;

		public void Awake()
		{
			if (pickupKey == null || itemNameText == null)
			{
				Debug.LogError("UIPickupItemPrompt: Missing required PickupItemPrompt components. Please assign them in the inspector.");
			}
		}

		public void RefreshPickupKey(PlayerController controller)
		{
			if (controller == null || controller.MyPlayerInput == null)
			{
				pickupKey.text = "";
				return;
			}

			// Ugh, this is a bit hacky, but it works for now.
			pickupKey.text = controller.MyPlayerInput.actions["Interact"].bindings.Count > 0 ? InputControlPath.ToHumanReadableString(controller.MyPlayerInput.actions["Interact"].bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice) : "";
		}

		public void SetPickupItem(DroppedItem item)
		{
			itemNameText.text = item != null ? item.Item.ItemName : "???";
		}

		public void SetPosition(Vector2 pos)
		{
			(transform as RectTransform).position = pos;
		}
	}
}
