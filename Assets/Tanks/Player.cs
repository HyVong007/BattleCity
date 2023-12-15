using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;


namespace BattleCity.Tanks
{
	public sealed class Player : Tank
	{
		[SerializeField]
		private SerializableDictionaryBase<int, SerializableDictionaryBase<Color,
			SerializableDictionaryBase<Vector3, Sprite>>> sprites;

		[SerializeField]
		private SerializableDictionaryBase<int, SerializableDictionaryBase<Color,
			SerializableDictionaryBase<Vector3, RuntimeAnimatorController>>> anims;


		// TEST
		[SerializeField] private Color Δcolor;
		public override Color color
		{
			get => Δcolor;

			protected set
			{
				Δcolor = value;
				spriteRenderer.sprite = star == 3 ? asset.gunSprites[color][direction]
					: sprites[star][color][direction];
			}
		}


		private Vector3 Δdirection;
		public override Vector3 direction
		{
			get => Δdirection;

			protected set
			{
				Δdirection = isMoving ? throw new Exception() : value;
				spriteRenderer.sprite = star == 3 ? asset.gunSprites[color][direction]
					: sprites[star][color][direction];
			}
		}


		private int Δstar;
		public int star
		{
			get => Δstar;

			set
			{
				Δstar = value;
				spriteRenderer.sprite = star == 3 ? asset.gunSprites[color][direction]
					: sprites[star][color][direction];
			}
		}



		public override bool OnCollision(Bullet bullet)
		{
			Explode();
			return true;
		}


		private new async void OnEnable()
		{
			base.OnEnable();

			// TEST
			color = Δcolor;

			// Green auto move
			if (color != Color.Green) return;

			Hehe();
		}


		public int count;
		async void Hehe()
		{
			while (true)
			{
				for (int i = 0; i < count; ++i)
					if (!isMoving && CanMove(Vector3.right)) await Move(Vector3.right);

				for (int i = 0; i < count; ++i)
					if (!isMoving && CanMove(Vector3.left)) await Move(Vector3.left);

				await UniTask.Yield();
			}
		}


		private void Update()
		{
			if (color != Color.Yellow) return;

			#region Input Move
			Vector3 newDir = default; ;
			if (Input.GetKey(KeyCode.UpArrow)) newDir = Vector3.up;
			else if (Input.GetKey(KeyCode.RightArrow)) newDir = Vector3.right;
			else if (Input.GetKey(KeyCode.DownArrow)) newDir = Vector3.down;
			else if (Input.GetKey(KeyCode.LeftArrow)) newDir = Vector3.left;

			if (!isMoving && newDir != default && CanMove(direction = newDir))
				Move(newDir);
			#endregion


			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (color == Color.Yellow)
					color = Color.Green;
				else color = Color.Yellow;
			}

			if (Input.GetKeyDown(KeyCode.LeftAlt))
			{
				if (star < 3) ++star;
				else star = 0;
			}
		}


		public override void Explode()
		{
			Destroy(gameObject);
		}
	}
}