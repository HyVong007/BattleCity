using BattleCity.AI;
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
			instance = instance ? throw new Exception() : this;
			ΔcancelSource = new CancellationTokenSource();
			isPause = false;
			awake?.Invoke();
		}


		private async void Start()
		{
			Platform.ImportCurrentStage();
			await UniTask.Yield();
			foreach (var color in humanPlayerColors)
				Gamepad.GetInstance(color).Add(gamepadListeners[color] = new GamepadListener(color));


			// spawn enemy
			EnemyTank.lifes[EnemyTank.Type.Big] = 1;
			EnemyAgent.SpawnTank().Forget();



			// spawn player
			PlayerTank.lifes[Tank.Color.Yellow] = 1; // Test
			PlayerTank.lifes[Tank.Color.Green] = 1; // Test

			if (aiPlayerColors.Length != 0) "Player Agent".Instantiate(transform);
			foreach (var color in humanPlayerColors) PlayerTank.Spawn(color, false).Forget();
			foreach (var color in aiPlayerColors) PlayerTank.Spawn(color, false).Forget();
		}


		private void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
			foreach (var color_listener in gamepadListeners) Gamepad.GetInstance(color_listener.Key).Remove(color_listener.Value);
		}


		[field: SerializeField] public Transform platformAnchor { get; private set; }
		[field: SerializeField] public Transform enemyTankAnchor { get; private set; }
		[field: SerializeField] public Transform bulletAnchor { get; private set; }



		// Test
		public Tank.Color[] humanPlayerColors, aiPlayerColors;







		#region Finish
		public bool isGameOver { get; private set; }


		[SerializeField] private int delayEndingMilisec;
		private bool isFinish;
		/// <summary>
		/// Trận chiến kết thúc trong các trường hợp:<para/> 
		/// - <see cref="Eagle"/> chết (GameOver)<br/> 
		/// - <see cref="PlayerTank"/> nổ hết và không thể sinh thêm (GameOver)<br/>
		/// - <see cref="EnemyTank"/> nổ hết và không thể sinh thêm (Victory)
		/// </summary>
		public async void Finish()
		{
			if (isGameOver) return;
			isGameOver = Eagle.instance.isDead || (!PlayerTank.lifes.hasLife && PlayerTank.livingTanks.Count == 0);
			if (isGameOver)
			{
				foreach (var player in PlayerTank.livingTanks)
					if (Gamepad.GetInstance(player.color).Contains(player)) Gamepad.GetInstance(player.color).Remove(player);
				if (Agent.GetInstance<PlayerAgent>()) Agent.GetInstance<PlayerAgent>().enabled = false;
			}

			if (isFinish) return;
			isFinish = true;
			await UniTask.Delay(delayEndingMilisec);

			Destroy(gameObject);
			foreach (var player in PlayerTank.livingTanks) player.gameObject.SetActive(false);
		}
		#endregion


		private readonly Dictionary<Tank.Color, GamepadListener> gamepadListeners = new Dictionary<Tank.Color, GamepadListener>();
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