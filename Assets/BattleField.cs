using BattleCity.Platforms;
using BattleCity.Tanks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

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



			map = _map; // Test
			var size = map.size;
#if DEBUG
			if (size.x < 5 || size.x > 253 || size.y < 3 || size.y > 253)
				throw new ArgumentOutOfRangeException("Kích thước map không hợp lệ !");
#endif
			awake();
		}


		private void Start()
		{
			var a = Tank.array;
		}


		private void Update()
		{
			if (Keyboard.current.spaceKey.wasPressedThisFrame)
				foreach (var data in bulletDatas) Bullet.Spawn(data);
		}


		public Bullet.Data[] bulletDatas;




		private void OnDisable()
		{
			ΔcancelSource.Cancel();
			ΔcancelSource.Dispose();
			ΔcancelSource = null;
		}


		// Test
		[SerializeField] private Map _map;
		public static Map map { get; private set; }

		[field: SerializeField] public Transform platformAnchor { get; private set; }
		[field: SerializeField] public Transform enemyTankAnchor { get; private set; }
		[field: SerializeField] public Transform bulletAnchor { get; private set; }
	}
}