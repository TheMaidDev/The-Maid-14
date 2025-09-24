// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2025 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using System.Linq;
using System.Numerics;
using Content.Shared.Input;
using Robust.Client.Graphics;
using Robust.Shared.Input;

namespace Content.Client.UserInterface.Controls;

[Virtual]
public class RadialMenu : BaseWindow
{
    /// <summary>
    /// Contextual button used to traverse through previous layers of the radial menu
    /// </summary>
    public TextureButton? ContextualButton { get; set; }

    /// <summary>
    /// Set a style class to be applied to the contextual button when it is set to move the user back through previous layers of the radial menu
    /// </summary>
    public string? BackButtonStyleClass
    {
        get
        {
            return _backButtonStyleClass;
        }

        set
        {
            _backButtonStyleClass = value;

            if (_path.Count > 0 && ContextualButton != null && _backButtonStyleClass != null)
                ContextualButton.SetOnlyStyleClass(_backButtonStyleClass);
        }
    }

    /// <summary>
    /// Set a style class to be applied to the contextual button when it will close the radial menu
    /// </summary>
    public string? CloseButtonStyleClass
    {
        get
        {
            return _closeButtonStyleClass;
        }

        set
        {
            _closeButtonStyleClass = value;

            if (_path.Count == 0 && ContextualButton != null && _closeButtonStyleClass != null)
                ContextualButton.SetOnlyStyleClass(_closeButtonStyleClass);
        }
    }

    private List<Control> _path = new();
    private string? _backButtonStyleClass;
    private string? _closeButtonStyleClass;

    /// <summary>
    /// A free floating menu which enables the quick display of one or more radial containers
    /// </summary>
    /// <remarks>
    /// Only one radial container is visible at a time (each container forming a separate 'layer' within
    /// the menu), along with a contextual button at the menu center, which will either return the user
    /// to the previous layer or close the menu if there are no previous layers left to traverse.
    /// To create a functional radial menu, simply parent one or more named radial containers to it,
    /// and populate the radial containers with RadialMenuButtons. Setting the TargetLayer field of these
    /// buttons to the name of a radial conatiner will display the container in question to the user
    /// whenever it is clicked in additon to any other actions assigned to the button
    /// <summary>
    /// Initializes a RadialMenu: ensures only the first child layer is visible, creates the central contextual button,
    /// and wires child-added and contextual-button events for layer navigation.
    /// </summary>
    /// <remarks>
    /// - Hides all child controls except the first one (the initial active layer).
    /// - Creates and centers a 64×64 TextureButton assigned to <see cref="ContextualButton"/> and hooks its
    ///   release event to <see cref="ReturnToPreviousLayer"/>.
    /// - Adds the contextual button as a child of the menu.
    /// - Subscribes to <see cref="OnChildAdded"/> to keep newly added children hidden unless they become the active layer.
    /// </remarks>
    public RadialMenu()
    {
        // Hide all starting children (if any) except the first (this is the active layer)
        if (ChildCount > 1)
        {
            for (int i = 1; i < ChildCount; i++)
                GetChild(i).Visible = false;
        }

        // Auto generate a contextual button for moving back through visited layers
        ContextualButton = new TextureButton()
        {
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SetSize = new Vector2(64f, 64f),
        };

        ContextualButton.OnButtonUp += _ => ReturnToPreviousLayer();
        AddChild(ContextualButton);

        // Hide any further add children, unless its promoted to the active layer
        OnChildAdded += child => child.Visible = (GetCurrentActiveLayer() == child);
    }

    /// <summary>
    /// Returns the currently visible non-contextual child layer, or null if none exist.
    /// </summary>
    /// <returns>The first child control that is visible and is not the contextual button; otherwise <c>null</c>.</returns>
    private Control? GetCurrentActiveLayer()
    {
        var children = Children.Where(x => x != ContextualButton);

        if (!children.Any())
            return null;

        return children.First(x => x.Visible);
    }

    /// <summary>
    /// Attempts to switch the radial menu to the layer with the given name.
    /// </summary>
    /// <param name="newLayer">The Name of the child layer to activate. An empty string will not be accepted.</param>
    /// <returns>True if a matching layer was found and made visible; false otherwise.</returns>
    /// <remarks>
    /// When a layer switch succeeds the previously active layer is pushed onto the internal traversal path so it can be returned to later.
    /// If the traversal path becomes non-empty and a contextual button exists, that button's style class is set to the configured back-button style.
    /// </remarks>
    public bool TryToMoveToNewLayer(string newLayer)
    {
        if (newLayer == string.Empty)
            return false;

        var currentLayer = GetCurrentActiveLayer();

        if (currentLayer == null)
            return false;

        var result = false;

        foreach (var child in Children)
        {
            if (child == ContextualButton)
                continue;

            // Hide layers which are not of interest
            if (result == true || child.Name != newLayer)
            {
                child.Visible = false;
            }

            // Show the layer of interest
            else
            {
                child.Visible = true;
                result = true;
            }
        }

        // Update the traversal path
        if (result)
            _path.Add(currentLayer);

        // Set the style class of the button
        if (_path.Count > 0 && ContextualButton != null && BackButtonStyleClass != null)
            ContextualButton.SetOnlyStyleClass(BackButtonStyleClass);

        return result;
    }

    /// <summary>
    /// Navigates the radial menu back to the previously active layer or closes the menu if there is no history.
    /// </summary>
    /// <remarks>
    /// If a previous layer exists, this hides all non-contextual children, makes the last visited layer visible,
    /// and removes it from the internal traversal path. If the traversal path becomes empty the method applies
    /// <see cref="CloseButtonStyleClass"/> to <see cref="ContextualButton"/> (when set). If there is no history,
    /// the menu is closed via <see cref="Close()"/>.
    /// </remarks>
    public void ReturnToPreviousLayer()
    {
        // Close the menu if the traversal path is empty
        if (_path.Count == 0)
        {
            Close();
            return;
        }

        var lastChild = _path[^1];

        // Hide all children except the contextual button
        foreach (var child in Children)
        {
            if (child != ContextualButton)
                child.Visible = false;
        }

        // Make the last visited layer visible, update the path list
        lastChild.Visible = true;
        _path.RemoveAt(_path.Count - 1);

        // Set the style class of the button
        if (_path.Count == 0 && ContextualButton != null && CloseButtonStyleClass != null)
            ContextualButton.SetOnlyStyleClass(CloseButtonStyleClass);
    }
}

[Virtual]
public class RadialMenuButton : Button
{
    /// <summary>
    /// Upon clicking this button the radial menu will transition to the named layer
    /// </summary>
    public string? TargetLayer { get; set; }

    /// <summary>
    /// A simple button that can move the user to a different layer within a radial menu
    /// <summary>
    /// Initializes a new instance of RadialMenuButton.
    /// </summary>
    /// <remarks>
    /// Subscribes the button's release event to the internal click handler that attempts to navigate its parent RadialMenu to this button's TargetLayer.
    /// </remarks>
    public RadialMenuButton()
    {
        OnButtonUp += OnClicked;
    }

    /// <summary>
    /// Handle the button release event by instructing the nearest parent RadialMenu to navigate to this button's TargetLayer.
    /// </summary>
    /// <remarks>
    /// Does nothing if <see cref="TargetLayer"/> is null or empty, or if no enclosing <see cref="RadialMenu"/> is found in the control ancestry.
    /// </remarks>
    private void OnClicked(ButtonEventArgs args)
    {
        if (TargetLayer == null || TargetLayer == string.Empty)
            return;

        var parent = FindParentMultiLayerContainer(this);

        if (parent == null)
            return;

        parent.TryToMoveToNewLayer(TargetLayer);
    }

    /// <summary>
    /// Walks the given control and its logical ancestors to find the nearest enclosing RadialMenu.
    /// </summary>
    /// <param name="control">The starting control whose ancestors will be searched.</param>
    /// <returns>The nearest RadialMenu ancestor if found; otherwise <c>null</c>.</returns>
    private RadialMenu? FindParentMultiLayerContainer(Control control)
    {
        foreach (var ancestor in control.GetSelfAndLogicalAncestors())
        {
            if (ancestor is RadialMenu)
                return ancestor as RadialMenu;
        }

        return null;
    }
}

[Virtual]
public class RadialMenuTextureButton : TextureButton
{
    /// <summary>
    /// Upon clicking this button the radial menu will be moved to the named layer
    /// </summary>
    public string TargetLayer { get; set; } = string.Empty;

    /// <summary>
    /// A simple texture button that can move the user to a different layer within a radial menu
    /// <summary>
    /// Initializes a new RadialMenuTextureButton and subscribes the <see cref="OnClicked"/> handler to the <see cref="OnButtonUp"/> event.
    /// </summary>
    public RadialMenuTextureButton()
    {
        OnButtonUp += OnClicked;
    }

    /// <summary>
    /// Invoked on button release; if this button has a non-empty TargetLayer, finds the nearest enclosing RadialMenu and instructs it to switch to that layer.
    /// </summary>
    private void OnClicked(ButtonEventArgs args)
    {
        if (TargetLayer == string.Empty)
            return;

        var parent = FindParentMultiLayerContainer(this);

        if (parent == null)
            return;

        parent.TryToMoveToNewLayer(TargetLayer);
    }

    /// <summary>
    /// Walks the given control and its logical ancestors to find the nearest enclosing RadialMenu.
    /// </summary>
    /// <param name="control">The starting control whose ancestors will be searched.</param>
    /// <returns>The nearest RadialMenu ancestor if found; otherwise <c>null</c>.</returns>
    private RadialMenu? FindParentMultiLayerContainer(Control control)
    {
        foreach (var ancestor in control.GetSelfAndLogicalAncestors())
        {
            if (ancestor is RadialMenu)
                return ancestor as RadialMenu;
        }

        return null;
    }
}
