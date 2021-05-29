using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Gun : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is PlayerTank)
			{
				var player = tank as PlayerTank;
				player.star += 3;
				//player.fieryStar += 3;
			}
			else
			{
				var enemy = tank as EnemyTank;
				if (enemy.weapon != EnemyTank.Weapon.Gun) enemy.weapon = EnemyTank.Weapon.Gun;
			}
			Destroy(gameObject);
		}
	}
}