using UnityEngine;

namespace Isometric2DGame.Items
{
	public class ItemPickupHelper : MonoBehaviour
	{
		void OnCollisionEnter2D(Collision2D col)
		{
			if (col.gameObject.CompareTag("ItemPickup"))
			{
				col.gameObject.GetComponent<DroppedItem>().PickedUp(gameObject);
			}
		}
	}
}
