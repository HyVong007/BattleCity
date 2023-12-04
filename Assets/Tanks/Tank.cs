using UnityEngine;


namespace BattleCity.Tanks
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public abstract class Tank : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Tank>> tanks { get; private set; }


		public Color color { get; protected set; }

		public Vector3 direction { get; protected set; }

		public float speed { get; protected set; }

		[SerializeField]
		[HideInInspector]
		protected SpriteRenderer spriteRenderer;

		[SerializeField]
		[HideInInspector]
		protected Animator animator;


		protected void Reset()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator = GetComponent<Animator>();
		}


		public abstract bool OnBulletCollision(Bullet bullet);
	}
}