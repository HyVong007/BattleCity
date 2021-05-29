using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BattleCity.Items
{
	[RequireComponent(typeof(Animator))]
	public sealed class Helmet : Item
	{
		[SerializeField] private Animator animator;
		[SerializeField] private SerializableDictionaryBase<Tank.Color, RuntimeAnimatorController> anims;
		public override void OnCollision(Tank tank)
		{
			if (tank is EnemyTank)
			{
				Life.UpgradeEnemyColorAndHP();
				gameObject.SetActive(false);
				return;
			}

			player = tank as PlayerTank;
			stopTimes[player.color] = Time.time + delaySeconds;
			if (player.helmet)
			{
				gameObject.SetActive(false);
				return;
			}

			Task();
		}


		private static readonly Dictionary<Tank.Color, float> stopTimes = new Dictionary<Tank.Color, float>();
		[SerializeField] private float delaySeconds;
		private PlayerTank player;
		private async void Task()
		{
			current = null;
			player.helmet = this;
			animator.runtimeAnimatorController = anims[player.color];
			transform.SetParent(player.transform);
			transform.localPosition = default;
			await UniTask.WaitWhile(() => enabled && Time.time < stopTimes[player.color]);
			if (enabled) gameObject.SetActive(false);
		}


		private new void OnDisable()
		{
			base.OnDisable();
			if (player) player.helmet = this == player.helmet ? null : player.helmet;
			Destroy(gameObject);
		}
	}
}