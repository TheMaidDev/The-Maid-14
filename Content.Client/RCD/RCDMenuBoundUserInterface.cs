// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.RCD;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.RCD;

[UsedImplicitly]
public sealed class RCDMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private RCDMenu? _menu;

    /// <summary>
    /// Constructs an RCDMenuBoundUserInterface for the given entity and UI key and injects required dependencies.
    /// </summary>
    /// <param name="owner">Entity UID that owns this user interface.</param>
    /// <param name="uiKey">Identifier for this UI instance.</param>
    public RCDMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Opens the RCD user interface window for the owner entity.
    /// </summary>
    /// <remarks>
    /// Creates an <see cref="RCDMenu"/>, assigns its entity to <see cref="Owner"/>, subscribes the
    /// menu's <c>SendRCDSystemMessageAction</c> to <see cref="SendRCDSystemMessage"/>, and opens the
    /// menu centered at the current mouse position (normalized by the display viewport size).
    /// </remarks>
    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<RCDMenu>();
        _menu.SetEntity(Owner);
        _menu.SendRCDSystemMessageAction += SendRCDSystemMessage;

        // Open the menu, centered on the mouse
        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    /// <summary>
    /// Sends a non-predicted RCD system message to the server requesting activation/use of the specified RCD prototype.
    /// </summary>
    /// <param name="protoId">The identifier of the RCD prototype to activate.</param>
    /// <remarks>
    /// The message is sent immediately (not predicted). The UI is expected to close right after sending, so the server-side action must not rely on client prediction.
    /// </remarks>
    public void SendRCDSystemMessage(ProtoId<RCDPrototype> protoId)
    {
        // A predicted message cannot be used here as the RCD UI is closed immediately
        // after this message is sent, which will stop the server from receiving it
        SendMessage(new RCDSystemMessage(protoId));
    }
}
