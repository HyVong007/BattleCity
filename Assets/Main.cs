using BattleCity.LevelEditors;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace BattleCity
{
	public sealed class Main : MonoBehaviour
	{
		private static Main instance;

		public LevelEditor editor; // Test

		private async void Awake()
		{
			instance = instance ? throw new Exception() : this;
			mousePointer.position = mousePositions[mouseIndex];
			level = new(editor.CreateLevelFile()); // Test
			Destroy(editor.gameObject);


			// Test
			await UniTask.Delay(3000);
			mouseIndex = 1;
			SceneManager.LoadScene("Battle Field");
		}


		[SerializeField] private Transform mousePointer;
		[SerializeField] private Vector3[] mousePositions;
		private static int ΔmouseIndex;
		public static int mouseIndex
		{
			get => ΔmouseIndex;

			private set => instance.mousePointer.position =
				instance.mousePositions[ΔmouseIndex = (value < 0
				? instance.mousePositions.Length - 1
				: value >= instance.mousePositions.Length ? 0 : value)];
		}


		public static Level level { get; private set; }

		public static void NextLevel()
		{
			// Test
		}


		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.UpArrow)) --mouseIndex;
			else if (Input.GetKeyDown(KeyCode.DownArrow)) ++mouseIndex;

			if (Input.GetKeyDown(KeyCode.Return))
			{
				switch (mouseIndex)
				{
					case 0:
					case 1:
						SceneManager.LoadScene("Battle Field");
						break;

					case 2:
						//SceneManager.LoadScene("Battle Field");
						break;

					case 3:
						//SceneManager.LoadScene("Setting");
						break;

					case 4:
						//SceneManager.LoadScene("Level Editor");
						break;
				}
			}
		}


		public const int ONE_PLAYER = 0, TWO_PLAYER = 1, PLAYER_WITH_AI = 2,
			SETTING = 3, LEVEL_EDITOR = 4;
	}
}