using BattleCity.Tanks;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity
{
	[Serializable]
	public struct Stage
	{
		/// <summary>
		/// Phải đạt kích thước tối thiểu
		/// </summary>
		public Vector2Int size;
		/// <summary>
		/// Tồn tại tất cả <see cref="Tank.Color.Yellow"/>, <see cref="Tank.Color.Green"/>, <see cref="Tank.Color.White"/><br/>
		/// Không nằm trong <see cref="enemySpawnIndexes"/> và không bao quanh <see cref="Eagle"/>
		/// </summary>
		public IReadOnlyDictionary<Tank.Color, Vector2Int> playerSpawnIndexes;
		/// <summary>
		/// Tồn tại ít nhất 1 index<br/>
		/// Không nằm trong <see cref="playerSpawnIndexes"/> và không bao quanh <see cref="Eagle"/>
		/// </summary>
		public ReadOnlyArray<Vector2Int> enemySpawnIndexes;

		// enemy stats
	}
}