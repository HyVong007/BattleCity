using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Life : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is PlayerTank)
			{
				++PlayerTank.lifes[tank.color];
				//Effect.Play(Effect.Sound.Life);
			}
			else UpgradeEnemyColorAndHP();
			Destroy(gameObject);
		}


		public static void UpgradeEnemyColorAndHP()
		{

		}
	}
}