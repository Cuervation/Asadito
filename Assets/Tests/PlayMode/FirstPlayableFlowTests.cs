using System;
using System.Collections;
using System.Reflection;
using Asadito.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Asadito.Tests.PlayMode
{
    /// <summary>Exercises the shipped Level 1 UI loop in the real Unity player loop.</summary>
    public sealed class FirstPlayableFlowTests
    {
        private const string SaveKey = "asadito.mvp.save";
        private bool hadSave;
        private string originalSave;

        [UnitySetUp]
        public IEnumerator CaptureLocalProgress()
        {
            hadSave = PlayerPrefs.HasKey(SaveKey);
            originalSave = PlayerPrefs.GetString(SaveKey, string.Empty);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator RestoreLocalProgress()
        {
            if (hadSave) PlayerPrefs.SetString(SaveKey, originalSave);
            else PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            ResetSaveCache();
            yield return null;
        }

        [UnityTest]
        public IEnumerator FirstPlayable_CookServePersistsResultAndNextOpensLevelTwo()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game, "SampleScene must start the Asadito runtime controller.");
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 1200f); // Accelerate simulation only for deterministic test duration.

            yield return EnterLevelOne(game);
            IgniteAndMoveEmbers(game);

            yield return CookAndPlate(game, 0, "TIRA DE ASADO", 25f, true);
            yield return CookAndPlate(game, 1, "CHORIZO", 25f, true);

            Button serve = FindButton("SERVIR");
            Assert.IsTrue(serve.interactable, "Serve must unlock after every portion reaches the tray.");
            ClickButton(serve);
            yield return new WaitForSecondsRealtime(1.8f);

            Assert.NotNull(GameObject.Find("Fin de nivel"), "Serving must reach the result screen.");
            Text resultScore = GameObject.Find("Resultado puntos").GetComponent<Text>();
            Assert.That(resultScore.text, Does.Contain("PUNTOS"));
            Assert.IsNotNull(GameObject.Find("REINTENTAR"));
            Button next = FindButton("SIGUIENTE");
            Assert.IsTrue(next.interactable, "A one-star result must unlock the next level.");
            Assert.GreaterOrEqual(ReadSaveInt("MaxUnlockedLevel"), 2);
            MvpSaveData saved = MvpSave.Load();
            Assert.AreEqual(MvpSaveData.CurrentVersion, saved.Version);
            Assert.Greater(saved.BestScoreByLevel[0], 0, "The result score must persist for level 1.");
            Assert.LessOrEqual(saved.BestScoreByLevel[0], 200, "A two-guest level is capped at 200 points.");
            Assert.GreaterOrEqual(saved.StarsByLevel[0], 1, "Passing level 1 must persist at least one star.");
            Assert.IsTrue(saved.Settings.TutorialCompleted, "Completing the in-game tutorial must persist its completion.");

            ClickButton(next);
            yield return new WaitForSecondsRealtime(.55f);
            Text intro = GameObject.Find("Intro título").GetComponent<Text>();
            Assert.That(intro.text, Does.Contain("NIVEL 2"));
            ClickButton("IR A LA PARRILLA");
            yield return new WaitForSecondsRealtime(.45f);
            Assert.IsNull(GameObject.Find("Fin de nivel"), "The previous results overlay must not survive Next.");
            Assert.IsNull(GameObject.Find("Resultado puntos"));
            Assert.AreEqual(2, ReadField<int>(game, "currentLevelNumber"));
            Assert.AreEqual(3, ((Array)GetField(game, "portions")).Length, "Level 2 must use its own three-portion setup.");
        }

        [UnityTest]
        public IEnumerator FirstPlayable_RetryResetsFireFoodAndTray()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 1200f);

            yield return EnterLevelOne(game);
            IgniteAndMoveEmbers(game);
            yield return CookAndPlate(game, 0, "TIRA DE ASADO", 25f, true);
            yield return CookAndPlate(game, 1, "CHORIZO", 25f, true);
            ClickButton("SERVIR");
            yield return new WaitForSecondsRealtime(1.8f);

            Button retry = FindButton("REINTENTAR");
            Assert.IsTrue(retry.interactable);
            ClickButton(retry);
            yield return new WaitForSecondsRealtime(.1f);

            CharcoalGrillModel grill = (CharcoalGrillModel)GetField(game, "grill");
            Assert.IsFalse(grill.IsLit, "Retry must extinguish/reset the tanda's fire.");
            Assert.AreEqual(0, ReadField<int>(game, "trayCount"));
            Assert.IsNull(GameObject.Find("Fin de nivel"));
            Array portions = (Array)GetField(game, "portions");
            for (int i = 0; i < portions.Length; i++)
            {
                object portion = portions.GetValue(i);
                Assert.IsFalse(ReadField<bool>(portion, "Started"), "Retry must reset portion state.");
                Assert.IsFalse(ReadField<bool>(portion, "OnTray"), "Retry must clear the tray assignment.");
            }
            Assert.NotNull(GameObject.Find("PRENDER CARBÓN"));
        }

        [UnityTest]
        public IEnumerator FirstPlayable_DebugScaleUsesOnlyConfiguredValuesAndPersists()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            Assert.AreEqual(30f, ReadField<float>(game, "SimulationTimeScale"), .001f,
                "A clean install should start at the specified 30x simulation scale.");
            yield return EnterLevelOne(game);

            Button scale = FindButton("DEBUG ×30");
            string[] expectedLabels = { "DEBUG ×35", "DEBUG ×40", "DEBUG ×20", "DEBUG ×25", "DEBUG ×30" };
            float[] expectedValues = { 35f, 40f, 20f, 25f, 30f };
            for (int i = 0; i < expectedValues.Length; i++)
            {
                ClickButton(scale);
                Assert.AreEqual(expectedValues[i], ReadField<float>(game, "SimulationTimeScale"), .001f);
                Assert.AreEqual(expectedLabels[i], scale.GetComponentInChildren<Text>().text);
                Assert.AreEqual(expectedValues[i], ReadSaveSetting<float>("SimulationTimeScale"), .001f,
                    "Scale selection must persist to local save.");
            }
        }

        [UnityTest]
        public IEnumerator VisualAssets_FourFoodsExposeRawWarmingIdealAndBurntStates()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            Sprite logo = (Sprite)GetField(game, "logoSprite");
            Assert.NotNull(logo, "The custom Lilita One wordmark must load at runtime.");
            Assert.AreEqual("AsaditoLogo", logo.texture.name);
            Sprite titleArt = (Sprite)GetField(game, "titleSprite");
            Assert.NotNull(titleArt, "The new title-screen illustration must load at runtime.");
            Assert.AreEqual("PortadaAsadito", titleArt.texture.name);
            Assert.That((float)titleArt.texture.width / titleArt.texture.height, Is.EqualTo(941f / 1672f).Within(.002f));
            Font displayFont = (Font)GetField(game, "displayFont");
            Font[] uiFonts =
            {
                (Font)GetField(game, "bodyFont"), (Font)GetField(game, "mediumFont"),
                (Font)GetField(game, "semiBoldFont"), (Font)GetField(game, "boldFont"),
                (Font)GetField(game, "extraBoldFont")
            };
            Assert.NotNull(displayFont, "Lilita One must load as the display face.");
            foreach (Font font in uiFonts) Assert.NotNull(font, "Every configured Baloo 2 static weight must load.");
            char[] spanishGlyphs = { 'ñ', 'Ñ', 'á', 'é', 'í', 'ó', 'ú', 'ü', '¿', '¡', '×', '·', '0', '9' };
            foreach (char glyph in spanishGlyphs)
            {
                Assert.IsTrue(displayFont.HasCharacter(glyph), "Lilita One must contain glyph " + glyph + ".");
                Assert.IsTrue(uiFonts[0].HasCharacter(glyph), "Baloo 2 must contain glyph " + glyph + ".");
            }
            Texture2D appIcon = Resources.Load<Texture2D>("Art/AsaditoAppIcon");
            Assert.NotNull(appIcon, "The authored Asadito launcher icon must be included as a runtime resource.");
            Assert.AreEqual(appIcon.width, appIcon.height, "The mobile launcher icon must remain square.");
            Button menuEnter = FindButton("ENTRAR");
            Assert.IsFalse(menuEnter.IsInteractable(), "The Enter CTA must not be clickable while its entrance animation is still hidden.");
            yield return new WaitForSecondsRealtime(.6f);
            Assert.IsTrue(menuEnter.IsInteractable(), "The Enter CTA must become interactable after its entrance animation.");

            Sprite[][] sprites = (Sprite[][])GetField(game, "foodStateSprites");
            Assert.NotNull(sprites, "The generated food-state atlas must load in runtime.");
            string[] foodIds = { "chorizo", "tira", "vacio", "provoleta" };
            string[] stages = { "raw", "warming", "ideal", "burnt" };
            Assert.AreEqual(foodIds.Length, sprites.Length);
            for (int food = 0; food < foodIds.Length; food++)
            {
                Assert.AreEqual(stages.Length, sprites[food].Length);
                for (int stage = 0; stage < stages.Length; stage++)
                {
                    Assert.NotNull(sprites[food][stage]);
                    Assert.AreEqual(foodIds[food] + "_" + stages[stage], sprites[food][stage].name);
                    Assert.AreEqual("FoodStateAtlas", sprites[food][stage].texture.name);
                }
            }

            Array portions = (Array)GetField(game, "portions");
            Image[] images = (Image[])GetField(game, "portionImages");
            MethodInfo refreshVisual = game.GetType().GetMethod("RefreshFoodVisual", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(refreshVisual);
            FoodFaceState[] cookingFaces =
            {
                new FoodFaceState { SurfaceTemperatureC = 20f },
                new FoodFaceState { SurfaceTemperatureC = 60f },
                new FoodFaceState { SurfaceTemperatureC = 140f, Maillard = .2f },
                new FoodFaceState { SurfaceTemperatureC = 220f, Maillard = .8f, Char = .35f }
            };
            for (int index = 0; index < portions.Length; index++)
            {
                FoodState state = (FoodState)GetField(portions.GetValue(index), "State");
                string foodId = ((FoodCookProfile)GetField(portions.GetValue(index), "Profile")).FoodId;
                for (int stage = 0; stage < cookingFaces.Length; stage++)
                {
                    state.SetCurrentFace(cookingFaces[stage]);
                    refreshVisual.Invoke(game, new object[] { index });
                    Assert.AreEqual(foodId + "_" + stages[stage], images[index].sprite.name,
                        foodId + " should swap to the matching cooking illustration.");
                }
                state.Reset();
                refreshVisual.Invoke(game, new object[] { index });
            }
        }

        [UnityTest]
        public IEnumerator FirstPlayable_L1CompletesAtConfigured30xWithoutCookingTimerShortcuts()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            Assert.AreEqual(30f, ReadField<float>(game, "SimulationTimeScale"), .001f,
                "L1 duration validation must use the shipped default simulation scale.");

            DateTime started = DateTime.UtcNow;
            yield return EnterLevelOne(game);
            IgniteAndMoveEmbers(game);
            yield return CookAndPlate(game, 0, "TIRA DE ASADO", 60f, true);
            yield return CookAndPlate(game, 1, "CHORIZO", 60f, true);
            Assert.IsTrue(FindButton("SERVIR").interactable);
            ClickButton("SERVIR");
            yield return new WaitForSecondsRealtime(2f);
            Assert.NotNull(GameObject.Find("Fin de nivel"));

            double elapsedSeconds = (DateTime.UtcNow - started).TotalSeconds;
            Assert.LessOrEqual(elapsedSeconds, 180d, "Automated L1 should remain under the 2–3 minute First Playable tuning ceiling at 30x.");
            Assert.Greater(ReadField<float>(game, "SimulationTimeScale"), 0f);
        }

        [UnityTest]
        public IEnumerator Mvp_AllFiveLevels_CookServeResultsUnlockNextAndReturnToSelection()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 1200f);

            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("IR A LA PARRILLA");
            yield return new WaitForSecondsRealtime(.45f);

            int[] expectedPortions = { 2, 3, 4, 4, 6 };
            for (int level = 1; level <= 5; level++)
            {
                Assert.AreEqual(level, ReadField<int>(game, "currentLevelNumber"));
                Array portions = (Array)GetField(game, "portions");
                Array guests = (Array)GetField(game, "activeGuests");
                Assert.AreEqual(expectedPortions[level - 1], portions.Length, "L" + level + " order size must match its MVP definition.");
                Assert.AreEqual(expectedPortions[level - 1], guests.Length, "Each portion must serve a guest in L" + level + ".");

                IgniteAndMoveEmbers(game, assertTutorialStep: level == 1);
                for (int portionIndex = 0; portionIndex < portions.Length; portionIndex++)
                    yield return CookAndPlateByIndex(game, portionIndex, 35f);

                Button serve = FindButton("SERVIR");
                Assert.IsTrue(serve.interactable, "All L" + level + " portions must be plated before service.");
                ClickButton(serve);
                yield return new WaitForSecondsRealtime(2f);

                Assert.NotNull(GameObject.Find("Fin de nivel"), "L" + level + " must reach results after service.");
                Assert.That(GameObject.Find("Resultado puntos").GetComponent<Text>().text, Does.Contain("PUNTOS"));
                int[] guestExpressions = (int[])GetField(game, "guestExpressions");
                string[] expressionNames = { "neutral", "happy", "very_happy", "disappointed" };
                for (int guestIndex = 0; guestIndex < guests.Length; guestIndex++)
                {
                    GuestProfile guest = (GuestProfile)guests.GetValue(guestIndex);
                    GameObject resultPortrait = GameObject.Find("Resultado retrato " + guest.Name);
                    Assert.NotNull(resultPortrait, guest.Name + " needs an individual portrait on L" + level + " results.");
                    Assert.AreEqual(guest.Id + "_" + expressionNames[guestExpressions[guestIndex]],
                        resultPortrait.GetComponent<Image>().sprite.name, "Result portrait expression must match that guest's evaluation.");
                }
                MvpSaveData result = MvpSave.Load();
                Assert.GreaterOrEqual(result.StarsByLevel[level - 1], 1, "L" + level + " must earn one star to unlock the next MVP level.");
                Assert.Greater(result.BestScoreByLevel[level - 1], 0, "L" + level + " must persist its score.");

                if (level < 5)
                {
                    Button next = FindButton("SIGUIENTE");
                    Assert.IsTrue(next.interactable, "Passing L" + level + " must unlock Next.");
                    ClickButton(next);
                    yield return new WaitForSecondsRealtime(.55f);
                    Assert.That(GameObject.Find("Intro título").GetComponent<Text>().text, Does.Contain("NIVEL " + (level + 1)));
                    Assert.IsNull(GameObject.Find("Resultado retrato " + ((GuestProfile)guests.GetValue(0)).Name),
                        "Result portraits must be cleared with their overlay when advancing to the next level.");
                    ClickButton("IR A LA PARRILLA");
                    yield return new WaitForSecondsRealtime(.45f);
                }
                else
                {
                    Assert.IsTrue(FindButton("NIVELES").interactable, "The last result must return to level selection.");
                    ClickButton("NIVELES");
                    yield return new WaitForSecondsRealtime(.5f);
                    Assert.NotNull(GameObject.Find("Seleccion de nivel"), "L5 completion must return to level selection.");
                }
            }
        }

        private static IEnumerator CookAndPlateByIndex(Component game, int index, float timeoutSeconds)
        {
            Button[] buttons = (Button[])GetField(game, "portionButtons");
            Array portions = (Array)GetField(game, "portions");
            object portion = portions.GetValue(index);
            FoodState state = (FoodState)GetField(portion, "State");
            FoodCookProfile profile = (FoodCookProfile)GetField(portion, "Profile");
            ClickButton(buttons[index]);
            yield return null;

            RectTransform grid = GameObject.Find("Mapa de calor carbón 8x6").GetComponent<RectTransform>();
            Vector3 centerWorld = grid.TransformPoint(new Vector3(grid.rect.width * .03f, -grid.rect.height * .015f, 0f));
            Vector2 centerScreen = RectTransformUtility.WorldToScreenPoint(null, centerWorld);
            Image image = ((Image[])GetField(game, "portionImages"))[index];
            Component foodTouch = FindComponent(image.gameObject, "Asadito.Runtime.FoodPieceTouch");
            var pointer = new PointerEventData(EventSystem.current);
            Assert.IsTrue(ExecuteEvents.Execute(foodTouch.gameObject, pointer, ExecuteEvents.beginDragHandler));
            pointer.position = centerScreen;
            Assert.IsTrue(ExecuteEvents.Execute(foodTouch.gameObject, pointer, ExecuteEvents.dragHandler));
            Assert.IsTrue(ExecuteEvents.Execute(foodTouch.gameObject, pointer, ExecuteEvents.endDragHandler));

            float elapsed = 0f;
            while (state.CoreTemperatureC < 38f && elapsed < timeoutSeconds)
            {
                yield return new WaitForSecondsRealtime(.01f);
                elapsed += .01f;
            }
            Assert.GreaterOrEqual(state.CoreTemperatureC, 38f, profile.FoodId + " must warm before flipping.");
            int exposedFace = state.ExposedFace;
            ClickButton("DAR VUELTA");
            Assert.AreNotEqual(exposedFace, state.ExposedFace, profile.FoodId + " flip must expose the second face.");

            elapsed = 0f;
            while (FoodCookingModel.GetDoneness(state, profile) != Doneness.A_Punto && elapsed < timeoutSeconds)
            {
                yield return new WaitForSecondsRealtime(.01f);
                elapsed += .01f;
            }
            Assert.AreEqual(Doneness.A_Punto, FoodCookingModel.GetDoneness(state, profile), profile.FoodId + " must reach a valid point band.");
            ClickButton(buttons[index]);
            yield return new WaitForSecondsRealtime(.5f);
            Assert.IsTrue(ReadField<bool>(portion, "OnTray"), profile.FoodId + " must reach the serving tray.");
            Assert.AreEqual(index + 1, ReadField<int>(game, "trayCount"));
        }

        private static IEnumerator LoadGameScene(Action<Component> setGame)
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
            yield return null;
            yield return null;
            Type gameType = Type.GetType("Asadito.AsaditoGame, Assembly-CSharp");
            Assert.NotNull(gameType, "AsaditoGame must be present in Assembly-CSharp.");
            GameObject root = GameObject.Find("Asadito MVP");
            Assert.NotNull(root, "Build scene must contain the Asadito MVP root.");
            setGame(root.GetComponent(gameType));
        }

        private static void AssertGameplayGrillArtLoaded(Component game)
        {
            Sprite sprite = (Sprite)GetField(game, "gameplayGrillSprite");
            Assert.NotNull(sprite, "The gameplay grill background must load as a Sprite.");
            Assert.AreEqual("ParrillaTopDownStylized", sprite.texture.name,
                "The new stylized top-down grill should be the runtime primary asset, not the legacy photo fallback.");

            Sprite[,] portraits = (Sprite[,])GetField(game, "guestPortraitSprites");
            Assert.NotNull(portraits, "The six-guest portrait atlas must load in runtime.");
            Assert.AreEqual(6, portraits.GetLength(0));
            Assert.AreEqual(4, portraits.GetLength(1));
            string[] guests = { "ana", "tito", "luz", "beto", "mora", "rulo" };
            string[] expressions = { "neutral", "happy", "very_happy", "disappointed" };
            for (int guest = 0; guest < guests.Length; guest++)
            for (int expression = 0; expression < expressions.Length; expression++)
            {
                Assert.NotNull(portraits[guest, expression]);
                Assert.AreEqual(guests[guest] + "_" + expressions[expression], portraits[guest, expression].name);
                Assert.AreEqual("GuestPortraitAtlas", portraits[guest, expression].texture.name);
            }
            Image avatarImage = (Image)GetField(game, "avatarImage");
            Assert.NotNull(avatarImage);
            Assert.AreEqual("ana_neutral", avatarImage.sprite.name, "Gameplay starts with a distinct neutral guest portrait.");
            MethodInfo applyPortrait = game.GetType().GetMethod("ApplyGuestPortrait", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(applyPortrait);
            Text guestName = (Text)GetField(game, "guestName");
            applyPortrait.Invoke(game, new object[] { 1, 3 });
            Assert.AreEqual("tito_disappointed", avatarImage.sprite.name, "Guest identity and expression must update as service reactions are shown.");
            Assert.AreEqual("TITO", guestName.text);
            applyPortrait.Invoke(game, new object[] { 0, 0 });
            guestName.text = "ANA + TITO";
        }

        private static IEnumerator EnterLevelOne(Component game)
        {
            // Menu CTAs become interactable after the short, staggered cover entrance.
            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("IR A LA PARRILLA");
            yield return new WaitForSecondsRealtime(.45f);
            Assert.NotNull(GameObject.Find("Mapa de calor carbón 8x6"));
            Assert.That(FindText("Tutorial contextual").text, Does.Contain("Paso 1"));
            Assert.AreEqual(2, ((Array)GetField(game, "portions")).Length);
            Assert.AreEqual(2, ((Array)GetField(game, "activeGuests")).Length);
            CharcoalGrillModel grill = (CharcoalGrillModel)GetField(game, "grill");
            Assert.AreEqual(8, grill.Grid.Width);
            Assert.AreEqual(6, grill.Grid.Height);
        }

        private static void IgniteAndMoveEmbers(Component game, bool assertTutorialStep = true)
        {
            ClickButton("PRENDER CARBÓN");
            if (assertTutorialStep) Assert.That(FindText("Tutorial contextual").text, Does.Contain("Paso 2"));
            CharcoalGrillModel grill = (CharcoalGrillModel)GetField(game, "grill");
            Assert.IsTrue(grill.IsLit);
            float sourceBefore = grill.Grid.GetCell(3, 2).EmberEnergy;
            float targetBefore = grill.Grid.GetCell(0, 2).EmberEnergy;
            Vector2 targetPosition = new Vector2(.0625f, .4167f);
            float targetHeatBefore = grill.Sample(targetPosition, new Vector2(.14f, .14f)).x;

            RectTransform grid = GameObject.Find("Mapa de calor carbón 8x6").GetComponent<RectTransform>();
            Vector3 targetWorld = grid.TransformPoint(new Vector3((0 - 3.5f) * 102f, (2 - 2.5f) * 46f, 0f));
            Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(null, targetWorld);
            Component emberTouch = FindComponent(GameObject.Find("Brasa 3,2"), "Asadito.Runtime.EmberCellTouch");
            PointerEventData emberPointer = new PointerEventData(EventSystem.current);
            Assert.IsTrue(ExecuteEvents.Execute(emberTouch.gameObject, emberPointer, ExecuteEvents.pointerDownHandler));
            emberPointer.position = targetScreen;
            Assert.IsTrue(ExecuteEvents.Execute(emberTouch.gameObject, emberPointer, ExecuteEvents.dragHandler));

            Assert.Less(grill.Grid.GetCell(3, 2).EmberEnergy, sourceBefore);
            Assert.Greater(grill.Grid.GetCell(0, 2).EmberEnergy, targetBefore);
            Assert.Greater(grill.Sample(targetPosition, new Vector2(.14f, .14f)).x, targetHeatBefore + 5f,
                "Moving embers must visibly change the target zone's sampled heat.");
            if (assertTutorialStep) Assert.That(FindText("Tutorial contextual").text, Does.Contain("Paso 3"));
        }

        private static IEnumerator CookAndPlate(Component game, int index, string foodButton, float timeoutSeconds, bool flip)
        {
            Button food = FindButton(foodButton);
            ClickButton(food);
            if (index == 0) Assert.That(FindText("Tutorial contextual").text, Does.Contain("Paso 4"));
            else Assert.That(FindText("Tutorial contextual").text, Does.Contain("Repetí con la otra porción"));
            Array portions = (Array)GetField(game, "portions");
            object portion = portions.GetValue(index);
            FoodState state = (FoodState)GetField(portion, "State");
            FoodCookProfile profile = (FoodCookProfile)GetField(portion, "Profile");
            Assert.IsTrue(ReadField<bool>(portion, "Started"));
            Vector2 startingPosition = ReadField<Vector2>(portion, "Position");

            // Exercise the same public handlers used by the touch-drag component.
            RectTransform grid = GameObject.Find("Mapa de calor carbón 8x6").GetComponent<RectTransform>();
            Vector3 moveWorld = grid.TransformPoint(new Vector3(grid.rect.width * .03f, -grid.rect.height * .015f, 0f));
            Vector2 moveScreen = RectTransformUtility.WorldToScreenPoint(null, moveWorld);
            Array images = (Array)GetField(game, "portionImages");
            Component foodTouch = FindComponent(((Image)images.GetValue(index)).gameObject, "Asadito.Runtime.FoodPieceTouch");
            PointerEventData foodPointer = new PointerEventData(EventSystem.current);
            Assert.IsTrue(ExecuteEvents.Execute(foodTouch.gameObject, foodPointer, ExecuteEvents.beginDragHandler));
            foodPointer.position = moveScreen;
            Assert.IsTrue(ExecuteEvents.Execute(foodTouch.gameObject, foodPointer, ExecuteEvents.dragHandler));
            Assert.IsTrue(ExecuteEvents.Execute(foodTouch.gameObject, foodPointer, ExecuteEvents.endDragHandler));
            Assert.Greater(Vector2.Distance(startingPosition, ReadField<Vector2>(portion, "Position")), .1f,
                "Dragging the portion must change its normalized grill position.");
            if (index == 0) Assert.That(FindText("Tutorial contextual").text, Does.Contain("Mové la pieza"));

            if (flip)
            {
                float elapsedBeforeFlip = 0f;
                while (state.CoreTemperatureC < 38f && elapsedBeforeFlip < timeoutSeconds)
                {
                    yield return new WaitForSecondsRealtime(.01f);
                    elapsedBeforeFlip += .01f;
                }
                Assert.GreaterOrEqual(state.CoreTemperatureC, 38f, foodButton + " should start warming before flip.");
                int faceBefore = state.ExposedFace;
                ClickButton("DAR VUELTA");
                Assert.AreNotEqual(faceBefore, state.ExposedFace, foodButton + " flip must expose its other side.");
                if (index == 0) Assert.That(FindText("Tutorial contextual").text, Does.Contain("Paso 5"));
            }

            float elapsed = 0f;
            while (FoodCookingModel.GetDoneness(state, profile) != Doneness.A_Punto && elapsed < timeoutSeconds)
            {
                yield return new WaitForSecondsRealtime(.01f);
                elapsed += .01f;
            }
            Assert.AreEqual(Doneness.A_Punto, FoodCookingModel.GetDoneness(state, profile), foodButton + " should reach its configured point band.");
            Assert.Greater(state.CoreTemperatureC, 0f);

            ClickButton(food); // The active cut's second tap removes it to the serving tray.
            yield return new WaitForSecondsRealtime(.5f);
            Assert.IsTrue(ReadField<bool>(portion, "OnTray"), foodButton + " should be on the tray before service.");
            Assert.That(FindText("Tutorial contextual").text, Does.Contain("Repetí con la otra porción"));
        }

        private static Button FindButton(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.NotNull(go, "Expected active UI button: " + name);
            Button button = go.GetComponent<Button>();
            Assert.NotNull(button, "Expected Button component: " + name);
            Assert.IsTrue(button.interactable, "Button should be interactable: " + name);
            return button;
        }

        private static void ClickButton(string name) { ClickButton(FindButton(name)); }

        private static void ClickButton(Button button)
        {
            Assert.NotNull(EventSystem.current, "The scene must provide a live EventSystem.");
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            Assert.IsTrue(ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler),
                "The button must receive a Unity pointer click.");
        }

        private static Text FindText(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.NotNull(go, "Expected active UI text: " + name);
            Text text = go.GetComponent<Text>();
            Assert.NotNull(text, "Expected Text component: " + name);
            return text;
        }

        private static Component FindComponent(GameObject go, string fullTypeName)
        {
            Type type = Type.GetType(fullTypeName + ", Assembly-CSharp");
            Assert.NotNull(type, "Expected runtime component type: " + fullTypeName);
            Component component = go.GetComponent(type);
            Assert.NotNull(component, "Expected component " + fullTypeName + " on " + go.name);
            return component;
        }

        private static object GetField(object target, string name)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(field, "Missing field: " + target.GetType().Name + "." + name);
            return field.GetValue(target);
        }

        private static T ReadField<T>(object target, string name) { return (T)GetField(target, name); }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(field, "Missing field: " + target.GetType().Name + "." + name);
            field.SetValue(target, value);
        }

        private static int ReadSaveInt(string name)
        {
            Type saveType = Type.GetType("Asadito.Runtime.MvpSave, Asadito.Runtime");
            Assert.NotNull(saveType);
            MethodInfo load = saveType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
            object data = load.Invoke(null, null);
            return ReadField<int>(data, name);
        }

        private static T ReadSaveSetting<T>(string name)
        {
            Type saveType = Type.GetType("Asadito.Runtime.MvpSave, Asadito.Runtime");
            Assert.NotNull(saveType);
            object data = saveType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            object settings = GetField(data, "Settings");
            return ReadField<T>(settings, name);
        }

        private static void ResetSaveCache()
        {
            Type saveType = Type.GetType("Asadito.Runtime.MvpSave, Asadito.Runtime");
            if (saveType == null) return;
            FieldInfo cache = saveType.GetField("cached", BindingFlags.NonPublic | BindingFlags.Static);
            if (cache != null) cache.SetValue(null, null);
        }
    }
}
