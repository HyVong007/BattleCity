using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Items
{
	public abstract class Item : MonoBehaviour, ITankCollision
	{
		public abstract void OnCollision(Tank tank);
	}
}