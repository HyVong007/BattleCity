using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Forest : Platform
	{
		[SerializeField] private ParticleArray particles;
		public override bool IsBlockingTankMove()
		{
			throw new System.NotImplementedException();
		}


		public override bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}