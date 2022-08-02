using BrickEngine.Editor.Gui;
using BrickEngine.Editor.Messages;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.EditorWindows
{
    internal class ComponentInspector : WindowBase
    {
        readonly string _title;
        readonly HashSet<Type> droppedTypes;
        readonly Dictionary<EcsWorld, List<Type>> SharedTypes;
        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;
        public ComponentInspector(EditorManager editorManager) : base(editorManager)
        {
            droppedTypes = new HashSet<Type>();
            SharedTypes = new Dictionary<EcsWorld, List<Type>>();
            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
            _title = $"Component Inspector##{Id}";
        }
        protected override void OnOpen()
        {
            RecalulateComponents();
        }
        protected override void OnUpdate()
        {
            if (BeginWindow(_title))
            {
                if (_entityChangedPool.HasMessages)
                {
                    RecalulateComponents();
                }
                foreach (var item in EditorManager.SelectedEntites)
                {
                    var world = item.Key;
                    var types = SharedTypes[world];
                    foreach (var type in types)
                    {
                        var pool = world.GetPoolByType(type);
                        if (pool == null)
                        {
                            continue;
                        }
                        var entities = item.Value;
                        var array = ArrayPool<object>.Shared.Rent(entities.Count);
                        for (int i = 0; i < entities.Count; i++)
                        {
                            array[i] = pool.GetRaw(entities[i]);
                        }
                        //Todo: set some changed flag!
                        bool changed = DefaultInspector.DrawComponents(array.AsSpan().Slice(0, entities.Count));
                        if (changed)
                        {
                            for (int i = 0; i < entities.Count; i++)
                            {
                                pool.SetRaw(entities[i], array[i]);
                            }
                        }
                        ArrayPool<object>.Shared.Return(array);
                    }
                    ImGui.Spacing();
                }
            }
            ImGui.End();
        }

        void RecalulateComponents()
        {
            SharedTypes.Clear();
            foreach (var item in EditorManager.SelectedEntites)
            {
                var world = item.Key;
                if (!SharedTypes.TryGetValue(world, out var types))
                {
                    types = new List<Type>();
                    SharedTypes.Add(world, types);
                }
                types.Clear();
                bool first = true;
                foreach (var entity in item.Value)
                {
                    Type[] entityTypes = null!;
                    int count = world.GetComponentTypes(entity, ref entityTypes);
                    if (count <= 0)
                        break;
                    var usedRange = new ArraySegment<Type>(entityTypes, 0, count);
                    if (first)
                    {
                        first = false;
                        types.AddRange(usedRange);
                    }
                    else
                    {
                        for (int i = 0; i < types.Count; i++)
                        {
                            if (!usedRange.Contains(types[i]))
                            {
                                droppedTypes.Add(types[i]);
                            }
                        }
                    }
                }
                foreach (var type in droppedTypes)
                {
                    types.Remove(type);
                }
                droppedTypes.Clear();
            }
        }
    }
}
