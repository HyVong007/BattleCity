using BattleCity.Tanks;
using System;
using System.Collections.Generic;
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
				var size = BattleField.stage.size;
				size.x += 2; size.y += 2;
				_array = new Platform[size.x][];
				var a = new ReadOnlyArray<Platform>[size.x];
				for (int x = 0; x < size.x; ++x) a[x] = new ReadOnlyArray<Platform>(_array[x] = new Platform[size.y]);
				array = new ReadOnlyArray<ReadOnlyArray<Platform>>(a);

				// quét map: Create

				// Create Border
				for (int x = 0, YMAX = size.y - 1; x < size.x; ++x)
				{
					Instantiate(PlatformPrefabs.instance.border, new Vector3(x, 0), Quaternion.identity);
					Instantiate(PlatformPrefabs.instance.border, new Vector3(x, YMAX), Quaternion.identity);
				}
				for (int y = size.y - 2, XMAX = size.x - 1; y > 0; --y)
				{
					Instantiate(PlatformPrefabs.instance.border, new Vector3(0, y), Quaternion.identity);
					Instantiate(PlatformPrefabs.instance.border, new Vector3(XMAX, y), Quaternion.identity);
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


		public abstract bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip);


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
							if (obj && obj.activeSelf) return false;
					return true;
				}
			}


			public bool Destroy(int x, int y)
			{
				if (this[x, y] && this[x, y].activeSelf)
				{
					this[x, y].SetActive(false);
					UnityEngine.Object.Destroy(this[x, y]);
					this[x, y] = null;
					return true;
				}
				return false;
			}
		}


		protected static class Bullet_Platform_Relatives
		{
			public static readonly Vector2 A = new Vector2(0, 0), B = new Vector2(0.5f, 0), C = new Vector2(1, 0),
				D = new Vector2(0, 0.5f), E = new Vector2(0.5f, 0.5f), F = new Vector2(1, 0.5f),
				G = new Vector2(0, 1), H = new Vector2(0.5f, 1), I = new Vector2(1, 1);
		}


		/// <summary>
		/// Dùng cho platform có 4 particle !
		/// </summary>
		/// <param name="relative">Tọa độ tương đối của <see cref="Bullet"/> đối với <see cref="Platform"/></param>
		protected static void FindBulletCollisionObjs(in Vector2 relative, in ParticleArray particles, List<GameObject> objs)
		{
			objs.Clear();
			if (relative == Bullet_Platform_Relatives.A)
			{
				if (particles[0, 0]) objs.Add(particles[0, 0]);
			}
			else if (relative == Bullet_Platform_Relatives.B)
			{
				if (particles[0, 0]) objs.Add(particles[0, 0]);
				if (particles[1, 0]) objs.Add(particles[1, 0]);
			}
			else if (relative == Bullet_Platform_Relatives.C)
			{
				if (particles[1, 0]) objs.Add(particles[1, 0]);
			}
			else if (relative == Bullet_Platform_Relatives.D)
			{
				if (particles[0, 0]) objs.Add(particles[0, 0]);
				if (particles[0, 1]) objs.Add(particles[0, 1]);
			}
			else if (relative == Bullet_Platform_Relatives.E)
			{
				if (particles[0, 0]) objs.Add(particles[0, 0]);
				if (particles[0, 1]) objs.Add(particles[0, 1]);
				if (particles[1, 0]) objs.Add(particles[1, 0]);
				if (particles[1, 1]) objs.Add(particles[1, 1]);
			}
			else if (relative == Bullet_Platform_Relatives.F)
			{
				if (particles[1, 0]) objs.Add(particles[1, 0]);
				if (particles[1, 1]) objs.Add(particles[1, 1]);
			}
			else if (relative == Bullet_Platform_Relatives.G)
			{
				if (particles[0, 1]) objs.Add(particles[0, 1]);
			}
			else if (relative == Bullet_Platform_Relatives.H)
			{
				if (particles[0, 1]) objs.Add(particles[0, 1]);
				if (particles[1, 1]) objs.Add(particles[1, 1]);
			}
			else if (relative == Bullet_Platform_Relatives.I)
			{
				if (particles[1, 1]) objs.Add(particles[1, 1]);
			}
			else throw new Exception();
		}
	}
}