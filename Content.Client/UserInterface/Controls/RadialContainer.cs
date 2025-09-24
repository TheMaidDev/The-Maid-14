// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2025 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using System.Linq;
using System.Numerics;

namespace Content.Client.UserInterface.Controls;

[Virtual]
public class RadialContainer : LayoutContainer
{
    /// <summary>
    /// Specifies the anglular range, in radians, in which child elements will be placed.
    /// The first value denotes the angle at which the first element is to be placed, and
    /// the second value denotes the angle at which the last element is to be placed.
    /// Both values must be between 0 and 2 PI radians
    /// </summary>
    /// <remarks>
    /// The top of the screen is at 0 radians, and the bottom of the screen is at PI radians
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 AngularRange
    {
        get
        {
            return _angularRange;
        }

        set
        {
            var x = value.X;
            var y = value.Y;

            x = x > MathF.Tau ? x % MathF.Tau : x;
            y = y > MathF.Tau ? y % MathF.Tau : y;

            x = x < 0 ? MathF.Tau + x : x;
            y = y < 0 ? MathF.Tau + y : y;

            _angularRange = new Vector2(x, y);
        }
    }

    private Vector2 _angularRange = new Vector2(0f, MathF.Tau - float.Epsilon);

    /// <summary>
    /// Determines the direction in which child elements will be arranged
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public RAlignment RadialAlignment { get; set; } = RAlignment.Clockwise;

    /// <summary>
    /// Determines how far from the radial container's center that its child elements will be placed
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Radius { get; set; } = 100f;

    /// <summary>
    /// Sets whether the container should reserve a space on the layout for child which are not currently visible
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ReserveSpaceForHiddenChildren { get; set; } = true;

    /// <summary>
    /// This container arranges its children, evenly separated, in a radial pattern
    /// <summary>
    /// Initializes a new instance of <see cref="RadialContainer"/>.
    /// </summary>
    /// <remarks>
    /// Defaults: <see cref="Radius"/> = 100f, <see cref="RadialAlignment"/> = <c>Clockwise</c>,
    /// <see cref="AngularRange"/> = (0, Tau - ε), and <see cref="ReserveSpaceForHiddenChildren"/> = true.
    /// </remarks>
    public RadialContainer()
    {

    }
	
    /// <summary>
    /// Positions child controls in a radial layout and updates the container's Radius based on child count.
    /// </summary>
    /// <remarks>
    /// Calculates an angular arc from <see cref="AngularRange"/>, normalizes it into [0, Tau), and reverses direction when <see cref="RadialAlignment"/> is <c>AntiClockwise</c>.
    /// The method optionally considers only visible children depending on <see cref="ReserveSpaceForHiddenChildren"/>. It updates <see cref="Radius"/> to add padding proportional to the number of considered children to reduce overlap, computes an even angular separation (treating a full circle specially), and places each child at polar coordinates converted to screen positions centered in the container.
    /// </remarks>
    protected override void Draw(DrawingHandleScreen handle)
    {
		
        const float baseRadius = 100f;
        const float radiusIncrement = 5f;
		
        var children = ReserveSpaceForHiddenChildren ? Children : Children.Where(x => x.Visible);
        var childCount = children.Count();
		
		// Add padding from the center at higher child counts so they don't overlap.
		Radius = baseRadius + (childCount * radiusIncrement);

        // Determine the size of the arc, accounting for clockwise and anti-clockwise arrangements
        var arc = AngularRange.Y - AngularRange.X;
        arc = (arc < 0) ? MathF.Tau + arc : arc;
        arc = (RadialAlignment == RAlignment.AntiClockwise) ? MathF.Tau - arc : arc;

        // Account for both circular arrangements and arc-based arrangements
        var childMod = MathHelper.CloseTo(arc, MathF.Tau, 0.01f) ? 0 : 1;

        // Determine the separation between child elements
        var sepAngle = arc / (childCount - childMod);
        sepAngle *= (RadialAlignment == RAlignment.AntiClockwise) ? -1f : 1f;

        // Adjust the positions of all the child elements
        foreach (var (i, child) in children.Select((x, i) => (i, x)))
        {
            var position = new Vector2(Radius * MathF.Sin(AngularRange.X + sepAngle * i) + Width / 2f - child.Width / 2f, -Radius * MathF.Cos(AngularRange.X + sepAngle * i) + Height / 2f - child.Height / 2f);
            SetPosition(child, position);
        }
    }

    /// <summary>
    /// Specifies the different radial alignment modes
    /// </summary>
    /// <seealso cref="RadialAlignment"/>
    public enum RAlignment : byte
    {
        Clockwise,
        AntiClockwise,
    }
}