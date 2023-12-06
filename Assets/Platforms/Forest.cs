using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Forest : Platform
	{
		[SerializeField]
		private Particle particle;

		public override bool CanMove(Tank tank, Vector3 newDir) => true;


		public override bool OnBulletCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}