using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Items
{
	[DisallowMultipleComponent]
	public abstract class Item : MonoBehaviour, ITankCollision
	{
		public static Item current { get; protected set; }
		public static Item RandomSpawn()
		{
			// Kiểm tra va chạm tank

			return null;
		}


		protected void OnDisable() => current = this == current ? null : current;


		public abstract void OnCollision(Tank tank);


		public enum Name
		{
			Clock,
			Grenade,
			Gun,
			Helmet,
			Life,
			Ship,
			Shovel,
			Star
		}
	}
}