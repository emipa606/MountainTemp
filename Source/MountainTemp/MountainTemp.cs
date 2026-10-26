//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using esm.ModConfigurationMenus;
using UnityEngine;
using Verse;

namespace esm.MapComponents;

public struct NaturalRoom
{
    public Room Room;
    public float NaturalEqualizationFactor;

    public float NaturalTemp;
}

[StaticConstructorOnStartup]
public abstract class MountainTemp(Map map) : MapComponent(map)
{
    // Constant delta for setting temp
    private const float TemperatureDelta = 0.49f;

    // Constant equalization factor
    private const float EqualizationFactor = 120.0f * 90.0f * 6f;

    private readonly bool enabled = map.Biome.defName != "BMT_EarthenDepths";

    // This will house all the rooms in the world which have
    // natural roofs
    private List<NaturalRoom> naturalRooms = [];

    // Target underground temperature
    private float TargetTemperature
    {
        get
        {
            switch (McmMountainTempMod.instance.Settings.TargetMode)
            {
                case McmMountainTempModSettings.TemperatureMode.Fixed:
                    return McmMountainTempMod.instance.Settings.FixedTarget;

                case McmMountainTempModSettings.TemperatureMode.Seasonal:
                    return SeasonalAverage(map);

                case McmMountainTempModSettings.TemperatureMode.Annual:
                    return AnnualAverage(map);

                default:
                    throw new Exception("MountainTemp :: Invalid temperature target mode");
            }
        }
    }

    public static float SeasonalAverage(Map map)
    {
        return Find.World.tileTemperatures.GetSeasonalTemp(map.Tile);
    }

    public static float AnnualAverage(Map map)
    {
        return Find.WorldGrid[map.Tile].temperature;
    }

    /// <summary>
    ///     Fetch the mountain rooms with thick roofs and no heating/cooling devices.
    /// </summary>
    private void fetchNaturalRooms()
    {
        // Clear our list of natural rooms
        naturalRooms = [];

        // Get the list of rooms from the region grid
        var allRooms = map.regionGrid.AllRooms;

        // No rooms to check, abort now
        if (allRooms == null || allRooms.Count < 1)
        {
            return;
        }

        // Get the outdoor temperature
        var outdoorTemp = GenTemperature.GetTemperatureAtTile(map.Tile);

        // Iterate the rooms
        foreach (var room in allRooms)
        {
            // Open roof?  Not what we want
            try
            {
                if (room.OpenRoofCount > 0)
                {
                    continue;
                }

                // Get the cells which are contained in the room
                var roomCells = room.Cells.ToList();

                // No cells, no want
                if (roomCells.Count < 1)
                {
                    continue;
                }

                // Make sure the roof is at least partly natural
                if (!roomCells.Any(cell => cell.GetRoof(map).isNatural))
                {
                    continue;
                }

                // Check for embrasures and fences, anything that transfers heat freely
                if (room.BorderCells.Any(cell => cell.GetCover(map).def.fillPercent < 1))
                {
                    continue;
                }

                // Create new natural room entry
                var naturalRoom = new NaturalRoom
                {
                    Room = room
                };

                var roofCount = (float)roomCells.Count;
                float thickCount = 0;
                float thinCount = 0;

                // Count thick roofs
                foreach (var cell in roomCells)
                {
                    var roof = cell.GetRoof(map);
                    if (roof.isThickRoof)
                    {
                        thickCount++;
                    }
                    else if (roof.isNatural)
                    {
                        thinCount++;
                    }
                }

                // Now calculate percent of roof that is thick/thin/constructed
                var thickFactor = thickCount / roofCount;
                var thinFactor = thinCount / roofCount;

                // Factor for pushing heat
                naturalRoom.NaturalEqualizationFactor = thickFactor + (thinFactor * 0.5f);

                // Calculate new temp based on roof factors
                var thickRate = thickFactor * TargetTemperature;
                var thinRate = thinFactor * (outdoorTemp - TargetTemperature) * 0.25f;

                // Assign the natural temp based on aggregate ratings
                naturalRoom.NaturalTemp = thickRate + thinRate;

                // Add the natural room to the list
                naturalRooms.Add(naturalRoom);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    /// <summary>
    ///     Map tick for component
    /// </summary>
    public override void MapComponentTick()
    {
        if (!enabled)
        {
            return;
        }

        // Only do this once every update ticks
        if (Find.TickManager.TicksGame % McmMountainTempMod.instance.Settings.UpdateTicks != 0)
        {
            return;
        }

        // Get the all the natural rooms in the world
        fetchNaturalRooms();

        // No rooms, nothing to do
        if (naturalRooms == null || naturalRooms.Count < 1)
        {
            return;
        }

        // Go through rooms and set the temperature
        foreach (var naturalRoom in naturalRooms)
        {
            var equalizationRate = naturalRoom.NaturalEqualizationFactor;
            var targetTemp = naturalRoom.NaturalTemp;


            // Move the room towards the desired temp
            var tempDelta = Mathf.Abs(naturalRoom.Room.Temperature - targetTemp);

            if (tempDelta > TemperatureDelta)
                // Difference is too large, move it
            {
                equalizeTemperature(naturalRoom.Room, targetTemp, equalizationRate);
            }
            else
                // Difference is within tolerance, set it

            {
                naturalRoom.Room.Temperature = targetTemp;
            }
        }
    }

    /// <summary>
    ///     Equalizes the temperature of the room.
    /// </summary>
    /// <param name="room">Room.</param>
    /// <param name="destTemp">Destination temp.</param>
    /// <param name="equalizationRate"></param>
    private static void equalizeTemperature(Room room, float destTemp, float equalizationRate)
    {
        // Temperature delta
        var delta = Mathf.Abs(room.Temperature - destTemp);
        // Movement delta
        var movement = Mathf.Min(
            (delta >= 100.0f ? (float)((0.000300000014249235 * delta) - 0.025000000372529) : 5E-05f * delta) *
            (0.22f * Mathf.Pow(room.CellCount, 0.33f)),
            delta);
        // Change delta
        var change = (destTemp > room.Temperature ? movement : -movement) *
            (EqualizationFactor * equalizationRate) / room.CellCount;
        // Push some heat
        room.Temperature += change;
    }
}