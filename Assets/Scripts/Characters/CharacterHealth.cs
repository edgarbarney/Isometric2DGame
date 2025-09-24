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

		[SerializeField]
		private float maxHealth = 10;
		public float MaxHealth
		{
			get => maxHealth;
			protected set { maxHealth = value; }
		}

		private bool isDead = false;
		public bool IsDead
		{
			get => isDead;
			//protected set { isDead = value; }
		}

		public GameObject characterCorpsePrefab;
		public SpriteRenderer healthBarRenderer;
		private Transform healthBarFill;

		private void Awake()
		{
			if (healthBarRenderer != null)
			{
				healthBarFill = healthBarRenderer.transform.Find("FillParent");
				if (healthBarFill == null)
				{
					Debug.LogWarning("Health bar fill not found. Make sure there is a child object named 'Fill' under the health bar renderer.");
				}
			}
		}

		private void Start()
		{
			health = Mathf.Clamp(health, 0, maxHealth);
			TakeDamage(0); // Update stuff, kill the character if necessary.
			//UpdateHealthBar();
		}

		public virtual void TakeDamage(float damage)
		{
			health -= damage;

			UpdateHealthBar();

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
			Die();
			isDead = true;
		}

		protected virtual void UpdateHealthBar()
		{
			if (healthBarFill != null)
			{
				healthBarFill.localScale = new Vector3(Mathf.Clamp01(health / maxHealth), 1, 1);
			}
		}
	}
}