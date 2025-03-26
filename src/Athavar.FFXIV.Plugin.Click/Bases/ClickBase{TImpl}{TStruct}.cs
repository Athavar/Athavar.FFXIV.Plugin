// <copyright file="ClickBase{TImpl}{TStruct}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Bases;

using Athavar.FFXIV.Plugin.Click.Enums;
using Athavar.FFXIV.Plugin.Click.Structures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     AtkUnitBase receive event delegate.
/// </summary>
/// <param name="eventListener">Type receiving the event.</param>
/// <param name="evt">Event type.</param>
/// <param name="which">Internal routing number.</param>
/// <param name="eventData">Event data.</param>
/// <param name="inputData">Keyboard and mouse data.</param>
/// <returns>The addon address.</returns>
internal unsafe delegate nint ReceiveEventDelegate(AtkEventListener* eventListener, EventType evt, int which, void* eventData, void* inputData);

/// <summary>
///     Click base class.
/// </summary>
/// <typeparam name="TImpl">The implementing type.</typeparam>
/// <typeparam name="TStruct">FFXIVClientStructs addon type.</typeparam>
public abstract unsafe class ClickBase<TImpl, TStruct> : ClickBase<TImpl>
    where TImpl : class
    where TStruct : unmanaged
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickBase{TImpl,TStruct}"/> class.
    /// </summary>
    /// <param name="name">Addon name.</param>
    /// <param name="addon">Addon address.</param>
    public ClickBase(string name, nint addon)
        : base(name, addon)
    {
    }

    /// <summary>
    ///     Gets a pointer to the type.
    /// </summary>
    public TStruct* Addon => (TStruct*)this.UnitBase;

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="target">Target node.</param>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonButton(AtkComponentButton* target, int which, AtkEventType type = AtkEventType.ButtonClick)
    {
        this.ClickAddonComponent(target->AtkComponentBase.OwnerNode, which, type);
    }

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="nodeIndex">Target node index.</param>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonButtonIndex(int nodeIndex, int which, AtkEventType type = AtkEventType.ButtonClick)
    {
        var node = (AtkComponentButton*)this.UnitBase->UldManager.NodeList[nodeIndex];
        this.ClickAddonButton(node, which, type);
    }

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="target">Target node.</param>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonRadioButton(AtkComponentButton* target, int which, AtkEventType type = AtkEventType.ButtonClick) => this.ClickAddonComponent(target->AtkComponentBase.OwnerNode, which, type);

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="target">Target node.</param>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonCheckBox(AtkComponentCheckBox* target, int which, AtkEventType type = AtkEventType.ButtonClick) => this.ClickAddonComponent(target->AtkComponentButton.AtkComponentBase.OwnerNode, which, type);

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="target">Target node.</param>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonDragDrop(AtkComponentDragDrop* target, int which, AtkEventType type = AtkEventType.DragDropCancel) => this.ClickAddonComponent(target->AtkComponentBase.OwnerNode, which, type);

    /// <summary>
    ///     Send a click.
    /// </summary>
    protected void ClickAddonStage()
    {
        var target = AtkStage.Instance();

        var @event = stackalloc AtkEvent[1]
        {
            EventData.ForNormalTarget(target, this.UnitBase),
        };
        var data = stackalloc AtkEventData[1];

        this.UnitBase->ReceiveEvent(AtkEventType.MouseClick, 0, @event, data);
    }

    /// <summary>
    ///     Send a right click.
    /// </summary>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void RightClickAddonStage(int which, EventType type = EventType.MOUSE_CLICK)
    {
        var target = AtkStage.Instance();
        /*
                var eventData = EventData.ForNormalRightTarget(target, this.UnitBase);
                var inputData = InputData.Empty();
                inputData.Data[0] = (byte*)0x0001000000000000;
                inputData.Data[1] = null;
                inputData.Data[2] = null;

                this.InvokeReceiveEvent(&this.UnitBase->AtkEventListener, type, which, eventData, inputData);
                */
    }

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="target">Target node.</param>
    /// <param name="which">Internal game click routing.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonComponent(AtkComponentNode* target, int which, AtkEventType type, AtkEvent? @event = null, AtkEventData? data = null)
    {
        var eventData = @event ?? EventData.ForNormalTarget(target, this.UnitBase);
        var inputData = data ?? InputData.Empty();

        this.InvokeReceiveEvent(&this.UnitBase->AtkEventListener, type, which, ref eventData, ref inputData);
    }

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="popupMenu">PopupMenu event listener.</param>
    /// <param name="index">List index.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonList(PopupMenu* popupMenu, ushort index, AtkEventType type = AtkEventType.ListItemClick)
    {
        var targetList = popupMenu->List;
        if (index < 0 || index >= popupMenu->EntryCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "List index is out of range");
        }

        var eventData = EventData.ForNormalTarget(targetList->AtkComponentBase.OwnerNode, popupMenu);
        var inputData = InputData.ForPopupMenu(popupMenu, index);

        this.InvokeReceiveEvent(&popupMenu->AtkEventListener, type, 0, ref eventData, ref inputData);
    }

    /// <summary>
    ///     Send a click.
    /// </summary>
    /// <param name="node">AtkComponentNode event listener.</param>
    /// <param name="index">List index.</param>
    /// <param name="which">Internal routing number.</param>
    /// <param name="type">Event type.</param>
    protected void ClickAddonComponentList(AtkComponentNode* node, ushort index, int which = 0, AtkEventType type = AtkEventType.ListItemClick)
    {
        var targetList = (AtkComponentList*)node->Component;
        if (index >= targetList->ListLength)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"List index {index} is out of range {targetList->ListLength}");
        }

        var eventData = EventData.ForNormalTarget(targetList->AtkComponentBase.OwnerNode, node);
        var inputData = InputData.ForAtkComponentNode(node, index);

        this.InvokeReceiveEvent(&this.UnitBase->AtkEventListener, type, which, ref eventData, ref inputData);
    }

    /// <summary>
    ///     Invoke the receive event delegate.
    /// </summary>
    /// <param name="eventListener">Type receiving the event.</param>
    /// <param name="type">Event type.</param>
    /// <param name="which">Internal routing number.</param>
    /// <param name="eventData">Event data.</param>
    /// <param name="inputData">Keyboard and mouse data.</param>
    private void InvokeReceiveEvent(AtkEventListener* eventListener, AtkEventType type, int which, ref AtkEvent eventData, ref AtkEventData inputData)
    {
        var eventPtr = SharedBuffer.Buffer.Add(ref eventData);
        var eventDataPtr = SharedBuffer.Buffer.Add(ref inputData);
        eventListener->ReceiveEvent(type, which, (AtkEvent*)eventPtr, (AtkEventData*)eventDataPtr);
    }
}