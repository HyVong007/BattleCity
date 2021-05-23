using BattleCity.Items;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
				  // Fix bug Addressables 1334114
				  var prefab = BattleField.instance.enemyTankAnchor.GetChild(0).GetComponent<EnemyTank>();
				  pool = new ObjectPool<EnemyTank>(prefab, BattleField.instance.enemyTankAnchor, BattleField.instance.enemyTankAnchor);
			  };
		}


		private static ObjectPool<EnemyTank> pool;
		public static async UniTask<EnemyTank> Spawn(Type type, Color color, Vector3 position)
		{
			// await Effect
			// Kiểm tra & chỉnh sửa lifes

			var tank = pool.Get(position, false);
			tank.type = type;
			tank.color = color;
			if (shipCount > 0)
			{
				--shipCount;
				nameof(Item.Name.Ship).Instantiate<Ship>().OnCollision(tank);
			}
			tank.gameObject.SetActive(true);
			return tank;
		}


		protected override void ExportSpecificBulletData(ref Bullet.Data data)
		{
			data.owner = Bullet.Owner.Enemy;

			// Test
			data.speed = BattleField.instance.bulletSpeed;
			data.canDestroySteel = data.canBurnForest = true;
		}


		private void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039

			lifes.valueChanged += type =>
			  {

			  };
		}


		private readonly struct Enumerable : IEnumerable<EnemyTank>
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<EnemyTank> GetEnumerator() => ΔcurrentEnemies.GetEnumerator();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator() => ΔcurrentEnemies.GetEnumerator();
		}
		private static readonly List<EnemyTank> ΔcurrentEnemies = new List<EnemyTank>();


		/// <summary>
		/// Không nên cache <see cref="currentEnemies"/> thay vào đó nên clone<br/>
		/// Bởi vì cache sẽ không hợp lệ nữa nếu tương lai có <see cref="EnemyTank"/> bị chết hoặc sinh thêm.
		/// </summary>
		public static IEnumerable<EnemyTank> currentEnemies => new Enumerable();
		private new void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039

			base.OnEnable();
			direction = Direction.Down;
			ΔcurrentEnemies.Add(this);
		}


		private new void OnDisable()
		{
			ΔcurrentEnemies.Remove(this);
			base.OnDisable();
		}


		public override void Explode()
		{
		}


		public override bool OnCollision(Bullet bullet)
		{

			// Test

			//pool.Recycle(this);

			if (bullet.owner == Bullet.Owner.Enemy) return false;
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
		/// Nếu <see cref="EnemyTank"/> đang sống thì sẽ kiểm tra thay đổi <see cref="shipCount"/>
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