using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(TLDRadioSyncFix.RadioSyncFixMod), "TLD Radio Sync Fix", "1.0.0", "you")]
// IMPORTANTE: troque "The Long Drive" pelo nome interno exato do executável
// se o MelonLoader reclamar ao carregar (normalmente é o nome do .exe sem extensão).
[assembly: MelonGame(null, null)]

namespace TLDRadioSyncFix
{
    public class RadioSyncFixMod : MelonMod
    {
        // Threshold original no jogo = 0.5s. Configurável via MelonPreferences
        // pra você poder testar valores diferentes sem recompilar.
        public static MelonPreferences_Category Prefs;
        public static MelonPreferences_Entry<float> Threshold;

        public override void OnInitializeMelon()
        {
            Prefs = MelonPreferences.CreateCategory("TLDRadioSyncFix");
            Threshold = Prefs.CreateEntry(
                "ResyncThreshold",
                2.5f, // valor novo sugerido - original era 0.5f
                description: "Segundos de deriva entre o clock manual (cTime) e o AudioSource antes de forcar um seek. Original do jogo = 0.5."
            );

            LoggerInstance.Msg($"[RadioSyncFix] Carregado. Threshold atual: {Threshold.Value}s (original do jogo: 0.5s).");
        }
    }

    // Patch cirurgico: só troca a constante 0.5f usada no SetRadio pelo valor
    // configuravel acima. Não reescreve a lógica do método, só relaxa o
    // gatilho do resync forçado (que é o que causa o "wobble"/seek audível).
    [HarmonyPatch(typeof(radioscript), "SetRadio")]
    public static class Patch_radioscript_SetRadio
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int replaced = 0;
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_R4 && ins.operand is float f && Math.Abs(f - 0.5f) < 0.0001f)
                {
                    ins.operand = RadioSyncFixMod.Threshold.Value;
                    replaced++;
                }
                yield return ins;
            }

            MelonLogger.Msg($"[RadioSyncFix] Transpiler aplicado em SetRadio, {replaced} constante(s) 0.5f substituida(s).");
        }
    }
}
