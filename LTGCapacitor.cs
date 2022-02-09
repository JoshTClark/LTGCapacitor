using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.IO;
using System.Reflection;
using UnityEngine;
using static R2API.SoundAPI;

namespace LTGCapcitor
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(SoundAPI), nameof(ItemAPI))]
	
    public class LTGCapacitor : BaseUnityPlugin
	{
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "JTPuff";
        public const string PluginName = "LTGCapacitor";
        public const string PluginVersion = "1.0.0";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Awake is automatically called by Unity")]
        public void Awake()
        {
            Log.Init(Logger);

            using (Stream bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LTGCapacitor.ThalassophobiaSounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundBanks.Add(bytes);
            }

            Hooks();


            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += (On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef) =>
            {
                if (equipmentDef == RoR2.RoR2Content.Equipment.Lightning)
                {
                    self.UpdateTargets();
                    HurtBox hurtBox = self.currentTarget.hurtBox;
                    if (hurtBox)
                    {
                        GameObject gameObj = new GameObject();
                        gameObj.transform.position = self.currentTarget.hurtBox.gameObject.transform.position;
                        SimpleTimer t = self.characterBody.gameObject.AddComponent<SimpleTimer>();
                        t.time = 0.5f;
                        t.OnTimerEnd += () =>
                        {
                            AkSoundEngine.PostEvent("Now", gameObj);
                        };
                    }
                }
                return orig(self, equipmentDef);
            };
        }
    }
}
