// <copyright file="IYesConfigTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Yes;

using Athavar.FFXIV.Plugin.Common.UI;

public interface IYesConfigTab : ITab
{
    /// <summary>
    ///     Setup the <see cref="YesConfigTab" />.
    /// </summary>
    /// <param name="m">The <see cref="YesModule" />.</param>
    internal void Setup(YesModule m);
}