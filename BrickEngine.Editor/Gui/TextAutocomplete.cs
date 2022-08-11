using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Gui
{
    public partial class ImHelper
    {
        public static bool InputTextAutoComplete(string label, ref string input, uint maxLength, out int selectedIndex, ref bool isOpen, Span<string> suggestions, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        {
            selectedIndex = -1;
            float xSpace = ImGui.GetContentRegionAvail().X;
            const int dims = 1;
            float widthPerColumn = (xSpace * (1 - StartPercentage)) / dims;
            if (widthPerColumn < 1 || (xSpace * StartPercentage) < 1) return false;
            bool isInList = false;
            for (int i = 0; i < suggestions.Length; i++)
            {
                if (suggestions[i] == input)
                {
                    isInList = true;
                    selectedIndex = i;
                }
            }
            ImGui.BeginGroup();
            ImGui.Columns(dims + 1, $" ##{label}Columns", false);
            ImGui.SetColumnWidth(0, xSpace * StartPercentage);
            ImGui.SetColumnWidth(1, widthPerColumn);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if (isInList)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, GuiColors.SelectedBlue);
            }
            bool changed = ImGui.InputText($" ##{label}X", ref input, maxLength, flags);
            if (isInList)
            {
                ImGui.PopStyleColor();
            }
            ImGui.Columns(1);
            bool windowSelected = false;
           
            if (isOpen)
            {
                var tl = ImGui.GetItemRectMin();
                float height = MathF.Min(suggestions.Length, 8) * ImGui.GetTextLineHeightWithSpacing();
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetItemRectSize().X, height));
                if (ImGui.Begin($"##{label}_popup", ref isOpen, ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar))
                {
                    windowSelected |= ImGui.IsWindowFocused();
                    for (int i = 0; i < suggestions.Length; i++)
                    {
                        if (i == selectedIndex)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, GuiColors.SelectedBlue);
                        }
                        bool selected = ImGui.Selectable(suggestions[i] + "##" + suggestions[i] + " " + i);
                        if (i == selectedIndex)
                        {
                            ImGui.PopStyleColor();
                        }
                        windowSelected |= ImGui.IsItemFocused();
                        if (selected || (ImGui.IsItemHovered() && ImGui.IsKeyPressed(ImGuiKey.Enter, false)))
                        {
                            selectedIndex = i;
                            input = suggestions[i];
                            changed = true;
                            isOpen = false;
                        }
                    }
                    ImGui.SetWindowPos(new System.Numerics.Vector2(tl.X, tl.Y - ImGui.GetWindowSize().Y));
                }
                ImGui.End();
            }
            bool focused = ImGui.IsItemActive();
            if (!windowSelected)
            {
                if (isOpen && !focused)
                {
                    isOpen = false;
                }
                if (focused && !isOpen)
                {
                    isOpen = true;
                }
            }
            ImGui.EndGroup();
            return changed;
        }
        public static bool MultiInputTextAutoComplete(string label, Span<string> input, uint maxLength, ref int selectedIndex, ref bool isOpen, Span<string> suggestions, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        {
            selectedIndex = -1;
            if (input.Length <= 0)
            {
                return false;
            }
            bool isInList = false;
            for (int i = 0; i < suggestions.Length; i++)
            {
                if (suggestions[i] == input[0])
                {
                    isInList = true;
                    selectedIndex = i;
                }
            }
            ImGui.BeginGroup();
            if (isInList)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, GuiColors.SelectedBlue);
            }
            bool changed = MultiInputText(label, input, maxLength, flags);
            if (isInList)
            {
                ImGui.PopStyleColor();
            }
            bool windowSelected = false;
            if (isOpen)
            {
                var tl = ImGui.GetItemRectMin();
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetItemRectSize().X, 0));
                if (ImGui.Begin($"##{label}_popup", ref isOpen, ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar))
                {
                    windowSelected |= ImGui.IsWindowFocused();
                    for (int i = 0; i < suggestions.Length; i++)
                    {
                        if (i == selectedIndex)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, GuiColors.SelectedBlue);
                        }
                        bool selected = ImGui.Selectable(suggestions[i] + "##" + suggestions[i] + " " + i);
                        if (i == selectedIndex)
                        {
                            ImGui.PopStyleColor();
                        }
                        windowSelected |= ImGui.IsItemFocused();
                        if (selected || (ImGui.IsItemHovered() && ImGui.IsKeyPressed(ImGuiKey.Enter)))
                        {
                            for (int j = 0; j < input.Length; j++)
                            {
                                input[j] = suggestions[i];
                            }
                            selectedIndex = i;
                            changed = true;
                            isOpen = false;
                        }
                    }
                    ImGui.SetWindowPos(new System.Numerics.Vector2(tl.X, tl.Y - ImGui.GetWindowSize().Y));
                }
                ImGui.End();
            }
            bool focused = ImGui.IsItemActive();
            if (!windowSelected)
            {
                if (isOpen && !focused)
                {
                    isOpen = false;
                }
                if (focused && !isOpen)
                {
                    isOpen = true;
                }
            }
            ImGui.EndGroup();
            return changed;
        }
    }
}
