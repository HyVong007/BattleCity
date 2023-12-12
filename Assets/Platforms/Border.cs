using BattleCity.Tanks;
using UnityEngine;

namespace BattleCity.Platforms
{
	public sealed class Border : Platform
	{
		public override bool CanMove(Tank tank, Vector3 newDir) => false;


		public override bool OnCollision(Bullet bullet) => true;
	}
}