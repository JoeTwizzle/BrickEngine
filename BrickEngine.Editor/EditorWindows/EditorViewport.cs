using BrickEngine.Editor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.EditorWindows
{
    internal class EditorViewport : WindowBase
    {
        readonly Game _game;
        readonly string _title;
        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;

        public EditorViewport(EditorManager editorManager) : base(editorManager)
        {
            _title = $"Editor Viewport##{Id}";
            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
            _game = EditorManager.GetSingleton<Game>();
        }

        protected override void OnUpdate()
        {
            if (BeginWindow(_title))
            {

            }
            ImGui.End();
        }
    }
}
