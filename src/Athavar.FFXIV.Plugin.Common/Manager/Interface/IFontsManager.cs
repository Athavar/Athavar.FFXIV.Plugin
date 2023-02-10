// <copyright file="IFontsManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

public interface IFontsManager
{
    void BuildFonts();

    void UpdateFonts(IEnumerable<FontData> fonts);

    int GetFontIndex(string fontKey);

    public struct FontData
    {
        public string Name;
        public int Size;
        public bool Chinese;
        public bool Korean;

        public FontData(string name, int size, bool chinese, bool korean)
        {
            this.Name = name;
            this.Size = size;
            this.Chinese = chinese;
            this.Korean = korean;
        }
    }
}