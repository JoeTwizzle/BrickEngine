//using BrickEngine.Editor.Messages;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor.EditorWindows
//{
//    public class ProjectView : WindowBase
//    {
//        readonly GameWindow _game;
//        readonly string _title;
//        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;

//        public ProjectView(EditorManager editorManager) : base(editorManager)
//        {
//            _title = $"Project View##{Id}";
//            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
//            _game = EditorManager.GetSingleton<GameWindow>();
//        }

//        protected override void OnUpdate()
//        {
//            if (BeginWindow(_title))
//            {
//                if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered())
//                {

//                }
//            }
//            ImGui.End();
//        }
//    }
//}
