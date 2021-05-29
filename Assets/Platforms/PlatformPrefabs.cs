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
		public SerializableDictionaryBase<Vector2, Brick> bricks;
		public SerializableDictionaryBase<Vector2, Steel> steels;
		public SerializableDictionaryBase<Vector2, Forest> forests;


		public static PlatformPrefabs instance { get; private set; }
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => instance = "Platform Prefabs".LoadAsset<PlatformPrefabs>();
	}
}