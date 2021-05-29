using BattleCity;
using BattleCity.Items;
using BattleCity.Platforms;
using UnityEngine;


public class Test : MonoBehaviour
{
	private void Awake()
	{
		"Shovel".Instantiate<Item>();
	}
}