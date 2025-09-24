// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Chat.UI;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Emotes;

[UsedImplicitly]
public sealed class EmotesUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private MenuButton? EmotesButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.EmotesButton;
    private EmotesMenu? _menu;

    /// <summary>
    /// Called when the gameplay state is entered; registers a key binding that opens the Emotes menu.
    /// </summary>
    /// <param name="state">The gameplay state being entered (not used by this method).</param>
    public void OnStateEntered(GameplayState state)
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenEmotesMenu,
                InputCmdHandler.FromDelegate(_ => ToggleEmotesMenu(false)))
            .Register<EmotesUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<EmotesUIController>();
    }

    /// <summary>
    /// Toggles the Emotes menu: opens it if closed, or closes it if open.
    /// </summary>
    /// <param name="centered">
    /// If true, open the menu centered on the screen; if false, open the menu centered at the current mouse position.
    /// </param>
    /// <remarks>
    /// When opening, this creates the EmotesMenu, subscribes to its open/close and play-emote events, and sets the top-bar Emotes button to a pressed state (if present).
    /// When closing, this unsubscribes those events, clears the Emotes button pressed state, and disposes the menu.
    /// </remarks>
    private void ToggleEmotesMenu(bool centered)
    {
        if (_menu == null)
        {
            // setup window
            _menu = UIManager.CreateWindow<EmotesMenu>();
            _menu.OnClose += OnWindowClosed;
            _menu.OnOpen += OnWindowOpen;
            _menu.OnPlayEmote += OnPlayEmote;

            if (EmotesButton != null)
                EmotesButton.SetClickPressed(true);

            if (centered)
            {
                _menu.OpenCentered();
            }
            else
            {
                // Open the menu, centered on the mouse
                var vpSize = _displayManager.ScreenSize;
                _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
            }
        }
        else
        {
            _menu.OnClose -= OnWindowClosed;
            _menu.OnOpen -= OnWindowOpen;
            _menu.OnPlayEmote -= OnPlayEmote;

            if (EmotesButton != null)
                EmotesButton.SetClickPressed(false);

            CloseMenu();
        }
    }

    public void UnloadButton()
    {
        if (EmotesButton == null)
            return;

        EmotesButton.OnPressed -= ActionButtonPressed;
    }

    public void LoadButton()
    {
        if (EmotesButton == null)
            return;

        EmotesButton.OnPressed += ActionButtonPressed;
    }

    private void ActionButtonPressed(BaseButton.ButtonEventArgs args)
    {
        ToggleEmotesMenu(true);
    }

    private void OnWindowClosed()
    {
        if (EmotesButton != null)
            EmotesButton.Pressed = false;

        CloseMenu();
    }

    private void OnWindowOpen()
    {
        if (EmotesButton != null)
            EmotesButton.Pressed = true;
    }

    /// <summary>
    /// Closes and disposes the currently open emotes menu, if any.
    /// </summary>
    /// <remarks>
    /// If no menu is open, this method does nothing. After disposing, the internal menu reference is cleared.
    /// </remarks>
    private void CloseMenu()
    {
        if (_menu == null)
            return;

        _menu.Dispose();
        _menu = null;
    }

    /// <summary>
    /// Sends a predictive request to play the emote identified by <paramref name="protoId"/>.
    /// </summary>
    /// <param name="protoId">The prototype identifier of the emote to play.</param>
    private void OnPlayEmote(ProtoId<EmotePrototype> protoId)
    {
        _entityManager.RaisePredictiveEvent(new PlayEmoteMessage(protoId));
    }
}