using BattleCity.Tanks;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Forest : Platform
	{
		[SerializeField] private ParticleArray particles;
		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip) => false;


		private static readonly List<GameObject> tmp = new List<GameObject>();
		public override bool OnCollision(Bullet bullet)
		{
			if (!bullet.canBurnForest) return false;

			FindBulletCollisionObjs(bullet.transform.position - transform.position, particles, tmp);
			if (tmp.Count == 0) return false;

			foreach (var obj in tmp)
			{
				obj.SetActive(false);
				Destroy(obj);
			}
			if (particles.isEmpty) Destroy(gameObject);
			// Sound

			return false;
		}
	}
}