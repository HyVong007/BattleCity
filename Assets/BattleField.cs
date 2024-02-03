using BattleCity.Items;
using BattleCity.Platforms;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace BattleCity
{
	public sealed class BattleField : MonoBehaviour
	{
		private static BattleField instance;
		public static event Action onAwake;

		/// <summary>
		/// Số thứ tự của trận chiến<br/>
		/// </summary>
		public static int count { get; private set; }

		public static readonly Dictionary<Color, byte> playerLifes = new();

		public static byte enemyLifes;


		private void Awake()
		{
			instance = instance ? throw new Exception() : this;
			++count;
			cts.Dispose();
			cts = new();
			Platform.LoadLevel(Main.level);
			Camera.main.transform.position = new Vector3(Main.level.width / 2f, Main.level.height / 2f, -10f);
			Camera.main.orthographicSize = Main.level.height / 2f;
			// Đăng ký nút bấm gamepad: người chơi nhấn nút mượn mạng khi đã chết
			onAwake();
		}


		private static bool twoPlayers;
		private async void Start()
		{
			await UniTask.Yield();

			// Sinh Enemy
			enemyLifes = 255;
			//for (int i = Main.mouseIndex == Main.ONE_PLAYER ? 3 : 6; i > 0; --i) Enemy.New().Forget();

			// Sinh Player
			if (count == 1)
			{
				playerLifes[Color.Yellow] = playerLifes[Color.Green] = 255;
				twoPlayers = Main.mouseIndex != Main.ONE_PLAYER;
			}
			Player.New(Color.Yellow).Forget();
			//if (twoPlayers) Player.New(Color.Green).Forget();
		}


		private static CancellationTokenSource cts = new();
		public static CancellationToken Token => cts.Token;

		public static async void End()
		{
			if (cts.IsCancellationRequested) return;
			await UniTask.Delay(3000);
			if (cts.IsCancellationRequested) return;

			cts.Cancel();
			if (Eagle.hasDead) count = 0;
			else
			{
				bool playerLive = false;
				foreach (var color in new Color[] { Color.Yellow, Color.Green })
					if (playerLifes[color] != 0 || Player.players[color].gameObject.activeSelf)
					{
						playerLive = true;
						break;
					}

				count = playerLive ? count : 0;
			}

			foreach (var color in new Color[] { Color.Yellow, Color.Green })
				Player.players[color].gameObject.SetActive(false);

			SceneManager.LoadScene("Score Board");
		}


		// Cài count = 0, cts.Cancel() và load scene Main
		[SerializeField] private Button buttonX;
	}
}