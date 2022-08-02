
using System;
using BrickEngine.Editor.EditorWindows;
using EcsLite;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Numerics;
using EcsLite.Messages;
using BrickEngine.Editor.Messages;

namespace BrickEngine.Editor
{
    public class EditorManager
    {
        private readonly Dictionary<Type, object> _injectedSingletons;
        private readonly Dictionary<string, object> _injected;
        public readonly int MessageId = 0;
        public readonly Game Game;
        public readonly WindowManager WindowManager;
        public readonly Dictionary<EcsWorld, List<int>> SelectedEntites;
        public readonly MessageBus EditorMsgBus;

        public EditorManager(Game game)
        {
            EditorMsgBus = new MessageBus();
            SelectedEntites = new Dictionary<EcsWorld, List<int>>();
            _injected = new Dictionary<string, object>();
            _injectedSingletons = new Dictionary<Type, object>();
            Game = game;
            WindowManager = new WindowManager();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public T GetSingleton<T>()
        {
            return (T)_injectedSingletons[typeof(T)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public T GetInjected<T>(string identifier)
        {
            return (T)_injected[identifier];
        }

        public void AddSelectedEntity(EcsWorld world, int entity)
        {
            if (!SelectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<int>();
                SelectedEntites.Add(world, entities);
            }
            entities.Add(entity);
        }

        public void SetSelectedEntity(EcsWorld world, int entity)
        {
            if (!SelectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<int>();
                SelectedEntites.Add(world, entities);
            }
            entities.Clear();
            entities.Add(entity);
        }

        public List<int> GetSelectedEntities(EcsWorld world)
        {
            if (!SelectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<int>();
                SelectedEntites.Add(world, entities);
                var pool = EditorMsgBus.GetPool<SelectedEnititiesChanged>();
                pool.Add(MessageId, new SelectedEnititiesChanged());
            }
            return entities;
        }

        public void SetSelectedEntities(EcsWorld world, int entity)
        {
            if (!SelectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<int>();
                SelectedEntites.Add(world, entities);
            }
            entities.Clear();
            entities.Add(entity);
        }

        public void ClearSelectedEntities(EcsWorld world)
        {
            if (!SelectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<int>();
                SelectedEntites.Add(world, entities);
            }
            entities.Clear();
        }

        public void Init()
        {
            WindowManager.AddWindow(new WorldInspector(this), "EntityInspector", "Entities");
            WindowManager.AddWindow(new ComponentInspector(this), "ComponentInspector", "Components");
            WindowManager.AddWindow(new WorldInspector(this), "EntityInspector3", "Test/Test3");
        }

        public void Inject<T>(string identifier, T data) where T : notnull
        {
            _injected.Add(identifier, data);
        }

        public void InjectSingleton<T>(T data) where T : notnull
        {
            _injectedSingletons.Add(typeof(T), data);
        }

        public void Update()
        {
            foreach (var pool in EditorMsgBus.Pools)
            {
                pool.Value.RemoveMessages(MessageId);
            }
            ImGui.DockSpaceOverViewport();
            WindowManager.UpdateWindows();
            //DrawDockSpaceBG();
            var size = ImGui.GetWindowSize();
            float menuBarHeight = 0;
            if (ImGui.BeginMainMenuBar())
            {
                menuBarHeight = ImGui.GetWindowSize().Y;
                foreach (KeyValuePair<string, Tree<int>.TreeNode> node in WindowManager.TitleBarWindows.RootNode.Nodes)
                {
                    DrawMenu(node.Key, node.Value);
                }
            }
            ImGui.EndMainMenuBar();
            //uint dockspace_id = ImGui.GetID("Dockspace");
            //ImGui.DockSpace(dockspace_id, new Vector2(size.X, size.Y - menuBarHeight));
        }

        void DrawDockSpaceBG()
        {
            var viewport = ImGui.GetMainViewport();

            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGuiWindowFlags host_window_flags = 0;
            host_window_flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.DockNodeHost;
            host_window_flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.Begin("MainWindow", host_window_flags);
            ImGui.PopStyleVar(3);

            ImGui.End();
        }

        void DrawMenu(string key, Tree<int>.TreeNode baseNode)
        {
            if (baseNode.IsLeaf)
            {
                if (ImGui.MenuItem(key))
                {
                    WindowManager.GetWindow(baseNode.Value).Enabled = true;
                }
            }
            else
            {
                if (ImGui.BeginMenu(key))
                {
                    foreach (var node in baseNode.Nodes)
                    {
                        DrawMenu($"{node.Key}##{key}", node.Value);
                    }
                    ImGui.EndMenu();
                }
            }
        }
    }
}
