using UnityEngine;

namespace Isometric2DGame.Characters.Player
{
	public class PlayerSpriteManager : CharacterSpriteManager
	{
		public void SetPlayerSprite(WorldData.Direction direction)
		{
			// TODO: Player specific sprite logic
			SetSprite(direction);
		}
	}
}
