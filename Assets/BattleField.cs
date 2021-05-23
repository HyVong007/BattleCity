using BattleCity.Platforms;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System;
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

			var size = map.size;
#if DEBUG
			if (size.x < 5 || size.x > 253 || size.y < 3 || size.y > 253)
				throw new ArgumentOutOfRangeException("Kích thước map không hợp lệ !");
#endif
			"MAP".SetValue(map);
			"SETTING".SetValue(setting);
			awake();
		}


		private async void Start()
		{
			await UniTask.Yield();
			foreach (var color in setting.humanPlayerColors) PlayerTank.Spawn(color, new Vector3(1.5f, 1.5f));
		}


		private void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
		}




		// Test
		[SerializeField] private Map map;

		// Test
		[SerializeField] private Setting setting;

		// Test
		public float bulletSpeed;





		[field: SerializeField] public Transform platformAnchor { get; private set; }
		[field: SerializeField] public Transform enemyTankAnchor { get; private set; }
		[field: SerializeField] public Transform bulletAnchor { get; private set; }


		#region Finish
		public bool finish { get; private set; }
		private static readonly Tank.Color[] COLORS = Enum.GetValues(typeof(Tank.Color)) as Tank.Color[];
		[SerializeField] private int delayEnding;


		/// <summary>
		/// Trận chiến kết thúc trong các trường hợp:<para/> 
		/// - <see cref="Eagle"/> chết (GameOver)<br/> 
		/// - <see cref="Tanks.PlayerTank"/> nổ hết và không thể sinh thêm (GameOver)<br/>
		/// - <see cref="Tanks.EnemyTank"/> nổ hết và không thể sinh thêm (Victory)
		/// </summary>
		public async void Finish()
		{
			if (finish) return;
			finish = true;
			foreach (var color in COLORS)
			{
				var player = PlayerTank.GetInstance(color);
				if (Gamepad.GetInstance(color).Contains(player)) Gamepad.GetInstance(color).Remove(player);
			}
			await UniTask.Delay(delayEnding);

			gameObject.SetActive(false);
			if (Eagle.instance.isDead /* || PlayerTank chết hết mạng */)
			{
				// GameOver
			}
		}
		#endregion
	}
}