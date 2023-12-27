using BattleCity.Tanks;
using UnityEngine;


public class Test : MonoBehaviour
{
	private void Start()
	{
		A<Tank>();
	}


	private void A<T>()
	{
		print(typeof(T) == typeof(Player));
	}
}
