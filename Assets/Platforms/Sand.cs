using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Sand : Platform
	{
		public override bool OnBulletCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}