using System;
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
		[SerializeField]
		private int maxStack = 1; // Maximum number of itemSlots that can be stacked in the inventory. MUST be greater than 0. If it's 1, its not stackable
		public int MaxStack
		{
			get { return maxStack; }
			set { maxStack = Mathf.Max(value, 1); }
		}

		// Use this item in the game world
		// Override this method in derived classes to implement specific item behavior
		public virtual bool Use(GameObject user)
		{
			if (user == null)
				return false;

			return true;
		}

		public bool IsStackable()
		{
			return maxStack > 1;
		}

		// == Equality and Hashing ==
		//
		// We verride equality and hash code methods for proper comparison.
		// We don't care about the instance ID of the ScriptableObject, just the data it contains.
		// We also ignore sprite, as it's not that relevant for equality checks.
		//
		// ==========================

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true; // Same instance. No need to compare.

			if (obj is not BaseItem other)
				return false;

			return string.Equals(itemName, other.itemName) &&
				   string.Equals(itemDescription, other.itemDescription) &&
				   maxStack == other.maxStack;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(itemName, itemDescription, maxStack);
		}

		public static bool operator ==(BaseItem a, BaseItem b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (a is null || b is null) return false; // Not ==, is will prevent recursive calls.
			return a.Equals(b);
		}

		public static bool operator !=(BaseItem a, BaseItem b) => !(a == b);

	}
}