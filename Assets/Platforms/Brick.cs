﻿using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Platforms
{
	public sealed class Brick : Platform
	{
		[SerializeField]
		private Particle particle;


		public override bool OnBulletCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}