using BattleCity.Items;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BattleCity.Tanks
{
	public sealed class Enemy : Tank
	{
		/// <summary>
		/// Cẩn thận khi dùng foreach vì Enemy nổ sẽ bị loại khỏi list
		/// </summary>
		public static readonly IReadOnlyList<Enemy> enemies = new List<Enemy>();

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
				spriteRenderer.sprite = (Δweapon = value) == Weapon.Gun
					? asset.gunSprites[color][direction]
					: sprites[type][color][direction];

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
		public static async UniTask<Enemy> New()
		{
			var token = BattleField.Token;
			await UniTask.Delay(1000); // Animation
			if (token.IsCancellationRequested) return null;

			if (BattleField.enemyLifes == 0) return null;
			--BattleField.enemyLifes;
			var enemy = pool.Get(Main.level.enemyIndexes[UnityEngine.Random.Range
				(0, Main.level.enemyIndexes.Length)].ToVector3(), false);
			enemy.type = (Type)UnityEngine.Random.Range(0, 4);
			enemy.color = (Color)UnityEngine.Random.Range(0, 4);
			if (shipCount > 0)
			{
				--shipCount;
				Item.New<Ship>().OnCollision(enemy);
			}
			enemy.gameObject.SetActive(true);
			return enemy;
		}


		private new void OnEnable()
		{
			base.OnEnable();
			(enemies as List<Enemy>).Add(this);
			health = 2; // Test
			weapon = Weapon.Normal;
		}


		private new void OnDisable()
		{
			base.OnDisable();
			(enemies as List<Enemy>).Remove(this);
		}


		protected override RuntimeAnimatorController anim
			=> weapon == Weapon.Gun ? asset.gunAnims[color][direction]
			: anims[type][color][direction];


		public int health;
		public override bool OnCollision(Bullet bullet)
		{
			if (bullet.color == null) return false;

			if (color == Color.Red) Item.New();
			if (--health == 0) Explode();

			return true;
		}



		public override async void Explode()
		{
			pool.Recycle(this);
			await UniTask.Delay(1000);

			if (BattleField.enemyLifes != 0) New().Forget();
			else if (enemies.Count == 0) BattleField.End();
		}


		protected override void AddBulletData(ref Bullet.Data data)
			=> data.canDestroySteel = weapon != Weapon.Normal;
	}
}