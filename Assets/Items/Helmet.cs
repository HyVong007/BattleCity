using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Threading;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Helmet : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is Enemy) foreach (var e in Enemy.enemies) ++e.health;
			else
			{
				var player = tank as Player;
				if (player.helmet)
				{
					player.helmet.stopTime = Time.time + delay;
					Destroy(gameObject);
				}
				else Task(player);
			}
		}


		[SerializeField] private float delay;
		[SerializeField] private SerializableDictionaryBase<Color, RuntimeAnimatorController> anims;
		private float stopTime;

		private async void Task(Player player)
		{
			using var token = CancellationTokenSource.CreateLinkedTokenSource(player.Token, BattleField.Token);
			if (this == current) current = null;
			transform.parent = player.transform;
			transform.position = default;
			GetComponent<Animator>().runtimeAnimatorController = anims[player.color];
			player.helmet = this;
			stopTime = Time.time + delay;

			do await UniTask.DelayFrame(1);
			while (!token.IsCancellationRequested && Time.time < stopTime);
			if (!token.IsCancellationRequested) player.helmet = null;
			Destroy(gameObject);
		}
	}
}