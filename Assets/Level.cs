using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace BattleCity
{
	public readonly struct Level
	{
		public readonly IReadOnlyDictionary<Color, Vector3> playerPositions;
		public readonly ReadOnlyArray<Vector3> enemyPositions;
		public readonly ReadOnlyArray<ReadOnlyArray<int>> platforms;


		private static readonly Stack<int> stack = new();
		public Level(string data)
		{
			using var reader = new StringReader(data);
			string[] words;

			#region playerPositions
			words = reader.ReadLine().Split(' ');
			var dict = new Dictionary<Color, Vector3>();
			playerPositions = dict;
			for (int i = 0; i < words.Length; i += 3)
				dict[(Color)n(i)] = new Vector3(n(i + 1), n(i + 2));
			#endregion

			#region enemyPositions
			words = reader.ReadLine().Split(' ');
			var e = new Vector3[words.Length / 2];
			enemyPositions = new(e);
			for (int x = 0, y = 0; x < words.Length; x += 2)
				e[y++] = new Vector3(n(x), n(x + 1));
			#endregion

			#region platforms
			var line = reader.ReadLine();
			int NUM_Y = (line.Length + 1) / 2;
			int NUM_X = 0;

			// data -> stack
			do
			{
				++NUM_X;
				words = line.Split(' ');
				for (int y = 0; y < NUM_Y; ++y) stack.Push(n(y));
			} while ((line = reader.ReadLine()) != null);

			// stack -> array
			var array = new ReadOnlyArray<int>[NUM_X];
			for (int x = NUM_X - 1; x >= 0; --x)
			{
				var a = new int[NUM_Y];
				array[x] = new(a);
				for (int y = NUM_Y - 1; y >= 0; --y) a[y] = stack.Pop();
			}

			platforms = new(array);
			#endregion

			int n(int i) => Convert.ToInt32(words[i]);
		}
	}
}