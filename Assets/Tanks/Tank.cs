using BattleCity.Items;
using BattleCity.Platforms;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Tanks
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public abstract class Tank : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Tank>> tanks { get; private set; }
		protected static Tank[][] Δarray;


		public abstract Color color { get; protected set; }

		[SerializeField] protected Asset asset;
		/// <summary>
		/// Không thể set khi đang Move
		/// </summary>
		public abstract Vector3 direction { get; protected set; }

		public float speed { get; protected set; }

		public bool hasShip { get; protected set; }

		[SerializeField]
		[HideInInspector]
		protected SpriteRenderer spriteRenderer;

		[SerializeField]
		[HideInInspector]
		protected Animator animator;


		static Tank()
		{
			BattleField.onAwake += () =>
			{
				tanks = Util.NewReadOnlyArray(BattleField.level.width * 2,
					BattleField.level.height * 2, out Δarray);
			};
		}


		protected void Reset()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator = GetComponent<Animator>();
		}


		protected void OnEnable()
		{
			direction = this is Player ? Vector3.up : Vector3.down;
			index = (transform.position * 2).ToVector3Int();
			Δarray[index.x][index.y] = this;

			#region Check Item
			if (Item.current && (Setting.enemyCanPickItem || this is Player)
				&& (Item.current.transform.position - transform.position).sqrMagnitude < 1)
				Item.current.OnCollision(this);
			#endregion
		}


		#region CanMove
		/*
		 * See "/Docs/Vectors for Tank check moving.png" to understand the Algorithm
		 */
		public bool CanMove(in Vector3 newDir)
		{
			var origin = transform.position;
			var vectors = DIR_VECTORS[newDir];

			#region Check Tanks
			for (int v = 0; v < 3; ++v)
			{
				var pos = (origin + vectors[v]) * 2;
				if (tanks[(int)pos.x][(int)pos.y]) return false;
			}
			#endregion

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

			return true;
		}

		private static readonly IReadOnlyDictionary<Vector3, ReadOnlyArray<Vector3>>
			DIR_VECTORS = new Dictionary<Vector3, ReadOnlyArray<Vector3>>
			{
				// [...]= {vector0, vector1, vector2....}
				// 3 first vectors (vector0, vector1, vector2) are used to check Tank vs Tank collision
				[Vector3.up] = new(new Vector3[] { new(-0.5f, 1), new(0, 1), new(0.5f, 1), new(-0.5f, 0.5f), new(0.5f, 0.5f), new(0, 0.5f) }),
				[Vector3.right] = new(new Vector3[] { new(1, 0.5f), new(1, 0), new(1, -0.5f), new(0.5f, 0.5f), new(0.5f, -0.5f), new(0.5f, 0) }),
				[Vector3.down] = new(new Vector3[] { new(-0.5f, -1), new(0, -1), new(0.5f, -1), new(-0.5f, -0.5f), new(0.5f, -0.5f), new(0, -0.5f) }),
				[Vector3.left] = new(new Vector3[] { new(-1, 0.5f), new(-1, 0), new(-1, -0.5f), new(-0.5f, 0.5f), new(-0.5f, -0.5f), new(-0.5f, 0) }),
			};
		#endregion


		public bool isMoving { get; private set; }
		[Tooltip("Tối đa 0.125")]
		[SerializeField] private float moveSpeed;
		[Tooltip("Thời gian (ms) mỗi bước moveSpeed")]
		[SerializeField] private int delayMoving;
		private Vector3Int index;

		public async UniTask Move(Vector3 dir)
		{
			direction = dir;
			isMoving = isMoving ? throw new InvalidOperationException() : true;
			try
			{
				Δarray[index.x][index.y] = null;
				index += dir.ToVector3Int();
				Δarray[index.x][index.y] = this;
				dir *= moveSpeed;
				for (float i = 0.5f / moveSpeed; i > 0; --i)
				{
					transform.position += dir;
					await UniTask.Delay(delayMoving);
				}
				transform.position = new(index.x * 0.5f, index.y * 0.5f);

				#region Check Item
				if (Item.current && (Setting.enemyCanPickItem || this is Player)
					&& (Item.current.transform.position - transform.position).sqrMagnitude < 1)
					Item.current.OnCollision(this);
				#endregion

				#region Check Special Platform

				#endregion
			}
			finally { isMoving = false; }
		}


		public abstract bool OnCollision(Bullet bullet);
	}
}