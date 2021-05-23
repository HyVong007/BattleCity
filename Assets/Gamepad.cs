using BattleCity.Tanks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;


namespace BattleCity
{
	public interface IGamepadListener
	{
		void OnDpad(Direction direction, Gamepad.ButtonState state);

		void OnButtonA(Gamepad.ButtonState state);

		void OnButtonB(Gamepad.ButtonState state);

		void OnButtonX(Gamepad.ButtonState state);

		void OnButtonY(Gamepad.ButtonState state);

		void OnButtonStart();

		void OnButtonSelect();

		//Gamepad.LocalState state { get; }
	}



	public sealed class Gamepad : MonoBehaviour
	{
		private static readonly Dictionary<Tank.Color, Gamepad> instances = new Dictionary<Tank.Color, Gamepad>();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Gamepad GetInstance(Tank.Color color) => instances.ContainsKey(color) ? instances[color] : null;


		public void Add(IGamepadListener listener)
		{
			if (listeners.Contains(listener))
				throw new InvalidOperationException($"Không thể thêm listener do Gamepad đang chứa listener. listener= {listener}");
			listeners.Add(listener);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(IGamepadListener listener) => listeners.Remove(listener);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(IGamepadListener listener) => listeners.Contains(listener);


		[SerializeField] private Tank.Color color;
		private void Awake()
			=> DontDestroyOnLoad(instances[color] = instances.ContainsKey(color) ? throw new Exception() : this);


		private readonly List<IGamepadListener> listeners = new List<IGamepadListener>();
		private void Update()
		{
			var k = Keyboard.current;
			var press = k.leftArrowKey.wasPressedThisFrame ? Direction.Left
				: k.rightArrowKey.wasPressedThisFrame ? Direction.Right
				: k.upArrowKey.wasPressedThisFrame ? Direction.Up
				: k.downArrowKey.wasPressedThisFrame ? Direction.Down
				: (Direction?)null;

			var hold = k.leftArrowKey.isPressed ? Direction.Left
				: k.rightArrowKey.isPressed ? Direction.Right
				: k.upArrowKey.isPressed ? Direction.Up
				: k.downArrowKey.isPressed ? Direction.Down
				: (Direction?)null;

			var release = k.leftArrowKey.wasReleasedThisFrame ? Direction.Left
				: k.rightArrowKey.wasReleasedThisFrame ? Direction.Right
				: k.upArrowKey.wasReleasedThisFrame ? Direction.Up
				: k.downArrowKey.wasReleasedThisFrame ? Direction.Down
				: (Direction?)null;


			// Test Yellow Gamepad
			if (press != null) foreach (var listener in listeners) listener.OnDpad(press.Value, Gamepad.ButtonState.Press);
			else if (release != null) foreach (var listener in listeners) listener.OnDpad(release.Value, Gamepad.ButtonState.Release);
			else if (hold != null) foreach (var listener in listeners) listener.OnDpad(hold.Value, Gamepad.ButtonState.Hold);
		}


		public enum ButtonState
		{
			Press, Hold, Release
		}
	}
}