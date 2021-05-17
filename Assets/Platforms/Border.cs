using BattleCity.Tanks;


namespace BattleCity.Platforms
{
	public sealed class Border : Platform
	{
		public override bool IsBlockingTankMove() => true;
		public override bool OnCollision(Bullet bullet) => true;
	}
}