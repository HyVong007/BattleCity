using BattleCity.LevelEditors;
using BattleCity.Platforms;
using BattleCity.Tanks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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


		public Vector3 bulletPos1, bulletDir1, bulletPos2, bulletDir2;
		private async void Start()
		{
			while (true)
			{
				Bullet.New(bulletPos1, bulletDir1, null, new List<Bullet>(), false, false);
				Bullet.New(bulletPos2, bulletDir2, null, new List<Bullet>(), false, false);
				await UniTask.Delay(1000);
			}
		}
	}
}