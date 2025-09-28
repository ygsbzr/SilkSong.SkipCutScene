using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using GlobalEnums;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
namespace SkipCutscene;

[BepInPlugin("ygsbzr.SkipCutScene", "SkipCutscene", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(Plugin));
    }
    #region Cinematic
    /*[HarmonyPrefix,HarmonyPatch(typeof(CinematicSequence),"Begin")]
    public static bool PatchCinematic(CinematicSequence __instance)
    {
        Debug.Log("Patch Cinematic");
        __instance.Skip();
        return false;
    }*/
    [HarmonyPrefix, HarmonyPatch(typeof(FadeSequence), "Begin")]
    public static bool PatchFade(FadeSequence __instance)
    {
        Debug.Log("Patch FadeSequence");
        __instance.Skip();
        return false;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PlayMakerFSM), "Start")]
    public static bool PatchFSM(PlayMakerFSM __instance)
    {
        if (__instance.gameObject.name == "Cinematic Player" && __instance.FsmName == "Cinematic Control")
        {
            __instance.GetAction<Wait>("Wait", 4).time.Value = 0.01f;
        }
        if (__instance.gameObject.name == "Pinstress Interior Ground Sit" && __instance.FsmName == "Behaviour")
        {
            __instance.GetState("Cinematic").AddCoroutine(WaitSkip);
        }
        if (__instance.gameObject.name == "Plinney Inside" && __instance.FsmName == "Dialogue")
        {
            __instance.GetState("Cinematic").AddCoroutine(WaitSkip);
        }
        if (__instance.gameObject.name == "Doctor Fly" && __instance.FsmName == "Dialogue")
        {
            __instance.GetState("Cinematic").AddCoroutine(WaitSkip);
        }
        /*if (__instance.gameObject.name == "Intro Sequence" && __instance.FsmName == "First Challenge")
        {
            __instance.GetAction<Wait>("Wait For Beat End", 2).time.Value = 1f;
            __instance.GetAction<Wait>("Ready Wait", 1).time.Value = 1f;
            __instance.ChangeTransition("Wait For Beat End", "FINISHED", "Quick Start");
        }
        if (__instance.gameObject.name == "Silk Boss" && __instance.FsmName == "Control")
        {
            __instance.GetAction<Wait>("Title Up", 2).time.Value = 1f;
            __instance.GetState("Intro Roar").RemoveAction<Wait>();
        }
        if (__instance.gameObject.name == "Boss Title" && __instance.FsmName == "Title Control")
        {
            __instance.GetAction<Wait>("Title Up", 1).time.Value = 1f;
        }
        */
        return true;
    }
    public static bool PatchAnimator(AnimatorSequence __instance)
    {
        Debug.Log("Patch AnimatorSequence");
        __instance.Skip();
        return false;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(InputHandler), "SetSkipMode")]
    public static bool PatchInput(ref SkipPromptMode newMode)
    {
        Debug.Log("Patch InputHandler");
        if ((newMode is not (SkipPromptMode.SKIP_INSTANT or SkipPromptMode.SKIP_PROMPT)) && !UnskipScene.Contains(SceneManager.GetActiveScene().name))
        {
            newMode = SkipPromptMode.SKIP_INSTANT;
        }
        return true;
    }
    private static IEnumerator WaitSkip()
    {
        yield return new WaitForSeconds(1f);
        GameManager.instance.inputHandler.SetSkipMode(SkipPromptMode.SKIP_INSTANT);
    }
    #endregion
    #region FastMenu
    [HarmonyPostfix, HarmonyPatch(typeof(StartManager), "Start")]
    public static void PatchLogo(StartManager __instance)
    {
        __instance.gameObject.GetComponent<Animator>().speed = 99999;
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(UIManager), "HideSaveProfileMenu")]
    public static IEnumerable<CodeInstruction> PatchUIWait(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.Instructions();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(UIManager), "HideCurrentMenu")]
    public static IEnumerable<CodeInstruction> PatchUIWait2(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.Instructions();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(UIManager), "HideMenu")]
    public static IEnumerable<CodeInstruction> PatchUIWait3(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.Instructions();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(UIManager), "ShowMenu")]
    public static IEnumerable<CodeInstruction> PatchUIWait4(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.Instructions();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(UIManager), "GoToProfileMenu")]
    public static IEnumerable<CodeInstruction> PatchUIWait5(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.Instructions();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(SaveSlotButton), "AnimateToSlotState")]
    public static IEnumerable<CodeInstruction> PatchUIWait6(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.InstructionEnumeration();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(GameManager), "RunContinueGame", MethodType.Enumerator)]
    public static IEnumerable<CodeInstruction> PatchGM(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 1f)).SetOperandAndAdvance(0.05f);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 1.6f)).SetOperandAndAdvance(0.05f);
        return cm.InstructionEnumeration();
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(GameManager), "PauseGameToggle", MethodType.Enumerator)]
    public static IEnumerable<CodeInstruction> PatchPause(IEnumerable<CodeInstruction> instruct)
    {
        var cm = new CodeMatcher(instruct);
        cm.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4)).Repeat(matcher => matcher.SetOperandAndAdvance(0f));
        return cm.InstructionEnumeration();
    }
    #endregion
    #region fastloads
    private static readonly float[] SKIP = { 0.4f, .165f };
    #endregion
    #region fastText
    #endregion
    private static readonly List<string> UnskipScene = new(){ "Belltown", "Room_Pinstress", "Belltown_Room_pinsmith", "Belltown_Room_doctor" };
    }
