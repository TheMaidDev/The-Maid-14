// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed partial class HereticRitualRuneRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    private readonly SpriteSystem _spriteSystem;

    public event Action<ProtoId<HereticRitualPrototype>>? SendHereticRitualRuneMessageAction;

    public EntityUid Entity { get; set; }

    public HereticRitualRuneRadialMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _spriteSystem = _entitySystem.GetEntitySystem<SpriteSystem>();
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        RefreshUI();
    }

    /// <summary>
    /// Rebuilds the radial menu UI for heretic rituals.
    /// </summary>
    /// <remarks>
    /// Locates the "Main" RadialContainer and, if the local player has a HereticComponent,
    /// creates one HereticRitualMenuButton per known ritual (setting size, style class,
    /// tooltip, icon, and ProtoId) and adds them to the container. After populating the
    /// menu it wires up the click handlers via AddHereticRitualMenuButtonOnClickAction.
    /// Exits early without changes if the container is missing or the local player is
    /// not a heretic.
    /// </remarks>
    private void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main");
        if (main == null)
            return;

        var player = _playerManager.LocalEntity;

        if (!_entityManager.TryGetComponent<HereticComponent>(player, out var heretic))
            return;

        foreach (var ritual in heretic.KnownRituals)
        {
            if (!_prototypeManager.TryIndex(ritual, out var ritualPrototype))
                continue;

            var button = new HereticRitualMenuButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = Loc.GetString(ritualPrototype.LocName),
                ProtoId = ritualPrototype.ID
            };

            var texture = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _spriteSystem.Frame0(ritualPrototype.Icon),
                TextureScale = new Vector2(2f, 2f)
            };

            button.AddChild(texture);
            main.AddChild(button);
        }

        AddHereticRitualMenuButtonOnClickAction(main);
    }

    /// <summary>
    /// Attaches click handlers to each HereticRitualMenuButton child of the given radial container.
    /// </summary>
    /// <param name="mainControl">The radial container whose HereticRitualMenuButton children will receive click handlers. If null, the method does nothing.</param>
    /// <remarks>
    /// When a button is released, this invokes <see cref="SendHereticRitualRuneMessageAction"/> with the button's <c>ProtoId</c> and closes the radial menu.
    /// </remarks>
    private void AddHereticRitualMenuButtonOnClickAction(RadialContainer mainControl)
    {
        if (mainControl == null)
            return;

        foreach(var child in mainControl.Children)
        {
            var castChild = child as HereticRitualMenuButton;

            if (castChild == null)
                continue;

            castChild.OnButtonUp += _ =>
            {
                SendHereticRitualRuneMessageAction?.Invoke(castChild.ProtoId);
                Close();
            };
        }
    }

    public sealed class HereticRitualMenuButton : RadialMenuTextureButton
    {
        public ProtoId<HereticRitualPrototype> ProtoId { get; set; }
    }
}
