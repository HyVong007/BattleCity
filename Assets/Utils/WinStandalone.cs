using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;


namespace BattleCity.Utils
{
	internal static class WinStandalone
	{
		public static void Maximize()
		{
#if UNITY_EDITOR || !UNITY_STANDALONE_WIN
			return;
#endif
			Task.Run(() =>
			{
				var wf = new WindowFinder();
				wf.FindWindows(0, null, new Regex(APP_NAME), new Regex(APP_NAME), new WindowFinder.FoundWindowCallback(MaximizeWindow));
			});
		}


		private static string APP_NAME;
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#endif
		private static void Init()
		{
			APP_NAME = Application.productName;
			Task.Run(Maximize);
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


		private static bool MaximizeWindow(int handle)
		{
			ShowWindowAsync(handle, (int)SW.SHOWMAXIMIZED);
			return true;
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
			private static extern Boolean EnumChildWindows(int hWndParent, PChildCallBack lpEnumFunc, int lParam);
			#endregion

			private delegate bool PChildCallBack(int hWnd, int lParam);

			private event FoundWindowCallback foundWindow;
			public delegate bool FoundWindowCallback(int hWnd);

			private int parentHandle;
			private Regex className;
			private Regex windowText;
			private Regex process;


			public void FindWindows(int parentHandle, Regex className, Regex windowText, Regex process, FoundWindowCallback fwc)
			{
				this.parentHandle = parentHandle;
				this.className = className;
				this.windowText = windowText;
				this.process = process;

				foundWindow = fwc;

				EnumChildWindows(parentHandle, new PChildCallBack(EnumChildWindowsCallback), 0);
			}

			private bool EnumChildWindowsCallback(int handle, int lParam)
			{
				if (className != null)
				{
					StringBuilder sbClass = new StringBuilder(256);
					GetClassName(handle, sbClass, sbClass.Capacity);

					if (!className.IsMatch(sbClass.ToString()))
						return true;
				}

				if (windowText != null)
				{
					int txtLength = SendMessage(handle, WM_GETTEXTLENGTH, 0, 0);
					StringBuilder sbText = new StringBuilder(txtLength + 1);
					SendMessage(handle, WM_GETTEXT, sbText.Capacity, sbText);

					if (!windowText.IsMatch(sbText.ToString()))
						return true;
				}

				if (process != null)
				{
					int processID;
					GetWindowThreadProcessId(handle, out processID);

					System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(processID);

					if (!process.IsMatch(p.ProcessName))
						return true;
				}

				return foundWindow(handle);
			}
		}
	}



	internal static class MinimumWindowSize
	{
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#endif
		private static void Init()
		{
			Set(1280, 720);
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

			// Get reference
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