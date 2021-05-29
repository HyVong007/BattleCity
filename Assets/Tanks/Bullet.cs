using BattleCity.Platforms;
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


			private static readonly SystemObjectPool<List<Bullet>> pool = new SystemObjectPool<List<Bullet>>(list => list.Clear());
			public Array(Vector2Int size)
			{
				size.x = size.x * 2 + 4;
				size.y = size.y * 2 + 4;
				rows = new List<Bullet>[size.y];
				pool.Recycle();
				for (int y = 0; y < size.y; ++y) rows[y] = pool.Get();
				columns = new List<Bullet>[size.x];
				for (int x = 0; x < size.x; ++x) columns[x] = pool.Get();
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
				if ((int)_ != _) throw new ArgumentException($"Tọa độ bullet không hợp lệ. {position:0.000000}, {direction}");
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
				  array = new Array(BattleField.stage.size);

				  var prefab = "Bullet".Load<Bullet>();
				  prefab.gameObject.SetActive(false);
				  pool = new ObjectPool<Bullet>(prefab, BattleField.instance.bulletAnchor, BattleField.instance.bulletAnchor);
			  };
		}
		#endregion


		private static ObjectPool<Bullet> pool;
		public static Bullet Spawn(in Data data)
		{
#if DEBUG
			if (data.speed <= 0 || data.speed > 0.125f)
				throw new ArgumentOutOfRangeException($"Tốc độ bullet không hợp lệ: {data.speed}");
#endif
			var bullet = pool.Get(data.position, false);
			bullet.data = data;
			bullet.gameObject.SetActive(true);
			return bullet;
		}


		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private SerializableDictionaryBase<Direction, Sprite> sprites;
		private void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
#if DEBUG
			if (data.movingBullets.Contains(this))
				throw new Exception($"Tank.movingBullets đang chứa this. Không thể thêm {this}");
#endif
			data.movingBullets.Add(this);
			array.Add(this);
			spriteRenderer.sprite = sprites[data.direction];
			pos = pos05 = transform.position;
			velocity = direction.ToUnitVector3();
			v05 = velocity * 0.5f;
			velocity *= data.speed;
			token = BattleField.Token;
		}


		private void OnDisable()
		{
			if (data.movingBullets.Contains(this)) data.movingBullets.Remove(this);
			array.Remove(this);
		}


		#region Move
		private const float TANK_RADIUS = 0.6035533905932738F + 0.001F,
			BULLET_RADIUS = 0.125F + 0.001F,
			BULLET_DIAMETER_SQR = 4 * BULLET_RADIUS * BULLET_RADIUS,
			TANK_BULLET_DISTANCE_SQR = BULLET_RADIUS * BULLET_RADIUS + 2 * BULLET_RADIUS * TANK_RADIUS + TANK_RADIUS * TANK_RADIUS;
		private static readonly List<Platform> tmpPlatforms = new List<Platform>();
		private static readonly List<Bullet> tmpBullets = new List<Bullet>();
		private static readonly List<Tank> tmpTanks = new List<Tank>();
		private Vector3 pos, pos05, velocity, v05;
		private CancellationToken token;


		private void Update()
		{
			if (token.IsCancellationRequested) goto HIDE;
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

			if (posIs05)
			{
				#region Kiểm tra va chạm Platform
				tmpPlatforms.Clear();
				var index = pos.ToVector2Int();
				bool X_INT = index.x == pos.x, Y_INT = index.y == pos.y;
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
				tmpBullets.AddRange(array[pos05, Direction.Right]);
				tmpBullets.AddRange(array[pos05 - v05, Direction.Right]);
			}
			else
			{
				tmpBullets.AddRange(array[pos, Direction.Right]);
				tmpBullets.AddRange(array[pos05, Direction.Up]);
				tmpBullets.AddRange(array[pos05 - v05, Direction.Up]);
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
			{
				var index = pos * 2;
				int minX, maxX, minY, maxY;
				if (direction == Direction.Up || direction == Direction.Down)
				{
					minX = (int)index.x - 1;
					maxX = minX + 2;
					minY = direction == Direction.Up ? (int)index.y + 1 : posIs05 ? (int)index.y - 1 : (int)index.y - 2;
					maxY = minY + (posIs05 ? 0 : 1);
				}
				else
				{
					minY = (int)index.y - 1;
					maxY = minY + 2;
					minX = direction == Direction.Right ? (int)index.x + 1 : posIs05 ? (int)index.x - 1 : (int)index.x - 2;
					maxX = minX + (posIs05 ? 0 : 1);
				}

				tmpTanks.Clear();
				for (int x = minX; x <= maxX; ++x)
					for (int y = minY; y <= maxY; ++y)
					{
						IReadOnlyList<Tank> list = null;

						try
						{
							list = Tank.array[x][y];
						}
						catch { print($"Loi ! index= {index:0.000000}, pos05= {pos05:0.000000}, pos= {pos:0.000000}, direction= {direction}"); throw; }
						if (list.Count != 0) tmpTanks.Add(list[list.Count - 1]);
					}

				if (tmpTanks.Count != 0)
					foreach (var tank in tmpTanks)
						hide |= (tank.transform.position - pos).sqrMagnitude <= TANK_BULLET_DISTANCE_SQR && tank.OnCollision(this);
			}
			#endregion

			if (hide) goto HIDE;
			transform.position = pos += velocity;
			return;

		HIDE:
			pool.Recycle(this);
		}
		#endregion


		public bool OnCollision(Bullet bullet)
		{
			if (owner == Owner.Enemy && bullet.owner == Owner.Enemy) return false;
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
			[Tooltip("0 < speed <= 0.125f")]
			[Range(0.01f, 0.125f)]
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