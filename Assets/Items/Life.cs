using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Life : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is Player) ++BattleField.playerLifes[tank.color];
			else foreach (var e in Enemy.enemies) ++e.health;
		}
	}
}