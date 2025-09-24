// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed class CarvingKnifeMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    private SpriteSystem _sprites;

    public EntityUid Entity { get; private set; }

    public event Action<ProtoId<RuneCarvingPrototype>>? SendCarvingKnifeSystemMessageAction;

    public CarvingKnifeMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _sprites = _ent.System<SpriteSystem>();
    }

    public void SetEntity(EntityUid ent)
    {
        Entity = ent;
        UpdateUI();
    }

    /// <summary>
    /// Rebuilds the radial menu contents to reflect the current carvings on the configured Entity.
    /// </summary>
    /// <remarks>
    /// Clears the "Main" RadialContainer and, if the Entity has a CarvingKnifeComponent, creates a button for each
    /// entry in its Carvings collection. Prototypes that cannot be resolved are skipped. Each button is populated
    /// with the prototype's icon and tooltip and then wired for click actions via AddCarvingKnifeMenuButtonOnClickActions.
    /// If the Entity does not have a CarvingKnifeComponent, the menu is left empty.
    /// </remarks>
    private void UpdateUI()
    {
        var main = FindControl<RadialContainer>("Main");
        main.RemoveAllChildren();

        if (!_ent.TryGetComponent(Entity, out CarvingKnifeComponent? carvingKnife))
            return;

        foreach (var ammo in carvingKnife.Carvings)
        {
            if (!_prot.TryIndex(ammo, out var prototype))
                continue;

            var button = new CarvingKnifeMenuButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = Loc.GetString(prototype.Desc),
                ProtoId = prototype.ID
            };

            var texture = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _sprites.Frame0(prototype.Icon),
                TextureScale = new Vector2(2f, 2f)
            };

            button.AddChild(texture);
            main.AddChild(button);
        }

        AddCarvingKnifeMenuButtonOnClickActions(main);
    }

    /// <summary>
    /// Wires click handlers for all CarvingKnifeMenuButton children in the given radial container.
    /// </summary>
    /// <param name="control">The radial container whose CarvingKnifeMenuButton children will be wired.</param>
    /// <remarks>
    /// For each CarvingKnifeMenuButton found, subscribes to its <c>OnButtonUp</c> event so that when pressed
    /// the menu invokes <c>SendCarvingKnifeSystemMessageAction</c> with the button's prototype ID and then closes the menu.
    /// </remarks>
    private void AddCarvingKnifeMenuButtonOnClickActions(RadialContainer control)
    {
        foreach (var child in control.Children)
        {
            if (child is not CarvingKnifeMenuButton castChild)
                continue;

            castChild.OnButtonUp += _ =>
            {
                SendCarvingKnifeSystemMessageAction?.Invoke(castChild.ProtoId);
                Close();
            };
        }
    }
}

public sealed class CarvingKnifeMenuButton : RadialMenuTextureButton
{
    public ProtoId<RuneCarvingPrototype> ProtoId { get; set; }
}
