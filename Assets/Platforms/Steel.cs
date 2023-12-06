using BattleCity.Tanks;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Steel : Platform
	{
		[SerializeField]
		private Particle particle;

		public override bool CanMove(Tank tank, Vector3 newDir)
			=> CanMove(tank.transform.position - transform.position, newDir, BLOCKS, particle);


		/*
		 * See "/Docs/Platform.CanMove().png" to understand the Algorithm !
		 */
		private static readonly IReadOnlyDictionary<int, ReadOnlyArray<Vector2Int>> BLOCKS
			= new Dictionary<int, ReadOnlyArray<Vector2Int>>
			{
				[1] = new(new Vector2Int[] { new(0, 0) }),
				[2] = new(new Vector2Int[] { new(0, 1) }),
				[3] = new(new Vector2Int[] { new(1, 0) }),
				[4] = new(new Vector2Int[] { new(1, 1) }),
				[12] = new(new Vector2Int[] { new(0, 0), new(0, 1) }),
				[24] = new(new Vector2Int[] { new(0, 1), new(1, 1) }),
				[34] = new(new Vector2Int[] { new(1, 0), new(1, 1) }),
				[13] = new(new Vector2Int[] { new(0, 0), new(1, 0) }),
			};


		public override bool OnBulletCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}