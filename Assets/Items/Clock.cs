using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Clock : Item
	{
		private static Clock workingClock;

		public override void OnCollision(Tank tank)
		{
			if (workingClock) Destroy(gameObject);
			else
			{
				if (this == current) current = null;
				gameObject.SetActive(false);
				workingClock = this;
			}

			if (tank is Player)
			{
				enemyStopTime = Time.time + enemyDelayTime;
				if (!freezeEnemy.isRunning()) freezeEnemy = FreezeEnemy();
			}
			else
			{
				playerStopTime = Time.time + playerDelayTime;
				if (!freezePlayer.isRunning()) freezePlayer = FreezePlayer();
			}
		}


		[SerializeField] private float enemyDelayTime;
		private static float enemyStopTime;
		private static UniTask freezeEnemy;

		private async UniTask FreezeEnemy()
		{
			var token = BattleField.Token;
			AI.SetActive<Enemy>(false);

			do await UniTask.DelayFrame(1);
			while (!token.IsCancellationRequested && Time.time < enemyStopTime);
			if (token.IsCancellationRequested) return;

			AI.SetActive<Enemy>(true);
			if (!freezePlayer.isRunning())
			{
				Destroy(workingClock.gameObject);
				workingClock = null;
			}
		}


		[SerializeField] private float playerDelayTime;
		private static float playerStopTime;
		private static UniTask freezePlayer;

		private async UniTask FreezePlayer()
		{
			var token = BattleField.Token;
			AI.SetActive<Player>(false);
			// Player: hủy input move, giữ input shoot

			do await UniTask.DelayFrame(1);
			while (!token.IsCancellationRequested && Time.time < playerStopTime);
			if (token.IsCancellationRequested) return;

			if (!freezeEnemy.isRunning())
			{
				Destroy(workingClock.gameObject);
				workingClock = null;
			}
		}
	}
}