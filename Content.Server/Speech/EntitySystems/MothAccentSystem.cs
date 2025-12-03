// SPDX-FileCopyrightText: 2023 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 router <messagebus@vk.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    //Maid edit start
    [Dependency] private readonly IRobustRandom _random = default!;
    private static readonly Regex RURegexLowerZH = new("ж+");
    private static readonly Regex RURegexUpperZH = new("ж+");
    private static readonly Regex RURegexLowerZ = new("ж+");
    private static readonly Regex RURegexUpperZ = new("ж+");
    //Maid edit end

    private static readonly Regex RegexLowerBuzz = new Regex("z{1,3}");
    private static readonly Regex RegexUpperBuzz = new Regex("Z{1,3}");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        //Maid edit start
        message = RURegexLowerZH.Replace(message, _random.Pick(new List<string>() { "жж", "жжж" }));
        message = RURegexUpperZH.Replace(message, _random.Pick(new List<string>() { "ЖЖ", "ЖЖЖ" }));
        message = RURegexLowerZ.Replace(message, _random.Pick(new List<string>() { "зз", "ззз" }));
        message = RURegexUpperZ.Replace(message, _random.Pick(new List<string>() { "ЗЗ", "ЗЗЗ" }));
        //Maid edit end

        // buzzz
        message = RegexLowerBuzz.Replace(message, "zzz");
        // buZZZ
        message = RegexUpperBuzz.Replace(message, "ZZZ");

        args.Message = message;
    }
}
