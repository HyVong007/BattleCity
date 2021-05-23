using BattleCity.Tanks;


namespace BattleCity.AI
{
	public sealed class EnemyAgent : Agent
	{
		private void FixedUpdate()
		{
			foreach (var enemy in EnemyTank.currentEnemies)
			{
				if (!IsTankMoving(enemy)) CheckMove(enemy);
				CheckShoot(enemy);
			}
		}
	}
}