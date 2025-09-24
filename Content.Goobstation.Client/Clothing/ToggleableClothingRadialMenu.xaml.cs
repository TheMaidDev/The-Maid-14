// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Clothing.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Goobstation.Client.Clothing;

public sealed partial class ToggleableClothingRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    public event Action<EntityUid>? SendToggleClothingMessageAction;

    public EntityUid Entity { get; set; }

    public ToggleableClothingRadialMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        RefreshUI();
    }

    /// <summary>
    /// Rebuilds the radial menu UI to reflect the current toggleable clothing attached to <see cref="Entity"/>.
    /// </summary>
    /// <remarks>
    /// Finds the "Main" radial container, reads the <see cref="ToggleableClothingComponent"/> on <see cref="Entity"/>, and
    /// populates the container with a button for each entry in the component's clothing list. Each button shows the
    /// clothing's sprite, sets an attach/unattach tooltip based on whether the clothing is present in the component's
    /// container, and registers the click actions that send toggle events and refresh the menu.
    /// 
    /// If the entity has no <see cref="ToggleableClothingComponent"/> or its clothing container is null, the method exits
    /// without modifying the UI.
    /// </remarks>
    public void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main");

        if (!_entityManager.TryGetComponent<ToggleableClothingComponent>(Entity, out var clothing))
            return;

        var clothingContainer = clothing.Container;

        if (clothingContainer == null)
            return;

        foreach (var attached in clothing.ClothingUids)
        {
            // Change tooltip text if attached clothing is toggle/untoggled
            var tooltipText = Loc.GetString("toggleable-clothing-unattach-tooltip");

            if (clothingContainer.Contains(attached.Key))
                tooltipText = Loc.GetString("toggleable-clothing-attach-tooltip");

            var button = new ToggleableClothingRadialMenuButton()
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = tooltipText,
                AttachedClothingId = attached.Key
            };

            var spriteView = new SpriteView()
            {
                SetSize = new Vector2(48, 48),
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Stretch = SpriteView.StretchMode.Fill
            };

            spriteView.SetEntity(attached.Key);

            button.AddChild(spriteView);
            main.AddChild(button);
        }

        AddToggleableClothingMenuButtonOnClickAction(main);
    }

    /// <summary>
    /// Attaches click handlers to toggleable clothing radial buttons inside the provided radial container.
    /// </summary>
    /// <param name="control">The radial container whose children are expected to be ToggleableClothingRadialMenuButton instances. If <paramref name="control"/> is not a RadialContainer the method is a no-op.</param>
    /// <remarks>
    /// For each button it subscribes to <c>OnPressed</c> so that pressing the button:
    /// - invokes <c>SendToggleClothingMessageAction</c> with the button's <c>AttachedClothingId</c> (if set),
    /// - disposes all children of the radial container, and
    /// - refreshes the menu UI by calling <c>RefreshUI()</c>.
    /// </remarks>
    private void AddToggleableClothingMenuButtonOnClickAction(Control control)
    {
        var mainControl = control as RadialContainer;

        if (mainControl == null)
            return;

        foreach (var child in mainControl.Children)
        {
            var castChild = child as ToggleableClothingRadialMenuButton;

            if (castChild == null)
                return;

            castChild.OnPressed += _ =>
            {
                SendToggleClothingMessageAction?.Invoke(castChild.AttachedClothingId);
                mainControl.DisposeAllChildren();
                RefreshUI();
            };
        }
    }
}

public sealed class ToggleableClothingRadialMenuButton : RadialMenuTextureButton
{
    public EntityUid AttachedClothingId { get; set; }
}
