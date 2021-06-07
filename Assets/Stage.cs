using BattleCity.Tanks;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity
{
	public sealed class Stage
	{
		/// <summary>
		/// Nếu [x][y] &lt; 0 : ô trống 
		/// </summary>
		public readonly ReadOnlyArray<ReadOnlyArray<int>> platforms;
		public Vector2Int size => new Vector2Int(platforms.Length, platforms[0].Length);
		/// <summary>
		/// Tồn tại tất cả <see cref="Tank.Color.Yellow"/>, <see cref="Tank.Color.Green"/>, <see cref="Tank.Color.White"/><br/>
		/// Không nằm trong <see cref="enemySpawnIndexes"/> và không bao quanh <see cref="Eagle"/>
		/// </summary>
		public readonly IReadOnlyDictionary<Tank.Color, Vector2Int> playerSpawnIndexes;
		/// <summary>
		/// Tồn tại ít nhất 1 index<br/>
		/// Không nằm trong <see cref="playerSpawnIndexes"/> và không bao quanh <see cref="Eagle"/>
		/// </summary>
		public readonly ReadOnlyArray<Vector2Int> enemySpawnIndexes;


		public Stage(int[][] platforms, IReadOnlyDictionary<Tank.Color, Vector2Int> playerSpawnIndexes, Vector2Int[] enemySpawnIndexes)
		{
			if (platforms.Length < 5 || platforms.Length > 253
			|| platforms[0].Length < 3 || platforms[0].Length > 253)
				throw new ArgumentOutOfRangeException("Kích thước platforms không hợp lệ !");

			var tmp = new ReadOnlyArray<int>[platforms.Length];
			for (int i = tmp.Length - 1; i >= 0; --i) tmp[i] = new ReadOnlyArray<int>(platforms[i]);
			this.platforms = new ReadOnlyArray<ReadOnlyArray<int>>(tmp);
			this.playerSpawnIndexes = playerSpawnIndexes;
			this.enemySpawnIndexes = new ReadOnlyArray<Vector2Int>(enemySpawnIndexes);
		}
	}
}