// <copyright file="ICommandInterface.Ui.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

public partial interface ICommandInterface
{
    /// <summary>
    ///     Gets a value indicating whether an addon is visible.
    /// </summary>
    /// <param name="addonName">Addon name.</param>
    /// <returns>A value indicating whether an addon is visible.</returns>
    public bool IsAddonVisible(string addonName);

    /// <summary>
    ///     Gets a value indicating whether an addon is ready to be used. It may not be visible.
    /// </summary>
    /// <param name="addonName">Addon name.</param>
    /// <returns>A value indicating whether an addon is ready to be used.</returns>
    public bool IsAddonReady(string addonName);

    /// <summary>
    ///     Close an addon that is visible.
    /// </summary>
    /// <param name="addonName">Addon name.</param>
    public void CloseAddon(string addonName);

    /// <summary>
    ///     Get the text of a TextNode by its index number. You can find this by using the addon inspector.
    ///     In general, these numbers do not change.
    /// </summary>
    /// <param name="addonName">Addon name.</param>
    /// <param name="nodeNumbers">Node numbers, can fetch nested nodes.</param>
    /// <returns>The node text.</returns>
    public string GetNodeText(string addonName, params int[] nodeNumbers);

    /// <summary>
    ///     Get the text of a 0-indexed SelectIconString entry.
    /// </summary>
    /// <param name="index">Item number, 0 indexed.</param>
    /// <returns>The item text, or an empty string.</returns>
    public string GetSelectStringText(int index);

    /// <summary>
    ///     Get the count of entries of a SelectIconString addon.
    /// </summary>
    /// <returns>The entry count.</returns>
    public int GetSelectStringEntryCount();

    /// <summary>
    ///     Get the text of a 0-indexed SelectIconString entry.
    /// </summary>
    /// <param name="index">Item number, 0 indexed.</param>
    /// <returns>The item text, or an empty string.</returns>
    public string GetSelectIconStringText(int index);
}