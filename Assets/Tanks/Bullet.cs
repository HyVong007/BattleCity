using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Tanks
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Bullet : MonoBehaviour, IBulletCollision
	{
		private static List<Bullet>[] Xs, Ys;

		public Color? color { get; private set; }

		public Vector3 direction { get; private set; }

		private float speed;


		static Bullet()
		{
			BattleField.onAwake += () =>
			{
				Xs = new List<Bullet>[BattleField.level.width * 2];
				Ys = new List<Bullet>[BattleField.level.height * 2];
				for (int x = 0; x < Xs.Length; ++x) Xs[x] = new();
				for (int y = 0; y < Ys.Length; ++y) Ys[y] = new();
			};
		}









		public bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}



	public interface IBulletCollision
	{
		/// <returns><see langword="true"/>: Bullet will be disappeared</returns>
		bool OnCollision(Bullet bullet);
	}
}