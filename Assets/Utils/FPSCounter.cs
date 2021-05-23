using System;
using UnityEngine;


namespace BattleCity.Utils
{
	public sealed class FPSCounter : MonoBehaviour
	{
#if DEBUG
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
		private static void Init()
		{
			DontDestroyOnLoad("FPS Counter".Instantiate());
			Application.quitting += () => quitting = true;
		}


		private static FPSCounter instance;
		private void Awake()
		{
			if (transform.position.x < 0) return; // Fix Addressable bug 1334039
			instance = instance ? throw new Exception() : this;
			name = "FPS Counter";
			transform.position = default;
			guiStyle.alignment = TextAnchor.UpperLeft;
			guiStyle.fontSize = Screen.height * 2 / 100;
		}


		private static bool quitting;
		private void OnDisable()
		{
			if (!quitting) throw new InvalidOperationException("FPS Counter nếu có thì sẽ tồn tại xuyên suốt game, không thể xóa hay ẩn !");
		}


		private static readonly Rect guiRect = new Rect(0, 0, Screen.width, Screen.height * 2 / 100);
		private static readonly GUIStyle guiStyle = new GUIStyle();
		private static float deltaTime;
		[SerializeField] private float LOW_FPS_LIMIT = 50, VERY_LOW_FPS_LIMIT = 40;
		private void OnGUI()
		{
			float fps = 1.0f / deltaTime;
			guiStyle.normal.textColor = fps <= VERY_LOW_FPS_LIMIT ? Color.red : fps <= LOW_FPS_LIMIT ? Color.yellow : Color.green;
			GUI.Label(guiRect, $"{deltaTime * 1000.0f:0.0} ms {fps:0.} FPS", guiStyle);
		}


		private void Update()
		{
			deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		}
	}
}