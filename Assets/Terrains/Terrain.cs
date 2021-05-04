using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Terrains
{
	public abstract class Terrain : MonoBehaviour, IBulletCollision
	{
		public abstract bool OnCollision(Bullet bullet);
	}
}