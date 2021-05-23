using BattleCity.Tanks;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Steel : Platform
	{
		[SerializeField] private ParticleArray particles;
		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip)
		{
			var p = tankPosition - transform.position;
			switch (tankDirection)
			{
				case Direction.Up:
					// A
					if (p.x == 0 && p.y == -0.5f) return particles[0, 0];
					// B
					if (p.x == 0.5f && p.y == -0.5f) return particles[0, 0] || particles[1, 0];
					// C
					if (p.x == 1 && p.y == -0.5f) return particles[1, 0];
					// D
					if (p.x == 0 && p.y == 0) return particles[0, 1];
					// E
					if (p.x == 0.5f && p.y == 0) return particles[0, 1] || particles[1, 1];
					// F
					if (p.x == 1 && p.y == 0) return particles[1, 1];
					break;

				case Direction.Right:
					// A
					if (p.x == -0.5f && p.y == 0) return particles[0, 0];
					// B
					if (p.x == 0 && p.y == 0) return particles[1, 0];
					// C
					if (p.x == -0.5f && p.y == 0.5f) return particles[0, 0] || particles[0, 1];
					// D
					if (p.x == 0 && p.y == 0.5f) return particles[1, 0] || particles[1, 1];
					// E
					if (p.x == -0.5f && p.y == 1) return particles[0, 1];
					// F
					if (p.x == 0 && p.y == 1) return particles[1, 1];
					break;

				case Direction.Down:
					// A
					if (p.x == 0 && p.y == 1) return particles[0, 0];
					// B
					if (p.x == 0.5f && p.y == 1) return particles[0, 0] || particles[1, 0];
					// C
					if (p.x == 1 && p.y == 1) return particles[1, 0];
					// D
					if (p.x == 0 && p.y == 1.5f) return particles[0, 1];
					// E
					if (p.x == 0.5f && p.y == 1.5f) return particles[0, 1] || particles[1, 1];
					// F
					if (p.x == 1 && p.y == 1.5f) return particles[1, 1];
					break;

				case Direction.Left:
					// A
					if (p.x == 1 && p.y == 0) return particles[0, 0];
					// B
					if (p.x == 1.5f && p.y == 0) return particles[1, 0];
					// C
					if (p.x == 1 && p.y == 0.5f) return particles[0, 0] || particles[0, 1];
					// D
					if (p.x == 1.5f && p.y == 0.5f) return particles[1, 0] || particles[1, 1];
					// E
					if (p.x == 1 && p.y == 1) return particles[0, 1];
					// F
					if (p.x == 1.5f && p.y == 1) return particles[1, 1];
					break;
			}
			throw new Exception();
		}


		private static readonly List<GameObject> tmp = new List<GameObject>();
		public override bool OnCollision(Bullet bullet)
		{
			FindBulletCollisionObjs(bullet.transform.position - transform.position, particles, tmp);
			if (tmp.Count == 0) return false;

			if (bullet.canDestroySteel)
			{
				foreach (var obj in tmp)
				{
					obj.SetActive(false);
					Destroy(obj);
				}

				if (particles.isEmpty) Destroy(gameObject);
				// Sound 1
			}
			else
			{
				// Bullet explode anim
				// Sound 2
			}

			return true;
		}
	}
}