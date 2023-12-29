using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace BattleCity
{
	public sealed class ScoreBoard : MonoBehaviour
	{
		private async void Awake()
		{
			await UniTask.Delay(1000);
			SceneManager.LoadScene(BattleField.count == 0 ? "Main" : "Battle Field");
		}
	}
}