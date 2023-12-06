using UnityEngine;


namespace BattleCity
{
	public sealed class BattleField : MonoBehaviour
	{
		public static BattleField instance { get; private set; }
		public static Level level;
	}
}