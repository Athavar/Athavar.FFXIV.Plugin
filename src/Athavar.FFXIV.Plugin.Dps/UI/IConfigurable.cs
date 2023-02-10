// <copyright file="IConfigurable.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI;

using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI.Config;

internal interface IConfigurable
{
    string Name { get; set; }

    IEnumerable<IConfigPage> GetConfigPages();

    void ImportConfig(IConfig c);
}