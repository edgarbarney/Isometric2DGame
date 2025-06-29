using UnityEngine;

namespace Isometric2DGame.Characters
{
	public class CharacterHealth : MonoBehaviour
	{
		[SerializeField]
		private float health = 10;
		public float Health
		{
			get => health;
			protected set { health = value; }
		}

		private bool isDead = false;
		public bool IsDead
		{
			get => isDead;
			//protected set { isDead = value; }
		}

		public GameObject characterCorpsePrefab;

		public virtual void TakeDamage(float damage)
		{
			health -= damage;
			if (health <= 0)
			{
				StartDying();
			}
		}

		protected void SpawnCorpse()
		{
			if (characterCorpsePrefab != null)
			{
				Instantiate(characterCorpsePrefab, transform.position, characterCorpsePrefab.transform.rotation);
			}
			else
			{
				Debug.LogWarning("SpawnCorpse is called but character corpse prefab is not assigned.");
			}
		}

		// Override this method to handle specific death logic for different character types
		protected virtual void Die()
		{
			SpawnCorpse();
			Destroy(gameObject);
		}

		protected void StartDying()
		{
			Debug.Log($"{gameObject.name} has died.");
			Die();
			isDead = true;
		}
	}
}