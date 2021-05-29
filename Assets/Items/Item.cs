using BattleCity.Tanks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BattleCity.Items
{
	[DisallowMultipleComponent]
	public abstract class Item : MonoBehaviour, ITankCollision
	{
		public static Item current { get; protected set; }
		private static readonly List<Tank> tmp = new List<Tank>();
		public static Item RandomSpawn()
		{
			if (current) Destroy(current.gameObject);

			// Test
			//current = ALL_NAMES[UnityEngine.Random.Range(0, ALL_NAMES.Length)].ToString().Instantiate<Item>(new Vector3(6, 6), Quaternion.identity);

			current = "Shovel".Instantiate<Item>(new Vector3(2.5f, 2.5f), Quaternion.identity);

			current.MIN = current.transform.position;
			current.MIN.x -= 0.5f; current.MIN.y -= 0.5f;
			current.MAX.x = current.MIN.x + 1; current.MAX.y = current.MIN.y + 1;

			#region Kiểm tra va chạm Tank
			var p = (current.transform.position * 2).ToVector2Int();
			tmp.Clear();
			for (int x = p.x - 1, cx = 0; cx < 3; ++cx, ++x)
				for (int y = p.y - 1, cy = 0; cy < 3; ++cy, ++y)
				{
					var list = Tank.array[x][y];
					if (list.Count == 0) continue;
					if (Setting.enemy_CanCollise_Item) tmp.AddRange(list);
					else foreach (var t in list) if (t is PlayerTank) tmp.Add(t);
				}

			if (tmp.Count == 0) return current;
			var tank = tmp[UnityEngine.Random.Range(0, tmp.Count)];
			current.OnCollision(tank);
			#endregion

			return current;
		}


		private Vector2 MIN, MAX;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsCollidedTank(in Vector3 tankPosition)
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
		private static readonly ReadOnlyArray<Name> ALL_NAMES = new ReadOnlyArray<Name>(Enum.GetValues(typeof(Name)) as Name[]);
	}
}