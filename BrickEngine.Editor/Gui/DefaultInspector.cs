﻿using BrickEngine.Core.Mathematics;
using BrickEngine.Editor.Gui.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Gui
{
    public static class DefaultInspector
    {
        static Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        static readonly Dictionary<Type, int> typeRenderers;
        private unsafe static readonly delegate*<FieldInfo, Span<object>, bool>[] functions;
        static List<FieldInfo> filteredFields = new List<FieldInfo>();
        static DefaultInspector()
        {
            typeRenderers = new Dictionary<Type, int>()
            {
                {typeof(string)     , 0  },
                {typeof(int)        , 1  },
                {typeof(Vector2i)   , 2  },
                {typeof(Vector3i)   , 3  },
                {typeof(Vector4i)   , 4  },
                {typeof(float)      , 5  },
                {typeof(Vector2)    , 6  },
                {typeof(Vector3)    , 7  },
                {typeof(Vector4)    , 8  },
                {typeof(Quaternion) , 9  },
                {typeof(double)     , 10  },
                {typeof(bool)       , 11 },
            };
            unsafe
            {
                functions = new delegate*<FieldInfo, Span<object>, bool>[]
                {
                    &DrawText    ,
                    &DrawInt     ,
                    &DrawInt2    ,
                    &DrawInt3    ,
                    &DrawInt4    ,
                    &DrawFloat   ,
                    &DrawFloat2  ,
                    &DrawFloat3  ,
                    &DrawFloat4  ,
                    &DrawFloat4  ,
                    &DrawDouble  ,
                    &DrawBool    ,
                };
            }
        }

        public static void PurgeCache()
        {
            fieldCache.Clear();
        }

        private static FieldInfo[] GetTypeCache(Type t)
        {
            if (!fieldCache.TryGetValue(t, out var fieldsCached))
            {
                filteredFields.Clear();
                var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].IsPublic && !Attribute.IsDefined(fields[i], typeof(EditorIgnoreAttribute)))
                    {
                        filteredFields.Add(fields[i]);
                    }
                    else if (!fields[i].IsPublic && Attribute.IsDefined(fields[i], typeof(EditableAttribute)))
                    {
                        filteredFields.Add(fields[i]);
                    }
                }
                fieldsCached = filteredFields.ToArray();
                fieldCache.Add(t, fieldsCached);
            }
            return fieldsCached;
        }

        public static bool DrawComponents(Span<object> components)
        {
            if (components.Length <= 0)
            {
                return false;
            }
            var t = components[0].GetType();
            var fieldsCached = GetTypeCache(t);
            bool anyChanged = false;
            if (fieldsCached.Length <= 0)
            {
                return false;
            }
            if (ImGui.CollapsingHeader(t.Name, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Indent();
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
                foreach (var field in fieldsCached)
                {
                    Type nonGenericType = field.FieldType.IsGenericType ? field.FieldType.GetGenericTypeDefinition() : field.FieldType;
                    if (typeRenderers.TryGetValue(nonGenericType, out var funcIndex))
                    {
                        unsafe
                        {
                            anyChanged |= functions[funcIndex](field, components);
                        }
                    }
                    else
                    {
                        //ImGui.TextUnformatted(field.Name);
                    }
                }
                ImGui.PopStyleVar();
                ImGui.Unindent();
            }
            return anyChanged;
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
            var data = ((sizeof(int) * components.Length) < 1024) ? stackalloc int[components.Length] : new int[components.Length];
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
                    dirty = ImHelper.MultiDragInt(field.Name, data, dragAttribute.Speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragInt(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector2i>() * components.Length) < 1024) ? stackalloc Vector2i[components.Length] : new Vector2i[components.Length];
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
                    dirty = ImHelper.MultiDragInt2(field.Name, data, dragAttribute.Speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt2(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragInt2(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector3i>() * components.Length) < 1024) ? stackalloc Vector3i[components.Length] : new Vector3i[components.Length];
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
                    dirty = ImHelper.MultiDragInt3(field.Name, data, dragAttribute.Speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt3(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragInt3(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector4i>() * components.Length) < 1024) ? stackalloc Vector4i[components.Length] : new Vector4i[components.Length];
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
                    dirty = ImHelper.MultiDragInt4(field.Name, data, dragAttribute.Speed, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderInt4(field.Name, data, (int)rangeAttribute.Min, (int)rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragInt4(field.Name, data, dragAttribute.Speed);
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
            var data = ((sizeof(float) * components.Length) < 1024) ? stackalloc float[components.Length] : new float[components.Length];
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
                    dirty = ImHelper.MultiDragFloat(field.Name, data, dragAttribute.Speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragFloat(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector2>() * components.Length) < 1024) ? stackalloc Vector2[components.Length] : new Vector2[components.Length];
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
                    dirty = ImHelper.MultiDragFloat2(field.Name, data, dragAttribute.Speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat2(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragFloat2(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector3>() * components.Length) < 1024) ? stackalloc Vector3[components.Length] : new Vector3[components.Length];
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
                    dirty = ImHelper.MultiDragFloat3(field.Name, data, dragAttribute.Speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat3(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragFloat3(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector4>() * components.Length) < 1024) ? stackalloc Vector4[components.Length] : new Vector4[components.Length];
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
                    dirty = ImHelper.MultiDragFloat4(field.Name, data, dragAttribute.Speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat4(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragFloat4(field.Name, data, dragAttribute.Speed);
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
            var data = ((Unsafe.SizeOf<Vector4>() * components.Length) < 1024) ? stackalloc Vector4[components.Length] : new Vector4[components.Length];
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
                    dirty = ImHelper.MultiDragFloat4(field.Name, data, dragAttribute.Speed, rangeAttribute.Min, rangeAttribute.Max);
                }
                else
                {
                    dirty = ImHelper.MultiSliderFloat4(field.Name, data, rangeAttribute.Min, rangeAttribute.Max);
                }
            }
            else
            {
                var dragAttribute = field.GetCustomAttribute<DragAttribute>();
                if (dragAttribute != null)
                {
                    dirty = ImHelper.MultiDragFloat4(field.Name, data, dragAttribute.Speed);
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
            var data = ((sizeof(double) * components.Length) < 1024) ? stackalloc double[components.Length] : new double[components.Length];
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
            var data = ((sizeof(bool) * components.Length) < 1024) ? stackalloc bool[components.Length] : new bool[components.Length];
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
