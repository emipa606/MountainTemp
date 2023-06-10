using System;
using esm.MapComponents;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace esm.ModConfigurationMenus;

[StaticConstructorOnStartup]
public class McmMountainTempMod : Mod
{
    private const GameFont FontCheckbox = GameFont.Small;
    private const GameFont FontSlider = GameFont.Tiny;

    private const float EntrySize = 24f;
    private const float InnerPadding = 4f;

    public static McmMountainTempMod instance;
    private static string currentVersion;

    /// <summary>
    ///     The private settings
    /// </summary>
    public readonly McmMountainTempModSettings Settings;

    private string intBuffer;

    public McmMountainTempMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings = GetSettings<McmMountainTempModSettings>();
    }

    public override string SettingsCategory()
    {
        return "Mountain Temp";
    }

    public override void DoSettingsWindowContents(Rect rect)
    {
        var originalFont = Text.Font;

        var descriptionLabel = "MountainTempMCMDescription".Translate();
        var descriptionHeight = Text.CalcHeight(descriptionLabel, rect.width);
        var descriptionRect = new Rect(
            0,
            40,
            rect.width,
            descriptionHeight);
        DoLabel(
            descriptionRect,
            descriptionLabel
        );

        var targetRect = new Rect(
            0,
            descriptionRect.y + descriptionRect.height + EntrySize,
            rect.width,
            EntrySize);
        DoLabel(
            targetRect,
            "MountainTempMCMTarget".Translate()
        );

        var radioBool = Settings.TargetMode == McmMountainTempModSettings.TemperatureMode.Fixed;
        var fixedRect = new Rect(
            0,
            targetRect.y + targetRect.height + InnerPadding,
            rect.width,
            EntrySize);
        DoRadio(
            fixedRect,
            ref radioBool,
            "MountainTempMCMFixed");
        if (radioBool)
        {
            Settings.TargetMode = McmMountainTempModSettings.TemperatureMode.Fixed;
        }

        var sliderMin = GenTemperature.CelsiusTo(-50f, Prefs.TemperatureMode);
        var sliderMax = GenTemperature.CelsiusTo(50f, Prefs.TemperatureMode);
        var sliderValue = GenTemperature.CelsiusTo(Settings.FixedTarget, Prefs.TemperatureMode);
        var tempRect = new Rect(
            0,
            fixedRect.y + fixedRect.height + InnerPadding,
            rect.width,
            EntrySize * 2);
        DoSlider(
            tempRect,
            ref sliderValue,
            "MountainTempMCMSlider",
            sliderMin,
            sliderMax
        );
        Settings.FixedTarget = CelsiusFrom(sliderValue, Prefs.TemperatureMode);

        radioBool = Settings.TargetMode == McmMountainTempModSettings.TemperatureMode.Seasonal;
        var tempStr = Current.ProgramState == ProgramState.Playing
            ? $"({MountainTemp.SeasonalAverage(Find.AnyPlayerHomeMap).ToStringTemperature()})"
            : "";
        var seasonalRect = new Rect(
            0,
            tempRect.y + tempRect.height + EntrySize,
            rect.width,
            EntrySize);
        DoRadio(
            seasonalRect,
            ref radioBool,
            "MountainTempMCMSeasonal",
            tempStr);
        if (radioBool)
        {
            Settings.TargetMode = McmMountainTempModSettings.TemperatureMode.Seasonal;
        }

        radioBool = Settings.TargetMode == McmMountainTempModSettings.TemperatureMode.Annual;
        tempStr = Current.ProgramState == ProgramState.Playing
            ? $"({MountainTemp.AnnualAverage(Find.AnyPlayerHomeMap).ToStringTemperature()})"
            : "";
        var annualRect = new Rect(
            0,
            seasonalRect.y + seasonalRect.height + InnerPadding,
            rect.width,
            EntrySize * 2);
        DoRadio(
            annualRect,
            ref radioBool,
            "MountainTempMCMAnnual",
            tempStr);
        if (radioBool)
        {
            Settings.TargetMode = McmMountainTempModSettings.TemperatureMode.Annual;
        }


        var updateLabelRect = new Rect(
            0,
            annualRect.y + annualRect.height + InnerPadding,
            rect.width,
            EntrySize);
        Widgets.Label(updateLabelRect, "MountainTempMCMUpdateSpeed".Translate(Settings.UpdateTicks));

        var updateSpeedRect = new Rect(
            0,
            updateLabelRect.y + updateLabelRect.height + InnerPadding,
            rect.width,
            EntrySize);
        Widgets.IntEntry(updateSpeedRect, ref Settings.UpdateTicks, ref intBuffer);

        Settings.UpdateTicks = Mathf.Clamp(Settings.UpdateTicks, 1, GenDate.TicksPerHour);
        intBuffer = Settings.UpdateTicks.ToString();

        if (currentVersion != null)
        {
            var versionRect = new Rect(
                0,
                updateSpeedRect.y + updateSpeedRect.height + InnerPadding,
                rect.width,
                EntrySize);
            GUI.contentColor = Color.gray;
            Widgets.Label(versionRect, "MountainTempMCMVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        Text.Font = originalFont;

        Settings.Write();
    }

    private static float CelsiusFrom(float temp, TemperatureDisplayMode oldMode)
    {
        switch (oldMode)
        {
            case TemperatureDisplayMode.Celsius:
                return temp;
            case TemperatureDisplayMode.Fahrenheit:
                return (float)((temp - 32.0) / 1.79999995231628);
            case TemperatureDisplayMode.Kelvin:
                return temp - 273.15f;
            default:
                throw new InvalidOperationException();
        }
    }

    private void DoRadio(Rect rect, ref bool value, string labelKey, string temp = "")
    {
        var originalFont = Text.Font;
        var originalAnchor = Text.Anchor;

        Text.Font = FontCheckbox;
        Text.Anchor = TextAnchor.MiddleLeft;

        var label = labelKey.Translate();

        var radioVec = new Vector2(
            rect.x,
            rect.y + ((rect.height - EntrySize) / 2));
        var labelWidth = Text.CalcSize(label).x;
        var labelRect = new Rect(
            rect.x + EntrySize + InnerPadding,
            rect.y,
            labelWidth,
            rect.height);

        value = Widgets.RadioButton(
            radioVec,
            value);

        Widgets.Label(
            labelRect,
            label
        );

        if (!string.IsNullOrEmpty(temp))
        {
            var tempRect = new Rect(
                labelRect.x + labelRect.width + InnerPadding,
                labelRect.y,
                Text.CalcSize(temp).x,
                rect.height);
            Widgets.Label(
                tempRect,
                temp
            );
        }

        Text.Anchor = originalAnchor;
        Text.Font = originalFont;
    }

    private void DoSlider(Rect rect, ref float value, string labelKey, float min, float max,
        float setMax = float.MinValue, float setMin = float.MaxValue)
    {
        var originalFont = Text.Font;
        var originalAnchor = Text.Anchor;

        Text.Font = FontSlider;
        Text.Anchor = TextAnchor.MiddleCenter;

        var label = labelKey.Translate(value.ToStringTemperature());
        var sectionHeight = rect.height / 2;

        var labelRect = new Rect(
            rect.x,
            rect.y,
            rect.width,
            sectionHeight);
        var sliderRect = new Rect(
            rect.x,
            rect.y + sectionHeight,
            rect.width,
            sectionHeight);

        Widgets.Label(
            labelRect,
            label
        );

        value = GUI.HorizontalSlider(
            sliderRect,
            value,
            min,
            max
        );

        if (
            min + 0.01f > setMin &&
            value < min + 0.01f
        )
        {
            value = setMin;
        }

        if (
            max - 0.01f < setMax &&
            value > max - 0.01f
        )
        {
            value = setMax;
        }

        Text.Anchor = originalAnchor;
        Text.Font = originalFont;
    }

    private void DoLabel(Rect rect, string label)
    {
        var originalFont = Text.Font;
        var originalAnchor = Text.Anchor;

        Text.Font = FontCheckbox;
        Text.Anchor = TextAnchor.MiddleLeft;

        Widgets.Label(
            rect,
            label
        );

        Text.Anchor = originalAnchor;
        Text.Font = originalFont;
    }
}