global using ImGuiNET;
global using BrickEngine.Core;
global using EntityForge;
using System;
using Veldrid;
using BrickEngine.Gui;

namespace BrickEngine.Editor
{
    public class Editor : IFeatureLayer
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ImGuiController _imGuiController;
        //private EditorManager _editor;
        private CommandList _cl;
        private GraphicsDevice _gd;
        public GameWindow Game { get; private set; }
        public World EditorWorld { get; private set; }

        public bool IsEnabled { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void OnLoad(GameWindow game)
        {
            IsEnabled = true;
            Game = game;
            EditorWorld = new();
            _gd = Game.GraphicsContext.GraphicsDevice;
            _imGuiController = new ImGuiController(_gd, Game.Window, _gd.MainSwapchain!.Framebuffer.OutputDescription, Game.Window.Width, Game.Window.Height);
            _cl = Game.GraphicsContext.ResourceFactory.CreateCommandList();
            //_editor = new EditorManager(this);
            game.Window.Resized += Window_Resized;
        }

        private void Window_Resized()
        {
            _imGuiController.WindowResized(Game.Window.Width, Game.Window.Height);
        }

        public void Update()
        {
            _imGuiController.Update(Game.DeltaTime);
            ImGui.ShowDemoWindow();
            //_editor.Update();
            _cl.Begin();
            _cl.SetFramebuffer(_gd.SwapchainFramebuffer!);
            _cl.SetFullViewport(0);
            _cl.ClearColorTarget(0, RgbaFloat.Blue);
            //_cl.ClearDepthStencil(0);
            _imGuiController.Render(_gd, _cl);
            //_cl.SetFramebuffer(_game.GraphicsDevice.SwapchainFramebuffer!);
            //_cl.ClearColorTarget(0, RgbaFloat.Green);
            //_cl.SetFullViewport(0);
            _cl.End();
            Game.GraphicsContext.SubmitCommands(_cl);
        }

        public void Display()
        {
            _imGuiController.SwapExtraWindows(_gd);
        }

        public void OnUnload()
        {
            Game.Window.Resized -= Window_Resized;
            EditorWorld.Dispose();
        }
    }
}
