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
			if (Contains(listener))
				throw new InvalidOperationException($"Không thể thêm listener do Gamepad đang chứa listener. listener= {listener}");
			commands.Add((add: true, listener));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(IGamepadListener listener) => commands.Add((add: false, listener));


		public bool Contains(IGamepadListener listener)
		{
			if (listeners.Contains(listener)) return true;
			if (commands.Count == 0) return false;
			int count = 0;
			foreach (var command in commands) count += command.listener != listener ? 0 : command.add ? 1 : -1;
			return count > 0;
		}


		[SerializeField] private Tank.Color color;
		private void Awake() => DontDestroyOnLoad(instances[color] = instances.ContainsKey(color) ? throw new Exception() : this);


		private readonly List<IGamepadListener> listeners = new List<IGamepadListener>();
		private void Update()
		{
			if (commands.Count != 0) ModifyListeners();
			var k = Keyboard.current;

			#region Dpad
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
			if (press != null) foreach (var listener in listeners) listener.OnDpad(press.Value, ButtonState.Press);
			else if (release != null) foreach (var listener in listeners) listener.OnDpad(release.Value, ButtonState.Release);
			else if (hold != null) foreach (var listener in listeners) listener.OnDpad(hold.Value, ButtonState.Hold);
			#endregion

			#region Button
			if (k.escapeKey.wasPressedThisFrame)
			{
				if (commands.Count != 0) ModifyListeners();
				foreach (var listener in listeners) listener.OnButtonStart();
			}

			if (commands.Count != 0) ModifyListeners();
			if (k.spaceKey.wasPressedThisFrame) foreach (var listener in listeners) listener.OnButtonA(ButtonState.Press);
			else if (k.spaceKey.wasReleasedThisFrame) foreach (var listener in listeners) listener.OnButtonA(ButtonState.Release);
			else if (k.spaceKey.isPressed) foreach (var listener in listeners) listener.OnButtonA(ButtonState.Hold);


			#endregion
		}


		public enum ButtonState
		{
			Press, Hold, Release
		}


		private readonly List<(bool add, IGamepadListener listener)> commands = new List<(bool add, IGamepadListener listener)>();
		private void ModifyListeners()
		{
			foreach (var (add, listener) in commands) if (add) listeners.Add(listener); else listeners.Remove(listener);
			commands.Clear();
		}
	}
}