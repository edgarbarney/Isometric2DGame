using UnityEngine;

namespace Isometric2DGame.Characters
{
	[System.Serializable]
	public class CharacterSpriteSet
	{
		public Sprite NE;
		public Sprite E;
		public Sprite SE;
		public Sprite S;
		public Sprite SW;
		public Sprite W;
		public Sprite NW;
		public Sprite N;

		public Sprite GetSprite(WorldData.Direction direction)
		{
			return direction switch
			{
				WorldData.Direction.NorthEast => NE,
				WorldData.Direction.East => E,
				WorldData.Direction.SouthEast => SE,
				WorldData.Direction.South => S,
				WorldData.Direction.SouthWest => SW,
				WorldData.Direction.West => W,
				WorldData.Direction.NorthWest => NW,
				WorldData.Direction.North => N,
				_ => null
			};
		}
	}

	public class CharacterSpriteManager : MonoBehaviour
	{
		private SpriteRenderer mySpriteRenderer;
		public CharacterSpriteSet spriteSet;

		protected void Awake()
		{
			mySpriteRenderer = GetComponent<SpriteRenderer>();

			if (spriteSet == null)
			{
				spriteSet = new CharacterSpriteSet();
			}
		}

		public void SetSprite(WorldData.Direction direction)
		{
			Sprite newSprite = spriteSet.GetSprite(direction);
			if (newSprite != null)
			{
				mySpriteRenderer.sprite = newSprite;
			}
		}
	}
}
