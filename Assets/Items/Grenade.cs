using BattleCity.Tanks;
using System.Collections.Generic;


namespace BattleCity.Items
{
	public sealed class Grenade : Item
	{
		private static readonly List<EnemyTank> tmp = new List<EnemyTank>();
		public override void OnCollision(Tank tank)
		{
			if (tank is PlayerTank)
			{
				tmp.Clear();
				tmp.AddRange(EnemyTank.livingTanks);
				foreach (var enemy in tmp) enemy.Explode();
			}
			else foreach (var player in PlayerTank.livingTanks) player.Explode();

			//Effect.Play(Effect.Sound.Grenade);
			Destroy(gameObject);
		}
	}
}