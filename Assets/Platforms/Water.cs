using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public sealed class Water : Platform
	{
		public override bool CanMove(Tank tank, Vector3 newDir) => tank.hasShip;


		public override bool OnCollision(Bullet bullet) => false;
	}
}