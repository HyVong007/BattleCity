using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Brick : Platform
	{
		[SerializeField] private ParticleArray particles;
		public override bool IsBlockingTankMove()
		{
			throw new System.NotImplementedException();
		}


		public override bool OnCollision(Bullet bullet)
		{
			var p = bullet.transform.position - transform.position;
			bool hideBullet =
				// A
				(p.x == 0 && p.y == 0) ?
					(
						(bullet.direction == Direction.Right) ? CheckShortRange(firstRange: (0, 0, 0, 1), secondRange: (1, 1, 0, 1)) :
						(bullet.direction == Direction.Up) && CheckShortRange(firstRange: (0, 1, 0, 0), secondRange: (0, 1, 1, 1))
					) :

				// B
				(p.x == 0.5f && p.y == 0) ?
					(
						(bullet.direction == Direction.Right) ? CheckShortRange(firstRange: (2, 2, 0, 1), secondRange: (3, 3, 0, 1)) :
						(bullet.direction == Direction.Left) ? CheckShortRange(firstRange: (1, 1, 0, 1), secondRange: (0, 0, 0, 1)) :
						(bullet.direction == Direction.Up) && CheckLongRange(firstRange: (0, 3, 0, 0), secondRange: (0, 3, 1, 1))
					) :

				// C
				(p.x == 1 && p.y == 0) ?
					(
						(bullet.direction == Direction.Left) ? CheckShortRange(firstRange: (3, 3, 0, 1), secondRange: (2, 2, 0, 1)) :
						(bullet.direction == Direction.Up) && CheckShortRange(firstRange: (2, 3, 0, 0), secondRange: (2, 3, 1, 1))
					) :

				// D
				(p.x == 0 && p.y == 0.5f) ?
					(
						(bullet.direction == Direction.Right) ? CheckLongRange(firstRange: (0, 0, 0, 3), secondRange: (1, 1, 0, 3)) :
						(bullet.direction == Direction.Down) ? CheckShortRange(firstRange: (0, 1, 1, 1), secondRange: (0, 1, 0, 0)) :
						(bullet.direction == Direction.Up) && CheckShortRange(firstRange: (0, 1, 2, 2), secondRange: (0, 1, 3, 3))
					) :

				// E
				(p.x == 0.5f && p.y == 0.5f) ?
					(
						(bullet.direction == Direction.Right) ? CheckLongRange(firstRange: (2, 2, 0, 3), secondRange: (3, 3, 0, 3)) :
						(bullet.direction == Direction.Down) ? CheckLongRange(firstRange: (0, 3, 1, 1), secondRange: (0, 3, 0, 0)) :
						(bullet.direction == Direction.Left) ? CheckLongRange(firstRange: (1, 1, 0, 3), secondRange: (0, 0, 0, 3)) :
						CheckLongRange(firstRange: (0, 3, 2, 2), secondRange: (0, 3, 3, 3))
					) :

				// F
				(p.x == 1 && p.y == 0.5f) ?
					(
						(bullet.direction == Direction.Down) ? CheckShortRange(firstRange: (2, 3, 1, 1), secondRange: (2, 3, 0, 0)) :
						(bullet.direction == Direction.Left) ? CheckLongRange(firstRange: (3, 3, 0, 3), secondRange: (2, 2, 0, 3)) :
						(bullet.direction == Direction.Up) && CheckShortRange(firstRange: (2, 3, 2, 2), secondRange: (2, 3, 3, 3))
					) :

				// G
				(p.x == 0 && p.y == 1) ?
					(
						(bullet.direction == Direction.Right) ? CheckShortRange(firstRange: (0, 0, 2, 3), secondRange: (1, 1, 2, 3)) :
						(bullet.direction == Direction.Down) && CheckShortRange(firstRange: (0, 1, 3, 3), secondRange: (0, 1, 2, 2))
					) :

				// H
				(p.x == 0.5f && p.y == 1) ?
					(
						(bullet.direction == Direction.Right) ? CheckShortRange(firstRange: (2, 2, 2, 3), secondRange: (3, 3, 2, 3)) :
						(bullet.direction == Direction.Down) ? CheckLongRange(firstRange: (0, 3, 3, 3), secondRange: (0, 3, 2, 2)) :
						(bullet.direction == Direction.Left) && CheckShortRange(firstRange: (1, 1, 2, 3), secondRange: (0, 0, 2, 3))
					) :

				// I
				(p.x == 1 && p.y == 1) && (
						(bullet.direction == Direction.Down) ? CheckShortRange(firstRange: (2, 3, 3, 3), secondRange: (2, 3, 2, 2)) :
						(bullet.direction == Direction.Left) && CheckShortRange(firstRange: (3, 3, 2, 3), secondRange: (2, 2, 2, 3))
					);

			if (hideBullet)
			{
				if (particles.isEmpty) Destroy(gameObject);
				// Effect small explosion
				if (bullet.owner != Bullet.Owner.Enemy || bullet.canDestroySteel)
					; // Effect bullet collise brick
			}
			return hideBullet;


			bool CheckShortRange((int minX, int maxX, int minY, int maxY) firstRange, (int minX, int maxX, int minY, int maxY) secondRange)
			{
				bool firstRange_HasItem = CheckRange(firstRange);
				return (!bullet.canDestroySteel && firstRange_HasItem) || CheckRange(secondRange) || firstRange_HasItem;


				bool CheckRange((int minX, int maxX, int minY, int maxY) range)
				{
					bool hasItem = false;
					for (int x = range.minX; x <= range.maxX; ++x)
						for (int y = range.minY; y <= range.maxY; ++y)
							if (particles[x, y])
							{
								hasItem = true;
								Destroy(particles[x, y]);
								particles[x, y] = null;
							}
					return hasItem;
				}
			}


			bool CheckLongRange((int minX, int maxX, int minY, int maxY) firstRange, (int minX, int maxX, int minY, int maxY) secondRange)
			{
				var array = new (int x, int y)[4];
				int x, y, i = 0;

				#region Kiểm tra firstRange
				for (x = firstRange.minX; x <= firstRange.maxX; ++x)
					for (y = firstRange.minY; y <= firstRange.maxY; ++y) array[i++] = (x, y);

				bool firstRange_HasItem = CheckPair(array[1], array[0]) | CheckPair(array[2], array[3]); // BIT OR
				#endregion

				if (!bullet.canDestroySteel && firstRange_HasItem) return true;

				#region Kiểm tra secondRange
				i = 0;
				for (x = secondRange.minX; x <= secondRange.maxX; ++x)
					for (y = secondRange.minY; y <= secondRange.maxY; ++y) array[i++] = (x, y);

				return (CheckPair(array[1], array[0]) | CheckPair(array[2], array[3])) // BIT OR
					|| firstRange_HasItem; // LOGIC OR
				#endregion


				bool CheckPair((int x, int y) a, (int x, int y) b)
				{
					if (particles[a.x, a.y])
					{
						Destroy(particles[a.x, a.y]);
						particles[a.x, a.y] = null;
						if (particles[b.x, b.y])
						{
							Destroy(particles[b.x, b.y]);
							particles[b.x, b.y] = null;
						}
						return true;
					}
					return false;
				}
			}
		}
	}
}