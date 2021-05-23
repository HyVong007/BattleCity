using BattleCity.Tanks;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BattleCity.Items
{
	[DisallowMultipleComponent]
	public abstract class Item : MonoBehaviour, ITankCollision
	{
		public static Item current { get; protected set; }
		public static Item RandomSpawn()
		{
			// Cập nhật MIN, MAX

			// Kiểm tra va chạm tank

			return null;
		}


		private static Vector2 MIN, MAX;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCollidedTank(in Vector3 tankPosition)
			=> MIN.x <= tankPosition.x && tankPosition.x <= MAX.x && MIN.y <= tankPosition.y && tankPosition.y <= MAX.y;


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