using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BattleCity.AI
{
	[DisallowMultipleComponent]
	public abstract class Agent : MonoBehaviour
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool IsTankMoving(Tank tank) => moveTasks.ContainsKey(tank) && moveTasks[tank].isRunning();


		[SerializeField] private float minDelayRotate, maxDelayRotate;
		private readonly Dictionary<Tank, float> rotateStopTimes = new Dictionary<Tank, float>();
		private readonly Dictionary<Tank, UniTask> moveTasks = new Dictionary<Tank, UniTask>();
		private readonly Dictionary<Tank, Direction> moveDirections = new Dictionary<Tank, Direction>();
		private static readonly Direction[] DIRECTIONS = Enum.GetValues(typeof(Direction)) as Direction[];
		protected void CheckMove(Tank tank)
		{
			if (!rotateStopTimes.ContainsKey(tank)) rotateStopTimes[tank] = Time.time + UnityEngine.Random.Range(minDelayRotate, maxDelayRotate);
			else if (Time.time >= rotateStopTimes[tank])
			{
				rotateStopTimes.Remove(tank);
				moveDirections[tank] = DIRECTIONS[UnityEngine.Random.Range(0, DIRECTIONS.Length)];
			}
			var direction = moveDirections.ContainsKey(tank) ? moveDirections[tank] : tank.direction;
			if (Tank.CanMove(tank.transform.position, direction, tank.ship)) (moveTasks[tank] = tank.Move(direction, 1)).Forget();
			else
			{
				rotateStopTimes.Remove(tank);
				moveDirections[tank] = DIRECTIONS[UnityEngine.Random.Range(0, DIRECTIONS.Length)];
			}
		}


		[SerializeField] private float minDelayShoot, maxDelayShoot;
		private readonly Dictionary<Tank, float> shootStopTimes = new Dictionary<Tank, float>();
		protected void CheckShoot(Tank tank)
		{
			if (!shootStopTimes.ContainsKey(tank)) shootStopTimes[tank] = Time.time + UnityEngine.Random.Range(minDelayShoot, maxDelayShoot);
			else if (Time.time >= shootStopTimes[tank] && tank.canShootBullets > 0)
			{
				shootStopTimes.Remove(tank);
				tank.Shoot().Forget();
			}
		}


		private static readonly Dictionary<Type, Agent> type_agent = new Dictionary<Type, Agent>();
		protected void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			var type = GetType();
			type_agent[type] = type_agent.ContainsKey(type) && type_agent[type] ?
				throw new Exception($"Với mỗi Agent type chỉ tồn tại tối đa 1 Agent. Type= {type}") : this;
		}
	}
}