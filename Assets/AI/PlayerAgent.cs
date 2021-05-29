using BattleCity.Tanks;
using System;


namespace BattleCity.AI
{
	public sealed class PlayerAgent : Agent
	{
		public static ReadOnlyArray<Tank.Color> colors { get; private set; } = new ReadOnlyArray<Tank.Color>(Array.Empty<Tank.Color>());
		private new void Awake()
		{
			base.Awake();
			colors = new ReadOnlyArray<Tank.Color>(BattleField.instance.aiPlayerColors);
		}


		private void OnDestroy()
		{
			colors = new ReadOnlyArray<Tank.Color>(Array.Empty<Tank.Color>());
		}


		private void FixedUpdate()
		{
			foreach (var player in PlayerTank.livingTanks)
			{
				if (!colors.Contains(player.color)) continue;
				CheckShoot(player);
				if (!player.isFreeze && !IsTankMoving(player)) CheckMove(player);
			}
		}


		public static void DecideBorrowingLife(Tank.Color color)
		{

		}
	}
}