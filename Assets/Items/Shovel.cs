using BattleCity.Platforms;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Shovel : Item
	{
		private static CancellationTokenSource cts;
		private static float stopTime;
		[SerializeField] private float enemyDelaySeconds, playerDelaySeconds;
		/// <summary>
		/// Các tọa độ <see cref="Platform"/> (không phải <see cref="Border"/>) bao quanh <see cref="Eagle"/><br/>
		/// [Tọa độ platform] = direction trong prefab
		/// </summary>
		private static readonly IReadOnlyDictionary<Vector2Int, Vector2Int> index_prefabDir = new Dictionary<Vector2Int, Vector2Int>();
		private static Vector3 lastEaglePosition;
		private static Vector2Int lastStageSize;
		private static readonly Vector2Int[] DIRECTIONS = new Vector2Int[]
		{
			Vector2Int.up, Vector2Int.right+Vector2Int.up, Vector2Int.right, Vector2Int.right+Vector2Int.down,
			Vector2Int.down, Vector2Int.down+Vector2Int.left, Vector2Int.left, Vector2Int.left+Vector2Int.up
		};
		public override void OnCollision(Tank tank)
		{
			#region Kiểm tra stage, cập nhật {index_prefabDir}
			var size = "STAGE".GetValue<Stage>().size;
			if (size != lastStageSize || Eagle.instance.transform.position != lastEaglePosition)
			{
				lastEaglePosition = Eagle.instance.transform.position;
				lastStageSize = size;
				var e = lastEaglePosition.ToVector2Int();
				var dict = index_prefabDir as Dictionary<Vector2Int, Vector2Int>;
				dict.Clear();
				foreach (var dir in DIRECTIONS)
				{
					var index = e + dir;
					if (!(Platform.array[index.x][index.y] is Border)) dict[index] = dir * -1;
				}
			}
			#endregion

			if (tank is PlayerTank)
			{
				if (enemyTask.isRunning())
				{
					cts?.Cancel();
					cts?.Dispose();
					cts = null;
				}

				stopTime = Time.time + playerDelaySeconds;
				if (!playerTask.isRunning())
				{
					cts = CancellationTokenSource.CreateLinkedTokenSource(BattleField.Token);
					(playerTask = PlayerTask()).Forget();
				}
			}
			else
			{
				if (playerTask.isRunning())
				{
					cts?.Cancel();
					cts?.Dispose();
					cts = null;
				}

				stopTime = Time.time + enemyDelaySeconds;
				if (!enemyTask.isRunning())
				{
					cts = CancellationTokenSource.CreateLinkedTokenSource(BattleField.Token);
					(enemyTask = EnemyTask()).Forget();
				}
			}

			Destroy(gameObject);
		}


		private static UniTask playerTask;
		private static async UniTask PlayerTask()
		{
			foreach (var index_dir in index_prefabDir)
			{
				var index = index_dir.Key;
				var p = Platform.array[index.x][index.y];
				if (p) Destroy(p.gameObject);
				Instantiate(PlatformPrefabs.instance.steels[index_dir.Value], index.ToVector3(), Quaternion.identity);
			}
			var token = cts.Token;
			await UniTask.WaitUntil(() => token.IsCancellationRequested || Time.time > stopTime);
			if (token.IsCancellationRequested) return;

			foreach (var index_dir in index_prefabDir)
			{
				var index = index_dir.Key;
				var p = Platform.array[index.x][index.y];
				if (p) Destroy(p.gameObject);
				Instantiate(PlatformPrefabs.instance.bricks[index_dir.Value], index.ToVector3(), Quaternion.identity);
			}
		}


		private static UniTask enemyTask;
		private static async UniTask EnemyTask()
		{
			foreach (var index_dir in index_prefabDir)
			{
				var index = index_dir.Key;
				var p = Platform.array[index.x][index.y];
				if (p) Destroy(p.gameObject);
			}
			var token = cts.Token;
			await UniTask.WaitUntil(() => token.IsCancellationRequested || Time.time > stopTime);
			if (token.IsCancellationRequested) return;

			foreach (var index_dir in index_prefabDir)
				Instantiate(PlatformPrefabs.instance.bricks[index_dir.Value], index_dir.Key.ToVector3(), Quaternion.identity);
		}
	}
}