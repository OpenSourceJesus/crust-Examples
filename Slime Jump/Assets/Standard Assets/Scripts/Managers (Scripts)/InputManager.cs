using System;
using Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SlimeJump
{
	public class InputManager : SingletonMonoBehaviour<InputManager>
	{
		public InputSettings settings;
		public static InputSettings Settings
		{
			get
			{
				return Instance.settings;
			}
		}
		public static bool UsingGamepad
		{
			get
			{
				for (int i = 0; i < Gamepad.all.Count; i ++)
					if (Gamepad.all[i].enabled)
						return true;
				return HasUsableJoystick();
			}
		}
		public static bool UsingMouse
		{
			get
			{
				return Mouse.current != null;
			}
		}
		public static bool UsingKeyboard
		{
			get
			{
				return Keyboard.current != null;
			}
		}
		public static bool UsingPhone
		{
			get
			{
#if UNITY_ANDROID || UNITY_IOS
				return true;
#else
				return false;
#endif
			}
		}
		public static bool LeftClickInput
		{
			get
			{
				return UsingMouse && Mouse.current.leftButton.isPressed;
			}
		}
		public bool _LeftClickInput
		{
			get
			{
				return LeftClickInput;
			}
		}
		public static bool RightClickInput
		{
			get
			{
				return UsingMouse && Mouse.current.rightButton.isPressed;
			}
		}
		public bool _RightClickInput
		{
			get
			{
				return RightClickInput;
			}
		}
		public static Vector2? MousePosition
		{
			get
			{
				if (UsingMouse)
					return Mouse.current.position.ReadValue();
				else
					return null;
			}
		}
		public Vector2? _MousePosition
		{
			get
			{
				return MousePosition;
			}
		}
		public static bool SubmitInput
		{
			get
			{
				if (UsingGamepad)
					return IsXboxButtonPressed(XboxButton.A);
				else
					return UsingKeyboard && Keyboard.current.enterKey.isPressed;
			}
		}
		public bool _SubmitInput
		{
			get
			{
				return SubmitInput;
			}
		}
		public static Vector2 UIMovementInput
		{
			get
			{
				Vector2 output = Vector2.zero;
				if (UsingKeyboard)
				{
					int x = 0;
					if (Keyboard.current.dKey.isPressed)
						x ++;
					if (Keyboard.current.aKey.isPressed)
						x --;
					int y = 0;
					if (Keyboard.current.wKey.isPressed)
						y ++;
					if (Keyboard.current.sKey.isPressed)
						y --;
					output = new Vector2(x, y);
				}
				if (output == Vector2.zero && UsingGamepad)
					output = LeftStickInput;
				return Vector2.ClampMagnitude(output, 1);
			}
		}
		public Vector2 _UIMovementInput
		{
			get
			{
				return UIMovementInput;
			}
		}
		public static float MoveInput
		{
			get
			{
				float output = 0;
				if (UsingKeyboard)
				{
					if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
						output --;
					if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
						output ++;
				}
				if (output == 0 && UsingGamepad)
					return Mathf.Clamp(LeftStickInput.x, -1, 1);
				return output;
			}
		}
		public float _MoveInput
		{
			get
			{
				return MoveInput;
			}
		}
		public static bool JumpInput
		{
			get
			{
				return (UsingKeyboard && (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed))
					|| (UsingGamepad && (IsXboxButtonPressed(XboxButton.A)
						|| IsXboxButtonPressed(XboxButton.B)
						|| IsXboxButtonPressed(XboxButton.LeftShoulder)
						|| IsXboxButtonPressed(XboxButton.LeftStick)));
			}
		}
		public bool _JumpInput
		{
			get
			{
				return JumpInput;
			}
		}
		public static bool LassoInput
		{
			get
			{
				return (UsingMouse && Mouse.current.rightButton.isPressed)
					|| (UsingGamepad && (IsXboxButtonPressed(XboxButton.X)
						|| IsXboxButtonPressed(XboxButton.Y)
						|| IsXboxButtonPressed(XboxButton.LeftTrigger)
						|| IsXboxButtonPressed(XboxButton.RightStick)))
					|| (UsingPhone && Touchscreen.current.primaryTouch.isInProgress);
			}
		}
		public bool _LassoInput
		{
			get
			{
				return LassoInput;
			}
		}
		public static int ChangeLassoLengthInput
		{
			get
			{
				int output = 0;
				if ((UsingKeyboard && (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed))
					|| (UsingGamepad && IsXboxButtonPressed(XboxButton.LeftShoulder)))
					output --;
				else if ((UsingKeyboard && (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed))
					|| (UsingGamepad && IsXboxButtonPressed(XboxButton.RightShoulder)))
					output ++;
				return output;
			}
		}
		public int _ChangeLassoLengthInput
		{
			get
			{
				return ChangeLassoLengthInput;
			}
		}
		public static bool AttackInput
		{
			get
			{
				return (UsingMouse && Mouse.current.leftButton.isPressed)
					|| (UsingGamepad && IsXboxButtonPressed(XboxButton.RightTrigger));
			}
		}
		public bool _AttackInput
		{
			get
			{
				return AttackInput;
			}
		}
		public static Vector2 LeftStickInput
		{
			get
			{
				Vector2 best = Vector2.zero;
				for (int i = 0; i < Gamepad.all.Count; i ++)
				{
					Gamepad gamepad = Gamepad.all[i];
					if (!ShouldReadGamepad(gamepad))
						continue;
					Vector2 stick = gamepad.leftStick.ReadValue();
					Vector2 dpad = gamepad.dpad.ReadValue();
					Vector2 value = stick.sqrMagnitude >= dpad.sqrMagnitude ? stick : dpad;
					if (value.sqrMagnitude > best.sqrMagnitude)
						best = value;
				}
				for (int i = 0; i < Joystick.all.Count; i ++)
				{
					Joystick joystick = Joystick.all[i];
					if (!IsUsableJoystick(joystick) || IsSteamVirtualDevice(joystick))
						continue;
					Vector2 stick = joystick.stick != null ? joystick.stick.ReadValue() : Vector2.zero;
					if (stick.sqrMagnitude < 0.0001f)
					{
						AxisControl axisX = joystick.TryGetChildControl<AxisControl>("X");
						AxisControl axisY = joystick.TryGetChildControl<AxisControl>("Y");
						if (axisX != null && axisY != null)
							stick = new Vector2(axisX.ReadValue(), axisY.ReadValue());
					}
					Vector2 hat = ReadJoystickHat(joystick);
					Vector2 value = stick.sqrMagnitude >= hat.sqrMagnitude ? stick : hat;
					if (value.sqrMagnitude > best.sqrMagnitude)
						best = value;
				}
				return best;
			}
		}
		public static Vector2 RightStickInput
		{
			get
			{
				Vector2 best = Vector2.zero;
				for (int i = 0; i < Gamepad.all.Count; i ++)
				{
					Gamepad gamepad = Gamepad.all[i];
					if (!ShouldReadGamepad(gamepad))
						continue;
					Vector2 value = gamepad.rightStick.ReadValue();
					if (value.sqrMagnitude > best.sqrMagnitude)
						best = value;
				}
				for (int i = 0; i < Joystick.all.Count; i ++)
				{
					Joystick joystick = Joystick.all[i];
					if (!IsUsableJoystick(joystick) || IsSteamVirtualDevice(joystick))
						continue;
					Vector2 value = ReadJoystickRightStick(joystick);
					if (value.sqrMagnitude > best.sqrMagnitude)
						best = value;
				}
				return best;
			}
		}
		public static bool StartButtonDown
		{
			get
			{
				return IsXboxButtonDown(XboxButton.Start);
			}
		}
		public static bool SelectButtonDown
		{
			get
			{
				return IsXboxButtonDown(XboxButton.Back);
			}
		}
		public static bool SouthButtonDown
		{
			get
			{
				return IsXboxButtonDown(XboxButton.A);
			}
		}

		public static float GetAxis (InputControl<float> positiveButton, InputControl<float> negativeButton)
		{
			return positiveButton.ReadValue() - negativeButton.ReadValue();
		}

		public static Vector2 GetAxis2D (InputControl<float> positiveXButton, InputControl<float> negativeXButton, InputControl<float> positiveYButton, InputControl<float> negativeYButton)
		{
			Vector2 output = new Vector2();
			output.x = positiveXButton.ReadValue() - negativeXButton.ReadValue();
			output.y = positiveYButton.ReadValue() - negativeYButton.ReadValue();
			output = Vector2.ClampMagnitude(output, 1);
			return output;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void EnsureGameSirSdlMapping ()
		{
			const string mapping = "03000000373500000f10000000010000,GameSir Nova 2 Lite,a:b0,b:b1,x:b2,y:b3,back:b6,guide:b8,start:b7,leftstick:b9,rightstick:b10,leftshoulder:b4,rightshoulder:b5,dpup:h0.1,dpdown:h0.4,dpleft:h0.8,dpright:h0.2,leftx:a0,lefty:a1,rightx:a3,righty:a4,lefttrigger:a2,righttrigger:a5,platform:Linux,";
			string existing = Environment.GetEnvironmentVariable("SDL_GAMECONTROLLERCONFIG");
			if (!string.IsNullOrEmpty(existing) && existing.IndexOf("3537", StringComparison.OrdinalIgnoreCase) >= 0)
				return;
			if (string.IsNullOrEmpty(existing))
				Environment.SetEnvironmentVariable("SDL_GAMECONTROLLERCONFIG", mapping);
			else
				Environment.SetEnvironmentVariable("SDL_GAMECONTROLLERCONFIG", existing + "\n" + mapping);
		}

		static bool HasUsableJoystick ()
		{
			for (int i = 0; i < Joystick.all.Count; i ++)
			{
				if (IsUsableJoystick(Joystick.all[i]))
					return true;
			}
			return false;
		}

		static bool HasPhysicalJoystick ()
		{
			for (int i = 0; i < Joystick.all.Count; i ++)
			{
				Joystick joystick = Joystick.all[i];
				if (IsUsableJoystick(joystick) && !IsSteamVirtualDevice(joystick))
					return true;
			}
			return false;
		}

		static bool IsSteamVirtualDevice (UnityEngine.InputSystem.InputDevice device)
		{
			string label = ((device.description.product ?? "") + " " + (device.description.manufacturer ?? "") + " " + device.name).ToLowerInvariant();
			return label.Contains("steam virtual");
		}

		static bool IsUsableJoystick (Joystick joystick)
		{
			if (joystick == null || !joystick.enabled || joystick.stick == null)
				return false;
			string label = ((joystick.description.product ?? "") + " " + joystick.name).ToLowerInvariant();
			if (label.Contains("keyd") || label.Contains("pointer") || label.Contains("mouse"))
				return false;
			return true;
		}

		static bool ShouldReadGamepad (Gamepad gamepad)
		{
			if (!gamepad.enabled)
				return false;
			// On Linux, Steam's virtual pad can remap shoulders while the real pad
			// still appears as a joystick. Prefer the physical stick in that case.
			// On Windows/Steam Deck, the virtual Xbox pad is usually the live device.
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
			if (IsSteamVirtualDevice(gamepad) && HasPhysicalJoystick())
				return false;
#endif
			return true;
		}

		static Vector2 ReadJoystickHat (Joystick joystick)
		{
			if (joystick.hatswitch != null)
				return joystick.hatswitch.ReadValue();
			DpadControl hat = joystick.TryGetChildControl<DpadControl>("Hat");
			if (hat != null)
				return hat.ReadValue();
			return Vector2.zero;
		}

		static Vector2 ReadJoystickRightStick (Joystick joystick)
		{
			// Xbox pads: RX/RY are the right stick. Z/RZ are the triggers — never use those here.
			AxisControl x = joystick.TryGetChildControl<AxisControl>("RotateX");
			AxisControl y = joystick.TryGetChildControl<AxisControl>("RotateY");
			if (x != null && y != null)
				return Vector2.ClampMagnitude(new Vector2(x.ReadValue(), y.ReadValue()), 1);
			x = joystick.TryGetChildControl<AxisControl>("Rx");
			y = joystick.TryGetChildControl<AxisControl>("Ry");
			if (x != null && y != null)
				return Vector2.ClampMagnitude(new Vector2(x.ReadValue(), y.ReadValue()), 1);
			return Vector2.zero;
		}

		static bool IsXboxButtonPressed (XboxButton button)
		{
			for (int i = 0; i < Gamepad.all.Count; i ++)
			{
				Gamepad gamepad = Gamepad.all[i];
				if (!ShouldReadGamepad(gamepad))
					continue;
				if (ReadGamepadButton(gamepad, button, false))
					return true;
			}
			for (int i = 0; i < Joystick.all.Count; i ++)
			{
				Joystick joystick = Joystick.all[i];
				if (!IsUsableJoystick(joystick) || IsSteamVirtualDevice(joystick))
					continue;
				if (ReadJoystickXboxButton(joystick, button, false))
					return true;
			}
			return false;
		}

		static bool IsXboxButtonDown (XboxButton button)
		{
			for (int i = 0; i < Gamepad.all.Count; i ++)
			{
				Gamepad gamepad = Gamepad.all[i];
				if (!ShouldReadGamepad(gamepad))
					continue;
				if (ReadGamepadButton(gamepad, button, true))
					return true;
			}
			for (int i = 0; i < Joystick.all.Count; i ++)
			{
				Joystick joystick = Joystick.all[i];
				if (!IsUsableJoystick(joystick) || IsSteamVirtualDevice(joystick))
					continue;
				if (ReadJoystickXboxButton(joystick, button, true))
					return true;
			}
			return false;
		}

		static bool ReadGamepadButton (Gamepad gamepad, XboxButton button, bool pressedThisFrame)
		{
			ButtonControl control = null;
			switch (button)
			{
				case XboxButton.A: control = gamepad.buttonSouth; break;
				case XboxButton.B: control = gamepad.buttonEast; break;
				case XboxButton.X: control = gamepad.buttonWest; break;
				case XboxButton.Y: control = gamepad.buttonNorth; break;
				case XboxButton.LeftShoulder: control = gamepad.leftShoulder; break;
				case XboxButton.RightShoulder: control = gamepad.rightShoulder; break;
				case XboxButton.Back: control = gamepad.selectButton; break;
				case XboxButton.Start: control = gamepad.startButton; break;
				case XboxButton.LeftStick: control = gamepad.leftStickButton; break;
				case XboxButton.RightStick: control = gamepad.rightStickButton; break;
				case XboxButton.LeftTrigger:
					return pressedThisFrame ? gamepad.leftTrigger.wasPressedThisFrame : gamepad.leftTrigger.isPressed;
				case XboxButton.RightTrigger:
					return pressedThisFrame ? gamepad.rightTrigger.wasPressedThisFrame : gamepad.rightTrigger.isPressed;
			}
			if (control == null)
				return false;
			return pressedThisFrame ? control.wasPressedThisFrame : control.isPressed;
		}

		static bool ReadJoystickXboxButton (Joystick joystick, XboxButton button, bool pressedThisFrame)
		{
			if (button == XboxButton.LeftTrigger || button == XboxButton.RightTrigger)
			{
				if (pressedThisFrame)
					return false;
				return IsJoystickTriggerPressed(joystick, button == XboxButton.LeftTrigger);
			}
			// Prefer xpad button index (matches GameSir / Xbox 360 on Linux). Named SDL
			// usages on unmapped joysticks are unreliable and caused shoulder/center swaps.
			ButtonControl indexed = GetJoystickButtonByIndex(joystick, (int) button);
			if (indexed != null)
				return pressedThisFrame ? indexed.wasPressedThisFrame : indexed.isPressed;
			string[] names = GetJoystickButtonNames(button);
			for (int i = 0; i < names.Length; i ++)
			{
				ButtonControl named = joystick.TryGetChildControl<ButtonControl>(names[i]);
				if (named != null && (pressedThisFrame ? named.wasPressedThisFrame : named.isPressed))
					return true;
			}
			return false;
		}

		static bool IsJoystickTriggerPressed (Joystick joystick, bool leftTrigger)
		{
			string[] names = leftTrigger
				? new[] { "Z", "Brake", "Axis2", "TriggerLeft2", "leftTrigger" }
				: new[] { "Rz", "Gas", "Axis5", "TriggerRight2", "rightTrigger", "RotateZ" };
			for (int i = 0; i < names.Length; i ++)
			{
				InputControl control = joystick.TryGetChildControl(names[i]);
				if (control == null)
					continue;
				AxisControl axis = control as AxisControl;
				if (axis != null && IsTriggerAxisPressed(axis))
					return true;
				ButtonControl button = control as ButtonControl;
				if (button != null && button.isPressed)
					return true;
			}
			return false;
		}

		static bool IsTriggerAxisPressed (AxisControl axis)
		{
			float value = axis.ReadValue();
			// SDL/js triggers rest at -1 and travel to +1; some layouts use 0..1 instead.
			float amount = value < 0f ? (value + 1f) * 0.5f : value;
			return amount >= 0.25f;
		}

		static string[] GetJoystickButtonNames (XboxButton button)
		{
			switch (button)
			{
				case XboxButton.A: return new[] { "A", "buttonSouth" };
				case XboxButton.B: return new[] { "B", "buttonEast" };
				case XboxButton.X: return new[] { "X", "buttonWest" };
				case XboxButton.Y: return new[] { "Y", "buttonNorth" };
				case XboxButton.LeftShoulder: return new[] { "TriggerLeft", "leftShoulder", "LeftShoulder" };
				case XboxButton.RightShoulder: return new[] { "TriggerRight", "rightShoulder", "RightShoulder" };
				case XboxButton.Back: return new[] { "Select", "Back", "select" };
				case XboxButton.Start: return new[] { "Start", "start" };
				case XboxButton.LeftStick: return new[] { "ThumbLeft", "leftStickPress", "LeftStick" };
				case XboxButton.RightStick: return new[] { "ThumbRight", "rightStickPress", "RightStick" };
				default: return Array.Empty<string>();
			}
		}

		static ButtonControl GetJoystickButtonByIndex (Joystick joystick, int index)
		{
			if (index < 0 || index > 10)
				return null;
			int buttonIndex = 0;
			bool hasDedicatedFaceButton = joystick.TryGetChildControl("A") != null
				|| joystick.TryGetChildControl("buttonSouth") != null;
			foreach (InputControl control in joystick.allControls)
			{
				ButtonControl button = control as ButtonControl;
				if (button == null || button.parent != joystick)
					continue;
				// Joystick always exposes a synthetic "trigger"; skip it when face buttons exist
				// so indices stay a:b0..rightstick:b10 like the Linux xpad layout.
				if (hasDedicatedFaceButton && button == joystick.trigger)
					continue;
				if (buttonIndex == index)
					return button;
				buttonIndex ++;
			}
			return null;
		}

		// Linux xpad / GameSir / Steam mapping: a:b0,b:b1,x:b2,y:b3,leftshoulder:b4,...
		enum XboxButton
		{
			A = 0,
			B = 1,
			X = 2,
			Y = 3,
			LeftShoulder = 4,
			RightShoulder = 5,
			Back = 6,
			Start = 7,
			Guide = 8,
			LeftStick = 9,
			RightStick = 10,
			LeftTrigger = 11,
			RightTrigger = 12
		}
		
		public enum HotkeyState
		{
			Down,
			Held,
			Up
		}
		
		public enum InputDevice
		{
			KeyboardAndMouse,
			Phone,
			Gamepad
		}
	}
}
