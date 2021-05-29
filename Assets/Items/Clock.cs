using BattleCity.AI;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Clock : Item
	{
		[SerializeField] private int enemyFreezeMilisec, playerFreezeMilisec;
		public override void OnCollision(Tank tank)
		{
			if (tank is PlayerTank)
			{
				enemyStopTime = Time.time + enemyFreezeMilisec * 0.001f;
				if (!taskFreezeEnemies.isRunning()) (taskFreezeEnemies = FreezeEnemies()).Forget();
			}
			else foreach (var player in PlayerTank.livingTanks) player.Freeze(playerFreezeMilisec);

			Destroy(gameObject);
		}


		private static float enemyStopTime;
		private static UniTask taskFreezeEnemies;
		private static async UniTask FreezeEnemies()
		{
			var token = BattleField.Token;
			Agent.GetInstance<EnemyAgent>().enabled = false;
			await UniTask.WaitUntil(() => token.IsCancellationRequested || Time.time > enemyStopTime);
			if (token.IsCancellationRequested || BattleField.isPause) return;
			Agent.GetInstance<EnemyAgent>().enabled = true;
		}
	}
}