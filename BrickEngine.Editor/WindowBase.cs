//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor
//{
//    public abstract class WindowBase
//    {
//        private static readonly Dictionary<Type, int> windowCounts = new Dictionary<Type, int>();
//        private static int MessageIdCounter = 1;
//        public readonly EditorManager EditorManager;
//        public readonly int Id;
//        public readonly int MessageId;
//        private bool enabledThisFrame;
//        private bool disabledThisFrame;
//        private bool enabled;

//        protected WindowBase(EditorManager editorManager)
//        {
//            EditorManager = editorManager;
//            Type myType = GetType();
//            if (!windowCounts.TryGetValue(myType, out int count))
//            {
//                count = 1;
//                windowCounts.Add(myType, count);
//            }
//            Id = count;
//            windowCounts[myType] = count + 1;
//            MessageId = MessageIdCounter++;
//        }

//        public bool Enabled
//        {
//            get
//            {
//                return enabled;
//            }
//            set
//            {
//                enabledThisFrame = !enabled && value;
//                disabledThisFrame = enabled && !value;
//                enabled = value;
//            }
//        }

//        public void UpdateWindow()
//        {
//            foreach (var pool in EditorManager.EditorMsgBus.Pools)
//            {
//                pool.Value.RemoveMessages(MessageId);
//            }
//            if (enabledThisFrame)
//            {
//                OnOpen();
//                enabledThisFrame = false;
//            }
//            if (disabledThisFrame)
//            {
//                OnClose();
//                disabledThisFrame = false;
//            }
//            if (enabled)
//            {
//                OnUpdate();
//            }
//        }

//        protected bool BeginWindow(string title, ImGuiWindowFlags flags)
//        {
//            bool b = Enabled;
//            bool result = ImGui.Begin(title, ref b, flags);
//            Enabled = b;
//            return result;
//        }

//        protected bool BeginWindow(string title)
//        {
//            bool b = Enabled;
//            bool result = ImGui.Begin(title, ref b);
//            Enabled = b;
//            return result;
//        }

//        protected T GetSingleton<T>()
//        {
//            return EditorManager.GetSingleton<T>();
//        }

//        protected T GetInjected<T>(string identifier)
//        {
//            return EditorManager.GetInjected<T>(identifier);
//        }


//        protected virtual void OnOpen() { }
//        protected virtual void OnClose() { }
//        protected abstract void OnUpdate();
//    }
//}
