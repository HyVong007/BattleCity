using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Star : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is PlayerTank)
			{
				var player = tank as PlayerTank;
				++player.star;
				//++player.fieryStar;
			}
			else foreach (var enemy in EnemyTank.livingTanks)
					if (enemy.weapon == EnemyTank.Weapon.Normal) enemy.weapon = EnemyTank.Weapon.Star;
			
			Destroy(gameObject);
		}
	}
}