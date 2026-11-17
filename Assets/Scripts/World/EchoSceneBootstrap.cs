using System;
using UnityEngine;

namespace ProjectEcho
{
    public sealed class EchoSceneBootstrap : MonoBehaviour
    {
        public EchoTuning Tuning { get; private set; }
        public EchoGameState State { get; private set; }
        public EchoProfile Profile => State.echo;
        public MemoryManager Memories { get; private set; }
        public TraitManager Traits { get; private set; }
        public NeedManager Needs { get; private set; }
        public RelationshipManager Relationships { get; private set; }
        public MutationManager Mutations { get; private set; }
        public WorldKnowledgeDatabase WorldKnowledge { get; private set; }
        public EchoBrain Brain { get; private set; }
        public EchoExpeditionDirector Expedition { get; private set; }
        public EchoAvatar Avatar { get; private set; }
        private readonly EchoSaveManager saveManager = new EchoSaveManager();
        private float autosaveElapsed;
        private float weatherClock;
        private int mutationHash;

        private void Awake()
        {
            Tuning = Resources.Load<EchoTuning>("EchoTuning");
            if (Tuning == null) { Tuning = ScriptableObject.CreateInstance<EchoTuning>(); Tuning.EnsureDefaults(); }
            else Tuning.EnsureDefaults();
            State = saveManager.LoadOrCreate();
            Memories = new MemoryManager(Profile, Tuning);
            Traits = new TraitManager(Profile);
            Needs = new NeedManager(Profile, Tuning);
            Relationships = new RelationshipManager(Profile);
            Mutations = new MutationManager(Profile);
            WorldKnowledge = new WorldKnowledgeDatabase(State);
            Brain = new EchoBrain(Profile, Memories, Tuning);
            Expedition = new EchoExpeditionDirector(this, Profile, Tuning);
            ConfigureCamera(); BuildWorld();
            GameObject echo = new GameObject("Echo - " + Profile.displayName);
            Avatar = echo.AddComponent<EchoAvatar>();
            Avatar.Initialize(this, new Vector3(-2.4f, 0, 1.3f));
            mutationHash = Profile.mutations.GetHashCode();
        }

        private void Update()
        {
            if (Profile == null) return;
            Brain.Tick(Time.deltaTime, Avatar != null ? Avatar.transform.position : Vector3.zero, Expedition.IsActive);
            Needs.Tick(Time.deltaTime, Expedition.IsActive, Brain.Current != null ? Brain.Current.state : EchoActionState.Idle);
            if (Avatar != null && Brain.Current != null) Avatar.ApplyDecision(Brain.Current, Time.deltaTime);
            autosaveElapsed += Time.deltaTime;
            if (autosaveElapsed >= 20f) { autosaveElapsed = 0; SaveNow(); }
            weatherClock += Time.deltaTime * .08f;
            RenderWeather();
            int newHash = 17;
            foreach (MutationId id in Profile.mutations) newHash = newHash * 31 + (int)id;
            if (newHash != mutationHash && Avatar != null) { mutationHash = newHash; Avatar.RebuildAppearance(); }
        }

        public void SaveNow() => saveManager.Save(State);

        public void CareFeed()
        {
            Needs.Feed(28); Relationships.SharedCare(2, 2); RememberCare("A warm meal waited in the Sanctuary.");
        }
        public void CareSoothe()
        {
            Needs.Soothe(20, 8); Relationships.SharedCare(3, 3, 1); RememberCare("A familiar voice made the Sanctuary feel safe.");
        }
        public void CarePlay()
        {
            Needs.Play(22); Relationships.SharedCare(2, 4); RememberCare("The Echo and its Creator shared a playful moment.");
        }
        public void CareRest()
        {
            Needs.Rest(27); Relationships.SharedCare(1, 1); RememberCare("The Echo rested beneath the quiet canopy.");
        }
        private void RememberCare(string summary)
        {
            Memories.Remember(MemoryKind.Social, MemoryTag.Care, "sanctuary_care", "Comfort",
                Profile.sanctuaryName, summary, .38f, Profile.displayName, "Creator"); SaveNow();
        }

        private void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null) { var go = new GameObject("Main Camera"); go.tag = "MainCamera"; camera = go.AddComponent<Camera>(); go.AddComponent<AudioListener>(); }
            camera.transform.position = new Vector3(0, 12.8f, -17.5f);
            camera.transform.rotation = Quaternion.Euler(34, 0, 0);
            camera.fieldOfView = 47;
            camera.backgroundColor = new Color(.035f, .065f, .13f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            RenderSettings.ambientLight = new Color(.34f, .40f, .58f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential; RenderSettings.fogDensity = .012f;
            RenderSettings.fogColor = new Color(.06f, .12f, .23f);
            if (FindObjectOfType<Light>() == null)
            {
                var lightObject = new GameObject("Moonlight");
                var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional;
                light.intensity = 1.25f; light.color = new Color(.62f, .74f, 1f);
                lightObject.transform.rotation = Quaternion.Euler(45, -25, 0);
            }
        }

        private void BuildWorld()
        {
            MakePrimitive(PrimitiveType.Plane, "Glass Jungle floor", new Vector3(0, -.18f, 0), new Vector3(7, 1, 7), new Color(.055f, .11f, .19f));
            MakePrimitive(PrimitiveType.Cube, "Sanctuary dais", new Vector3(0, -.06f, 0), new Vector3(9.5f, .14f, 7.8f), new Color(.11f, .17f, .28f));
            for (int i = 0; i < 18; i++)
            {
                float angle = i * 2.39996f;
                float radius = 9f + (i % 4) * 1.4f;
                Vector3 p = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                float height = 2.3f + (i % 5) * .45f;
                MakePrimitive(PrimitiveType.Cylinder, "Crystal tree trunk", p + Vector3.up * (height * .48f), new Vector3(.24f, height * .48f, .24f), new Color(.21f, .31f, .48f));
                GameObject crown = MakePrimitive(PrimitiveType.Cube, "Crystal canopy", p + Vector3.up * (height + .35f), new Vector3(1.2f, 2.4f, 1.1f), i % 2 == 0 ? new Color(.15f, .65f, .8f) : new Color(.45f, .32f, .78f));
                crown.transform.rotation = Quaternion.Euler(12, i * 23, 16);
                crown.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                crown.GetComponent<Renderer>().material.SetColor("_EmissionColor", crown.GetComponent<Renderer>().material.color * .3f);
                MakePrimitive(PrimitiveType.Sphere, "Bioluminescent plant", p + new Vector3(.7f, .22f, .5f), new Vector3(.24f, .42f, .24f), new Color(.25f, .85f, .9f));
            }
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2 / 7;
                Vector3 p = new Vector3(Mathf.Cos(a) * 6.4f, .35f, Mathf.Sin(a) * 5.5f);
                MakePrimitive(PrimitiveType.Cube, "Ancient ruin pillar", p + Vector3.up * .45f, new Vector3(.6f, 1.8f + (i % 3) * .4f, .6f), new Color(.24f, .32f, .48f));
                MakePrimitive(PrimitiveType.Cube, "Ruin glyph", p + Vector3.up * 1.25f + Vector3.forward * .32f, new Vector3(.22f, .08f, .03f), new Color(.28f, .87f, 1f));
            }
            // A distant, deliberately non-hostile silhouette suggests the biome's predators without combat.
            var predator = MakePrimitive(PrimitiveType.Sphere, "Distant canopy shadow", new Vector3(13, 1.1f, 12), new Vector3(2.5f, .7f, .8f), new Color(.045f, .055f, .12f));
            predator.transform.rotation = Quaternion.Euler(0, 35, 0);
        }

        private void RenderWeather()
        {
            float pulse = (Mathf.Sin(weatherClock) + 1f) * .5f;
            RenderSettings.fogColor = Color.Lerp(new Color(.045f, .09f, .18f), new Color(.10f, .13f, .30f), pulse * .5f);
        }

        private static GameObject MakePrimitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Color color)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = objectName; item.transform.position = position; item.transform.localScale = scale;
            var collider = item.GetComponent<Collider>(); if (collider != null) Destroy(collider);
            var renderer = item.GetComponent<Renderer>(); renderer.material.color = color;
            return item;
        }
    }

    public sealed class EchoAvatar : MonoBehaviour
    {
        private EchoSceneBootstrap game;
        private Transform visualRoot;
        private Transform body;
        private Transform core;
        private Vector3 target;
        private float animationClock;
        private EchoActionState currentState;
        private Material bodyMaterial;

        public void Initialize(EchoSceneBootstrap owner, Vector3 spawn)
        {
            game = owner; transform.position = spawn; visualRoot = new GameObject("Echo form").transform; visualRoot.SetParent(transform, false);
            RebuildAppearance();
        }

        public void RebuildAppearance()
        {
            if (visualRoot == null) return;
            for (int i = visualRoot.childCount - 1; i >= 0; i--) Destroy(visualRoot.GetChild(i).gameObject);
            Color skin = new Color(.51f, .83f, .92f);
            if (game.Profile.mutations.Contains(MutationId.AdaptiveSkin)) skin = new Color(.68f, .78f, .88f);
            body = Part(PrimitiveType.Sphere, "Soft luminous body", new Vector3(0, .78f, 0), new Vector3(.83f, .93f, 1.05f), skin);
            bodyMaterial = body.GetComponent<Renderer>().material;
            Part(PrimitiveType.Sphere, "Face", new Vector3(0, 1.46f, .37f), new Vector3(.55f, .55f, .54f), skin);
            Color eyeColor = game.Profile.mutations.Contains(MutationId.ThermalVision) ? new Color(1f, .57f, .22f) : new Color(.05f, .18f, .32f);
            Part(PrimitiveType.Sphere, "Left eye", new Vector3(-.18f, 1.53f, .80f), new Vector3(.10f, .14f, .06f), eyeColor);
            Part(PrimitiveType.Sphere, "Right eye", new Vector3(.18f, 1.53f, .80f), new Vector3(.10f, .14f, .06f), eyeColor);
            Part(PrimitiveType.Sphere, "Left ear", new Vector3(-.39f, 1.82f, .29f), new Vector3(.13f, .42f, .16f), new Color(.22f, .56f, .76f));
            Part(PrimitiveType.Sphere, "Right ear", new Vector3(.39f, 1.82f, .29f), new Vector3(.13f, .42f, .16f), new Color(.22f, .56f, .76f));
            Part(PrimitiveType.Capsule, "Tail", new Vector3(0, .44f, -.73f), new Vector3(.17f, .56f, .17f), new Color(.25f, .62f, .75f)).localRotation = Quaternion.Euler(60, 0, 0);
            core = Part(PrimitiveType.Sphere, "Bioluminescent core", new Vector3(0, .8f, .82f), new Vector3(.25f, .25f, .18f), new Color(.2f, .85f, 1f));
            SetEmission(core.GetComponent<Renderer>().material, new Color(.15f, .75f, 1f));
            if (game.Profile.mutations.Contains(MutationId.Bioluminescence))
            {
                SetEmission(bodyMaterial, new Color(.16f, .58f, .9f));
                Part(PrimitiveType.Sphere, "Light node", new Vector3(0, 1.03f, -.52f), new Vector3(.23f, .23f, .2f), new Color(.2f, .8f, 1f));
            }
            if (game.Profile.mutations.Contains(MutationId.DefensiveShell))
            {
                Transform shell = Part(PrimitiveType.Sphere, "Defensive shell", new Vector3(0, 1.02f, -.22f), new Vector3(.72f, .65f, .24f), new Color(.31f, .49f, .68f));
                shell.GetComponent<Renderer>().material.SetFloat("_Metallic", .35f);
            }
            if (game.Profile.mutations.Contains(MutationId.EnhancedHearing))
            {
                Part(PrimitiveType.Capsule, "Hearing crest", new Vector3(0, 2.12f, .12f), new Vector3(.09f, .26f, .09f), new Color(.64f, .45f, .98f));
            }
            if (game.Profile.mutations.Contains(MutationId.EchoSense))
                Part(PrimitiveType.Sphere, "Echo sense node", new Vector3(0, .30f, -.55f), new Vector3(.12f, .12f, .12f), new Color(.58f, .45f, 1f));
        }

        public void ApplyDecision(EchoDecision decision, float deltaTime)
        {
            currentState = decision.state;
            target = decision.target;
            if (game.Expedition.IsActive) target += new Vector3(0, 0, 3f);
            float speed = game.Tuning != null ? game.Tuning.movementSpeed : 1.5f;
            if (currentState == EchoActionState.Sleep || currentState == EchoActionState.Idle) speed = 0;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * deltaTime);
            if (speed > .01f)
            {
                Vector3 direction = target - transform.position;
                if (direction.sqrMagnitude > .03f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), deltaTime * 2f);
            }
            animationClock += deltaTime;
            float bob = currentState == EchoActionState.Sleep ? .015f : .055f;
            if (body != null) body.localPosition = new Vector3(0, .78f + Mathf.Sin(animationClock * 2.6f) * bob, 0);
            if (visualRoot != null) visualRoot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(animationClock * 1.3f) * 2f);
            if (core != null)
            {
                float pulse = 1 + (Mathf.Sin(animationClock * 3.1f) + 1) * .07f;
                core.localScale = new Vector3(.25f * pulse, .25f * pulse, .18f * pulse);
            }
        }

        private Transform Part(PrimitiveType kind, string partName, Vector3 localPos, Vector3 scale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(kind); part.name = partName; part.transform.SetParent(visualRoot, false);
            part.transform.localPosition = localPos; part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>(); if (collider != null) Destroy(collider);
            Renderer renderer = part.GetComponent<Renderer>(); renderer.material.color = color;
            return part.transform;
        }

        private static void SetEmission(Material material, Color color)
        {
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color);
            }
        }
    }
}
