using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BattleCity.AI
{
	public sealed class EnemyAgent : Agent
	{
		public static async UniTask<EnemyTank> SpawnTank()
		{
			return await EnemyTank.Spawn(EnemyTank.Type.Big, Tank.Color.Red, new Vector3(2.5f, 2.5f));
		}


		private void FixedUpdate()
		{
			foreach (var enemy in EnemyTank.livingTanks)
			{
				CheckShoot(enemy);
				if (!IsTankMoving(enemy)) CheckMove(enemy);
			}
		}
	}
}