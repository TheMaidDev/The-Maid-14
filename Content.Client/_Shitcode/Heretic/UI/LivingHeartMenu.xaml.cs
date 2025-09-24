// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Lobby;
using Content.Client.UserInterface.Controls;
using Content.Shared.Heretic;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed class LivingHeartMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly LobbyUIController _controller;

    public EntityUid Entity { get; private set; }

    public event Action<NetEntity>? SendActivateMessageAction;

    public LivingHeartMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        _controller = UserInterfaceManager.GetUIController<LobbyUIController>();
    }

    public void SetEntity(EntityUid ent)
    {
        Entity = ent;
        UpdateUI();
    }

    /// <summary>
    /// Rebuilds the radial menu contents for the current Entity based on the local player's HereticComponent.
    /// </summary>
    /// <remarks>
    /// Finds the "Main" RadialContainer in the control, reads the local player's HereticComponent, and populates the container
    /// with a button for each sacrifice target. For targets whose entity is missing, a profile-based entity is loaded.
    /// Each button is configured with the target's NetEntity and a SpriteView representing the target. After populating the menu,
    /// <see cref="AddAction(RadialContainer)"/> is called to wire up button activations.
    /// </remarks>
    private void UpdateUI()
    {
        var main = FindControl<RadialContainer>("Main");
        if (main == null) return;

        var player = _player.LocalEntity;

        if (!_ent.TryGetComponent<HereticComponent>(player, out var heretic))
            return;

        foreach (var target in heretic.SacrificeTargets)
        {
            if (!_ent.TryGetEntity(target.Entity, out var ent) || !_ent.EntityExists(ent))
                ent = _controller.LoadProfileEntity(target.Profile, _prot.Index(target.Job), true);

            var button = new EmbeddedEntityMenuButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = target.Profile.Name,
                NetEntity = target.Entity,
            };

            var texture = new SpriteView(ent.Value, _ent)
            {
                OverrideDirection = Direction.South,
                VerticalAlignment = VAlignment.Center,
                SetSize = new Vector2(64, 64),
                VerticalExpand = true,
                Stretch = SpriteView.StretchMode.Fill,
            };
            button.AddChild(texture);

            main.AddChild(button);
        }
        AddAction(main);
    }

    /// <summary>
    /// Subscribes each EmbeddedEntityMenuButton in the given radial container to activate its associated NetEntity when clicked and then close the menu.
    /// </summary>
    /// <param name="main">RadialContainer whose child buttons will receive the activation handlers. If null, no action is taken.</param>
    private void AddAction(RadialContainer main)
    {
        if (main == null)
            return;

        foreach (var child in main.Children)
        {
            var castChild = child as EmbeddedEntityMenuButton;
            if (castChild == null)
                continue;

            castChild.OnButtonUp += _ =>
            {
                SendActivateMessageAction?.Invoke(castChild.NetEntity);
                Close();
            };
        }
    }

    public sealed class EmbeddedEntityMenuButton : RadialMenuTextureButton
    {
        public NetEntity NetEntity;
    }
}
