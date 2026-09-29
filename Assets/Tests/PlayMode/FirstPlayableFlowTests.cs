using System;
using System.Collections.Generic;
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
            Assert.AreEqual("Asadito UI Icon Star", GameObject.Find("Resultado estrella 1").GetComponent<Image>().sprite.name);
            Assert.AreEqual("tira_ideal", GameObject.Find("Resultado icon comida TIRA DE ASADO").GetComponent<Image>().sprite.name);
            Assert.AreEqual("chorizo_ideal", GameObject.Find("Resultado icon comida CHORIZO").GetComponent<Image>().sprite.name);
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
        public IEnumerator HeatGridColorTween_IsNotCancelledBySimulationUpdate()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            yield return EnterLevelOne(game);

            ClickButton("PRENDER CARBÓN");
            object tween = GetField(game, "heatVisualRoutine");
            Assert.NotNull(tween, "Igniting the grill should start the heat-cell color transition.");

            MethodInfo update = game.GetType().GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(update);
            update.Invoke(game, null);
            Assert.AreSame(tween, GetField(game, "heatVisualRoutine"),
                "The normal simulation update must not cancel the active heat-cell transition.");
        }

        [UnityTest]
        public IEnumerator VisualAssets_CatalogFoodsExposeSixThermalStatesPerFace()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
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
            Text menuBrand = FindText("Menu marca");
            Assert.AreEqual("Asadito", menuBrand.text, "The cover wordmark must use the requested exact spelling without a comma.");
            Assert.AreEqual(displayFont, menuBrand.font, "Lilita One is reserved for the chunky cover wordmark.");
            Assert.AreEqual(112, menuBrand.fontSize, "The main title should read as a large game-style display heading.");
            Assert.AreEqual(Color.white, menuBrand.color, "The title uses the reference's bright white face.");
            Assert.AreEqual((Color)new Color32(14, 14, 18, 255), menuBrand.GetComponent<Outline>().effectColor);
            Assert.Greater(menuBrand.rectTransform.anchorMin.y, .9f, "The wordmark belongs above the grill illustration.");
            Text menuTagline = FindText("Menu subtitulo");
            Assert.AreEqual("el sabor Argentino", menuTagline.text, "Keep the requested tagline exact and on its own line.");
            Assert.AreEqual(displayFont, menuTagline.font, "The tagline should use the chunky display face from the reference style.");
            Assert.AreEqual(50, menuTagline.fontSize, "Keep the tagline prominent and readable below the title.");
            Assert.AreEqual(HorizontalWrapMode.Overflow, menuTagline.horizontalOverflow);
            Assert.AreEqual(VerticalWrapMode.Overflow, menuTagline.verticalOverflow);
            Assert.Less(menuBrand.rectTransform.anchorMin.y - menuTagline.rectTransform.anchorMin.y, .05f,
                "Keep the tagline close beneath the wordmark instead of leaving a large vertical gap.");
            Assert.GreaterOrEqual(menuTagline.rectTransform.rect.height, 100f, "The tagline needs enough vertical room to render on device.");
            Assert.AreEqual((Color)new Color32(14, 14, 18, 245), menuTagline.GetComponent<Outline>().effectColor);
            Assert.Greater(menuTagline.rectTransform.anchorMin.y, .8f, "The tagline must remain in the clear title area above the grill.");
            Assert.IsNull(GameObject.Find("Bandera argentina"),
                "The Argentine flag should be part of the illustrated dish towel, not a separate waving UI badge.");
            Image coverIllustration = GameObject.Find("Portada ilustrada").GetComponent<Image>();
            Assert.AreSame(titleArt.texture, coverIllustration.sprite.texture,
                "The cover must keep its authored illustration, including the integrated Argentine dish towel.");
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
            AssertActionIconInsideButton(menuEnter, "ENTRAR");
            Assert.AreEqual("Arcade Gold Button", ((Image)menuEnter.targetGraphic).sprite.name,
                "Primary actions should use the raised gold arcade-button skin.");
            Text menuEnterLabel = menuEnter.GetComponentInChildren<Text>();
            Assert.AreEqual(displayFont, menuEnterLabel.font, "Button labels share the chunky display face.");
            Assert.GreaterOrEqual(menuEnterLabel.fontSize, 48, "Primary calls to action should be sized to match the bold arcade reference.");
            Assert.AreEqual(new Vector2(560f, 118f), menuEnter.GetComponent<RectTransform>().rect.size,
                "The two title-screen actions should share the same button dimensions.");
            Assert.AreEqual(new Vector2(12f, 8f), menuEnterLabel.rectTransform.offsetMin,
                "Inset the CTA label symmetrically so its center matches the button center.");
            Assert.AreEqual(new Vector2(-12f, -8f), menuEnterLabel.rectTransform.offsetMax,
                "Inset the CTA label symmetrically so its center matches the button center.");
            Assert.AreEqual(Color.white, menuEnterLabel.color, "Button labels use bright white fill.");
            Assert.NotNull(menuEnterLabel.GetComponent<Outline>(), "Chunky button lettering needs its dark arcade outline.");
            Assert.NotNull(menuEnter.transform.Find("Relieve inferior ENTRAR"), "The arcade button has a visible lower extrusion.");
            Button menuExit = GameObject.Find("SALIR").GetComponent<Button>();
            Assert.AreEqual("Arcade Coral Button", ((Image)menuExit.targetGraphic).sprite.name,
                "Back/exit actions should use the coral red style from the reference.");
            Assert.AreEqual(menuEnter.GetComponent<RectTransform>().rect.size, menuExit.GetComponent<RectTransform>().rect.size,
                "The Enter and Exit actions should match in width and height.");
            Assert.NotNull(menuExit.transform.Find("Borde boton SALIR"), "The button face has a dark contour layer.");
            Assert.IsFalse(menuEnter.IsInteractable(), "The Enter CTA must not be clickable while its entrance animation is still hidden.");
            yield return new WaitForSecondsRealtime(.6f);
            Assert.IsTrue(menuEnter.IsInteractable(), "The Enter CTA must become interactable after its entrance animation.");

            ClickButton("ENTRAR");
            yield return new WaitForSecondsRealtime(.45f);
            Button levelOne = FindButton("NIVEL 1");
            Assert.AreEqual("Nivel 1", levelOne.GetComponentInChildren<Text>().text,
                "Each level card should show only its simple level label.");
            Image levelOneImage = levelOne.transform.Find("Imagen representativa nivel 1").GetComponent<Image>();
            Assert.AreEqual("Imagen Nivel 1", levelOneImage.sprite.name,
                "Level 1 should use its own representative illustration.");
            Assert.AreEqual(1, levelOne.GetComponentsInChildren<Text>().Length,
                "The level card must not show descriptive metadata or menu details.");
            Assert.IsNull(levelOne.transform.Find("Icon comensal NIVEL 1"));
            Assert.IsNull(levelOne.transform.Find("Icon candado NIVEL 1"));
            Assert.IsNull(levelOne.transform.Find("Icon comida tira NIVEL 1"));
            Assert.IsNull(levelOne.transform.Find("Icon estrella 1 NIVEL 1"));
            Button levelTwo = GameObject.Find("NIVEL 2").GetComponent<Button>();
            Assert.IsNotNull(levelTwo);
            Assert.AreEqual("Nivel 2", levelTwo.GetComponentInChildren<Text>().text);
            Assert.AreEqual("Imagen Nivel 2",
                levelTwo.transform.Find("Imagen representativa nivel 2").GetComponent<Image>().sprite.name);
            Assert.AreEqual(1, levelTwo.GetComponentsInChildren<Text>().Length);
            Assert.IsFalse(levelTwo.interactable);
            Assert.IsNull(GameObject.Find("Seleccion ayuda"),
                "The level-selection screen should not add a secondary block of descriptive copy.");
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            Assert.AreEqual("Asadito UI Icon Guest", GameObject.Find("Icon comensales intro").GetComponent<Image>().sprite.name);
            Assert.AreEqual("tira_ideal", GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<Image>().sprite.name);
            Assert.AreEqual("chorizo_ideal", GameObject.Find("Icon comida intro CHORIZO").GetComponent<Image>().sprite.name);
            Button introStart = FindButton("IR A LA PARRILLA");
            Assert.AreEqual("Asadito UI Icon Next", introStart.transform.Find("Icono accion IR A LA PARRILLA").GetComponent<Image>().sprite.name);
            AssertActionIconInsideButton(introStart, "IR A LA PARRILLA");

            var sprites = (Dictionary<string, Sprite[]>)GetField(game, "foodStateSprites");
            Assert.NotNull(sprites, "Food-state atlases should be cached by catalog id.");
            string[] foodIds = { "tira", "chorizo" };
            string[] stages = FoodSpriteLibrary.StateNames;
            Assert.AreEqual(foodIds.Length, sprites.Count,
                "Runtime should load only foods needed by the selected order, not eagerly load all atlases.");
            foreach (string foodId in foodIds)
            {
                Assert.IsTrue(sprites.TryGetValue(foodId, out Sprite[] states), "Missing lazily loaded state atlas " + foodId);
                Assert.AreEqual(stages.Length, states.Length);
                for (int stage = 0; stage < stages.Length; stage++)
                {
                    Assert.NotNull(states[stage]);
                    Assert.AreEqual(foodId + "_" + stages[stage], states[stage].name);
                    Assert.AreEqual(foodId, states[stage].texture.name,
                        "Each cut should have its own optimized six-state atlas.");
                    Assert.Greater(states[stage].rect.width, 0f);
                    Assert.Greater(states[stage].rect.height, 0f);
                }
            }

            Array portions = (Array)GetField(game, "portions");
            Image[] images = (Image[])GetField(game, "portionImages");
            MethodInfo refreshVisual = game.GetType().GetMethod("RefreshFoodVisual", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(refreshVisual);
            FoodFaceState[] cookingFaces =
            {
                new FoodFaceState { SurfaceTemperatureC = 20f },
                new FoodFaceState { SurfaceTemperatureC = 45f },
                new FoodFaceState { SurfaceTemperatureC = 140f, Maillard = .05f },
                new FoodFaceState { SurfaceTemperatureC = 160f, Maillard = .36f },
                new FoodFaceState { SurfaceTemperatureC = 190f, Maillard = .6f },
                new FoodFaceState { SurfaceTemperatureC = 220f, Maillard = .8f, Char = .75f }
            };
            float[] coreTemperatures = { 20f, 30f, 40f, 60f, 60f, 72f };
            float[] moistures = { 1f, 1f, 1f, 1f, .2f, .1f };
            for (int index = 0; index < portions.Length; index++)
            {
                FoodState state = (FoodState)GetField(portions.GetValue(index), "State");
                FoodCookProfile profile = (FoodCookProfile)GetField(portions.GetValue(index), "Profile");
                string foodId = profile.FoodId;
                for (int stage = 0; stage < cookingFaces.Length; stage++)
                {
                    state.CoreTemperatureC = coreTemperatures[stage];
                    if (stage == 3) state.CoreTemperatureC = profile.DonenessBands[0].MinimumCoreC + .5f;
                    state.Moisture = moistures[stage];
                    state.SetCurrentFace(cookingFaces[stage]);
                    refreshVisual.Invoke(game, new object[] { index });
                    Assert.AreEqual(foodId + "_" + stages[stage], images[index].sprite.name,
                        foodId + " should swap to the matching cooking illustration.");
                }

                state.Reset();
                state.CoreTemperatureC = profile.DonenessBands[0].MinimumCoreC + .5f;
                state.SetCurrentFace(cookingFaces[3]);
                refreshVisual.Invoke(game, new object[] { index });
                Assert.AreEqual(foodId + "_ideal", images[index].sprite.name);
                state.Flip();
                refreshVisual.Invoke(game, new object[] { index });
                Assert.AreEqual(foodId + "_raw", images[index].sprite.name,
                    "The unexposed side must retain its own uncooked visual state.");
                state.Flip();
                refreshVisual.Invoke(game, new object[] { index });
                Assert.AreEqual(foodId + "_ideal", images[index].sprite.name,
                    "Flipping back must restore the first face's cooked visual state.");
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
        public IEnumerator Mvp_AllTwelveLevels_CookServeResultsUnlockNextAndReturnToSelection()
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

            int[] expectedPortions = { 2, 3, 4, 4, 6, 5, 4, 5, 5, 6, 6, 6 };
            for (int level = 1; level <= expectedPortions.Length; level++)
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

                if (level < expectedPortions.Length)
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
                    Assert.NotNull(GameObject.Find("Seleccion de nivel"), "L12 completion must return to level selection.");
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
            float plateElapsed = 0f;
            while (ReadField<int>(game, "trayCount") < index + 1 && plateElapsed < 3f)
            {
                yield return new WaitForSecondsRealtime(.05f);
                plateElapsed += .05f;
            }
            Assert.IsTrue(ReadField<bool>(portion, "OnTray"), profile.FoodId + " must reach the serving tray.");
            Assert.AreEqual(index + 1, ReadField<int>(game, "trayCount"));
        }

        private static IEnumerator LoadGameScene(Action<Component> setGame)
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;
            yield return null;
            Assert.AreEqual("SampleScene", SceneManager.GetActiveScene().name,
                "PlayMode tests must explicitly load the shipped gameplay scene.");
            Type gameType = Type.GetType("Asadito.AsaditoGame, Assembly-CSharp");
            Assert.NotNull(gameType, "AsaditoGame must be present in Assembly-CSharp.");
            GameObject root = GameObject.Find("Asadito MVP");
            Assert.NotNull(root, "Build scene must contain the Asadito MVP root.");
            Component game = root.GetComponent(gameType);
            Assert.NotNull(game);
            Assert.NotNull(GameObject.Find("Asadito UI"),
                "AsaditoGame.Start must construct its runtime Canvas in the shipped scene.");
            GameObject menuRoot = (GameObject)GetField(game, "menuRoot");
            Assert.NotNull(menuRoot, "BuildFrontEnd must create the title menu root.");
            Assert.IsTrue(menuRoot.activeInHierarchy, "The title menu should be active immediately after loading the shipped scene.");
            setGame(game);
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
            Image ignitionFlame = GameObject.Find("Destello de encendido").GetComponent<Image>();
            Assert.AreEqual("Asadito UI Icon Flame", ignitionFlame.sprite.name);
            if (assertTutorialStep) Assert.That(FindText("Tutorial contextual").text, Does.Contain("Paso 2"));
            CharcoalGrillModel grill = (CharcoalGrillModel)GetField(game, "grill");
            Assert.IsTrue(grill.IsLit);
            float sourceBefore = grill.Grid.GetCell(3, 2).EmberEnergy;
            float targetBefore = grill.Grid.GetCell(0, 2).EmberEnergy;
            Image targetVisual = GameObject.Find("Brasa 0,2").GetComponent<Image>();
            Color targetColorBefore = targetVisual.color;
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
            Assert.AreNotEqual(targetColorBefore, targetVisual.color,
                "Raking must update the changed cell's color without waiting for a full-grid refresh.");
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
            float plateElapsed = 0f;
            while (ReadField<int>(game, "trayCount") < index + 1 && plateElapsed < 3f)
            {
                yield return new WaitForSecondsRealtime(.05f);
                plateElapsed += .05f;
            }
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

        private static void AssertActionIconInsideButton(Button button, string label)
        {
            Assert.NotNull(button);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            RectTransform iconRect = button.transform.Find("Icono accion " + label).GetComponent<RectTransform>();
            float iconLeft = iconRect.anchoredPosition.x - iconRect.rect.width * .5f;
            float iconRight = iconRect.anchoredPosition.x + iconRect.rect.width * .5f;
            Assert.GreaterOrEqual(iconLeft, 0f, label + " action icon must not render outside the left edge of its button.");
            Assert.LessOrEqual(iconRight, buttonRect.rect.width, label + " action icon must remain inside its button.");
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
