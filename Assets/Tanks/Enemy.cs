using BattleCity.Items;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BattleCity.Tanks
{
	public sealed class Enemy : Tank
	{
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
				spriteRenderer.sprite = sprites[isActiveAndEnabled
					? throw new InvalidOperationException() : Δtype = value]
					[color][direction];

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
				spriteRenderer.sprite = weapon == Weapon.Gun
					? asset.gunSprites[value][direction] : sprites[type][value][direction];
				if (ship) ship.ChangeColor(value);
			}
		}


		private Vector3 Δdirection = Vector3.down;
		public override Vector3 direction
		{
			get => Δdirection;

			set
			{
				Δdirection = value;
				spriteRenderer.sprite = weapon == Weapon.Gun
					? asset.gunSprites[color][value] : sprites[type][color][value];
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
				spriteRenderer.sprite =
					isActiveAndEnabled &&
					(Δweapon == Weapon.Star && value == Weapon.Normal)
					|| (Δweapon == Weapon.Gun && value != Weapon.Gun)
					? throw new InvalidOperationException()
					: value == Weapon.Gun ? asset.gunSprites[color][direction]
					: sprites[type][color][direction];

				Δweapon = value;
				//moveSpeed = value == Weapon.Gun
				//? Extensions.Load<GlobalAsset>().enemyStat.moveSpeed[Type.Armored]
				//: Extensions.Load<GlobalAsset>().enemyStat.moveSpeed[type];
			}
		}


		[SerializeField]
		private SerializableDictionaryBase<Type, SerializableDictionaryBase<Color,
			SerializableDictionaryBase<Vector3, Sprite>>> sprites;

		[SerializeField]
		private SerializableDictionaryBase<Type, SerializableDictionaryBase<Color,
			SerializableDictionaryBase<Vector3, RuntimeAnimatorController>>> anims;


		private static ObjectPool<Enemy> pool;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			BattleField.onAwake += () =>
				pool = new(Addressables.LoadAssetAsync<GameObject>("Assets/Tanks/Prefab/Enemy.prefab")
					.WaitForCompletion().GetComponent<Enemy>());
		}


		private static int shipCount;
		public static Enemy New()
		{
			if (BattleField.enemyLifes == 0) return null;

			// animation

			--BattleField.enemyLifes;
			var enemy = pool.Get(Main.level.enemyIndexes[UnityEngine.Random.Range
				(0, Main.level.enemyIndexes.Length)].ToVector3(), false);
			enemy.type = (Type)UnityEngine.Random.Range(0, 4);
			enemy.color = (Color)UnityEngine.Random.Range(0, 4);
			if (shipCount > 0)
			{
				--shipCount;
				Addressables.InstantiateAsync("Assets/Items/Prefab/Ship.prefab")
					.WaitForCompletion().GetComponent<Ship>().OnCollision(enemy);
			}
			enemy.gameObject.SetActive(true);
			return enemy;
		}


		private new void OnEnable()
		{
			base.OnEnable();
			weapon = Weapon.Normal;
		}


		protected override RuntimeAnimatorController anim
			=> weapon == Weapon.Gun ? asset.gunAnims[color][direction]
			: anims[type][color][direction];


		public override bool OnCollision(Bullet bullet)
		{
			//if (bullet.color == null) return false;
			Explode();
			return true;
		}


		public override async void Explode()
		{
			pool.Recycle(this);
			await UniTask.Delay(1000);

			New();
		}


		protected override void AddBulletData(ref Bullet.Data data)
		{
		}




		bool s;
		private void Update()
		{
			#region Input Move
			Vector3 newDir = default; ;
			if (Input.GetKey(KeyCode.W)) newDir = Vector3.up;
			else if (Input.GetKey(KeyCode.D)) newDir = Vector3.right;
			else if (Input.GetKey(KeyCode.S)) newDir = Vector3.down;
			else if (Input.GetKey(KeyCode.A)) newDir = Vector3.left;

			if (!isMoving && newDir != default)
			{
				if (direction != newDir) direction = newDir;
				else if (CanMove(newDir)) Move(newDir).ContinueWith(() =>
				{
					if (s)
					{
						s = false;
						Shoot();
					}
				});
			}
			#endregion

			if (Input.GetKey(KeyCode.LeftShift))
			{
				if (!isMoving) Shoot();
				else s = true;
			}
		}
	}
}