using BattleCity.LevelEditors;
using BattleCity.Platforms;
using System;
using UnityEngine;


namespace BattleCity
{
	public sealed class BattleField : MonoBehaviour
	{
		public static BattleField instance { get; private set; }
		public static Level level;

		public static event Action onAwake;

		// Test
		public LevelEditor editor;

		private void Awake()
		{
			instance = instance ? throw new Exception() : this;
			string data = editor.CreateLevelFile();
			Destroy(editor.gameObject);
			level = new Level(data);
			Platform.LoadLevel(level);
			Camera.main.transform.position = new Vector3(level.platforms.Length / 2f,
				level.platforms[0].Length / 2f, -1f);

			Camera.main.orthographicSize = level.platforms[0].Length / 2f;
			onAwake();
		}
	}
}