namespace Athavar.FFXIV.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using ImGuiNET;

    public sealed class ImGuiRaii : IDisposable
    {
        private int _colorStack;
        private int _fontStack;
        private int _styleStack;
        private float _indentation;

        private Stack<Action>? _onDispose;

        public static ImGuiRaii NewGroup()
            => new ImGuiRaii().Group();

        public ImGuiRaii Group()
            => this.Begin(ImGui.BeginGroup, ImGui.EndGroup);

        public static ImGuiRaii NewTooltip()
            => new ImGuiRaii().Tooltip();

        public ImGuiRaii Tooltip()
            => this.Begin(ImGui.BeginTooltip, ImGui.EndTooltip);

        public ImGuiRaii PushColor(ImGuiCol which, uint color)
        {
            ImGui.PushStyleColor(which, color);
            ++this._colorStack;
            return this;
        }

        public ImGuiRaii PushColor(ImGuiCol which, Vector4 color)
        {
            ImGui.PushStyleColor(which, color);
            ++this._colorStack;
            return this;
        }

        public ImGuiRaii PopColors(int n = 1)
        {
            var actualN = Math.Min(n, this._colorStack);
            if (actualN > 0)
            {
                ImGui.PopStyleColor(actualN);
                this._colorStack -= actualN;
            }

            return this;
        }

        public ImGuiRaii PushStyle(ImGuiStyleVar style, Vector2 value)
        {
            ImGui.PushStyleVar(style, value);
            ++this._styleStack;
            return this;
        }

        public ImGuiRaii PushStyle(ImGuiStyleVar style, float value)
        {
            ImGui.PushStyleVar(style, value);
            ++this._styleStack;
            return this;
        }

        public ImGuiRaii PopStyles(int n = 1)
        {
            var actualN = Math.Min(n, this._styleStack);
            if (actualN > 0)
            {
                ImGui.PopStyleVar(actualN);
                this._styleStack -= actualN;
            }

            return this;
        }

        public ImGuiRaii PushFont(ImFontPtr font)
        {
            ImGui.PushFont(font);
            ++this._fontStack;
            return this;
        }

        public ImGuiRaii PopFonts(int n = 1)
        {
            var actualN = Math.Min(n, this._fontStack);

            while (actualN-- > 0)
            {
                ImGui.PopFont();
                --this._fontStack;
            }

            return this;
        }

        public ImGuiRaii Indent(float width)
        {
            if (width != 0)
            {
                ImGui.Indent(width);
                this._indentation += width;
            }

            return this;
        }

        public ImGuiRaii Unindent(float width)
            => this.Indent(-width);

        public bool Begin(Func<bool> begin, Action end)
        {
            if (begin())
            {
                this._onDispose ??= new Stack<Action>();
                this._onDispose.Push(end);
                return true;
            }

            return false;
        }

        public ImGuiRaii Begin(Action begin, Action end)
        {
            begin();
            this._onDispose ??= new Stack<Action>();
            this._onDispose.Push(end);
            return this;
        }

        public void End(int n = 1)
        {
            var actualN = Math.Min(n, this._onDispose?.Count ?? 0);
            while (actualN-- > 0)
            {
                this._onDispose!.Pop()();
            }
        }

        public void Dispose()
        {
            this.Unindent(this._indentation);
            this.PopColors(this._colorStack);
            this.PopStyles(this._styleStack);
            this.PopFonts(this._fontStack);
            if (this._onDispose != null)
            {
                this.End(this._onDispose.Count);
                this._onDispose = null;
            }
        }
    }
}