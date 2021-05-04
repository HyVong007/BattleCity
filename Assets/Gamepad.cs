
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






	public static class Gamepad
	{
		public enum ButtonState
		{
			PRESS, HOLD, RELEASE
		}


	}
}