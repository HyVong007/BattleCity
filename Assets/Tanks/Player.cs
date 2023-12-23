using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BattleCity.Tanks
{
	public sealed class Player : Tank
	{
		public static readonly IReadOnlyDictionary<Color, Player> players = new Dictionary<Color, Player>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			var dict = players as Dictionary<Color, Player>;
			var anchor = new GameObject().transform;
			anchor.name = "Players";
			DontDestroyOnLoad(anchor);
			foreach (var color in new Color[] { Color.Green, Color.Yellow })
			{
				var player = dict[color] = Instantiate(Addressables.LoadAssetAsync<GameObject>("Assets/Tanks/Prefab/Player.prefab")
					.WaitForCompletion().GetComponent<Player>(), anchor);
				player.name = color.ToString();
				player.color = color;
			}
		}


		public static Player New(Color color, bool borrowLife = false)
		{
			if (BattleField.count == 1) goto CHECK_LIFE;
			if (players[color].isExploded) goto CHECK_LIFE;
			goto SPAWN_PLAYER;

		CHECK_LIFE:
			if (BattleField.playerLifes[color] != 0)
			{
				--BattleField.playerLifes[color];
				goto SPAWN_PLAYER;
			}
			if (!borrowLife) return null;

			// Kiểm tra đồng đội còn mạng không để mượn mạng
			var allyColor = color == Color.Yellow ? Color.Green : Color.Yellow;
			if (BattleField.playerLifes[allyColor] != 0)
			{
				--BattleField.playerLifes[allyColor];
				goto SPAWN_PLAYER;
			}
			return null;

		SPAWN_PLAYER:
			// Animation

			var player = players[color];
			if (player.isExploded) player.star = 0;
			player.transform.position = Main.level.playerIndexes[color].ToVector3();
			player.gameObject.SetActive(true);
			return player;
		}


		[SerializeField]
		private SerializableDictionaryBase<int, SerializableDictionaryBase<Color,
			SerializableDictionaryBase<Vector3, Sprite>>> sprites;

		[SerializeField]
		private SerializableDictionaryBase<int, SerializableDictionaryBase<Color,
			SerializableDictionaryBase<Vector3, RuntimeAnimatorController>>> anims;

		protected override RuntimeAnimatorController anim
			=> star == 3 ? asset.gunAnims[color][direction]
			: anims[star][color][direction];


		private Color Δcolor;
		public override Color color
		{
			get => Δcolor;

			set
			{
				Δcolor = value;
				spriteRenderer.sprite = star == 3 ? asset.gunSprites[color][direction]
					: sprites[star][color][direction];
			}
		}


		private Vector3 Δdirection = Vector3.up;
		public override Vector3 direction
		{
			get => Δdirection;

			set
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


		private new void OnEnable()
		{
			base.OnEnable();
			isExploded = false;
		}


		public bool isExploded { get; private set; }
		public override async void Explode()
		{
			gameObject.SetActive(false);
			await UniTask.Delay(1000);

			New(color);
		}


		protected override void AddBulletData(ref Bullet.Data data)
		{
		}



		bool s;
		private void Update()
		{
			if (color != Color.Yellow) return;

			#region Input Move
			Vector3 newDir = default; ;
			if (Input.GetKey(KeyCode.UpArrow)) newDir = Vector3.up;
			else if (Input.GetKey(KeyCode.RightArrow)) newDir = Vector3.right;
			else if (Input.GetKey(KeyCode.DownArrow)) newDir = Vector3.down;
			else if (Input.GetKey(KeyCode.LeftArrow)) newDir = Vector3.left;

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

			if (Input.GetKey(KeyCode.RightAlt))
			{
				if (!isMoving) Shoot();
				else s = true;
			}
		}

	}
}