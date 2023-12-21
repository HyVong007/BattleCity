using BattleCity.LevelEditors;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class Test : MonoBehaviour
{
	private void Start()
	{
		var hehe = new Vector3();
		hehe.X();
		print(hehe);
	}




}


public static class A
{
	public static void X(this ref Vector3 v) => v = new(456, 789);
}
