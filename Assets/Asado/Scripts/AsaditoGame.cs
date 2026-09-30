using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using System.Collections.Generic;
using Asadito.Runtime;

namespace Asadito
{
    /// <summary>A compact, self-contained playable asado loop for the first MVP.</summary>
    public sealed class AsaditoGame : MonoBehaviour
    {
        [Min(0f)] public float SimulationTimeScale = 20f;
        [Range(100f, 300f)] public float FireTemperature = 210f;
        [SerializeField] private CharcoalGrillModel grill = new CharcoalGrillModel();
        private static readonly Color Cream = new Color32(255, 239, 205, 255);
        private static readonly Color Gold = new Color32(245, 177, 72, 255);
        private static readonly Color Green = new Color32(67, 106, 73, 255);

        private sealed class PlayablePortion
        {
            public readonly FoodState State = new FoodState();
            public readonly FoodCookProfile Profile;
            public readonly float Amount;
            public Vector2 Position = new Vector2(.5f, .5f);
            public bool Started, OnTray;
            public PlayablePortion(string foodId, float amount)
            {
                Amount = amount;
                Profile = FoodCookingModel.CreateProfile(foodId);
                Reset();
            }
            public string Point
            {
                get
                {
                    if (Profile.UsesCheeseStages)
                    {
                        switch (FoodCookingModel.GetProvoletaStage(State))
                        {
                            case ProvoletaCookingStage.Cold: return "FRÍA";
                            case ProvoletaCookingStage.Softening: return "ABLANDANDO";
                            case ProvoletaCookingStage.Browning: return "DORANDO";
                            case ProvoletaCookingStage.Ideal: return "IDEAL";
                            case ProvoletaCookingStage.Failed: return "PASADA";
                            case ProvoletaCookingStage.Burnt: return "QUEMADA";
                        }
                    }
                    if (State.Char >= .75f) return "QUEMADO";
                    if (State.CoreTemperatureC < Profile.DonenessBands[0].MinimumCoreC) return "CRUDO";
                    if (State.CoreTemperatureC > Profile.DonenessBands[Profile.DonenessBands.Length - 1].MaximumCoreC) return "PASADO";
                    return FoodCookingModel.GetDoneness(State, Profile).ToSpanish();
                }
            }
            public void Flip() => State.Flip();
            public void Reset()
            {
                State.Reset();
                State.CoreTemperatureC = 20f;
                State.SurfaceTemperatureC = 20f;
                State.Faces = new[] { new FoodFaceState { SurfaceTemperatureC = 20f }, new FoodFaceState { SurfaceTemperatureC = 20f } };
                Started = OnTray = false;
                Position = new Vector2(.5f, .5f);
            }
        }

        private PlayablePortion[] portions = new PlayablePortion[0];
        private MvpLevelDefinition currentLevel;
        private GuestProfile[] activeGuests;
        private int currentLevelNumber = 1;
        private int activePortion = -1, trayCount;

        private Canvas canvas;
        private RectTransform contentRoot;
        private RectTransform gameplayRoot;
        private RectTransform foodInteractionRoot;
        private GameObject menuRoot;
        private GameObject levelSelectRoot;
        private GameObject introRoot;
        private CanvasGroup menuCanvasGroup;
        private CanvasGroup levelSelectCanvasGroup;
        private CanvasGroup introCanvasGroup;
        private CanvasGroup gameplayCanvasGroup;
        private bool menuTransitionActive;
        private Text tutorialText;
        private Image background;
        private Image[] portionImages;
        private RectTransform[] portionHitTargets;
        private Image[] portionSelectionHalos;
        private Shadow[] portionSelectionShadows;
        private Image cookFill;
        private Image fireGlow;
        private RectTransform heatGridRect;
        private RectTransform trayRect;
        private RectTransform trayDropRect;
        private RectTransform tongsVisual;
        private RectTransform[] tongsArms = new RectTransform[2];
        private readonly System.Collections.Generic.List<Image> heatCellImages = new System.Collections.Generic.List<Image>(48);
        private Text guestName;
        private Text guestOrder;
        private Text progressText;
        private Text feedbackText;
        private Text scoreText;
        private Text resultScoreText;
        private Image[] resultStarIcons;
        private Image[] resultFoodIcons;
        private GameObject resultsRoot;
        private Image avatarImage;
        private int[] guestExpressions = new int[0];
        private Text flipButtonText;
        private Text introTitleText;
        private Text introGuestsText;
        private Text introMenuText;
        private Text introObjectiveText;
        private Button[] levelCards;
        private Image[] levelCardImages;
        private Text[] levelCardLabels;
        private Sprite[] levelCardSprites;
        private Image[] introFoodIcons;
        private Button flipButton;
        private Button plateButton;
        private Button serveButton;
        private Button igniteButton;
        private Button debugScaleButton;
        private Button pauseButton;
        private Button pauseSoundButton;
        private Button pauseHapticsButton;
        private GameObject pauseRoot;
        private CanvasGroup pauseCanvasGroup;
        private Text pauseSoundLabel;
        private Text pauseHapticsLabel;
        private bool paused;
        private bool platingInProgress;
        private bool activeFoodPointerDragging;
        private int activeFoodPointerId = int.MinValue;
        private int activeFoodPointerIndex = -1;
        private Coroutine tongsMoveRoutine;
        private Coroutine tongsGripRoutine;
        private GameObject avatar;
        private Sprite whiteSprite;
        private Sprite circleSprite;
        private readonly Dictionary<string, Sprite[]> foodStateSprites = new Dictionary<string, Sprite[]>(System.StringComparer.OrdinalIgnoreCase);
        private Sprite[,] guestPortraitSprites;
        private Sprite patioSprite;
        private Sprite gameplayGrillSprite;
        private Sprite titleSprite;
        private Sprite logoSprite;
        private Sprite roundedButtonSprite;
        private Sprite arcadeButtonShapeSprite;
        private Sprite arcadeGoldButtonSprite;
        private Sprite arcadeCoralButtonSprite;
        private Font displayFont;
        private Font bodyFont;
        private Font mediumFont;
        private Font semiBoldFont;
        private Font boldFont;
        private Font extraBoldFont;
        private int totalScore;
        private int totalStars;
        [SerializeField] private StarThresholds starThresholds = new StarThresholds();
        private MvpSaveData saveData;
        private bool cooking;
        private bool servingLocked;
        private int tutorialStep;
        private Coroutine smokeRoutine;
        private AudioSource sizzleSource;
        private AudioClip sizzleClip;
        private ProceduralSfx proceduralSfx;
        private Coroutine scoreAnimationRoutine;
        private Coroutine heatVisualRoutine;
        private float heatGridVisualRefreshTimer;
        private float sfxVolumeBeforeMute = .8f;
        private const float HeatGridVisualRefreshInterval = .25f;

        private void Start()
        {
            saveData = MvpSave.Load();
            sfxVolumeBeforeMute = saveData.Settings.SfxVolume > 0f ? saveData.Settings.SfxVolume : .8f;
            ConfigureLevel(1);
            SimulationTimeScale = saveData.Settings.SimulationTimeScale;
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f));
            circleSprite = MakeCircleSprite(128);
            roundedButtonSprite = MakeRoundedRectSprite(128, 34);
            arcadeButtonShapeSprite = MakeRoundedRectSprite(128, 16);
            arcadeGoldButtonSprite = MakeGradientButtonSprite(128, 16,
                new Color32(255, 211, 58, 255), new Color32(255, 166, 13, 255), "Arcade Gold Button");
            arcadeCoralButtonSprite = MakeGradientButtonSprite(128, 16,
                new Color32(255, 100, 78, 255), new Color32(237, 49, 43, 255), "Arcade Coral Button");
            displayFont = Resources.Load<Font>("Fonts/LilitaOne-Regular");
            bodyFont = Resources.Load<Font>("Fonts/Baloo2-Regular");
            mediumFont = Resources.Load<Font>("Fonts/Baloo2-Medium");
            semiBoldFont = Resources.Load<Font>("Fonts/Baloo2-SemiBold");
            boldFont = Resources.Load<Font>("Fonts/Baloo2-Bold");
            extraBoldFont = Resources.Load<Font>("Fonts/Baloo2-ExtraBold");
            patioSprite = Resources.Load<Sprite>("Art/PatioParrilla");
            Texture2D gameplayGrillTexture = Resources.Load<Texture2D>("Art/ParrillaTopDownStylized");
            if (gameplayGrillTexture == null)
                gameplayGrillTexture = Resources.Load<Texture2D>("Art/ParrillaTopDownGameplay");
            if (gameplayGrillTexture != null)
                gameplayGrillSprite = Sprite.Create(gameplayGrillTexture, new Rect(0, 0, gameplayGrillTexture.width, gameplayGrillTexture.height), new Vector2(.5f, .5f), 100f);
            Texture2D titleTexture = Resources.Load<Texture2D>("Art/PortadaAsadito");
            if (titleTexture != null)
                titleSprite = Sprite.Create(titleTexture, new Rect(0, 0, titleTexture.width, titleTexture.height), new Vector2(.5f, .5f), 100f);
            levelCardSprites = new Sprite[MvpLevelCatalog.Count];
            for (int i = 0; i < levelCardSprites.Length; i++)
            {
                Texture2D levelArt = Resources.Load<Texture2D>("Art/LevelCards/Nivel" + (i + 1));
                if (levelArt == null) continue;
                levelCardSprites[i] = Sprite.Create(levelArt,
                    new Rect(0, 0, levelArt.width, levelArt.height), new Vector2(.5f, .5f), 100f);
                levelCardSprites[i].name = "Imagen Nivel " + (i + 1);
            }
            Texture2D logoTexture = Resources.Load<Texture2D>("Art/AsaditoLogo");
            if (logoTexture != null)
            {
                logoSprite = Sprite.Create(logoTexture, new Rect(0, 0, logoTexture.width, logoTexture.height), new Vector2(.5f, .5f), 100f);
                logoSprite.name = "AsaditoLogo";
            }
            Texture2D guestPortraitAtlas = Resources.Load<Texture2D>("Art/GuestPortraitAtlas");
            if (guestPortraitAtlas != null) guestPortraitSprites = CreateGuestPortraitSprites(guestPortraitAtlas);
            BuildSizzleAudio();
            grill.HeatWhenLitC = FireTemperature;
            grill.Reset();
            BuildInterface();
            RefreshOrder();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (paused) ContinueGame();
                else PauseGame();
            }
            if (paused || servingLocked) return;
            float minutes = Time.deltaTime * SimulationTimeScale / 60f;
            grill.Step(minutes);
            // Fuel decays continuously, but repainting all 48 UI cells every frame needlessly
            // rebuilds the Canvas and cancels the ignition/ember color tween. Keep simulation
            // precise while refreshing this slow-changing visual at a modest cadence.
            if (heatVisualRoutine == null)
            {
                heatGridVisualRefreshTimer += Time.unscaledDeltaTime;
                if (heatGridVisualRefreshTimer >= HeatGridVisualRefreshInterval)
                {
                    heatGridVisualRefreshTimer = 0f;
                    RefreshHeatGridVisuals();
                }
            }
            for (int i = 0; i < portions.Length; i++)
            {
                PlayablePortion portion = portions[i];
                if (!portion.Started || portion.OnTray) continue;
                bool onGrill = portion.Position.x >= 0f && portion.Position.x <= 1f && portion.Position.y >= 0f && portion.Position.y <= 1f;
                float localHeat = grill.Sample(portion.Position, new Vector2(.14f, .14f)).x;
                FoodCookingModel.Step(portion.State, portion.Profile, localHeat, minutes, onGrill && grill.IsLit);
            }
            if (cooking) UpdateCookFeedback();
            else if (trayCount > 0)
                progressText.text = trayCount == portions.Length ? "BANDEJA LISTA PARA SERVIR" :
                    "BANDEJA " + trayCount + " / " + portions.Length + "  ·  RETIRÁ EL RESTO";
        }

        private int lastSafeScreenWidth;
        private int lastSafeScreenHeight;
        private Rect lastSafeArea;

        private void LateUpdate()
        {
            ApplySafeAreaIfChanged();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && !paused && pauseRoot != null && !servingLocked)
                PauseGame();
        }

        private void OnDestroy()
        {
            if (Time.timeScale == 0f) Time.timeScale = 1f;
            paused = false;
            if (whiteSprite != null) Destroy(whiteSprite);
            if (circleSprite != null) Destroy(circleSprite);
            foreach (KeyValuePair<string, Sprite[]> foodSprites in foodStateSprites)
                foreach (Sprite stateSprite in foodSprites.Value)
                    if (stateSprite != null) Destroy(stateSprite);
            foodStateSprites.Clear();
            if (guestPortraitSprites != null)
                for (int guest = 0; guest < guestPortraitSprites.GetLength(0); guest++)
                    for (int expression = 0; expression < guestPortraitSprites.GetLength(1); expression++)
                        if (guestPortraitSprites[guest, expression] != null) Destroy(guestPortraitSprites[guest, expression]);
            if (gameplayGrillSprite != null) Destroy(gameplayGrillSprite);
            if (titleSprite != null) Destroy(titleSprite);
            if (logoSprite != null) Destroy(logoSprite);
            if (roundedButtonSprite != null) Destroy(roundedButtonSprite);
            if (arcadeButtonShapeSprite != null) Destroy(arcadeButtonShapeSprite);
            if (arcadeGoldButtonSprite != null) Destroy(arcadeGoldButtonSprite);
            if (arcadeCoralButtonSprite != null) Destroy(arcadeCoralButtonSprite);
            if (levelCardSprites != null)
                foreach (Sprite levelCardSprite in levelCardSprites)
                    if (levelCardSprite != null) Destroy(levelCardSprite);
            if (sizzleClip != null) Destroy(sizzleClip);
        }

        private void ConfigureLevel(int levelNumber)
        {
            currentLevelNumber = Mathf.Clamp(levelNumber, 1, MvpLevelCatalog.Count);
            currentLevel = MvpLevelCatalog.Get(currentLevelNumber);
            portions = new PlayablePortion[currentLevel.FoodIds.Length];
            for (int i = 0; i < portions.Length; i++)
                portions[i] = new PlayablePortion(currentLevel.FoodIds[i], currentLevel.PortionAmounts[i]);
            activeGuests = MvpLevelCatalog.CreateGuests(currentLevelNumber);
        }

        private void BuildInterface()
        {
            EnsureEventSystem();
            var canvasObject = new GameObject("Asadito UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;

            Sprite sceneSprite = gameplayGrillSprite != null ? gameplayGrillSprite : patioSprite;
            background = MakeImage("Parrilla cenital ilustrada", canvas.transform, sceneSprite != null ? sceneSprite : whiteSprite, Color.white, Vector2.zero, new Vector2(1, 1), Vector2.zero);
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;
            background.transform.SetAsFirstSibling();
            background.raycastTarget = false;
            background.rectTransform.localScale = sceneSprite == null ? Vector3.one : new Vector3((sceneSprite.rect.width / sceneSprite.rect.height) / (1080f / 1920f), 1f, 1f);

            var contentObject = new GameObject("Safe Area", typeof(RectTransform));
            contentRoot = contentObject.GetComponent<RectTransform>();
            contentRoot.SetParent(canvas.transform, false);
            ApplySafeAreaIfChanged(true);

            var gameplayObject = new GameObject("Gameplay UI", typeof(RectTransform), typeof(CanvasGroup));
            gameplayRoot = gameplayObject.GetComponent<RectTransform>();
            gameplayRoot.SetParent(contentRoot, false);
            gameplayRoot.anchorMin = Vector2.zero;
            gameplayRoot.anchorMax = Vector2.one;
            gameplayRoot.offsetMin = gameplayRoot.offsetMax = Vector2.zero;
            gameplayCanvasGroup = gameplayObject.GetComponent<CanvasGroup>();
            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;

            MakePanel("Sombra del titulo", gameplayRoot, new Color(0, 0, 0, .38f), .5f, .945f, 950, 112);
            MakeText("Marca", gameplayRoot, "ASADITO", 44, Cream, TextAnchor.MiddleCenter, .5f, .945f, 920, 90, true);
            MakeText("Subtitulo", gameplayRoot, "TU PARRILLA, TU MOMENTO", 19, new Color32(250, 207, 141, 255), TextAnchor.MiddleCenter, .5f, .905f, 920, 42, true);

            MakePanel("Pedido", gameplayRoot, new Color32(38, 48, 39, 238), .5f, .795f, 930, 166);
            avatar = new GameObject("Comensal", typeof(RectTransform));
            var avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.SetParent(gameplayRoot, false);
            SetRect(avatarRect, .17f, .795f, 110, 110);
            guestName = MakeText("Nombre comensal", gameplayRoot, "", 30, Cream, TextAnchor.MiddleLeft, .49f, .818f, 490, 50, true);
            guestOrder = MakeText("Pedido de carne", gameplayRoot, "", 25, new Color32(243, 197, 121, 255), TextAnchor.MiddleLeft, .49f, .773f, 490, 46, true);

            fireGlow = MakeImage("Resplandor de brasas", gameplayRoot, circleSprite, new Color(1f, .35f, .07f, .16f), new Vector2(.5f, .57f), new Vector2(.5f, .57f), new Vector2(330, 170));
            fireGlow.raycastTarget = false;
            BuildHeatGrid();
            Image tray = MakeImage("Bandeja de madera", gameplayRoot, circleSprite, new Color32(105, 63, 43, 255), new Vector2(.5f, .468f), new Vector2(.5f, .468f), new Vector2(344, 112));
            trayRect = tray.rectTransform;
            MakeImage("Plato de servir", gameplayRoot, circleSprite, new Color32(238, 222, 187, 255), new Vector2(.5f, .468f), new Vector2(.5f, .468f), new Vector2(286, 78));
            Image trayDrop = MakeImage("Zona tactil bandeja", gameplayRoot, whiteSprite, new Color(1f, 1f, 1f, 0f),
                new Vector2(.5f, .468f), new Vector2(.5f, .468f), new Vector2(520f, 190f));
            trayDrop.raycastTarget = false;
            trayDropRect = trayDrop.rectTransform;
            MakePanel("Indicador coccion", gameplayRoot, new Color32(39, 34, 28, 230), .5f, .405f, 890, 132);
            progressText = MakeText("Estado coccion", gameplayRoot, "ELEGÍ UN CORTE PARA EMPEZAR", 26, Cream, TextAnchor.MiddleCenter, .5f, .432f, 820, 52, true);
            MakePanel("Barra base", gameplayRoot, new Color32(91, 70, 51, 255), .5f, .385f, 760, 18);
            cookFill = MakeImage("Barra progreso", gameplayRoot, whiteSprite, Gold, new Vector2(.5f, .385f), new Vector2(.5f, .385f), new Vector2(0, 18));
            cookFill.rectTransform.pivot = new Vector2(0, .5f);
            cookFill.rectTransform.anchorMin = cookFill.rectTransform.anchorMax = new Vector2(.12f, .385f);
            cookFill.rectTransform.anchoredPosition = new Vector2(0, 0);
            feedbackText = MakeText("Feedback", gameplayRoot, "", 25, Cream, TextAnchor.MiddleCenter, .5f, .36f, 850, 60, true);
            tutorialText = MakeText("Tutorial contextual", gameplayRoot, "", 19, new Color32(255, 213, 146, 255), TextAnchor.MiddleCenter, .5f, .70f, 830, 58, true);

            BuildFoodInteractionLayer();
            BuildPortionControls();
            BuildTongsVisual();

            flipButton = MakeButton("DAR VUELTA", gameplayRoot, .19f, .105f, 286, 96, Green, FlipMeat);
            plateButton = MakeButton("BANDEJA", gameplayRoot, .5f, .105f, 286, 96,
                new Color32(164, 105, 43, 255), PlateSelectedPortion);
            serveButton = MakeButton("SERVIR", gameplayRoot, .81f, .105f, 286, 96, new Color32(199, 139, 54, 255), Serve);
            flipButtonText = flipButton.GetComponentInChildren<Text>();
            scoreText = MakeText("Progreso nivel", gameplayRoot, "", 22, Cream, TextAnchor.MiddleCenter, .5f, .055f, 960, 54, true);
            if (Debug.isDebugBuild)
                debugScaleButton = MakeButton("CONTROL DEBUG", gameplayRoot, .14f, .855f, 190, 58,
                    new Color32(58, 65, 56, 230), CycleSimulationScale);
            pauseButton = MakeButton("PAUSA", gameplayRoot, .91f, .945f, 142, 72, Green, PauseGame);
            MakeText("Ayuda", gameplayRoot, "Cociná, retirá cada pieza y serví la bandeja.", 18, new Color32(255, 224, 177, 255), TextAnchor.MiddleCenter, .5f, .022f, 960, 38, false);
            igniteButton = MakeButton("PRENDER CARBÓN", gameplayRoot, .5f, .315f, 620, 86, new Color32(132, 64, 39, 255), IgniteCharcoal);
            UpdateGameplayActionButtons();
            StartCoroutine(AnimateEmbers());
            BuildFrontEnd();
            if (Debug.isDebugBuild) RefreshDebugScaleLabel();
            BuildPauseMenu();
        }

        private void BuildFoodInteractionLayer()
        {
            // Food and tongs move every pointer event. Keep their geometry/raycast rebuilds
            // isolated from the large static gameplay canvas (heat cells, labels, buttons).
            var layer = new GameObject("Interacción carne Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            foodInteractionRoot = layer.GetComponent<RectTransform>();
            foodInteractionRoot.SetParent(gameplayRoot, false);
            foodInteractionRoot.anchorMin = Vector2.zero;
            foodInteractionRoot.anchorMax = Vector2.one;
            foodInteractionRoot.offsetMin = foodInteractionRoot.offsetMax = Vector2.zero;

            Canvas interactionCanvas = layer.GetComponent<Canvas>();
            interactionCanvas.overrideSorting = true;
            interactionCanvas.sortingOrder = 1;
        }

        private void BuildTongsVisual()
        {
            var root = new GameObject("Pinza de parrilla", typeof(RectTransform));
            root.transform.SetParent(foodInteractionRoot, false);
            tongsVisual = root.GetComponent<RectTransform>();
            tongsVisual.anchorMin = tongsVisual.anchorMax = new Vector2(.5f, .5f);
            tongsVisual.sizeDelta = new Vector2(150f, 90f);
            Image handle = MakeImage("Mango de pinza", root.transform, whiteSprite, new Color32(93, 65, 45, 255), new Vector2(.2f, .42f), new Vector2(.82f, .58f), new Vector2(115f, 10f));
            handle.rectTransform.localRotation = Quaternion.Euler(0, 0, -12f);
            for (int i = 0; i < 2; i++)
            {
                Image arm = MakeImage("Brazo metal " + i, root.transform, whiteSprite, new Color32(190, 185, 165, 255), new Vector2(i == 0 ? .72f : .72f, i == 0 ? .63f : .38f), new Vector2(.98f, i == 0 ? .88f : .62f), new Vector2(56f, 5f));
                arm.rectTransform.localRotation = Quaternion.Euler(0, 0, i == 0 ? 16f : -16f);
                tongsArms[i] = arm.rectTransform;
            }
            tongsVisual.gameObject.SetActive(false);
        }

        private void BuildPauseMenu()
        {
            pauseRoot = new GameObject("Pausa", typeof(RectTransform), typeof(CanvasGroup));
            pauseRoot.transform.SetParent(contentRoot, false);
            RectTransform rootRect = pauseRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            pauseCanvasGroup = pauseRoot.GetComponent<CanvasGroup>();
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;

            Image dim = MakeImage("Fondo pausa", pauseRoot.transform, whiteSprite, new Color(0f, 0f, 0f, .68f),
                Vector2.zero, Vector2.one, Vector2.zero);
            dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;
            Image card = MakePanel("Panel pausa", pauseRoot.transform, new Color32(41, 48, 39, 250), .5f, .5f, 820, 850);
            card.raycastTarget = true;
            MakeText("Pausa titulo", pauseRoot.transform, "PAUSA", 50, Cream, TextAnchor.MiddleCenter, .5f, .83f, 700, 80, true);
            MakeButton("CONTINUAR", pauseRoot.transform, .5f, .70f, 520, 92, Green, ContinueGame);
            MakeButton("REINICIAR NIVEL", pauseRoot.transform, .5f, .565f, 520, 92,
                new Color32(184, 111, 49, 255), RestartLevelFromPause);
            MakeButton("VOLVER A NIVELES", pauseRoot.transform, .5f, .43f, 520, 92,
                new Color32(150, 75, 54, 255), ReturnToLevelsFromPause);
            pauseSoundButton = MakeButton("SONIDO", pauseRoot.transform, .34f, .295f, 300, 86,
                new Color32(173, 125, 52, 255), ToggleSound);
            pauseHapticsButton = MakeButton("VIBRACIÓN", pauseRoot.transform, .66f, .295f, 300, 86,
                new Color32(173, 125, 52, 255), ToggleHaptics);
            pauseSoundLabel = pauseSoundButton.GetComponentInChildren<Text>();
            pauseHapticsLabel = pauseHapticsButton.GetComponentInChildren<Text>();
            UpdatePauseSettingsLabels();
            pauseRoot.SetActive(false);
        }

        private void PauseGame()
        {
            if (paused || pauseRoot == null || servingLocked || menuTransitionActive) return;
            paused = true;
            if (gameplayCanvasGroup != null)
            {
                gameplayCanvasGroup.interactable = false;
                gameplayCanvasGroup.blocksRaycasts = false;
            }
            if (activeFoodPointerId != int.MinValue)
                EndFoodPointer(activeFoodPointerIndex, activeFoodPointerId);
            pauseRoot.SetActive(true);
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
            UpdatePauseSettingsLabels();
            if (sizzleSource != null) sizzleSource.Pause();
            if (proceduralSfx != null) proceduralSfx.Pause();
            Time.timeScale = 0f;
        }

        private void ContinueGame()
        {
            if (!paused) return;
            Time.timeScale = 1f;
            paused = false;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
            pauseRoot.SetActive(false);
            gameplayCanvasGroup.interactable = true;
            gameplayCanvasGroup.blocksRaycasts = true;
            if (saveData.Settings.SfxVolume > 0f && proceduralSfx != null) proceduralSfx.Resume();
            if (cooking && saveData.Settings.SfxVolume > 0f && sizzleSource != null) sizzleSource.UnPause();
        }

        private void RestartLevelFromPause()
        {
            ContinueGame();
            if (smokeRoutine != null) StopCoroutine(smokeRoutine);
            smokeRoutine = null;
            platingInProgress = false;
            if (tongsMoveRoutine != null) StopCoroutine(tongsMoveRoutine);
            tongsMoveRoutine = null;
            if (tongsVisual != null) tongsVisual.gameObject.SetActive(false);
            if (sizzleSource != null) sizzleSource.Stop();
            tutorialStep = 0;
            RefreshOrder();
            UpdateGameplayActionButtons();
            tutorialText.text = saveData.Settings.TutorialCompleted
                ? "Tocá la carne y la pinza la acompaña."
                : "Paso 1: prendé el carbón para empezar.";
        }

        private void ReturnToLevelsFromPause()
        {
            ContinueGame();
            ShowLevelSelect();
        }

        private void ToggleSound()
        {
            if (saveData == null || saveData.Settings == null) return;
            if (saveData.Settings.SfxVolume > 0f)
            {
                sfxVolumeBeforeMute = saveData.Settings.SfxVolume;
                saveData.Settings.SfxVolume = 0f;
            }
            else saveData.Settings.SfxVolume = Mathf.Clamp01(sfxVolumeBeforeMute > 0f ? sfxVolumeBeforeMute : .8f);
            if (proceduralSfx != null) proceduralSfx.SetVolume(saveData.Settings.SfxVolume);
            if (sizzleSource != null) sizzleSource.volume = saveData.Settings.SfxVolume * .22f;
            if (saveData.Settings.SfxVolume <= 0f && sizzleSource != null) sizzleSource.Stop();
            UpdatePauseSettingsLabels();
            MvpSave.Save(saveData);
        }

        private void ToggleHaptics()
        {
            if (saveData == null || saveData.Settings == null) return;
            saveData.Settings.HapticsEnabled = !saveData.Settings.HapticsEnabled;
            UpdatePauseSettingsLabels();
            MvpSave.Save(saveData);
        }

        private void UpdatePauseSettingsLabels()
        {
            if (saveData == null || saveData.Settings == null) return;
            if (pauseSoundLabel != null) pauseSoundLabel.text = "SONIDO " + (saveData.Settings.SfxVolume > 0f ? "ON" : "OFF");
            if (pauseHapticsLabel != null) pauseHapticsLabel.text = "VIBRACIÓN " + (saveData.Settings.HapticsEnabled ? "ON" : "OFF");
        }

        private void BuildSizzleAudio()
        {
            proceduralSfx = gameObject.AddComponent<ProceduralSfx>();
            proceduralSfx.Initialize(saveData != null ? saveData.Settings.SfxVolume : .8f);
            var audioObject = new GameObject("Sizzle loop");
            audioObject.transform.SetParent(transform, false);
            sizzleSource = audioObject.AddComponent<AudioSource>();
            sizzleSource.playOnAwake = false;
            sizzleSource.loop = true;
            sizzleSource.spatialBlend = 0f;
            sizzleSource.volume = saveData != null ? saveData.Settings.SfxVolume * .22f : .18f;
            const int sampleRate = 22050;
            var samples = new float[sampleRate * 4];
            var random = new System.Random(1729);
            float filtered = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered = Mathf.Lerp(filtered, noise, .48f);
                float pulse = .55f + .45f * Mathf.Abs(Mathf.Sin(i * .00071f) * Mathf.Sin(i * .00019f));
                samples[i] = filtered * pulse * .24f;
            }
            int crossfadeSamples = sampleRate / 8;
            int crossfadeStart = samples.Length - crossfadeSamples;
            for (int i = 0; i < crossfadeSamples; i++)
            {
                float t = i / (float)(crossfadeSamples - 1);
                samples[crossfadeStart + i] = Mathf.Lerp(samples[crossfadeStart + i], samples[i], t);
            }
            sizzleClip = AudioClip.Create("Asadito Sizzle", samples.Length, 1, sampleRate, false);
            sizzleClip.SetData(samples, 0);
            sizzleSource.clip = sizzleClip;
        }

        private void PlaySfx(AsaditoSfxCue cue)
        {
            if (proceduralSfx != null) proceduralSfx.Play(cue);
        }

        private void CycleSimulationScale()
        {
            float[] choices = { 20f, 25f, 30f, 35f, 40f };
            int next = 0;
            for (int i = 0; i < choices.Length; i++)
                if (Mathf.Approximately(SimulationTimeScale, choices[i])) { next = (i + 1) % choices.Length; break; }
            SimulationTimeScale = choices[next];
            if (saveData != null)
            {
                saveData.Settings.SimulationTimeScale = SimulationTimeScale;
                MvpSave.Save(saveData);
            }
            RefreshDebugScaleLabel();
        }

        private void RefreshDebugScaleLabel()
        {
            if (debugScaleButton != null)
                debugScaleButton.GetComponentInChildren<Text>().text = "DEBUG ×" + Mathf.RoundToInt(SimulationTimeScale);
        }

        private void BuildFrontEnd()
        {
            menuRoot = CreateFullScreenOverlay("Menu principal");
            menuCanvasGroup = menuRoot.AddComponent<CanvasGroup>();
            Image cover = MakeImage("Portada ilustrada", menuRoot.transform, titleSprite != null ? titleSprite : patioSprite,
                titleSprite != null || patioSprite != null ? Color.white : new Color32(31, 34, 29, 255), Vector2.zero, Vector2.one, Vector2.zero);
            cover.rectTransform.offsetMin = cover.rectTransform.offsetMax = Vector2.zero;
            cover.transform.SetAsFirstSibling();
            cover.raycastTarget = false;
            if (titleSprite != null)
                cover.rectTransform.localScale = new Vector3((titleSprite.rect.width / titleSprite.rect.height) / (1080f / 1920f), 1f, 1f);
            Image shade = MakePanel("Velo de contraste portada", menuRoot.transform, new Color32(25, 24, 19, 54), .5f, .5f, 1080, 1920);
            shade.raycastTarget = false;
            Text brand = MakeText("Menu marca", menuRoot.transform, "Asadito", 112, Color.white, TextAnchor.MiddleCenter, .5f, .925f, 720, 142, true);
            var brandOutline = brand.gameObject.AddComponent<Outline>();
            brandOutline.effectColor = new Color32(14, 14, 18, 255);
            brandOutline.effectDistance = new Vector2(5, -5);
            var brandShadow = brand.gameObject.AddComponent<Shadow>();
            brandShadow.effectColor = new Color32(15, 15, 18, 235);
            brandShadow.effectDistance = new Vector2(1, -8);
            RectTransform brandRect = brand.rectTransform;

            Text subtitle = MakeText("Menu subtitulo", menuRoot.transform, "el sabor Argentino", 50,
                Color.white, TextAnchor.MiddleCenter, .5f, .89f, 920, 112, true);
            subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            subtitle.verticalOverflow = VerticalWrapMode.Overflow;
            subtitle.resizeTextForBestFit = true;
            subtitle.resizeTextMinSize = 46;
            subtitle.resizeTextMaxSize = 50;
            Button enterButton = MakeButton("ENTRAR", menuRoot.transform, .5f, .245f, 560, 118, new Color32(218, 121, 45, 255), ShowLevelIntro);
            Button exitButton = MakeButton("SALIR", menuRoot.transform, .5f, .158f, 560, 118, new Color32(92, 72, 55, 245), ExitGame);
            CanvasGroup brandEntrance = brandRect.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup subtitleEntrance = subtitle.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup enterEntrance = enterButton.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup exitEntrance = exitButton.gameObject.AddComponent<CanvasGroup>();
            enterEntrance.interactable = enterEntrance.blocksRaycasts = false;
            exitEntrance.interactable = exitEntrance.blocksRaycasts = false;
            StartCoroutine(AnimateMenuEntrance(brandRect, brandEntrance, .0f, .34f, 18f));
            StartCoroutine(AnimateMenuEntrance(subtitle.rectTransform, subtitleEntrance, .08f, .34f, 14f));
            StartCoroutine(AnimateMenuEntrance(enterButton.GetComponent<RectTransform>(), enterEntrance, .14f, .34f, 22f));
            StartCoroutine(AnimateMenuEntrance(exitButton.GetComponent<RectTransform>(), exitEntrance, .22f, .34f, 18f));
            Image emberGlow = MakeImage("Resplandor ambiental portada", menuRoot.transform, circleSprite,
                new Color(1f, .29f, .07f, .055f), new Vector2(.5f, .48f), new Vector2(.5f, .48f), new Vector2(650, 300));
            emberGlow.transform.SetSiblingIndex(2);
            emberGlow.raycastTarget = false;
            StartCoroutine(AnimateTitleGlow(emberGlow));

            levelSelectRoot = CreateFullScreenOverlay("Seleccion de nivel");
            levelSelectCanvasGroup = levelSelectRoot.AddComponent<CanvasGroup>();
            levelSelectCanvasGroup.alpha = 0f;
            levelSelectRoot.SetActive(false);
            MakePanel("Fondo seleccion nivel", levelSelectRoot.transform, new Color32(42, 40, 33, 250), .5f, .5f, 1080, 1920);
            MakeText("Seleccion titulo", levelSelectRoot.transform, "ELEGÍ TU ASADO", 62, Cream, TextAnchor.MiddleCenter, .5f, .85f, 920, 110, true);
            var viewportObject = new GameObject("Niveles viewport", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect), typeof(Image));
            viewportObject.transform.SetParent(levelSelectRoot.transform, false);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(.045f, .145f);
            viewportRect.anchorMax = new Vector2(.955f, .765f);
            viewportRect.offsetMin = viewportRect.offsetMax = Vector2.zero;
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0f);
            viewportImage.raycastTarget = false;
            var levelContentObject = new GameObject("Niveles contenido", typeof(RectTransform));
            levelContentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform levelContent = levelContentObject.GetComponent<RectTransform>();
            int levelRows = Mathf.CeilToInt(MvpLevelCatalog.Count / 2f);
            const float levelCardStride = 350f;
            levelContent.anchorMin = new Vector2(0f, 1f);
            levelContent.anchorMax = new Vector2(1f, 1f);
            levelContent.pivot = new Vector2(.5f, 1f);
            levelContent.anchoredPosition = Vector2.zero;
            levelContent.sizeDelta = new Vector2(0f, levelRows * levelCardStride + 32f);
            ScrollRect levelScroll = viewportObject.GetComponent<ScrollRect>();
            levelScroll.viewport = viewportRect;
            levelScroll.content = levelContent;
            levelScroll.horizontal = false;
            levelScroll.vertical = true;
            levelScroll.movementType = ScrollRect.MovementType.Clamped;
            levelScroll.scrollSensitivity = 48f;
            levelCards = new Button[MvpLevelCatalog.Count];
            levelCardImages = new Image[levelCards.Length];
            levelCardLabels = new Text[levelCards.Length];
            for (int i = 0; i < levelCards.Length; i++)
            {
                int levelNumber = i + 1;
                int row = i / 2;
                int column = i % 2;
                float x = column == 0 ? .275f : .725f;
                float y = 1f - (row * levelCardStride + 160f) / levelContent.sizeDelta.y;

                Image cardFrame = MakeImage("Marco nivel " + levelNumber, levelContent,
                    roundedButtonSprite, new Color32(55, 34, 23, 255), new Vector2(x, y), new Vector2(x, y),
                    new Vector2(434f, 296f));
                cardFrame.gameObject.name = "NIVEL " + levelNumber;
                cardFrame.type = Image.Type.Sliced;
                cardFrame.raycastTarget = true;
                var cardShadow = cardFrame.gameObject.AddComponent<Shadow>();
                cardShadow.effectColor = new Color(0f, 0f, 0f, .48f);
                cardShadow.effectDistance = new Vector2(0f, -7f);
                levelCards[i] = cardFrame.gameObject.AddComponent<Button>();
                levelCards[i].targetGraphic = cardFrame;
                levelCards[i].onClick.AddListener(() => SelectLevel(levelNumber));
                ColorBlock levelColors = levelCards[i].colors;
                levelColors.normalColor = Color.white;
                levelColors.highlightedColor = new Color32(255, 239, 207, 255);
                levelColors.pressedColor = new Color32(221, 186, 137, 255);
                levelColors.disabledColor = new Color32(126, 120, 111, 255);
                levelCards[i].colors = levelColors;
                levelCards[i].gameObject.AddComponent<AsaditoButtonFeedback>();

                Image illustration = MakeImage("Imagen representativa nivel " + levelNumber,
                    levelCards[i].transform, levelCardSprites != null ? levelCardSprites[i] : null,
                    Color.white, Vector2.zero, Vector2.one, Vector2.zero);
                illustration.rectTransform.offsetMin = new Vector2(7f, 7f);
                illustration.rectTransform.offsetMax = new Vector2(-7f, -7f);
                illustration.preserveAspect = false;
                illustration.raycastTarget = false;
                levelCardImages[i] = illustration;

                Image labelBacking = MakeImage("Fondo etiqueta nivel " + levelNumber,
                    levelCards[i].transform, whiteSprite, new Color32(36, 25, 18, 205),
                    Vector2.zero, Vector2.zero, new Vector2(0f, 72f));
                labelBacking.rectTransform.anchorMin = Vector2.zero;
                labelBacking.rectTransform.anchorMax = new Vector2(1f, 0f);
                labelBacking.rectTransform.pivot = new Vector2(.5f, 0f);
                labelBacking.rectTransform.anchoredPosition = new Vector2(0f, 7f);
                labelBacking.raycastTarget = false;

                levelCardLabels[i] = MakeText("Texto Nivel " + levelNumber, levelCards[i].transform,
                    "Nivel " + levelNumber, 48, Cream, TextAnchor.MiddleCenter, .5f, .145f, 400f, 66f, true);
            }
            MakeButton("VOLVER", levelSelectRoot.transform, .5f, .08f, 390, 82, new Color32(92, 72, 55, 245), BackToMenu);
            RefreshLevelCards();

            introRoot = CreateFullScreenOverlay("Intro de nivel");
            introCanvasGroup = introRoot.AddComponent<CanvasGroup>();
            introCanvasGroup.alpha = 0f;
            introRoot.SetActive(false);
            MakePanel("Sombra intro", introRoot.transform, new Color32(31, 34, 29, 245), .5f, .5f, 1080, 1920);
            introTitleText = MakeText("Intro título", introRoot.transform, "EL DEBUT", 64, Cream, TextAnchor.MiddleCenter, .5f, .66f, 900, 115, true);
            introGuestsText = MakeText("Intro comensales", introRoot.transform, "2 COMENSALES", 31, Gold, TextAnchor.MiddleCenter, .5f, .56f, 850, 64, true);
            Image introGuestIcon = MakeImage("Icon comensales intro", introRoot.transform, AsaditoUiIcons.Get(AsaditoUiIcon.Guest), Gold,
                new Vector2(.34f, .56f), new Vector2(.34f, .56f), new Vector2(32f, 32f));
            introGuestIcon.preserveAspect = true;
            introMenuText = MakeText("Intro menu", introRoot.transform, "CHORIZO ×1   ·   TIRA DE ASADO ×1", 24, Cream, TextAnchor.MiddleCenter, .5f, .535f, 920, 74, true);
            introFoodIcons = new Image[4];
            for (int i = 0; i < introFoodIcons.Length; i++)
            {
                Image icon = MakeImage("Icon comida intro " + (i + 1), introRoot.transform, whiteSprite, Color.white,
                    new Vector2(.5f, .46f), new Vector2(.5f, .46f), new Vector2(42f, 42f));
                icon.rectTransform.anchoredPosition = new Vector2((i - 1.5f) * 52f, 0f);
                icon.preserveAspect = true;
                icon.gameObject.SetActive(false);
                introFoodIcons[i] = icon;
            }
            introObjectiveText = MakeText("Intro objetivo", introRoot.transform, "Prendé el carbón, repartí las brasas y cociná.\nRetirá todo a punto y serví la bandeja.", 25, Cream, TextAnchor.MiddleCenter, .5f, .415f, 850, 118, false);
            MakeButton("IR A LA PARRILLA", introRoot.transform, .5f, .35f, 550, 115, new Color32(199, 139, 54, 255), StartLevel);
            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;
        }

        private GameObject CreateFullScreenOverlay(string objectName)
        {
            var root = new GameObject(objectName, typeof(RectTransform));
            root.transform.SetParent(contentRoot, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return root;
        }

        private void ApplySafeAreaIfChanged(bool force = false)
        {
            if (contentRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            if (!force && lastSafeScreenWidth == Screen.width && lastSafeScreenHeight == Screen.height && lastSafeArea == safe) return;
            lastSafeScreenWidth = Screen.width;
            lastSafeScreenHeight = Screen.height;
            lastSafeArea = safe;
            contentRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            contentRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            contentRoot.offsetMin = contentRoot.offsetMax = Vector2.zero;
        }

        private void ShowLevelIntro()
        {
            ShowLevelSelect();
        }

        private void ShowLevelSelect()
        {
            if (!menuTransitionActive) StartCoroutine(TransitionToLevelSelect());
        }

        private IEnumerator TransitionToLevelSelect()
        {
            menuTransitionActive = true;
            ClearResultUi();
            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;
            RefreshLevelCards();
            bool menuWasVisible = menuRoot.activeInHierarchy;
            levelSelectRoot.SetActive(true);
            levelSelectCanvasGroup.alpha = 0f;
            float t = 0f;
            while (t < .38f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .38f));
                if (menuWasVisible) menuCanvasGroup.alpha = 1f - p;
                levelSelectCanvasGroup.alpha = p;
                yield return null;
            }
            menuRoot.SetActive(false);
            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;
            menuCanvasGroup.alpha = 1f;
            levelSelectCanvasGroup.alpha = 1f;
            menuTransitionActive = false;
        }

        private void BackToMenu()
        {
            if (menuTransitionActive) return;
            levelSelectRoot.SetActive(false);
            menuRoot.SetActive(true);
            levelSelectCanvasGroup.alpha = 0f;
        }

        private void RefreshLevelCards()
        {
            if (levelCards == null || saveData == null) return;
            for (int i = 0; i < levelCards.Length; i++)
            {
                int levelNumber = i + 1;
                bool unlocked = levelNumber <= saveData.MaxUnlockedLevel;
                levelCardLabels[i].text = "Nivel " + levelNumber;
                levelCards[i].interactable = unlocked;
                levelCardImages[i].color = unlocked ? Color.white : new Color32(166, 157, 143, 255);
            }
        }

        private void SelectLevel(int levelNumber)
        {
            if (menuTransitionActive || levelNumber > saveData.MaxUnlockedLevel) return;
            ClearResultUi();
            ConfigureLevel(levelNumber);
            BuildPortionControls();
            RefreshOrder();
            introTitleText.text = "NIVEL " + currentLevelNumber + " · " + currentLevel.Title;
            introGuestsText.text = currentLevel.GuestCount + " COMENSALES";
            introMenuText.text = BuildOrderSummary(currentLevel);
            RefreshIntroFoodIcons(currentLevel);
            introObjectiveText.text = currentLevelNumber == 1
                ? "Prendé el carbón, repartí las brasas y cociná.\nDale vuelta, retiralo a la bandeja y serví."
                : "Leé los pedidos, repartí el calor por zonas y cociná cada pieza.\nRetirá todas a la bandeja y serví la juntada.";
            StartCoroutine(TransitionToIntro());
        }

        private string BuildOrderSummary() => BuildOrderSummary(currentLevel);

        private static string BuildOrderSummary(MvpLevelDefinition level)
        {
            var order = new System.Collections.Generic.List<string>();
            for (int i = 0; i < level.FoodIds.Length; i++)
            {
                string food = level.FoodIds[i];
                bool seen = false;
                for (int j = 0; j < i; j++) if (level.FoodIds[j] == food) { seen = true; break; }
                if (seen) continue;
                int count = 0;
                for (int j = i; j < level.FoodIds.Length; j++) if (level.FoodIds[j] == food) count++;
                order.Add(FoodDisplayName(food) + " ×" + count);
            }
            return string.Join("  ·  ", order.ToArray());
        }

        private static string[] UniqueFoodIds(MvpLevelDefinition level)
        {
            var foods = new System.Collections.Generic.List<string>();
            if (level == null || level.FoodIds == null) return foods.ToArray();
            for (int i = 0; i < level.FoodIds.Length; i++)
            {
                bool seen = false;
                for (int j = 0; j < foods.Count; j++)
                    if (foods[j] == level.FoodIds[i]) { seen = true; break; }
                if (!seen) foods.Add(level.FoodIds[i]);
            }
            return foods.ToArray();
        }

        private void RefreshIntroFoodIcons(MvpLevelDefinition level)
        {
            string[] foods = UniqueFoodIds(level);
            for (int i = 0; i < introFoodIcons.Length; i++)
            {
                bool visible = i < foods.Length;
                introFoodIcons[i].gameObject.SetActive(visible);
                if (!visible) continue;
                introFoodIcons[i].sprite = FoodStateSprite(foods[i], 3) ?? FoodStateSprite(foods[i], 0) ?? whiteSprite;
                introFoodIcons[i].name = "Icon comida intro " + FoodDisplayName(foods[i]);
            }
        }

        private IEnumerator TransitionToIntro()
        {
            menuTransitionActive = true;
            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;
            introRoot.SetActive(true);
            introCanvasGroup.alpha = 0f;
            float t = 0f;
            while (t < .38f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .38f));
                levelSelectCanvasGroup.alpha = 1f - p;
                introCanvasGroup.alpha = p;
                yield return null;
            }
            levelSelectRoot.SetActive(false);
            levelSelectCanvasGroup.alpha = 1f;
            introCanvasGroup.alpha = 1f;
            menuTransitionActive = false;
        }

        private IEnumerator AnimateTitleGlow(Image glow)
        {
            float elapsed = 0f;
            while (glow != null && menuRoot != null && menuRoot.activeInHierarchy)
            {
                elapsed += Time.unscaledDeltaTime;
                float pulse = (Mathf.Sin(elapsed * 1.8f) + 1f) * .5f;
                glow.color = new Color(1f, .29f, .07f, Mathf.Lerp(.035f, .075f, pulse));
                glow.rectTransform.localScale = Vector3.one * Mathf.Lerp(.97f, 1.035f, pulse);
                yield return null;
            }
        }

        private IEnumerator AnimateMenuEntrance(RectTransform target, CanvasGroup group, float delay, float duration, float offsetY)
        {
            if (target == null || group == null) yield break;
            Vector2 destination = target.anchoredPosition;
            Vector3 finalScale = target.localScale;
            group.alpha = 0f;
            target.anchoredPosition = destination - Vector2.up * offsetY;
            target.localScale = finalScale * .94f;
            yield return new WaitForSecondsRealtime(delay);
            float elapsed = 0f;
            while (target != null && group != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                group.alpha = p;
                target.anchoredPosition = Vector2.Lerp(destination - Vector2.up * offsetY, destination, p);
                target.localScale = Vector3.Lerp(finalScale * .94f, finalScale, p);
                yield return null;
            }
            if (target == null || group == null) yield break;
            group.alpha = 1f;
            target.anchoredPosition = destination;
            target.localScale = finalScale;
            group.interactable = group.blocksRaycasts = true;
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void StartLevel()
        {
            if (!menuTransitionActive) StartCoroutine(TransitionToGameplay());
        }

        private IEnumerator TransitionToGameplay()
        {
            menuTransitionActive = true;
            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;
            float t = 0f;
            while (t < .34f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .34f));
                introCanvasGroup.alpha = 1f - p;
                gameplayCanvasGroup.alpha = p;
                yield return null;
            }
            introRoot.SetActive(false);
            introCanvasGroup.alpha = 1f;
            gameplayCanvasGroup.alpha = 1f;
            gameplayCanvasGroup.interactable = true;
            gameplayCanvasGroup.blocksRaycasts = true;
            menuTransitionActive = false;
            tutorialStep = 0;
            tutorialText.text = saveData.Settings.TutorialCompleted ? "El carbón responde a dónde agrupás las brasas." : "Paso 1: prendé el carbón para empezar.";
        }

        private void BuildHeatGrid()
        {
            Image root = MakeImage("Mapa de calor carbón 8x6", gameplayRoot, whiteSprite, new Color(0, 0, 0, 0), new Vector2(.5f, .575f), new Vector2(.5f, .575f), new Vector2(820, 276));
            root.raycastTarget = false;
            heatGridRect = root.rectTransform;
            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 8; x++)
            {
                Image cell = MakeImage("Brasa " + x + "," + y, root.transform, whiteSprite, new Color(.16f, .14f, .12f, .75f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(99, 43));
                cell.rectTransform.anchoredPosition = new Vector2((x - 3.5f) * 102f, (y - 2.5f) * 46f);
                cell.color = new Color(.16f, .14f, .12f, .75f);
                cell.raycastTarget = true;
                EmberCellTouch touch = cell.gameObject.AddComponent<EmberCellTouch>();
                touch.Owner = this; touch.X = x; touch.Y = y;
                heatCellImages.Add(cell);
            }
            RefreshHeatGridVisuals();
        }

        private void IgniteCharcoal()
        {
            if (servingLocked || grill.IsLit) return;
            PlaySfx(AsaditoSfxCue.Ignite);
            grill.HeatWhenLitC = FireTemperature;
            grill.Ignite();
            igniteButton.interactable = false;
            igniteButton.GetComponentInChildren<Text>().text = "CARBÓN ENCENDIDO";
            UpdateGameplayActionButtons();
            feedbackText.text = "Las brasas están listas. Arrastrá las celdas para repartir el calor.";
            AdvanceTutorial(1, "Paso 2: arrastrá brasas para cambiar el calor por zona.");
            AnimateHeatGridTransition();
            StartCoroutine(PlayIgnitionCue());
        }

        public void DragEmbers(int fromX, int fromY, Vector2 screenPosition)
        {
            if (!grill.IsLit || heatGridRect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(heatGridRect, screenPosition, null, out Vector2 local)) return;
            Vector2 normalized = new Vector2(local.x / heatGridRect.rect.width + .5f, local.y / heatGridRect.rect.height + .5f);
            int x = Mathf.Clamp(Mathf.FloorToInt(normalized.x * 8), 0, 7);
            int y = Mathf.Clamp(Mathf.FloorToInt(normalized.y * 6), 0, 5);
            if (grill.MoveEmbers(fromX, fromY, x, y))
            {
                // Raking changes only two cells. Finish any ignition tween once, then update
                // just these cells so a long drag doesn't continually reanimate all 48.
                if (heatVisualRoutine != null)
                {
                    StopCoroutine(heatVisualRoutine);
                    heatVisualRoutine = null;
                    RefreshHeatGridVisuals();
                }
                RefreshHeatCellVisual(fromX, fromY);
                RefreshHeatCellVisual(x, y);
                AdvanceTutorial(2, "Paso 3: tocá directamente una pieza para agarrarla.");
            }
        }

        public void BeginFoodDrag(int index)
        {
            if (!SelectFoodPiece(index)) return;
            activeFoodPointerDragging = activeFoodPointerId != int.MinValue;
            AnimateTongsGrip(true);
            StartCoroutine(LiftFood(PortionImage(index).rectTransform));
        }

        public void DragFood(int index, Vector2 screenPosition)
        {
            DragFood(index, int.MinValue, screenPosition);
        }

        public void DragFood(int index, int pointerId, Vector2 screenPosition)
        {
            if (pointerId != int.MinValue && !IsActiveFoodPointer(index, pointerId)) return;
            if (paused || servingLocked || platingInProgress || !cooking || activePortion != index ||
                !portions[index].Started || portions[index].OnTray || heatGridRect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(heatGridRect, screenPosition, null, out Vector2 local)) return;
            Vector2 size = heatGridRect.rect.size;
            Vector2 normalized = new Vector2(local.x / size.x + .5f, local.y / size.y + .5f);
            portions[index].Position = normalized;
            portionHitTargets[index].position = heatGridRect.TransformPoint(local);
            if (tongsVisual != null)
            {
                tongsVisual.position = screenPosition + new Vector2(38f, 45f);
                if (!tongsVisual.gameObject.activeSelf)
                {
                    // Keep the tong sprite above the food, but don't reorder the Canvas on
                    // every pointer-move event during a drag.
                    tongsVisual.gameObject.SetActive(true);
                    tongsVisual.SetAsLastSibling();
                }
            }
            activePortion = index;
            cooking = true;
            AdvanceTutorial(4, "Mové la pieza por la parrilla y observá el calor de cada zona.");
        }

        public void EndFoodDrag(int index, Vector2 screenPosition)
        {
            EndFoodDrag(index, int.MinValue, screenPosition);
        }

        public void EndFoodDrag(int index, int pointerId, Vector2 screenPosition)
        {
            if (pointerId != int.MinValue && !IsActiveFoodPointer(index, pointerId)) return;
            if (paused || servingLocked || platingInProgress || !cooking || activePortion != index ||
                !portions[index].Started || portions[index].OnTray) return;
            if (trayDropRect != null && RectTransformUtility.RectangleContainsScreenPoint(trayDropRect, screenPosition, null))
            {
                StartCoroutine(PlatePortion(index));
                return;
            }

            ClampFoodToGrill(index);
            cooking = true;
            activePortion = index;
            activeFoodPointerDragging = false;
            StartCoroutine(ReleaseFood(PortionImage(index).rectTransform));
            PositionTongsAtFood(index, true);
            AnimateTongsGrip(false);
        }

        private void ClampFoodToGrill(int index)
        {
            Vector2 position = portions[index].Position;
            Vector2 targetHalf = portionHitTargets[index].rect.size * .5f;
            Vector2 margin = new Vector2(targetHalf.x / heatGridRect.rect.width, targetHalf.y / heatGridRect.rect.height);
            position.x = Mathf.Clamp(position.x, margin.x, 1f - margin.x);
            position.y = Mathf.Clamp(position.y, margin.y, 1f - margin.y);
            SetFoodTargetPosition(index, position);
        }

        private void AdvanceTutorial(int step, string message)
        {
            if (tutorialText == null || saveData == null || saveData.Settings.TutorialCompleted) return;
            // Drag handlers may call this once per pointer-move. Rewriting the same Text
            // and CanvasGroup on every event dirties the whole gameplay canvas needlessly.
            if (step <= tutorialStep) return;
            tutorialStep = step;
            tutorialText.text = message;
        }

        private void RefreshHeatGridVisuals()
        {
            if (heatVisualRoutine != null)
            {
                StopCoroutine(heatVisualRoutine);
                heatVisualRoutine = null;
            }
            if (heatCellImages.Count != 48) return;
            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 8; x++)
            {
                heatCellImages[y * 8 + x].color = HeatCellColor(x, y);
            }
        }

        private void RefreshHeatCellVisual(int x, int y)
        {
            if (x < 0 || x >= 8 || y < 0 || y >= 6 || heatCellImages.Count != 48) return;
            int index = y * 8 + x;
            if (heatCellImages[index] != null)
                heatCellImages[index].color = HeatCellColor(x, y);
        }

        private Color HeatCellColor(int x, int y)
        {
            HeatCell cell = grill.Grid.GetCell(x, y);
            float ember = Mathf.Clamp01(cell.EmberEnergy);
            return grill.IsLit
                ? Color.Lerp(new Color32(77, 57, 44, 215), new Color32(247, 93, 31, 245), ember)
                : (Color)new Color32(64, 58, 49, 200);
        }

        private void AnimateHeatGridTransition()
        {
            if (heatCellImages.Count != 48) return;
            if (heatVisualRoutine != null) StopCoroutine(heatVisualRoutine);
            heatVisualRoutine = StartCoroutine(TransitionHeatGridColors());
        }

        private IEnumerator TransitionHeatGridColors()
        {
            const float duration = .18f;
            var starts = new Color[48];
            var targets = new Color[48];
            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 8; x++)
            {
                int index = y * 8 + x;
                starts[index] = heatCellImages[index].color;
                targets[index] = HeatCellColor(x, y);
            }
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                for (int i = 0; i < heatCellImages.Count; i++)
                    if (heatCellImages[i] != null) heatCellImages[i].color = Color.Lerp(starts[i], targets[i], t);
                yield return null;
            }
            for (int i = 0; i < heatCellImages.Count; i++)
                if (heatCellImages[i] != null) heatCellImages[i].color = targets[i];
            heatVisualRoutine = null;
        }

        private IEnumerator PlayIgnitionCue()
        {
            if (heatGridRect == null) yield break;
            Image flame = MakeImage("Destello de encendido", heatGridRect, AsaditoUiIcons.Get(AsaditoUiIcon.Flame),
                new Color32(255, 194, 93, 245), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(54f, 54f));
            flame.preserveAspect = true;
            float elapsed = 0f;
            const float duration = .42f;
            while (elapsed < duration && flame != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                flame.rectTransform.localScale = Vector3.one * Mathf.Lerp(.45f, 1.45f, Mathf.SmoothStep(0f, 1f, p));
                Color color = flame.color;
                color.a = Mathf.Sin(p * Mathf.PI) * .96f;
                flame.color = color;
                yield return null;
            }
            if (flame != null) Destroy(flame.gameObject);
        }

        private void RefreshOrder()
        {
            grill.Reset();
            grill.HeatWhenLitC = FireTemperature;
            if (igniteButton != null)
            {
                igniteButton.interactable = true;
                igniteButton.GetComponentInChildren<Text>().text = "PRENDER CARBÓN";
            }
            RefreshHeatGridVisuals();
            guestName.text = BuildGuestNamesSummary();
            guestOrder.text = BuildOrderSummary();
            scoreText.text = "NIVEL " + currentLevelNumber + "  •  BANDEJA 0 / " + portions.Length + "  •  " + activeGuests.Length + " COMENSALES";
            DrawAvatar();
            servingLocked = cooking = platingInProgress = false;
            activePortion = -1;
            activeFoodPointerId = int.MinValue;
            activeFoodPointerIndex = -1;
            activeFoodPointerDragging = false;
            trayCount = totalScore = 0;
            for (int i = 0; i < portions.Length; i++)
            {
                portions[i].Reset();
                portions[i].Position = PortionHome(i);
            }
            for (int i = 0; i < portionImages.Length; i++)
            {
                ResetMeat(i);
                RefreshFoodVisual(i);
            }
            cookFill.rectTransform.sizeDelta = new Vector2(0, 18);
            progressText.text = "TOCÁ UNA PIEZA SOBRE LA PARRILLA";
            feedbackText.text = "Retirá todas las piezas antes de servir la bandeja.";
            UpdateFoodSelectionVisuals();
            UpdateGameplayActionButtons();
        }

        private string BuildGuestNamesSummary()
        {
            if (activeGuests.Length <= 3)
            {
                var names = new string[activeGuests.Length];
                for (int i = 0; i < activeGuests.Length; i++) names[i] = activeGuests[i].Name;
                return string.Join(" + ", names);
            }
            return activeGuests[0].Name + " + " + activeGuests[1].Name + " + " + (activeGuests.Length - 2) + " MÁS";
        }

        private void ResetMeat(int index)
        {
            RectTransform target = portionHitTargets[index];
            target.gameObject.SetActive(true);
            target.anchoredPosition = Vector2.zero;
            target.localScale = Vector3.one;
            SetFoodTargetPosition(index, PortionHome(index));
            CanvasGroup hitGroup = target.GetComponent<CanvasGroup>();
            hitGroup.alpha = 1f;
            hitGroup.interactable = true;
            hitGroup.blocksRaycasts = true;
            target.GetComponent<Image>().raycastTarget = true;
            Image meat = portionImages[index];
            meat.gameObject.SetActive(true);
            meat.rectTransform.anchoredPosition = Vector2.zero;
            meat.rectTransform.localScale = Vector3.one;
            meat.color = Color.white;
            portionSelectionHalos[index].gameObject.SetActive(false);
            portionSelectionShadows[index].effectColor = new Color(0f, 0f, 0f, .34f);
            portionSelectionShadows[index].effectDistance = new Vector2(2f, -3f);
        }

        private Image PortionImage(int index) => portionImages[index];

        private void BuildPortionControls()
        {
            if (portionImages != null)
                for (int i = 0; i < portionImages.Length; i++)
                {
                    if (portionHitTargets != null && i < portionHitTargets.Length && portionHitTargets[i] != null)
                        Destroy(portionHitTargets[i].gameObject);
                    else if (portionImages[i] != null) Destroy(portionImages[i].gameObject);
                }

            portionImages = new Image[portions.Length];
            portionHitTargets = new RectTransform[portions.Length];
            portionSelectionHalos = new Image[portions.Length];
            portionSelectionShadows = new Shadow[portions.Length];
            for (int i = 0; i < portions.Length; i++)
            {
                int index = i;
                string foodId = portions[index].Profile.FoodId;
                Vector2 home = PortionHome(index);
                portions[index].Position = home;
                FoodDefinition definition = FoodCatalog.Get(foodId);
                Vector2 visualSize = new Vector2(260f, 190f) * definition.DisplayScale;
                Vector2 touchSize = new Vector2(Mathf.Max(190f, visualSize.x * .72f),
                    Mathf.Max(180f, visualSize.y * .92f));
                Image hitGraphic = MakeImage("Food hit target " + (index + 1), foodInteractionRoot, whiteSprite,
                    new Color(1f, 1f, 1f, 0f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), touchSize);
                hitGraphic.rectTransform.position = heatGridRect.TransformPoint(GridLocalPosition(home));
                hitGraphic.raycastTarget = true;
                CanvasGroup hitGroup = hitGraphic.gameObject.AddComponent<CanvasGroup>();
                hitGroup.alpha = 1f;
                FoodPieceTouch touch = hitGraphic.gameObject.AddComponent<FoodPieceTouch>();
                touch.Owner = this;
                touch.PortionIndex = index;
                portionHitTargets[index] = hitGraphic.rectTransform;

                Sprite rawSprite = FoodStateSprite(foodId, (int)FoodCookVisualStage.Raw);
                Image halo = MakeImage("Halo selección " + definition.DisplayName, hitGraphic.transform,
                    rawSprite != null ? rawSprite : whiteSprite, new Color(1f, .62f, .16f, .58f),
                    new Vector2(.5f, .5f), new Vector2(.5f, .5f), visualSize * 1.12f);
                halo.preserveAspect = true;
                halo.raycastTarget = false;
                halo.gameObject.SetActive(false);
                portionSelectionHalos[index] = halo;

                Image foodImage = MakeImage(definition.DisplayName + " en parrilla", hitGraphic.transform,
                    rawSprite != null ? rawSprite : whiteSprite,
                    rawSprite != null ? Color.white : FoodButtonColor(foodId),
                    new Vector2(.5f, .5f), new Vector2(.5f, .5f), visualSize);
                foodImage.preserveAspect = true;
                foodImage.raycastTarget = false;
                Shadow foodShadow = foodImage.gameObject.AddComponent<Shadow>();
                foodShadow.effectColor = new Color(0f, 0f, 0f, .34f);
                foodShadow.effectDistance = new Vector2(2f, -3f);
                portionSelectionShadows[index] = foodShadow;
                portionImages[index] = foodImage;
            }
        }

        private Vector2 PortionHome(int index)
        {
            int count = portions != null ? portions.Length : 0;
            if (count <= 1) return new Vector2(.5f, .5f);
            if (count == 2) return new Vector2(index == 0 ? .36f : .64f, .5f);
            if (count == 3) return new Vector2(new[] { .2f, .5f, .8f }[index], .5f);
            if (count == 4)
                return new Vector2(index % 2 == 0 ? .34f : .66f, index < 2 ? .75f : .25f);
            if (count == 5)
            {
                if (index < 3) return new Vector2(new[] { .18f, .5f, .82f }[index], .75f);
                return new Vector2(index == 3 ? .36f : .64f, .25f);
            }
            return new Vector2(new[] { .18f, .5f, .82f }[index % 3], index < 3 ? .75f : .25f);
        }

        private Vector2 GridLocalPosition(Vector2 normalizedPosition)
        {
            return new Vector2((normalizedPosition.x - .5f) * heatGridRect.rect.width,
                (normalizedPosition.y - .5f) * heatGridRect.rect.height);
        }

        private void SetFoodTargetPosition(int index, Vector2 normalizedPosition)
        {
            portions[index].Position = normalizedPosition;
            if (portionHitTargets == null || index < 0 || index >= portionHitTargets.Length || portionHitTargets[index] == null)
                return;
            portionHitTargets[index].position = heatGridRect.TransformPoint(GridLocalPosition(normalizedPosition));
        }

        public bool IsFoodTargetClosest(int index, Vector2 screenPoint, Camera eventCamera)
        {
            if (portionHitTargets == null || index < 0 || index >= portionHitTargets.Length ||
                portionHitTargets[index] == null || portions[index].OnTray)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(gameplayRoot, screenPoint, eventCamera, out Vector2 point))
                return false;

            float closestScore = float.MaxValue;
            int closestIndex = -1;
            bool pointInsideAnyTarget = false;
            for (int i = 0; i < portionHitTargets.Length; i++)
            {
                RectTransform target = portionHitTargets[i];
                if (target == null || !target.gameObject.activeInHierarchy || portions[i].OnTray) continue;
                Vector2 delta = point - target.anchoredPosition;
                Vector2 half = target.rect.size * .5f;
                if (half.x <= 0f || half.y <= 0f) continue;
                float score = (delta.x * delta.x) / (half.x * half.x) + (delta.y * delta.y) / (half.y * half.y);
                if (score <= 1f) pointInsideAnyTarget = true;
                if (score < closestScore)
                {
                    closestScore = score;
                    closestIndex = i;
                }
            }
            return pointInsideAnyTarget && closestIndex == index && closestScore <= 1f;
        }

        private static string FoodDisplayName(string foodId)
        {
            return FoodCatalog.Get(foodId).DisplayName;
        }

        private static Color FoodButtonColor(string foodId)
        {
            string category = FoodCatalog.Get(foodId).Category;
            switch (category)
            {
                case "Queso": return new Color32(172, 133, 66, 255);
                case "Cerdo": return new Color32(154, 87, 66, 255);
                case "Ave": return new Color32(145, 111, 63, 255);
                case "Achura":
                case "Embutido": return new Color32(136, 66, 52, 255);
                default: return new Color32(114, 63, 46, 255);
            }
        }

        private void UpdateGameplayActionButtons()
        {
            bool canInteract = !paused && !servingLocked && !platingInProgress;
            if (flipButton != null) flipButton.interactable = canInteract && cooking && activePortion >= 0;
            if (plateButton != null) plateButton.interactable = canInteract && cooking && activePortion >= 0;
            if (serveButton != null) serveButton.interactable = canInteract && !cooking && trayCount == portions.Length;
            if (pauseButton != null) pauseButton.interactable = !paused && !servingLocked;
        }

        public bool BeginFoodPointer(int index, int pointerId)
        {
            if (index < 0 || portions == null || index >= portions.Length || paused || servingLocked || platingInProgress)
                return false;
            if (activeFoodPointerId != int.MinValue &&
                (activeFoodPointerId != pointerId || activeFoodPointerIndex != index))
                return false;
            if (!SelectFoodPiece(index)) return false;
            activeFoodPointerId = pointerId;
            activeFoodPointerIndex = index;
            return true;
        }

        public bool IsActiveFoodPointer(int index, int pointerId)
        {
            return activeFoodPointerId == pointerId && activeFoodPointerIndex == index;
        }

        public void EndFoodPointer(int index, int pointerId)
        {
            if (!IsActiveFoodPointer(index, pointerId)) return;
            if (activeFoodPointerDragging && index >= 0 && index < portions.Length &&
                !portions[index].OnTray && !platingInProgress)
            {
                ClampFoodToGrill(index);
                StartCoroutine(ReleaseFood(PortionImage(index).rectTransform));
                if (tongsVisual != null) PositionTongsAtFood(index, true);
                AnimateTongsGrip(false);
            }
            activeFoodPointerId = int.MinValue;
            activeFoodPointerIndex = -1;
            activeFoodPointerDragging = false;
        }

        public bool SelectFoodPiece(int index)
        {
            if (index < 0 || portions == null || index >= portions.Length || paused || servingLocked || platingInProgress)
                return false;
            if (portions[index].OnTray) return false;
            if (!grill.IsLit)
            {
                if (feedbackText != null) feedbackText.text = "Prendé el carbón antes de cocinar.";
                return false;
            }
            if (cooking && activePortion != index)
            {
                if (feedbackText != null) feedbackText.text = "Terminá la pieza que está en la pinza primero.";
                return false;
            }

            bool newlyStarted = !portions[index].Started;
            activePortion = index;
            cooking = true;
            if (newlyStarted)
            {
                portions[index].Started = true;
                PlaySfx(AsaditoSfxCue.FoodDrop);
                StartCoroutine(PlaceMeat(PortionImage(index).rectTransform));
                if (sizzleSource != null && saveData.Settings.SfxVolume > 0f && !sizzleSource.isPlaying) sizzleSource.Play();
                smokeRoutine = StartCoroutine(SmokePuffs());
            }
            UpdateFoodSelectionVisuals();
            PositionTongsAtFood(index, newlyStarted);
            AnimateTongsGrip(true);
            UpdateGameplayActionButtons();
            if (feedbackText != null) feedbackText.text = "La pinza la sostiene: arrastrala, dale vuelta o llevala a la bandeja.";
            AdvanceTutorial(3, "Paso 4: tocá una pieza y arrastrala con la pinza.");
            return true;
        }

        private void UpdateFoodSelectionVisuals()
        {
            if (portionSelectionHalos == null) return;
            for (int i = 0; i < portionSelectionHalos.Length; i++)
            {
                bool selected = cooking && !portions[i].OnTray && i == activePortion;
                if (portionSelectionHalos[i] != null) portionSelectionHalos[i].gameObject.SetActive(selected);
                if (portionImages[i] != null) portionImages[i].rectTransform.localScale = selected ? Vector3.one * 1.045f : Vector3.one;
                if (portionSelectionShadows[i] != null)
                {
                    portionSelectionShadows[i].effectColor = selected
                        ? new Color(0f, 0f, 0f, .52f) : new Color(0f, 0f, 0f, .34f);
                    portionSelectionShadows[i].effectDistance = selected ? new Vector2(5f, -8f) : new Vector2(2f, -3f);
                }
            }
        }

        private void PositionTongsAtFood(int index, bool animate)
        {
            if (tongsVisual == null || portionHitTargets == null || portionHitTargets[index] == null) return;
            if (tongsMoveRoutine != null) StopCoroutine(tongsMoveRoutine);
            Vector3 destination = portionHitTargets[index].position + new Vector3(34f, 34f, 0f);
            bool wasHidden = !tongsVisual.gameObject.activeSelf;
            tongsVisual.gameObject.SetActive(true);
            if (wasHidden) tongsVisual.SetAsLastSibling();
            if (animate) tongsMoveRoutine = StartCoroutine(MoveTongsTo(destination));
            else tongsVisual.position = destination;
        }

        private void AnimateTongsGrip(bool gripping)
        {
            if (tongsGripRoutine != null) StopCoroutine(tongsGripRoutine);
            tongsGripRoutine = StartCoroutine(AnimateTongsGripRoutine(gripping));
        }

        private IEnumerator AnimateTongsGripRoutine(bool gripping)
        {
            if (tongsArms == null || tongsArms.Length < 2 || tongsArms[0] == null || tongsArms[1] == null)
                yield break;
            float[] starts = { tongsArms[0].localRotation.eulerAngles.z, tongsArms[1].localRotation.eulerAngles.z };
            float targetMagnitude = gripping ? 7f : 16f;
            float[] targets = { targetMagnitude, -targetMagnitude };
            float elapsed = 0f;
            while (elapsed < .1f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / .1f));
                for (int i = 0; i < 2; i++)
                    tongsArms[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(starts[i], targets[i], t));
                yield return null;
            }
            for (int i = 0; i < 2; i++)
                if (tongsArms[i] != null) tongsArms[i].localRotation = Quaternion.Euler(0f, 0f, targets[i]);
            tongsGripRoutine = null;
        }

        private IEnumerator MoveTongsTo(Vector3 destination)
        {
            Vector3 start = tongsVisual.position;
            float elapsed = 0f;
            while (elapsed < .16f && tongsVisual != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / .16f));
                tongsVisual.position = Vector3.Lerp(start, destination, t);
                yield return null;
            }
            if (tongsVisual != null) tongsVisual.position = destination;
            tongsMoveRoutine = null;
        }

        private void PlateSelectedPortion()
        {
            if (cooking && activePortion >= 0 && !servingLocked && !platingInProgress)
                StartCoroutine(PlatePortion(activePortion));
        }

        private IEnumerator PlatePortion(int index)
        {
            if (index < 0 || index >= portions.Length || !cooking || activePortion != index || portions[index].OnTray || platingInProgress)
                yield break;
            platingInProgress = true;
            cooking = false;
            activePortion = -1;
            portions[index].OnTray = true;
            if (portionHitTargets[index] != null)
            {
                portionHitTargets[index].GetComponent<Image>().raycastTarget = false;
                portionHitTargets[index].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
            if (portionSelectionHalos[index] != null) portionSelectionHalos[index].gameObject.SetActive(false);
            if (tongsVisual != null) tongsVisual.gameObject.SetActive(false);
            UpdateFoodSelectionVisuals();
            UpdateGameplayActionButtons();
            PlaySfx(AsaditoSfxCue.Plate);
            VibrateFeedback();
            if (smokeRoutine != null) StopCoroutine(smokeRoutine);
            smokeRoutine = null;
            if (sizzleSource != null) sizzleSource.Stop();
            yield return ServeAnimation(portionHitTargets[index], index);
            trayCount++;
            platingInProgress = false;
            scoreText.text = "NIVEL " + currentLevelNumber + "  •  BANDEJA " + trayCount + " / " + portions.Length + "  •  " + activeGuests.Length + " COMENSALES";
            progressText.text = trayCount == portions.Length ? "BANDEJA LISTA PARA SERVIR" : "TOCÁ LA PRÓXIMA PIEZA";
            feedbackText.text = trayCount == portions.Length
                ? "Todo listo: serví la bandeja a los comensales."
                : "Elegí directamente otra pieza sobre la parrilla.";
            AdvanceTutorial(trayCount == portions.Length ? 6 : 5,
                trayCount == portions.Length ? "Paso 6: serví la bandeja." : "Tocá la próxima pieza para continuar.");
            UpdateGameplayActionButtons();
        }

        private void FlipMeat()
        {
            if (!cooking || servingLocked || activePortion < 0 || platingInProgress) return;
            PlaySfx(AsaditoSfxCue.Flip);
            portions[activePortion].Flip();
            StartCoroutine(FlipAnimation(PortionImage(activePortion).rectTransform));
            RefreshFoodVisual(activePortion);
            PositionTongsAtFood(activePortion, true);
            AdvanceTutorial(5, "La otra cara ya está al fuego. Retirala cuando llegue al punto pedido.");
            VibrateFeedback();
            feedbackText.text = "La cara fría queda hacia las brasas.";
        }

        private void VibrateFeedback()
        {
            if (saveData != null && saveData.Settings.HapticsEnabled && Application.isMobilePlatform)
                Handheld.Vibrate();
        }

        private IEnumerator FlipAnimation(RectTransform target)
        {
            Vector3 start = target.localScale;
            Vector2 startPosition = target.anchoredPosition;
            Quaternion startRotation = target.localRotation;
            float t = 0f;
            while (t < .3f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / .3f);
                float arc = Mathf.Sin(p * Mathf.PI);
                float scale = Mathf.Abs(Mathf.Cos(p * Mathf.PI));
                target.anchoredPosition = startPosition + Vector2.up * (32f * arc);
                target.localScale = new Vector3(Mathf.Max(.05f, scale), start.y * (1f + .08f * arc), start.z);
                target.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(startRotation.eulerAngles.z, startRotation.eulerAngles.z - 8f, arc));
                yield return null;
            }
            target.localScale = start;
            target.anchoredPosition = startPosition;
            target.localRotation = startRotation;
        }

        private IEnumerator LiftFood(RectTransform target)
        {
            if (target == null) yield break;
            Vector3 startScale = target.localScale;
            Quaternion startRotation = target.localRotation;
            float elapsed = 0f;
            const float duration = .12f;
            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                target.localScale = Vector3.Lerp(startScale, startScale * 1.08f, t);
                target.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(startRotation.eulerAngles.z, startRotation.eulerAngles.z - 5f, t));
                yield return null;
            }
        }

        private IEnumerator ReleaseFood(RectTransform target)
        {
            if (target == null) yield break;
            Vector3 startScale = target.localScale;
            Quaternion startRotation = target.localRotation;
            float elapsed = 0f;
            const float duration = .14f;
            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                target.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                target.localRotation = Quaternion.Lerp(startRotation, Quaternion.identity, t);
                yield return null;
            }
            if (target == null) yield break;
            target.localScale = Vector3.one;
            target.localRotation = Quaternion.identity;
        }

        private IEnumerator PlaceMeat(RectTransform target)
        {
            float t = 0f;
            Vector3 small = Vector3.one * .58f;
            while (t < .22f)
            {
                t += Time.deltaTime;
                float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / .22f), 3f);
                target.localScale = Vector3.Lerp(small, Vector3.one, eased);
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        private IEnumerator ServeAnimation(RectTransform target, int index)
        {
            if (target == null || trayRect == null) yield break;
            Vector3 start = target.position;
            Vector3 destination = trayRect.position + new Vector3((index % 3 - 1) * 48f, (index / 3) % 2 == 0 ? 12f : -12f, 0f);
            Vector3 startScale = target.localScale;
            float elapsed = 0f;
            while (elapsed < .42f)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.Clamp01(elapsed / .42f);
                float eased = Mathf.SmoothStep(0f, 1f, p);
                target.position = Vector3.Lerp(start, destination, eased) + Vector3.up * (36f * Mathf.Sin(p * Mathf.PI));
                target.localScale = Vector3.Lerp(startScale, Vector3.one * .68f, eased);
                yield return null;
            }
            target.position = destination;
            target.localScale = Vector3.one * .68f;
        }

        private void Serve()
        {
            if (servingLocked || cooking || trayCount != portions.Length) return;
            PlaySfx(AsaditoSfxCue.Serve);
            servingLocked = true;
            SetActionButtons(false);
            GuestProfile[] profiles = activeGuests;
            var servings = new System.Collections.Generic.List<ServingPortion>(portions.Length);
            for (int i = 0; i < portions.Length; i++)
                servings.Add(new ServingPortion { Id = PortionId(i), FoodId = portions[i].Profile.FoodId, Amount = portions[i].Amount,
                    Doneness = FoodCookingModel.GetDoneness(portions[i].State, portions[i].Profile), CookingQuality = GetCookingQuality(portions[i]) });
            var assignments = ServingAllocator.Allocate(profiles, servings);
            string reactions = "";
            totalScore = 0;
            guestExpressions = new int[profiles.Length];
            ScoreConfig scoreConfig = new ScoreConfig();
            for (int i = 0; i < profiles.Length; i++)
            {
                int portionIndex = FindAssignedPortion(assignments, profiles[i].Id);
                if (portionIndex < 0) { reactions += profiles[i].Name + ": sin porción\n"; guestExpressions[i] = 3; continue; }
                PlayablePortion portion = portions[portionIndex];
                float assignedAmount = Mathf.Min(portion.Amount, profiles[i].TargetFoodAmount);
                ScoreBreakdown breakdown = new ScoreBreakdown
                {
                    CookingQuality = GetCookingQuality(portion),
                    Satiety = profiles[i].TargetFoodAmount <= 0f ? 100f : Mathf.Clamp01(assignedAmount / profiles[i].TargetFoodAmount) * 100f,
                    DonenessMatch = FoodCookingModel.EvaluateDonenessMatch(portion.State, portion.Profile, profiles[i].PreferredDoneness),
                    FoodPreference = ServingAllocator.GetFoodPreferenceScore(profiles[i], servings[portionIndex].FoodId)
                };
                int points = Mathf.RoundToInt(breakdown.Total(scoreConfig));
                guestExpressions[i] = points >= 85 ? 2 : points >= 55 ? 1 : 3;
                totalScore += points;
                reactions += profiles[i].Name + ": cocción " + Mathf.RoundToInt(breakdown.CookingQuality) +
                    " · saciedad " + Mathf.RoundToInt(breakdown.Satiety) + " · punto " + Mathf.RoundToInt(breakdown.DonenessMatch) +
                    " · gusto " + Mathf.RoundToInt(breakdown.FoodPreference) + " = " + points + "/100\n";
            }
            reactions += "Total: " + totalScore + " / " + (profiles.Length * 100);
            int normalizedScore = Mathf.RoundToInt(totalScore * 2f / Mathf.Max(1, profiles.Length));
            totalStars = starThresholds.Evaluate(normalizedScore);
            if (saveData != null && saveData.Settings != null && !saveData.Settings.TutorialCompleted)
            {
                saveData.Settings.TutorialCompleted = true;
                MvpSave.Save(saveData);
            }
            MvpSave.RecordLevelResult(currentLevelNumber, totalScore, totalStars);
            saveData = MvpSave.Load();
            feedbackText.text = reactions;
            StartCoroutine(GuestReaction(totalScore / Mathf.Max(1, activeGuests.Length)));
        }

        private int FindAssignedPortion(System.Collections.Generic.List<ServingAssignment> assignments, string guestId)
        {
            for (int i = 0; i < assignments.Count; i++)
                if (assignments[i].GuestId == guestId)
                    for (int portion = 0; portion < portions.Length; portion++)
                        if (assignments[i].PortionId == PortionId(portion)) return portion;
            return -1;
        }

        private static string PortionId(int index) => "portion-" + index;

        private static float GetCookingQuality(PlayablePortion portion)
        {
            float faceImbalance = Mathf.Abs(portion.State.Faces[0].Maillard - portion.State.Faces[1].Maillard);
            return Mathf.Clamp(100f - portion.State.Char * 65f - (1f - portion.State.Moisture) * 30f + portion.State.Maillard * 12f
                               - faceImbalance * portion.Profile.FaceBalanceWeight * 22f - portion.State.SplitRisk * 38f, 0f, 100f);
        }

        private IEnumerator GuestReaction(int points)
        {
            RectTransform face = avatar.GetComponent<RectTransform>();
            Vector3 original = face.localScale;
            for (int guestIndex = 0; guestIndex < activeGuests.Length; guestIndex++)
            {
                ApplyGuestPortrait(guestIndex, guestExpressions != null && guestIndex < guestExpressions.Length ? guestExpressions[guestIndex] : 1);
                float t = 0f;
                while (t < .18f)
                {
                    t += Time.unscaledDeltaTime;
                    face.localScale = original * (1f + Mathf.Sin(t / .18f * Mathf.PI) * (points >= 65 ? .22f : .1f));
                    yield return null;
                }
            }
            face.localScale = original;
            ApplyGuestPortrait(0, guestExpressions != null && guestExpressions.Length > 0 ? guestExpressions[0] : 1);
            yield return new WaitForSecondsRealtime(.5f);
            ShowFinalScore();
        }

        private void ShowFinalScore()
        {
            PlaySfx(AsaditoSfxCue.Result);
            resultsRoot = new GameObject("Resultados", typeof(RectTransform), typeof(CanvasGroup));
            resultsRoot.transform.SetParent(contentRoot, false);
            RectTransform resultsRect = resultsRoot.GetComponent<RectTransform>();
            resultsRect.anchorMin = Vector2.zero;
            resultsRect.anchorMax = Vector2.one;
            resultsRect.offsetMin = resultsRect.offsetMax = Vector2.zero;
            CanvasGroup resultsGroup = resultsRoot.GetComponent<CanvasGroup>();
            resultsGroup.alpha = 0f;
            resultsGroup.interactable = false;
            resultsGroup.blocksRaycasts = true;
            Transform resultContent = resultsRoot.transform;

            MakePanel("Fin de nivel", resultContent, new Color32(39, 48, 39, 248), .5f, .515f, 900, 900);
            MakeText("Resultado titulo", resultContent, "¡ASADO TERMINADO!", 46, Cream, TextAnchor.MiddleCenter, .5f, .68f, 850, 80, true);
            int maxScore = activeGuests.Length * 100;
            resultScoreText = MakeText("Resultado puntos", resultContent, "0 / " + maxScore + " PUNTOS", 43, Gold, TextAnchor.MiddleCenter, .5f, .616f, 900, 74, true);
            if (scoreAnimationRoutine != null) StopCoroutine(scoreAnimationRoutine);
            scoreAnimationRoutine = StartCoroutine(AnimateResultScore(resultScoreText));
            resultStarIcons = new Image[3];
            for (int i = 0; i < resultStarIcons.Length; i++)
            {
                Image star = MakeImage("Resultado estrella " + (i + 1), resultContent, AsaditoUiIcons.Get(AsaditoUiIcon.Star),
                    i < totalStars ? Gold : new Color32(139, 119, 91, 155), new Vector2(.5f, .575f), new Vector2(.5f, .575f), new Vector2(26f, 26f));
                star.rectTransform.anchoredPosition = new Vector2((i - 1) * 34f, 0f);
                resultStarIcons[i] = star;
                StartCoroutine(PopResultStar(star, i * .055f, i < totalStars));
            }
            MakeText("Resultado detalle", resultContent, feedbackText.text, 15, Cream, TextAnchor.MiddleCenter, .5f, .532f, 850, 95, false);
            string[] resultFoods = UniqueFoodIds(currentLevel);
            resultFoodIcons = new Image[resultFoods.Length];
            for (int i = 0; i < resultFoods.Length; i++)
            {
                Image food = MakeImage("Resultado icon comida " + FoodDisplayName(resultFoods[i]), resultContent,
                    FoodStateSprite(resultFoods[i], 3) ?? FoodStateSprite(resultFoods[i], 0), Color.white,
                    new Vector2(.5f, .472f), new Vector2(.5f, .472f), new Vector2(42f, 42f));
                food.rectTransform.anchoredPosition = new Vector2((i - (resultFoods.Length - 1) * .5f) * 54f, 0f);
                food.preserveAspect = true;
                resultFoodIcons[i] = food;
            }
            BuildResultGuestPortraits(resultContent);
            MakeButton("REINTENTAR", resultContent, .29f, .32f, 390, 86, Green, Retry).interactable = true;
            bool hasNext = currentLevelNumber < MvpLevelCatalog.Count;
            int nextLevel = currentLevelNumber + 1;
            bool nextUnlocked = hasNext && saveData.MaxUnlockedLevel >= nextLevel;
            UnityEngine.Events.UnityAction nextAction = !hasNext ? (UnityEngine.Events.UnityAction)ShowLevelSelect :
                (nextUnlocked ? PlayNextLevel : () => { });
            MakeButton(hasNext ? (nextUnlocked ? "SIGUIENTE" : "BLOQUEADO") : "NIVELES", resultContent,
                .71f, .32f, 390, 86, new Color32(199, 139, 54, 255), nextAction).interactable = !hasNext || nextUnlocked;
            SetActionButtons(false);
            StartCoroutine(AnimateResultsEntrance(resultsRoot));
        }

        private IEnumerator AnimateResultsEntrance(GameObject root)
        {
            if (root == null) yield break;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            RectTransform rect = root.GetComponent<RectTransform>();
            float elapsed = 0f;
            const float duration = .26f;
            rect.localScale = Vector3.one * .96f;
            while (elapsed < duration && root != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                group.alpha = t;
                rect.localScale = Vector3.Lerp(Vector3.one * .96f, Vector3.one, t);
                yield return null;
            }
            if (root == null) yield break;
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
            rect.localScale = Vector3.one;
        }

        private IEnumerator PopResultStar(Image star, float delay, bool earned)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            if (star == null) yield break;
            if (earned) PlaySfx(AsaditoSfxCue.Star);
            const float duration = .18f;
            float elapsed = 0f;
            star.rectTransform.localScale = Vector3.one * .58f;
            while (elapsed < duration && star != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                float scale = Mathf.Lerp(.58f, 1f, p) + Mathf.Sin(p * Mathf.PI) * .12f;
                star.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            if (star != null) star.rectTransform.localScale = Vector3.one;
        }

        private void BuildResultGuestPortraits(Transform parent)
        {
            if (activeGuests == null || activeGuests.Length == 0) return;
            float span = activeGuests.Length <= 1 ? 0f : Mathf.Min(.48f, .08f * (activeGuests.Length - 1));
            float start = .5f - span * .5f;
            for (int i = 0; i < activeGuests.Length; i++)
            {
                GuestProfile guest = activeGuests[i];
                float x = activeGuests.Length <= 1 ? .5f : start + span * i / (activeGuests.Length - 1);
                int expression = guestExpressions != null && i < guestExpressions.Length ? guestExpressions[i] : 0;
                Sprite portrait = GuestPortraitSprite(guest.Id, expression);
                Image image = MakeImage("Resultado retrato " + guest.Name, parent, portrait, Color.white,
                    new Vector2(x, .405f), new Vector2(x, .405f), new Vector2(60f, 60f));
                image.preserveAspect = true;
                image.raycastTarget = false;
                var outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(245, 177, 72, 225);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
                var shadow = image.gameObject.AddComponent<Shadow>();
                shadow.effectDistance = new Vector2(1f, -2f);
            }
        }

        private IEnumerator AnimateResultScore(Text label)
        {
            if (label == null) yield break;
            float elapsed = 0f;
            label.rectTransform.localScale = Vector3.one * .82f;
            while (elapsed < .58f)
            {
                if (label == null) { scoreAnimationRoutine = null; yield break; }
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / .58f);
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                int points = Mathf.RoundToInt(totalScore * eased);
                label.text = points + " / " + (activeGuests.Length * 100) + " PUNTOS";
                label.rectTransform.localScale = Vector3.one * Mathf.Lerp(.82f, 1f, eased);
                yield return null;
            }
            if (label == null) { scoreAnimationRoutine = null; yield break; }
            label.text = totalScore + " / " + (activeGuests.Length * 100) + " PUNTOS";
            label.rectTransform.localScale = Vector3.one;
            scoreAnimationRoutine = null;
        }

        private void Retry()
        {
            totalScore = 0;
            ClearResultUi();
            RefreshOrder();
        }

        private void ClearResultUi()
        {
            if (scoreAnimationRoutine != null)
            {
                StopCoroutine(scoreAnimationRoutine);
                scoreAnimationRoutine = null;
            }
            if (resultsRoot != null) Destroy(resultsRoot);
            else
            {
                Transform result = contentRoot.Find("Fin de nivel");
                if (result != null) Destroy(result.gameObject);
            }
            resultsRoot = null;
            resultScoreText = null;
            resultStarIcons = null;
            resultFoodIcons = null;
        }

        private void PlayNextLevel()
        {
            if (currentLevelNumber >= MvpLevelCatalog.Count) return;
            SelectLevel(currentLevelNumber + 1);
        }

        private void UpdateCookFeedback()
        {
            if (portions == null || activePortion < 0 || activePortion >= portions.Length ||
                cookFill == null || progressText == null)
            {
                // A scene/test transition can tear down the active UI between frames.
                // Do not keep a cooking flag with an invalid portion index or let the
                // next Update throw while Unity is unloading the old scene.
                cooking = false;
                activePortion = -1;
                return;
            }

            PlayablePortion portion = portions[activePortion];
            cookFill.rectTransform.sizeDelta = new Vector2(760f * Mathf.Clamp01(portion.State.CoreTemperatureC / 80f), 18f);
            progressText.text = Mathf.RoundToInt(portion.State.CoreTemperatureC) + " °C  •  " + portion.Point + "  •  zona " + Mathf.RoundToInt(grill.Sample(portion.Position, new Vector2(.14f, .14f)).x) + " °C";
            RefreshFoodVisual(activePortion);
        }

        private void RefreshFoodVisual(int index)
        {
            PlayablePortion portion = portions[index];
            FoodFaceState face = portion.State.CurrentFace;
            float charAmount = Mathf.Clamp01(face.Char);
            int stage = (int)FoodCookingModel.GetVisualStage(portion.State, portion.Profile);
            Sprite stateSprite = FoodStateSprite(portion.Profile.FoodId, stage);
            Image image = PortionImage(index);
            if (stateSprite != null)
            {
                image.sprite = stateSprite;
                image.preserveAspect = true;
                image.color = Color.white;
                if (portionSelectionHalos != null && index < portionSelectionHalos.Length && portionSelectionHalos[index] != null)
                    portionSelectionHalos[index].sprite = stateSprite;
            }
            else
            {
                Color burnt = new Color32(46, 36, 31, 255);
                image.color = Color.Lerp(Color.white, burnt, charAmount);
            }
        }

        private IEnumerator AnimateEmbers()
        {
            while (true)
            {
                if (fireGlow != null)
                {
                    float pulse = .12f + Mathf.PingPong(Time.time * .12f, .12f);
                    fireGlow.color = new Color(1f, .34f, .06f, pulse);
                    fireGlow.rectTransform.localScale = Vector3.one * (1f + pulse * .3f);
                }
                yield return null;
            }
        }

        private IEnumerator SmokePuffs()
        {
            while (cooking && !servingLocked)
            {
                var puff = MakeImage("Humo", gameplayRoot, circleSprite, new Color(.94f, .92f, .86f, .34f), new Vector2(.43f + Random.Range(0, .14f), .65f), new Vector2(.43f + Random.Range(0, .14f), .65f), new Vector2(Random.Range(34, 68), Random.Range(34, 68)));
                puff.raycastTarget = false;
                StartCoroutine(FloatSmoke(puff.rectTransform));
                yield return new WaitForSeconds(.48f);
            }
        }

        private IEnumerator FloatSmoke(RectTransform puff)
        {
            Vector2 start = puff.anchoredPosition;
            float duration = Random.Range(1.1f, 1.8f);
            float t = 0;
            Image image = puff.GetComponent<Image>();
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                puff.anchoredPosition = start + new Vector2(Mathf.Sin(p * 4f) * 28f, p * 230f);
                puff.localScale = Vector3.one * (1f + p * .65f);
                image.color = new Color(.94f, .92f, .86f, .34f * (1f - p));
                yield return null;
            }
            Destroy(puff.gameObject);
        }

        private void DrawAvatar()
        {
            foreach (Transform child in avatar.transform) Destroy(child.gameObject);
            Sprite portrait = GuestPortraitSprite(activeGuests != null && activeGuests.Length > 0 ? activeGuests[0].Id : "ana", 0);
            avatarImage = MakeImage("Retrato comensal", avatar.transform, portrait, Color.white, Vector2.zero, Vector2.one, Vector2.zero);
            avatarImage.preserveAspect = true;
            avatarImage.raycastTarget = false;
            var outline = avatarImage.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(244, 184, 88, 225);
            outline.effectDistance = new Vector2(2f, -2f);
            var shadow = avatarImage.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, .24f);
            shadow.effectDistance = new Vector2(1f, -3f);
        }

        private void ApplyGuestPortrait(int guestIndex, int expression)
        {
            if (avatarImage == null || activeGuests == null || guestIndex < 0 || guestIndex >= activeGuests.Length) return;
            Sprite portrait = GuestPortraitSprite(activeGuests[guestIndex].Id, expression);
            if (portrait == null) return;
            avatarImage.sprite = portrait;
            if (guestName != null) guestName.text = activeGuests[guestIndex].Name;
        }

        private Sprite GuestPortraitSprite(string guestId, int expression)
        {
            if (guestPortraitSprites == null) return null;
            string[] guestIds = { "ana", "tito", "luz", "beto", "mora", "rulo" };
            int guestIndex = System.Array.IndexOf(guestIds, guestId);
            if (guestIndex < 0 || expression < 0 || expression >= guestPortraitSprites.GetLength(1)) return null;
            return guestPortraitSprites[guestIndex, expression];
        }

        private static Sprite[,] CreateGuestPortraitSprites(Texture2D atlas)
        {
            string[] guestIds = { "ana", "tito", "luz", "beto", "mora", "rulo" };
            string[] expressions = { "neutral", "happy", "very_happy", "disappointed" };
            int cellWidth = atlas.width / expressions.Length;
            int cellHeight = atlas.height / guestIds.Length;
            var sprites = new Sprite[guestIds.Length, expressions.Length];
            for (int guest = 0; guest < guestIds.Length; guest++)
            for (int expression = 0; expression < expressions.Length; expression++)
            {
                float y = atlas.height - (guest + 1) * cellHeight;
                var rect = new Rect(expression * cellWidth, y, cellWidth, cellHeight);
                sprites[guest, expression] = Sprite.Create(atlas, rect, new Vector2(.5f, .5f), 100f);
                sprites[guest, expression].name = guestIds[guest] + "_" + expressions[expression];
            }
            return sprites;
        }

        private Sprite FoodStateSprite(string foodId, int stage)
        {
            if (!foodStateSprites.TryGetValue(foodId, out Sprite[] states))
            {
                if (!FoodCatalog.TryGet(foodId, out FoodDefinition definition)) return null;
                Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + definition.Id);
                if (atlas == null) return null;
                states = FoodSpriteLibrary.CreateStateSprites(definition, atlas);
                foodStateSprites.Add(definition.Id, states);
            }
            return states[Mathf.Clamp(stage, 0, states.Length - 1)];
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private void SetActionButtons(bool active)
        {
            if (flipButtonText != null) flipButtonText.text = "DAR VUELTA";
            if (active) UpdateGameplayActionButtons();
            else
            {
                if (flipButton != null) flipButton.interactable = false;
                if (plateButton != null) plateButton.interactable = false;
                if (serveButton != null) serveButton.interactable = false;
            }
        }

        private Image MakePanel(string objectName, Transform parent, Color color, float x, float y, float width, float height)
        {
            Image image = MakeImage(objectName, parent, roundedButtonSprite != null ? roundedButtonSprite : whiteSprite, color,
                new Vector2(x, y), new Vector2(x, y), new Vector2(width, height));
            if (roundedButtonSprite != null) image.type = Image.Type.Sliced;
            return image;
        }

        private Image MakeChildIcon(string objectName, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 size)
        {
            Image icon = MakeImage(objectName, parent, sprite, color, anchor, anchor, size);
            icon.preserveAspect = true;
            return icon;
        }

        private Image MakeImage(string objectName, Transform parent, Sprite sprite, Color color, Vector2 min, Vector2 max, Vector2 size)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = sprite != null ? sprite : whiteSprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text MakeText(string objectName, Transform parent, string value, int size, Color color, TextAnchor align, float x, float y, float width, float height, bool bold)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetRect(rect, x, y, width, height);
            var text = go.GetComponent<Text>();
            text.text = value;
            bool isBrand = objectName == "Menu marca";
            bool isButtonLabel = objectName.StartsWith("Texto ");
            bool isChunkyDisplayText = isBrand || isButtonLabel || (bold && size >= 27);
            Font preferredFont = isChunkyDisplayText ? displayFont
                : (bold ? (size >= 42 ? extraBoldFont : semiBoldFont) : (size <= 20 ? mediumFont : bodyFont));
            text.font = preferredFont != null ? preferredFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = preferredFont != null ? FontStyle.Normal : (bold ? FontStyle.Bold : FontStyle.Normal);
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            if (isChunkyDisplayText && !isBrand)
            {
                var outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(14, 14, 18, 245);
                outline.effectDistance = new Vector2(2.4f, -2.4f);
                var shadow = text.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, .78f);
                shadow.effectDistance = new Vector2(1f, -3f);
            }
            return text;
        }

        private Button MakeButton(string label, Transform parent, float x, float y, float width, float height, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(label, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), x, y, width, height);

            bool isBackAction = label == "SALIR" || label == "VOLVER" || label == "NIVELES";
            Image extrusion = MakeImage("Relieve inferior " + label, buttonObject.transform,
                arcadeButtonShapeSprite != null ? arcadeButtonShapeSprite : roundedButtonSprite,
                new Color32(80, 34, 13, 255), new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(width + 8f, height + 12f));
            extrusion.type = Image.Type.Sliced;
            extrusion.rectTransform.anchoredPosition = new Vector2(0f, -5f);

            Image border = MakeImage("Borde boton " + label, buttonObject.transform,
                arcadeButtonShapeSprite != null ? arcadeButtonShapeSprite : roundedButtonSprite,
                new Color32(48, 23, 19, 255), new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(width + 6f, height + 6f));
            border.type = Image.Type.Sliced;

            bool isDebugAction = label.StartsWith("DEBUG ");
            Image face = MakeImage("Cara boton " + label, buttonObject.transform,
                isBackAction ? arcadeCoralButtonSprite : (isDebugAction ? arcadeButtonShapeSprite : arcadeGoldButtonSprite),
                isDebugAction ? color : Color.white,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(width, height));
            face.type = Image.Type.Sliced;
            face.raycastTarget = true;

            Image topHighlight = MakeImage("Brillo boton " + label, buttonObject.transform, whiteSprite,
                new Color32(255, 255, 225, 200), new Vector2(.09f, .84f), new Vector2(.91f, .84f), new Vector2(0f, 4f));
            topHighlight.raycastTarget = false;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = face;
            ColorBlock colors = button.colors;
            Color restingTint = isDebugAction ? color : Color.white;
            colors.normalColor = restingTint;
            colors.highlightedColor = isDebugAction ? Color.Lerp(color, Color.white, .16f) : Color.white;
            colors.pressedColor = isDebugAction ? Color.Lerp(color, Color.black, .17f) : new Color(.84f, .82f, .76f, 1f);
            colors.disabledColor = new Color(.5f, .5f, .5f, .75f);
            button.colors = colors;
            AsaditoButtonFeedback buttonFeedback = buttonObject.AddComponent<AsaditoButtonFeedback>();
            buttonFeedback.PulseWhenInteractable = label == "SERVIR";
            button.onClick.AddListener(() => PlaySfx(AsaditoSfxCue.UiTap));
            button.onClick.AddListener(onClick);
            bool hasActionIcon = TryGetActionIcon(label, out AsaditoUiIcon actionIcon);
            if (hasActionIcon)
            {
                Image icon = MakeChildIcon("Icono accion " + label, buttonObject.transform, AsaditoUiIcons.Get(actionIcon), Color.white,
                    Vector2.zero, new Vector2(34f, 34f));
                icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0f, .5f);
                icon.rectTransform.anchoredPosition = new Vector2(43f, 0f);
                var iconOutline = icon.gameObject.AddComponent<Outline>();
                iconOutline.effectColor = new Color32(45, 29, 21, 255);
                iconOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }
            int labelSize = Mathf.Clamp(Mathf.RoundToInt(height * .42f), 28, 50);
            Text labelText = MakeText("Texto " + label, buttonObject.transform, label, labelSize, Color.white, TextAnchor.MiddleCenter, .5f, .5f, width - 24, height - 16, true);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = new Vector2(12f, 8);
            labelText.rectTransform.offsetMax = new Vector2(-12, -8);
            return button;
        }

        private static bool TryGetActionIcon(string label, out AsaditoUiIcon icon)
        {
            switch (label)
            {
                case "ENTRAR":
                case "IR A LA PARRILLA":
                case "SIGUIENTE":
                    icon = AsaditoUiIcon.Next;
                    return true;
                case "SALIR":
                    icon = AsaditoUiIcon.Exit;
                    return true;
                case "VOLVER":
                case "NIVELES":
                    icon = AsaditoUiIcon.Back;
                    return true;
                case "PRENDER CARBÓN":
                    icon = AsaditoUiIcon.Flame;
                    return true;
                case "DAR VUELTA":
                    icon = AsaditoUiIcon.Flip;
                    return true;
                case "SERVIR":
                case "BANDEJA":
                    icon = AsaditoUiIcon.Tray;
                    return true;
                case "REINTENTAR":
                    icon = AsaditoUiIcon.Retry;
                    return true;
                default:
                    icon = default;
                    return false;
            }
        }

        private static Sprite MakeRoundedRectSprite(int size, int radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Runtime Rounded Button" };
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float r = Mathf.Clamp(radius, 1, size / 2 - 1);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, r, size - 1 - r);
                float cy = Mathf.Clamp(y, r, size - 1 - r);
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(r + .75f - distance));
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f,
                0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        }

        private static Sprite MakeGradientButtonSprite(int size, int radius, Color topColor, Color bottomColor, string spriteName)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = spriteName };
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float r = Mathf.Clamp(radius, 1, size / 2 - 1);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, r, size - 1 - r);
                float cy = Mathf.Clamp(y, r, size - 1 - r);
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                Color fill = Color.Lerp(bottomColor, topColor, y / (float)(size - 1));
                fill.a = Mathf.Clamp01(r + .75f - distance);
                pixels[y * size + x] = fill;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f,
                0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            sprite.name = spriteName;
            return sprite;
        }

        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(x, y);
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static Sprite MakeCircleSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Runtime Circle";
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float center = (size - 1) * .5f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(radius + .5f - distance));
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }
    }
}
