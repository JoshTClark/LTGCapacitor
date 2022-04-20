using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using System.IO;
using System.Reflection;
using UnityEngine;
using static R2API.SoundAPI;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace LTGCapacitor
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(SoundAPI), nameof(ItemAPI), nameof(NetworkingAPI))]

    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "JTPuff";
        public const string PluginName = "LTGCapacitor";
        public const string PluginVersion = "1.0.3";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Awake is automatically called by Unity")]
        public void Awake()
        {
            NetworkingAPI.RegisterMessageType<SyncSound>();

            Log.Init(Logger);

            using (Stream bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LTGCapacitor.NOW.bnk"))
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
                    Log.LogInfo("Check");
                    self.UpdateTargets(EquipmentCatalog.FindEquipmentIndex(RoR2.RoR2Content.Equipment.Lightning.name), false);
                    HurtBox hurtBox = self.currentTarget.hurtBox;
                    if (hurtBox)
                    {
                        Log.LogInfo("Check2");
                        Vector3 soundPosition;
                        soundPosition = self.currentTarget.hurtBox.gameObject.transform.position;
                        SimpleTimer t = self.characterBody.gameObject.AddComponent<SimpleTimer>();
                        t.time = 0.5f;
                        t.OnTimerEnd += () =>
                        {
                            Log.LogInfo("Check3");
                            GameObject gameObj = new GameObject();
                            gameObj.transform.position = soundPosition;
                            AkSoundEngine.PostEvent("Now", gameObj);
                            new SyncSound(soundPosition).Send(NetworkDestination.Clients);
                        };
                    }
                }
                return orig(self, equipmentDef);
            };
        }
    }

    public class SyncSound : INetMessage
    {
        Vector3 soundPosition;

        public SyncSound()
        {
        }

        public SyncSound(Vector3 soundPosition)
        {
            this.soundPosition = soundPosition;
        }

        public void Deserialize(NetworkReader reader)
        {
            soundPosition = reader.ReadVector3();
        }

        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                Log.LogMessage("SyncSound: Host ran this. Skip.");
                return;
            }
            GameObject bodyObject = new GameObject();
            bodyObject.transform.position = soundPosition;
            AkSoundEngine.PostEvent("Now", bodyObject);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(soundPosition);
        }
    }
}
