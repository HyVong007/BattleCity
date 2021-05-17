using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;


namespace BattleCity
{
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


		#region Convert
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


		#region Convert Direction
		private static readonly IReadOnlyDictionary<Direction, Vector3> DIRECTION_VECTOR3 = new Dictionary<Direction, Vector3>
		{
			[Direction.Up] = Vector3.up,
			[Direction.Right] = Vector3.right,
			[Direction.Down] = Vector3.down,
			[Direction.Left] = Vector3.left
		};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToUnitVector3(this Direction direction) => DIRECTION_VECTOR3[direction];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToUnitVector3(this Direction? direction) => DIRECTION_VECTOR3[direction.Value];
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


		#region Scene
		/// <summary>
		/// <paramref name="relativePath"/> ví dụ: "A/B/C. Không cần "Assets" và ".unity"
		/// </summary>
		public static async UniTask LoadScene(this string relativePath, bool additive = false)
		{
			await SceneManager.LoadSceneAsync(relativePath, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			await UniTask.Yield();
			var scene = SceneManager.GetSceneByPath($"Assets/{relativePath}.unity");
			if (scene.IsValid()) SceneManager.SetActiveScene(scene);
		}


		/// <summary>
		/// <paramref name="relativePath"/> ví dụ: "A/B/C. Không cần "Assets" và ".unity"
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask UnloadScene(this string relativePath)
			=> await SceneManager.UnloadSceneAsync(relativePath);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask ReloadScene(this string relativePath, bool additive = false)
		{
			await UnloadScene(relativePath);
			await LoadScene(relativePath, additive);
		}


		public static async UniTask LoadScene(this int sceneBuildIndex, bool additive = false)
		{
			await SceneManager.LoadSceneAsync(sceneBuildIndex, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			await UniTask.Yield();
			var scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
			if (scene.IsValid()) SceneManager.SetActiveScene(scene);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask UnloadScene(this int sceneBuildIndex)
			=> await SceneManager.UnloadSceneAsync(sceneBuildIndex);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask ReloadScene(this int sceneBuildIndex, bool additive = false)
		{
			await UnloadScene(sceneBuildIndex);
			await LoadScene(sceneBuildIndex, additive);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async UniTask ReloadActiveScene(bool additive = false)
			=> await ReloadScene(SceneManager.GetActiveScene().buildIndex, additive);
		#endregion


		#region Json
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object FromJson(this string json, Type type) => JsonConvert.DeserializeObject(json, type);
		#endregion


		#region Addressable asset

		// Addressable đang có bug: https://fogbugz.unity3d.com/default.asp?1334039_anm5dn0bo5rg7mdr

		// Fix tạm thời: prefab pos =(-1,-1,-1) và khi Instantiate cài pos=(0,0,0). Awake kiểm tra pos


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GameObject Instantiate(this string assetAddress, Vector3 position, Quaternion rotation)
			=> Addressables.InstantiateAsync(assetAddress, position, rotation).WaitForCompletion();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GameObject Instantiate(this string assetAddress, Transform parent = null)
			=> Addressables.InstantiateAsync(assetAddress, Vector3.zero, Quaternion.identity, parent).WaitForCompletion();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Instantiate<T>(this string assetAddress, Vector3 position, Quaternion rotation) where T : Component
			=> Addressables.InstantiateAsync(assetAddress, position, rotation).WaitForCompletion().GetComponent<T>();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Instantiate<T>(this string assetAddress, Transform parent = null) where T : Component
			=> Addressables.InstantiateAsync(assetAddress, Vector3.zero, Quaternion.identity, parent).WaitForCompletion().GetComponent<T>();


		// Bug: chỉ load được ScriptableObject (đã test)
		// Error nếu load Mono


		/// <summary>
		///  Bug: chỉ load được ScriptableObject (đã test)<br/>
		///  Error nếu load Mono
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Object Load(this string assetAddress)
			=> Addressables.LoadAssetAsync<UnityEngine.Object>(assetAddress).WaitForCompletion();


		/// <summary>
		///  Bug: chỉ load được ScriptableObject (đã test)<br/>
		///  Error nếu load Mono
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Load<T>(this string assetAddress) where T : UnityEngine.Object
			=> Addressables.LoadAssetAsync<T>(assetAddress).WaitForCompletion();
		#endregion


		private static ushort customTypeCode;
		private static readonly object lock_CustomTypeCode = new object();
		public static byte NextCustomTypeCode()
		{
			byte result;
			lock (lock_CustomTypeCode)
			{
				if (customTypeCode > byte.MaxValue) throw new InvalidOperationException("Không thể đăng ký thêm Custom type !");
				result = (byte)customTypeCode;
				while (++customTypeCode <= byte.MaxValue &&
					(customTypeCode == 'W' || customTypeCode == 'V' || customTypeCode == 'Q' || customTypeCode == 'P')) ;
			}

			return result;
		}


		/// <summary>
		/// Di chuyển một bước 0.5f theo <paramref name="direction"/><br/>
		/// Chú ý: Không bao giờ throw <see cref="OperationCanceledException"/>, nên kiểm tra <paramref name="token"/> độc lập !
		/// </summary>
		public static async UniTask Move(this Transform transform, Direction direction, float speed, int delay, CancellationToken token = default)
		{
			var v = direction.ToUnitVector3();
			var pos = transform.position;
			var stop = pos + v * 0.5f;
			v *= speed;

			for (float i = 0.5f / speed; i > 0; --i)
			{
				transform.position = pos += v;
				await UniTask.Delay(delay, delayTiming: PlayerLoopTiming.LastUpdate);
				if (token.IsCancellationRequested) return;
			}

			if (!token.IsCancellationRequested) transform.position = stop;
		}
	}



	public enum Direction
	{
		Up = 0, Right = 1, Down = 2, Left = 3
	}



	[Serializable]
	public sealed class ObjectPool<T> : IEnumerable<T> where T : Component
	{
		[SerializeField] private T prefab;
		[SerializeField] private Transform usingAnchor, freeAnchor;
		[SerializeField] private List<T> free = new List<T>();
		private readonly List<T> @using = new List<T>();


		private ObjectPool() { }


		public ObjectPool(T prefab, Transform freeAnchor = null, Transform usingAnchor = null)
		{
			this.prefab = prefab;
			this.freeAnchor = freeAnchor;
			this.usingAnchor = usingAnchor;
		}


		public T Get(Vector3 position = default, bool active = true)
		{
			T item;
			if (free.Count != 0)
			{
				(item = free[0]).transform.position = position;
				item.transform.SetParent(usingAnchor, false);
				free.RemoveAt(0);
			}
			else item = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity, usingAnchor);

			@using.Add(item);
			item.gameObject.SetActive(active);
			return item;
		}


		public void Recycle(T item)
		{
			item.gameObject.SetActive(false);
			item.transform.SetParent(freeAnchor, false);
			@using.Remove(item);
			free.Add(item);
		}


		public void Recycle()
		{
			for (int i = 0; i < @using.Count; ++i)
			{
				var item = @using[i];
				item.gameObject.SetActive(false);
				item.transform.SetParent(freeAnchor, false);
				free.Add(item);
			}
			@using.Clear();
		}


		public void DestroyItem(T item)
		{
			@using.Remove(item);
			UnityEngine.Object.Destroy(item.gameObject);
		}


		public void DestroyItem()
		{
			foreach (var item in @using) UnityEngine.Object.Destroy(item.gameObject);
			@using.Clear();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => @using.GetEnumerator();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => @using.GetEnumerator();
	}



	public readonly struct ReadOnlyArray<T> : IEnumerable<T>
	{
		private readonly T[] array;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlyArray(T[] array) => this.array = array;

		public T this[int index] => array[index];

		public int Length => array.Length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => (array as IEnumerable<T>).GetEnumerator();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => array.GetEnumerator();
	}
}