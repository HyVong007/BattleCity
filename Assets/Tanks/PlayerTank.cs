using BattleCity.AI;
using BattleCity.Items;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BattleCity.Tanks
{
	public sealed class PlayerTank : Tank, IGamepadListener
	{
		#region Spawn
		private static readonly IReadOnlyDictionary<Color, PlayerTank> players = new Dictionary<Color, PlayerTank>();
		private static readonly IReadOnlyDictionary<Color, Vector3> spawnPositions = new Dictionary<Color, Vector3>();
		public static async UniTask<PlayerTank> Spawn(Color color, bool canBorrowLife, int? star = null)
		{
			// anim spawn
			if (await UniTask.Delay(1000, cancellationToken: BattleField.Token).SuppressCancellationThrow()) return null;

			if (!players.ContainsKey(color)) goto CHECK;
			var player = players[color];
			if (player.enabled) return null;

			if (!player.isExploded) return Spawn();

			CHECK:
			if (lifes[color] != 0)
			{
				--lifes[color];
				return Spawn();
			}

			if (!canBorrowLife) return null;

			Color ally = default;
			int count = 0;
			foreach (var c in PLAYER_COLORS) count = lifes[c] > count ? lifes[ally = c] : count;
			if (count == 0) return null;

			--lifes[ally];
			return Spawn();


			PlayerTank Spawn()
			{
				PlayerTank player;
				if (players.ContainsKey(color)) (player = players[color]).transform.position = spawnPositions[color];
				else (players as Dictionary<Color, PlayerTank>)[color] = player = $"{color} Player Tank".Instantiate<PlayerTank>(spawnPositions[color], Quaternion.identity);
				player.star = star != null ? star.Value : player.star;
				player.gameObject.SetActive(true);
				return player;
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PlayerTank GetLivingTank(Color color) => players.ContainsKey(color) && players[color].enabled ? players[color] : null;


		private static readonly List<PlayerTank> tmp = new List<PlayerTank>();
		/// <summary>
		/// Sử dụng ngay hoặc clone. Không nên cache.<br/>
		/// </summary>
		public static IReadOnlyList<PlayerTank> livingTanks
		{
			get
			{
				tmp.Clear();
				foreach (var player in players.Values) if (player.enabled) tmp.Add(player);
				return tmp;
			}
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			BattleField.awake += () =>
			  {
				  // khởi tạo {spawnPositions}

				  // Test
				  (spawnPositions as Dictionary<Color, Vector3>)[Color.Yellow] = new Vector3(1.5f, 1.5f);
				  (spawnPositions as Dictionary<Color, Vector3>)[Color.Green] = new Vector3(6.5f, 6.5f);
			  };
		}
		#endregion


		private bool canBurnForest;
		protected override void ExportSpecificBulletData(ref Bullet.Data data)
		{
			data.owner = (Bullet.Owner)(int)color;
			data.speed = star == 0 ? 3 * moveSpeed : 6 * moveSpeed;
			data.speed = data.speed <= 0.125f ? data.speed : 0.125f;
			data.canDestroySteel = star == 3;
			data.canBurnForest = canBurnForest;
		}


		private void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			DontDestroyOnLoad(players.ContainsKey(color) ? throw new InvalidOperationException($"Không thể sinh PlayerTank vì PlayerTank.players đang chứa {color} player")
				: (players as Dictionary<Color, PlayerTank>)[color] = this);
		}


		private new void OnEnable()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			base.OnEnable();
			isExploded = false;
			direction = Direction.Up;
			if (!BattleField.instance.isGameOver && !PlayerAgent.colors.Contains(color)) Gamepad.GetInstance(color).Add(this);
		}


		private new void OnDisable()
		{
			base.OnDisable();
			if (Gamepad.GetInstance(color).Contains(this)) Gamepad.GetInstance(color).Remove(this);
			if (FreezeTask.dict[color].Count != 0)
			{
				// Dọn dẹp tất cả freezeTask của this
				isFreeze = false;
				FreezeTask.dict[color].Clear();
				animator.runtimeAnimatorController = null;
				spriteRenderer.enabled = true;
			}
		}


		public override bool OnCollision(Bullet bullet)
		{
			if ((int)bullet.owner == (int)color) return false;
			if (helmet) return bullet.owner == Bullet.Owner.Enemy || Setting.playerBullet_CanCollise_Player;
			if (bullet.owner != Bullet.Owner.Enemy)
				if (Setting.playerBullet_CanCollise_Player)
				{
					// Anim small explosion
					Freeze(Setting.playerBullet_FreezePlayer_Milisec);
					return true;
				}
				else return false;

			if (ship)
			{
				Destroy(ship.gameObject);
				ship = null;
				return true;
			}

			//fieryStar = 0;
			--star; // có thể gây nổ nếu star xuống quá thấp
			if (enabled)
			{
				// anim small explosion
				// sound bullet collise alive tank
				// Nếu là human thì rung tay cầm
			}

			return true;
		}


		private bool isExploded;
		public override void Explode()
		{
			// anim big explosion
			// sound player explosion
			// Nếu là human thì rung tay cầm

			isExploded = true;
			gameObject.SetActive(false);
			star = Setting.defaultPlayerStar;
			if (lifes[color] != 0)
			{
				Spawn(color, false).Forget();
				return;
			}

			foreach (int life in lifes.Values)
				if (life != 0)
				{
					if (PlayerAgent.colors.Contains(color)) PlayerAgent.DecideBorrowingLife(color);
					return;
				}

			foreach (var player in players.Values) if (player.enabled) return;
			BattleField.instance.Finish();
		}


		#region Freeze
		/// <summary>
		/// Đang bị đóng băng (không thể di chuyển nhưng có thể bắn) ?
		/// </summary>
		public bool isFreeze { get; private set; }


		[SerializeField] private RuntimeAnimatorController flashAnim;
		/// <summary>
		/// Đóng băng (không thể di chuyển nhưng có thể bắn)
		/// </summary>
		public void Freeze(int delayMilisec)
		{
			if (FreezeTask.dict[color].TryGetValue(delayMilisec, out FreezeTask task)) task.UpdateStopTime();
			else new FreezeTask(this, delayMilisec);
		}



		/// <summary>
		/// Thuật toán đóng băng: <para/>
		/// Mỗi delay tương ứng 1 loại task<br/>
		/// Mỗi player mỗi khi <see cref="PlayerTank.Freeze(int)"/> được gọi thì sẽ tạo mới task nếu chưa có <br/>
		/// => cập nhật thời gian cho task => chạy task nếu chưa chạy => khi task kết thúc thì chỉ đánh thức player khi không còn task nào khác đang chạy.
		/// </summary>
		private sealed class FreezeTask
		{
			public static readonly IReadOnlyDictionary<Color, Dictionary<int, FreezeTask>> dict = new Dictionary<Color, Dictionary<int, FreezeTask>>
			{
				[Color.Yellow] = new Dictionary<int, FreezeTask>(),
				[Color.Green] = new Dictionary<int, FreezeTask>(),
				[Color.White] = new Dictionary<int, FreezeTask>()
			};
			private readonly PlayerTank player;
			private readonly int delayMilisec;
			public FreezeTask(PlayerTank player, int delayMilisec)
			{
				this.player = player;
				this.delayMilisec = delayMilisec;
				dict[player.color][delayMilisec] = this;
				UpdateStopTime();
				Freeze();
			}


			private float stopTime;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void UpdateStopTime() => stopTime = Time.time + delayMilisec * 0.001f;


			private async void Freeze()
			{
				player.isFreeze = true;
				player.animator.runtimeAnimatorController = player.flashAnim;
				var token = player.Token;
				await UniTask.WaitUntil(() => token.IsCancellationRequested || Time.time > stopTime);
				if (token.IsCancellationRequested) return;

				var d = dict[player.color];
				d.Remove(delayMilisec);
				if (d.Count != 0) return;

				player.isFreeze = false;
				player.animator.runtimeAnimatorController = null;
				player.spriteRenderer.enabled = true;
			}
		}
		#endregion


		#region Gamepad Listener
		private UniTask moveTask;
		public void OnDpad(Direction direction, Gamepad.ButtonState state)
		{
			if (isFreeze || moveTask.isRunning()) return;
			if (this.direction != direction)
			{
				this.direction = direction;
				return;
			}

			if (canMove) (moveTask = Move(direction, 1)).Forget();
		}


		public void OnButtonA(Gamepad.ButtonState state)
		{
			for (int i = canShootBullets; i > 0; --i) Shoot().Forget();
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


		public Helmet helmet { get; set; }


		private int Δstar;
		public int star
		{
			get => Δstar;

			set
			{
				// Test
				Δstar = value < 0 ? 0 : value > 3 ? 3 : value;
			}
		}


		public static readonly TankLifes<Color> lifes = new TankLifes<Color>
		{
			[Color.Yellow] = 0,
			[Color.Green] = 0
		};


		public static readonly ReadOnlyArray<Color> PLAYER_COLORS = new ReadOnlyArray<Color>(new Color[]
			{
				Color.Yellow, Color.Green, Color.White
			});
	}
}