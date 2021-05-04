using UnityEngine;


namespace BattleCity.Tanks
{
	public abstract class Tank : MonoBehaviour, IBulletCollision
	{
		public abstract bool OnCollision(Bullet bullet);
	}



	public interface ITankCollision
	{
		void OnCollision(Tank tank);
	}
}