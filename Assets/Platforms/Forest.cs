using BattleCity.Tanks;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Forest : Platform
	{
		[SerializeField]
		private Particle particle;

		public override bool CanMove(Tank tank, Vector3 newDir) => true;


		public override bool OnCollision(Bullet bullet)
		{
			if (!bullet.canBurnForest) return false;

			var block = BLOCKS[BULLET_DIR_RELATIVE_BLOCK[bullet.direction]
				[bullet.transform.position - transform.position]];

			bool collise = false;
			for (int b = 0; b < block.Length; ++b)
				collise |= particle.Destroy(block[b].x, block[b].y);

			if (collise && particle.isEmpty) Destroy(gameObject);
			return false;
		}

		private static readonly IReadOnlyDictionary<int, ReadOnlyArray<Vector2Int>>
			BLOCKS = new Dictionary<int, ReadOnlyArray<Vector2Int>>
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
	}
}