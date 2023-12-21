using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace BattleCity
{
	public static class Util
	{
		public static bool Contains<T>(this T[] array, T item)
		{
			for (int i = 0; i < array.Length; ++i) if (array[i].Equals(item)) return true;
			return false;
		}


		#region Converts
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3Int ToVector3Int(this in Vector2Int value) => new(value.x, value.y, 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2Int ToVector2Int(this in Vector3Int value) => new(value.x, value.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ToVector2(this in Vector3Int value) => new(value.x, value.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToVector3(this in Vector2Int value) => new(value.x, value.y);


#if !DEBUG
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Vector2Int ToVector2Int(this in Vector3 value) =>
#if DEBUG
				value.x < 0 || value.y < 0 ? throw new IndexOutOfRangeException($"value= {value} phải là tọa độ không âm !") :
#endif
			new((int)value.x, (int)value.y);

		/// <summary>
		/// z = 0
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3Int ToVector3Int(this in Vector3 value) => new((int)value.x, (int)value.y, 0);
		#endregion


		public static async UniTask Move(this Transform transform, Vector3 dest, float speed, CancellationToken token = default)
		{
			if (token.IsCancellationRequested) return;
			while (!token.IsCancellationRequested && transform && transform.gameObject.activeSelf && transform.position != dest)
			{
				transform.position = Vector3.MoveTowards(transform.position, dest, speed);
				await UniTask.Yield();
			}
			if (!token.IsCancellationRequested && transform) transform.position = dest;
		}


		public static ReadOnlyArray<T> NewReadOnlyArray<T>(int size, out T[] array) => new(array = new T[size]);


		public static ReadOnlyArray<ReadOnlyArray<T>> NewReadOnlyArray<T>(int sizeX, int sizeY, out T[][] array, Func<int, int, T> initialize = null)
		{
			array = new T[sizeX][];
			var baking = new ReadOnlyArray<T>[sizeX];
			for (int x = 0; x < sizeX; ++x)
			{
				array[x] = new T[sizeY];
				baking[x] = new(array[x]);
				if (initialize != null)
					for (int y = 0; y < sizeY; ++y) array[x][y] = initialize(x, y);
			}
			return new(baking);
		}


		public static T[][] NewArray<T>(int sizeX, int sizeY, Func<int, int, T> initialize = null)
		{
			var array = new T[sizeX][];
			for (int x = 0; x < sizeX; ++x)
			{
				array[x] = new T[sizeY];
				if (initialize != null)
					for (int y = 0; y < sizeY; ++y) array[x][y] = initialize(x, y);
			}
			return array;
		}


		public static bool isRunning(this ref UniTask task)
		{
			try { return task.Status == UniTaskStatus.Pending; }
			catch (InvalidOperationException)
			{
				task = UniTask.CompletedTask;
				return false;
			}
		}


		public static bool isRunning<T>(this ref UniTask<T> task)
		{
			try { return task.Status == UniTaskStatus.Pending; }
			catch (InvalidOperationException)
			{
				task = default;
				return false;
			}
		}
	}



	public enum Color
	{
		Green = 0,
		Red = 1,
		White = 2,
		Yellow = 3
	}



	[Serializable]
	public struct ReadOnlyArray<T> : IEnumerable<T>
	{
		[SerializeField] private T[] array;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlyArray(T[] array) => this.array = array;

		public ReadOnlyArray(in ReadOnlyArray<T> wrapper) => array = wrapper.array.Clone() as T[];

		public T this[int index] => array[index];

		public int Length => array.Length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => (array as IEnumerable<T>).GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => array.GetEnumerator();


		public bool Contains(in T item)
		{
			for (int i = 0; i < array.Length; ++i) if (array[i].Equals(item)) return true;
			return false;
		}
	}



	[Serializable]
	public sealed class ObjectPool<T> : IEnumerable<T> where T : Component
	{
		[SerializeField] private T prefab;
		[SerializeField] private Transform visibleAnchor, hiddenAnchor;
		[SerializeField] private List<T> hiddenObj = new();
		private readonly List<T> visibleObj = new();
		public GameObject gameObject { get; private set; }


		private ObjectPool() { }


		public ObjectPool(T prefab)
		{
			this.prefab = prefab;
			gameObject = new() { name = $"{prefab.name} Pool" };
			hiddenAnchor = new GameObject().transform;
			hiddenAnchor.name = "Hidden";
			hiddenAnchor.SetParent(gameObject.transform);
			visibleAnchor = new GameObject().transform;
			visibleAnchor.name = "Visible";
			visibleAnchor.SetParent(gameObject.transform);
		}


		public ObjectPool(T prefab, Transform hiddenAnchor = null, Transform visibleAnchor = null)
		{
			this.prefab = prefab;
			this.hiddenAnchor = hiddenAnchor;
			this.visibleAnchor = visibleAnchor;
		}


		public T Get(Vector3 position = default, bool active = true)
		{
			T item;
			if (hiddenObj.Count != 0)
			{
				item = hiddenObj[0];
				hiddenObj.RemoveAt(0);
			}
			else item = UnityEngine.Object.Instantiate(prefab);

			item.transform.parent = visibleAnchor;
			visibleObj.Add(item);
			item.transform.position = position;
			item.gameObject.SetActive(active);
			return item;
		}


		public void Recycle(T item)
		{
			item.gameObject.SetActive(false);
			item.transform.parent = hiddenAnchor;
			visibleObj.Remove(item);
			hiddenObj.Add(item);
		}


		public void Recycle()
		{
			for (int i = 0; i < visibleObj.Count; ++i)
			{
				var item = visibleObj[i];
				item.gameObject.SetActive(false);
				item.transform.parent = hiddenAnchor;
				hiddenObj.Add(item);
			}
			visibleObj.Clear();
		}


		public void DestroyGameObject(T item)
		{
			visibleObj.Remove(item);
			UnityEngine.Object.Destroy(item.gameObject);
		}


		public void DestroyGameObject()
		{
			foreach (var item in visibleObj) UnityEngine.Object.Destroy(item.gameObject);
			visibleObj.Clear();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => visibleObj.GetEnumerator();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => (visibleObj as IEnumerable<T>).GetEnumerator();
	}



	public static class WinStandalone
	{
		private static int handle;
		public static void Maximize()
		{
#if UNITY_EDITOR || !UNITY_STANDALONE_WIN
			return;
#endif
			if (handle != 0) ShowWindowAsync(handle, (int)SW.SHOWMAXIMIZED);
			else Task.Run(() =>
			{
				var wf = new WindowFinder();
				wf.FindWindows(0, null, new Regex(APP_NAME), new Regex(APP_NAME));
			});
		}


		private static string APP_NAME;
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#endif
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private static void Init()
		{
			APP_NAME = Application.productName;
			Maximize();
		}


		[DllImport("user32.dll")]
		private static extern bool ShowWindowAsync(int hWnd, int nCmdShow);


		private enum SW : int
		{
			HIDE = 0,
			SHOWNORMAL = 1,
			SHOWMINIMIZED = 2,
			SHOWMAXIMIZED = 3,
			SHOWNOACTIVATE = 4,
			SHOW = 5,
			MINIMIZE = 6,
			SHOWMINNOACTIVE = 7,
			SHOWNA = 8,
			RESTORE = 9,
			SHOWDEFAULT = 10
		}


		private class WindowFinder
		{
			const int WM_GETTEXT = 0x000D;
			const int WM_GETTEXTLENGTH = 0x000E;

			#region Win32 functions that have all been used in previous blogs.
			[DllImport("User32.Dll")]
			private static extern void GetClassName(int hWnd, StringBuilder s, int nMaxCount);

			[DllImport("User32.dll")]
			private static extern int GetWindowText(int hWnd, StringBuilder text, int count);

			[DllImport("User32.dll")]
			private static extern Int32 SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);

			[DllImport("User32.dll")]
			private static extern Int32 SendMessage(int hWnd, int Msg, int wParam, int lParam);

			[DllImport("user32")]
			private static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

			/// EnumChildWindows works just like EnumWindows, except we can provide a parameter that specifies the parent
			/// window handle. If this is NULL or zero, it works just like EnumWindows. Otherwise it'll only return windows
			/// whose parent window handle matches the hWndParent parameter.
			[DllImport("user32.Dll")]
			private static extern bool EnumChildWindows(int hWndParent, PChildCallBack lpEnumFunc, int lParam);
			#endregion

			private delegate bool PChildCallBack(int hWnd, int lParam);

			private int parentHandle;
			private Regex className;
			private Regex windowText;
			private Regex process;


			public void FindWindows(int parentHandle, Regex className, Regex windowText, Regex process)
			{
				this.parentHandle = parentHandle;
				this.className = className;
				this.windowText = windowText;
				this.process = process;

				EnumChildWindows(parentHandle, EnumChildWindowsCallback, 0);
			}

			private bool EnumChildWindowsCallback(int handle, int lParam)
			{
				if (className != null)
				{
					var sbClass = new StringBuilder(256);
					GetClassName(handle, sbClass, sbClass.Capacity);

					if (!className.IsMatch(sbClass.ToString())) return true;
				}

				if (windowText != null)
				{
					int txtLength = SendMessage(handle, WM_GETTEXTLENGTH, 0, 0);
					var sbText = new StringBuilder(txtLength + 1);
					SendMessage(handle, WM_GETTEXT, sbText.Capacity, sbText);

					if (!windowText.IsMatch(sbText.ToString())) return true;
				}

				if (process != null)
				{
					GetWindowThreadProcessId(handle, out int processID);
					System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(processID);
					if (!process.IsMatch(p.ProcessName)) return true;
				}

				ShowWindowAsync(WinStandalone.handle = handle, (int)SW.SHOWMAXIMIZED);
				return false;
			}
		}



		public static class MinWindowSize
		{
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#endif
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
			private static void Init()
			{
				Set(800, 600);
				Application.quitting += () => Reset();
			}


			// This code works exclusively with standalone build.
			// Executing GetActiveWindow in unity editor returns editor window.
			private const int DefaultValue = -1;

			// Identifier of MINMAXINFO message
			private const uint WM_GETMINMAXINFO = 0x0024;

			// SetWindowLongPtr argument : Sets a new address for the window procedure.
			private const int GWLP_WNDPROC = -4;

			private static int width;
			private static int height;
			private static bool enabled;

			// Reference to current window
			private static HandleRef hMainWindow;

			// Reference to unity WindowsProcedure handler
			private static IntPtr unityWndProcHandler;

			// Reference to custom WindowsProcedure handler
			private static IntPtr customWndProcHandler;

			// Delegate signature for WindowsProcedure
			private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

			// Instance of delegate
			private static WndProcDelegate procDelegate;

			[StructLayout(LayoutKind.Sequential)]
			private struct Minmaxinfo
			{
				public Point ptReserved;
				public Point ptMaxSize;
				public Point ptMaxPosition;
				public Point ptMinTrackSize;
				public Point ptMaxTrackSize;
			}

			private struct Point
			{
				public int x;
				public int y;
			}


			public static void Set(int minWidth, int minHeight)
			{
#if UNITY_EDITOR || !UNITY_STANDALONE_WIN
				return;
#endif
				if (minWidth < 0 || minHeight < 0) throw new ArgumentOutOfRangeException("Any component of min size cannot be less than 0");

				width = minWidth;
				height = minHeight;

				if (enabled) return;

				// GetList reference
				hMainWindow = new HandleRef(null, GetActiveWindow());
				procDelegate = WndProc;
				// Generate handler
				customWndProcHandler = Marshal.GetFunctionPointerForDelegate(procDelegate);
				// Replace unity mesages handler with custom
				unityWndProcHandler = SetWindowLongPtr(hMainWindow, GWLP_WNDPROC, customWndProcHandler);

				enabled = true;
			}


			public static void Reset()
			{
#if UNITY_EDITOR || !UNITY_STANDALONE_WIN
				return;
#endif
				if (!enabled) return;
				// Replace custom message handler with unity handler
				SetWindowLongPtr(hMainWindow, GWLP_WNDPROC, unityWndProcHandler);
				hMainWindow = new HandleRef(null, IntPtr.Zero);
				unityWndProcHandler = IntPtr.Zero;
				customWndProcHandler = IntPtr.Zero;
				procDelegate = null;

				width = 0;
				height = 0;

				enabled = false;
			}


			private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
			{
				// All messages except WM_GETMINMAXINFO will send to unity handler
				if (msg != WM_GETMINMAXINFO) return CallWindowProc(unityWndProcHandler, hWnd, msg, wParam, lParam);

				// Intercept and change MINMAXINFO message
				var x = (Minmaxinfo)Marshal.PtrToStructure(lParam, typeof(Minmaxinfo));
				x.ptMinTrackSize = new Point { x = width, y = height };
				Marshal.StructureToPtr(x, lParam, false);

				// Send changed message
				return DefWindowProc(hWnd, msg, wParam, lParam);
			}

			[DllImport("user32.dll")]
			private static extern IntPtr GetActiveWindow();

			[DllImport("user32.dll", EntryPoint = "CallWindowProcA")]
			private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint wMsg, IntPtr wParam,
				IntPtr lParam);

			[DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
			private static extern IntPtr DefWindowProc(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

			private static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
			{
				if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
				return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
			}

			[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
			private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

			[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
			private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);
		}
	}
}