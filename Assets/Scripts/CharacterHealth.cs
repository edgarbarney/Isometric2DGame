using UnityEngine;

namespace Isometric2DGame.Characters
{
	public class CharacterHealth : MonoBehaviour
	{
		[SerializeField]
		private float health = 10;

		public void TakeDamage(float damage)
		{
			health -= damage;
			if (health <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			// TODO: Handle generic death logic here

			Debug.Log($"{gameObject.name} has died.");
			Destroy(gameObject);
		}
	}
}