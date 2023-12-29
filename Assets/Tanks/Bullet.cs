using BattleCity.Platforms;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BattleCity.Tanks
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Bullet : MonoBehaviour, IBulletCollision
	{
		private static List<Bullet>[] Xs, Ys;
		private static ObjectPool<Bullet> pool;

		private List<Bullet> getList =>
			direction.x == 0 ? Xs[(int)(transform.position.x * 2)]
			: Ys[(int)(transform.position.y * 2)];


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			BattleField.onAwake += () =>
			{
				Xs = new List<Bullet>[Main.level.width * 2];
				Ys = new List<Bullet>[Main.level.height * 2];
				for (int x = 0; x < Xs.Length; ++x) Xs[x] = new();
				for (int y = 0; y < Ys.Length; ++y) Ys[y] = new();
				pool = new(Addressables.LoadAssetAsync<GameObject>("Assets/Tanks/Prefab/Bullet.prefab")
					.WaitForCompletion().GetComponent<Bullet>());
			};
		}


		public Color? color { get; private set; }

		public Vector3 direction { get; private set; }

		[SerializeField] private float speed;
		[SerializeField] private int delay;

		public bool canBurnForest { get; private set; }

		public bool canDestroySteel { get; private set; }

		private ValueWrapper<int> count;
		[SerializeField] private SerializableDictionaryBase<Vector3, Sprite> sprites;


		public struct Data
		{
			public Vector3 position, direction;
			public Color? color;
			public ValueWrapper<int> count;
			public bool canBurnForest, canDestroySteel;
		}

		public static Bullet New(in Data data)
		{
			var bullet = pool.Get(data.position);
			bullet.direction = data.direction;
			bullet.GetComponent<SpriteRenderer>().sprite = bullet.sprites[data.direction];
			bullet.getList.Add(bullet);
			bullet.color = data.color;
			bullet.count = data.count;
			bullet.canBurnForest = data.canBurnForest;
			bullet.canDestroySteel = data.canDestroySteel;
			bullet.Move();
			return bullet;
		}


		private CancellationTokenSource cts = new();
		private void OnDisable()
		{
			cts.Cancel();
			cts.Dispose();
			cts = new();
			getList.Remove(this);
			--count.value;
		}


		public bool OnCollision(Bullet bullet)
		{
			if (color == null && bullet.color == null) return false;
			pool.Recycle(this);
			return true;
		}


		private static readonly List<Bullet> tmp = new();
		private async void Move()
		{
			var dir = direction * speed;
			float count = 0.5f / speed;
			int halfCount = (int)count / 2;
			var vectors = DIR_VECTORS[direction];
			var origin = transform.position;
			bool stop = false;
			using var token = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, BattleField.Token);

			while (true)
			{
				#region Check Platform
				for (int v = 0; v < vectors.Length; ++v)
				{
					var pos = origin + vectors[v];
					if (pos.ToVector3Int() != pos) continue;

					var platform = Platform.platforms[(int)pos.x][(int)pos.y];
					if (platform) stop |= platform.OnCollision(this);
				}
				#endregion

				for (float i = 0; i < count; ++i)
				{
					if (i == 0 || i == halfCount)
						#region Check Tank
						for (int v = 0; v < 3; ++v)
						{
							var pos = origin + vectors[v];
							var tank = Tank.tanks[(int)(pos.x * 2)][(int)(pos.y * 2)];
							if (tank && (tank.transform.position - transform.position).sqrMagnitude <= TANK_BULLET_DISTANCE)
								stop |= tank.OnCollision(this);
						}
					#endregion

					#region Check Bullet
					tmp.Clear();
					tmp.AddRange(getList);
					foreach (var bullet in tmp)
						if (this != bullet && (transform.position - bullet.transform.position).sqrMagnitude <= BULLET_BULLET_DISTANCE)
							stop |= bullet.OnCollision(this);

					tmp.Clear();
					tmp.AddRange(Perpendicular(i < halfCount ? origin : origin + direction * 0.5f));
					foreach (var bullet in tmp)
						if ((transform.position - bullet.transform.position).sqrMagnitude <= BULLET_BULLET_DISTANCE)
							stop |= bullet.OnCollision(this);
					#endregion

					if (stop)
					{
						pool.Recycle(this);
						return;
					}
					transform.position += dir;
					await UniTask.Delay(delay);
					if (token.IsCancellationRequested) return;
				}
				transform.position = origin += direction * 0.5f;
			}

			List<Bullet> Perpendicular(in Vector3 pos) =>
				direction.x == 0 ? Ys[(int)(pos.y * 2)] : Xs[(int)(pos.x * 2)];
		}

		private static readonly IReadOnlyDictionary<Vector3, ReadOnlyArray<Vector3>>
			DIR_VECTORS = new Dictionary<Vector3, ReadOnlyArray<Vector3>>
			{
				[Vector3.up] = new(new Vector3[] { new(-0.5f, 0.5f), new(0, 0.5f), new(0.5f, 0.5f), new(-0.5f, 0), default, new(0.5f, 0) }),
				[Vector3.right] = new(new Vector3[] { new(0.5f, 0.5f), new(0.5f, 0), new(0.5f, -0.5f), new(0, 0.5f), default, new(0, -0.5f) }),
				[Vector3.down] = new(new Vector3[] { new(-0.5f, -0.5f), new(0, -0.5f), new(0.5f, -0.5f), new(-0.5f, 0), default, new(0.5f, 0) }),
				[Vector3.left] = new(new Vector3[] { new(-0.5f, 0.5f), new(-0.5f, 0), new(-0.5f, -0.5f), new(0, 0.5f), default, new(0, -0.5f) })
			};

		private static readonly float TANK_BULLET_DISTANCE = Mathf.Pow(0.71f + 0.2f, 2),
			BULLET_BULLET_DISTANCE = Mathf.Pow(0.2f * 2, 2);
	}



	public interface IBulletCollision
	{
		/// <returns><see langword="true"/>: Bullet will be disappeared</returns>
		bool OnCollision(Bullet bullet);
	}
}