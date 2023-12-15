using BattleCity.LevelEditors;
using UnityEngine;


public class Test : MonoBehaviour
{
	public LevelEditor editor;

	private void Start()
	{
		Destroy(gameObject);
	}


	private void OnDisable()
	{
		print("disable");
	}
}
