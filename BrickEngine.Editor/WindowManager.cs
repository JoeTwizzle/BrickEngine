//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor
//{
//    public sealed class WindowManager
//    {
//        readonly List<WindowBase> windows;
//        readonly Dictionary<string, int> namedWindows;
//        readonly Tree<int> titleBarWindows;
//        public IReadOnlyList<WindowBase> Windows => windows;
//        public IReadOnlyDictionary<string, int> NamedWindows => namedWindows;
//        public IReadOnlyTree<int> TitleBarWindows => titleBarWindows;
//        public WindowManager()
//        {
//            titleBarWindows = new Tree<int>();
//            windows = new List<WindowBase>();
//            namedWindows = new Dictionary<string, int>();
//        }

//        public WindowBase AddWindow(WindowBase window, string name, string titleBarEntry)
//        {
//            windows.Add(window);
//            namedWindows.Add(name, windows.Count - 1);
//            titleBarWindows.AddNode(titleBarEntry, windows.Count - 1);
//            return window;
//        }

//        public WindowBase AddWindow(WindowBase window, string name)
//        {
//            windows.Add(window);
//            namedWindows.Add(name, windows.Count - 1);
//            return window;
//        }

//        public WindowBase AddWindow(WindowBase window)
//        {
//            windows.Add(window);
//            return window;
//        }

//        public int GetWindowIndex(string name)
//        {
//            return namedWindows[name];
//        }

//        public WindowBase GetWindow(string name)
//        {
//            return windows[GetWindowIndex(name)];
//        }

//        public WindowBase GetWindow(int index)
//        {
//            return windows[index];
//        }

//        public void UpdateWindows()
//        {
//            foreach (var window in windows)
//            {
//                window.UpdateWindow();
//            }
//        }
//    }
//}
