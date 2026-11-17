using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectEcho
{
    public sealed class EchoSaveManager
    {
        private const int CurrentSchema = 2;
        private bool writesBlocked;
        private bool warnedAboutBlockedWrites;
        public string SavePath => Path.Combine(Application.persistentDataPath, "project_echo_save.json");

        public EchoGameState LoadOrCreate()
        {
            string backupPath = SavePath + ".bak";
            if (File.Exists(SavePath))
            {
                try
                {
                    EchoGameState primary = ReadAndMigrate(SavePath);
                    if (primary != null)
                    {
                        Debug.Log("Project Echo save loaded from " + SavePath);
                        return primary;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Primary Project Echo save could not be loaded: " + ex.Message);
                }
            }

            if (File.Exists(backupPath))
            {
                try
                {
                    EchoGameState recovered = ReadAndMigrate(backupPath);
                    if (recovered != null)
                    {
                        // Restore only after the backup has parsed and passed migration/validation.
                        // Keep the backup intact until a later successful save rotates it.
                        if (!writesBlocked) File.Copy(backupPath, SavePath, true);
                        Debug.LogWarning("Project Echo recovered its profile from the backup save.");
                        return recovered;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Project Echo backup could not be loaded: " + ex.Message);
                }
            }

            return CreateNewState();
        }

        public void Save(EchoGameState state)
        {
            if (writesBlocked)
            {
                if (!warnedAboutBlockedWrites)
                {
                    Debug.LogWarning("Project Echo did not save: this profile uses a newer save schema. Open it with a compatible newer game version to avoid overwriting data.");
                    warnedAboutBlockedWrites = true;
                }
                return;
            }
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

        private EchoGameState ReadAndMigrate(string path)
        {
            EchoGameState state = JsonUtility.FromJson<EchoGameState>(File.ReadAllText(path));
            if (state == null) throw new InvalidDataException("Save file could not be deserialized.");

            if (state.schemaVersion > CurrentSchema)
            {
                writesBlocked = true;
                if (state.echo == null)
                    throw new InvalidDataException("Save uses a newer schema with an incompatible profile shape; writes are disabled to protect the original file.");
                state.EnsureValid();
                Debug.LogWarning("Project Echo save schema " + state.schemaVersion + " is newer than this build's schema " + CurrentSchema + ". Saves are disabled to protect the original file.");
                return state;
            }

            if (state.echo == null) throw new InvalidDataException("Save file has no Echo profile.");
            if (state.schemaVersion < 1) throw new InvalidDataException("Save file has an invalid schema version.");
            if (state.schemaVersion == 1)
            {
                // Schema 2 introduced persistent world-location knowledge.
                if (state.worldKnowledge == null) state.worldKnowledge = new List<WorldKnowledgeEntry>();
                state.schemaVersion = 2;
            }

            state.EnsureValid();
            return state;
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
                },
                worldKnowledge = new List<WorldKnowledgeEntry>()
            };
        }
    }
}
