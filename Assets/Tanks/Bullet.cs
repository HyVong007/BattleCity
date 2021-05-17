using BattleCity.Platforms;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;


namespace BattleCity.Tanks
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Bullet : MonoBehaviour, IBulletCollision
	{
		#region Init
		private readonly struct Array
		{
			private readonly List<Bullet>[] rows, columns;

			public Vector2Int size { get; }

			public Vector2 worldSize { get; }

			public IEnumerable<Bullet> this[in Vector2Int index, Direction direction] => GetList(index, direction);

			public IEnumerable<Bullet> this[in Vector3 position, Direction direction] => GetList(position, direction);

#if !DEBUG
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
			public void Add(Bullet bullet)
			{
				var list = GetList(bullet.transform.position, bullet.direction);
#if DEBUG
				if (list.Contains(bullet))
					throw new InvalidOperationException($"Không thể thêm bullet. Array đang chứa {bullet}");
#endif
				list.Add(bullet);
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove(Bullet bullet) => GetList(bullet.transform.position, bullet.direction).Remove(bullet);


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Clear(in Vector2Int index, Direction direction) => GetList(index, direction).Clear();


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Clear(in Vector3 position, Direction direction) => GetList(position, direction).Clear();


			public void Clear()
			{
				for (int i = 0; i < rows.Length; ++i) rows[i].Clear();
				for (int i = 0; i < columns.Length; ++i) columns[i].Clear();
			}


			public Array(Vector2Int size)
			{
				size.x = size.x * 2 + 2;
				size.y = size.y * 2 + 2;
				rows = new List<Bullet>[size.y];
				for (int y = 0; y < size.y; ++y) rows[y] = new List<Bullet>();
				columns = new List<Bullet>[size.x];
				for (int x = 0; x < size.x; ++x) columns[x] = new List<Bullet>();
				this.size = default;
				worldSize = default;
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private List<Bullet> GetList(in Vector2Int index, Direction direction)
				=> direction == Direction.Up || direction == Direction.Down ? columns[index.x] : rows[index.y];


#if !DEBUG
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
			private List<Bullet> GetList(in Vector3 position, Direction direction)
			{
#if DEBUG
				float _ = direction == Direction.Up || direction == Direction.Down ? position.x * 2 : position.y * 2;
				if ((int)_ != _) throw new ArgumentException($"{position} không phải dạng 0.5*N với N nguyên không âm !");
#endif
				return direction == Direction.Up || direction == Direction.Down ? columns[(int)(position.x * 2)] : rows[(int)(position.y * 2)];
			}
		}
		private static Array array;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			BattleField.awake += () =>
			  {
				  MAX_BULLET_POSITION = new Vector3(BattleField.map.size.x + 1, BattleField.map.size.y + 1);
				  array = new Array(BattleField.map.size);

				  // Fix bug Addressables 1334114
				  var prefab = BattleField.instance.bulletAnchor.GetChild(0).GetComponent<Bullet>();
				  pool = new ObjectPool<Bullet>(prefab, BattleField.instance.bulletAnchor, BattleField.instance.bulletAnchor);
			  };
		}
		#endregion


		private static ObjectPool<Bullet> pool;
		/// <returns>bullet có thể còn sống (bullet.isActiveAndEnabled==<see langword="true"/>) hoặc đã bị phá hủy (bullet.isActiveAndEnabled==<see langword="false"/>)</returns>
		public static Bullet Spawn(in Data data)
		{
#if DEBUG
			if (data.speed <= 0 || data.speed > 0.5f)
				throw new ArgumentOutOfRangeException($"Tốc độ bullet không hợp lệ: {data.speed}");
#endif
			var bullet = pool.Get(data.position, false);
			bullet.data = data;
			bullet.gameObject.SetActive(true);
			return bullet;
		}


		#region Move
		private const float TANK_RADIUS = 0.6035533905932738F + 0.001F,
			BULLET_RADIUS = 0.125F + 0.001F,
			BULLET_DIAMETER_SQR = 4 * BULLET_RADIUS * BULLET_RADIUS,
			TANK_BULLET_DISTANCE_SQR = BULLET_RADIUS * BULLET_RADIUS + 2 * BULLET_RADIUS * TANK_RADIUS + TANK_RADIUS * TANK_RADIUS;
		private static readonly List<Platform> tmpPlatforms = new List<Platform>();
		private static readonly List<Bullet> tmpBullets = new List<Bullet>();
		private static readonly List<Tank> tmpTanks = new List<Tank>();


		private async UniTask Move()
		{
			var token = Token;
			var pos = transform.position;
			var pos05 = pos;
			var velocity = direction.ToUnitVector3();
			var v05 = velocity * 0.5f;
			velocity *= data.speed;

			do
			{
				bool posIs05 = false;

				#region Chuẩn hóa pos
				switch (direction)
				{
					case Direction.Up:
						if (pos.y >= pos05.y)
						{
							transform.position = pos = pos05;
							pos05 += v05;
							posIs05 = true;
						}
						break;

					case Direction.Right:
						if (pos.x >= pos05.x)
						{
							transform.position = pos = pos05;
							pos05 += v05;
							posIs05 = true;
						}
						break;

					case Direction.Down:
						if (pos.y <= pos05.y)
						{
							transform.position = pos = pos05;
							pos05 += v05;
							posIs05 = true;
						}
						break;

					case Direction.Left:
						if (pos.x <= pos05.x)
						{
							transform.position = pos = pos05;
							pos05 += v05;
							posIs05 = true;
						}
						break;
				}
				#endregion

				bool hide = false;
				var index = pos.ToVector2Int();
				bool X_INT = index.x == pos.x, Y_INT = index.y == pos.y;
				if (posIs05)
				{
					#region Kiểm tra va chạm Platform
					tmpPlatforms.Clear();
					Platform p;

					switch (direction)
					{
						case Direction.Up:
							if (p = Platform.array[index.x][index.y]) tmpPlatforms.Add(p);
							if (X_INT && (p = Platform.array[index.x - 1][index.y])) tmpPlatforms.Add(p);
							break;

						case Direction.Right:
							if (p = Platform.array[index.x][index.y]) tmpPlatforms.Add(p);
							if (Y_INT && (p = Platform.array[index.x][index.y - 1])) tmpPlatforms.Add(p);
							break;

						case Direction.Down:
							int y = index.y == pos.y ? index.y - 1 : index.y;
							if (p = Platform.array[index.x][y]) tmpPlatforms.Add(p);
							if (X_INT && (p = Platform.array[index.x - 1][y])) tmpPlatforms.Add(p);
							break;

						case Direction.Left:
							int x = index.x == pos.x ? index.x - 1 : index.x;
							if (p = Platform.array[x][index.y]) tmpPlatforms.Add(p);
							if (Y_INT && (p = Platform.array[x][index.y - 1])) tmpPlatforms.Add(p);
							break;
					}

					if (tmpPlatforms.Count != 0)
					{
						bool colliseBorder = false;
						foreach (var platform in tmpPlatforms)
						{
							colliseBorder |= platform is Border;
							hide |= platform.OnCollision(this);
						}
						if (colliseBorder)
						{
							// Effect SmallExplosion
							// Nếu bullet không phải Enemy -> Sound: Bullet_Collise_Wall
							goto HIDE;
						}
					}
					#endregion
				}

				#region Kiểm tra va chạm Bullet
				tmpBullets.Clear();
				if (posIs05)
				{
					tmpBullets.AddRange(array[pos, Direction.Up]);
					tmpBullets.AddRange(array[pos, Direction.Right]);
				}
				else if (direction == Direction.Up || direction == Direction.Down)
				{
					tmpBullets.AddRange(array[pos, Direction.Up]);
					if (BulletInsideBattle(pos05)) tmpBullets.AddRange(array[pos05, Direction.Right]);
					var p = pos05 - v05;
					if (BulletInsideBattle(p)) tmpBullets.AddRange(array[p, Direction.Right]);
				}
				else
				{
					tmpBullets.AddRange(array[pos, Direction.Right]);
					if (BulletInsideBattle(pos05)) tmpBullets.AddRange(array[pos05, Direction.Up]);
					var p = pos05 - v05;
					if (BulletInsideBattle(p)) tmpBullets.AddRange(array[p, Direction.Up]);
				}

				while (tmpBullets.Contains(this)) tmpBullets.Remove(this);
				if (tmpBullets.Count != 0)
				{
					bool hideByBullets = false;
					foreach (var bullet in tmpBullets)
						hideByBullets |= !data.movingBullets.Contains(bullet)
							&& (bullet.transform.position - pos).sqrMagnitude <= BULLET_DIAMETER_SQR
							&& bullet.OnCollision(this);

					if (hideByBullets) goto HIDE;
				}
				#endregion

				#region Kiểm tra va chạm Tank
				tmpTanks.Clear();
				switch (direction)
				{
					case Direction.Up:
						{
							int y = Y_INT ? index.y : index.y + 1;
							AddTank(index.x, y);
							AddTank(X_INT ? index.x - 1 : index.x + 1, y);
						}
						break;

					case Direction.Right:
						{
							int x = X_INT ? index.x : index.x + 1;
							AddTank(x, index.y);
							AddTank(x, Y_INT ? index.y - 1 : index.y + 1);
						}
						break;

					case Direction.Down:
						{
							int y = Y_INT ? index.y - 1 : index.y;
							AddTank(index.x, y);
							AddTank(X_INT ? index.x - 1 : index.x + 1, y);
						}
						break;

					case Direction.Left:
						{
							int x = X_INT ? index.x - 1 : index.x;
							AddTank(x, index.y);
							AddTank(x, Y_INT ? index.y - 1 : index.y + 1);
						}
						break;
				}

				if (tmpTanks.Count != 0)
					foreach (var tank in tmpTanks)
						hide |= (tank.transform.position - pos).sqrMagnitude <= TANK_BULLET_DISTANCE_SQR && tank.OnCollision(this);


				static void AddTank(int x, int y)
				{
					var list = Tank.array[x][y];
					if (list.Count != 0) tmpTanks.Add(list[list.Count - 1]);
				}
				#endregion

				if (hide) goto HIDE;
				transform.position = pos += velocity;
				await UniTask.Yield();
			} while (!token.IsCancellationRequested);

		HIDE:
			if (!token.IsCancellationRequested) gameObject.SetActive(false);
		}


		private static Vector3 MAX_BULLET_POSITION;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool BulletInsideBattle(in Vector3 position)
			=> position.x > 1 && position.x < MAX_BULLET_POSITION.x && position.y > 1 && position.y < MAX_BULLET_POSITION.y;
		#endregion


		private CancellationTokenSource ΔcancelSource;
		public CancellationToken Token => ΔcancelSource.Token;
		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private SerializableDictionaryBase<Direction, Sprite> sprites;
		private async void OnEnable()
		{
#if DEBUG
			if (data.movingBullets.Contains(this))
				throw new Exception($"Tank.movingBullets đang chứa this. Không thể thêm {this}");
#endif
			data.movingBullets.Add(this);
			array.Add(this);
			spriteRenderer.sprite = sprites[data.direction];
			ΔcancelSource = CancellationTokenSource.CreateLinkedTokenSource(BattleField.Token);
			await Move();
		}


		private void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
			if (data.movingBullets.Contains(this)) data.movingBullets.Remove(this);
			array.Remove(this);
		}


		public bool OnCollision(Bullet bullet)
		{
			if (owner == Owner.Enemy && owner == bullet.owner) return false;
			pool.Recycle(this);
			return true;
		}


		#region Data
		[Serializable]
		public struct Data
		{
			public Vector3 position;
			public Direction direction;
			public Owner owner;
			public List<Bullet> movingBullets;
			public float speed;
			public bool canDestroySteel, canBurnForest;
		}
		private Data data;


		public Direction direction => data.direction;

		public enum Owner
		{
			YellowPlayer = 0, GreenPlayer = 1, WhitePlayer = 2, Enemy = 3
		}
		public Owner owner => data.owner;

		public bool canDestroySteel => data.canDestroySteel;

		public bool canBurnForest => data.canBurnForest;
		#endregion
	}



	public interface IBulletCollision
	{
		/// <returns><see langword="true"/> nếu bullet sẽ bị phá hủy sau va chạm</returns>
		bool OnCollision(Bullet bullet);
	}
}