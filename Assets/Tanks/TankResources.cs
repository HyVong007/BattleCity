using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;


namespace BattleCity.Tanks
{
	[CreateAssetMenu(fileName = "New Tank Resources", menuName = "Tank/Resources")]
	public sealed class TankResources : ScriptableObject
	{
		public Color_Direction_Sprite gunSprites;
		public Color_Direction_Anim gunAnims;
	}


	[Serializable] public sealed class Direction_Sprite : SerializableDictionaryBase<Direction, Sprite> { }
	[Serializable] public sealed class Color_Direction_Sprite : SerializableDictionaryBase<Tank.Color, Direction_Sprite> { }
	[Serializable] public sealed class Direction_Anim : SerializableDictionaryBase<Direction, RuntimeAnimatorController> { }
	[Serializable] public sealed class Color_Direction_Anim : SerializableDictionaryBase<Tank.Color, Direction_Anim> { }
}