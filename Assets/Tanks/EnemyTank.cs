using BattleCity.Items;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
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


		protected override void AddBulletSpecialInfo(ref Bullet.Data data)
		{
			throw new NotImplementedException();
		}


		private void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039

			lifes.valueChanged += type =>
			  {

			  };
		}


		private new void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039

			base.OnEnable();
			turretDirection = Direction.Down;
		}


		private new void OnDisable()
		{
			base.OnDisable();
		}


		public override void Explode()
		{
			throw new NotImplementedException();
		}


		public override bool OnCollision(Bullet bullet)
		{

			// Test

			pool.Recycle(this);
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
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Δtype;

			private set
			{
				if (isActiveAndEnabled) throw new InvalidOperationException("Khi Enemy đang sống thì không thể set type !");
				Δtype = value;
				spriteRenderer.sprite = sprites[value][color][turretDirection];

				//var stat = Extensions.Load<GlobalAsset>().enemyStat;
				//moveSpeed = stat.moveSpeed[value];
				//shootingDelayMiliseconds = Mathf.RoundToInt(stat.delayShootSeconds[value] * 1000);
			}
		}


		private Color Δcolor;
		public override Color color
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Δcolor;

			set
			{
				Δcolor = value;
				spriteRenderer.sprite = weapon == Weapon.Gun ? resources.gunSprites[value][turretDirection] : sprites[type][value][turretDirection];
				if (ship) ship.ChangeColor(value);
			}
		}


		private Direction ΔturretDirection;
		public override Direction turretDirection
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ΔturretDirection;

			protected set
			{
				ΔturretDirection = value;
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
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
						spriteRenderer.sprite = value == Weapon.Gun ? resources.gunSprites[color][turretDirection] : sprites[type][color][turretDirection];
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
		/// Nếu Enemy đang sống thì sẽ kiểm tra thay đổi <see cref="shipCount"/>
		/// </summary>
		public override Ship ship
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Δship;

			set
			{
				shipCount += isActiveAndEnabled && (Δship ^ value) ? (value ? 1 : -1) : 0;
				if (Δship && (!value || value != Δship)) Destroy(Δship.gameObject);
				Δship = value;
			}
		}
	}
}