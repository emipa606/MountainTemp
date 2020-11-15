using System;
using Verse;
using UnityEngine;

namespace esm
{

    [StaticConstructorOnStartup]
    public class MCM_MountainTempMod : Mod
    {
        private const GameFont fontLabel = GameFont.Small;
        private const GameFont fontCheckbox = GameFont.Small;
        private const GameFont fontSlider = GameFont.Tiny;

        private const float entrySize = 24f;
        private const float innerPadding = 4f;

        public MCM_MountainTempMod(ModContentPack content) : base(content)
        {
            instance = this;
        }
        public override string SettingsCategory()
        {
            return "MountainTemp";
        }

        /// <summary>
        /// The instance-settings for the mod
        /// </summary>
        internal MCM_MountainTempModSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = GetSettings<MCM_MountainTempModSettings>();
                }
                return settings;
            }
            set => settings = value;
        }

        #region Widgets

        private void DoRadio(Rect rect, ref bool value, string labelKey, string temp = "")
        {
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;

            Text.Font = fontCheckbox;
            Text.Anchor = TextAnchor.MiddleLeft;

            var label = labelKey.Translate();

            var radioVec = new Vector2(
                rect.x,
                rect.y + ((rect.height - entrySize) / 2));
            var labelWidth = Text.CalcSize(label).x;
            var labelRect = new Rect(
                rect.x + entrySize + innerPadding,
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
                    labelRect.x + labelRect.width + innerPadding,
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

        private void DoSlider(Rect rect, ref float value, string labelKey, float min, float max, float setMax = float.MinValue, float setMin = float.MaxValue)
        {
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;

            Text.Font = fontSlider;
            Text.Anchor = TextAnchor.MiddleCenter;

            var label = labelKey.Translate(GenText.ToStringTemperature(value));
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
                (min + 0.01f > setMin) &&
                (value < min + 0.01f)
            )
            {
                value = setMin;
            }
            if (
                (max - 0.01f < setMax) &&
                (value > max - 0.01f)
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

            Text.Font = fontCheckbox;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(
                rect,
                label
            );

            Text.Anchor = originalAnchor;
            Text.Font = originalFont;
        }

        #endregion


        public override void DoSettingsWindowContents(Rect rect)
        {
            #region Save State
            var originalFont = Text.Font;
            #endregion

            #region MCM Description
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
            #endregion

            #region Target Label
            var targetRect = new Rect(
                0,
                descriptionRect.y + descriptionRect.height + entrySize,
                rect.width,
                entrySize);
            DoLabel(
                targetRect,
                "MountainTempMCMTarget".Translate()
            );
            #endregion

            #region Fixed Temp Radio
            var radioBool = Settings.TargetMode == MCM_MountainTempModSettings.TemperatureMode.Fixed;
            var fixedRect = new Rect(
                0,
                targetRect.y + targetRect.height + innerPadding,
                rect.width,
                entrySize);
            DoRadio(
                fixedRect,
                ref radioBool,
                "MountainTempMCMFixed");
            if (radioBool)
            {
                Settings.TargetMode = MCM_MountainTempModSettings.TemperatureMode.Fixed;
            }
            #endregion

            #region Fixed Temp Slider
            var sliderMin = GenTemperature.CelsiusTo(-50f, Prefs.TemperatureMode);
            var sliderMax = GenTemperature.CelsiusTo(50f, Prefs.TemperatureMode);
            var sliderValue = GenTemperature.CelsiusTo(Settings.FixedTarget, Prefs.TemperatureMode);
            var tempRect = new Rect(
                0,
                fixedRect.y + fixedRect.height + innerPadding,
                rect.width,
                entrySize * 2);
            DoSlider(
                tempRect,
                ref sliderValue,
                "MountainTempMCMSlider",
                sliderMin,
                sliderMax
            );
            Settings.FixedTarget = CelsiusFrom(sliderValue, Prefs.TemperatureMode);
            #endregion

            #region Seasonal Temp Radio
            radioBool = Settings.TargetMode == MCM_MountainTempModSettings.TemperatureMode.Seasonal;
            var tempStr = Current.ProgramState == ProgramState.Playing ? string.Format("({0})", GenText.ToStringTemperature(MountainTemp.SeasonalAverage(Find.AnyPlayerHomeMap))) : "";
            var seasonalRect = new Rect(
                0,
                tempRect.y + tempRect.height + entrySize,
                rect.width,
                entrySize);
            DoRadio(
                seasonalRect,
                ref radioBool,
                "MountainTempMCMSeasonal",
                tempStr);
            if (radioBool)
            {
                Settings.TargetMode = MCM_MountainTempModSettings.TemperatureMode.Seasonal;
            }
            #endregion

            #region Annual Temp Radio
            radioBool = Settings.TargetMode == MCM_MountainTempModSettings.TemperatureMode.Annual;
            tempStr = Current.ProgramState == ProgramState.Playing ? string.Format("({0})", GenText.ToStringTemperature(MountainTemp.AnnualAverage(Find.AnyPlayerHomeMap))) : "";
            var annualRect = new Rect(
                0,
                seasonalRect.y + seasonalRect.height + innerPadding,
                rect.width,
                entrySize);
            DoRadio(
                annualRect,
                ref radioBool,
                "MountainTempMCMAnnual",
                tempStr);
            if (radioBool)
            {
                
                Settings.TargetMode = MCM_MountainTempModSettings.TemperatureMode.Annual;
            }
            #endregion

            #region Restore State
            Text.Font = originalFont;
            #endregion
            Settings.Write();

        }

        
        /// <summary>
        /// The private settings
        /// </summary>
        private MCM_MountainTempModSettings settings;

        public static float CelsiusFrom(float temp, TemperatureDisplayMode oldMode)
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

        /// <summary>
        /// The instance of the settings to be read by the mod
        /// </summary>
        public static MCM_MountainTempMod instance;

    }


}
