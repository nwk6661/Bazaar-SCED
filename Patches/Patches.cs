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
        if(InfarctusPluginCombatEncounterInfo.isPVEEncounter && InfarctusPluginCombatEncounterInfo.ToolTip_CardName!=""){
            InfarctusPluginCombatEncounterInfo.CreateImageDisplayFromCardName();
        }
    }
}
[HarmonyPatch(typeof(CardTooltipController), "StartTooltipFadeOut")]
public class Infarctus_Hook_Tooltip_FadeOut
{
    public static void Postfix()
    {
        InfarctusPluginCombatEncounterInfo.isPVEEncounter = false;
        InfarctusPluginCombatEncounterInfo.ToolTip_CardName = "";
        InfarctusPluginCombatEncounterInfo.clean_destroy();
    }
}
[HarmonyPatch(typeof(CardTooltipData), "GetPVEEncounterLevel")]
public class Infarctus_Hook_GetPVEEncounterLevel
{
    public static void Postfix(CardTooltipData __instance, ref uint __result)
    {
        InfarctusPluginCombatEncounterInfo.ToolTip_CardName = __instance.GetTitle();
        InfarctusPluginCombatEncounterInfo.isPVEEncounter = true;
    }
}