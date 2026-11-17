using System.Linq;
using UnityEngine;

namespace ProjectEcho
{
    public sealed class EchoHUD : MonoBehaviour
    {
        private enum Page { Sanctuary, Expedition, Memories, Mutations }
        private EchoSceneBootstrap game;
        private Page page = Page.Sanctuary;
        private Vector2 journalScroll;
        private Vector2 mutationScroll;
        private Vector2 sanctuaryLeftScroll;
        private Vector2 sanctuaryRightScroll;
        private MemoryKind? journalFilter;
        private string echoNameDraft = "";
        private string sanctuaryNameDraft = "";
        private bool identityDraftsLoaded;
        private GUIStyle panel, button, label, title, muted, tab, tabActive, heading, small, action;
        private Texture2D panelTexture, buttonTexture, activeTexture, barTexture;
        private string toast = "The Sanctuary is listening.";
        private float toastTimer;

        private void Start()
        {
            game = UnityEngine.Object.FindAnyObjectByType<EchoSceneBootstrap>();
            SyncIdentityDrafts();
        }

        private void OnGUI()
        {
            if (game == null) game = UnityEngine.Object.FindAnyObjectByType<EchoSceneBootstrap>();
            if (game == null || game.Profile == null) return;
            if (!identityDraftsLoaded) SyncIdentityDrafts();
            EnsureStyles();
            float w = Screen.width, h = Screen.height;
            GUI.Box(new Rect(18, 16, w - 36, 58), GUIContent.none, panel);
            GUI.Label(new Rect(36, 25, 290, 22), "PROJECT ECHO", title);
            GUI.Label(new Rect(36, 49, 380, 18), "LIFE, LEARNING, AND THE MEMORIES BETWEEN", muted);
            GUI.Label(new Rect(w - 410, 28, 170, 28), "DAY " + game.Profile.day, heading);
            GUI.Label(new Rect(w - 265, 28, 228, 28), "◇  " + game.Profile.crystalShards + "    ◌  " + game.Profile.bioSamples + "    ✦  " + game.Profile.insight, label);

            DrawNavigation();
            switch (page)
            {
                case Page.Sanctuary: DrawSanctuary(w, h); break;
                case Page.Expedition: DrawExpedition(w, h); break;
                case Page.Memories: DrawMemories(w, h); break;
                case Page.Mutations: DrawMutations(w, h); break;
            }
            DrawFooter(w, h);
            if (toastTimer > 0)
            {
                toastTimer -= Time.unscaledDeltaTime;
                GUI.Box(new Rect(w * .5f - 220, h - 94, 440, 34), toast, button);
            }
        }

        private void DrawNavigation()
        {
            float x = 28;
            x += NavButton(x, "SANCTUARY", Page.Sanctuary, 138);
            x += NavButton(x, "EXPEDITION", Page.Expedition, 138);
            x += NavButton(x, "MEMORY ARCHIVE", Page.Memories, 160);
            NavButton(x, "GENE FORGE", Page.Mutations, 138);
        }

        private float NavButton(float x, string text, Page target, float width)
        {
            if (GUI.Button(new Rect(x, 84, width - 8, 34), text, page == target ? tabActive : tab)) page = target;
            return width;
        }

        private void DrawSanctuary(float w, float h)
        {
            float top = 132, panelHeight = Mathf.Max(220, h - 198);
            Rect left = new Rect(24, top, 315, panelHeight);
            Rect right = new Rect(w - 344, top, 320, panelHeight);
            GUI.Box(left, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(left.x + 18, left.y + 17, left.width - 36, left.height - 34));
            sanctuaryLeftScroll = GUILayout.BeginScrollView(sanctuaryLeftScroll, GUILayout.ExpandHeight(true));
            GUILayout.Label("YOUR ECHO", muted);
            GUILayout.Label(game.Profile.displayName, title);
            GUILayout.Label(game.Profile.species + "  ·  " + game.Profile.sanctuaryName, small);
            GUILayout.Space(12);
            NeedRow("ENERGY", game.Profile.needs.energy);
            NeedRow("HUNGER", game.Profile.needs.hunger);
            NeedRow("SAFETY", game.Profile.needs.safety);
            NeedRow("SOCIAL", game.Profile.needs.social);
            NeedRow("CURIOSITY", game.Profile.needs.curiosity);
            GUILayout.Space(14);
            GUILayout.Label("PERSONALITY", heading);
            TraitRow("Courage", TraitKey.Courage);
            TraitRow("Curiosity", TraitKey.Curiosity);
            TraitRow("Loyalty", TraitKey.Loyalty);
            TraitRow("Creativity", TraitKey.Creativity);
            TraitRow("Self-preservation", TraitKey.SelfPreservation);
            GUILayout.Space(10);
            GUILayout.Label("BOND WITH CREATOR", heading);
            RelationshipRow("Trust", game.Profile.playerRelationship.trust);
            RelationshipRow("Friendship", game.Profile.playerRelationship.friendship);
            RelationshipRow("Respect", game.Profile.playerRelationship.respect);
            RelationshipRow("Fear", game.Profile.playerRelationship.fear);
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUI.Box(right, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(right.x + 18, right.y + 17, right.width - 36, right.height - 34));
            sanctuaryRightScroll = GUILayout.BeginScrollView(sanctuaryRightScroll, GUILayout.ExpandHeight(true));
            GUILayout.Label("SANCTUARY", muted);
            GUILayout.Label("A place to return to.", heading);
            GUILayout.Space(6);
            GUILayout.Label("Watch " + game.Profile.displayName + " choose what to do. Needs and remembered experiences shape each decision.", small);
            GUILayout.Space(10);
            GUILayout.Label("RIGHT NOW", muted);
            var decision = game.Brain.Current;
            GUILayout.Label(decision == null ? "Listening to the canopy" : decision.state.ToString(), heading);
            GUILayout.Label(decision == null ? "The Echo is taking in its surroundings." : decision.explanation, small);
            GUILayout.Space(12);
            GUILayout.Label("CARE", heading);
            CareButton("Offer food", "Hunger rises · a shared memory forms", game.CareFeed);
            CareButton("Stay close", "Safety and trust grow", game.CareSoothe);
            CareButton("Play together", "Social need and curiosity rise", game.CarePlay);
            CareButton("Make a quiet place", "Energy returns", game.CareRest);
            GUILayout.Space(10);
            GUILayout.Label("EXPEDITIONS", muted);
            GUILayout.Label(game.Profile.expeditionsCompleted + " journeys remembered", heading);
            if (GUILayout.Button("CHOOSE AN EXPEDITION  →", action, GUILayout.Height(43))) page = Page.Expedition;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawExpedition(float w, float h)
        {
            Rect area = new Rect(w * .5f - 370, 132, 740, Mathf.Max(400, h - 198));
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 32, area.y + 24, area.width - 64, area.height - 48));
            GUILayout.Label("FIELD NOTES  /  GLASS JUNGLE", muted);
            GUILayout.Label("Every path leaves a trace.", title);
            GUILayout.Space(6);
            GUILayout.Label("Bioluminescent roots · crystal ruins · distant movement in the canopy", small);
            GUILayout.Space(18);

            if (!game.Expedition.IsActive)
            {
                GUILayout.Label("EXPEDITION HUB", heading);
                GUILayout.Label("A short journey, three situations, and a memory your Echo will carry home. Choices resolve from personality, needs, trust, and prior experience—not a random stat roll.", label);
                GUILayout.Space(24);
                GUILayout.Label("Before departure", heading);
                GUILayout.Label("Energy  " + Mathf.RoundToInt(game.Profile.needs.energy) + "     Safety  " + Mathf.RoundToInt(game.Profile.needs.safety) + "     Curiosity  " + Mathf.RoundToInt(game.Profile.needs.curiosity), small);
                GUILayout.Space(22);
                if (GUILayout.Button("BEGIN GLASS JUNGLE EXPEDITION", action, GUILayout.Height(48)))
                {
                    game.Expedition.Begin(); SetToast(game.Profile.displayName + " is on the way.");
                }
                GUILayout.Space(10);
                GUILayout.Label("The Echo returns automatically to the Sanctuary after the third encounter. No Echo can be lost in this prototype.", muted);
            }
            else if (game.Expedition.AwaitingReturn)
            {
                GUILayout.Label("THE PATH HOME", heading);
                GUILayout.Label(game.Expedition.LastOutcome, label);
                GUILayout.Space(12);
                GUILayout.Label("Journey memories created: " + game.Expedition.CompletedEncounters + "     ·     Materials recovered: " + game.Profile.crystalShards + " bioglass / " + game.Profile.bioSamples + " samples", small);
                GUILayout.Space(24);
                if (GUILayout.Button("RETURN TO SANCTUARY", action, GUILayout.Height(48)))
                {
                    game.Expedition.ReturnToSanctuary(); page = Page.Sanctuary; SetToast("Welcome home. New memories were added to the archive.");
                }
            }
            else
            {
                var encounter = game.Expedition.CurrentEncounter;
                GUILayout.Label("ENCOUNTER  " + (game.Expedition.CompletedEncounters + 1) + " / 3", heading);
                GUILayout.Space(4);
                if (encounter != null)
                {
                    GUILayout.Label(encounter.location, muted);
                    GUILayout.Label(encounter.title, title);
                    GUILayout.Label(encounter.description, label);
                    GUILayout.Space(14);
                    ChoiceButton(encounter.first, 0);
                    ChoiceButton(encounter.second, 1);
                }
                GUILayout.Space(12);
                GUILayout.Label(game.Expedition.LastOutcome, small);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Remembered experience changes personality slowly. The same Echo will not meet the next moment as a blank slate.", muted);
            }
            GUILayout.EndArea();
        }

        private void ChoiceButton(EncounterChoiceData choice, int option)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("LIKELY APPROACH: " + choice.favoredTrait.ToString().ToUpperInvariant() + "  ·  DIFFICULTY " + choice.difficulty, muted);
            if (GUILayout.Button(choice.label + "  →", action, GUILayout.Height(42)))
            {
                game.Expedition.Choose(option); SetToast(game.Expedition.LastOutcome);
            }
            GUILayout.Label(choice.consequence, small);
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        private void DrawMemories(float w, float h)
        {
            Rect area = new Rect(w * .5f - 410, 132, 820, Mathf.Max(400, h - 198));
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 30, area.y + 20, area.width - 60, area.height - 40));
            GUILayout.Label("MEMORY ARCHIVE", muted);
            GUILayout.Label("What stays with " + game.Profile.displayName + ".", title);
            GUILayout.Label(game.Profile.memories.Count + " experiences in the current record", small);
            GUILayout.Space(7);

            GUILayout.BeginHorizontal();
            GUILayout.Label("ECHO", muted, GUILayout.Width(40));
            echoNameDraft = GUILayout.TextField(echoNameDraft, GUILayout.Width(170));
            GUILayout.Label("SANCTUARY", muted, GUILayout.Width(76));
            sanctuaryNameDraft = GUILayout.TextField(sanctuaryNameDraft, GUILayout.Width(210));
            if (GUILayout.Button("SAVE NAMES", button, GUILayout.Width(120), GUILayout.Height(27))) SaveIdentityNames();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("COPY ECHO STORY", action, GUILayout.Width(190), GUILayout.Height(34))) CopyEchoStory();
            GUILayout.Label("Share a remembered moment with a friend.", small);
            GUILayout.EndHorizontal();

            var places = game.State.worldKnowledge
                .Where(entry => entry != null && entry.visits > 0)
                .OrderByDescending(entry => entry.visits)
                .Take(3)
                .ToList();
            if (places.Count > 0)
            {
                GUILayout.Label("PLACES THE ECHO KNOWS", muted);
                foreach (var place in places)
                {
                    string tags = place.knownTags == null || place.knownTags.Count == 0
                        ? "unmapped"
                        : string.Join(", ", place.knownTags.Select(tag => tag.ToString()).ToArray());
                    GUILayout.Label(place.location + "  ·  " + place.visits + " visits  ·  " + tags, small);
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ALL", journalFilter == null ? tabActive : tab, GUILayout.Width(72), GUILayout.Height(27))) journalFilter = null;
            if (GUILayout.Button("PERSONAL", journalFilter == MemoryKind.Personal ? tabActive : tab, GUILayout.Width(100), GUILayout.Height(27))) journalFilter = MemoryKind.Personal;
            if (GUILayout.Button("ENVIRONMENT", journalFilter == MemoryKind.Environmental ? tabActive : tab, GUILayout.Width(125), GUILayout.Height(27))) journalFilter = MemoryKind.Environmental;
            if (GUILayout.Button("SOCIAL", journalFilter == MemoryKind.Social ? tabActive : tab, GUILayout.Width(85), GUILayout.Height(27))) journalFilter = MemoryKind.Social;
            GUILayout.EndHorizontal();

            journalScroll = GUILayout.BeginScrollView(journalScroll, GUILayout.ExpandHeight(true));
            var memories = game.Profile.memories
                .Where(memory => memory != null && (!journalFilter.HasValue || memory.kind == journalFilter.Value))
                .OrderByDescending(memory => memory.timestampUtc)
                .ToList();
            foreach (var memory in memories)
            {
                GUILayout.BeginVertical(panel);
                GUILayout.Label(memory.location + "  ·  " + memory.kind.ToString().ToUpperInvariant() + "  ·  " + memory.emotionalValue, muted);
                GUILayout.Label(memory.summary, label);
                GUILayout.Label("Importance " + Mathf.RoundToInt(memory.importance * 100) + "%   ·   " + memory.tag, small);
                GUILayout.EndVertical(); GUILayout.Space(7);
            }
            if (memories.Count == 0)
                GUILayout.Label(game.Profile.memories.Count == 0
                    ? "No memories yet. A small moment in the Sanctuary can be the first."
                    : "No memories in this category yet. Choose another filter to keep exploring the archive.", label);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void SyncIdentityDrafts()
        {
            if (game == null || game.Profile == null) return;
            echoNameDraft = game.Profile.displayName;
            sanctuaryNameDraft = game.Profile.sanctuaryName;
            identityDraftsLoaded = true;
        }

        private void SaveIdentityNames()
        {
            string echoName = (echoNameDraft ?? "").Trim();
            string sanctuaryName = (sanctuaryNameDraft ?? "").Trim();
            if (string.IsNullOrEmpty(echoName))
            {
                echoNameDraft = "";
                SetToast("Give your Echo a name before saving.");
                return;
            }
            if (string.IsNullOrEmpty(sanctuaryName))
            {
                sanctuaryNameDraft = "";
                SetToast("Give your Sanctuary a name before saving.");
                return;
            }

            game.Profile.displayName = echoName.Substring(0, Mathf.Min(24, echoName.Length));
            game.Profile.sanctuaryName = sanctuaryName.Substring(0, Mathf.Min(32, sanctuaryName.Length));
            if (game.Avatar != null) game.Avatar.gameObject.name = "Echo - " + game.Profile.displayName;
            SyncIdentityDrafts();
            game.SaveNow();
            SetToast("Identity saved. This Echo is yours.");
        }

        private void CopyEchoStory()
        {
            EchoMemory memory = game.Profile.memories
                .Where(item => item != null)
                .OrderByDescending(item => item.importance)
                .ThenByDescending(item => item.timestampUtc)
                .FirstOrDefault();
            if (memory == null)
            {
                SetToast("Create a memory first, then share its story.");
                return;
            }

            string emotion = string.IsNullOrWhiteSpace(memory.emotionalValue) ? "something unforgettable" : memory.emotionalValue.ToLowerInvariant();
            string location = string.IsNullOrWhiteSpace(memory.location) ? "the Glass Jungle" : memory.location;
            string summary = string.IsNullOrWhiteSpace(memory.summary) ? "It still remembers the moment." : memory.summary;
            GUIUtility.systemCopyBuffer = game.Profile.displayName + " remembers " + emotion + " at " + location + ": " + summary;
            SetToast("Echo story copied to clipboard.");
        }

        private void DrawMutations(float w, float h)
        {
            Rect area = new Rect(w * .5f - 410, 132, 820, Mathf.Max(400, h - 198));
            GUI.Box(area, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(area.x + 30, area.y + 20, area.width - 60, area.height - 40));
            GUILayout.Label("GENE FORGE", muted);
            GUILayout.Label("Anatomy shaped by experience.", title);
            GUILayout.Label("Unlocking a mutation changes the Echo's body and, for some, its simulation behavior.", small);
            GUILayout.Space(12);
            mutationScroll = GUILayout.BeginScrollView(mutationScroll, GUILayout.ExpandHeight(true));
            foreach (MutationData data in game.Tuning.mutations)
            {
                bool owned = game.Mutations.Has(data.id);
                GUILayout.BeginHorizontal(panel);
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.Label(data.title + (owned ? "  ·  EXPRESSED" : ""), heading);
                GUILayout.Label(data.description, small);
                GUILayout.EndVertical();
                GUILayout.Space(14);
                if (owned) GUILayout.Label("OWNED", muted, GUILayout.Width(92));
                else if (GUILayout.Button("◇ " + data.shardCost + "   ◌ " + data.sampleCost, button, GUILayout.Width(125), GUILayout.Height(38)))
                {
                    if (game.Mutations.Unlock(data, out string result))
                    {
                        game.Avatar.RebuildAppearance(); game.SaveNow(); SetToast(result);
                    }
                    else SetToast(result);
                }
                GUILayout.EndHorizontal(); GUILayout.Space(7);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawFooter(float w, float h)
        {
            GUI.Box(new Rect(18, h - 48, w - 36, 34), GUIContent.none, panel);
            var decision = game.Brain.Current;
            string state = decision == null ? "LISTENING" : decision.state.ToString().ToUpperInvariant();
            string explanation = decision == null ? "The canopy shifts in the wind." : decision.explanation;
            GUI.Label(new Rect(32, h - 42, 185, 24), "BEHAVIOR  /  " + state, muted);
            GUI.Label(new Rect(220, h - 42, w - 252, 24), explanation, small);
        }

        private void NeedRow(string name, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, muted, GUILayout.Width(94));
            Rect bar = GUILayoutUtility.GetRect(100, 14, GUILayout.ExpandWidth(true));
            DrawBar(bar, value, value < 30 ? new Color(.9f, .37f, .43f) : new Color(.27f, .77f, .85f));
            GUILayout.Label(Mathf.RoundToInt(value).ToString(), small, GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private void TraitRow(string name, TraitKey trait)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, small, GUILayout.Width(128));
            Rect bar = GUILayoutUtility.GetRect(80, 10, GUILayout.ExpandWidth(true));
            DrawBar(bar, game.Profile.traits.Get(trait), new Color(.61f, .49f, .9f));
            GUILayout.Label(Mathf.RoundToInt(game.Profile.traits.Get(trait)).ToString(), muted, GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private void RelationshipRow(string name, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, small, GUILayout.Width(128));
            Rect bar = GUILayoutUtility.GetRect(80, 10, GUILayout.ExpandWidth(true));
            Color color = name == "Fear" ? new Color(.88f, .48f, .56f) : new Color(.32f, .76f, .83f);
            DrawBar(bar, value, color);
            GUILayout.Label(Mathf.RoundToInt(value).ToString(), muted, GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private void DrawBar(Rect rect, float value, Color color)
        {
            GUI.color = new Color(.12f, .17f, .26f, .95f); GUI.DrawTexture(rect, barTexture);
            GUI.color = color; GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value / 100f), rect.height), barTexture);
            GUI.color = Color.white;
        }

        private void CareButton(string titleText, string description, System.Action actionCallback)
        {
            if (GUILayout.Button(titleText, button, GUILayout.Height(34))) { actionCallback(); SetToast(description); game.SaveNow(); }
            GUILayout.Label(description, small); GUILayout.Space(3);
        }

        private void SetToast(string text) { toast = text; toastTimer = 3.4f; }

        private void EnsureStyles()
        {
            if (panel != null) return;
            panelTexture = MakeTexture(new Color(.035f, .065f, .12f, .92f));
            buttonTexture = MakeTexture(new Color(.12f, .22f, .34f, .96f));
            activeTexture = MakeTexture(new Color(.12f, .42f, .54f, .98f));
            barTexture = MakeTexture(Color.white);
            panel = new GUIStyle(GUI.skin.box) { normal = { background = panelTexture }, border = new RectOffset(8, 8, 8, 8), padding = new RectOffset(10, 10, 8, 8) };
            button = new GUIStyle(GUI.skin.button) { normal = { background = buttonTexture, textColor = new Color(.86f, .94f, 1f) }, hover = { background = activeTexture }, active = { background = activeTexture }, fontSize = 13, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(12, 10, 6, 6) };
            action = new GUIStyle(button) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            tab = new GUIStyle(button) { fontSize = 11, alignment = TextAnchor.MiddleCenter, padding = new RectOffset(6, 6, 4, 4) };
            tabActive = new GUIStyle(tab) { normal = { background = activeTexture, textColor = Color.white } };
            label = new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(.79f, .87f, .96f) }, fontSize = 14, wordWrap = true, richText = true };
            title = new GUIStyle(label) { fontSize = 23, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.91f, .96f, 1f) } };
            heading = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.45f, .83f, .94f) } };
            muted = new GUIStyle(label) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.45f, .66f, .80f) } };
            small = new GUIStyle(label) { fontSize = 11, normal = { textColor = new Color(.61f, .72f, .84f) } };
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }
    }
}
