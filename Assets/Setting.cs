using BattleCity.Tanks;
using System;


namespace BattleCity
{
	[Serializable]
	public struct Setting
	{
		public Tank.Color[] humanPlayerColors, aiPlayerColors;
	}
}