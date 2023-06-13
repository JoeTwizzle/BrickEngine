//using BrickEngine.Editor.Messages;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor.EditorWindows
//{
//    internal class EditorViewport : WindowBase
//    {
//        readonly GameWindow _game;
//        readonly string _title;

//        public EditorViewport(EditorManager editorManager) : base(editorManager)
//        {
//            _title = $"Editor Viewport##{Id}";
//            _game = EditorManager.GetSingleton<GameWindow>();
//        }

//        protected override void OnUpdate()
//        {
//            if (BeginWindow(_title))
//            {

//            }
//            ImGui.End();
//        }
//    }
//}
