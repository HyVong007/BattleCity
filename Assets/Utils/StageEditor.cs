using System;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace BattleCity.Utils
{
	[DefaultExecutionOrder(-2)] //  Test
	public sealed class StageEditor : MonoBehaviour
	{
		[SerializeField] private Tilemap tilemap;
		[SerializeField] private Vector2Int size;
		private void Awake()
		{
			tilemap.CompressBounds();
			if (size.x >= tilemap.size.x && size.y >= tilemap.size.y) tilemap.size = (Vector3Int)size;
			else if (size.x > 0 || size.y > 0) throw new Exception($"size= {size} không hợp lệ ! Gán size = 0, 0 nếu tự động cài size");



			// Test
			Export();
			Destroy(gameObject);
		}


		private void Export()
		{
			var size = tilemap.size;
			var platforms = new int[size.x][];
			var index = new Vector3Int();
			int x, y;
			var origin = tilemap.origin;
			for (x = 0, index.x = origin.x; x < size.x; ++x, ++index.x)
			{
				platforms[x] = new int[size.y];
				for (y = 0, index.y = origin.y; y < size.y; ++y, ++index.y)
				{
					var tile = tilemap.GetTile(index);
					platforms[x][y] = tile ? Convert.ToInt32(tile.name) : -1;
				}
			}
			"STAGE".SetValue(new Stage(platforms, null, null));
		}
	}
}