using UnityEngine;


public class Test : MonoBehaviour
{
	public A prefab;
	private void Start()
	{
		var anchor = new GameObject().transform;
		anchor.gameObject.SetActive(false);
		var a = Instantiate(prefab, anchor);
		a.gameObject.SetActive(false);
	}
}
