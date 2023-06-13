using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core
{
    public class Input
    {
        public Sdl2ControllerTracker? DefaultController { get; private set; }
        public Dictionary<int, Sdl2ControllerTracker> Controllers { get; private set; }

        private readonly Sdl2Window Window;
        public Input(Sdl2Window window)
        {
            Sdl2Native.SDL_SetHint("SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");
            Sdl2Native.SDL_Init(SDLInitFlags.Sensor);
            Sdl2Native.SDL_Init(SDLInitFlags.Joystick);
            //Sdl2Native.SDL_Init(SDLInitFlags.Haptic);
            Sdl2Native.SDL_Init(SDLInitFlags.GameController);
            Sdl2Events.Subscribe(ProcessEvent);
            if (Sdl2ControllerTracker.CreateDefault(out var controller))
            {
                DefaultController = controller;
            }
            Controllers = new Dictionary<int, Sdl2ControllerTracker>();
            Window = window;
        }

        private void ProcessEvent(ref SDL_Event ev)
        {
            switch (ev.type)
            {

                case SDL_EventType.ControllerDeviceAdded:
                case SDL_EventType.ControllerDeviceRemoved:
                    {
                        SDL_ControllerDeviceEvent deviceEvent = Unsafe.As<SDL_Event, SDL_ControllerDeviceEvent>(ref ev);
                        if (deviceEvent.type == (uint)SDL_EventType.ControllerDeviceAdded)
                        {
                            Controllers.TryAdd(deviceEvent.which, new Sdl2ControllerTracker(deviceEvent.which));
                        }
                        if (deviceEvent.type == (uint)SDL_EventType.ControllerDeviceRemoved)
                        {
                            if (Controllers.TryGetValue(deviceEvent.which, out var cont))
                            {
                                cont.Dispose();
                                Controllers.Remove(deviceEvent.which);
                            }
                        }
                        break;
                    }
                case SDL_EventType.ControllerAxisMotion:
                    {
                        SDL_ControllerAxisEvent axisEvent = Unsafe.As<SDL_Event, SDL_ControllerAxisEvent>(ref ev);
                        if (Controllers.TryGetValue(axisEvent.which, out var cont))
                        {
                            DefaultController = cont;
                            //cont.ProcessEvent(ref ev);
                        }

                        break;
                    }
                case SDL_EventType.ControllerButtonDown:
                case SDL_EventType.ControllerButtonUp:
                    {
                        SDL_ControllerButtonEvent buttonEvent = Unsafe.As<SDL_Event, SDL_ControllerButtonEvent>(ref ev);
                        if (Controllers.TryGetValue(buttonEvent.which, out var cont))
                        {
                            DefaultController = cont;
                            //cont.ProcessEvent(ref ev);
                        }
                        break;
                    }
            }
        }

        private readonly HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private readonly HashSet<Key> _newKeysDownThisFrame = new HashSet<Key>();
        private readonly HashSet<Key> _newKeysUpThisFrame = new HashSet<Key>();

        private readonly HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> _newMouseButtonsDownThisFrame = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> _newMouseButtonsUpThisFrame = new HashSet<MouseButton>();

        public Vector2 MousePosition;
        public Vector2 MouseDelta;
        public InputSnapshot? FrameSnapshot { get; private set; }

        private delegate void SDL_SetWindowGrab_t(IntPtr windowPtr, int boolean);
        private SDL_SetWindowGrab_t? p_sdl_SetWindowGrab;

        private delegate int SDL_GetWindowGrab_t(IntPtr windowPtr);
        private SDL_GetWindowGrab_t? p_sdl_GetWindowGrab;

        private delegate int SDL_SetRelativeMouseMode_t(int boolean);
        private SDL_SetRelativeMouseMode_t? p_sdl_SetRelativeMouseMode;

        private delegate int SDL_GetRelativeMouseMode_t();
        private SDL_GetRelativeMouseMode_t? p_sdl_GetRelativeMouseMode;

        private delegate void SDL_WarpMouseInWindow_t(IntPtr windowPtr, int x, int y);
        private SDL_WarpMouseInWindow_t? p_sdl_WarpMouseInWindow;

        public bool IsMouseGrabbed
        {
            get
            {
                if (p_sdl_GetWindowGrab == null)
                {
                    p_sdl_GetWindowGrab = Sdl2Native.LoadFunction<SDL_GetWindowGrab_t>("SDL_GetWindowGrab");
                }
                return p_sdl_GetWindowGrab(Window.SdlWindowHandle) != 0;
            }
        }
        public void SetMouseGrab(bool state)
        {
            if (p_sdl_SetWindowGrab == null)
            {
                p_sdl_SetWindowGrab = Sdl2Native.LoadFunction<SDL_SetWindowGrab_t>("SDL_SetWindowGrab");
            }
            p_sdl_SetWindowGrab(Window.SdlWindowHandle, state ? 1 : 0);
        }
        public void GrabMouse()
        {
            SetMouseGrab(true);
        }

        public void UngrabMouse()
        {
            SetMouseGrab(false);
        }

        public bool IsMouseLocked
        {
            get
            {
                if (p_sdl_GetRelativeMouseMode == null)
                {
                    p_sdl_GetRelativeMouseMode = Sdl2Native.LoadFunction<SDL_GetRelativeMouseMode_t>("SDL_GetRelativeMouseMode");
                }
                return p_sdl_GetRelativeMouseMode() != 0;
            }
        }

        public void SetMouseLock(bool state)
        {
            if (p_sdl_SetRelativeMouseMode == null)
            {
                p_sdl_SetRelativeMouseMode = Sdl2Native.LoadFunction<SDL_SetRelativeMouseMode_t>("SDL_SetRelativeMouseMode");
            }
            p_sdl_SetRelativeMouseMode(state ? 1 : 0);
        }

        public void WarpMouse(int x, int y)
        {
            if (p_sdl_WarpMouseInWindow == null)
            {
                p_sdl_WarpMouseInWindow = Sdl2Native.LoadFunction<SDL_WarpMouseInWindow_t>("SDL_WarpMouseInWindow");
            }
            p_sdl_WarpMouseInWindow(Window.SdlWindowHandle, x, y);
        }

        public void LockMouse()
        {
            SetMouseLock(true);
        }

        public void UnlockMouse()
        {
            SetMouseLock(false);
        }

        public bool IsMouseVisible => Window.CursorVisible;

        public void SetMouseVisible(bool value)
        {
            Window.CursorVisible = value;
        }

        public void HideMouse()
        {
            SetMouseVisible(false);
        }

        public void UnhideMouse()
        {
            SetMouseVisible(true);
        }

        public bool GetKey(Key key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public bool GetKeyDown(Key key)
        {
            return _newKeysDownThisFrame.Contains(key);
        }

        public bool GetKeyUp(Key key)
        {
            return _newKeysUpThisFrame.Contains(key);
        }

        public bool GetMouseButton(MouseButton button)
        {
            return _currentlyPressedMouseButtons.Contains(button);
        }

        public bool GetMouseButtonDown(MouseButton button)
        {
            return _newMouseButtonsDownThisFrame.Contains(button);
        }

        public bool GetMouseButtonUp(MouseButton button)
        {
            return _newMouseButtonsUpThisFrame.Contains(button);
        }

        public void UpdateFrameInput(InputSnapshot snapshot, Sdl2Window window)
        {
            if (IsMouseGrabbed)
            {
                WarpMouse(window.Width / 2, window.Height / 2);
            }
            FrameSnapshot = snapshot;
            _newKeysDownThisFrame.Clear();
            _newKeysUpThisFrame.Clear();
            _newMouseButtonsDownThisFrame.Clear();
            _newMouseButtonsUpThisFrame.Clear();

            MouseDelta = window.MouseDelta;
            MousePosition = snapshot.MousePosition;
            for (int i = 0; i < snapshot.KeyEvents.Length; i++)
            {
                KeyEvent ke = snapshot.KeyEvents[i];
                if (ke.Down)
                {
                    KeyDown(ke.Physical);
                }
                else
                {
                    KeyUp(ke.Physical);
                }
            }
            for (int i = 0; i < snapshot.MouseEvents.Length; i++)
            {
                MouseButtonEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    MouseDown(me.MouseButton);
                }
                else
                {
                    MouseUp(me.MouseButton);
                }
            }

            foreach (var controller in Controllers)
            {
                controller.Value.Update();
            }
        }

        private void MouseUp(MouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Remove(mouseButton))
            {
                _newMouseButtonsUpThisFrame.Add(mouseButton);
            }
            _newMouseButtonsDownThisFrame.Remove(mouseButton);
        }

        private void MouseDown(MouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Add(mouseButton))
            {
                _newMouseButtonsDownThisFrame.Add(mouseButton);
            }
            _newMouseButtonsUpThisFrame.Remove(mouseButton);
        }

        private void KeyUp(Key key)
        {
            if (_currentlyPressedKeys.Remove(key))
            {
                _newKeysUpThisFrame.Add(key);
            }
            _newKeysDownThisFrame.Remove(key);
        }

        private void KeyDown(Key key)
        {
            if (_currentlyPressedKeys.Add(key))
            {
                _newKeysDownThisFrame.Add(key);
            }
            _newKeysUpThisFrame.Remove(key);
        }
    }
}
