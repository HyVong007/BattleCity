using BattleCity.Tanks;
using System;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Eagle : Platform
	{
		public static Eagle instance { get; private set; }
		private void Awake()
		{
			instance = instance ? throw new Exception() : this;
		}


		public override bool CanMove(Tank tank, Vector3 newDir) => false;


		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private Sprite dead;
		public override bool OnBulletCollision(Bullet bullet)
		{
			throw new NotImplementedException();
		}



	}
}