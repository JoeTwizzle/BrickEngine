
//using System;
//using BrickEngine.Editor.EditorWindows;
//using System.Threading.Tasks;
//using System.Runtime.CompilerServices;
//using System.Numerics;
//using BrickEngine.Editor.Messages;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using BrickEngine.Editor.Data;

//namespace BrickEngine.Editor
//{
//    public sealed class EditorManager : IDisposable
//    {
//        const string statusFileName = "EditorStatus.cfg";
//        readonly JsonSerializerOptions options = new()
//        {
//            ReferenceHandler = ReferenceHandler.Preserve,
//            WriteIndented = true,
//            Converters = { }
//        };
//        private readonly Dictionary<Type, object> _injectedSingletons;
//        private readonly Dictionary<string, object> _injected;
//        public readonly int MessageId = 0;
//        public readonly Editor Editor;
//        public readonly WindowManager WindowManager;
//        public readonly ActionManager ActionManager;
//        private int selectedEntityCount = 0;
//        public int SelectedEntityCount => selectedEntityCount;
//        public EditorManager(Editor editor)
//        {
//            ActionManager = new ActionManager();
//            _injected = new Dictionary<string, object>();
//            _injectedSingletons = new Dictionary<Type, object>();
//            Editor = editor;
//            WindowManager = new WindowManager();
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
//        public T GetSingleton<T>()
//        {
//            return (T)_injectedSingletons[typeof(T)];
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
//        public T GetInjected<T>(string identifier)
//        {
//            return (T)_injected[identifier];
//        }

//        public void AddSelectedEntity(EcsWorld world, EcsLocalEntity entity)
//        {
//            if (!_selectedEntites.TryGetValue(world, out var entities))
//            {
//                entities = new List<EcsLocalEntity>();
//                _selectedEntites.Add(world, entities);
//            }
//            entities.Add(entity);
//            selectedEntityCount++;
//        }

//        public void SetSelectedEntity(EcsWorld world, EcsLocalEntity entity)
//        {
//            if (!_selectedEntites.TryGetValue(world, out var entities))
//            {
//                entities = new List<EcsLocalEntity>();
//                _selectedEntites.Add(world, entities);
//            }
//            ClearSelectedEntities();
//            entities.Clear();
//            entities.Add(entity);
//            selectedEntityCount++;
//        }

//        public List<EcsLocalEntity> GetSelectedEntities(EcsWorld world)
//        {
//            if (!_selectedEntites.TryGetValue(world, out var entities))
//            {
//                entities = new List<EcsLocalEntity>();
//                _selectedEntites.Add(world, entities);
//                var pool = EditorMsgBus.GetPool<SelectedEnititiesChanged>();
//                pool.Add(MessageId, new SelectedEnititiesChanged());
//            }
//            return entities;
//        }

//        public void RemoveSelectedEntity(EcsWorld world, EcsLocalEntity entity)
//        {
//            if (!_selectedEntites.TryGetValue(world, out var entities))
//            {
//                entities = new List<EcsLocalEntity>();
//                _selectedEntites.Add(world, entities);
//            }
//            if (entities.Remove(entity))
//            {
//                selectedEntityCount--;
//            }
//        }

//        public void ClearSelectedEntities()
//        {
//            foreach (var item in _selectedEntites)
//            {
//                item.Value.Clear();
//            }
//            selectedEntityCount = 0;
//        }

//        public void ClearSelectedEntities(EcsWorld world)
//        {
//            if (!_selectedEntites.TryGetValue(world, out var entities))
//            {
//                entities = new List<EcsLocalEntity>();
//                _selectedEntites.Add(world, entities);
//            }
//            selectedEntityCount -= entities.Count;
//            entities.Clear();
//        }

//        public void Init()
//        {
//            WindowManager.AddWindow(new WorldInspector(this), "EntityInspector", "Entities");
//            WindowManager.AddWindow(new ComponentInspector(this), "ComponentInspector", "Components");
//            WindowManager.AddWindow(new ProjectView(this), "ProjectView", "Project Files");
//            WindowManager.AddWindow(new EditorViewport(this), "EditorViewport", "Viewport");
//            if (File.Exists(statusFileName))
//            {
//                using var fs = new FileStream(statusFileName, FileMode.Open);
//                var config = JsonSerializer.Deserialize<EditorConfig>(fs, options)!;
//                foreach (var item in config.WindowStates)
//                {
//                    WindowManager.GetWindow(item.Key).Enabled = item.Value;
//                }
//            }
//        }

//        public void Inject<T>(string identifier, T data) where T : notnull
//        {
//            _injected.Add(identifier, data);
//        }

//        public void InjectSingleton<T>(T data) where T : notnull
//        {
//            _injectedSingletons.Add(typeof(T), data);
//        }

//        public void Update()
//        {
//            foreach (var pool in EditorMsgBus.Pools)
//            {
//                pool.Value.RemoveMessages(MessageId);
//            }
//            var io = ImGui.GetIO();
//            if (!io.WantCaptureKeyboard && io.KeyMods.HasFlag(ImGuiKey.ImGuiMod_Ctrl))
//            {
//                if (ImGui.IsKeyPressed(ImGuiKey.Z, true))
//                {
//                    Console.WriteLine("Ctrl+Z Pressed");
//                    ActionManager.Undo();
//                }
//                else if (ImGui.IsKeyPressed(ImGuiKey.Y, true))
//                {
//                    Console.WriteLine("Ctrl+Y Pressed");
//                    ActionManager.Redo();
//                }
//            }
//            ImGui.DockSpaceOverViewport();
//            WindowManager.UpdateWindows();

//            var size = ImGui.GetWindowSize();
//            float menuBarHeight = 0;
//            if (ImGui.BeginMainMenuBar())
//            {
//                menuBarHeight = ImGui.GetWindowSize().Y;
//                foreach (KeyValuePair<string, Tree<int>.TreeNode> node in WindowManager.TitleBarWindows.RootNode.Nodes)
//                {
//                    DrawMenu(node.Key, node.Value);
//                }
//            }
//            ImGui.EndMainMenuBar();
//        }

//        void DrawMenu(string key, Tree<int>.TreeNode baseNode)
//        {
//            if (baseNode.IsLeaf)
//            {
//                if (ImGui.MenuItem(key))
//                {
//                    WindowManager.GetWindow(baseNode.Value).Enabled = true;
//                }
//            }
//            else
//            {
//                if (ImGui.BeginMenu(key))
//                {
//                    foreach (var node in baseNode.Nodes)
//                    {
//                        DrawMenu($"{node.Key}##{key}", node.Value);
//                    }
//                    ImGui.EndMenu();
//                }
//            }
//        }

//        public void Dispose()
//        {
//            var cfg = new EditorConfig();
//            foreach (var item in WindowManager.NamedWindows)
//            {
//                cfg.WindowStates.Add(item.Key, WindowManager.GetWindow(item.Value).Enabled);
//            }
//            using var fs = new FileStream(statusFileName, FileMode.Create);
//            JsonSerializer.Serialize(fs, cfg, options);
//            using var fs2 = new FileStream("World.dmp", FileMode.Create);
//            using var writer = new Utf8JsonWriter(fs2);
//            var c = new GigaTestComponent();
//            c.Init();
//            JsonTextSerializer.Serialize(c, typeof(GigaTestComponent), writer);
//        }
//    }
//}
