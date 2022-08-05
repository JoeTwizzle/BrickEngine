using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using BrickEngine.Core.Mathematics;

namespace BrickEngine.Editor.Gui
{
    public static partial class ImHelper
    {
        private const float StartPercentage = 0.4f;
        private readonly static Vector4 DifferentColor = new Vector4(217 / 255f, 24 / 255f, 17 / 255f, 1);
        #region DragInt
        public static bool MultiDragInt(string label, Span<int> vals, float speed, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }
            int prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {
                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.DragInt($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }

            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiDragInt2(string label, Span<Vector2i> vals, float speed, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector2 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                prev = vals[i];
                if (diffX && diffY)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 2;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());


            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.DragInt($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.DragInt($" ##{label}Y", ref y, speed, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged;
        }
        public static bool MultiDragInt3(string label, Span<Vector3i> vals, float speed, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector3 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                prev = vals[i];
                if (diffX && diffY && diffZ)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            int z = diffZ ? default : vals[0].Z;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 3;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.DragInt($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool yChanged = ImGui.DragInt($" ##{label}Y", ref y, speed, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool zChanged = ImGui.DragInt($" ##{label}Z", ref z, speed, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged;
        }
        public static bool MultiDragInt4(string label, Span<Vector4i> vals, float speed, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector4 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            bool diffW = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                diffW |= prev.W != vals[i].W;
                prev = vals[i];
                if (diffX && diffY && diffZ && diffW)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            int z = diffZ ? default : vals[0].Z;
            int w = diffW ? default : vals[0].W;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 4;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / 4;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.SetColumnWidth(4, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.DragInt($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool yChanged = ImGui.DragInt($" ##{label}Y", ref y, speed, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool zChanged = ImGui.DragInt($" ##{label}Z", ref z, speed, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool wChanged = ImGui.DragInt($" ##{label}W", ref w, speed, minVal, maxVal);
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
                if (wChanged)
                {
                    vec.W = w;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged || wChanged;
        }

        #endregion
        #region SliderInt
        public static bool MultiSliderInt(string label, Span<int> vals, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            int prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.SliderInt($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }

            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiSliderInt2(string label, Span<Vector2i> vals, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector2i prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                prev = vals[i];
                if (diffX && diffY)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 2;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.SliderInt($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool yChanged = ImGui.SliderInt($" ##{label}Y", ref y, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged;
        }
        public static bool MultiSliderInt3(string label, Span<Vector3i> vals, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector3i prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                prev = vals[i];
                if (diffX && diffY && diffZ)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            int z = diffZ ? default : vals[0].Z;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 3;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.SliderInt($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool yChanged = ImGui.SliderInt($" ##{label}Y", ref y, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool zChanged = ImGui.SliderInt($" ##{label}Z", ref z, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged;
        }
        public static bool MultiSliderInt4(string label, Span<Vector4i> vals, int minVal = int.MinValue, int maxVal = int.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector4i prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            bool diffW = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                diffW |= prev.W != vals[i].W;
                prev = vals[i];
                if (diffX && diffY && diffZ && diffW)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            int z = diffZ ? default : vals[0].Z;
            int w = diffW ? default : vals[0].W;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 4;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / 4;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.SetColumnWidth(4, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.SliderInt($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool yChanged = ImGui.SliderInt($" ##{label}Y", ref y, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool zChanged = ImGui.SliderInt($" ##{label}Z", ref z, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool wChanged = ImGui.SliderInt($" ##{label}W", ref w, minVal, maxVal);
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
                if (wChanged)
                {
                    vec.W = w;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged || wChanged;
        }
        #endregion
        #region InputInt
        public static bool MultiInputInt(string label, Span<int> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            int prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputInt($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }

            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiInputInt2(string label, Span<Vector2i> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector2i prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                prev = vals[i];
                if (diffX && diffY)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 2;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputInt($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.InputInt($" ##{label}Y", ref y);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }

            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
            }
            ImGui.Columns(1);
            return xChanged || yChanged;
        }
        public static bool MultiInputInt3(string label, Span<Vector3i> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector3i prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                prev = vals[i];
                if (diffX && diffY && diffZ)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            int z = diffZ ? default : vals[0].Z;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 3;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputInt($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.InputInt($" ##{label}Y", ref y);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.InputInt($" ##{label}Z", ref z);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
            }
            ImGui.Columns(1);
            return xChanged || yChanged || zChanged;
        }
        public static bool MultiInputInt4(string label, Span<Vector4i> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector4i prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            bool diffW = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                diffW |= prev.W != vals[i].W;
                prev = vals[i];
                if (diffX && diffY && diffZ && diffW)
                {
                    break;
                }
            }
            int x = diffX ? default : vals[0].X;
            int y = diffY ? default : vals[0].Y;
            int z = diffZ ? default : vals[0].Z;
            int w = diffW ? default : vals[0].W;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 4;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / 4;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.SetColumnWidth(4, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputInt($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.InputInt($" ##{label}Y", ref y);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.InputInt($" ##{label}Z", ref z);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool wChanged = ImGui.InputInt($" ##{label}W", ref w);
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
                if (wChanged)
                {
                    vec.W = w;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged || wChanged;
        }
        #endregion
        #region DragFloat
        public static bool MultiDragFloat(string label, Span<float> vals, float speed, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }
            float prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {
                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.DragFloat($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }

            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiDragFloat2(string label, Span<Vector2> vals, float speed, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector2 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                prev = vals[i];
                if (diffX && diffY)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 2;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool xChanged = ImGui.DragFloat($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            bool yChanged = ImGui.DragFloat($" ##{label}Y", ref y, speed, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged;
        }
        public static bool MultiDragFloat3(string label, Span<Vector3> vals, float speed, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector3 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                prev = vals[i];
                if (diffX && diffY && diffZ)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float z = diffZ ? default : vals[0].Z;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 3;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.DragFloat($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.DragFloat($" ##{label}Y", ref y, speed, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.DragFloat($" ##{label}Z", ref z, speed, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged;
        }
        public static bool MultiDragFloat4(string label, Span<Vector4> vals, float speed, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector4 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            bool diffW = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                diffW |= prev.W != vals[i].W;
                prev = vals[i];
                if (diffX && diffY && diffZ && diffW)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float z = diffZ ? default : vals[0].Z;
            float w = diffW ? default : vals[0].W;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 4;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / 4;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.SetColumnWidth(4, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.DragFloat($" ##{label}X", ref x, speed, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.DragFloat($" ##{label}Y", ref y, speed, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.DragFloat($" ##{label}Z", ref z, speed, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool wChanged = ImGui.DragFloat($" ##{label}W", ref w, speed, minVal, maxVal);
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
                if (wChanged)
                {
                    vec.W = w;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged || wChanged;
        }
        #endregion
        #region SliderFloat
        public static bool MultiSliderFloat(string label, Span<float> vals, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            float prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.SliderFloat($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }

            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiSliderFloat2(string label, Span<Vector2> vals, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector2 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                prev = vals[i];
                if (diffX && diffY)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 2;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.SliderFloat($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.SliderFloat($" ##{label}Y", ref y, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged;
        }
        public static bool MultiSliderFloat3(string label, Span<Vector3> vals, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector3 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                prev = vals[i];
                if (diffX && diffY && diffZ)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float z = diffZ ? default : vals[0].Z;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 3;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.SliderFloat($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.SliderFloat($" ##{label}Y", ref y, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.SliderFloat($" ##{label}Z", ref z, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
            }

            ImGui.Columns(1);
            return xChanged || yChanged || zChanged;
        }
        public static bool MultiSliderFloat4(string label, Span<Vector4> vals, float minVal = float.MinValue, float maxVal = float.MaxValue)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector4 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            bool diffW = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                diffW |= prev.W != vals[i].W;
                prev = vals[i];
                if (diffX && diffY && diffZ && diffW)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float z = diffZ ? default : vals[0].Z;
            float w = diffW ? default : vals[0].W;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 4;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / 4;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.SetColumnWidth(4, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.SliderFloat($" ##{label}X", ref x, minVal, maxVal);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.SliderFloat($" ##{label}Y", ref y, minVal, maxVal);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.SliderFloat($" ##{label}Z", ref z, minVal, maxVal);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool wChanged = ImGui.SliderFloat($" ##{label}W", ref w, minVal, maxVal);
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
                if (wChanged)
                {
                    vec.W = w;
                }
            }
            ImGui.Columns(1);
            return xChanged || yChanged || zChanged || wChanged;
        }
        #endregion
        #region InputFloat
        public static bool MultiInputFloat(string label, Span<float> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            float prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputFloat($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }
            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiInputFloat2(string label, Span<Vector2> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector2 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                prev = vals[i];
                if (diffX && diffY)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 2;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputFloat($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.InputFloat($" ##{label}Y", ref y);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }

            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
            }
            ImGui.Columns(1);
            return xChanged || yChanged;
        }
        public static bool MultiInputFloat3(string label, Span<Vector3> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector3 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                prev = vals[i];
                if (diffX && diffY && diffZ)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float z = diffZ ? default : vals[0].Z;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 3;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputFloat($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.InputFloat($" ##{label}Y", ref y);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.InputFloat($" ##{label}Z", ref z);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }

            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
            }
            ImGui.Columns(1);
            return xChanged || yChanged || zChanged;
        }
        public static bool MultiInputFloat4(string label, Span<Vector4> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            Vector4 prev = vals[0];
            bool diffX = false;
            bool diffY = false;
            bool diffZ = false;
            bool diffW = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev.X != vals[i].X;
                diffY |= prev.Y != vals[i].Y;
                diffZ |= prev.Z != vals[i].Z;
                diffW |= prev.W != vals[i].W;
                prev = vals[i];
                if (diffX && diffY && diffZ && diffW)
                {
                    break;
                }
            }
            float x = diffX ? default : vals[0].X;
            float y = diffY ? default : vals[0].Y;
            float z = diffZ ? default : vals[0].Z;
            float w = diffW ? default : vals[0].W;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 4;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / 4f;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.SetColumnWidth(2, widthPerColumn);
            ImGui.SetColumnWidth(3, widthPerColumn);
            ImGui.SetColumnWidth(4, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            float cw = ImGui.GetColumnWidth();
            ImGui.SetNextItemWidth(cw);

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputFloat($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            cw = ImGui.GetColumnWidth();
            ImGui.SetNextItemWidth(cw);
            //ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffY)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool yChanged = ImGui.InputFloat($" ##{label}Y", ref y);
            if (diffY)
            {
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();
            cw = ImGui.GetColumnWidth();
            ImGui.SetNextItemWidth(cw);
            //ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool zChanged = ImGui.InputFloat($" ##{label}Z", ref z);
            if (diffZ)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            ImGui.NextColumn();
            cw = ImGui.GetColumnWidth();
            ImGui.SetNextItemWidth(cw);
            //ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool wChanged = ImGui.InputFloat($" ##{label}W", ref w);
            if (diffW)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec.X = x;
                }
                if (yChanged)
                {
                    vec.Y = y;
                }
                if (zChanged)
                {
                    vec.Z = z;
                }
                if (wChanged)
                {
                    vec.W = w;
                }
            }
            ImGui.Columns(1);
            return xChanged || yChanged || zChanged || wChanged;
        }
        #endregion
        #region InputDouble
        public static bool MultiInputDouble(string label, Span<double> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }

            double prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {

                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            double x = diffX ? default : vals[0];
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputDouble($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }
            ImGui.Columns(1);
            return xChanged;
        }
        #endregion
        #region Checkbox
        public static bool MultiCheckbox(string label, Span<bool> vals)
        {
            if (vals.Length <= 0)
            {
                return false;
            }
            bool prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {
                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            bool x = diffX ? default : vals[0];

            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.Checkbox($" ##{label}X", ref x);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }
            ImGui.Columns(1);
            return xChanged;
        }
        #endregion
        #region InputText
        public static bool MultiInputText(string label, Span<string> vals, uint maxLength, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        {
            if (vals.Length <= 0)
            {
                return false;
            }
            string prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {
                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            string x = diffX ? "" : (vals[0] ?? "");
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());

            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputText($" ##{label}X", ref x, maxLength, flags);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }
            ImGui.Columns(1);
            return xChanged;
        }
        public static bool MultiInputTextMultiline(string label, Span<string> vals, uint maxLength, Vector2 size, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        {
            if (vals.Length <= 0)
            {
                return false;
            }
            string prev = vals[0];
            bool diffX = false;
            for (int i = 0; i < vals.Length; i++)
            {
                diffX |= prev != vals[i];
                prev = vals[i];
                if (diffX)
                {
                    break;
                }
            }
            string x = diffX ? "" : (vals[0] ?? "");
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (diffX)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, DifferentColor);
            }
            bool xChanged = ImGui.InputTextMultiline($" ##{label}X", ref x, maxLength, size, flags);
            if (diffX)
            {
                ImGui.PopStyleColor();
            }
            for (int i = 0; i < vals.Length; i++)
            {
                ref var vec = ref vals[i];
                if (xChanged)
                {
                    vec = x;
                }
            }
            ImGui.Columns(1);
            return xChanged;
        }
        #endregion
    }
}
