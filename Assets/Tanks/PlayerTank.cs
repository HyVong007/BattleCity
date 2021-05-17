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
		public static async UniTask<PlayerTank> Spawn(Color color, Vector3 position, byte? star = null)
		{
			// await Effect
			// Kiểm tra & chỉnh sửa lifes

			PlayerTank tank = players.TryGetValue(color, out tank) ? tank
				: $"{color} Player Tank".Instantiate<PlayerTank>(position, Quaternion.identity);
			tank.star = star != null ? star.Value : tank.star;
			tank.gameObject.SetActive(true);
			return tank;
		}


		#region Shoot
		protected override void AddBulletSpecialInfo(ref Bullet.Data data)
		{
			throw new NotImplementedException();
		}


		#endregion


		private static readonly IReadOnlyDictionary<Color, PlayerTank> players = new Dictionary<Color, PlayerTank>();
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
			turretDirection = Direction.Up;
		}


		private new void OnDisable()
		{
			base.OnDisable();
		}


		public override bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}


		public override void Explode()
		{
			throw new NotImplementedException();
		}


		#region Gamepad Listener
		private void Update()
		{
			var k = Keyboard.current;
			var press = k.leftArrowKey.wasPressedThisFrame ? Direction.Left
				: k.rightArrowKey.wasPressedThisFrame ? Direction.Right
				: k.upArrowKey.wasPressedThisFrame ? Direction.Up
				: k.downArrowKey.wasPressedThisFrame ? Direction.Down
				: (Direction?)null;

			var hold = k.leftArrowKey.isPressed ? Direction.Left
				: k.rightArrowKey.isPressed ? Direction.Right
				: k.upArrowKey.isPressed ? Direction.Up
				: k.downArrowKey.isPressed ? Direction.Down
				: (Direction?)null;

			var release = k.leftArrowKey.wasReleasedThisFrame ? Direction.Left
				: k.rightArrowKey.wasReleasedThisFrame ? Direction.Right
				: k.upArrowKey.wasReleasedThisFrame ? Direction.Up
				: k.downArrowKey.wasReleasedThisFrame ? Direction.Down
				: (Direction?)null;

			if (press != null) OnDpad(press.Value, Gamepad.ButtonState.Press);
			else if (release != null) OnDpad(release.Value, Gamepad.ButtonState.Release);
			else if (hold != null) OnDpad(hold.Value, Gamepad.ButtonState.Hold);
		}


		public void OnDpad(Direction direction, Gamepad.ButtonState state)
		{
			if (state != Gamepad.ButtonState.Release) moveDirection = direction;
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
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Δcolor;
			set => throw new InvalidOperationException();
		}


		private Direction ΔturretDirection;
		public override Direction turretDirection
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ΔturretDirection;

			protected set
			{
				ΔturretDirection = value;
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