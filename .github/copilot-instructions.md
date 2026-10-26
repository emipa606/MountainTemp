# GitHub Copilot Instructions for Mountain Temp (Continued) Mod

## Mod Overview and Purpose

The **Mountain Temp (Continued)** mod is an update to the original Mountain Temp mod by 1000101. It aims to stabilize room temperatures underground by adjusting them towards either a seasonal average, an annual average, or a fixed point temperature, based on the thickness and presence of natural roofs. The mod ensures that rooms with any portion of a natural roof are influenced by these factors, while also ignoring rooms defined by embrasures or fences. Additionally, a setting for update speed has been added to allow for more control over how frequently the temperature adjustments occur.

## Key Features and Systems

- **Natural Roof Temperature Stabilization**: Rooms with natural roofs are adjusted towards a target temperature based on the thickness and coverage of the roof.
- **Temperature Modes**: Users can choose between Seasonal Average, Annual Average, or a Fixed Point Temperature for stabilization.
- **Embrasure/Fence Ignore**: Rooms defined by embrasures or fences are excluded from temperature adjustments.
- **Update Speed Setting**: Allows users to configure the frequency of temperature adjustments for optimal performance.

## Coding Patterns and Conventions

- **Mod Structure**: The mod follows a typical RimWorld mod structure with separate files for the main mod class, settings, and core logic.
- **Harmony Patching**: The mod uses Harmony patching to inject custom logic into vanilla RimWorld methods, allowing for seamless integration with existing game mechanics.
- **XML Integration**: While this mod does not use extensive XML integration, it follows best practices for defining and accessing mod settings and data.

## XML Integration

The Mountain Temp (Continued) mod does not heavily rely on XML for its functionality. However, it does use XML for defining mod settings. The settings are defined in the `ModSettings.xml` file, and the mod accesses these settings using the Mod Settings API provided by the ModCompatibilityManager (MCM) framework.

xml
<ModSettings>
  <Settings>
    <Setting>
      <Key>updateSpeed</Key>
      <Type>Float</Type>
      <Value>1.0</Value>
      <Label>Update Speed</Label>
      <Tooltip>The speed at which temperature adjustments are made.</Tooltip>
    </Setting>
  </Settings>
</ModSettings>


## Harmony Patching

The mod uses Harmony patching to inject custom logic into vanilla RimWorld methods. For example, the `MapComponentTick` method in the `MountainTemp.cs` file is patched to periodically check and adjust room temperatures based on the defined criteria.

csharp
[HarmonyPatch(typeof(Map), "MapComponentTick")]
public static class MountainTemp_MapComponentTick_Patch
{
    public static void Postfix(Map __instance)
    {
        if (MountainTemp.SettingsCategory.updateSpeed > 0)
        {
            MountainTemp.Instance.MapComponentTick(__instance);
        }
    }
}


## Suggestions for Copilot

When using GitHub Copilot for this mod, consider the following suggestions:

- **Focus on Harmony Patching**: Since Harmony patching is a core part of the mod, prioritize using Copilot to generate and review patches for vanilla RimWorld methods.
- **XML Integration**: Although minimal, ensure that Copilot generates correct XML for mod settings and data definitions.
- **Logic and Core Features**: Use Copilot to assist in developing the core logic for temperature stabilization and the update speed feature.
- **Mod Settings**: Leverage Copilot to generate UI elements and logic for the mod settings using the MCM framework.
- **Code Review and Testing**: Regularly review the generated code for adherence to coding conventions and ensure that it integrates seamlessly with the mod's existing functionality.

By following these instructions and utilizing GitHub Copilot effectively, developers can enhance the development process for the Mountain Temp (Continued) mod, ensuring stability and performance improvements for users.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

