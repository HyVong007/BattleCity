using BattleCity.Tanks;
using System;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Eagle : Platform
	{
		public static Eagle instance { get; private set; }
		private new void Awake()
		{
			instance = instance ? throw new Exception() : this;
			base.Awake();
		}


		public override bool IsBlockingTankMove(in Vector3 tankPosition, Direction tankDirection, bool tankHasShip) => true;


		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private Sprite dead;
		public bool isDead => spriteRenderer.sprite == dead;


		public override bool OnCollision(Bullet bullet)
		{
			if (isDead) return false;
			spriteRenderer.sprite = dead;
			// Eagle explode
			// BattleField.instance.Finish();
			return true;
		}
	}
}