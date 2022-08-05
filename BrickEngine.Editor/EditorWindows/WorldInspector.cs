﻿using BrickEngine.Editor.Gui;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using BrickEngine.Editor.Messages;

namespace BrickEngine.Editor.EditorWindows
{
    internal class WorldInspector : WindowBase
    {
        readonly Game _game;
        readonly string _title;
        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;
        public WorldInspector(EditorManager editorManager) : base(editorManager)
        {
            _title = $"EntityInspector##{Id}";
            _game = EditorManager.GetSingleton<Game>();
            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
        }
        EcsLocalEntity[]? entities;
        int selectedEntity = -1;
        int prevSelectedEntity = 0;
        int prevWorldIndex = -1;
        protected override void OnUpdate()
        {
            if (BeginWindow(_title))
            {
                if (ImGui.BeginTabBar($"Worlds##{Id}"))
                {
                    int worldIndex = 0;
                    foreach (var item in _game.AllWorlds)
                    {
                        if (ImGui.BeginTabItem(item.Key))
                        {
                            if (prevWorldIndex != worldIndex)
                            {
                                prevWorldIndex = worldIndex;
                                selectedEntity = -1;
                            }
                            DrawWorldEntities(item.Value);
                            ImGui.EndTabItem();
                        }
                        worldIndex++;
                    }
                }
                ImGui.EndTabBar();
            }
            ImGui.End();
        }
        DelayedAdd delayedAdd;
        void DrawWorldEntities(EcsWorld world)
        {
            var selectedEnities = EditorManager.GetSelectedEntities(world);
            int count = world.GetAllPackedLocalEntities(ref entities);
            bool ctrlPressed = ImGui.IsKeyDown((int)Veldrid.Key.LeftControl) || ImGui.IsKeyDown((int)Veldrid.Key.RightControl);
            bool shiftPressed = ImGui.IsKeyDown((int)Veldrid.Key.LeftShift) || ImGui.IsKeyDown((int)Veldrid.Key.RightShift);
            for (int i = 0; i < count; i++)
            {
                var entity = entities![i];
                if (!entity.TryUnpack(world, out int entityId))
                {
                    continue;
                }
                bool isSelected = selectedEnities.Contains(entity);
                if (isSelected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, GuiColors.SelectedBlue);
                }

                if (ImGui.Selectable($"ID: {entityId + 1}"))
                {
                    prevSelectedEntity = selectedEntity;
                    if (prevSelectedEntity < 0)
                    {
                        prevSelectedEntity = 0;
                    }
                    selectedEntity = entityId;
                    if (ctrlPressed)
                    {
                        if (shiftPressed)
                        {
                            int start = Math.Min(prevSelectedEntity, selectedEntity);
                            int end = Math.Max(prevSelectedEntity, selectedEntity);
                            delayedAdd = new DelayedAdd(true, new Range(start, end));
                        }
                        else
                        {
                            EditorManager.AddSelectedEntity(world, entity);
                        }
                    }
                    else
                    {
                        if (shiftPressed)
                        {
                            int start = Math.Min(prevSelectedEntity, selectedEntity);
                            int end = Math.Max(prevSelectedEntity, selectedEntity);
                            delayedAdd = new DelayedAdd(false, new Range(start, end));
                        }
                        else
                        {
                            EditorManager.SetSelectedEntity(world, entity);
                        }
                    }
                    _entityChangedPool.Add(MessageId, new SelectedEnititiesChanged());
                }
                if (isSelected)
                {
                    ImGui.PopStyleColor();
                }
            }
            if (delayedAdd != default)
            {
                if (!delayedAdd.Additive)
                {
                    EditorManager.ClearSelectedEntities(world);
                }
                for (int i = 0; i < count; i++)
                {
                    var entity = entities![i];
                    if (!entity.TryUnpack(world, out int entityId))
                    {
                        continue;
                    }
                    if (entityId >= delayedAdd.SelectedEntityRange.Start.Value && entityId <= delayedAdd.SelectedEntityRange.End.Value)
                    {
                        EditorManager.AddSelectedEntity(world, entity);
                    }
                }
                delayedAdd = default;
            }
        }

        readonly struct DelayedAdd : IEquatable<DelayedAdd>
        {
            public readonly bool Additive;
            public readonly Range SelectedEntityRange;

            public DelayedAdd(bool additive, Range selectedEntityRange)
            {
                Additive = additive;
                SelectedEntityRange = selectedEntityRange;
            }

            public override bool Equals(object? obj)
            {
                return obj is DelayedAdd add &&
                       Additive == add.Additive &&
                       SelectedEntityRange.Equals(add.SelectedEntityRange);
            }

            public bool Equals(DelayedAdd other)
            {
                return Additive == other.Additive &&
                       SelectedEntityRange.Equals(other.SelectedEntityRange);
            }
            public static bool operator ==(DelayedAdd lhs, DelayedAdd rhs)
            {
                return lhs.Additive == rhs.Additive &&
                       lhs.SelectedEntityRange.Equals(rhs.SelectedEntityRange);
            }
            public static bool operator !=(DelayedAdd lhs, DelayedAdd rhs)
            {
                return !(lhs == rhs);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Additive, SelectedEntityRange);
            }
        }
    }
}
