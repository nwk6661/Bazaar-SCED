using TheBazaar.Tooltips;
using TheBazaar.UI.Tooltips;

namespace ShowCombatEncounterDetail.Hooks;

class Hooks
{
    public static void InitializeHooks()
    {
        On.TheBazaar.UI.Tooltips.CardTooltipController.StartTooltipFadeIn += CardTooltipController_StartTooltipFadeIn;
        On.TheBazaar.UI.Tooltips.CardTooltipController.StartTooltipFadeOut += CardTooltipController_StartTooltipFadeOut;
        On.TheBazaar.Tooltips.CardTooltipData.GetPVEEncounterLevel += CardTooltipData_GetPVEEncounterLevel;
    }

    private static void CardTooltipController_StartTooltipFadeIn(
        On.TheBazaar.UI.Tooltips.CardTooltipController.orig_StartTooltipFadeIn orig, CardTooltipController self)
    {
        orig(self);
        ShowCombatEncounterDetail.TooltipManager.CreateImageDisplayFromCardName();
    }

    private static void CardTooltipController_StartTooltipFadeOut(
        On.TheBazaar.UI.Tooltips.CardTooltipController.orig_StartTooltipFadeOut orig, CardTooltipController self)
    {
        orig(self);
        ShowCombatEncounterDetail.TooltipManager.IsPveEncounter = false;
        ShowCombatEncounterDetail.TooltipManager.ToolTipCardName = "";
        ShowCombatEncounterDetail.TooltipManager.CleanDestroy();
    }

    private static uint CardTooltipData_GetPVEEncounterLevel(
        On.TheBazaar.Tooltips.CardTooltipData.orig_GetPVEEncounterLevel orig, CardTooltipData self)
    {
        var result = orig(self);
        ShowCombatEncounterDetail.TooltipManager.ToolTipCardName = self.GetTitle();
        ShowCombatEncounterDetail.TooltipManager.IsPveEncounter = true;
        return result;
    }

    public static void UninitializeHooks()
    {
        On.TheBazaar.UI.Tooltips.CardTooltipController.StartTooltipFadeIn -= CardTooltipController_StartTooltipFadeIn;
        On.TheBazaar.UI.Tooltips.CardTooltipController.StartTooltipFadeOut -= CardTooltipController_StartTooltipFadeOut;
        On.TheBazaar.Tooltips.CardTooltipData.GetPVEEncounterLevel -= CardTooltipData_GetPVEEncounterLevel;
    }
}