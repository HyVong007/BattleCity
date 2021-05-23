using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Water : Platform
	{
		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip) => !tankHasShip;

		public override bool OnCollision(Bullet bullet) => false;
	}
}