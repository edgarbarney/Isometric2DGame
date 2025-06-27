using UnityEngine;

namespace Isometric2DGame.Items
{
	[CreateAssetMenu(menuName = "Inventory/Item")]
	public class BaseItem : ScriptableObject
	{
		private static GameObject currentPlayer;
		public static GameObject CurrentPlayer
		{
			get
			{
				if (currentPlayer == null)
				{
					currentPlayer = GameObject.FindGameObjectWithTag("Player");
				}
				return currentPlayer;
			}
			private set
			{
				currentPlayer = value;
			}
		}

		[SerializeField]
		private string itemName;
		public string ItemName
		{
			get { return itemName; }
			set { itemName = value; }
		}
		[SerializeField]
		private string itemDescription;
		public string ItemDescription
		{
			get { return itemDescription; }
			set { itemDescription = value; }
		}
		[SerializeField]
		private Sprite itemIcon;
		public Sprite ItemIcon
		{
			get { return itemIcon; }
			set { itemIcon = value; }
		}

		// Use this item in the game world
		// Override this method in derived classes to implement specific item behavior
		public virtual bool Use(GameObject user)
		{
			if (user == null)
				return false;

			return true;
		}
	}
}