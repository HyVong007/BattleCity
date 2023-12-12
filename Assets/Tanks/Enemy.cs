using UnityEngine;


namespace BattleCity.Tanks
{
	public sealed class Enemy : Tank
	{
		public override Vector3 direction { get; protected set; }
		public override Color color { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }


		public override bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}


		private void Update()
		{
			#region Input Move
			Vector3 newDir = default; ;
			if (Input.GetKey(KeyCode.UpArrow)) newDir = Vector3.up;
			else if (Input.GetKey(KeyCode.RightArrow)) newDir = Vector3.right;
			else if (Input.GetKey(KeyCode.DownArrow)) newDir = Vector3.down;
			else if (Input.GetKey(KeyCode.LeftArrow)) newDir = Vector3.left;

			if (!isMoving && newDir != default)
			{
				// Xoay tank
				direction = newDir;
			}
			#endregion
		}
	}
}