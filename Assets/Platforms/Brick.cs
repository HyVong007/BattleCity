using BattleCity.Tanks;
using System;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Brick : Platform
	{
		[SerializeField] private ParticleArray particles;
		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip)
		{
			var p = tankPosition - transform.position;
			switch (tankDirection)
			{
				case Direction.Up:
					// A
					if (p.x == 0 && p.y == -0.5f) return ExistParticle(0, 1, 0, 1);
					// B
					if (p.x == 0.5f && p.y == -0.5f) return ExistParticle(0, 3, 0, 1);
					// C
					if (p.x == 1 && p.y == -0.5f) return ExistParticle(2, 3, 0, 1);
					// D
					if (p.x == 0 && p.y == 0) return ExistParticle(0, 1, 2, 3);
					// E
					if (p.x == 0.5f && p.y == 0) return ExistParticle(0, 3, 2, 3);
					// F
					if (p.x == 1 && p.y == 0) return ExistParticle(2, 3, 2, 3);
					break;

				case Direction.Right:
					// A
					if (p.x == -0.5f && p.y == 0) return ExistParticle(0, 1, 0, 1);
					// B
					if (p.x == 0 && p.y == 0) return ExistParticle(2, 3, 0, 1);
					// C
					if (p.x == -0.5f && p.y == 0.5f) return ExistParticle(0, 1, 0, 3);
					// D
					if (p.x == 0 && p.y == 0.5f) return ExistParticle(2, 3, 0, 3);
					// E
					if (p.x == -0.5f && p.y == 1) return ExistParticle(0, 1, 2, 3);
					// F
					if (p.x == 0 && p.y == 1) return ExistParticle(2, 3, 2, 3);
					break;

				case Direction.Down:
					// A
					if (p.x == 0 && p.y == 1) return ExistParticle(0, 1, 0, 1);
					// B
					if (p.x == 0.5f && p.y == 1) return ExistParticle(0, 3, 0, 1);
					// C
					if (p.x == 1 && p.y == 1) return ExistParticle(2, 3, 0, 1);
					// D
					if (p.x == 0 && p.y == 1.5f) return ExistParticle(0, 1, 2, 3);
					// E
					if (p.x == 0.5f && p.y == 1.5f) return ExistParticle(0, 3, 2, 3);
					// F
					if (p.x == 1 && p.y == 1.5f) return ExistParticle(2, 3, 2, 3);
					break;

				case Direction.Left:
					// A
					if (p.x == 1 && p.y == 0) return ExistParticle(0, 1, 0, 1);
					// B
					if (p.x == 1.5f && p.y == 0) return ExistParticle(2, 3, 0, 1);
					// C
					if (p.x == 1 && p.y == 0.5f) return ExistParticle(0, 1, 0, 3);
					// D
					if (p.x == 1.5f && p.y == 0.5f) return ExistParticle(2, 3, 0, 3);
					// E
					if (p.x == 1 && p.y == 1) return ExistParticle(0, 1, 2, 3);
					// F
					if (p.x == 1.5f && p.y == 1) return ExistParticle(2, 3, 2, 3);
					break;
			}
			throw new Exception();


			bool ExistParticle(int minX, int maxX, int minY, int maxY)
			{
				for (int x = minX; x <= maxX; ++x)
					for (int y = minY; y <= maxY; ++y)
						if (particles[x, y]) return true;
				return false;
			}
		}


		public override bool OnCollision(Bullet bullet)
		{
			Vector2 p = bullet.transform.position - transform.position;
			bool hideBullet =
				p == Bullet_Platform_Relatives.A ?
						bullet.direction == Direction.Right ? CheckShortRange(firstRange: (0, 0, 0, 1), secondRange: (1, 1, 0, 1)) :
						bullet.direction == Direction.Up && CheckShortRange(firstRange: (0, 1, 0, 0), secondRange: (0, 1, 1, 1))
					:
				p == Bullet_Platform_Relatives.B ?
						bullet.direction == Direction.Right ? CheckShortRange(firstRange: (2, 2, 0, 1), secondRange: (3, 3, 0, 1)) :
						bullet.direction == Direction.Left ? CheckShortRange(firstRange: (1, 1, 0, 1), secondRange: (0, 0, 0, 1)) :
						bullet.direction == Direction.Up && CheckLongRange(firstRange: (0, 3, 0, 0), secondRange: (0, 3, 1, 1))
					 :
				p == Bullet_Platform_Relatives.C ?
						bullet.direction == Direction.Left ? CheckShortRange(firstRange: (3, 3, 0, 1), secondRange: (2, 2, 0, 1)) :
						bullet.direction == Direction.Up && CheckShortRange(firstRange: (2, 3, 0, 0), secondRange: (2, 3, 1, 1))
					 :
				p == Bullet_Platform_Relatives.D ?
						bullet.direction == Direction.Right ? CheckLongRange(firstRange: (0, 0, 0, 3), secondRange: (1, 1, 0, 3)) :
						bullet.direction == Direction.Down ? CheckShortRange(firstRange: (0, 1, 1, 1), secondRange: (0, 1, 0, 0)) :
						bullet.direction == Direction.Up && CheckShortRange(firstRange: (0, 1, 2, 2), secondRange: (0, 1, 3, 3))
					 :
				p == Bullet_Platform_Relatives.E ?
						bullet.direction == Direction.Right ? CheckLongRange(firstRange: (2, 2, 0, 3), secondRange: (3, 3, 0, 3)) :
						bullet.direction == Direction.Down ? CheckLongRange(firstRange: (0, 3, 1, 1), secondRange: (0, 3, 0, 0)) :
						bullet.direction == Direction.Left ? CheckLongRange(firstRange: (1, 1, 0, 3), secondRange: (0, 0, 0, 3)) :
						CheckLongRange(firstRange: (0, 3, 2, 2), secondRange: (0, 3, 3, 3))
					 :
				p == Bullet_Platform_Relatives.F ?
						bullet.direction == Direction.Down ? CheckShortRange(firstRange: (2, 3, 1, 1), secondRange: (2, 3, 0, 0)) :
						bullet.direction == Direction.Left ? CheckLongRange(firstRange: (3, 3, 0, 3), secondRange: (2, 2, 0, 3)) :
						bullet.direction == Direction.Up && CheckShortRange(firstRange: (2, 3, 2, 2), secondRange: (2, 3, 3, 3))
					 :
				p == Bullet_Platform_Relatives.G ?
						bullet.direction == Direction.Right ? CheckShortRange(firstRange: (0, 0, 2, 3), secondRange: (1, 1, 2, 3)) :
						bullet.direction == Direction.Down && CheckShortRange(firstRange: (0, 1, 3, 3), secondRange: (0, 1, 2, 2))
					 :
				p == Bullet_Platform_Relatives.H ?
						bullet.direction == Direction.Right ? CheckShortRange(firstRange: (2, 2, 2, 3), secondRange: (3, 3, 2, 3)) :
						bullet.direction == Direction.Down ? CheckLongRange(firstRange: (0, 3, 3, 3), secondRange: (0, 3, 2, 2)) :
						bullet.direction == Direction.Left && CheckShortRange(firstRange: (1, 1, 2, 3), secondRange: (0, 0, 2, 3))
					 :
				p == Bullet_Platform_Relatives.I && (
						bullet.direction == Direction.Down ? CheckShortRange(firstRange: (2, 3, 3, 3), secondRange: (2, 3, 2, 2)) :
						bullet.direction == Direction.Left && CheckShortRange(firstRange: (3, 3, 2, 3), secondRange: (2, 2, 2, 3)));

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