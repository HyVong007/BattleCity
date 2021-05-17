using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class Eagle : Platform
	{
		public override bool IsBlockingTankMove()
		{
			throw new System.NotImplementedException();
		}


		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private Sprite dead;
		public override bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}