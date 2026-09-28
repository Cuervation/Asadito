using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Asadito.Runtime;

namespace Asadito
{
    /// <summary>A compact, self-contained playable asado loop for the first MVP.</summary>
    public sealed class AsaditoGame : MonoBehaviour
    {
        [Min(0f)] public float SimulationTimeScale = 30f;
        [Range(100f, 300f)] public float FireTemperature = 210f;
        [SerializeField] private CharcoalGrillModel grill = new CharcoalGrillModel();
        private static readonly Color Ink = new Color32(37, 34, 28, 245);
        private static readonly Color Cream = new Color32(255, 239, 205, 255);
        private static readonly Color Gold = new Color32(245, 177, 72, 255);
        private static readonly Color Green = new Color32(67, 106, 73, 255);

        private sealed class Guest
        {
            public string name;
            public string meat;
            public string point;
            public Color shirt;
        }

        private readonly Guest[] guests =
        {
            new Guest { name = "ANA", meat = "TIRA", point = "JUGOSO", shirt = new Color32(190, 91, 67, 255) },
            new Guest { name = "TITO", meat = "CHORI", point = "A PUNTO", shirt = new Color32(71, 117, 127, 255) }
        };
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

        private readonly PlayablePortion[] portions = { new PlayablePortion("tira", .065f), new PlayablePortion("chorizo", .095f) };
        private int activePortion = -1, trayCount;

        private Canvas canvas;
        private RectTransform contentRoot;
        private GameObject menuRoot;
        private GameObject introRoot;
        private CanvasGroup menuCanvasGroup;
        private CanvasGroup introCanvasGroup;
        private CanvasGroup gameplayCanvasGroup;
        private bool menuTransitionActive;
        private Text tutorialText;
        private Image background;
        private Image tiraImage;
        private Image sausageImage;
        private Image cookFill;
        private Image fireGlow;
        private RectTransform heatGridRect;
        private RectTransform trayRect;
        private RectTransform tongsVisual;
        private readonly System.Collections.Generic.List<Image> heatCellImages = new System.Collections.Generic.List<Image>(48);
        private Text guestName;
        private Text guestOrder;
        private Text progressText;
        private Text feedbackText;
        private Text scoreText;
        private Text flipButtonText;
        private Button tiraButton;
        private Button choriButton;
        private Button flipButton;
        private Button serveButton;
        private Button igniteButton;
        private Button debugScaleButton;
        private GameObject avatar;
        private Sprite whiteSprite;
        private Sprite circleSprite;
        private Sprite tiraSprite;
        private Sprite chorizoSprite;
        private Sprite patioSprite;
        private Sprite titleSprite;
        private Sprite roundedButtonSprite;
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

        private void Start()
        {
            saveData = MvpSave.Load();
            SimulationTimeScale = saveData.Settings.SimulationTimeScale;
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f));
            circleSprite = MakeCircleSprite(128);
            roundedButtonSprite = MakeRoundedRectSprite(128, 34);
            displayFont = Resources.Load<Font>("Fonts/LilitaOne-Regular");
            bodyFont = Resources.Load<Font>("Fonts/Baloo2-Regular");
            mediumFont = Resources.Load<Font>("Fonts/Baloo2-Medium");
            semiBoldFont = Resources.Load<Font>("Fonts/Baloo2-SemiBold");
            boldFont = Resources.Load<Font>("Fonts/Baloo2-Bold");
            extraBoldFont = Resources.Load<Font>("Fonts/Baloo2-ExtraBold");
            tiraSprite = Resources.Load<Sprite>("Art/TiraAsadoCruda");
            patioSprite = Resources.Load<Sprite>("Art/PatioParrilla");
            Texture2D titleTexture = Resources.Load<Texture2D>("Art/PortadaAsadito");
            if (titleTexture != null)
                titleSprite = Sprite.Create(titleTexture, new Rect(0, 0, titleTexture.width, titleTexture.height), new Vector2(.5f, .5f), 100f);
            Texture2D chorizoTexture = Resources.Load<Texture2D>("Art/ChorizoCrudoCutout");
            if (chorizoTexture != null)
                chorizoSprite = Sprite.Create(chorizoTexture, new Rect(0, 0, chorizoTexture.width, chorizoTexture.height), new Vector2(.5f, .5f), 100f);
            BuildSizzleAudio();
            grill.HeatWhenLitC = FireTemperature;
            grill.Reset();
            BuildInterface();
            RefreshOrder();
        }

        private void Update()
        {
            if (servingLocked) return;
            float minutes = Time.deltaTime * SimulationTimeScale / 60f;
            grill.Step(minutes);
            RefreshHeatGridVisuals();
            for (int i = 0; i < portions.Length; i++)
            {
                PlayablePortion portion = portions[i];
                if (!portion.Started || portion.OnTray) continue;
                bool onGrill = portion.Position.x >= 0f && portion.Position.x <= 1f && portion.Position.y >= 0f && portion.Position.y <= 1f;
                float localHeat = grill.Sample(portion.Position, new Vector2(.14f, .14f)).x;
                FoodCookingModel.Step(portion.State, portion.Profile, localHeat, minutes, onGrill && grill.IsLit);
            }
            if (cooking) UpdateCookFeedback();
            else if (trayCount == 2)
                progressText.text = "TIRA " + portions[0].Point + "  •  CHORI " + portions[1].Point;
        }

        private void OnDestroy()
        {
            if (whiteSprite != null) Destroy(whiteSprite);
            if (circleSprite != null) Destroy(circleSprite);
            if (chorizoSprite != null) Destroy(chorizoSprite);
            if (titleSprite != null) Destroy(titleSprite);
            if (roundedButtonSprite != null) Destroy(roundedButtonSprite);
            if (sizzleClip != null) Destroy(sizzleClip);
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

            background = MakeImage("Patio ilustrado", canvas.transform, whiteSprite, Color.white, Vector2.zero, new Vector2(1, 1), Vector2.zero);
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;
            background.sprite = patioSprite != null ? patioSprite : whiteSprite;
            background.transform.SetAsFirstSibling();
            background.raycastTarget = false;
            background.rectTransform.localScale = patioSprite == null ? Vector3.one : new Vector3((patioSprite.rect.width / patioSprite.rect.height) / (1080f / 1920f), 1f, 1f);

            var contentObject = new GameObject("Safe Area", typeof(RectTransform));
            gameplayCanvasGroup = contentObject.AddComponent<CanvasGroup>();
            contentRoot = contentObject.GetComponent<RectTransform>();
            contentRoot.SetParent(canvas.transform, false);
            Rect safe = Screen.safeArea;
            contentRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            contentRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            contentRoot.offsetMin = contentRoot.offsetMax = Vector2.zero;

            MakePanel("Sombra del titulo", contentRoot, new Color(0, 0, 0, .38f), .5f, .945f, 950, 112);
            MakeText("Marca", contentRoot, "ASADITO", 44, Cream, TextAnchor.MiddleCenter, .5f, .945f, 920, 90, true);
            MakeText("Subtitulo", contentRoot, "TU PARRILLA, TU MOMENTO", 19, new Color32(250, 207, 141, 255), TextAnchor.MiddleCenter, .5f, .905f, 920, 42, true);

            MakePanel("Pedido", contentRoot, new Color32(38, 48, 39, 238), .5f, .795f, 930, 166);
            avatar = new GameObject("Comensal", typeof(RectTransform));
            var avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.SetParent(contentRoot, false);
            SetRect(avatarRect, .17f, .795f, 110, 110);
            guestName = MakeText("Nombre comensal", contentRoot, "", 30, Cream, TextAnchor.MiddleLeft, .49f, .818f, 490, 50, true);
            guestOrder = MakeText("Pedido de carne", contentRoot, "", 25, new Color32(243, 197, 121, 255), TextAnchor.MiddleLeft, .49f, .773f, 490, 46, true);

            fireGlow = MakeImage("Resplandor de brasas", contentRoot, circleSprite, new Color(1f, .35f, .07f, .16f), new Vector2(.5f, .57f), new Vector2(.5f, .57f), new Vector2(330, 170));
            fireGlow.raycastTarget = false;
            BuildHeatGrid();
            Image tray = MakeImage("Bandeja de madera", contentRoot, circleSprite, new Color32(105, 63, 43, 255), new Vector2(.5f, .468f), new Vector2(.5f, .468f), new Vector2(344, 112));
            trayRect = tray.rectTransform;
            MakeImage("Plato de servir", contentRoot, circleSprite, new Color32(238, 222, 187, 255), new Vector2(.5f, .468f), new Vector2(.5f, .468f), new Vector2(286, 78));
            MakePanel("Indicador coccion", contentRoot, new Color32(39, 34, 28, 230), .5f, .405f, 890, 132);
            progressText = MakeText("Estado coccion", contentRoot, "ELEGÍ UN CORTE PARA EMPEZAR", 26, Cream, TextAnchor.MiddleCenter, .5f, .432f, 820, 52, true);
            MakePanel("Barra base", contentRoot, new Color32(91, 70, 51, 255), .5f, .385f, 760, 18);
            cookFill = MakeImage("Barra progreso", contentRoot, whiteSprite, Gold, new Vector2(.5f, .385f), new Vector2(.5f, .385f), new Vector2(0, 18));
            cookFill.rectTransform.pivot = new Vector2(0, .5f);
            cookFill.rectTransform.anchorMin = cookFill.rectTransform.anchorMax = new Vector2(.12f, .385f);
            cookFill.rectTransform.anchoredPosition = new Vector2(0, 0);
            feedbackText = MakeText("Feedback", contentRoot, "", 25, Cream, TextAnchor.MiddleCenter, .5f, .342f, 850, 60, true);
            tutorialText = MakeText("Tutorial contextual", contentRoot, "", 19, new Color32(255, 213, 146, 255), TextAnchor.MiddleCenter, .5f, .70f, 830, 58, true);

            tiraImage = MakeImage("Tira de asado en parrilla", contentRoot, tiraSprite != null ? tiraSprite : whiteSprite, Color.white, new Vector2(.5f, .585f), new Vector2(.5f, .585f), new Vector2(238, 156));
            tiraImage.gameObject.AddComponent<CanvasGroup>();
            AttachFoodTouch(tiraImage, 0);
            tiraImage.gameObject.SetActive(false);
            sausageImage = MakeSausage(new Vector2(.5f, .585f));
            sausageImage.gameObject.AddComponent<CanvasGroup>();
            AttachFoodTouch(sausageImage, 1);
            sausageImage.gameObject.SetActive(false);
            BuildTongsVisual();

            tiraButton = MakeButton("TIRA DE ASADO", contentRoot, .28f, .245f, 425, 116, new Color32(114, 63, 46, 255), CookTira);
            choriButton = MakeButton("CHORIZO", contentRoot, .72f, .245f, 425, 116, new Color32(156, 79, 48, 255), CookChori);
            flipButton = MakeButton("DAR VUELTA", contentRoot, .28f, .145f, 425, 104, Green, FlipMeat);
            serveButton = MakeButton("SERVIR", contentRoot, .72f, .145f, 425, 104, new Color32(199, 139, 54, 255), Serve);
            flipButtonText = flipButton.GetComponentInChildren<Text>();
            scoreText = MakeText("Progreso nivel", contentRoot, "NIVEL 1  •  2 COMENSALES", 22, Cream, TextAnchor.MiddleCenter, .5f, .055f, 960, 54, true);
            debugScaleButton = MakeButton("DEBUG ×30", contentRoot, .14f, .855f, 190, 58, new Color32(58, 65, 56, 230), CycleSimulationScale);
            MakeText("Ayuda", contentRoot, "Cociná, retirá cada pieza y serví la bandeja.", 18, new Color32(255, 224, 177, 255), TextAnchor.MiddleCenter, .5f, .022f, 960, 38, false);
            igniteButton = MakeButton("PRENDER CARBÓN", contentRoot, .5f, .315f, 620, 86, new Color32(132, 64, 39, 255), IgniteCharcoal);
            SetActionButtons(false);
            tiraButton.interactable = choriButton.interactable = false;
            StartCoroutine(AnimateEmbers());
            BuildFrontEnd();
            RefreshDebugScaleLabel();
        }

        private void BuildTongsVisual()
        {
            var root = new GameObject("Pinza de parrilla", typeof(RectTransform));
            root.transform.SetParent(contentRoot, false);
            tongsVisual = root.GetComponent<RectTransform>();
            tongsVisual.anchorMin = tongsVisual.anchorMax = new Vector2(.5f, .5f);
            tongsVisual.sizeDelta = new Vector2(150f, 90f);
            Image handle = MakeImage("Mango de pinza", root.transform, whiteSprite, new Color32(93, 65, 45, 255), new Vector2(.2f, .42f), new Vector2(.82f, .58f), new Vector2(115f, 10f));
            handle.rectTransform.localRotation = Quaternion.Euler(0, 0, -12f);
            for (int i = 0; i < 2; i++)
            {
                Image arm = MakeImage("Brazo metal " + i, root.transform, whiteSprite, new Color32(190, 185, 165, 255), new Vector2(i == 0 ? .72f : .72f, i == 0 ? .63f : .38f), new Vector2(.98f, i == 0 ? .88f : .62f), new Vector2(56f, 5f));
                arm.rectTransform.localRotation = Quaternion.Euler(0, 0, i == 0 ? 16f : -16f);
            }
            tongsVisual.gameObject.SetActive(false);
        }

        private void BuildSizzleAudio()
        {
            var audioObject = new GameObject("Sizzle loop");
            audioObject.transform.SetParent(transform, false);
            sizzleSource = audioObject.AddComponent<AudioSource>();
            sizzleSource.playOnAwake = false;
            sizzleSource.loop = true;
            sizzleSource.spatialBlend = 0f;
            sizzleSource.volume = saveData != null ? saveData.Settings.SfxVolume * .22f : .18f;
            const int sampleRate = 22050;
            var samples = new float[sampleRate];
            var random = new System.Random(1729);
            float filtered = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered = Mathf.Lerp(filtered, noise, .48f);
                float pulse = .55f + .45f * Mathf.Abs(Mathf.Sin(i * .00071f) * Mathf.Sin(i * .00019f));
                samples[i] = filtered * pulse * .24f;
            }
            sizzleClip = AudioClip.Create("Asadito Sizzle", sampleRate, 1, sampleRate, false);
            sizzleClip.SetData(samples, 0);
            sizzleSource.clip = sizzleClip;
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
            Text brand = MakeText("Menu marca", menuRoot.transform, "ASADITO", 104, Cream, TextAnchor.MiddleCenter, .5f, .825f, 940, 155, true);
            var brandOutline = brand.gameObject.AddComponent<Outline>();
            brandOutline.effectColor = new Color32(79, 39, 29, 245);
            brandOutline.effectDistance = new Vector2(3, -3);
            var brandShadow = brand.gameObject.AddComponent<Shadow>();
            brandShadow.effectColor = new Color(0, 0, 0, .45f);
            brandShadow.effectDistance = new Vector2(1, -7);
            MakeText("Menu subtitulo", menuRoot.transform, "EL SABOR DEL PATIO ARGENTINO", 25, Cream, TextAnchor.MiddleCenter, .5f, .755f, 900, 74, true);
            MakeButton("ENTRAR", menuRoot.transform, .5f, .245f, 560, 118, new Color32(218, 121, 45, 255), ShowLevelIntro);
            MakeButton("SALIR", menuRoot.transform, .5f, .158f, 420, 92, new Color32(92, 72, 55, 245), ExitGame);
            Image emberGlow = MakeImage("Resplandor ambiental portada", menuRoot.transform, circleSprite,
                new Color(1f, .29f, .07f, .055f), new Vector2(.5f, .48f), new Vector2(.5f, .48f), new Vector2(650, 300));
            emberGlow.transform.SetSiblingIndex(2);
            emberGlow.raycastTarget = false;
            StartCoroutine(AnimateTitleGlow(emberGlow));

            introRoot = CreateFullScreenOverlay("Intro de nivel");
            introCanvasGroup = introRoot.AddComponent<CanvasGroup>();
            introCanvasGroup.alpha = 0f;
            introRoot.SetActive(false);
            MakePanel("Sombra intro", introRoot.transform, new Color32(31, 34, 29, 245), .5f, .5f, 1080, 1920);
            MakeText("Intro título", introRoot.transform, "EL DEBUT", 64, Cream, TextAnchor.MiddleCenter, .5f, .66f, 900, 115, true);
            MakeText("Intro comensales", introRoot.transform, "2 COMENSALES", 31, Gold, TextAnchor.MiddleCenter, .5f, .56f, 850, 64, true);
            MakeText("Intro menu", introRoot.transform, "CHORIZO ×1   ·   TIRA DE ASADO ×1", 27, Cream, TextAnchor.MiddleCenter, .5f, .505f, 880, 72, true);
            MakeText("Intro objetivo", introRoot.transform, "Prendé el carbón, repartí las brasas y cociná.\nRetirá todo a punto y serví la bandeja.", 25, Cream, TextAnchor.MiddleCenter, .5f, .445f, 850, 118, false);
            MakeButton("IR A LA PARRILLA", introRoot.transform, .5f, .35f, 550, 115, new Color32(199, 139, 54, 255), StartLevel);
            contentRoot.gameObject.SetActive(false);
        }

        private GameObject CreateFullScreenOverlay(string objectName)
        {
            var root = new GameObject(objectName, typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return root;
        }

        private void ShowLevelIntro()
        {
            if (!menuTransitionActive) StartCoroutine(TransitionToIntro());
        }

        private IEnumerator TransitionToIntro()
        {
            menuTransitionActive = true;
            foreach (Button button in menuRoot.GetComponentsInChildren<Button>()) button.interactable = false;
            introRoot.SetActive(true);
            float t = 0f;
            while (t < .38f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .38f));
                menuCanvasGroup.alpha = 1f - p;
                introCanvasGroup.alpha = p;
                yield return null;
            }
            menuRoot.SetActive(false);
            menuCanvasGroup.alpha = 1f;
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
            contentRoot.gameObject.SetActive(true);
            gameplayCanvasGroup.alpha = 0f;
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
            menuTransitionActive = false;
            tutorialStep = 0;
            tutorialText.text = saveData.Settings.TutorialCompleted ? "El carbón responde a dónde agrupás las brasas." : "Paso 1: prendé el carbón para empezar.";
        }

        private void BuildHeatGrid()
        {
            Image root = MakeImage("Mapa de calor carbón 8x6", contentRoot, whiteSprite, new Color(0, 0, 0, 0), new Vector2(.5f, .575f), new Vector2(.5f, .575f), new Vector2(820, 276));
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

        private void AttachFoodTouch(Image image, int index)
        {
            FoodPieceTouch touch = image.gameObject.AddComponent<FoodPieceTouch>();
            touch.Owner = this;
            touch.PortionIndex = index;
            image.raycastTarget = true;
        }

        private void IgniteCharcoal()
        {
            if (servingLocked || grill.IsLit) return;
            grill.HeatWhenLitC = FireTemperature;
            grill.Ignite();
            igniteButton.interactable = false;
            igniteButton.GetComponentInChildren<Text>().text = "CARBÓN ENCENDIDO";
            tiraButton.interactable = choriButton.interactable = true;
            feedbackText.text = "Las brasas están listas. Arrastrá las celdas para repartir el calor.";
            AdvanceTutorial(1, "Paso 2: arrastrá brasas para cambiar el calor por zona.");
            RefreshHeatGridVisuals();
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
                RefreshHeatGridVisuals();
                AdvanceTutorial(2, "Paso 3: elegí el chorizo o la tira.");
            }
        }

        public void BeginFoodDrag(int index)
        {
            if (servingLocked || portions[index].OnTray) return;
            if (!portions[index].Started) ChoosePortion(index);
            else { activePortion = index; cooking = true; }
        }

        public void DragFood(int index, Vector2 screenPosition)
        {
            if (servingLocked || !portions[index].Started || portions[index].OnTray || heatGridRect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(heatGridRect, screenPosition, null, out Vector2 local)) return;
            Vector2 size = heatGridRect.rect.size;
            Vector2 normalized = new Vector2(local.x / size.x + .5f, local.y / size.y + .5f);
            portions[index].Position = normalized;
            PortionImage(index).rectTransform.position = heatGridRect.TransformPoint(local);
            if (tongsVisual != null)
            {
                tongsVisual.position = screenPosition + new Vector2(38f, 45f);
                tongsVisual.gameObject.SetActive(true);
                tongsVisual.SetAsLastSibling();
            }
            activePortion = index;
            cooking = true;
            AdvanceTutorial(3, "Mové la pieza por la parrilla y observá el calor de cada zona.");
        }

        public void EndFoodDrag(int index, Vector2 screenPosition)
        {
            if (servingLocked || !portions[index].Started || portions[index].OnTray) return;
            if (trayRect != null && RectTransformUtility.RectangleContainsScreenPoint(trayRect, screenPosition, null))
            {
                if (tongsVisual != null) tongsVisual.gameObject.SetActive(false);
                StartCoroutine(PlatePortion(index));
                return;
            }

            Vector2 position = portions[index].Position;
            position = new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
            portions[index].Position = position;
            Vector2 localPosition = new Vector2((position.x - .5f) * heatGridRect.rect.width, (position.y - .5f) * heatGridRect.rect.height);
            PortionImage(index).rectTransform.position = heatGridRect.TransformPoint(localPosition);
            cooking = true;
            activePortion = index;
            if (tongsVisual != null) tongsVisual.gameObject.SetActive(false);
        }

        private void AdvanceTutorial(int step, string message)
        {
            if (tutorialText == null || saveData == null || saveData.Settings.TutorialCompleted) return;
            if (step < tutorialStep) return;
            tutorialStep = step;
            tutorialText.text = message;
        }

        private void RefreshHeatGridVisuals()
        {
            if (heatCellImages.Count != 48) return;
            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 8; x++)
            {
                HeatCell cell = grill.Grid.GetCell(x, y);
                float ember = Mathf.Clamp01(cell.EmberEnergy);
                heatCellImages[y * 8 + x].color = grill.IsLit
                    ? Color.Lerp(new Color32(77, 57, 44, 215), new Color32(247, 93, 31, 245), ember)
                    : (Color)new Color32(64, 58, 49, 200);
            }
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
            guestName.text = "ANA + TITO";
            guestOrder.text = "TIRA JUGOSA  +  CHORIZO A PUNTO";
            scoreText.text = "NIVEL 1  •  BANDEJA 0 / 2  •  2 COMENSALES";
            DrawAvatar(guests[0].shirt);
            servingLocked = cooking = false;
            activePortion = -1;
            trayCount = totalScore = 0;
            for (int i = 0; i < portions.Length; i++) portions[i].Reset();
            ResetMeat(tiraImage);
            ResetMeat(sausageImage);
            cookFill.rectTransform.sizeDelta = new Vector2(0, 18);
            progressText.text = "ELEGÍ UNA PIEZA PARA EMPEZAR";
            feedbackText.text = "Retirá ambas piezas antes de servir la bandeja.";
            tiraButton.GetComponentInChildren<Text>().text = "TIRA DE ASADO";
            choriButton.GetComponentInChildren<Text>().text = "CHORIZO";
            SetActionButtons(false);
            tiraButton.interactable = choriButton.interactable = false;
            for (int i = 0; i < portions.Length; i++) portions[i].Position = new Vector2(.5f, .5f);
            SetActionButtons(false);
        }

        private void ResetMeat(Image meat)
        {
            meat.gameObject.SetActive(false);
            meat.rectTransform.anchorMin = meat.rectTransform.anchorMax = new Vector2(.5f, .585f);
            meat.rectTransform.anchoredPosition = Vector2.zero;
            meat.rectTransform.localScale = Vector3.one;
            meat.GetComponent<CanvasGroup>().alpha = 1f;
        }

        private void CookTira() => ChoosePortion(0);
        private void CookChori() => ChoosePortion(1);
        private Image PortionImage(int index) => index == 0 ? tiraImage : sausageImage;

        private void ChoosePortion(int index)
        {
            if (servingLocked || portions[index].OnTray || !grill.IsLit) return;
            if (activePortion == index) { StartCoroutine(PlatePortion(index)); return; }
            if (cooking) return;
            activePortion = index;
            cooking = portions[index].Started = true;
            portions[index].Position = new Vector2(.5f, .5f);
            AdvanceTutorial(3, "Paso 4: arrastrá la pieza a una zona de calor.");
            Image meat = PortionImage(index);
            meat.gameObject.SetActive(true);
            meat.color = index == 0 ? Color.white : (Color)new Color32(181, 74, 44, 255);
            meat.rectTransform.position = heatGridRect.TransformPoint(Vector3.zero);
            StartCoroutine(PlaceMeat(meat.rectTransform));
            if (sizzleSource != null && !sizzleSource.isPlaying) sizzleSource.Play();
            (index == 0 ? tiraButton : choriButton).GetComponentInChildren<Text>().text = "RETIRAR A BANDEJA";
            (index == 0 ? choriButton : tiraButton).interactable = false;
            flipButton.interactable = true;
            feedbackText.text = "Dale vuelta y retirá al punto pedido.";
            smokeRoutine = StartCoroutine(SmokePuffs());
        }

        private IEnumerator PlatePortion(int index)
        {
            cooking = false;
            activePortion = -1;
            portions[index].OnTray = true;
            VibrateFeedback();
            tiraButton.interactable = choriButton.interactable = false;
            flipButton.interactable = false;
            if (smokeRoutine != null) StopCoroutine(smokeRoutine);
            if (sizzleSource != null) sizzleSource.Stop();
            yield return ServeAnimation(PortionImage(index));
            trayCount++;
            (index == 0 ? tiraButton : choriButton).GetComponentInChildren<Text>().text = "EN BANDEJA  ✓";
            tiraButton.interactable = !portions[0].OnTray;
            choriButton.interactable = !portions[1].OnTray;
            serveButton.interactable = trayCount == 2;
            scoreText.text = "NIVEL 1  •  BANDEJA " + trayCount + " / 2  •  2 COMENSALES";
            progressText.text = trayCount == 2 ? "BANDEJA LISTA PARA SERVIR" : "ELEGÍ LA OTRA PIEZA";
            feedbackText.text = "Las piezas conservan calor mientras reposan.";
            AdvanceTutorial(5, "Repetí con la otra porción y serví la bandeja.");
        }

        private void FlipMeat()
        {
            if (!cooking || servingLocked) return;
            portions[activePortion].Flip();
            StartCoroutine(FlipAnimation(PortionImage(activePortion).rectTransform));
            RefreshFoodVisual(activePortion);
            AdvanceTutorial(4, "Paso 5: retirala cuando alcance el punto pedido.");
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
            float t = 0f;
            while (t < .3f)
            {
                t += Time.deltaTime;
                float scale = Mathf.Abs(Mathf.Cos(t / .3f * Mathf.PI));
                target.localScale = new Vector3(Mathf.Max(.05f, scale), start.y, start.z);
                yield return null;
            }
            target.localScale = start;
            target.localRotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
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

        private IEnumerator ServeAnimation(Image meat)
        {
            RectTransform rect = meat.rectTransform;
            Vector2 start = rect.anchorMin;
            Vector2 destination = new Vector2(meat == tiraImage ? .43f : .57f, .468f);
            float t = 0f;
            while (t < .42f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / .42f);
                rect.anchorMin = rect.anchorMax = Vector2.Lerp(start, destination, p);
                rect.localScale = Vector3.one * (1f - .42f * p);
                yield return null;
            }
            rect.anchorMin = rect.anchorMax = destination;
            rect.localScale = Vector3.one * .58f;
        }

        private void Serve()
        {
            if (servingLocked || cooking || trayCount != 2) return;
            servingLocked = true;
            SetActionButtons(false);
            GuestProfile[] profiles = BuildGuestProfiles();
            var servings = new System.Collections.Generic.List<ServingPortion>(2);
            for (int i = 0; i < portions.Length; i++)
                servings.Add(new ServingPortion { Id = i == 0 ? "tira-1" : "chorizo-1", FoodId = i == 0 ? "tira" : "chorizo", Amount = portions[i].Amount,
                    Doneness = FoodCookingModel.GetDoneness(portions[i].State, portions[i].Profile), CookingQuality = GetCookingQuality(portions[i]) });
            var assignments = ServingAllocator.Allocate(profiles, servings);
            string reactions = "";
            totalScore = 0;
            ScoreConfig scoreConfig = new ScoreConfig();
            for (int i = 0; i < profiles.Length; i++)
            {
                int portionIndex = FindAssignedPortion(assignments, i == 0 ? "ana" : "tito");
                if (portionIndex < 0) { reactions += profiles[i].Name + ": sin porción\n"; continue; }
                PlayablePortion portion = portions[portionIndex];
                float assignedAmount = Mathf.Min(portion.Amount, profiles[i].TargetFoodAmount);
                ScoreBreakdown breakdown = new ScoreBreakdown
                {
                    CookingQuality = GetCookingQuality(portion),
                    Satiety = profiles[i].TargetFoodAmount <= 0f ? 100f : Mathf.Clamp01(assignedAmount / profiles[i].TargetFoodAmount) * 100f,
                    DonenessMatch = FoodCookingModel.EvaluateDonenessMatch(portion.State, portion.Profile, profiles[i].PreferredDoneness),
                    FoodPreference = ServingAllocator.GetFoodPreference(profiles[i], servings[portionIndex].FoodId) * 25f
                };
                int points = Mathf.RoundToInt(breakdown.Total(scoreConfig));
                totalScore += points;
                reactions += profiles[i].Name + ": cocción " + Mathf.RoundToInt(breakdown.CookingQuality) +
                    " · saciedad " + Mathf.RoundToInt(breakdown.Satiety) + " · punto " + Mathf.RoundToInt(breakdown.DonenessMatch) +
                    " · gusto " + Mathf.RoundToInt(breakdown.FoodPreference) + " = " + points + "/100\n";
            }
            reactions += "Total: " + totalScore + " / 200";
            totalStars = starThresholds.Evaluate(totalScore);
            if (saveData != null && saveData.Settings != null && !saveData.Settings.TutorialCompleted)
            {
                saveData.Settings.TutorialCompleted = true;
                MvpSave.Save(saveData);
            }
            MvpSave.RecordLevelResult(1, totalScore, totalStars);
            saveData = MvpSave.Load();
            feedbackText.text = reactions;
            StartCoroutine(GuestReaction(totalScore / guests.Length));
        }

        private GuestProfile[] BuildGuestProfiles()
        {
            var tuning = new GuestAmountTuning { AdultAgeBase = .2f };
            var ana = new GuestProfile { Id = "ana", Name = "ANA", Age = 30, Weight = 62f, Appetite = .35f, PreferredDoneness = Doneness.Jugoso };
            ana.FavoriteFoods.Add("tira");
            ana.CalculateTarget(tuning);
            var tito = new GuestProfile { Id = "tito", Name = "TITO", Age = 40, Weight = 82f, Appetite = .35f, PreferredDoneness = Doneness.A_Punto };
            tito.FavoriteFoods.Add("chorizo");
            tito.CalculateTarget(tuning);
            return new[] { ana, tito };
        }

        private int FindAssignedPortion(System.Collections.Generic.List<ServingAssignment> assignments, string guestId)
        {
            for (int i = 0; i < assignments.Count; i++)
                if (assignments[i].GuestId == guestId) return assignments[i].PortionId == "tira-1" ? 0 : 1;
            return -1;
        }

        private static float GetCookingQuality(PlayablePortion portion)
        {
            return Mathf.Clamp(100f - portion.State.Char * 65f - (1f - portion.State.Moisture) * 30f + portion.State.Maillard * 12f, 0f, 100f);
        }

        private IEnumerator GuestReaction(int points)
        {
            RectTransform face = avatar.GetComponent<RectTransform>();
            Vector3 original = face.localScale;
            float t = 0;
            while (t < .38f)
            {
                t += Time.deltaTime;
                face.localScale = original * (1f + Mathf.Sin(t / .38f * Mathf.PI) * (points >= 65 ? .22f : .1f));
                yield return null;
            }
            face.localScale = original;
            yield return new WaitForSeconds(1.25f);
            ShowFinalScore();
        }

        private void ShowFinalScore()
        {
            MakePanel("Fin de nivel", contentRoot, new Color32(39, 48, 39, 248), .5f, .54f, 900, 420);
            MakeText("Resultado titulo", contentRoot, "¡ASADO TERMINADO!", 46, Cream, TextAnchor.MiddleCenter, .5f, .625f, 850, 90, true);
            scoreText = MakeText("Resultado puntos", contentRoot, "0 / 200 PUNTOS", 43, Gold, TextAnchor.MiddleCenter, .5f, .555f, 900, 82, true);
            StartCoroutine(AnimateResultScore(scoreText));
            MakeText("Resultado detalle", contentRoot, feedbackText.text, 18, Cream, TextAnchor.MiddleCenter, .5f, .49f, 850, 115, false);
            Button replay = MakeButton("OTRO ASADO", contentRoot, .5f, .405f, 470, 100, new Color32(199, 139, 54, 255), Replay);
            replay.interactable = true;
            SetActionButtons(false);
        }

        private IEnumerator AnimateResultScore(Text label)
        {
            float elapsed = 0f;
            label.rectTransform.localScale = Vector3.one * .82f;
            while (elapsed < .58f)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / .58f);
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                int points = Mathf.RoundToInt(totalScore * eased);
                label.text = points + " / 200 PUNTOS";
                label.rectTransform.localScale = Vector3.one * Mathf.Lerp(.82f, 1f, eased);
                yield return null;
            }
            label.text = totalScore + " / 200 PUNTOS  ·  " + new string('★', totalStars) + new string('☆', 3 - totalStars);
            label.rectTransform.localScale = Vector3.one;
        }

        private void Replay()
        {
            totalScore = 0;
            Transform result = contentRoot.Find("Fin de nivel");
            if (result != null) Destroy(result.gameObject);
            RemoveNamed("Resultado titulo");
            RemoveNamed("Resultado puntos");
            RemoveNamed("Resultado detalle");
            RemoveNamed("OTRO ASADO");
            RefreshOrder();
        }

        private void RemoveNamed(string objectName)
        {
            Transform target = contentRoot.Find(objectName);
            if (target != null) Destroy(target.gameObject);
        }

        private void UpdateCookFeedback()
        {
            PlayablePortion portion = portions[activePortion];
            cookFill.rectTransform.sizeDelta = new Vector2(760f * Mathf.Clamp01(portion.State.CoreTemperatureC / 80f), 18f);
            progressText.text = Mathf.RoundToInt(portion.State.CoreTemperatureC) + " °C  •  " + portion.Point + "  •  zona " + Mathf.RoundToInt(grill.Sample(portion.Position, new Vector2(.14f, .14f)).x) + " °C";
            RefreshFoodVisual(activePortion);
        }

        private void RefreshFoodVisual(int index)
        {
            PlayablePortion portion = portions[index];
            float charAmount = Mathf.Clamp01(portion.State.CurrentFace.Char);
            float browning = Mathf.Clamp01(portion.State.CurrentFace.Maillard);
            Color raw = index == 1 && chorizoSprite == null ? (Color)new Color32(181, 74, 44, 255) : Color.white;
            Color browned = index == 0 ? new Color32(151, 83, 53, 255) : new Color32(116, 55, 36, 255);
            Color burnt = new Color32(46, 36, 31, 255);
            PortionImage(index).color = Color.Lerp(Color.Lerp(raw, browned, browning), burnt, charAmount);
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
                var puff = MakeImage("Humo", contentRoot, circleSprite, new Color(.94f, .92f, .86f, .34f), new Vector2(.43f + Random.Range(0, .14f), .65f), new Vector2(.43f + Random.Range(0, .14f), .65f), new Vector2(Random.Range(34, 68), Random.Range(34, 68)));
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

        private void DrawAvatar(Color shirt)
        {
            foreach (Transform child in avatar.transform) Destroy(child.gameObject);
            Image body = MakeImage("Cuerpo", avatar.transform, whiteSprite, shirt, new Vector2(.5f, .08f), new Vector2(.5f, .08f), new Vector2(96, 48));
            body.raycastTarget = false;
            Image face = MakeImage("Cara", avatar.transform, circleSprite, new Color32(223, 166, 117, 255), new Vector2(.5f, .57f), new Vector2(.5f, .57f), new Vector2(67, 67));
            face.raycastTarget = false;
            Image hair = MakeImage("Pelo", avatar.transform, circleSprite, new Color32(66, 47, 37, 255), new Vector2(.5f, .78f), new Vector2(.5f, .78f), new Vector2(71, 39));
            hair.raycastTarget = false;
            MakeText("Sonrisa", avatar.transform, "•‿•", 21, Ink, TextAnchor.MiddleCenter, .5f, .46f, 66, 26, true);
        }

        private Image MakeSausage(Vector2 anchor)
        {
            Image sausage = MakeImage("Chorizo en parrilla", contentRoot, chorizoSprite != null ? chorizoSprite : whiteSprite,
                chorizoSprite != null ? Color.white : new Color32(181, 74, 44, 255), anchor, anchor, new Vector2(300, 150));
            sausage.preserveAspect = chorizoSprite != null;
            sausage.rectTransform.localRotation = Quaternion.Euler(0, 0, -5);
            if (chorizoSprite != null) return sausage;
            var shine = MakeImage("Brillo del chori", sausage.transform, whiteSprite, new Color32(255, 172, 104, 255), new Vector2(.5f, .68f), new Vector2(.5f, .68f), new Vector2(184, 7));
            shine.raycastTarget = false;
            for (int i = -1; i <= 1; i++)
            {
                var mark = MakeImage("Marca de parrilla", sausage.transform, whiteSprite, new Color32(88, 44, 34, 255), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(5, 58));
                mark.rectTransform.anchoredPosition = new Vector2(i * 58f, 0);
                mark.rectTransform.localRotation = Quaternion.Euler(0, 0, 19);
                mark.raycastTarget = false;
            }
            return sausage;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private void SetActionButtons(bool active)
        {
            if (flipButton == null) return;
            flipButton.interactable = active;
            serveButton.interactable = active;
            if (flipButtonText != null) flipButtonText.text = "DAR VUELTA";
        }

        private Image MakePanel(string objectName, Transform parent, Color color, float x, float y, float width, float height)
        {
            Image image = MakeImage(objectName, parent, roundedButtonSprite != null ? roundedButtonSprite : whiteSprite, color,
                new Vector2(x, y), new Vector2(x, y), new Vector2(width, height));
            if (roundedButtonSprite != null) image.type = Image.Type.Sliced;
            return image;
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
            Font preferredFont = isBrand ? displayFont
                : (isButtonLabel ? boldFont : (bold ? (size >= 42 ? extraBoldFont : semiBoldFont) : (size <= 20 ? mediumFont : bodyFont)));
            text.font = preferredFont != null ? preferredFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = preferredFont != null ? FontStyle.Normal : (bold ? FontStyle.Bold : FontStyle.Normal);
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private Button MakeButton(string label, Transform parent, float x, float y, float width, float height, Color color, UnityEngine.Events.UnityAction onClick)
        {
            Image image = MakeImage(label, parent, whiteSprite, color, new Vector2(x, y), new Vector2(x, y), new Vector2(width, height));
            if (roundedButtonSprite != null)
            {
                image.sprite = roundedButtonSprite;
                image.type = Image.Type.Sliced;
            }
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, .16f);
            colors.pressedColor = Color.Lerp(color, Color.black, .17f);
            colors.disabledColor = new Color(color.r * .45f, color.g * .45f, color.b * .45f, .75f);
            button.colors = colors;
            var shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, .35f);
            shadow.effectDistance = new Vector2(0, -5);
            AsaditoButtonFeedback buttonFeedback = image.gameObject.AddComponent<AsaditoButtonFeedback>();
            buttonFeedback.PulseWhenInteractable = label == "SERVIR";
            button.onClick.AddListener(onClick);
            Text labelText = MakeText("Texto " + label, image.transform, label, 27, Cream, TextAnchor.MiddleCenter, .5f, .5f, width - 24, height - 16, true);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = new Vector2(12, 8);
            labelText.rectTransform.offsetMax = new Vector2(-12, -8);
            return button;
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
