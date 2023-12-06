using BattleCity.Platforms;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Tanks
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public abstract class Tank : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Tank>> tanks { get; private set; }


		public Color color { get; protected set; }

		public Vector3 direction { get; protected set; }

		public float speed { get; protected set; }

		public bool hasShip { get; protected set; }

		[SerializeField]
		[HideInInspector]
		protected SpriteRenderer spriteRenderer;

		[SerializeField]
		[HideInInspector]
		protected Animator animator;


		protected void Reset()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator = GetComponent<Animator>();
		}


		public abstract bool OnBulletCollision(Bullet bullet);


		#region CanMove
		/*
		 * See "/Docs/Vectors for Tank check moving.png" to understand the Algorithm
		 */
		public bool CanMove(in Vector3 newDir)
		{
			var origin = transform.position;
			var vectors = DIR_VECTORS[newDir];

			#region Check Platforms
			for (int v = 0; v < vectors.Length; ++v)
			{
				var pos = origin + vectors[v];
				if (pos.ToVector3Int() != pos) continue;

				var platform = Platform.platforms[(int)pos.x][(int)pos.y];
				if (!platform) continue;

				if (!platform.CanMove(this, newDir)) return false;
			}
			#endregion

			#region Check Tanks
			for (int v = 0; v < 3; ++v) // Only loop 3 first vectors 
			{
				var pos = (origin + vectors[v]) * 2; // (...)*2 => convert Tank world to Tank array coordinate system
				if (tanks[(int)pos.x][(int)pos.y]) return false;
			}
			#endregion

			return true;
		}

		private static readonly IReadOnlyDictionary<Vector3, ReadOnlyArray<Vector3>> DIR_VECTORS
			= new Dictionary<Vector3, ReadOnlyArray<Vector3>>
			{
				// [...]= {vector0, vector1, vector2....}
				// 3 first vectors (vector0, vector1, vector2) are used to check Tank vs Tank collision
				[Vector3.up] = new(new Vector3[] { new(-0.5f, 1), new(0, 1), new(0.5f, 1), new(-0.5f, 0.5f), new(0.5f, 0.5f), new(0, 0.5f) }),
				[Vector3.right] = new(new Vector3[] { new(1, 0.5f), new(1, 0), new(1, -0.5f), new(0.5f, 0.5f), new(0.5f, -0.5f), new(0.5f, 0) }),
				[Vector3.down] = new(new Vector3[] { new(-0.5f, -1), new(0, -1), new(0.5f, -1), new(-0.5f, -0.5f), new(0.5f, -0.5f), new(0, -0.5f) }),
				[Vector3.left] = new(new Vector3[] { new(-1, 0.5f), new(-1, 0), new(-1, -0.5f), new(-0.5f, 0.5f), new(-0.5f, -0.5f), new(-0.5f, 0) }),
			};
		#endregion
	}
}