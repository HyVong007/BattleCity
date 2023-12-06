using RotaryHeart.Lib.SerializableDictionary;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace BattleCity.LevelEditors
{
	public sealed class LevelEditor : MonoBehaviour
	{
		[SerializeField] private Tilemap tilemap;
		[SerializeField] private SerializableDictionaryBase<Color, Vector3> playerPositions;
		[SerializeField] private Vector3[] enemyPositions;


		public Tile tile;
		public string CreateLevelFile()
		{
			using var writer = new StringWriter();

			foreach (var color_pos in playerPositions)
				writer.Write($"{(int)color_pos.Key} {(int)color_pos.Value.x} {(int)color_pos.Value.y} ");
			writer.WriteLine();

			foreach (var pos in enemyPositions) writer.Write($"{(int)pos.x} {(int)pos.y} ");
			writer.WriteLine();

			#region Write platforms
			tilemap.CompressBounds();
			tilemap.origin = default;
			tilemap.size = tilemap.size;
			tilemap.ResizeBounds();

			print(tilemap.GetTile(tilemap.origin));

			return null;
			#endregion

		}
	}
}