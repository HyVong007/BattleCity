using UnityEngine;
using System;
using UnityEngine.SceneManagement;


namespace BattleCity.Utils
{
	public sealed class FPSCounter : MonoBehaviour
	{
		[SerializeField] private float LOW_FPS_LIMIT = 50, VERY_LOW_FPS_LIMIT = 40;

		private static FPSCounter instance;
		private float deltaTime = 0.0f;


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			//DontDestroyOnLoad(Instantiate(Extensions.Load<FPSCounter>()));
			SceneManager.activeSceneChanged += (Scene before, Scene active) => instance.enabled = active.name == "BattleField";
		}


		private void Awake()
		{
			if (instance) throw new Exception();
			instance = this;
			name = "FPS Counter";
			guiStyle.alignment = TextAnchor.UpperLeft;
			guiStyle.fontSize = Screen.height * 2 / 50;
		}


		private readonly Rect guiRect = new Rect(0, 0, Screen.width, Screen.height * 2 / 100);
		private readonly GUIStyle guiStyle = new GUIStyle();

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