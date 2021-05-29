using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Border : Platform
	{
		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip) => true;

		public override bool OnCollision(Bullet bullet) => true;


#if !UNITY_EDITOR && DEBUG
		private new void OnDisable()
		{
			if (BattleField.instance && BattleField.instance.enabled)
				throw new System.Exception("Khổng thể tắt/hủy Border khi màn chơi chưa kết thúc !");
			base.OnDisable();
		}
#endif
	}
}