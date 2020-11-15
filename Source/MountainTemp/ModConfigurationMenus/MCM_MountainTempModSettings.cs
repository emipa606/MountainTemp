using System;
using Verse;
using UnityEngine;

namespace esm
{
    internal class MCM_MountainTempModSettings : ModSettings
    {
        public TemperatureMode TargetMode = TemperatureMode.Annual;
        public float FixedTarget = 10.0f;

        public enum TemperatureMode
        {
            Fixed,
            Seasonal,
            Annual
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref TargetMode, "TargetMode", TemperatureMode.Annual, true);
            Scribe_Values.Look(ref FixedTarget, "FixedTarget", 10.0f, true);
        }
    }

}
