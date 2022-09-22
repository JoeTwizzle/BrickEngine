
using System;
using BrickEngine.Editor.EditorWindows;
using EcsLite;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Numerics;
using EcsLite.Messages;
using BrickEngine.Editor.Messages;
using System.Text.Json;

namespace BrickEngine.Editor
{
    public class EditorManager : IDisposable
    {
        const string statusFileName = "EditorStatus.cfg";
        readonly JsonSerializerOptions options = new JsonSerializerOptions() { IncludeFields = true };
        private readonly Dictionary<Type, object> _injectedSingletons;
        private readonly Dictionary<string, object> _injected;
        private readonly Dictionary<EcsWorld, List<EcsLocalEntity>> _selectedEntites;
        public readonly int MessageId = 0;
        public readonly Game Game;
        public readonly WindowManager WindowManager;
        public readonly MessageBus EditorMsgBus;
        public readonly ActionManager ActionManager;
        private int selectedEntityCount = 0;
        public IReadOnlyDictionary<EcsWorld, List<EcsLocalEntity>> SelectedEntites => _selectedEntites;
        public int SelectedEntityCount => selectedEntityCount;
        public EditorManager(Game game)
        {
            ActionManager = new ActionManager();
            EditorMsgBus = new MessageBus();
            _selectedEntites = new Dictionary<EcsWorld, List<EcsLocalEntity>>();
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

        public void AddSelectedEntity(EcsWorld world, EcsLocalEntity entity)
        {
            if (!_selectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<EcsLocalEntity>();
                _selectedEntites.Add(world, entities);
            }
            entities.Add(entity);
            selectedEntityCount++;
        }

        public void SetSelectedEntity(EcsWorld world, EcsLocalEntity entity)
        {
            if (!_selectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<EcsLocalEntity>();
                _selectedEntites.Add(world, entities);
            }
            ClearSelectedEntities();
            entities.Clear();
            entities.Add(entity);
            selectedEntityCount++;
        }

        public List<EcsLocalEntity> GetSelectedEntities(EcsWorld world)
        {
            if (!_selectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<EcsLocalEntity>();
                _selectedEntites.Add(world, entities);
                var pool = EditorMsgBus.GetPool<SelectedEnititiesChanged>();
                pool.Add(MessageId, new SelectedEnititiesChanged());
            }
            return entities;
        }

        public void RemoveSelectedEntity(EcsWorld world, EcsLocalEntity entity)
        {
            if (!_selectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<EcsLocalEntity>();
                _selectedEntites.Add(world, entities);
            }
            if (entities.Remove(entity))
            {
                selectedEntityCount--;
            }
        }

        public void ClearSelectedEntities()
        {
            foreach (var item in _selectedEntites)
            {
                item.Value.Clear();
            }
            selectedEntityCount = 0;
        }

        public void ClearSelectedEntities(EcsWorld world)
        {
            if (!_selectedEntites.TryGetValue(world, out var entities))
            {
                entities = new List<EcsLocalEntity>();
                _selectedEntites.Add(world, entities);
            }
            selectedEntityCount -= entities.Count;
            entities.Clear();
        }

        public void Init()
        {
            WindowManager.AddWindow(new WorldInspector(this), "EntityInspector", "Entities");
            WindowManager.AddWindow(new ComponentInspector(this), "ComponentInspector", "Components");
            WindowManager.AddWindow(new ProjectView(this), "ProjectView", "Project Files");
            WindowManager.AddWindow(new EditorViewport(this), "EditorViewport", "Viewport");
            if (File.Exists(statusFileName))
            {
                using var fs = new FileStream(statusFileName, FileMode.Open);
                var config = JsonSerializer.Deserialize<EditorConfig>(fs, options)!;
                foreach (var item in config.WindowStates)
                {
                    WindowManager.GetWindow(item.Key).Enabled = item.Value;
                }
            }
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
            var io = ImGui.GetIO();
            if (!io.WantCaptureKeyboard && io.KeyMods.HasFlag(ImGuiKeyModFlags.Ctrl))
            {
                if (ImGui.IsKeyPressed(ImGuiKey.Z, true))
                {
                    Console.WriteLine("Ctrl+Z Pressed");
                    ActionManager.Undo();
                }
                else if (ImGui.IsKeyPressed(ImGuiKey.Y, true))
                {
                    Console.WriteLine("Ctrl+Y Pressed");
                    ActionManager.Redo();
                }
            }
            ImGui.DockSpaceOverViewport();
            WindowManager.UpdateWindows();

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

        public void Dispose()
        {
            EditorConfig cfg = new EditorConfig();
            foreach (var item in WindowManager.NamedWindows)
            {
                cfg.WindowStates.Add(item.Key, WindowManager.GetWindow(item.Value).Enabled);
            }
            using var fs = new FileStream(statusFileName, FileMode.Create);
            JsonSerializer.Serialize(fs, cfg, options);
        }
    }
}
