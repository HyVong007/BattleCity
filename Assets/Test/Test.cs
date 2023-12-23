using BattleCity.Tanks;
using UnityEngine;


public class Test : MonoBehaviour
{
	private async void Awake()
	{
		var tank = GetComponent<Tank>();

		for (int i = 0; i < 20; ++i) 
			await tank.Move(Vector3.up);
	}


}


public static class A
{
	public static void X(this ref Vector3 v) => v = new(456, 789);
}
