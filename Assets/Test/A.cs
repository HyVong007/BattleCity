using UnityEngine;


public class A : MonoBehaviour
{
	private void Awake()
	{
		print("awake");
	}


	private void OnEnable()
	{
		print("enable");
	}


	private void OnDisable()
	{
		print("disable");
	}
}