// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!; //Maid edit

    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    //Maid edit start
    private static readonly Regex RURegexLowerSh= new("ш+");
    private static readonly Regex RURegexUpperSh= new("Ш+");
    private static readonly Regex RURegexLowerСh= new("ч+");
    private static readonly Regex RURegexUpperСh= new("Ч+");
    private static readonly Regex RURegexLowerS = new("с+");
    private static readonly Regex RURegexUpperS = new("С+");
    private static readonly Regex RURegexLowerZ = new("з+");
    private static readonly Regex RURegexUpperZ = new("З+");
    //Maid edit end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        //Maid edit end
        message = RURegexLowerSh.Replace(message, _random.Pick(new List<string>() { "шш", "шшш" }));
        message = RURegexUpperSh.Replace(message, _random.Pick(new List<string>() { "ШШ", "ШШШ" }));
        message = RURegexLowerСh.Replace(message, _random.Pick(new List<string>() { "щщ", "щщщ" }));
        message = RURegexUpperСh.Replace(message, _random.Pick(new List<string>() { "ЩШ", "ЩЩЩ" }));
        message = RURegexLowerS.Replace(message, _random.Pick(new List<string>() { "сс", "ссс" }));
        message = RURegexUpperS.Replace(message, _random.Pick(new List<string>() { "СС", "ССС" }));
        message = RURegexLowerZ.Replace(message, _random.Pick(new List<string>() { "сс", "ссс" }));
        message = RURegexUpperZ.Replace(message, _random.Pick(new List<string>() { "СС", "ССС" }));
        //Maid edit end

        // hissss
        message = RegexLowerS.Replace(message, "sss");
        // hiSSS
        message = RegexUpperS.Replace(message, "SSS");
        // ekssit
        message = RegexInternalX.Replace(message, "$1kss");
        // ecks
        message = RegexLowerEndX.Replace(message, "ecks$1");
        // eckS
        message = RegexUpperEndX.Replace(message, "ECKS$1");

        args.Message = message;
    }
}
