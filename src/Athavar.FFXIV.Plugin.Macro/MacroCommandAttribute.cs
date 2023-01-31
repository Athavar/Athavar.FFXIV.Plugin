// <copyright file="MacroCommandAttribute.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Macro;

internal class MacroCommandAttribute : Attribute
{
    public MacroCommandAttribute(string name, string? alias = null)
        : this(name, alias, string.Empty, Array.Empty<string>(), Array.Empty<string>())
        => this.Hidden = true;

    public MacroCommandAttribute(string name, string? alias, string description, string[] modifiers, string[] examples)
    {
        this.Name = name;
        this.Alias = alias;
        this.Description = description;
        this.Modifiers = modifiers;
        this.Examples = examples;
    }

    public string Name { get; }

    public string? Alias { get; }

    public string Description { get; }

    public string[] Modifiers { get; }

    public string[] Examples { get; }

    public bool Hidden { get; }
}