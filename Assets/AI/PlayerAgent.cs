using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.AI
{
	public sealed class PlayerAgent : Agent
	{
		private static readonly IReadOnlyList<PlayerTank> aiPlayers = new List<PlayerTank>();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			var aiPlayers = PlayerAgent.aiPlayers as List<PlayerTank>;
			BattleField.awake += async () =>
			  {
				  var setting = "SETTING".GetValue<Setting>();
				  if (setting.aiPlayerColors == null || setting.aiPlayerColors.Length == 0) return;

				  var agent = "Player Agent".Instantiate<PlayerAgent>(BattleField.instance.transform);
				  await UniTask.Yield();
				  foreach (var color in setting.aiPlayerColors) aiPlayers.Add(await PlayerTank.Spawn(color, new Vector3(1.5f, 1.5f)));
			  };
		}


		private void FixedUpdate()
		{
			foreach (var player in aiPlayers)
			{
				if (!player.enabled) continue;
				if (!IsTankMoving(player)) CheckMove(player);
				CheckShoot(player);
			}
		}
	}
}