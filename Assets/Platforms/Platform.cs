using BattleCity.Tanks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BattleCity.Platforms
{
	public abstract class Platform : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Platform>> platforms { get; private set; }


		public abstract bool OnBulletCollision(Bullet bullet);


		public static readonly IReadOnlyDictionary<int, string> INT_NAME = new Dictionary<int, string>
		{
			[0] = null,
			[1] = $"{nameof(Brick)} full",
			[2] = $"{nameof(Brick)} left",
			[3] = $"{nameof(Brick)} left up",
			[4] = $"{nameof(Brick)} left down",
			[5] = $"{nameof(Brick)} right",
			[6] = $"{nameof(Brick)} right up",
			[7] = $"{nameof(Brick)} righ down",
			[8] = $"{nameof(Brick)} up",
			[9] = $"{nameof(Brick)} down",
			[10] = $"{nameof(Steel)} full",
			[11] = $"{nameof(Steel)} left",
			[12] = $"{nameof(Steel)} left up",
			[13] = $"{nameof(Steel)} left down",
			[14] = $"{nameof(Steel)} right",
			[15] = $"{nameof(Steel)} right up",
			[16] = $"{nameof(Steel)} right down",
			[17] = $"{nameof(Steel)} up",
			[18] = $"{nameof(Steel)} down",
			[19] = nameof(Border),
			[20] = nameof(Water),
			[21] = nameof(Eagle),
			[22] = nameof(Forest),
			[23] = nameof(Sand),
		};
		public static void LoadLevel(in Level level)
		{
			var anchor = new GameObject().transform;
			anchor.name = "Platforms";
			var array = new ReadOnlyArray<Platform>[level.platforms.Length];
			int NUM_Y = level.platforms[0].Length;
			for (int x = 0; x < array.Length; ++x)
			{
				var a = new Platform[NUM_Y];
				array[x] = new(a);
				for (int y = 0; y < NUM_Y; ++y)
				{
					var name = INT_NAME[level.platforms[x][y]];
					a[y] = name == null ? null : Addressables.InstantiateAsync(
						$"Assets/Platforms/Prefab/{name}.prefab",
						new Vector3(x, y), Quaternion.identity, anchor)
						.WaitForCompletion().GetComponent<Platform>();
				}
			}
			platforms = new(array);
		}


		[Serializable]
		protected sealed class Particle
		{
			[Serializable]
			private sealed class Y
			{
				public GameObject[] objs;
			}

			[SerializeField] private Y[] X;

			public GameObject this[int x, int y] => X[x].objs[y];
		}


		public abstract bool CanMove(Tank tank, Vector3 newDir);


		#region Common codes for Brick, Steel, Forest
		/*
		 * See "/Docs/Platform.CanMove().png" to understand the Algorithm !
		 */
		protected static bool CanMove(in Vector3 relativePos, in Vector3 newDir,
			IReadOnlyDictionary<int, ReadOnlyArray<Vector2Int>> BLOCKS, Particle particle)
		{
			ReadOnlyArray<Vector2Int> block;
			if (RELATIVE_BLOCK.TryGetValue(relativePos, out int blockID)) block = BLOCKS[blockID];
			else if (RELATIVE_DIR_BLOCK.TryGetValue(relativePos, out var dir_blockID))
				block = BLOCKS[dir_blockID[newDir]];
			else throw new ArgumentOutOfRangeException();

			for (int b = 0; b < block.Length; ++b)
			{
				var v = block[b];
				if (particle[v.x, v.y]) return false;
			}

			return true;
		}


		private static readonly IReadOnlyDictionary<Vector3, int> RELATIVE_BLOCK = new Dictionary<Vector3, int>
		{
			[new(-0.5f, 1)] = 2,
			[new(0, 1)] = 24,
			[new(0.5f, 1)] = 4,
			[new(-1, 0.5f)] = 2,
			[new(0, 0.5f)] = 13,
			[new(1, 0.5f)] = 4,
			[new(-1, 0)] = 12,
			[new(-0.5f, 0)] = 34,
			[new(0.5f, 0)] = 12,
			[new(1, 0)] = 34,
			[new(-1, -0.5f)] = 1,
			[new(0, -0.5f)] = 24,
			[new(1, -0.5f)] = 3,
			[new(-0.5f, -1)] = 1,
			[new(0, -1)] = 13,
			[new(0.5f, -1)] = 3,
		};

		private static readonly IReadOnlyDictionary<Vector3, Dictionary<Vector3, int>> RELATIVE_DIR_BLOCK
			= new Dictionary<Vector3, Dictionary<Vector3, int>>
			{
				[new(-0.5f, 0.5f)] = new()
				{
					[Vector3.right] = 4,
					[Vector3.down] = 1
				},
				[new(0.5f, 0.5f)] = new()
				{
					[Vector3.left] = 2,
					[Vector3.down] = 3
				},
				[new(-0.5f, -0.5f)] = new()
				{
					[Vector3.right] = 3,
					[Vector3.up] = 2
				},
				[new(0.5f, -0.5f)] = new()
				{
					[Vector3.left] = 1,
					[Vector3.up] = 4
				}
			};
		#endregion
	}
}