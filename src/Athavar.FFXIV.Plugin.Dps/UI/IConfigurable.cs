// <copyright file="IConfigurable.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
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