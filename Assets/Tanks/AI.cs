using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace BattleCity.Tanks
{
	public sealed class AI : MonoBehaviour
	{
		private Tank tank;
		private void Awake()
		{
			tank = GetComponent<Tank>();
		}


		private void OnEnable()
		{
			MoveAndShoot();
		}


		private CancellationTokenSource cts = new();
		private void OnDisable()
		{
			cts.Cancel();
			cts.Dispose();
			cts = new();
		}


		private const int DELAY = 200;
		private async void MoveAndShoot()
		{
			using var token = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, tank.Token, BattleField.Token);
			var vectors = new List<Vector3>
			{
				Vector3.up, Vector3.right, Vector3.down, Vector3.left
			};
			var lastDir = vectors[Random.Range(0, vectors.Count)];
			vectors.Remove(lastDir);

			while (true)
			{
				var dir = vectors[Random.Range(0, vectors.Count)];
				vectors.Add(lastDir);
				vectors.Remove(lastDir = dir);
				tank.direction = dir;
				CheckShoot();
				if (!tank.CanMove(dir))
				{
					await UniTask.Delay(DELAY);
					if (token.IsCancellationRequested) return;
					continue;
				}

				for (int i = Random.Range(1, 21); i > 0; --i)
				{
					await tank.Move(dir);
					if (token.IsCancellationRequested) return;
					CheckShoot();
					if (!tank.CanMove(dir))
					{
						await UniTask.Delay(DELAY);
						if (token.IsCancellationRequested) return;
						break;
					}
				}
			}


			void CheckShoot()
			{
				if (Random.Range(0, 2) == 0) tank.Shoot();
			}
		}
	}
}