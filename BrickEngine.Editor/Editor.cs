global using ImGuiNET;
global using BrickEngine.Core;
global using EcsLite;
global using EcsLite.Systems;
global using EcsLite.Messages;
using System;
using Veldrid;
using BrickEngine.Gui;

namespace BrickEngine.Editor
{
    public class Editor
    {
        private readonly ImGuiController _imGuiController;
        private readonly EditorManager _editor;
        private readonly Game _game;
        private readonly CommandList _cl;

        public Editor(Game game)
        {
            _editor = new EditorManager(game);
            _game = game;
            _imGuiController = new ImGuiController(_game.GraphicsDevice, _game.Window, _game.GraphicsDevice.MainSwapchain!.Framebuffer.OutputDescription, _game.Window.Width, _game.Window.Height);
            _cl = _game.ResourceFactory.CreateCommandList();
            game.OnResized += Game_OnResized;
            game.OnCreateDefaultWorld += Game_OnCreateDefaultWorld;
            game.OnRegisterWorlds += Game_OnRegisterWorlds;
            game.OnCreateSystems += Game_OnCreateSystems;
            game.OnInit += Game_OnInit;
            game.OnPreUpdate += Game_OnPreUpdate;
            game.OnPostUpdate += Game_OnPostUpdate;
            game.OnPostSwap += Game_OnPostSwap;
            game.OnDisposeGame += Game_OnDisposeGame;
        }

        private void Game_OnDisposeGame()
        {
            _imGuiController.Dispose();
        }

        private void Game_OnCreateDefaultWorld(EcsWorld world)
        {
            world.AllowPool<GigaTestComponent>();
        }

        private void Game_OnRegisterWorlds()
        {
            EcsWorld world = new EcsWorld();
            _game.AddWorld("Editor", world);
        }

        private void Game_OnCreateSystems(EcsSystemsBuilder obj)
        {

        }

        private void Game_OnInit()
        {
            _editor.InjectSingleton(_game);
            _editor.Init();
            _game.DefaultWorld.NewEntity();
            _game.DefaultWorld.NewEntity();
            var pool = _game.DefaultWorld.GetPool<GigaTestComponent>();
            pool.Add(_game.DefaultWorld.NewEntity());
            pool.Add(_game.DefaultWorld.NewEntity());
        }

        private void Game_OnPreUpdate()
        {
            _imGuiController.Update(_game.DeltaTime, _game.InputSnapshot);
        }

        private void Game_OnPostUpdate()
        {
            ImGui.ShowDemoWindow();
            _editor.Update();
            _cl.Begin();
            _cl.SetFramebuffer(_game.GraphicsDevice.SwapchainFramebuffer!);
            _cl.SetFullViewport(0);
            _cl.ClearColorTarget(0, RgbaFloat.Blue);
            //_cl.ClearDepthStencil(0);
            _imGuiController.Render(_game.GraphicsDevice, _cl);
            //_cl.SetFramebuffer(_game.GraphicsDevice.SwapchainFramebuffer!);
            //_cl.ClearColorTarget(0, RgbaFloat.Green);
            //_cl.SetFullViewport(0);
            _cl.End();
            _game.SubmitCommands(_cl);
        }

        private void Game_OnPostSwap()
        {
            _imGuiController.SwapExtraWindows(_game.GraphicsDevice);
        }

        private void Game_OnResized()
        {
            _imGuiController.WindowResized(_game.Window.Width, _game.Window.Height);
        }
    }
}
