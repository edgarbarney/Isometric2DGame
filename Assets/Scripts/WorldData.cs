using System.Collections.Generic;
using UnityEngine;

namespace Isometric2DGame
{
	[System.Serializable]
	public static class WorldData
	{
		public enum Direction
		{
			NorthEast,
			East,
			SouthEast,
			South,
			SouthWest,
			West,
			NorthWest,
			North,

			Top = NorthEast,
			TopRight = East,
			Right = SouthEast,
			BottomRight = South,
			Bottom = SouthWest,
			BottomLeft = West,
			Left = NorthWest,
			TopLeft = North
		}

		public static readonly Vector2[] directionVectors = new Vector2[]
		{
			new Vector2(0, 1),		// NorthEast (Top)
			new Vector2(1, 1),		// East (TopRight)
			new Vector2(1, 0),		// SouthEast (Right)
			new Vector2(1, -1),		// South (BottomRight)
			new Vector2(0, -1),		// SouthWest (Bottom)
			new Vector2(-1, -1),	// West (BottomLeft)
			new Vector2(-1, 0),		// NorthWest (Left)
			new Vector2(-1, 1),		// North (TopLeft)
		};

		// Returns the direction vector for the given direction enum.
		public static Vector2 GetDirectionVector(Direction direction, bool normalised = true)
		{
			int index = (int)direction;

			if (index >= 0 && index < directionVectors.Length)
				return normalised ? directionVectors[index].normalized : directionVectors[index];

			return Vector2.zero;
		}

		// Rounds given vector to the nearest direction vector.
		public static Direction GetDirectionFromVector(Vector2 vector)
		{
			if (vector == Vector2.zero)
				return Direction.Top;

			float angle = Mathf.Atan2(vector.y, -vector.x) * Mathf.Rad2Deg;
			angle = (angle + 360f - 90f) % 360f;

			int index = Mathf.RoundToInt(angle / 45f) % 8;
			return (Direction)index;
		}
	}
}
