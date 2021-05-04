using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;


namespace BattleCity
{
	public enum Direction
	{
		Left, Right, Up, Down
	}



	public static class Util
	{
		#region Global Dict
		private static readonly Dictionary<string, object> dict = new Dictionary<string, object>();

		public static bool TryGetValue<TValue>(this string key, out TValue value)
		{
			bool hasValue = dict.TryGetValue(key, out object v);
			value = hasValue ? (TValue)v : default;
			return hasValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue GetValue<TValue>(this string key) => (TValue)dict[key];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContainsKey(this string key) => dict.ContainsKey(key);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove(this string key) => dict.Remove(key);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetValue(this string key, object value) => dict[key] = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ClearAllKeys() => dict.Clear();
		#endregion


		#region Converts
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3Int ToVector3Int(this in Vector2Int value) => new Vector3Int(value.x, value.y, 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2Int ToVector2Int(this in Vector3Int value) => new Vector2Int(value.x, value.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ToVector2(this in Vector3Int value) => new Vector2(value.x, value.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToVector3(this in Vector2Int value) => new Vector3(value.x, value.y);


#if !DEBUG
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Vector2Int ToVector2Int(this in Vector3 value) =>
#if DEBUG
				value.x < 0 || value.y < 0 ? throw new IndexOutOfRangeException($"value= {value} phải là tọa độ không âm !") :
#endif
			new Vector2Int((int)value.x, (int)value.y);

		/// <summary>
		/// z = 0
		/// </summary>
#if !DEBUG
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Vector3Int ToVector3Int(this in Vector3 value) =>
#if DEBUG
				value.x < 0 || value.y < 0 ? throw new IndexOutOfRangeException($"value= {value} phải là tọa độ không âm !") :
#endif
new Vector3Int((int)value.x, (int)value.y, 0);


		#endregion


		#region UniTask
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool isRunning(this in UniTask task) => task.Status == UniTaskStatus.Pending;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool isRunning<T>(this in UniTask<T> task) => task.Status == UniTaskStatus.Pending;


		/// <summary>
		/// Bắt <see cref="Exception"/> ngoại trừ <see cref="OperationCanceledException"/><br/>
		/// Không <see langword="await"/> <paramref name="task"/>, đảm bảo luôn kiểm tra được status của <paramref name="task"/>
		/// </summary>
		public static void Forget(this in UniTask task)
		{
			tasks.Add(task);
			if (tasks.Count == 1) Forget();
		}
		private static readonly List<UniTask> tasks = new List<UniTask>(), tmp = new List<UniTask>();
		private static async void Forget()
		{
			while (true)
			{
				tmp.Clear();
				foreach (var task in tasks)
					if (!task.isRunning())
						if (task.Status == UniTaskStatus.Faulted) await task;
						else tmp.Add(task);

				foreach (var task in tmp) tasks.Remove(task);
				if (tasks.Count == 0) break;
				await UniTask.Yield();
			}
		}


		/// <summary>
		/// Bắt <see cref="Exception"/> ngoại trừ <see cref="OperationCanceledException"/><br/>
		/// Không <see langword="await"/> <paramref name="task"/>, đảm bảo luôn kiểm tra được status của <paramref name="task"/>
		/// </summary>
		public static void Forget<T>(this in UniTask<T> task)
		{
			GenericTasks<T>.tasks.Add(task);
			if (GenericTasks<T>.tasks.Count == 1) GenericTasks<T>.Forget();
		}


		private static class GenericTasks<T>
		{
			public static readonly List<UniTask<T>> tasks = new List<UniTask<T>>();
			private static readonly List<UniTask<T>> tmp = new List<UniTask<T>>();


			public static async void Forget()
			{
				while (true)
				{
					tmp.Clear();
					foreach (var task in tasks)
						if (!task.isRunning())
							if (task.Status == UniTaskStatus.Faulted) await task;
							else tmp.Add(task);

					foreach (var task in tmp) tasks.Remove(task);
					if (tasks.Count == 0) break;
					await UniTask.Yield();
				}
			}
		}
		#endregion


		#region Move
		/// <summary>
		/// Có <see cref="Transform"/> đang di chuyển ?
		/// </summary>
		public static bool hasMovingTransform => moveCount != 0;
		private static int moveCount;

		/// <summary>
		/// <para>Chú ý: KHÔNG đảm bảo luôn luôn <see langword="async"/> !</para>
		/// Nếu trước khi gọi mà <c>dest == transform.position</c> hoặc <c>token.IsCancellationRequested == <see langword="true"/></c> thì sẽ thoát ngay lập tức !
		/// </summary>
		public static async UniTask Move(this Transform transform, Vector3 dest, float speed, CancellationToken token = default)
		{
			if (token.IsCancellationRequested) return;
#if DEBUG
			if (transform.position.z != 0 || dest.z != 0) throw new Exception("Position.Z phải = 0 !");
#endif
			++moveCount;
			while (!token.IsCancellationRequested && transform.position != dest)
			{
				transform.position = Vector3.MoveTowards(transform.position, dest, speed);
				await UniTask.Yield();
#if UNITY_EDITOR
				try
				{
					// Kiểm tra xem có bị Destroy chưa ?
					var _ = transform.gameObject;
				}
				catch { --moveCount; return; }
#endif
			}

			if (!token.IsCancellationRequested) transform.position = dest;
			--moveCount;
		}
		#endregion
	}
}