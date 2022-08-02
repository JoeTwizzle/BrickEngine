using BrickEngine.Core;
using EcsLite;
using EcsLite.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrickEngine.Gui;

namespace Masonry
{
    internal class EditorGame
    {
        private readonly ImGuiController _imGuiController;
        private readonly Game _game;
        public EditorGame(Game game)
        {
            _game = game;
            _imGuiController = new ImGuiController(_game.GraphicsDevice, _game.Window);
            game.OnResized += Game_OnResized;
            game.OnPostUpdate += Game_OnPostUpdate;
            game.OnPreUpdate += Game_OnPreUpdate;
            game.OnInit += Game_OnInit;
        }

        private void Game_OnInit()
        {
            
        }

        private void Game_OnPreUpdate()
        {
            
        }

        private void Game_OnPostUpdate()
        {
            _imGuiController.Update(_game, _game.InputSnapshot);
            _imGuiController.SwapExtraWindows(_game.GraphicsDevice);
        }

        private void Game_OnResized()
        {
            _imGuiController.WindowResized(_game.Window.Width, _game.Window.Height);
        }
    }
}
