using BattleCity.Tanks;


namespace BattleCity.Items
{
	public sealed class Gun : Item
	{
		public override void OnCollision(Tank tank)
		{
			if (tank is Player) (tank as Player).IncreaseStar(3);
			else (tank as Enemy).weapon = Enemy.Weapon.Gun;

			Destroy(gameObject);
		}
	}
}