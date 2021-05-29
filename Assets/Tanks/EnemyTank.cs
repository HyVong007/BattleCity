using BattleCity.AI;
using BattleCity.Items;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BattleCity.Tanks
{
	public sealed class EnemyTank : Tank
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			BattleField.awake += () =>
			 {
				 var prefab = "Enemy Tank".Load<EnemyTank>();
				 prefab.gameObject.SetActive(false);
				 pool = new ObjectPool<EnemyTank>(prefab, BattleField.instance.enemyTankAnchor, BattleField.instance.enemyTankAnchor);
			 };
		}


		private static ObjectPool<EnemyTank> pool;
		public static async UniTask<EnemyTank> Spawn(Type type, Color color, Vector3 position)
		{
			// anim spawn
			if (await UniTask.Delay(500, cancellationToken: BattleField.Token).SuppressCancellationThrow() || lifes[type] == 0) return null;

			--lifes[type];
			var enemy = pool.Get(position, false);
			enemy.type = type;
			enemy.color = color;
			if (shipCount > 0)
			{
				--shipCount;
				nameof(Item.Name.Ship).Instantiate<Ship>().OnCollision(enemy);
			}
			enemy.gameObject.SetActive(true);
			return enemy;
		}


		protected override void ExportSpecificBulletData(ref Bullet.Data data)
		{
			data.owner = Bullet.Owner.Enemy;

			// Test
			data.speed = 0.05f;
			data.canDestroySteel = data.canBurnForest = true;
		}


		public struct Enumerable : IEnumerable<EnemyTank>
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<EnemyTank> GetEnumerator() => pool.GetEnumerator();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator() => pool.GetEnumerator();
		}


		/// <summary>
		/// Sử dụng ngay hoặc clone. Không nên cache.<br/>
		/// Nên clone trước khi sinh thêm hoặc gây nổ <see cref="EnemyTank"/> 
		/// </summary>
		public static IEnumerable<EnemyTank> livingTanks => new Enumerable();
		public static int livingTankCount => pool.usingCount;
		private new void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			base.OnEnable();
			direction = Direction.Down;
		}


		private new void OnDisable()
		{
			base.OnDisable();
			weapon = Weapon.Normal;
		}


		public override void Explode()
		{
			// anim big explosion
			// sound enemy explosion

			pool.Recycle(this);
			foreach (int life in lifes.Values)
				if (life != 0)
				{
					EnemyAgent.SpawnTank().Forget();
					return;
				}

			if (pool.usingCount == 0)
			{
				// remove sound enemy exist
				BattleField.instance.Finish();
			}
		}


		public override bool OnCollision(Bullet bullet)
		{
			if (bullet.owner == Bullet.Owner.Enemy) return false;
			var originalColor = color;

			// Cập nhật color, color HP
			// Nếu enemy còn sống thì anim small explosion, sound bullet collise alive tank
			// Nếu enemy bị nổ và không phải Gun -> tăng điểm
			// Nếu enemy nổ thì phá thuyền nếu có



			ship = null; // Test
			Explode();  // Test



			if (originalColor == Color.Red) Item.RandomSpawn();
			return true;
		}


		[Serializable] private sealed class Type_Color_Direction_Sprite : SerializableDictionaryBase<Type, Color_Direction_Sprite> { }
		[SerializeField] private Type_Color_Direction_Sprite sprites;
		[Serializable] private sealed class Type_Color_Direction_Anim : SerializableDictionaryBase<Type, Color_Direction_Anim> { }
		[SerializeField] private Type_Color_Direction_Anim anims;


		public enum Type
		{
			Small, Fast, Big, Armored
		}
		private Type Δtype;
		public Type type
		{
			get => Δtype;

			private set
			{
				if (isActiveAndEnabled) throw new InvalidOperationException("Khi Enemy đang sống thì không thể set type !");
				Δtype = value;
				spriteRenderer.sprite = sprites[value][color][direction];

				//var stat = Extensions.Load<GlobalAsset>().enemyStat;
				//moveSpeed = stat.moveSpeed[value];
				//shootingDelayMiliseconds = Mathf.RoundToInt(stat.delayShootSeconds[value] * 1000);
			}
		}


		private Color Δcolor;
		public override Color color
		{
			get => Δcolor;

			set
			{
				Δcolor = value;
				spriteRenderer.sprite = weapon == Weapon.Gun ? resources.gunSprites[value][direction] : sprites[type][value][direction];
				if (ship) ship.ChangeColor(value);
			}
		}


		private Direction Δdirection;
		public override Direction direction
		{
			get => Δdirection;

			protected set
			{
				Δdirection = value;
				spriteRenderer.sprite = weapon == Weapon.Gun ? resources.gunSprites[color][value] : sprites[type][color][value];
			}
		}


		public enum Weapon
		{
			Normal, Star, Gun
		}
		private Weapon Δweapon;
		public Weapon weapon
		{
			get => Δweapon;

			set
			{
				switch (Δweapon)
				{
					case Weapon.Star:
						if (isActiveAndEnabled && value == Weapon.Normal)
							throw new InvalidOperationException("Enemy đang có Weapon.Star không thể về Weapon.Normal khi đang sống !");
						goto case Weapon.Normal;

					case Weapon.Gun:
						if (isActiveAndEnabled && value != Weapon.Gun)
							throw new InvalidOperationException("Enemy đang có Weapon.Gun không thể thay đổi Weapon khác khi đang sống !");
						goto case Weapon.Normal;

					case Weapon.Normal:
						spriteRenderer.sprite = value == Weapon.Gun ? resources.gunSprites[color][direction] : sprites[type][color][direction];
						break;
				}

				Δweapon = value;
				//moveSpeed = value == Weapon.Gun ? Extensions.Load<GlobalAsset>().enemyStat.moveSpeed[Type.Armored] : Extensions.Load<GlobalAsset>().enemyStat.moveSpeed[type];
			}
		}


		public static readonly TankLifes<Type> lifes = new TankLifes<Type>
		{
			[Type.Small] = 0,
			[Type.Fast] = 0,
			[Type.Big] = 0,
			[Type.Armored] = 0
		};


		private static int shipCount;
		private Ship Δship;
		/// <summary>
		/// Nếu <see cref="EnemyTank"/> đang sống thì sẽ kiểm tra thay đổi <see cref="shipCount"/><br/>
		/// Gán = <see langword="null"/> sẽ phá hủy thuyền (nếu có)
		/// </summary>
		public override Ship ship
		{
			get => Δship;

			set
			{
				shipCount += enabled && (Δship ^ value) ? (value ? 1 : -1) : 0;
				if (Δship && (!value || value != Δship)) Destroy(Δship.gameObject);
				Δship = value;
			}
		}
	}
}