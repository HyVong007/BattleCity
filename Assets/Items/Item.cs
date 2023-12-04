using BattleCity.Tanks;
using UnityEngine;


namespace BattleCity.Items
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public abstract class Item : MonoBehaviour
	{
		public static Item current { get; private set; }


		public abstract void OnTankCollision(Tank tank);
	}
}