// <copyright file="ModuleAttribute.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

public sealed class ModuleAttribute : Attribute
{
    public ModuleAttribute(string name) => this.Name = name;

    public string Name { get; }

    public Type? ModuleConfigurationType { get; set; }

    public bool HasTab { get; set; } = false;

    public bool Hidden { get; set; } = false;

    public bool Debug { get; set; } = false;
}