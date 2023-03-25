using BrickEngine.Core.Mathematics;
using BrickEngine.Core.Utilities;
using BrickEngine.Editor.Gui.Attributes;
using System.Buffers;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BrickEngine.Editor.Gui
{
    [Flags]
    public enum EditResult
    {
        Unchanged = 0,
        Changed = 1,
        Removed = 2,
        EditStart = 4,
        EditEnd = 8,
    }
    public static class DefaultInspector
    {
        const int maxStack = 512;

        static readonly Dictionary<Type, int> typeRenderers;
        private static readonly unsafe delegate*<FieldInfo, Span<object>, bool>[] functions;

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igClearActiveID();
        static DefaultInspector()
        {
            typeRenderers = new Dictionary<Type, int>()
            {
                {typeof(string)         , 0  },
                {typeof(int)            , 1  },
                {typeof(Vector2i)       , 2  },
                {typeof(Vector3i)       , 3  },
                {typeof(Vector4i)       , 4  },
                {typeof(float)          , 5  },
                {typeof(Vector2)        , 6  },
                {typeof(Vector3)        , 7  },
                {typeof(Vector4)        , 8  },
                {typeof(Quaternion)     , 9  },
                {typeof(double)         , 10 },
                {typeof(bool)           , 11 },
                //{typeof(List<>)         , 12 },
                //{typeof(HashSet<>)      , 13 },
                //{typeof(Dictionary<,>)  , 14 },
                //{typeof(Array)          , 15 },
            };
            unsafe
            {
                functions = new delegate*<FieldInfo, Span<object>, bool>[]
                {
                    &DrawText       ,
                    &DrawInt        ,
                    &DrawInt2       ,
                    &DrawInt3       ,
                    &DrawInt4       ,
                    &DrawFloat      ,
                    &DrawFloat2     ,
                    &DrawFloat3     ,
                    &DrawFloat4     ,
                    &DrawQuaternion ,
                    &DrawDouble     ,
                    &DrawBool       ,
                };
            }
        }

        public static EditResult DrawComponents(Span<object> components, out FieldInfo? activefield)
        {
            activefield = null;
            if (components.Length <= 0)
            {
                return EditResult.Unchanged;
            }
            var t = components[0].GetType();
            bool open = ImGui.CollapsingHeader(t.Name, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow);
            if (ImGui.BeginPopupContextItem(t.Name, ImGuiPopupFlags.MouseButtonRight))
            {
                if (ImGui.MenuItem("Remove"))
                {
                    ImGui.EndPopup();
                    return EditResult.Removed;
                }
                ImGui.EndPopup();
            }
            var fieldsCached = ReflectionCache.GetCacheForType(t);
            if (fieldsCached.Length <= 0)
            {
                return EditResult.Unchanged;
            }
            EditResult result = EditResult.Unchanged;
            if (open)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
                for (int i = 0; i < fieldsCached.Length; i++)
                {
                    var field = fieldsCached[i];
                    Type nonGenericType = field.FieldType.IsGenericType ? field.FieldType.GetGenericTypeDefinition() : field.FieldType;
                    if (typeRenderers.TryGetValue(nonGenericType, out var funcIndex))
                    {
                        EditResult currentResult;
                        unsafe
                        {
                            currentResult = functions[funcIndex](field, components) ? EditResult.Changed : EditResult.Unchanged;
                        }
                        if (ImGui.IsItemActivated())
                        {
                            activefield = field;
                            currentResult |= EditResult.EditStart;
                        }
                        if (ImGui.IsItemActive())
                        {
                            activefield = field;
                        }
                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            activefield = field;
                            currentResult |= EditResult.EditEnd;
                        }
                        result |= currentResult;
                    }
                    else
                    {
                        ImGui.TextUnformatted("TODO: " + field.Name);
                    }
                }
                ImGui.PopStyleVar();
            }
            return result;
        }

        public static bool DrawText(FieldInfo field, Span<object> components)
        {
            var data = ArrayPool<string>.Shared.Rent(components.Length);
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (string)field.GetValue(components[i])!;
            }

            var multiline = field.GetCustomAttribute(typeof(MultilineAttribute), false) != null;
            uint length = field.GetCustomAttribute<TextLengthAttribute>()?.Length ?? 1024;
            bool dirty;
            if (multiline)
            {
                float spaceX = ImGui.GetContentRegionAvail().X;

                dirty = ImHelper.MultiInputTextMultiline(field.Name, data.AsSpan(0, components.Length), length, new Vector2(spaceX, 120));
            }
            else
            {
                dirty = ImHelper.MultiInputText(field.Name, data.AsSpan(0, components.Length), length);
            }
            if (dirty)
            {
                Console.WriteLine("Dirty");
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            ArrayPool<string>.Shared.Return(data);
            return dirty;
        }

        public static bool DrawInt(FieldInfo field, Span<object> components)
        {
            var data = ((sizeof(int) * components.Length) < maxStack) ? stackalloc int[components.Length] : new int[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (int)field.GetValue(components[i])!;
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;
                    ImGui.IsItemDeactivatedAfterEdit();
                    dirty = ImHelper.MultiDragInt(field.Name, data, speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragInt(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputInt(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawInt2(FieldInfo field, Span<object> components)
        {
            var data = ((Unsafe.SizeOf<Vector2i>() * components.Length) < maxStack) ? stackalloc Vector2i[components.Length] : new Vector2i[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (Vector2i)field.GetValue(components[i])!;
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragInt2(field.Name, data, speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt2(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragInt2(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputInt2(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawInt3(FieldInfo field, Span<object> components)
        {
            var data = ((Unsafe.SizeOf<Vector3i>() * components.Length) < maxStack) ? stackalloc Vector3i[components.Length] : new Vector3i[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (Vector3i)field.GetValue(components[i])!;
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragInt3(field.Name, data, speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt3(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;
                    dirty = ImHelper.MultiDragInt3(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputInt3(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawInt4(FieldInfo field, Span<object> components)
        {
            var data = ((Unsafe.SizeOf<Vector4i>() * components.Length) < maxStack) ? stackalloc Vector4i[components.Length] : new Vector4i[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (Vector4i)field.GetValue(components[i])!;
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragInt4(field.Name, data, speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt4(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragInt4(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputInt4(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawFloat(FieldInfo field, Span<object> components)
        {
            var data = ((sizeof(float) * components.Length) < maxStack) ? stackalloc float[components.Length] : new float[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (float)field.GetValue(components[i])!;
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragFloat(field.Name, data, speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragFloat(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputFloat(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawFloat2(FieldInfo field, Span<object> components)
        {
            var data = ((Unsafe.SizeOf<Vector2>() * components.Length) < maxStack) ? stackalloc Vector2[components.Length] : new Vector2[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (Vector2)field.GetValue(components[i])!;
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragFloat2(field.Name, data, speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat2(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragFloat2(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputFloat2(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawFloat3(FieldInfo field, Span<object> components)
        {
            var pos = ImGui.GetCursorPos();
            var data = ((Unsafe.SizeOf<Vector3>() * components.Length) < maxStack) ? stackalloc Vector3[components.Length] : new Vector3[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (Vector3)field.GetValue(components[i])!;
            }
            bool dirty;

            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragFloat3(field.Name, data, speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat3(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragFloat3(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputFloat3(field.Name, data);
                }
            }
            var colorPickerAttribute = field.GetCustomAttribute<ColorPickerAttribute>();
            if (colorPickerAttribute != null)
            {
                ImGui.SetCursorPos(pos + new Vector2(ImGui.CalcTextSize(field.Name).X + 5, 0));
                dirty = ImGui.ColorEdit3($"##{field.Name}", ref data[0], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | colorPickerAttribute.Flags);

                if (dirty)
                {
                    for (int i = 1; i < data.Length; i++)
                    {
                        data[i] = data[0];
                    }
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawFloat4(FieldInfo field, Span<object> components)
        {
            var data = ((Unsafe.SizeOf<Vector4>() * components.Length) < maxStack) ? stackalloc Vector4[components.Length] : new Vector4[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (Vector4)field.GetValue(components[i])!;
            }
            bool dirty;
            var pos = ImGui.GetCursorPos();
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragFloat4(field.Name, data, speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat4(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragFloat4(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputFloat4(field.Name, data);
                }
            }
            var colorPickerAttribute = field.GetCustomAttribute<ColorPickerAttribute>();
            if (colorPickerAttribute != null)
            {
                ImGui.SetCursorPos(pos + new Vector2(ImGui.CalcTextSize(field.Name).X + 5, 0));
                dirty = ImGui.ColorEdit4($"##{field.Name}", ref data[0], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | colorPickerAttribute.Flags);
                if (dirty)
                {
                    for (int i = 1; i < data.Length; i++)
                    {
                        data[i] = data[0];
                    }
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }
        public static bool DrawQuaternion(FieldInfo field, Span<object> components)
        {
            var data = ((Unsafe.SizeOf<Vector4>() * components.Length) < maxStack) ? stackalloc Vector4[components.Length] : new Vector4[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                var quat = (Quaternion)field.GetValue(components[i])!;
                data[i] = new Vector4(quat.X, quat.Y, quat.Z, quat.W);
            }
            var rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            bool dirty;
            if (rangeAttribute != null)
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    var speed = dragAttribute.Speed;

                    dirty = ImHelper.MultiDragFloat4(field.Name, data, speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat4(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var noDragAttribute = field.GetCustomAttribute<NoDragAttribute>();
                if (noDragAttribute == null)
                {
                    var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                    var speed = dragAttribute?.Speed ?? 0.1f;

                    dirty = ImHelper.MultiDragFloat4(field.Name, data, speed);
                }
                else
                {
                    dirty = ImHelper.MultiInputFloat4(field.Name, data);
                }
            }
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    ref var v = ref data[i];
                    field.SetValue(components[i], new Quaternion(v.X, v.Y, v.Z, v.W));
                }
            }
            return dirty;
        }

        public static bool DrawDouble(FieldInfo field, Span<object> components)
        {
            var data = ((sizeof(double) * components.Length) < maxStack) ? stackalloc double[components.Length] : new double[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (double)field.GetValue(components[i])!;
            }
            bool dirty = ImHelper.MultiInputDouble(field.Name, data);
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }

        public static bool DrawBool(FieldInfo field, Span<object> components)
        {
            var data = ((sizeof(bool) * components.Length) < maxStack) ? stackalloc bool[components.Length] : new bool[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                data[i] = (bool)field.GetValue(components[i])!;
            }
            bool dirty = ImHelper.MultiCheckbox(field.Name, data);
            if (dirty)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    field.SetValue(components[i], data[i]);
                }
            }
            return dirty;
        }
    }
}
