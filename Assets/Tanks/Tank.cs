using BattleCity.Items;
using BattleCity.Platforms;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;


namespace BattleCity.Tanks
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public abstract class Tank : MonoBehaviour, IBulletCollision
	{
		protected static TankResources resources { get; private set; }
		public static ReadOnlyArray<ReadOnlyArray<IReadOnlyList<Tank>>> array { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			resources = "Tank Resources".LoadAsset<TankResources>();

			var pool = new SystemObjectPool<List<Tank>>(list => list.Clear());
			BattleField.awake += () =>
			  {
				  var size = "STAGE".GetValue<Stage>().size;
				  size.x = size.x * 2 + 4;
				  size.y = size.y * 2 + 4;
				  var _array = new List<Tank>[size.x][];
				  var a = new ReadOnlyArray<IReadOnlyList<Tank>>[size.x];
				  pool.Recycle();
				  for (int x = 0; x < size.x; ++x)
				  {
					  a[x] = new ReadOnlyArray<IReadOnlyList<Tank>>(_array[x] = new List<Tank>[size.y]);
					  for (int y = 0; y < size.y; ++y) _array[x][y] = pool.Get();
				  }
				  array = new ReadOnlyArray<ReadOnlyArray<IReadOnlyList<Tank>>>(a);
			  };
		}


		#region Move
		public abstract Direction direction { get; protected set; }
		[SerializeField] protected float moveSpeed;
		[SerializeField] private int delayMoving;
		private Vector2Int index;


		public async UniTask Move(Direction direction, int step)
		{
#if DEBUG
			if (step <= 0) throw new ArgumentOutOfRangeException($"step = {step} không hợp lệ !");
#endif
			var token = Token;
			this.direction = direction;
			var vi = direction.ToUnitVector3().ToVector2Int();
			float lastMoveSpeed = moveSpeed;
			var v = direction.ToUnitVector3() * lastMoveSpeed;
			float moveCount = 0.5f / lastMoveSpeed;

			int s = 0;
			while (true)
			{
				(array[index.x][index.y] as List<Tank>).Remove(this);
				index += vi;
				(array[index.x][index.y] as List<Tank>).Add(this);

				for (float m = moveCount; m > 0; --m)
				{
					transform.position += v;
					await UniTask.Delay(delayMoving, cancellationToken: token);
				}
#if DEBUG
				if (this.direction != direction)
					throw new InvalidOperationException($"Tank.direction thay đổi khi đang Move. direction ={this.direction}");
#endif
				transform.position = new Vector3(index.x * 0.5f, index.y * 0.5f);

				#region Kiểm tra va chạm Item
				if ((Setting.enemy_CanCollise_Item || (this is PlayerTank))
				&& Item.current && Item.current.IsCollidedTank(transform.position))
				{
					Item.current.OnCollision(this);
				}
				#endregion

				#region Kiểm tra va chạm Platform : ITankCollison

				#endregion

				if (++s == step || !canMove) break;
				moveCount = moveSpeed != lastMoveSpeed ? 0.5f / (lastMoveSpeed = moveSpeed) : moveCount;
			}
		}


		public bool canMove => CanMove(transform.position, direction, ship);


		private static readonly List<Platform> tmpPlatforms = new List<Platform>();
		public static bool CanMove(in Vector3 position, Direction direction, bool hasShip)
		{
#if DEBUG
			position.ThrowIfInvalid();
#endif
			#region Kiểm tra va chạm Platform
			var index = position.ToVector2Int();
			Platform p;
			tmpPlatforms.Clear();
			switch (direction)
			{
				case Direction.Up:
					{
						int y = (index.y == position.y) ? index.y : index.y + 1;
						if (p = Platform.array[index.x][y]) tmpPlatforms.Add(p);
						if (index.x == position.x && (p = Platform.array[index.x - 1][y])) tmpPlatforms.Add(p);
					}
					break;

				case Direction.Right:
					{
						int x = (index.x == position.x) ? index.x : index.x + 1;
						if (p = Platform.array[x][index.y]) tmpPlatforms.Add(p);
						if (index.y == position.y && (p = Platform.array[x][index.y - 1])) tmpPlatforms.Add(p);
					}
					break;

				case Direction.Down:
					{
						if (p = Platform.array[index.x][index.y - 1]) tmpPlatforms.Add(p);
						if (index.x == position.x && (p = Platform.array[index.x - 1][index.y - 1])) tmpPlatforms.Add(p);
					}
					break;

				case Direction.Left:
					{
						if (p = Platform.array[index.x - 1][index.y]) tmpPlatforms.Add(p);
						if (index.y == position.y && (p = Platform.array[index.x - 1][index.y - 1])) tmpPlatforms.Add(p);
					}
					break;
			}

			if (tmpPlatforms.Count != 0)
				foreach (var platform in tmpPlatforms)
					if (platform.IsBlockingTankMove(position, direction, hasShip)) return false;
			#endregion

			#region Kiểm tra va chạm Tank
			var tankIndex = (position * 2).ToVector2Int(); ;
			if (direction == Direction.Up || direction == Direction.Down)
			{
				int y = tankIndex.y + (direction == Direction.Up ? 2 : -2);
				if (array[tankIndex.x - 1][y].Count != 0
					|| array[tankIndex.x][y].Count != 0
					|| array[tankIndex.x + 1][y].Count != 0) return false;
			}
			else
			{
				int x = tankIndex.x + (direction == Direction.Right ? 2 : -2);
				if (array[x][tankIndex.y - 1].Count != 0
					|| array[x][tankIndex.y].Count != 0
					|| array[x][tankIndex.y + 1].Count != 0) return false;
			}
			#endregion

			return true;
		}
		#endregion


		#region Shoot
		[SerializeField] protected int delayShooting;
		[SerializeField] protected int MAX_MOVING_BULLETS = 1;
		private readonly List<Bullet> movingBullets = new List<Bullet>();
		private int shootTaskCount;


		public int canShootBullets => MAX_MOVING_BULLETS - movingBullets.Count - shootTaskCount;


		public async UniTask<Bullet> Shoot()
		{
			++shootTaskCount;
			if (await UniTask.Delay(delayShooting, cancellationToken: Token).SuppressCancellationThrow()
				|| movingBullets.Count == MAX_MOVING_BULLETS)
			{
				--shootTaskCount;
				return null;
			}

			--shootTaskCount;
			var data = new Bullet.Data
			{
				position = transform.position + direction.ToUnitVector3() * 0.5f,
				direction = direction,
				movingBullets = movingBullets
			};
			if (direction == Direction.Up || direction == Direction.Down) data.position.y = Mathf.Round(data.position.y * 2) * 0.5f;
			else data.position.x = Mathf.Round(data.position.x * 2) * 0.5f;
			ExportSpecificBulletData(ref data);
			return Bullet.Spawn(data);
		}


		protected abstract void ExportSpecificBulletData(ref Bullet.Data data);
		#endregion


		private CancellationTokenSource ΔcancelSource;
		public CancellationToken Token => ΔcancelSource.Token;
		protected void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			ΔcancelSource = CancellationTokenSource.CreateLinkedTokenSource(BattleField.Token);
			index = (transform.position * 2).ToVector2Int();
#if DEBUG
			if ((array[index.x][index.y] as List<Tank>).Contains(this))
				throw new Exception($"Không thể add Tank ! {transform.position} đang tồn tại this tank !");
#endif
			(array[index.x][index.y] as List<Tank>).Add(this);
			movingBullets.Clear();

			if ((Setting.enemy_CanCollise_Item || (this is PlayerTank))
				&& Item.current && Item.current.IsCollidedTank(transform.position))
			{
				Item.current.OnCollision(this);
			}
		}


		protected void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
			(array[index.x][index.y] as List<Tank>).Remove(this);
		}


		public abstract bool OnCollision(Bullet bullet);
		public abstract void Explode();


		[SerializeField] protected SpriteRenderer spriteRenderer;
		[SerializeField] protected Animator animator;
		public enum Color
		{
			Yellow = 0, Green = 1, White = 2, Red = 3
		}
		public abstract Color color { get; set; }
		public virtual Ship ship { get; set; }
	}



	public interface ITankCollision
	{
		void OnCollision(Tank tank);
	}



	public sealed class TankLifes<TKey> : IEnumerable<KeyValuePair<TKey, int>> where TKey : struct, Enum
	{
		private readonly Dictionary<TKey, int> dict = new Dictionary<TKey, int>();
		public event Action<TKey> valueChanged;


		public int this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => dict[key];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				dict[key] = value < 0 ? 0 : value > 100 ? 100 : value;
				valueChanged?.Invoke(key);
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() => dict.GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();

		public Dictionary<TKey, int>.KeyCollection Keys => dict.Keys;

		public Dictionary<TKey, int>.ValueCollection Values => dict.Values;


		public bool hasLife
		{
			get
			{
				foreach (var key in dict.Keys) if (dict[key] != 0) return true;
				return false;
			}
		}
	}
}