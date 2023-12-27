using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Star : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is Player) (tank as Player).IncreaseStar(1);
			else foreach (var enemy in Enemy.enemies)
					if (enemy.weapon == Enemy.Weapon.Normal) enemy.weapon = Enemy.Weapon.Star;

			Destroy(gameObject);
		}
	}
}