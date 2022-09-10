using BrickEngine.Editor.Commands;
using BrickEngine.Editor.Gui;
using BrickEngine.Editor.Messages;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        object[]? oldVals;
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

        void CheckSelection()
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
        }

        protected override void OnUpdate()
        {
            if (BeginWindow(_title))
            {
                CheckSelection();
                foreach (var worldList in EditorManager.SelectedEntites)
                {
                    var world = worldList.Key;
                    var types = _sharedTypes[world];
                    string worldName = EditorManager.Game.AllWorldsReverse[world];
                    ImGui.PushID(worldName);
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.TextUnformatted(worldName);
                    ImGui.Separator();
                    foreach (var type in types)
                    {
                        var pool = world.GetPoolByType(type);
                        var entities = worldList.Value;
                        if (entities.Count <= 0 || pool == null)
                        {
                            continue;
                        }
                        var array = ArrayPool<object>.Shared.Rent(entities.Count);
                        int id = -1;
                        int gen = -1;
                        for (int i = 0; i < entities.Count; i++)
                        {
                            Debug.Assert(worldList.Value[i].TryUnpack(worldList.Key, out int entity));
                            array[i] = pool.GetRaw(entity);
                            id = HashCode.Combine(id, entity);
                            gen = HashCode.Combine(gen, worldList.Value[i].GetGen());
                        }

                        //Todo: set some changed flag!
                        var span = array.AsSpan().Slice(0, entities.Count);
                        ImGui.PushID(id);
                        ImGui.PushID(gen);
                        var result = DefaultInspector.DrawComponents(span, out var activeField);
                        ImGui.PopID();
                        ImGui.PopID();
                        EvalResult(result, type, activeField, span);
                        ArrayPool<object>.Shared.Return(array);
                        ImGui.Spacing();
                    }
                    ImGui.PopID();
                    ImGui.Spacing();
                }
                ImGui.Spacing();
                ImGui.Separator();
                DrawAddButton();
            }
            ImGui.End();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EvalResult(EditResult result, Type componentType, FieldInfo? activeField, Span<object> results)
        {
            if (result == EditResult.Unchanged)
            {
                return;
            }
            if (result.HasFlag(EditResult.EditStart) && activeField is not null)
            {
                oldVals = new object[EditorManager.SelectedEntites.Values.Sum(x => x.Count)];
                foreach (var worldList in EditorManager.SelectedEntites)
                {
                    var world = worldList.Key;
                    var entities = worldList.Value;
                    var pool = world.GetPoolByType(componentType)!;
                    for (int i = 0; i < entities.Count; i++)
                    {
                        worldList.Value[i].TryUnpack(worldList.Key, out int entity);
                        oldVals[i] = activeField.GetValue(pool.GetRaw(entity))!;
                    }
                }
            }
            if (result.HasFlag(EditResult.EditEnd) && activeField is not null && oldVals is not null && results.Length > 0)
            {
                var selEntities = new Dictionary<EcsWorld, EcsLocalEntity[]>(EditorManager.SelectedEntites.Count);
                foreach (var worldList in EditorManager.SelectedEntites)
                {
                    selEntities.Add(worldList.Key, worldList.Value.ToArray());
                }
                EditorManager.ActionManager.Execute(new ComponentFieldChangedCommand(selEntities, componentType, activeField, oldVals, activeField.GetValue(results[0])!));
                oldVals = null;
            }
            if (result.HasFlag(EditResult.Removed))
            {
                var selEntities = new Dictionary<EcsWorld, EcsLocalEntity[]>(EditorManager.SelectedEntites.Count);
                foreach (var worldList in EditorManager.SelectedEntites)
                {
                    selEntities.Add(worldList.Key, worldList.Value.ToArray());
                }
                EditorManager.ActionManager.Execute(new ComponentRemoveCommand(EditorManager, selEntities, componentType));
                RecalulateComponents();
            }
            if (result.HasFlag(EditResult.Changed))
            {
                foreach (var worldList in EditorManager.SelectedEntites)
                {
                    var world = worldList.Key;
                    var pool = world.GetPoolByType(componentType)!;
                    for (int i = 0; i < results.Length; i++)
                    {
                        pool.SetRaw(worldList.Value[i].Unpack(), results[i]);
                    }
                }
            }
        }

        private void DrawAddButton()
        {
            if (EditorManager.SelectedEntityCount > 0)
            {
                ImHelper.InputTextAutoComplete("Component Name", ref input, 128, out int index, ref _suggestionsOpen, _allowedTypeNames);
                if (ImGui.Button("Add") && index != -1)
                {
                    var selEntities = new Dictionary<EcsWorld, EcsLocalEntity[]>(EditorManager.SelectedEntites.Count);
                    foreach (var worldList in EditorManager.SelectedEntites)
                    {
                        selEntities.Add(worldList.Key, worldList.Value.ToArray());
                    }
                    Type componentType = _allAllowedTypes[index];
                    EditorManager.ActionManager.Execute(new ComponentAddCommand(EditorManager, selEntities, componentType));
                    RecalulateComponents();
                }
            }
        }

        private void RecalulateComponents()
        {
            _allAllowedTypesSet.Clear();
            _sharedTypes.Clear();
            bool typesFirst = true;
            Type[] allTypes = null!;
            foreach (var item in EditorManager.SelectedEntites)
            {
                var world = item.Key;
                int typeCount = world.GetAllowedTypes(ref allTypes!);
                ArraySegment<Type> allowedTypeRange = new ArraySegment<Type>(allTypes, 0, typeCount);
                if (typesFirst)
                {
                    for (int i = 0; i < allowedTypeRange.Count; i++)
                    {
                        _allAllowedTypesSet.Add(allowedTypeRange[i]);
                    }
                    typesFirst = false;
                }
                foreach (var type in _allAllowedTypesSet)
                {
                    if (!allowedTypeRange.Contains(type))
                    {
                        _droppedTypes.Add(type);
                    }
                }
            }
            foreach (var type in _droppedTypes)
            {
                _allAllowedTypesSet.Remove(type);
            }
            _droppedTypes.Clear();

            foreach (var item in EditorManager.SelectedEntites)
            {
                var world = item.Key;
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
