using BattleCity.Items;
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
			resources = "Tank Resources".Load<TankResources>();
			BattleField.awake += () =>
			  {
				  var size = BattleField.map.size;
				  size.x += 2; size.y += 2;
				  var _array = new List<Tank>[size.x][];
				  var a = new ReadOnlyArray<IReadOnlyList<Tank>>[size.x];
				  for (int x = 0; x < size.x; ++x)
				  {
					  a[x] = new ReadOnlyArray<IReadOnlyList<Tank>>(_array[x] = new List<Tank>[size.y]);
					  for (int y = 0; y < size.y; ++y) _array[x][y] = new List<Tank>();
				  }
				  array = new ReadOnlyArray<ReadOnlyArray<IReadOnlyList<Tank>>>(a);
			  };
		}


		#region Move
		public abstract Direction turretDirection { get; protected set; }

		private Direction? currentMoveDirection;
		public Direction? moveDirection
		{
			get => currentMoveDirection;

			set
			{
				if (value != turretDirection)
				{
					turretDirection = value != null ? value.Value : turretDirection;
					return;
				}

				currentMoveDirection = value;
				if (!moveTask.isRunning()) moveTask = Move();
			}
		}


		[SerializeField] protected float moveSpeed;
		[SerializeField] protected int delayMoving;
		private UniTask moveTask;
		private async UniTask Move()
		{
			var token = Token;
			var d = currentMoveDirection;
			var v = d.ToUnitVector3() * moveSpeed;

			while (true)
			{
				currentMoveDirection = null;
				await transform.Move(d.Value, moveSpeed, delayMoving, token);
				if (token.IsCancellationRequested || currentMoveDirection == null) break;
				v = currentMoveDirection != d ? (d = currentMoveDirection).ToUnitVector3() * moveSpeed : v;
			}
		}
		#endregion


		#region Shoot
		protected int delayShooting;
		protected byte MAX_MOVING_BULLETS = 1;
		private readonly List<Bullet> movingBullets = new List<Bullet>();
		private int shootTaskCount;

		public int canShootBullets => MAX_MOVING_BULLETS - movingBullets.Count - shootTaskCount;


		public async UniTask<Bullet> Shoot()
		{
			++shootTaskCount;
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token, BattleField.Token);
			if (await UniTask.Delay(delayShooting, cancellationToken: cts.Token).SuppressCancellationThrow()
				|| movingBullets.Count == MAX_MOVING_BULLETS)
			{
				--shootTaskCount;
				return null;
			}

			var data = new Bullet.Data
			{
				// ...............
			};

			AddBulletSpecialInfo(ref data);
			--shootTaskCount;
			return Bullet.Spawn(data);
		}


		protected abstract void AddBulletSpecialInfo(ref Bullet.Data data);
		#endregion


		private CancellationTokenSource ΔcancelSource;
		public CancellationToken Token => ΔcancelSource.Token;
		protected void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			ΔcancelSource = CancellationTokenSource.CreateLinkedTokenSource(BattleField.Token);
			var index = transform.position.ToVector2Int();
#if DEBUG
			if ((array[index.x][index.y] as List<Tank>).Contains(this))
				throw new Exception($"Không thể add Tank ! {transform.position} đang tồn tại this tank !");
#endif
			(array[index.x][index.y] as List<Tank>).Add(this);
			movingBullets.Clear();

			// kiểm tra va chạm item
		}


		protected void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
			var index = transform.position.ToVector2Int();
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
	}
}