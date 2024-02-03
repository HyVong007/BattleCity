using BattleCity.Platforms;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Threading;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Shovel : Item
	{
		private static Shovel workingShovel;
		private static bool isPlayerCollided;
		public override void OnCollision(Tank tank)
		{
			stopTime = Time.time + delay;
			if (workingShovel)
				if ((tank is Player) == isPlayerCollided)
				{
					Destroy(gameObject);
					return;
				}
				else
				{
					cts.Cancel();
					cts.Dispose();
					cts = new();
				}

			isPlayerCollided = tank is Player;
			Task();
		}


		private static CancellationTokenSource cts = new();
		private static float stopTime;
		[SerializeField] private float delay;

		[Tooltip("[dir][0] : Steel ID. [dir][1]: Brick ID")]
		[SerializeField] private SerializableDictionaryBase<Vector3, ReadOnlyArray<int>> DIR_ID;

		private async void Task()
		{
			workingShovel = this;
			if (this == current) current = null;
			gameObject.SetActive(false);
			using var token = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, BattleField.Token);

		LOOP:
			// Modify platform
			foreach (var eagle in Eagle.list)
			{
				var origin = eagle.transform.position;
				foreach (var kvp in DIR_ID)
				{
					var pos = origin + kvp.Key;
					var platform = Platform.array[(int)pos.x][(int)pos.y];
					if (platform)
						if (platform is Border || platform is Eagle) continue;
						else Destroy(platform.gameObject);

					if (token.IsCancellationRequested) Platform.New(kvp.Value[1], pos);
					else if (isPlayerCollided) Platform.New(kvp.Value[0], pos);
				}
			}

			// Wait
			while (!token.IsCancellationRequested && Time.time < stopTime) await UniTask.DelayFrame(1);
			if (token.IsCancellationRequested)
			{
				if (this)
				{
					Destroy(gameObject);
					if (workingShovel == this) workingShovel = null;
				}
				return;
			}

			// Goto modify platform (with minor changes) => Exit
			cts.Cancel();
			cts.Dispose();
			cts = new();
			goto LOOP;
		}
	}
}
