using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public class Test : MonoBehaviour
{
	public Vector3 v;

	private async void Move()
	{
		var pos = transform.position;
		while (true)
		{
			transform.position += v;

			await UniTask.Yield();
		}
	}


	private void Update()
	{
		if (Keyboard.current.spaceKey.wasPressedThisFrame) Move();
	}
}


