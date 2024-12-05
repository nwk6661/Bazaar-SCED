using HarmonyLib;
using TheBazaar.UI.Tooltips;
using TheBazaar.Tooltips;
using UnityEngine;

using ShowCombatEncounterDetail;

[HarmonyPatch(typeof(CardTooltipController), "StartTooltipFadeIn")]
public class Infarctus_Hook_Tooltip_FadeIn
{
    public static void Postfix()
    {
        InfarctusPluginCombatEncounterInfo.clean_destroy();
        if(InfarctusPluginCombatEncounterInfo.IsPveEncounter && InfarctusPluginCombatEncounterInfo.ToolTipCardName!=""){
            InfarctusPluginCombatEncounterInfo.CreateImageDisplayFromCardName();
        }
    }
}
[HarmonyPatch(typeof(CardTooltipController), "StartTooltipFadeOut")]
public class Infarctus_Hook_Tooltip_FadeOut
{
    public static void Postfix()
    {
        InfarctusPluginCombatEncounterInfo.IsPveEncounter = false;
        InfarctusPluginCombatEncounterInfo.ToolTipCardName = "";
        InfarctusPluginCombatEncounterInfo.clean_destroy();
    }
}
[HarmonyPatch(typeof(CardTooltipData), "GetPVEEncounterLevel")]
public class Infarctus_Hook_GetPVEEncounterLevel
{
    public static void Postfix(CardTooltipData __instance, ref uint __result)
    {
        InfarctusPluginCombatEncounterInfo.ToolTipCardName = __instance.GetTitle();
        InfarctusPluginCombatEncounterInfo.IsPveEncounter = true;
    }
}