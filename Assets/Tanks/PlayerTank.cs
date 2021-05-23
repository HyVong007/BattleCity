using BattleCity.Items;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;


namespace BattleCity.Tanks
{
	public sealed class PlayerTank : Tank, IGamepadListener
	{
		#region Spawn
		private static readonly IReadOnlyDictionary<Color, PlayerTank> players = new Dictionary<Color, PlayerTank>();
		public static async UniTask<PlayerTank> Spawn(Color color, Vector3 position, int? star = null)
		{
			// await Effect
			// Kiểm tra & chỉnh sửa lifes

			PlayerTank tank = players.TryGetValue(color, out tank) ? tank
				: $"{color} Player Tank".Instantiate<PlayerTank>(position, Quaternion.identity);

			tank.star = star != null ? star.Value : tank.star;
			tank.gameObject.SetActive(true);
			return tank;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PlayerTank GetInstance(Color color) => players.ContainsKey(color) ? players[color] : null;
		#endregion


		protected override void ExportSpecificBulletData(ref Bullet.Data data)
		{
			data.owner = (Bullet.Owner)(int)color;

			// Test
			data.speed = BattleField.instance.bulletSpeed;
			data.canDestroySteel = data.canBurnForest = true;
		}


		private void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
#if DEBUG
			if (players.ContainsKey(color))
				throw new InvalidOperationException($"Không thể sinh PlayerTank vì PlayerTank.players đang chứa {color} player");
#endif
			DontDestroyOnLoad((players as Dictionary<Color, PlayerTank>)[color] = this);

			lifes.valueChanged += color =>
			  {

			  };
		}


		private new void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			base.OnEnable();
			direction = Direction.Up;
			if (!BattleField.instance.finish && "SETTING".GetValue<Setting>().humanPlayerColors.ContainsValue(color))
				Gamepad.GetInstance(color).Add(this);
		}


		private new void OnDisable()
		{
			if (Gamepad.GetInstance(color).Contains(this)) Gamepad.GetInstance(color).Remove(this);
			base.OnDisable();
		}


		public override bool OnCollision(Bullet bullet)
		{
			if ((int)bullet.owner == (int)color) return false;
			return true;
		}


		public override void Explode()
		{

		}


		#region Gamepad Listener
		private UniTask moveTask;
		public void OnDpad(Direction direction, Gamepad.ButtonState state)
		{
			if (moveTask.isRunning()) return;
			if (this.direction != direction)
			{
				this.direction = direction;
				return;
			}

			if (canMove) (moveTask = Move(direction, 1)).Forget();
		}


		public void OnButtonA(Gamepad.ButtonState state)
		{
		}


		public void OnButtonB(Gamepad.ButtonState state)
		{
		}


		public void OnButtonX(Gamepad.ButtonState state)
		{
		}


		public void OnButtonY(Gamepad.ButtonState state)
		{
		}


		public void OnButtonStart()
		{
		}


		public void OnButtonSelect()
		{
		}
		#endregion


		[Serializable] private sealed class Star_Color_Direction_Sprite : SerializableDictionaryBase<int, Color_Direction_Sprite> { }
		[SerializeField] private Star_Color_Direction_Sprite sprites;
		[Serializable] private sealed class Star_Color_Direction_Anim : SerializableDictionaryBase<int, Color_Direction_Anim> { }
		[SerializeField] private Star_Color_Direction_Anim anims;


		[Label("Color")] [SerializeField] private Color Δcolor;
		public override Color color
		{
			get => Δcolor;
			set => throw new InvalidOperationException();
		}


		private Direction Δdirection;
		public override Direction direction
		{
			get => Δdirection;

			protected set
			{
				Δdirection = value;
				spriteRenderer.sprite = star == 3 ? resources.gunSprites[color][value] : sprites[star][color][value];
			}
		}


		public Helmet helmet { get; private set; }


		public int star { get; set; }


		public static readonly TankLifes<Color> lifes = new TankLifes<Color>
		{
			[Color.Yellow] = 0,
			[Color.Green] = 0
		};
	}
}