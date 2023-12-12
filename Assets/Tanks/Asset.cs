using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BattleCity.Tanks
{
	[CreateAssetMenu(fileName = "Asset", menuName = "Tank/Asset")]
	public sealed class Asset : ScriptableObject
	{
		public SerializableDictionaryBase<Color, SerializableDictionaryBase<Vector3, Sprite>> gunSprites;
		public SerializableDictionaryBase<Color, SerializableDictionaryBase<Vector3, RuntimeAnimatorController>> gunAnims;
	}
}