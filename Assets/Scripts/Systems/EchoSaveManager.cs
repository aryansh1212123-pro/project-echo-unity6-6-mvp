using System;
using System.IO;
using UnityEngine;

namespace ProjectEcho
{
    public sealed class EchoSaveManager
    {
        private const int CurrentSchema = 2;
        public string SavePath => Path.Combine(Application.persistentDataPath, "project_echo_save.json");

        public EchoGameState LoadOrCreate()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    EchoGameState loaded = JsonUtility.FromJson<EchoGameState>(File.ReadAllText(SavePath));
                    if (loaded != null && loaded.echo != null)
                    {
                        int previousSchema = loaded.schemaVersion;
                        if (previousSchema < 2 && loaded.worldKnowledge == null)
                            loaded.worldKnowledge = new System.Collections.Generic.List<WorldKnowledgeEntry>();
                        loaded.schemaVersion = CurrentSchema;
                        loaded.EnsureValid();
                        Debug.Log("Project Echo save loaded from " + SavePath);
                        return loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Could not read Project Echo save; starting a fresh Echo. " + ex.Message);
                TryRecoverBackup();
            }
            return CreateNewState();
        }

        public void Save(EchoGameState state)
        {
            if (state == null || state.echo == null) return;
            try
            {
                state.schemaVersion = CurrentSchema;
                state.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                state.EnsureValid();
                string directory = Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string temp = SavePath + ".tmp";
                string backup = SavePath + ".bak";
                File.WriteAllText(temp, JsonUtility.ToJson(state, true));
                if (File.Exists(SavePath))
                {
                    try { File.Replace(temp, SavePath, backup); }
                    catch
                    {
                        File.Copy(SavePath, backup, true);
                        File.Delete(SavePath);
                        File.Move(temp, SavePath);
                    }
                }
                else File.Move(temp, SavePath);
            }
            catch (Exception ex) { Debug.LogError("Project Echo could not save: " + ex.Message); }
        }

        private void TryRecoverBackup()
        {
            try
            {
                string backup = SavePath + ".bak";
                if (File.Exists(backup)) File.Copy(backup, SavePath, true);
            }
            catch (Exception ex) { Debug.LogWarning("Save backup recovery failed: " + ex.Message); }
        }

        private static EchoGameState CreateNewState()
        {
            string[] names = { "Luma", "Aster", "Morrow", "Vesper", "Neri", "Serein" };
            int index = Mathf.Abs(DateTime.UtcNow.DayOfYear * 17 + DateTime.UtcNow.Hour * 7) % names.Length;
            return new EchoGameState
            {
                schemaVersion = CurrentSchema,
                echo = new EchoProfile
                {
                    id = Guid.NewGuid().ToString("N"), displayName = names[index],
                    traits = new EchoTraits(), needs = new EchoNeeds(), playerRelationship = new EchoRelationship()
                }
            };
        }
    }
}
