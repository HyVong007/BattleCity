using BattleCity.AI;
using BattleCity.Items;
using BattleCity.Platforms;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace BattleCity
{
	[DefaultExecutionOrder(-1)] // Test
	public sealed class BattleField : MonoBehaviour
	{
		public static event Action awake;
		private static CancellationTokenSource ΔcancelSource;
		public static CancellationToken Token => ΔcancelSource.Token;
		public static BattleField instance { get; private set; }
		private void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			instance = instance ? throw new Exception() : this;
			ΔcancelSource = new CancellationTokenSource();
			isPause = false;

			stage = _stage;
			var size = stage.size;
#if DEBUG
			if (size.x < 5 || size.x > 253 || size.y < 3 || size.y > 253)
				throw new ArgumentOutOfRangeException("Kích thước map không hợp lệ !");
#endif



			awake?.Invoke();
		}


		private readonly Dictionary<Tank.Color, GamepadListener> gamepadListeners = new Dictionary<Tank.Color, GamepadListener>();
		private async void Start()
		{
			await UniTask.Yield();
			foreach (var color in humanPlayerColors)
				Gamepad.GetInstance(color).Add(gamepadListeners[color] = new GamepadListener(color));


			// spawn enemy
			foreach (EnemyTank.Type type in Enum.GetValues(typeof(EnemyTank.Type))) // Test
				EnemyTank.lifes[type] = 1;

			EnemyAgent.SpawnTank().Forget();



			// spawn player
			foreach (var color in PlayerTank.PLAYER_COLORS) PlayerTank.lifes[color] = 1; // Test
			if (aiPlayerColors.Length != 0) "Player Agent".Instantiate(transform);
			foreach (var color in humanPlayerColors) PlayerTank.Spawn(color, false).Forget();
			foreach (var color in aiPlayerColors) PlayerTank.Spawn(color, false).Forget();
		}


		private void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
		}


		[field: SerializeField] public Transform platformAnchor { get; private set; }
		[field: SerializeField] public Transform enemyTankAnchor { get; private set; }
		[field: SerializeField] public Transform bulletAnchor { get; private set; }

		public static Stage stage { get; private set; }

		// Test
		[SerializeField] private Stage _stage;


		// Test
		public Tank.Color[] humanPlayerColors, aiPlayerColors;







		#region Finish
		public bool finish { get; private set; }
		private static readonly Tank.Color[] COLORS = Enum.GetValues(typeof(Tank.Color)) as Tank.Color[];
		[SerializeField] private int delayEndingMilisec;


		/// <summary>
		/// Trận chiến kết thúc trong các trường hợp:<para/> 
		/// - <see cref="Eagle"/> chết (GameOver)<br/> 
		/// - <see cref="PlayerTank"/> nổ hết và không thể sinh thêm (GameOver)<br/>
		/// - <see cref="EnemyTank"/> nổ hết và không thể sinh thêm (Victory)
		/// </summary>
		public async void Finish()
		{
			if (finish) return;
			finish = true;




			//foreach (var color_listener in gamepadListeners)
			//	Gamepad.GetInstance(color_listener.Key).Remove(color_listener.Value);
			//foreach (var color in COLORS)
			//{
			//	var player = PlayerTank.GetLivingTank(color);
			//	if (Gamepad.GetInstance(color).Contains(player)) Gamepad.GetInstance(color).Remove(player);
			//}
			//var playerAgent = Agent.GetInstance<PlayerAgent>();
			//if (playerAgent) playerAgent.enabled = false;
			//await UniTask.Delay(delayEndingMilisec);

			//gameObject.SetActive(false);
			//Agent.GetInstance<EnemyAgent>().enabled = false;

			//if (Eagle.instance.isDead /* || PlayerTank chết hết mạng */)
			//{
			//	// GameOver
			//}
		}
		#endregion


		public static bool isPause { get; private set; }
		private sealed class GamepadListener : IGamepadListener
		{
			private readonly Tank.Color gamepadColor;
			public GamepadListener(Tank.Color color) => gamepadColor = color;


			public void OnButtonA(Gamepad.ButtonState state)
			{
			}


			public void OnButtonB(Gamepad.ButtonState state)
			{
			}


			public void OnButtonSelect()
			{
			}


			public void OnButtonStart()
			{
				// Đang còn BUG


				// Chỉ được Pause trong Offline Mode

				if (gamepadColor != Tank.Color.Yellow) return;
				isPause = !isPause;
				foreach (var c in instance.humanPlayerColors)
				{
					var g = Gamepad.GetInstance(c);
					var player = PlayerTank.GetLivingTank(c);
					if (!isPause) g.Add(player);
					else if (g.Contains(player)) g.Remove(player);
				}
				Agent.GetInstance<EnemyAgent>().enabled = !isPause;
				var agent = Agent.GetInstance<PlayerAgent>();
				if (agent) agent.enabled = !isPause;
			}


			public void OnButtonX(Gamepad.ButtonState state)
			{
			}


			public void OnButtonY(Gamepad.ButtonState state)
			{
			}


			public void OnDpad(Direction direction, Gamepad.ButtonState state)
			{
			}
		}
	}
}