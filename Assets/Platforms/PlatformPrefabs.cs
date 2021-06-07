using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BattleCity.Platforms
{
	[CreateAssetMenu(menuName = "Platform/Prefabs", fileName = "New Platform Prefab")]
	public sealed class PlatformPrefabs : ScriptableObject
	{
		public Border border;
		public Eagle eagle;
		public Sand sand;
		public Water water;
		public Brick fullBrick;
		public Steel fullSteel;
		public Forest fullForest;
		[Tooltip("Direction cùng chiều kim đồng hồ, bắt đầu từ 0h")]
		public SerializableDictionaryBase<Vector2, Brick> bricks;
		[Tooltip("Direction cùng chiều kim đồng hồ, bắt đầu từ 0h")]
		public SerializableDictionaryBase<Vector2, Steel> steels;
		[Tooltip("Direction cùng chiều kim đồng hồ, bắt đầu từ 0h")]
		public SerializableDictionaryBase<Vector2, Forest> forests;


		public static PlatformPrefabs instance { get; private set; }
		public static ReadOnlyArray<Platform> allPrefabs { get; private set; }
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			instance = "Platform Prefabs".LoadAsset<PlatformPrefabs>();
			var tmp = new Platform[31];
			int i = 0;
			tmp[i++] = instance.eagle;
			tmp[i++] = instance.sand;
			tmp[i++] = instance.water;
			tmp[i++] = instance.fullBrick;
			tmp[i++] = instance.fullSteel;
			tmp[i++] = instance.fullForest;
			foreach (var brick in instance.bricks.Values) tmp[i++] = brick;
			foreach (var steel in instance.steels.Values) tmp[i++] = steel;
			allPrefabs = new ReadOnlyArray<Platform>(tmp);
		}
	}
}