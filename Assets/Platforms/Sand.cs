using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Sand : Platform, ITankCollision
	{
		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip) => false;

		public override bool OnCollision(Bullet bullet) => false;


		public void OnCollision(Tank tank)
		{
		}
	}
}