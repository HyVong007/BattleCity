using BattleCity;
using BattleCity.LevelEditors;
using BattleCity.Platforms;
using UnityEngine;


public class Test : MonoBehaviour
{
	[SerializeField] private LevelEditor editor;
	public Vector3 v;
	private void Start()
	{
		char x = 'A', y = 'B';
		print((char)(x +y));
		switch(x)
		{
			case (char)('A' + 'B'): return;
		}
	}
}
