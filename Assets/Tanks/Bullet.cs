using UnityEngine;


namespace BattleCity.Tanks
{
	public sealed class Bullet : MonoBehaviour, IBulletCollision
	{
		public bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}



	public interface IBulletCollision
	{
		bool OnCollision(Bullet bullet);
	}
}