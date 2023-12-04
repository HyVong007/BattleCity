using BattleCity.Tanks;
using System;
using UnityEngine;


namespace BattleCity.Platforms
{
	public abstract class Platform : MonoBehaviour, IBulletCollision
	{
		public static ReadOnlyArray<ReadOnlyArray<Platform>> platforms { get; private set; }


		public abstract bool OnBulletCollision(Bullet bullet);



		[Serializable]
		protected sealed class Particle
		{
			[Serializable]
			private sealed class Y
			{
				public GameObject[] objs;
			}

			[SerializeField] private Y[] X;
		}
	}
}