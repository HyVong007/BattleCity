using BattleCity.Tanks;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Eagle : Platform
	{
		public static readonly IReadOnlyList<Eagle> list = new List<Eagle>();
		private void Awake()
		{
			(list as List<Eagle>).Add(this);
		}


		private void OnDisable()
		{
			(list as List<Eagle>).Remove(this);
		}


		public override bool CanMove(Tank tank, Vector3 newDir) => false;


		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private Sprite dead;
		public override bool OnCollision(Bullet bullet)
		{
			if (spriteRenderer.sprite == dead) return false;

			spriteRenderer.sprite = dead;
			BattleField.End();
			return true;
		}


		public static bool hasDead
		{
			get
			{
				foreach (var eagle in list)
					if (eagle.spriteRenderer.sprite == eagle.dead) return true;

				return false;
			}
		}
	}
}