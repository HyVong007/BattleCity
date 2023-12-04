using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public sealed class Water : Platform
	{
		public override bool OnBulletCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}