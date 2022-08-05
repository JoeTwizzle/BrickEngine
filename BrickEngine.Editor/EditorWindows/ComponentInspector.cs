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
        readonly HashSet<Type> _droppedTypes;
        readonly HashSet<Type> _allAllowedTypesSet;
        readonly Dictionary<EcsWorld, List<Type>> _sharedTypes;
        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;
        Type[] _allAllowedTypes;
        string[] _allowedTypeNames;
        string input;
        bool _changedInternal;
        bool _suggestionsOpen;
        public ComponentInspector(EditorManager editorManager) : base(editorManager)
        {
            input = "";
            _allAllowedTypesSet = new HashSet<Type>();
            _droppedTypes = new HashSet<Type>();
            _sharedTypes = new Dictionary<EcsWorld, List<Type>>();
            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
            _title = $"Component Inspector##{Id}";
            _allowedTypeNames = Array.Empty<string>();
            _allAllowedTypes = Array.Empty<Type>();
        }
        protected override void OnOpen()
        {
            RecalulateComponents();
        }
        protected override void OnUpdate()
        {
            if (BeginWindow(_title))
            {
                if (_changedInternal || _entityChangedPool.HasMessages)
                {
                    _changedInternal = false;
                    bool changed = false;
                    foreach (var item in EditorManager.SelectedEntites)
                    {
                        for (int i = item.Value.Count - 1; i >= 0; i--)
                        {
                            if (!item.Value[i].TryUnpack(item.Key, out _))
                            {
                                EditorManager.RemoveSelectedEntity(item.Key, item.Value[i]);
                                changed = true;
                            }
                        }
                    }
                    if (changed)
                    {
                        _changedInternal = true;
                        _entityChangedPool.Add(MessageId, new SelectedEnititiesChanged());
                    }
                    RecalulateComponents();
                }
                foreach (var worldList in EditorManager.SelectedEntites)
                {
                    var world = worldList.Key;
                    var types = _sharedTypes[world];
                    foreach (var type in types)
                    {
                        var pool = world.GetPoolByType(type);
                        if (pool == null)
                        {
                            continue;
                        }
                        var entities = worldList.Value;
                        var array = ArrayPool<object>.Shared.Rent(entities.Count);
                        for (int i = 0; i < entities.Count; i++)
                        {
                            worldList.Value[i].TryUnpack(worldList.Key, out int entity);
                            array[i] = pool.GetRaw(entity);
                        }
                        //Todo: set some changed flag!
                        var result = DefaultInspector.DrawComponents(array.AsSpan().Slice(0, entities.Count));
                        if (result == EditResult.Changed)
                        {
                            for (int i = 0; i < entities.Count; i++)
                            {
                                pool.SetRaw(worldList.Value[i].Unpack(), array[i]);
                            }
                        }
                        ArrayPool<object>.Shared.Return(array);
                        ImGui.Spacing();
                    }
                    ImGui.Spacing();
                }
                ImGui.Spacing();
                ImGui.Separator();
                if (EditorManager.SelectedEntityCount > 0)
                {
                    ImHelper.InputTextAutoComplete("Component Name", ref input, 128, out int index, ref _suggestionsOpen, _allowedTypeNames);
                    if (ImGui.Button("Add") && index != -1)
                    {
                        Type componentType = _allAllowedTypes[index];
                        foreach (var worldList in EditorManager.SelectedEntites)
                        {
                            var world = worldList.Key;
                            var entities = worldList.Value;
                            var pool = world.GetPoolByType(componentType)!;
                            for (int i = 0; i < entities.Count; i++)
                            {
                                int entity = entities[i].Unpack();
                                if (!pool.Has(entity))
                                {
                                    pool.AddRaw(entity);
                                }
                            }
                        }
                        RecalulateComponents();
                    }
                }
            }
            ImGui.End();
        }

        void RecalulateComponents()
        {
            _allAllowedTypesSet.Clear();
            _sharedTypes.Clear();
            bool typesFirst = true;
            foreach (var item in EditorManager.SelectedEntites)
            {
                var world = item.Key;
                Type[] allTypes = null!;
                int typeCount = world.GetAllowedTypes(ref allTypes!);
                if (typesFirst)
                {
                    for (int i = 0; i < typeCount; i++)
                    {
                        _allAllowedTypesSet.Add(allTypes[i]);
                    }
                    typesFirst = false;
                }
                else
                {
                    for (int i = 0; i < typeCount; i++)
                    {
                        _allAllowedTypesSet.Remove(allTypes[i]);
                    }
                }
                if (!_sharedTypes.TryGetValue(world, out var types))
                {
                    types = new List<Type>();
                    _sharedTypes.Add(world, types);
                }

                types.Clear();
                bool first = true;
                foreach (var entity in item.Value)
                {
                    Type[] entityTypes = null!;
                    int count = world.GetComponentTypes(entity.Unpack(), ref entityTypes!);
                    if (count <= 0)
                    {
                        types.Clear();
                        break;
                    }
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
                                _droppedTypes.Add(types[i]);
                            }
                        }
                    }
                }
                foreach (var type in _droppedTypes)
                {
                    types.Remove(type);
                }
                _droppedTypes.Clear();
            }
            _allAllowedTypes = _allAllowedTypesSet.ToArray();
            _allowedTypeNames = new string[_allAllowedTypesSet.Count];
            int idx = 0;
            for (int i = 0; i < _allAllowedTypes.Length; i++)
            {
                var type = _allAllowedTypes[i];
                _allowedTypeNames[idx++] = type.Name;
                for (int j = 0; j < _allowedTypeNames.Length; j++)
                {
                    if (idx - 1 != j && _allowedTypeNames[idx - 1] == _allowedTypeNames[j])
                    {
                        _allowedTypeNames[idx - 1] = type.FullName!;
                    }
                }
            }
        }
    }
}
