using UnityEngine;

namespace Isometric2DGame.Items
{
	// Items that are dropped in the game world, such as when a player dies or drops an item
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Collider2D))]
	public class DroppedItem : MonoBehaviour
	{
		// Cache from resource load
		private static GameObject droppedItemPrefab;
		public static GameObject DroppedItemPrefab
		{
			get
			{
				if (droppedItemPrefab == null)
				{
					droppedItemPrefab = Resources.Load<GameObject>("Prefabs/DroppedItem");
				}
				return droppedItemPrefab;
			}
		}

		private Rigidbody2D myRigidbody;
		private Collider2D myCollider;
		private SpriteRenderer mySpriteRenderer;
		private GameObject mySpriteObject;

		[SerializeField]
		private BaseItem item;
		public BaseItem Item
		{
			get { return item; }
			private set
			{
				item = value;
				RefreshDisplay();
			}
		}

		// Animation parameters for the bobbing effect
		// Doesn't need encapsulation as these are insignificant except visually
		// TODO: Should these be read-only static properties?
		// TODO: Replace with unity animation curves or similar 
		public float bobbingSpeed = 2.0f;
		public float bobbingHeight = 0.1f;

		private void Awake()
		{
			// Component caching.
			myRigidbody = GetComponent<Rigidbody2D>();
			myCollider = GetComponent<Collider2D>();
			mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
			mySpriteObject = mySpriteRenderer.gameObject;
		}

		private void Start()
		{
			RefreshDisplay();
		}

		private void OnValidate()
		{
			myRigidbody = GetComponent<Rigidbody2D>();
			myCollider = GetComponent<Collider2D>();
			mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
			mySpriteObject = mySpriteRenderer.gameObject;
			RefreshDisplay();
		}

		private void Update()
		{
			// Anmate the item with a bobbing effect.
			float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
			mySpriteObject.transform.localPosition = new Vector3(mySpriteObject.transform.localPosition.x, newY, mySpriteObject.transform.localPosition.z);
		}

		// Refresh the name and icon of the dropped item.
		public void RefreshDisplay()
		{
			if (Item != null)
			{
				gameObject.name = "Dropped_" + Item.ItemName; // Set the name of the game object to the item's name
				mySpriteRenderer.sprite = Item.ItemIcon; // Set the sprite to the item's icon
			}
		}


		// Make the item fly in a random direction, creating a small arc.
		public void Fly()
		{
			Vector2 randomDirection = Random.insideUnitCircle.normalized;
			float arcHeight = 1.0f;
			float arcSpeed = 5.0f;

			myRigidbody.linearVelocity = new Vector2(randomDirection.x, randomDirection.y + arcHeight).normalized * arcSpeed;
			myRigidbody.angularVelocity = Random.Range(-180f, 180f); // Add some random rotation
		}

		public void PickedUp(GameObject picker)
		{
			if (picker == null || Item == null)
				return;

			if (!picker.CompareTag("Player"))
			{
				// Non-player characters will just use the item directly
				if (!Item.Use(picker))
					return;
			}
			else
			{
				var inv = picker.GetComponent<Characters.Player.PlayerInventory>();
				if (inv != null && inv.AddItem(Item))
				{
					// Added!
				}
				else
				{
					return;
				}
			}

			Destroy(gameObject);
		}

		static void DropItem(BaseItem item, Vector2 position, bool fly = true)
		{
			if (item == null) 
				return;

			GameObject droppedItemObject = GameObject.Instantiate(DroppedItemPrefab, position, droppedItemPrefab.transform.rotation);

			DroppedItem droppedItem = droppedItemObject.GetComponent<DroppedItem>();
			droppedItem.Item = item;

			if (fly)
				droppedItem.Fly();
		}
	}
}
