using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace BattleCity.LevelEditors
{
	public sealed class LevelEditor : MonoBehaviour
	{
		[SerializeField] private Tilemap tilemap;
		[SerializeField] private SerializableDictionaryBase<Color, Vector2Int> playerIndexes;
		[SerializeField] private Vector2Int[] enemyIndexes;


		public string CreateLevelFile()
		{
			var sb = new StringBuilder();
			using var writer = new StringWriter(sb);

			foreach (var color_pos in playerIndexes)
				writer.Write($"{(int)color_pos.Key} {color_pos.Value.x} {color_pos.Value.y} ");
			sb.Remove(sb.Length - 1, 1);
			writer.WriteLine();

			foreach (var pos in enemyIndexes) writer.Write($"{pos.x} {pos.y} ");
			sb.Remove(sb.Length - 1, 1);
			writer.WriteLine();

			#region Write platforms
			tilemap.CompressBounds();
			var min = tilemap.cellBounds.min;
			var max = tilemap.cellBounds.max;
			var index = new Vector3Int();
			for (index.x = min.x; index.x < max.x; ++index.x)
			{
				for (index.y = min.y; index.y < max.y; ++index.y)
				{
					var tile = tilemap.GetTile(index);
					writer.Write($"{(tile ? Convert.ToInt32(tile.name) : 0)} ");
				}

				sb.Remove(sb.Length - 1, 1);
				writer.WriteLine();
			}
			#endregion

			return sb.ToString();
		}
	}
}