using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Bomb : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is Player) while (Enemy.enemies.Count != 0) Enemy.enemies[0].Explode();
			else
			{
				if (Player.players[Color.Yellow].gameObject.activeSelf)
					Player.players[Color.Yellow].Explode();

				if (Player.players[Color.Green].gameObject.activeSelf)
					Player.players[Color.Green].Explode();
			}

			Destroy(gameObject);
		}
	}
}