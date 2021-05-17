using BattleCity.Tanks;


namespace BattleCity.Platforms
{
	public sealed class Sand : Platform
	{
		public override bool IsBlockingTankMove()
		{
			throw new System.NotImplementedException();
		}


		public override bool OnCollision(Bullet bullet)
		{
			throw new System.NotImplementedException();
		}
	}
}