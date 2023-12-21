using BattleCity.Tanks;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Brick : Platform
	{
		[SerializeField] private Particle particle;


		public override bool CanMove(Tank tank, Vector3 newDir)
			=> CanMove(tank.transform.position - transform.position, newDir, POINT_BLOCKS, particle);

		private static readonly IReadOnlyDictionary<int, ReadOnlyArray<Vector2Int>>
			POINT_BLOCKS = new Dictionary<int, ReadOnlyArray<Vector2Int>>
			{
				[1] = new(new Vector2Int[] { new(0, 0), new(0, 1), new(1, 0), new(1, 1), }),
				[2] = new(new Vector2Int[] { new(0, 2), new(0, 3), new(1, 2), new(1, 3), }),
				[3] = new(new Vector2Int[] { new(2, 0), new(2, 1), new(3, 0), new(3, 1), }),
				[4] = new(new Vector2Int[] { new(2, 2), new(2, 3), new(3, 2), new(3, 3), }),
				[12] = new(new Vector2Int[] { new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(0, 2), new(0, 3), new(1, 2), new(1, 3) }),
				[24] = new(new Vector2Int[] { new(0, 2), new(0, 3), new(1, 2), new(1, 3), new(2, 2), new(2, 3), new(3, 2), new(3, 3) }),
				[34] = new(new Vector2Int[] { new(2, 0), new(2, 1), new(3, 0), new(3, 1), new(2, 2), new(2, 3), new(3, 2), new(3, 3) }),
				[13] = new(new Vector2Int[] { new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(2, 0), new(2, 1), new(3, 0), new(3, 1) }),
			};


		public override bool OnCollision(Bullet bullet)
		{
			int id = BULLET_DIR_RELATIVE_BLOCK[bullet.direction]
				[bullet.transform.position - transform.position];

			if (bullet.canDestroySteel)
			{
				#region Bullet phá sắt thì va chạm như Steel
				var block = POINT_BLOCKS[id];
				bool hideBullet = false;
				for (int b = 0; b < block.Length; ++b)
					hideBullet |= particle.Destroy(block[b].x, block[b].y);

				if (hideBullet && particle.isEmpty) Destroy(gameObject);
				return hideBullet;
				#endregion
			}

			#region Bullet phá từng lớp gạch
			var (Xmin, Xmax, Ymin, Ymax) = RANGE_BLOCKS[id];
			var d = bullet.direction;

			return d.x == 0 ?
				 // Up, Down
				 DestroyRange(Xmin, Xmax, d.y > 0 ? Ymin : Ymax, d.y > 0 ? Ymin : Ymax)
					|| DestroyRange(Xmin, Xmax, d.y > 0 ? Ymax : Ymin, d.y > 0 ? Ymax : Ymin)
			:
				// Left, Right
				DestroyRange(d.x > 0 ? Xmin : Xmax, d.x > 0 ? Xmin : Xmax, Ymin, Ymax)
					|| DestroyRange(d.x > 0 ? Xmax : Xmin, d.x > 0 ? Xmax : Xmin, Ymin, Ymax);


			bool DestroyRange(int Xmin, int Xmax, int Ymin, int Ymax)
			{
				bool hideBullet = false;
				for (int x = Xmin; x <= Xmax; ++x)
					for (int y = Ymin; y <= Ymax; ++y) hideBullet |= particle.Destroy(x, y);

				if (hideBullet && particle.isEmpty) Destroy(gameObject);
				return hideBullet;
			}
			#endregion
		}

		private static readonly IReadOnlyDictionary<int, (int Xmin, int Xmax, int Ymin, int Ymax)>
			RANGE_BLOCKS = new Dictionary<int, (int Xmin, int Xmax, int Ymin, int Ymax)>
			{
				[1] = (0, 1, 0, 1),
				[2] = (0, 1, 2, 3),
				[3] = (2, 3, 0, 1),
				[4] = (2, 3, 2, 3),
				[12] = (0, 1, 0, 3),
				[24] = (0, 3, 2, 3),
				[34] = (2, 3, 0, 3),
				[13] = (0, 3, 0, 1)
			};
	}
}