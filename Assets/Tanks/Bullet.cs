using UnityEngine;


namespace BattleCity.Tanks
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Bullet : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Bullet>> bullets { get; private set; }

		public Color? color { get; private set; }

		public Vector3 direction { get; private set; }

		private float speed;


		public bool OnBulletCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}



	public interface IBulletCollision
	{
		/// <returns><see langword="true"/>: Bullet will be disappeared</returns>
		bool OnBulletCollision(Bullet bullet);
	}
}