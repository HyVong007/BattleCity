using BattleCity.Tanks;
using System;
using UnityEngine;


namespace BattleCity.Platforms
{
	[DisallowMultipleComponent]
	public abstract class Platform : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Platform>> array { get; private set; }
		private static Platform[][] _array;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			BattleField.awake += () =>
			{
				var size = BattleField.map.size;
				size.x += 2; size.y += 2;
				_array = new Platform[size.x][];
				var a = new ReadOnlyArray<Platform>[size.x];
				for (int x = 0; x < size.x; ++x) a[x] = new ReadOnlyArray<Platform>(_array[x] = new Platform[size.y]);
				array = new ReadOnlyArray<ReadOnlyArray<Platform>>(a);

				// quét map: Create

				// Create Border
				for (int x = 0, YMAX = size.y - 1; x < size.x; ++x)
				{
					"Border".Instantiate(new Vector3(x, 0), Quaternion.identity);
					"Border".Instantiate(new Vector3(x, YMAX), Quaternion.identity);
				}
				for (int y = size.y - 2, XMAX = size.x - 1; y > 0; --y)
				{
					"Border".Instantiate(new Vector3(0, y), Quaternion.identity);
					"Border".Instantiate(new Vector3(XMAX, y), Quaternion.identity);
				}
			};
		}


		public static Platform Spawn(/*name, pos*/)
		{
			throw new NotImplementedException();
		}


		protected void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			var index = transform.position.ToVector2Int();
#if DEBUG
			if (_array[index.x][index.y])
				throw new InvalidOperationException($"Không thể sinh platform vì tọa độ {index} đã có platform !");
#endif
			_array[index.x][index.y] = this;
			transform.SetParent(BattleField.instance.platformAnchor, false);
		}


		protected void OnDisable()
		{
			var index = transform.position.ToVector2Int();
#if DEBUG
			if (this != _array[index.x][index.y])
				throw new Exception($"Tọa độ {index} hiện tại không phải là platform: {this}");
#endif
			_array[index.x][index.y] = null;
		}


		public abstract bool IsBlockingTankMove(/*pos, direction, hasShip*/);


		public abstract bool OnCollision(Bullet bullet);


		[Serializable]
		protected struct ParticleArray
		{
			[Serializable]
			private struct Column
			{
				public GameObject[] objs;
			}
			[SerializeField] private Column[] columns;


			public GameObject this[int x, int y]
			{
				get => columns[x].objs[y];
				set => columns[x].objs[y] = value;
			}


			public bool isEmpty
			{
				get
				{
					foreach (var column in columns)
						foreach (var obj in column.objs)
							if (obj) return false;
					return true;
				}
			}
		}
	}
}