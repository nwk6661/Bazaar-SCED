using HarmonyLib;
using TheBazaar.UI.Tooltips;
using TheBazaar.Tooltips;

namespace ShowCombatEncounterDetail.Patches;

class Patches
{
    [HarmonyPatch(typeof(CardTooltipController), "StartTooltipFadeIn")]
    [HarmonyPostfix]
    private static void CardTooltipController_StartTooltipFadeIn(CardTooltipController __instance)
    {
        if (ShowCombatEncounterDetail.IsPveEncounter && ShowCombatEncounterDetail.ToolTipCardName != "")
        {
            ShowCombatEncounterDetail.CreateImageDisplayFromCardName();
        }
    }

    [HarmonyPatch(typeof(CardTooltipController), "StartTooltipFadeOut")]
    [HarmonyPostfix]
    private static void CardTooltipController_StartTooltipFadeOut(CardTooltipController __instance)
    {
        ShowCombatEncounterDetail.IsPveEncounter = false;
        ShowCombatEncounterDetail.ToolTipCardName = "";
        ShowCombatEncounterDetail.CleanDestroy();
    }

    [HarmonyPatch(typeof(CardTooltipData), nameof(CardTooltipData.GetPVEEncounterLevel))]
    [HarmonyPostfix]
    private static void CardTooltipController_StartTooltipFadeOut(CardTooltipData __instance)
    {
        ShowCombatEncounterDetail.ToolTipCardName = __instance.GetTitle();
        ShowCombatEncounterDetail.IsPveEncounter = true;
    }
}
