using BattleCity.Tanks;
using System;


namespace BattleCity.Items
{
	public sealed class Ship : Item
	{
		public override void OnCollision(Tank tank)
		{
			throw new System.NotImplementedException();
		}


		public void ChangeColor(Color color)
		{
			throw new NotImplementedException();
		}
	}
}