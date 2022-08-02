using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Veldrid.Sdl2.Sdl2Native;

namespace BrickEngine.Core
{
    public class Sdl2ControllerTracker : IDisposable
    {
        private readonly int _controllerIndex;
        private readonly SDL_GameController _controller;

        public string ControllerName { get; }
        //public bool HasGyroscope { get; }
        private readonly float[] _axisValues = new float[6];

        private readonly bool[] _buttons = new bool[15];
        private readonly bool[] _buttonsLastTick = new bool[15];

        public unsafe Sdl2ControllerTracker(int index)
        {
            _controller = SDL_GameControllerOpen(index);
            SDL_Joystick joystick = SDL_GameControllerGetJoystick(_controller);
            _controllerIndex = SDL_JoystickInstanceID(joystick);
            // HasGyroscope = SDL_GameControllerHasSensor(_controller, SDL_SensorType.Gyroscope);
            ControllerName = Marshal.PtrToStringUTF8((IntPtr)SDL_GameControllerName(_controller)) ?? "";
        }

        public float GetAxis(SDL_GameControllerAxis axis)
        {
            axis = (SDL_GameControllerAxis)Math.Max(Math.Min((byte)axis, (byte)6), (byte)0);
            return _axisValues[(byte)axis];
        }

        public bool GetButton(SDL_GameControllerButton button)
        {
            button = button & SDL_GameControllerButton.Max;
            return _buttons[(byte)button];
        }

        public bool GetButtonDown(SDL_GameControllerButton button)
        {
            button = button & SDL_GameControllerButton.Max;
            var lastB = _buttonsLastTick[(byte)button];
            var currB = _buttons[(byte)button];
            return !lastB && currB;
        }

        public bool GetButtonUp(SDL_GameControllerButton button)
        {
            button = button & SDL_GameControllerButton.Max;
            var lastB = _buttonsLastTick[(byte)button];
            var currB = _buttons[(byte)button];
            return lastB && !currB;
        }

        public static bool CreateDefault([NotNullWhen(true)] out Sdl2ControllerTracker? sct)
        {
            int jsCount = SDL_NumJoysticks();
            for (int i = 0; i < jsCount; i++)
            {
                if (SDL_IsGameController(i))
                {
                    sct = new Sdl2ControllerTracker(i);
                    return true;
                }
            }

            sct = null;
            return false;
        }

        public void Update()
        {
            Span<byte> buttons = stackalloc byte[15]
            {
                (byte)SDL_GameControllerButton.A,
                (byte)SDL_GameControllerButton.B,
                (byte)SDL_GameControllerButton.X,
                (byte)SDL_GameControllerButton.Y,
                (byte)SDL_GameControllerButton.Back,
                (byte)SDL_GameControllerButton.Guide,
                (byte)SDL_GameControllerButton.Start,
                (byte)SDL_GameControllerButton.LeftStick,
                (byte)SDL_GameControllerButton.RightStick,
                (byte)SDL_GameControllerButton.LeftShoulder,
                (byte)SDL_GameControllerButton.RightShoulder,
                (byte)SDL_GameControllerButton.DPadUp,
                (byte)SDL_GameControllerButton.DPadDown,
                (byte)SDL_GameControllerButton.DPadLeft,
                (byte)SDL_GameControllerButton.DPadRight,
            };
            Span<byte> axes = stackalloc byte[6]
            {
                (byte)SDL_GameControllerAxis.LeftX,
                (byte)SDL_GameControllerAxis.LeftY,
                (byte)SDL_GameControllerAxis.RightX,
                (byte)SDL_GameControllerAxis.RightY,
                (byte)SDL_GameControllerAxis.TriggerLeft,
                (byte)SDL_GameControllerAxis.TriggerRight,
            };
            foreach (ref var button in buttons)
            {
                _buttonsLastTick[button] = _buttons[button];
                _buttons[button] = SDL_GameControllerGetButton(_controller, (SDL_GameControllerButton)button) != 0;
            }
            foreach (ref var axis in axes)
            {
                _axisValues[axis] = Normalize(SDL_GameControllerGetAxis(_controller, (SDL_GameControllerAxis)axis));
            }
        }
        private float Normalize(short value)
        {
            return value < 0
                ? -(value / (float)short.MinValue)
                : (value / (float)short.MaxValue);
        }

        public void Dispose()
        {
            SDL_GameControllerClose(_controller);
        }
    }
}
