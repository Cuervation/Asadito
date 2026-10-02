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
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
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
        public IEnumerator FirstPlayable_CookServePersistsResultAndUnlocksNextAsado()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game, "SampleScene must start the Asadito runtime controller.");
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 120f); // Keep enough real-time resolution to observe each cut's individual point band.

            yield return EnterLevelOne(game);
            AssertGrillReadyWithoutCoal(game);

            yield return CookAndPlateOrder(game, 35f);

            Assert.IsTrue(CanServeFromBoard(game), "Serving unlocks on the physical board after every portion is plated.");
            TapBoard(game);
            yield return new WaitForSecondsRealtime(1.8f);

            Assert.NotNull(GameObject.Find("Management ¡ASADO COMPLETADO!"), "Serving must reach the result screen.");
            Text resultScore = GameObject.Find("GENERAL " + Mathf.RoundToInt(((EconomicResult)GetField(game,"economicResult")).Overall) + "%").GetComponent<Text>();
            Assert.That(resultScore.text, Does.EndWith("%"));
            Assert.AreEqual("RESULTADO GENERAL",FindText("Result general label").text);
            Assert.IsNotNull(GameObject.Find("OTRO ASADO"));
            Button next = GameObject.Find("SIGUIENTE").GetComponent<Button>();
            Assert.IsTrue(next.interactable, "Completing the tutorial unlocks the next asado.");
            Assert.AreEqual("Asadito UI Icon Star", GameObject.Find("Management star 0").GetComponent<Image>().sprite.name);
            Assert.GreaterOrEqual(ReadSaveInt("MaxUnlockedLevel"), 2);
            MvpSaveData saved = MvpSave.Load();
            Assert.AreEqual(MvpSaveData.CurrentVersion, saved.Version);
            Assert.Greater(saved.BestScoreByLevel[0], 0, "The result score must persist for level 1.");
            Assert.LessOrEqual(saved.BestScoreByLevel[0], 200, "A two-guest level is capped at 200 points.");
            Assert.GreaterOrEqual(saved.StarsByLevel[0], 1, "Passing level 1 must persist at least one star.");
            Assert.IsTrue(saved.Settings.TutorialCompleted, "Completing the in-game tutorial must persist its completion.");
        }

        [UnityTest]
        public IEnumerator Progression_LevelTwoRequiresCompletedLevelOneEvenWithLegacyUnlock()
        {
            var save = MvpSaveData.Migrate(new MvpSaveData { MaxUnlockedLevel = 6 });
            MvpSave.Save(save);
            Component game = null;
            yield return LoadGameScene(value => game = value);
            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR"); yield return new WaitForSecondsRealtime(.5f);
            Button levelTwo = GameObject.Find("NIVEL 2").GetComponent<Button>();
            Assert.IsFalse(levelTwo.interactable,
                "A legacy unlock flag cannot make Level 2 playable before Level 1 is completed.");
            Assert.IsTrue(levelTwo.transform.Find("Candado nivel 2").gameObject.activeSelf);
            Assert.IsTrue(levelTwo.transform.Find("Disabled nivel 2").gameObject.activeSelf);
            string persisted = PlayerPrefs.GetString(SaveKey);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            game.GetType().GetMethod("SelectLevel", flags).Invoke(game, new object[] { 2 });
            game.GetType().GetMethod("PlayNextLevel", flags).Invoke(game, null);
            yield return null;
            Assert.AreEqual(1, ReadField<int>(game, "currentLevelNumber"));
            Assert.IsFalse(((GameObject)GetField(game, "introRoot")).activeInHierarchy);
            Assert.AreEqual(persisted, PlayerPrefs.GetString(SaveKey), "Denied navigation must not alter the save.");

            save.RecordLevelResult(1, 77, 0); MvpSave.Save(save);
            ResetSaveCache(); yield return LoadGameScene(value => game = value);
            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR"); yield return new WaitForSecondsRealtime(.5f);
            levelTwo = GameObject.Find("NIVEL 2").GetComponent<Button>();
            Assert.IsFalse(levelTwo.interactable, "Finishing with no star does not pass Level 1, including after reload.");
            var loaded = (MvpSaveData)GetField(game, "saveData");
            int balance = loaded.Management.Balance;
            loaded.RecordLevelResult(1, 130, 1); MvpSave.Save(loaded);
            game.GetType().GetMethod("RefreshLevelCards", flags).Invoke(game, null);
            Assert.IsTrue(levelTwo.interactable, "Completing Level 1 with a star must enable Level 2.");
            Assert.IsFalse(levelTwo.transform.Find("Candado nivel 2").gameObject.activeSelf);
            Assert.IsFalse(levelTwo.transform.Find("Disabled nivel 2").gameObject.activeSelf);
            Assert.IsFalse(GameObject.Find("NIVEL 3").GetComponent<Button>().interactable, "An old unlock flag must not skip Level 2 either.");
            Assert.AreEqual(balance, loaded.Management.Balance);
            TapVisibleButton(levelTwo); yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(2, ReadField<int>(game, "currentLevelNumber"));
            Assert.IsTrue(((GameObject)GetField(game, "introRoot")).activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator Intro_IllustratedCardFitsOrdersAndKeepsNavigationAndSave()
        {
            var save = MvpSaveData.Migrate(new MvpSaveData());
            for (int completed = 1; completed < 6; completed++) save.RecordLevelResult(completed, 130, 1);
            MvpSave.Save(save);
            Component game = null; yield return LoadGameScene(value => game = value);
            yield return new WaitForSecondsRealtime(.6f);
            TapVisibleButton(FindButton("ENTRAR")); yield return new WaitForSecondsRealtime(.5f);
            string persisted = PlayerPrefs.GetString(SaveKey);
            foreach (int level in new[] { 1, 3, 6 })
            {
                TapVisibleButton(FindButton("NIVEL " + level)); yield return new WaitForSecondsRealtime(.5f);
                var popup = GameObject.Find("Popup nivel").GetComponent<RectTransform>();
                var root = (RectTransform)((GameObject)GetField(game, "introRoot")).transform;
                Assert.AreEqual("NIVEL " + level, GameObject.Find("Intro nivel").GetComponent<Text>().text);
                Assert.AreEqual(MvpLevelCatalog.Get(level).Title, GameObject.Find("Intro título").GetComponent<Text>().text);
                Assert.AreEqual(MvpLevelCatalog.Get(level).GuestCount + " COMENSALES", GameObject.Find("Intro comensales").GetComponent<Text>().text);
                Assert.AreEqual("Frame_ProductCard", popup.GetComponent<Image>().sprite.name);
                Assert.IsNull(popup.GetComponent<CanvasGroup>(), "Persistent intro fit must not leave an inactive entrance group blocking CTA taps.");
                Assert.AreEqual("LilitaOne-Regular", GameObject.Find("Intro título").GetComponent<Text>().font.name);
                int diners = 0;
                foreach (Image icon in (Image[])GetField(game, "introGuestIcons"))
                    if (icon.gameObject.activeInHierarchy) { diners++; Assert.AreEqual("GuestPortraitAtlas", icon.sprite.texture.name); Assert.IsFalse(icon.raycastTarget); }
                Assert.AreEqual(MvpLevelCatalog.Get(level).GuestCount, diners);
                foreach (Image icon in (Image[])GetField(game, "introFoodIcons"))
                    if (icon.gameObject.activeInHierarchy) { Assert.IsTrue(icon.preserveAspect); Assert.IsFalse(icon.raycastTarget); Assert.IsNotNull(icon.transform.parent.GetComponent<RectMask2D>()); }
                foreach (int width in new[] { 1080, 720 })
                    CaptureManagementFrame(game, "/tmp/asadito-intro-" + level + "-" + width + ".png", () =>
                    {
                        AssertRectInside(root, popup, "Whole intro must fit the safe-area Canvas");
                        foreach (Text label in popup.GetComponentsInChildren<Text>())
                        {
                            if (string.IsNullOrEmpty(label.text)) continue;
                            Assert.LessOrEqual(label.preferredHeight, label.rectTransform.rect.height + 1f, "Intro label clips: " + label.text);
                            AssertRectInside(popup, label.rectTransform, label.name);
                        }
                        AssertRectInside(popup, FindButton("Volver intro").GetComponent<RectTransform>(), "Back touch target");
                        AssertRectInside(popup, FindButton("IR A LA PARRILLA").GetComponent<RectTransform>(), "Start touch target");
                    }, width, width == 1080 ? 1920 : 1600);
                Assert.AreEqual(persisted, PlayerPrefs.GetString(SaveKey), "Reading the illustrated brief must not modify progression or resources.");
                TapVisibleButton(FindButton("Volver intro")); yield return new WaitForSecondsRealtime(.5f);
                Assert.IsFalse(((GameObject)GetField(game, "introRoot")).activeInHierarchy);
                Assert.IsTrue(((GameObject)GetField(game, "levelSelectRoot")).activeInHierarchy);
                Assert.AreEqual(persisted, PlayerPrefs.GetString(SaveKey), "Backing out of the intro must not start/abandon a run.");
            }
            TapVisibleButton(FindButton("NIVEL 1")); yield return new WaitForSecondsRealtime(.5f);
            int balance = MvpSave.Load().Management.Balance;
            TapVisibleButton(FindButton("IR A LA PARRILLA")); yield return new WaitForSecondsRealtime(.5f);
            Assert.IsFalse(((GameObject)GetField(game, "introRoot")).activeInHierarchy);
            Assert.IsNotNull(GameObject.Find("Management PRÓXIMO ASADO"));
            Assert.AreEqual(balance, MvpSave.Load().Management.Balance);
            Assert.IsNull(MvpSave.Load().Management.ActiveRun);
        }

        [UnityTest]
        public IEnumerator FirstPlayable_RetryResetsFoodAndTable()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 120f);

            yield return EnterLevelOne(game);
            AssertGrillReadyWithoutCoal(game);
            yield return CookAndPlateOrder(game, 35f);
            TapBoard(game);
            yield return new WaitForSecondsRealtime(1.8f);

            Button retry = FindButton("OTRO ASADO");
            Assert.IsTrue(retry.interactable);
            ClickButton(retry);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.IsNotNull(GameObject.Find("Management PRÓXIMO ASADO"));
            yield return BuyAndPrepareCurrentOrder(game);

            GrillHeatModel grill = (GrillHeatModel)GetField(game, "grillHeat");
            Assert.AreEqual(210f, grill.GetTemperatureC(), .001f, "Retry keeps the always-hot grill ready for the next tanda.");
            Assert.AreEqual(0, ReadField<int>(game, "trayCount"));
            Assert.IsNull(GameObject.Find("Management ¡ASADO COMPLETADO!"));
            Array portions = (Array)GetField(game, "portions");
            for (int i = 0; i < portions.Length; i++)
            {
                object portion = portions.GetValue(i);
                Assert.IsFalse(ReadField<bool>(portion, "Started"), "Retry must reset portion state.");
                Assert.IsFalse(ReadField<bool>(portion, "OnTray"), "Retry must clear the tray assignment.");
            }
            Assert.IsNull(GameObject.Find("PRENDER CARBÓN"));
        }

        [UnityTest]
        public IEnumerator FirstPlayable_DebugScaleUsesOnlyConfiguredValuesAndPersists()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            Assert.AreEqual(20f, ReadField<float>(game, "SimulationTimeScale"), .001f,
                "A clean install should start at the tactile 20x simulation scale.");
            yield return EnterLevelOne(game);

            Button scale = FindButton("CONTROL DEBUG");
            string[] expectedLabels = { "DEBUG ×25", "DEBUG ×30", "DEBUG ×35", "DEBUG ×40", "DEBUG ×20" };
            float[] expectedValues = { 25f, 30f, 35f, 40f, 20f };
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
        public IEnumerator FoodPieces_AreVisibleDirectTouchTargetsAndNoFoodNameButtonsRemain()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            yield return EnterLevelOne(game);

            Array portions = (Array)GetField(game, "portions");
            Image[] images = (Image[])GetField(game, "portionImages");
            RectTransform[] targets = (RectTransform[])GetField(game, "portionHitTargets");
            Vector2[] grillFoodSizes = (Vector2[])GetField(game, "portionVisualSizes");
            bool[] rawTrayStacked = (bool[])GetField(game, "rawTrayPortionStacked");
            Assert.AreEqual(portions.Length, images.Length);
            Assert.AreEqual(portions.Length, targets.Length);
            Assert.AreEqual(portions.Length, rawTrayStacked.Length);
            RectTransform rawTray = ((Image)GetField(game, "rawTrayImage")).rectTransform;
            Vector2 rawTrayFoodArea = new Vector2(rawTray.rect.width * .84f, rawTray.rect.height * .71f);
            var rawTrayCenters = new Vector2[portions.Length];
            var rawTrayFoodSizes = new Vector2[portions.Length];
            for (int i = 0; i < portions.Length; i++)
            {
                object portion = portions.GetValue(i);
                string foodId = ((FoodCookProfile)GetField(portion, "Profile")).FoodId;
                Assert.IsTrue(targets[i].gameObject.activeInHierarchy, foodId + " hit target should be available on the raw-food tray.");
                Assert.IsTrue(images[i].gameObject.activeInHierarchy, foodId + " art should be visible before selection.");
                Assert.IsTrue(ReadField<bool>(portion, "OnSourceTray"), foodId + " should start on the aluminum tray.");
                Assert.IsFalse(ReadField<bool>(portion, "Started"), foodId + " should not start cooking before being dragged to the grill.");
                Assert.That(Vector2.Distance(images[i].rectTransform.sizeDelta, grillFoodSizes[i]), Is.LessThan(.01f),
                    foodId + " should render at its grill size while resting on the source tray.");
                Assert.IsTrue(RectTransformUtility.RectangleContainsScreenPoint(rawTray, targets[i].position, null),
                    foodId + " hit target should sit inside the aluminum tray.");
                Vector3 localCenter = rawTray.InverseTransformPoint(targets[i].position);
                rawTrayCenters[i] = new Vector2(localCenter.x, localCenter.y);
                rawTrayFoodSizes[i] = images[i].rectTransform.sizeDelta;
                Assert.IsTrue(FoodFootprintLayout.FitsInside(rawTrayFoodArea, rawTrayFoodSizes[i], rawTrayCenters[i]),
                    foodId + " food art, including stacked layers, must stay inside the aluminum tray's flat center.");
                Assert.IsNotNull(FindComponent(targets[i].gameObject, "Asadito.Runtime.FoodPieceTouch"));
                Assert.IsNull(GameObject.Find(FoodCatalog.Get(foodId).DisplayName),
                    foodId + " must not be presented as a separate food-name button.");
            }
            for (int i = 0; i < portions.Length; i++)
                for (int j = i + 1; j < portions.Length; j++)
                {
                    bool overlap = FoodFootprintLayout.Overlaps(rawTrayCenters[i], rawTrayFoodSizes[i],
                        rawTrayCenters[j], rawTrayFoodSizes[j]);
                    if (overlap)
                        Assert.IsTrue(rawTrayStacked[i] || rawTrayStacked[j],
                            "Food art may overlap only when the layout explicitly marks a layer as intentionally stacked.");
                    if (!rawTrayStacked[i] && !rawTrayStacked[j])
                        Assert.IsFalse(overlap, "Food pieces that fit must remain separate on the raw tray.");
                }

            Assert.IsFalse(rawTrayStacked[0] || rawTrayStacked[1],
                "Level 1's tira and chorizo should be laid out separately; the aluminum tray has room for both footprints.");
            Vector2 foodBoundsMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 foodBoundsMax = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < portions.Length; i++)
            {
                foodBoundsMin = Vector2.Min(foodBoundsMin, rawTrayCenters[i] - rawTrayFoodSizes[i] * .5f);
                foodBoundsMax = Vector2.Max(foodBoundsMax, rawTrayCenters[i] + rawTrayFoodSizes[i] * .5f);
            }
            Assert.That(Vector2.Distance((foodBoundsMin + foodBoundsMax) * .5f, Vector2.zero), Is.LessThan(.01f),
                "When all portions fit, their overall footprint should be centered on the tray instead of crowded to one side.");

            Assert.AreEqual(Debug.isDebugBuild, GameObject.Find("CONTROL DEBUG") != null,
                "Only Editor/development builds may expose the simulation debug control.");

            AssertGrillReadyWithoutCoal(game);
            Image rawSurface = (Image)GetField(game, "rawTrayImage");
            Image boardSurface = (Image)GetField(game, "servingBoardImage");
            Assert.IsTrue(rawSurface.gameObject.activeInHierarchy, "Raw food starts with the aluminum tray visible.");
            Assert.IsFalse(boardSurface.gameObject.activeInHierarchy, "The serving board stays hidden until the source tray is empty.");
            Sprite rawTraySprite = (Sprite)GetField(game, "aluminumTraySprite");
            Assert.NotNull(rawTraySprite);
            Assert.AreEqual("Raw Aluminum Tray", rawTraySprite.name);
            Assert.IsNull(GameObject.Find("Pinza parrillera ilustrada"), "The tong illustration is intentionally removed for this MVP interaction.");
            TapFoodPiece(game, 0, 41);
            Assert.IsFalse(ReadField<bool>(portions.GetValue(0), "Started"), "A raw tray piece must be dragged to the grill before cooking.");
            Vector2 sourceTraySize = images[0].rectTransform.sizeDelta;
            DragRawFoodToGrill(game, 0, 42);
            Assert.IsTrue(ReadField<bool>(portions.GetValue(0), "Started"), "Dragging the raw food to the grill must start it.");
            Assert.That(Vector2.Distance(images[0].rectTransform.sizeDelta, sourceTraySize), Is.LessThan(.01f),
                "Moving meat from aluminum tray to grill must not resize its visual.");
            Assert.AreEqual(0, ReadField<int>(game, "activePortion"), "The newly placed piece remains selected during loading.");
            Assert.IsTrue(ReadField<bool>(game, "cooking"), "A portion starts cooking as soon as it reaches the grill.");
            Assert.IsTrue(rawSurface.gameObject.activeInHierarchy);
            Assert.IsFalse(boardSurface.gameObject.activeInHierarchy);
            Vector2 waitingPosition = ReadField<Vector2>(portions.GetValue(0), "Position");
            DragActiveFoodToDifferentGrillPosition(game, 0, 45);
            Assert.Greater(Vector2.Distance(waitingPosition, ReadField<Vector2>(portions.GetValue(0), "Position")), .1f,
                "A loaded piece can still be repositioned while the remaining raw order is transferred.");
            yield return new WaitForSecondsRealtime(.3f);
            float firstGrilledTemperature = ((FoodState)GetField(portions.GetValue(0), "State")).CoreTemperatureC;
            Assert.Greater(firstGrilledTemperature, 20f, "A grilled portion must cook even while raw food remains on the source tray.");
            TapFoodPiece(game, 1, 43);
            Assert.IsFalse(ReadField<bool>(portions.GetValue(1), "Started"), "Tapping raw food must not transfer it to the grill.");
            DragRawFoodToGrill(game, 1, 44);
            Assert.IsTrue(ReadField<bool>(portions.GetValue(1), "Started"));
            Assert.IsFalse(rawSurface.gameObject.activeInHierarchy, "The aluminum tray disappears after its last piece leaves.");
            Assert.IsTrue(boardSurface.gameObject.activeInHierarchy, "The board replaces the tray in the same spot.");
            Assert.AreEqual(1, ReadField<int>(game, "activePortion"));
            Assert.IsTrue((bool)game.GetType().GetMethod("SelectFoodPiece").Invoke(game, new object[] { 0 }),
                "Selecting another cooking portion must not lock input to a single active piece.");
            Assert.IsTrue((bool)game.GetType().GetMethod("SelectFoodPiece").Invoke(game, new object[] { 1 }));
            yield return new WaitForSecondsRealtime(.3f);
            Assert.Greater(((FoodState)GetField(portions.GetValue(1), "State")).CoreTemperatureC, 20f,
                "The newly placed piece must start heating immediately.");
            Assert.Greater(((FoodState)GetField(portions.GetValue(0), "State")).CoreTemperatureC, firstGrilledTemperature,
                "The previously placed piece continues cooking while another piece is added.");
            Assert.That(Vector3.Distance(images[0].rectTransform.localScale, Vector3.one), Is.LessThan(.001f),
                "Selection feedback must not permanently enlarge food on the grill.");
            Assert.IsFalse((bool)game.GetType().GetMethod("SelectFoodPiece").Invoke(game, new object[] { -1 }),
                "Out-of-range/empty selections must be harmless.");
        }

        [UnityTest]
        public IEnumerator FoodCookingBars_AreThinFoodWidthIndependentAndNeverBlockDrag()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);yield return EnterLevelOne(game);
            SetField(game,"SimulationTimeScale",0f);
            var tracks=(Image[])GetField(game,"portionCookTracks");var fills=(Image[])GetField(game,"portionCookFills");
            var images=(Image[])GetField(game,"portionImages");var portions=(Array)GetField(game,"portions");
            Assert.AreEqual(portions.Length,tracks.Length);Assert.AreEqual(portions.Length,fills.Length);
            foreach(var track in tracks)Assert.IsFalse(track.gameObject.activeSelf,"Bars appear only while on the grill, not cluttering storage");
            LoadAllRawFoodToGrill(game,0);yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();
            for(int i=0;i<tracks.Length;i++)
            {
                Assert.IsTrue(tracks[i].gameObject.activeInHierarchy);Assert.IsFalse(tracks[i].raycastTarget);Assert.IsFalse(fills[i].raycastTarget);
                Assert.AreSame(images[i].transform,tracks[i].transform.parent);
                Assert.AreEqual(images[i].rectTransform.rect.width,tracks[i].rectTransform.rect.width,.01f);
                Assert.AreEqual(8f,tracks[i].rectTransform.rect.height,.01f);Assert.AreEqual(6f,fills[i].rectTransform.rect.height,.01f);
                Assert.AreEqual(.025f,fills[i].fillAmount,.001f);Assert.AreEqual(FoodCookingProgress.Red,fills[i].color);
            }
            // Fixture-controlled states verify individual color windows, without changing tuning/time.
            FoodState first=(FoodState)GetField(portions.GetValue(0),"State"),second=(FoodState)GetField(portions.GetValue(1),"State");
            var secondProfile=(FoodCookProfile)GetField(portions.GetValue(1),"Profile");
            var band=FindDonenessBand(secondProfile,Doneness.A_Punto);
            second.CoreTemperatureC=(band.MinimumCoreC+band.MaximumCoreC)*.5f;
            second.SetCurrentFace(new FoodFaceState{SurfaceTemperatureC=160f,Maillard=.2f});yield return null;
            Assert.AreEqual(FoodCookingProgress.Green,fills[1].color);Assert.AreEqual(FoodCookingProgress.Red,fills[0].color);
            Action portraitPositions=()=>{
                var place=game.GetType().GetMethod("SetFoodTargetPosition",BindingFlags.Instance|BindingFlags.NonPublic);
                place.Invoke(game,new object[]{0,new Vector2(.46f,.62f)});place.Invoke(game,new object[]{1,new Vector2(.46f,.36f)});
            };
            CaptureManagementFrame(game,"/tmp/asadito-food-bars-red-green.png",portraitPositions);
            first.SetCurrentFace(new FoodFaceState{SurfaceTemperatureC=210f,Char=.8f,Maillard=.4f});yield return null;
            Assert.AreEqual(1f,fills[0].fillAmount);Assert.AreEqual(FoodCookingProgress.Red,fills[0].color);
            Assert.AreEqual(FoodCookingProgress.Green,fills[1].color);
            CaptureManagementFrame(game,"/tmp/asadito-food-bars-burnt-green.png",portraitPositions);
            // Progress is driven by both real food states, never the selected piece or a wall clock.
            foreach(object portion in portions)
            {
                var state=(FoodState)GetField(portion,"State");state.Reset();state.CoreTemperatureC=20f;
                state.SetCurrentFace(new FoodFaceState{SurfaceTemperatureC=20f});
            }
            SetField(game,"SimulationTimeScale",120f);yield return new WaitForSecondsRealtime(3f);
            Assert.Greater(first.CoreTemperatureC,20f);Assert.Greater(second.CoreTemperatureC,20f);
            Assert.Greater(fills[0].fillAmount,.025f);Assert.Greater(fills[1].fillAmount,.025f);
            SetField(game,"SimulationTimeScale",0f);yield return null;
            game.GetType().GetMethod("PauseGame",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            float a=fills[0].fillAmount,b=fills[1].fillAmount;yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(a,fills[0].fillAmount);Assert.AreEqual(b,fills[1].fillAmount);
            game.GetType().GetMethod("ContinueGame",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            DragActiveFoodToDifferentGrillPosition(game,0,980);yield return new WaitForSecondsRealtime(.3f);
            Assert.AreEqual(images[0].rectTransform.rect.width,tracks[0].rectTransform.rect.width,.01f,"Gauge follows the dragged food without changing size");
            DragFoodToBoard(game,0,981);yield return new WaitForSecondsRealtime(.55f);
            Assert.IsTrue(ReadField<bool>(portions.GetValue(0),"OnTray"));Assert.IsFalse(tracks[0].gameObject.activeSelf);
            Assert.IsTrue(tracks[1].gameObject.activeInHierarchy,"Remaining cut retains its own gauge");
            game.GetType().GetMethod("RefreshOrder",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);yield return null;
            foreach(var track in tracks)Assert.IsFalse(track.gameObject.activeSelf);
            foreach(var fill in fills){Assert.AreEqual(.025f,fill.fillAmount,.001f);Assert.AreEqual(FoodCookingProgress.Red,fill.color);}
        }

        [UnityTest]
        public IEnumerator MobilePause_ResumeRestartLevelsAndPersistSoundAndHaptics()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            yield return EnterLevelOne(game);
            DragRawFoodToGrill(game, 0, 51);
            AssertGrillReadyWithoutCoal(game, false);

            Assert.IsNull(GameObject.Find("PAUSA"), "The gameplay HUD should no longer show the Pause button.");
            game.GetType().GetMethod("PauseGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(GameObject.Find("Pausa").activeInHierarchy);
            Assert.IsTrue(FindButton("CONTINUAR").interactable);
            ClickButton("SONIDO");
            ClickButton("VIBRACIÓN");
            Assert.AreEqual(0f, MvpSave.Load().Settings.SfxVolume);
            Assert.IsFalse(MvpSave.Load().Settings.HapticsEnabled);
            Assert.That(FindButton("SONIDO").GetComponentInChildren<Text>().text, Does.Contain("OFF"));
            Assert.That(FindButton("VIBRACIÓN").GetComponentInChildren<Text>().text, Does.Contain("OFF"));

            ClickButton("REINICIAR NIVEL");
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(((GameObject)GetField(game, "pauseRoot")).activeInHierarchy);
            Assert.AreEqual(210f, ((GrillHeatModel)GetField(game, "grillHeat")).GetTemperatureC());
            Assert.IsFalse(ReadField<bool>(((Array)GetField(game, "portions")).GetValue(0), "Started"));
            Assert.AreEqual(0f, MvpSave.Load().Settings.SfxVolume, "Restart must preserve settings.");
            Assert.IsFalse(MvpSave.Load().Settings.HapticsEnabled, "Restart must preserve haptics preference.");

            game.GetType().GetMethod("PauseGame", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            ClickButton("VOLVER A NIVELES");
            yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.NotNull(GameObject.Find("Seleccion de nivel"), "Pause should return to the level selector.");
        }

        [UnityTest]
        public IEnumerator GameplayBackButton_ReturnsToLevelSelectorAndStopsCooking()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            yield return EnterLevelOne(game);

            DragRawFoodToGrill(game, 0, 81);
            DragRawFoodToGrill(game, 1, 82);
            Assert.IsTrue(ReadField<bool>(game, "cooking"));

            Button backButton = FindButton("VOLVER");
            Assert.AreEqual("VOLVER", backButton.GetComponentInChildren<Text>().text);
            Assert.AreEqual("Asadito UI Icon Back",
                backButton.transform.Find("Icono accion VOLVER").GetComponent<Image>().sprite.name,
                "The gameplay navigation control must show a clear left arrow.");
            Assert.AreEqual(((Transform)GetField(game, "gameplayRoot")).transform, backButton.transform.parent);
            Assert.IsNull(GameObject.Find("PAUSA"), "Gameplay should keep the removed Pause button absent.");

            ClickButton(backButton);
            yield return new WaitForSecondsRealtime(.5f);
            Assert.IsNotNull(GameObject.Find("Seleccion de nivel"), "Back should return to the level selector.");
            Assert.IsFalse(((CanvasGroup)GetField(game, "gameplayCanvasGroup")).blocksRaycasts);
            Assert.IsFalse(ReadField<bool>(game, "cooking"), "Leaving gameplay must stop the active cooking state.");
            Assert.AreEqual(-1, ReadField<int>(game, "activePortion"));
        }

        [UnityTest]
        public IEnumerator FoodPieceDrag_CanReachTrayAndMultiTouchCannotMoveTwoPieces()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            yield return EnterLevelOne(game);
            AssertGrillReadyWithoutCoal(game);

            RectTransform[] targets = (RectTransform[])GetField(game, "portionHitTargets");
            Component firstTouch = FindComponent(targets[0].gameObject, "Asadito.Runtime.FoodPieceTouch");
            Component secondTouch = FindComponent(targets[1].gameObject, "Asadito.Runtime.FoodPieceTouch");
            PointerEventData firstPointer = MakePointer(targets[0], 61);
            PointerEventData secondPointer = MakePointer(targets[1], 62);
            Assert.IsTrue(ExecuteEvents.Execute(firstTouch.gameObject, firstPointer, ExecuteEvents.pointerDownHandler));
            Assert.IsTrue(ExecuteEvents.Execute(secondTouch.gameObject, secondPointer, ExecuteEvents.pointerDownHandler));
            Assert.IsTrue(ExecuteEvents.Execute(secondTouch.gameObject, secondPointer, ExecuteEvents.pointerUpHandler));
            Assert.IsTrue(ExecuteEvents.Execute(firstTouch.gameObject, firstPointer, ExecuteEvents.pointerUpHandler));
            Assert.AreEqual(-1, ReadField<int>(game, "activePortion"), "Raw tray taps must not select or start a piece.");
            Assert.IsFalse(ReadField<bool>(((Array)GetField(game, "portions")).GetValue(1), "Started"));

            DragRawFoodToGrill(game, 0, 63);
            DragRawFoodToGrill(game, 1, 64);
            RectTransform trayDrop = (RectTransform)GetField(game, "trayDropRect");
            Vector2 trayPoint = RectTransformUtility.WorldToScreenPoint(null, trayDrop.position);
            DragFoodPiece(game, 1, trayPoint, 65);
            yield return new WaitForSecondsRealtime(.55f);
            Assert.IsTrue(ReadField<bool>(((Array)GetField(game, "portions")).GetValue(1), "OnTray"),
                "Dragging a selected food piece to the tray must plate that piece.");
            Assert.AreEqual(1, ReadField<int>(game, "trayCount"));
            Assert.That(targets[1].localScale.x, Is.EqualTo(1f).Within(.001f),
                "Plated meat must preserve its exact grill/source size instead of shrinking into a cell.");
        }

        [UnityTest]
        public IEnumerator RemainingChorizo_IsRaycastableAndDraggableAfterTiraMovesToGrill()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            yield return EnterLevelOne(game);

            RectTransform[] targets = (RectTransform[])GetField(game, "portionHitTargets");
            DragRawFoodToGrill(game, 0, 70);
            yield return null;
            RectTransform auxiliaryTable = ((Image)GetField(game, "auxiliaryTableImage")).rectTransform;
            Assert.That(auxiliaryTable.anchorMin.x, Is.EqualTo(.78f).Within(.001f),
                "The tray should be inset from the right edge so the sausage and its touch area stay reachable on narrow phones.");

            Vector2 sourcePoint = RectTransformUtility.WorldToScreenPoint(null, targets[1].position);
            Assert.IsTrue((bool)game.GetType().GetMethod("IsFoodTargetClosest").Invoke(game,
                    new object[] { 1, sourcePoint, null }),
                "The remaining chorizo must remain the closest valid touch target on the tray.");

            var pointer = new PointerEventData(EventSystem.current)
            {
                pointerId = 71,
                button = PointerEventData.InputButton.Left,
                position = sourcePoint
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.IsTrue(hits.Exists(hit => hit.gameObject.transform == targets[1] ||
                hit.gameObject.transform.IsChildOf(targets[1])),
                "Unity's real UI raycast must reach the remaining chorizo, not just direct test callbacks.");

            int foodHit = hits.FindIndex(hit => hit.gameObject.transform == targets[1] ||
                hit.gameObject.transform.IsChildOf(targets[1]));
            GameObject hitObject = hits[foodHit].gameObject;
            Assert.IsTrue(ExecuteEvents.ExecuteHierarchy(hitObject, pointer, ExecuteEvents.pointerDownHandler));
            Assert.IsTrue(ExecuteEvents.ExecuteHierarchy(hitObject, pointer, ExecuteEvents.beginDragHandler));
            RectTransform grill = (RectTransform)GetField(game, "grillAreaRect");
            Vector2 grillPoint = RectTransformUtility.WorldToScreenPoint(null, grill.position);
            pointer.position = grillPoint;
            Assert.IsTrue(ExecuteEvents.ExecuteHierarchy(hitObject, pointer, ExecuteEvents.dragHandler));
            Assert.IsTrue(ExecuteEvents.ExecuteHierarchy(hitObject, pointer, ExecuteEvents.pointerUpHandler));
            Assert.IsTrue(ExecuteEvents.ExecuteHierarchy(hitObject, pointer, ExecuteEvents.endDragHandler));
            Assert.IsTrue(ReadField<bool>(((Array)GetField(game, "portions")).GetValue(1), "Started"),
                "Dragging the chorizo from its tray target onto the grill should load it.");
        }

        [UnityTest]
        public IEnumerator AluminumTrayDrag_StartsCooking_AndBoardTapServes()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            yield return EnterLevelOne(game);

            AssertGrillReadyWithoutCoal(game);
            Sprite boardSprite = (Sprite)GetField(game, "servingBoardSprite");
            Sprite tableSprite = (Sprite)GetField(game, "auxiliaryTableSprite");
            Sprite aluminumTray = (Sprite)GetField(game, "aluminumTraySprite");
            Assert.NotNull(boardSprite);
            Assert.NotNull(tableSprite);
            Assert.NotNull(aluminumTray);
            Assert.AreEqual("TablaAsador_0", boardSprite.name);
            Assert.AreEqual("Raw Aluminum Tray", aluminumTray.name);
            Assert.AreSame(boardSprite, ((Image)GetField(game, "servingBoardImage")).sprite);
            Image tableImage = (Image)GetField(game, "auxiliaryTableImage");
            Assert.AreSame(tableSprite, tableImage.sprite);
            Assert.AreSame(tableImage.transform, ((Image)GetField(game, "servingBoardImage")).transform.parent,
                "The interactive board must be a distinct object layered on the auxiliary table.");
            RectTransform rawTrayRect = ((Image)GetField(game, "rawTrayImage")).rectTransform;
            RectTransform boardRect = ((Image)GetField(game, "servingBoardImage")).rectTransform;
            RectTransform tableRect = tableImage.rectTransform;
            Assert.That(Vector2.Distance(boardRect.rect.size, rawTrayRect.rect.size), Is.LessThan(.01f),
                "The board and aluminum tray must use exactly equal dimensions.");
            Assert.That(Vector2.Distance(boardRect.anchoredPosition, rawTrayRect.anchoredPosition), Is.LessThan(.01f),
                "The board must appear exactly where the aluminum tray was, not beside it.");
            Assert.That(Vector2.Distance(boardRect.anchorMin, rawTrayRect.anchorMin), Is.LessThan(.001f));
            Assert.That(Vector2.Distance(boardRect.anchorMax, rawTrayRect.anchorMax), Is.LessThan(.001f));
            Assert.That(boardRect.rect.width / boardRect.rect.height,
                Is.EqualTo(boardSprite.rect.width / boardSprite.rect.height).Within(.01f),
                "The serving board must not be stretched to match the aluminum tray's shape.");
            Assert.That((float)aluminumTray.rect.width / aluminumTray.rect.height,
                Is.EqualTo(boardRect.rect.width / boardRect.rect.height).Within(.01f),
                "The source tray artwork must have the same native proportions as the board.");
            Assert.IsTrue(tableImage.preserveAspect,
                "The auxiliary table illustration must keep its source aspect ratio.");
            Assert.That(Vector3.Distance(rawTrayRect.position, boardRect.position), Is.LessThan(.01f),
                "The surfaces occupy the same visual slot and swap visibility.");
            Assert.IsTrue(((Image)GetField(game, "rawTrayImage")).gameObject.activeInHierarchy);
            Assert.IsFalse(((Image)GetField(game, "servingBoardImage")).gameObject.activeInHierarchy);
            float sharedServingScale = ReadField<float>(game, "servingBoardPortionScale");
            Assert.That(sharedServingScale, Is.EqualTo(1f).Within(.001f),
                "Food scale on the serving board must be exactly the same as on the grill and raw tray.");
            Assert.NotNull(FindComponent(((Image)GetField(game, "servingBoardImage")).gameObject,
                "Asadito.Runtime.ServingBoardTouch"));
            Assert.IsNull(GameObject.Find("DAR VUELTA"));
            Assert.IsNull(GameObject.Find("BANDEJA"));
            Assert.IsNull(GameObject.Find("SERVIR"));
            game.GetType().GetMethod("OnServingBoardTap").Invoke(game, null);
            Assert.AreEqual(0, ReadField<int>(game, "trayCount"),
                "A double tap on an incomplete physical board must not bypass cooking/serving flow.");

            Array portions = (Array)GetField(game, "portions");
            object portion = portions.GetValue(0);
            FoodState state = (FoodState)GetField(portion, "State");
            FoodState firstGrilledState = (FoodState)GetField(portions.GetValue(1), "State");
            float initialCore = state.CoreTemperatureC;
            TapFoodPiece(game, 0, 71);
            Assert.IsFalse(ReadField<bool>(portion, "Started"), "Tapping raw meat leaves it on the source tray.");
            DragRawFoodToGrill(game, 1, 72);
            Assert.IsTrue(((Image)GetField(game, "rawTrayImage")).gameObject.activeInHierarchy,
                "The source tray must stay while it still contains raw food.");
            Assert.IsFalse(((Image)GetField(game, "servingBoardImage")).gameObject.activeInHierarchy,
                "The serving board cannot replace a tray that still holds food.");
            Assert.That(state.CoreTemperatureC, Is.EqualTo(initialCore).Within(.01f),
                "Raw food left on the source tray must not cook.");
            yield return new WaitForSecondsRealtime(.3f);
            float firstGrilledTemperature = firstGrilledState.CoreTemperatureC;
            Assert.Greater(firstGrilledTemperature, 20f,
                "A piece already on the grill cooks while the raw order is still being transferred.");
            DragRawFoodToGrill(game, 0, 73);
            Assert.IsTrue(ReadField<bool>(portion, "Started"));
            Assert.IsFalse(((Image)GetField(game, "rawTrayImage")).gameObject.activeInHierarchy);
            Assert.IsTrue(((Image)GetField(game, "servingBoardImage")).gameObject.activeInHierarchy);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.Greater(state.CoreTemperatureC, initialCore, "Food must heat directly without an ignition step.");
            Assert.Greater(firstGrilledState.CoreTemperatureC, firstGrilledTemperature,
                "The first cut continues cooking concurrently after a second cut joins it.");
            DragFoodToBoard(game, 0, 74);
            yield return new WaitForSecondsRealtime(.55f);
            Assert.IsTrue(ReadField<bool>(portion, "OnTray"), "Dragging cooked food onto the board plates it.");
            RectTransform[] targets = (RectTransform[])GetField(game, "portionHitTargets");
            Assert.That(targets[0].localScale.x, Is.EqualTo(1f).Within(.001f),
                "Plating preserves the exact grill scale, rather than a board fit scale.");
            Assert.IsTrue((bool)game.GetType().GetMethod("SelectFoodPiece").Invoke(game, new object[] { 1 }));
            DragFoodToBoard(game, 1, 75);
            yield return new WaitForSecondsRealtime(.55f);
            Assert.That(targets[1].localScale.x, Is.EqualTo(1f).Within(.001f),
                "Every cut on the board keeps the same multiplier, regardless of sprite aspect.");
            RectTransform servingBoard = ((Image)GetField(game, "servingBoardImage")).rectTransform;
            Image[] platedImages = (Image[])GetField(game, "portionImages");
            Vector2 boardFoodArea = new Vector2(servingBoard.rect.width * .74f, servingBoard.rect.height * .56f);
            Vector2 boardFoodAreaCenter = new Vector2((.45f - .5f) * servingBoard.rect.width,
                (.505f - .5f) * servingBoard.rect.height);
            for (int i = 0; i < targets.Length; i++)
            {
                Vector3 boardLocalCenter3 = servingBoard.InverseTransformPoint(targets[i].position);
                Vector2 boardLocalCenter = new Vector2(boardLocalCenter3.x, boardLocalCenter3.y) - boardFoodAreaCenter;
                Vector2 displayedSize = platedImages[i].rectTransform.rect.size * targets[i].localScale.x;
                Assert.IsTrue(FoodFootprintLayout.FitsInside(boardFoodArea, displayedSize, boardLocalCenter),
                    targets[i].name + " food art, including stacked layers, must stay on the board's flat cutting surface.");
            }
            Assert.Greater(targets[1].GetSiblingIndex(), targets[0].GetSiblingIndex(),
                "When food footprints collide on the board, the portion plated last must render above the earlier cut.");
            Assert.IsTrue(CanServeFromBoard(game));
            TapBoard(game);
            Assert.IsTrue(ReadField<bool>(game, "servingLocked"), "A single tap on the ready board serves the order.");
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
            RectTransform levelOneRect = levelOne.GetComponent<RectTransform>();
            RectTransform levelTwoRect = levelTwo.GetComponent<RectTransform>();
            float levelContentWidth = levelOneRect.parent.GetComponent<RectTransform>().rect.width;
            RectTransform selectorTitleRect = GameObject.Find("Seleccion titulo").GetComponent<RectTransform>();
            Vector3[] titleCorners = new Vector3[4];
            Vector3[] firstCardCorners = new Vector3[4];
            selectorTitleRect.GetWorldCorners(titleCorners);
            levelOneRect.GetWorldCorners(firstCardCorners);
            float titleToCardGap = titleCorners[0].y - firstCardCorners[1].y;
            Assert.GreaterOrEqual(titleToCardGap, -1f, "The first row should not overlap the selector title.");
            Assert.LessOrEqual(titleToCardGap, 40f, "The first row should sit closer to the title instead of leaving a large empty band above the images.");
            float levelCardGap = levelTwoRect.anchorMin.x * levelContentWidth + levelTwoRect.anchoredPosition.x - levelTwoRect.rect.width * .5f
                - (levelOneRect.anchorMax.x * levelContentWidth + levelOneRect.anchoredPosition.x + levelOneRect.rect.width * .5f);
            Assert.AreEqual(390f, levelOneRect.rect.width, .01f, "Narrower cards leave a visible center gutter.");
            Assert.GreaterOrEqual(levelCardGap, 90f, "The odd/even columns need a clearly visible gap.");
            Assert.IsFalse(levelOne.transform.Find("Candado nivel 1").gameObject.activeSelf);
            Assert.IsFalse(levelOne.transform.Find("Disabled nivel 1").gameObject.activeSelf);
            Assert.IsTrue(levelTwo.transform.Find("Candado nivel 2").gameObject.activeSelf);
            Assert.IsTrue(levelTwo.transform.Find("Disabled nivel 2").gameObject.activeSelf);
            Component levelGame = game;
            var levelSave = (MvpSaveData)GetField(levelGame, "saveData");
            levelSave.RecordLevelResult(1, 130, 1);
            levelGame.GetType().GetMethod("RefreshLevelCards", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(levelGame, null);
            Assert.IsTrue(levelTwo.interactable, "Persisted progression unlocks the next Chapter One asado.");
            Assert.IsFalse(levelTwo.transform.Find("Candado nivel 2").gameObject.activeSelf);
            Assert.IsFalse(levelTwo.transform.Find("Disabled nivel 2").gameObject.activeSelf);
            Assert.IsNull(GameObject.Find("Seleccion ayuda"),
                "The level-selection screen should not add a secondary block of descriptive copy.");
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            Assert.AreEqual("tira_ideal", GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<Image>().sprite.name);
            Assert.AreEqual("chorizo_ideal", GameObject.Find("Icon comida intro CHORIZO").GetComponent<Image>().sprite.name);
            Transform introPopup = GameObject.Find("Popup nivel").transform;
            Transform guestIconOne = introPopup.Find("Icon comensal intro 1");
            Transform guestIconTwo = introPopup.Find("Icon comensal intro 2");
            Assert.IsNotNull(guestIconOne);
            Assert.IsNotNull(guestIconTwo);
            Assert.IsTrue(guestIconOne.gameObject.activeSelf);
            Assert.IsTrue(guestIconTwo.gameObject.activeSelf,
                "The intro must show one diner icon for each of the two diners.");
            Assert.IsFalse(introPopup.Find("Icon comensal intro 3").gameObject.activeSelf,
                "Diner icons must match the selected level's guest count.");
            Assert.AreEqual("ana_neutral", guestIconOne.GetComponent<Image>().sprite.name);
            Assert.AreEqual("tito_neutral", guestIconTwo.GetComponent<Image>().sprite.name);
            var firstGuestCorners = new Vector3[4];
            var secondGuestCorners = new Vector3[4];
            var guestLabelCorners = new Vector3[4];
            guestIconOne.GetComponent<RectTransform>().GetWorldCorners(firstGuestCorners);
            guestIconTwo.GetComponent<RectTransform>().GetWorldCorners(secondGuestCorners);
            GameObject.Find("Intro comensales").GetComponent<RectTransform>().GetWorldCorners(guestLabelCorners);
            Assert.Less(firstGuestCorners[3].x, secondGuestCorners[0].x, "The two diner icons must not overlap each other.");
            Assert.Less(secondGuestCorners[3].x, guestLabelCorners[0].x, "Diner icons must remain separate from their count label.");
            Assert.IsNull(GameObject.Find("Intro objetivo"),
                "The drag/cook instructions should be removed from the level intro for now.");
            RectTransform introPopupRect = introPopup.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(720f, 780f), introPopupRect.rect.size,
                "The painted intro stays compact and fits as a single authored composition.");
            Assert.AreEqual(1f, introPopup.GetComponent<Image>().color.a, .001f);
            Assert.AreEqual("Frame_ProductCard", introPopup.GetComponent<Image>().sprite.name);
            Assert.AreEqual(Image.Type.Sliced, introPopup.GetComponent<Image>().type);
            Assert.IsNull(introPopup.GetComponent<Outline>(), "The painted frame must not retain a glass rim.");
            Assert.IsNull(introPopup.Find("Reflejo vidrio popup"));
            Assert.NotNull(introPopup.Find("Cartel verde intro"));
            Assert.AreEqual("NIVEL 1", GameObject.Find("Intro nivel").GetComponent<Text>().text);
            Assert.AreEqual("EL DEBUT", GameObject.Find("Intro título").GetComponent<Text>().text);
            Assert.AreEqual(introPopup, GameObject.Find("Intro título").transform.parent);
            Assert.AreEqual(introPopup, GameObject.Find("IR A LA PARRILLA").transform.parent);
            Text popupTitle = GameObject.Find("Intro título").GetComponent<Text>();
            Text popupGuests = GameObject.Find("Intro comensales").GetComponent<Text>();
            Text popupMenu = GameObject.Find("Intro menu").GetComponent<Text>();
            Assert.GreaterOrEqual(popupTitle.fontSize, 48, "The compact title must remain readable on a phone.");
            Assert.GreaterOrEqual(popupGuests.fontSize, 32, "The guest count must remain comfortably legible.");
            Assert.GreaterOrEqual(popupMenu.fontSize, 26, "Order copy must not be tiny.");
            Assert.LessOrEqual(popupTitle.preferredHeight, popupTitle.rectTransform.rect.height + 1f, "The level title must fit without clipping.");
            Assert.LessOrEqual(popupGuests.preferredHeight, popupGuests.rectTransform.rect.height + 1f, "The guest count must fit without clipping.");
            Assert.LessOrEqual(popupMenu.preferredHeight, popupMenu.rectTransform.rect.height + 1f, "The order must fit without clipping.");
            Assert.AreEqual(new Vector2(176f, 112f), GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<RectTransform>().sizeDelta,
                "Food illustrations should stay distinct while fitting the compact card.");
            Button introStart = FindButton("IR A LA PARRILLA");
            Assert.AreEqual(new Vector2(520f, 88f), introStart.GetComponent<RectTransform>().rect.size,
                "The primary action should fit the compact popup without dominating it.");
            AssertRectInside(introPopupRect, popupTitle.rectTransform, "Title");
            AssertRectInside(introPopupRect, popupGuests.rectTransform, "Guest count");
            AssertRectInside(introPopupRect, popupMenu.rectTransform, "Order");
            AssertRectInside(introPopupRect, guestIconOne.GetComponent<RectTransform>(), "First guest icon");
            AssertRectInside(introPopupRect, guestIconTwo.GetComponent<RectTransform>(), "Second guest icon");
            AssertRectInside(introPopupRect, introStart.GetComponent<RectTransform>(), "Primary action");
            AssertRectInside(introPopupRect, GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<RectTransform>(), "Food icon");
            Assert.AreEqual("Asadito UI Icon Next", introStart.transform.Find("Icono accion IR A LA PARRILLA").GetComponent<Image>().sprite.name);
            Assert.AreEqual("Arcade Gold Button", ((Image)introStart.targetGraphic).sprite.name,
                "The painted intro preserves the app's current arcade CTA button styling.");
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
        public IEnumerator FirstPlayable_L1CompletesAtMeasured20xWithoutCookingTimerShortcuts()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            Assert.AreEqual(20f, ReadField<float>(game, "SimulationTimeScale"), .001f,
                "L1 duration validation must use the measured default simulation scale.");

            DateTime started = DateTime.UtcNow;
            yield return EnterLevelOne(game);
            AssertGrillReadyWithoutCoal(game);
            yield return CookAndPlateOrder(game, 120f);
            Assert.IsTrue(CanServeFromBoard(game));
            TapBoard(game);
            yield return new WaitForSecondsRealtime(2f);
            Assert.NotNull(GameObject.Find("Management ¡ASADO COMPLETADO!"));

            double elapsedSeconds = (DateTime.UtcNow - started).TotalSeconds;
            Assert.LessOrEqual(elapsedSeconds, 180d, "Automated L1 should remain under the 2–3 minute First Playable tuning ceiling at 20x.");
            Assert.Greater(ReadField<float>(game, "SimulationTimeScale"), 0f);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Mvp_TutorialProgressIsSavedAndUnlocksNextAsado()
        {
            Component game=null;yield return LoadGameScene(value=>game=value);
            SetField(game,"SimulationTimeScale",120f);
            yield return EnterLevelOne(game);
            for(int level=1;level<=6;level++)
            {
                Assert.AreEqual(level,ReadField<int>(game,"currentLevelNumber"));
                Assert.IsNotNull(MvpSave.Load().Management.ActiveRun,"Every level must use paid preparation.");
                int balance=MvpSave.Load().Management.Balance;
                yield return CookAndPlateOrder(game,90f);TapBoard(game);yield return new WaitForSecondsRealtime(2f);
                Assert.IsNotNull(GameObject.Find("Management ¡ASADO COMPLETADO!"));
                var result=MvpSave.Load();
                Assert.Greater(result.Management.Balance,balance,"L"+level+" pays a real reward.");
                Assert.GreaterOrEqual(result.StarsByLevel[level-1],1,"L"+level+" passes culinary gates.");
                Assert.IsNull(result.Management.ActiveRun);Assert.AreEqual(level,result.Management.Cycle);
                Assert.IsTrue(result.Management.ManagementTutorialCompleted);
                AssertAllManagementTextFits();
                if(level<6)
                {
                    var nextButton=FindButton("SIGUIENTE");
                    Assert.IsTrue(nextButton.IsInteractable(),"Result next action must be interactable through every ancestor CanvasGroup.");
                    TapVisibleButton(nextButton);yield return new WaitForSecondsRealtime(.55f);
                    Assert.AreEqual(level+1,ReadField<int>(game,"currentLevelNumber"),"Native next action must advance the current level.");
                    Assert.IsTrue(((GameObject)GetField(game,"introRoot")).activeInHierarchy,"Next must show its intro; transition="+ReadField<bool>(game,"menuTransitionActive"));
                    ClickButton("IR A LA PARRILLA");yield return new WaitForSecondsRealtime(.1f);
                    yield return BuyAndPrepareCurrentOrder(game);
                }
                else
                {
                    Assert.IsNotNull(GameObject.Find("NIVELES"));
                    Assert.LessOrEqual(result.MaxUnlockedLevel,7);
                    ClickButton("NIVELES");yield return new WaitForSecondsRealtime(.5f);
                    Assert.IsFalse(GameObject.Find("NIVEL 7").GetComponent<Button>().interactable);
                }
            }
        }

        [UnityTest]
        public IEnumerator Management_2DShopSwipeOwnsDirectionAndNeverAddsOrSpends()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            ClickButton("CARNICERÍA");yield return null;
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            var service=(ManagementService)GetField(game,"management");
            string before=JsonUtility.ToJson(service.State),persisted=PlayerPrefs.GetString(SaveKey);
            Assert.IsNull(GameObject.Find("Página anterior"));Assert.IsNull(GameObject.Find("Página siguiente"));
            Assert.AreEqual(new Vector2(.025f,.31f),view.Viewport.rectTransform.anchorMin,"Pager removal must not lower the cards into the cart");
            Assert.AreEqual(.175f,view.CartDropZone.anchorMin.y,.001f);
            var pager=GameObject.Find("Paginación carnicería").GetComponent<Text>();
            StringAssert.Contains("a los lados para ver más cortes",pager.text);
            Assert.IsFalse(pager.raycastTarget);Assert.IsNull(pager.GetComponent<Button>());
            Assert.AreEqual(new Vector2(.5f,.286f),pager.rectTransform.anchorMin);
            var hint=GameObject.Find("Pista de deslizamiento").GetComponent<Image>();
            Assert.IsFalse(hint.raycastTarget);Assert.IsNull(hint.GetComponent<Button>());
            Assert.AreEqual(new Vector2(.5f,.286f),hint.rectTransform.anchorMin);
            Assert.AreEqual(58f,hint.rectTransform.sizeDelta.y);
            Assert.AreEqual(.286f,pager.rectTransform.anchorMin.y,.001f);StringAssert.Contains("Deslizá",pager.text);
            SwipeShopPage(view,-1);yield return null;Assert.AreEqual(0,view.PageIndex,"First page does not wrap");
            TapVisibleButton(FindButton("Sumar corte chorizo"));yield return null;Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            // A canceled native + press is not rescued into a click by a later drag on the same Button.
            var plus=FindButton("Sumar corte chorizo");var plusPoint=UiScreenPoint(plus.transform.position);
            var canceledPress=new PointerEventData(EventSystem.current){position=plusPoint,pressPosition=plusPoint,pointerId=958,button=PointerEventData.InputButton.Left,eligibleForClick=true,pointerPress=plus.gameObject,pointerDrag=plus.gameObject};
            ExecuteEvents.Execute(plus.gameObject,canceledPress,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(view.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.cancelHandler);
            canceledPress.position=plusPoint+Vector2.left*15;
            ExecuteEvents.Execute(plus.gameObject,canceledPress,ExecuteEvents.beginDragHandler);
            FinishShopGesture(canceledPress,canceledPress.position);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $100"));Assert.IsFalse(canceledPress.eligibleForClick);
            // A second finger on + cancels both gestures without changing quantity.
            var main=BeginShopGesture(UiScreenPoint(view.Viewport.rectTransform.position),959);
            main.position=ShopGestureEnd(view,main.pressPosition,-.25f,0);ExecuteEvents.Execute(main.pointerDrag,main,ExecuteEvents.dragHandler);
            var extraTap=new PointerEventData(EventSystem.current){position=plusPoint,pressPosition=plusPoint,pointerId=969,button=PointerEventData.InputButton.Left,eligibleForClick=true};
            ExecuteEvents.Execute(plus.gameObject,extraTap,ExecuteEvents.pointerDownHandler);
            bool candidate=extraTap.eligibleForClick;ExecuteEvents.Execute(plus.gameObject,extraTap,ExecuteEvents.pointerUpHandler);
            if(candidate)ExecuteEvents.Execute(plus.gameObject,extraTap,ExecuteEvents.pointerClickHandler);
            FinishShopGesture(main,main.position);yield return null;
            Assert.IsFalse(candidate);Assert.AreEqual(0,view.PageIndex);Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            // Swiping directly across the food suppresses both click and cart-drop actions.
            var point=ExposedFoodPoint(view,view.Targets.Find(v=>v.FoodId=="tira"));
            var data=BeginShopGesture(point,960);
            FinishShopGesture(data,ShopGestureEnd(view,point,-.035f,0));yield return null;
            Assert.AreEqual(0,view.PageIndex,"A short movement is not a page swipe");Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            data=BeginShopGesture(point,961);data.position=ShopGestureEnd(view,point,-.25f,0);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.dragHandler);
            Assert.IsNull(GameObject.Find("Dragged food preview"),"Horizontal browsing hides any food-drop preview");
            // Once horizontal, crossing the basket must not add the food.
            FinishShopGesture(data,UiScreenPoint(view.CartDropZone.position)+Vector2.left*view.Viewport.rectTransform.rect.width*.3f);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $100"));Assert.IsFalse(view.IsDragging);
            // Quantity buttons keep native taps, but also relay browsing gestures without adding.
            int oldPage=view.PageIndex;
            point=UiScreenPoint(FindButton("Sumar corte "+view.Targets[0].FoodId).transform.position);
            SwipeShopPage(view,1,point);yield return null;Assert.AreEqual(oldPage+1,view.PageIndex);
            StringAssert.Contains((view.PageIndex+1)+" / 5",pager.text);Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            point=UiScreenPoint(GameObject.Find("Restar corte "+view.Targets[0].FoodId).transform.position);
            SwipeShopPage(view,-1,point);yield return null;Assert.AreEqual(oldPage,view.PageIndex);Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            point=UiScreenPoint(view.Viewport.rectTransform.position);
            data=BeginShopGesture(point,962);data.position=ShopGestureEnd(view,point,-.3f,0);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.dragHandler);
            var second=new PointerEventData(EventSystem.current){pointerId=963,position=point,pressPosition=point,button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(view.gameObject,second,ExecuteEvents.pointerDownHandler);
            FinishShopGesture(data,data.position);yield return null;Assert.AreEqual(oldPage,view.PageIndex);Assert.IsFalse(view.IsDragging);
            data=BeginShopGesture(point,964);data.position=ShopGestureEnd(view,point,-.3f,0);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.dragHandler);
            var wrong=new PointerEventData(EventSystem.current){pointerId=9999,position=data.position};
            ExecuteEvents.Execute(data.pointerDrag,wrong,ExecuteEvents.endDragHandler);yield return null;
            Assert.AreEqual(oldPage,view.PageIndex);Assert.IsFalse(view.IsDragging);
            data=BeginShopGesture(point,965);data.position=ShopGestureEnd(view,point,-.3f,0);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(view.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.cancelHandler);
            FinishShopGesture(data,data.position);yield return null;Assert.AreEqual(oldPage,view.PageIndex);
            data=BeginShopGesture(point,966);FinishShopGesture(data,ShopGestureEnd(view,point,0,-.25f));yield return null;
            Assert.AreEqual(oldPage,view.PageIndex,"Vertical empty-space movement must not browse");Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            Assert.AreEqual(before,JsonUtility.ToJson(service.State));Assert.AreEqual(persisted,PlayerPrefs.GetString(SaveKey));
            CaptureManagementFrame(game,"/tmp/asadito-shop-swipe-1080.png",()=>AssertAllManagementTextFits());
            CaptureManagementFrame(game,"/tmp/asadito-shop-swipe-720.png",()=>AssertAllManagementTextFits(),720,1600);
            data=BeginShopGesture(point,967);data.position=ShopGestureEnd(view,point,-.3f,0);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.dragHandler);ClickButton("Volver carnicería");yield return null;yield return null;
            Assert.IsNull(UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>());Assert.IsNull(GameObject.Find("Dragged food preview"));
            Assert.AreEqual(before,JsonUtility.ToJson(service.State));Assert.AreEqual(persisted,PlayerPrefs.GetString(SaveKey));
        }

        [UnityTest]
        public IEnumerator Management_2DShopPagesAllOriginalFoodsAndKeepsCart()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            ClickButton("CARNICERÍA");yield return null;var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            var root=GameObject.Find("Management CARNICERÍA");var service=(ManagementService)GetField(game,"management");
            string before=JsonUtility.ToJson(service.State);Assert.AreEqual(5,view.PageCount);Assert.IsFalse(view.IsFridge);
            Assert.IsNull(GameObject.Find("Página anterior"));Assert.IsNull(GameObject.Find("Página siguiente"));Assert2DManagementPresentation(view);
            TapFood(view,view.Targets.Find(v=>v.FoodId=="chorizo"));yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $100"));
            var ids=new HashSet<string>();
            for(int page=0;page<view.PageCount;page++)
            {
                Assert.AreEqual(page,view.PageIndex);Assert.AreEqual(page==4?2:4,view.Targets.Count);
                foreach(var target in view.Targets)
                {
                    Assert.IsTrue(ids.Add(target.FoodId),"Each catalog item occurs once across pages");
                    AssertOriginalRawFoodSprite(game,target);Assert.AreSame(target,view.Raycast(ExposedFoodPoint(view,target)));
                    Assert.AreEqual("$"+service.Config.Product(target.FoodId).Price,GameObject.Find("Precio "+target.FoodId).GetComponent<Text>().text);
                }
                AssertShopGrillSize(view,game);AssertShop2DOverlapLayout(view);
                CaptureManagementFrame(game,"/tmp/asadito-shop-page-"+(page+1)+".png");
                if(page==0)
                {
                    CaptureManagementFrame(game,"/tmp/asadito-shop-1080.png",()=>AssertAllManagementTextFits());
                    CaptureManagementFrame(game,"/tmp/asadito-shop-720.png",()=>AssertAllManagementTextFits(),720,1600);
                }
                if(page<view.PageCount-1)
                {
                    view.CartDropZone.localScale=Vector3.one*1.02f; // Simulate paging while feedback pulse is in flight.
                    SwipeShopPage(view,1);yield return null;
                    Assert.AreEqual(Vector3.one,view.CartDropZone.localScale,"Page rebuild must reset interrupted cart feedback");
                }
            }
            Assert.AreEqual(FoodCatalog.GetAll().Length,ids.Count);SwipeShopPage(view,1);yield return null;Assert.AreEqual(4,view.PageIndex,"Last page does not wrap");
            Assert.IsNotNull(GameObject.Find("TOTAL $100"));Assert.AreEqual(before,JsonUtility.ToJson(service.State));
            for(int i=0;i<4;i++){SwipeShopPage(view,-1);yield return null;}
            Assert.AreEqual("1",GameObject.Find("Cantidad chorizo").GetComponent<Text>().text);
            ClickButton("Sumar corte tira");yield return null;Assert.IsNotNull(GameObject.Find("TOTAL $280"));
            ClickButton("Restar corte chorizo");yield return null;Assert.IsNotNull(GameObject.Find("TOTAL $180"));
            foreach(var button in root.GetComponentsInChildren<Button>())Assert.GreaterOrEqual(button.GetComponent<RectTransform>().rect.height,132,"44dp-equivalent reference hit height");
            ClickButton("DETALLE");yield return null;Canvas.ForceUpdateCanvases();AssertAllManagementTextFits();
            CaptureManagementFrame(game,"/tmp/asadito-shop-detail.png");
            ClickButton("VACIAR");yield return null;Assert.IsNotNull(GameObject.Find("TOTAL $0"));
            Assert.AreEqual("0",GameObject.Find("Cantidad tira").GetComponent<Text>().text);
            ClickButton("Volver carnicería");yield return null;yield return null;
            Assert.IsNull(UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>());Assert.AreEqual(before,JsonUtility.ToJson(service.State));
        }

        [UnityTest]
        public IEnumerator Management_2DStockingNonOrderFoodPaysAndReloadsExactRawUnits()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            ClickButton("CARNICERÍA");yield return null;var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            while(!view.Targets.Exists(v=>v.FoodId=="lomo")){SwipeShopPage(view,1);yield return null;}
            AssertOriginalRawFoodSprite(game,view.Targets.Find(v=>v.FoodId=="lomo"));
            ClickButton("Sumar corte lomo");yield return null;ClickButton("Sumar corte lomo");yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $640"));Assert.IsTrue(FindButton("PAGAR Y SALIR").interactable);
            ClickButton("PAGAR Y SALIR");yield return new WaitForSecondsRealtime(.7f);
            Assert.AreEqual(10,MvpSave.Load().Management.Balance);Assert.AreEqual(2,MvpSave.Load().Management.Inventory.Count);
            int first=MvpSave.Load().Management.Inventory[0].Id,second=MvpSave.Load().Management.Inventory[1].Id;
            view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();Assert.IsTrue(view.IsFridge);
            Assert.AreEqual(2,view.Targets.Count);foreach(var target in view.Targets)AssertOriginalRawFoodSprite(game,target);
            CaptureManagementFrame(game,"/tmp/asadito-shop-stocked-lomo.png");
            ResetSaveCache();yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            Assert.AreEqual(10,MvpSave.Load().Management.Balance);Assert.AreEqual(first,MvpSave.Load().Management.Inventory[0].Id);Assert.AreEqual(second,MvpSave.Load().Management.Inventory[1].Id);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            Assert.IsTrue(FindButton("HELADERA").interactable,"Stock access must not require enough food for the current order");
            ClickButton("HELADERA");yield return null;view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            Assert.AreEqual(2,view.Targets.Count);foreach(var target in view.Targets)AssertOriginalRawFoodSprite(game,target);
            // Buying any cut must also allow explicitly preparing it during the debut guide.
            // Use the real exposed sprite surface and EventSystem, not the selection callback.
            TapFood(view,view.Targets.Find(t=>t.UnitId==first));yield return new WaitForSecondsRealtime(.3f);
            Assert.IsTrue(view.Targets.Find(t=>t.UnitId==first).Selected,"A paid non-order cut must not be rejected by the debut tutorial");
            TapFood(view,view.Targets.Find(t=>t.UnitId==second));yield return new WaitForSecondsRealtime(.3f);
            TapVisibleButton(FindButton("PREPARAR"));yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(10,ServiceBalance(game));Assert.AreEqual(0,MvpSave.Load().Management.Inventory.Count);
            CollectionAssert.AreEqual(new[]{first,second},MvpSave.Load().Management.ActiveRun.Units.ConvertAll(u=>u.Id));
            foreach(var portion in (Array)GetField(game,"portions"))Assert.AreEqual("lomo",((FoodCookProfile)GetField(portion,"Profile")).FoodId);
            // A cold resume retains the exact paid units and never charges/abandons them again.
            ResetSaveCache();yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return new WaitForSecondsRealtime(.5f);
            Assert.IsNull(GameObject.Find("Management PRÓXIMO ASADO"));Assert.AreEqual(10,ServiceBalance(game));
            Assert.AreEqual(0,MvpSave.Load().Management.PendingWaste);
            CollectionAssert.AreEqual(new[]{first,second},MvpSave.Load().Management.ActiveRun.Units.ConvertAll(u=>u.Id));
        }

        private static int ServiceBalance(Component game)=>((ManagementService)GetField(game,"management")).State.Balance;

        private static Text FindTextContaining(string value)
        {
            foreach(var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                if(text.gameObject.activeInHierarchy&&text.text.Contains(value))return text;
            return null;
        }

        private static void TapVisibleButton(Button button)
        {
            Canvas.ForceUpdateCanvases();var point=UiScreenPoint(button.transform.position);
            var data=new PointerEventData(EventSystem.current){position=point,pressPosition=point,pointerId=947,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Assert.IsNotEmpty(hits,"The visible button must receive a native UI raycast");
            Assert.AreSame(button.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),"No overlay may consume the visible button");
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerUpHandler);
            Assert.IsNotNull(ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler));
        }

        [UnityTest]
        public IEnumerator Management_WrongRequestedFoodCannotApproveEvenWithWellCookedStock()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            SetField(game,"SimulationTimeScale",0f);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            var service=(ManagementService)GetField(game,"management");
            Assert.IsNotNull(FindTextContaining("ANA · PIDE TIRA DE ASADO"));
            Assert.IsNotNull(FindTextContaining("TITO · PIDE CHORIZO"));
            CaptureManagementFrame(game,"/tmp/asadito-request-planning-1080.png",()=>AssertAllManagementTextFits());
            CaptureManagementFrame(game,"/tmp/asadito-request-planning-720.png",()=>AssertAllManagementTextFits(),720,1600);
            ClickButton("CARNICERÍA");yield return null;
            TapVisibleButton(FindButton("Sumar corte chorizo"));yield return null;
            TapVisibleButton(FindButton("Sumar corte tira"));yield return null;
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            while(!view.Targets.Exists(t=>t.FoodId=="morcilla")){SwipeShopPage(view,1);yield return null;}
            TapVisibleButton(FindButton("Sumar corte morcilla"));yield return null;TapVisibleButton(FindButton("Sumar corte morcilla"));yield return null;
            TapVisibleButton(FindButton("PAGAR Y SALIR"));yield return new WaitForSecondsRealtime(.4f);
            int purchasedBalance=service.State.Balance;
            var kept=new List<InventoryUnit>();foreach(var unit in service.State.Inventory)if(unit.FoodId!="morcilla")kept.Add(unit);
            Assert.AreEqual(2,kept.Count);
            view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            var chosen=new List<ManagementFoodTarget>(view.Targets.FindAll(t=>t.FoodId=="morcilla"));
            foreach(var target in chosen){TapFood(view,target);yield return new WaitForSecondsRealtime(.3f);}
            Assert.IsTrue(FindButton("PREPARAR").interactable,"Preparing off-menu stock stays allowed; only approval requires the request");
            TapVisibleButton(FindButton("PREPARAR"));yield return new WaitForSecondsRealtime(.5f);
            CollectionAssert.AreEqual(MvpLevelCatalog.Get(1).FoodIds,ReadField<MvpLevelDefinition>(game,"currentLevel").FoodIds,"Prepared food must not rewrite authored request");
            foreach(var unit in service.State.ActiveRun.Units)Assert.AreEqual("morcilla",unit.FoodId);
            Array portions=(Array)GetField(game,"portions");LoadAllRawFoodToGrill(game,0);yield return null;
            // Seed ideal cooked states to isolate order identity from the independently tested cooking simulation.
            for(int i=0;i<portions.Length;i++)
            {
                var state=(FoodState)GetField(portions.GetValue(i),"State");
                var profile=(FoodCookProfile)GetField(portions.GetValue(i),"Profile");
                var band=FindDonenessBand(profile,Doneness.A_Punto);state.CoreTemperatureC=(band.MinimumCoreC+band.MaximumCoreC)*.5f;
                state.Moisture=1f;state.SplitRisk=0f;state.SetCurrentFace(new FoodFaceState{SurfaceTemperatureC=150f,Maillard=.4f,Char=0f});
                DragFoodToBoard(game,i,980+i);
                float started=Time.realtimeSinceStartup;
                while(ReadField<bool>(game,"platingInProgress")&&Time.realtimeSinceStartup-started<3f)yield return null;
                Assert.IsTrue(ReadField<bool>(portions.GetValue(i),"OnTray"),"Each native drop must finish plating before dragging another piece");
            }
            Assert.IsTrue(CanServeFromBoard(game));TapBoard(game);yield return new WaitForSecondsRealtime(2f);
            var result=(EconomicResult)GetField(game,"economicResult");
            Assert.IsNotNull(result,"Serving the completed board must create an economic result");
            Assert.GreaterOrEqual(result.Cooking,service.Config.MinimumCookingForStar,"Failure must not be attributed to raw/burnt morcilla");
            Assert.IsFalse(result.Order.IsComplete);Assert.AreEqual(0,result.Order.MatchedCount);
            Assert.AreEqual(1,result.Order.MissingByFood["tira"]);Assert.AreEqual(1,result.Order.MissingByFood["chorizo"]);
            Assert.AreEqual(2,result.Order.UnexpectedByFood["morcilla"]);
            Assert.IsFalse(result.Passed);Assert.AreEqual(0,result.Stars);StringAssert.Contains("Pedido incorrecto",result.Advice);
            Assert.IsNull(GameObject.Find("SIGUIENTE"));Assert.IsNotNull(FindTextContaining("PEDIDO INCORRECTO"));
            Assert.IsFalse(MvpSave.Load().IsLevelUnlocked(2));Assert.AreEqual(0,MvpSave.Load().StarsByLevel[0]);
            Assert.AreEqual(purchasedBalance+result.Income,service.State.Balance,"Existing paid failure reward formula remains unchanged");
            Assert.AreEqual(kept.Count,service.State.Inventory.Count);
            foreach(var unit in kept)Assert.IsTrue(service.State.Inventory.Exists(u=>u.Id==unit.Id&&u.FoodId==unit.FoodId&&u.Cost==unit.Cost));
            string persisted=PlayerPrefs.GetString(SaveKey);TapBoard(game);yield return null;Assert.AreEqual(persisted,PlayerPrefs.GetString(SaveKey));
            Action assertResultFits=()=>
            {
                foreach(var label in GameObject.Find("Management ¡ASADO COMPLETADO!").GetComponentsInChildren<Text>())
                    Assert.LessOrEqual(label.preferredHeight,label.rectTransform.rect.height+1f,"Truncated result: "+label.text);
            };
            CaptureManagementFrame(game,"/tmp/asadito-wrong-request-result-1080.png",assertResultFits);
            CaptureManagementFrame(game,"/tmp/asadito-wrong-request-result-720.png",assertResultFits,720,1600);
            TapVisibleButton(FindButton("VER DETALLE"));yield return null;
            Assert.IsNotNull(FindTextContaining("PEDIDO INCORRECTO · 0 / 2 piezas correctas"));
            Assert.IsNotNull(FindTextContaining("Faltó: TIRA DE ASADO ×1 · CHORIZO ×1"));
            Assert.IsNotNull(FindTextContaining("No pedido: MORCILLA ×2"));
            Assert.IsNotNull(FindTextContaining("ANA: faltó su pedido ("+FoodCatalog.Get("tira").DisplayName+")"));
            ResetSaveCache();var reloaded=MvpSave.Load();Assert.IsFalse(reloaded.IsLevelUnlocked(2));
            foreach(var unit in kept)Assert.IsTrue(reloaded.Management.Inventory.Exists(u=>u.Id==unit.Id&&u.FoodId==unit.FoodId&&u.Cost==unit.Cost));
        }

        [UnityTest]
        public IEnumerator Management_StockedDebutHasClearNativeRouteToGrillAndKeepsExtras()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            SetField(game,"SimulationTimeScale",120f);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            var service=(ManagementService)GetField(game,"management");
            ClickButton("CARNICERÍA");yield return null;
            for(int i=0;i<2;i++){AddToBasket("chorizo");yield return null;AddToBasket("tira");yield return null;}
            TapVisibleButton(FindButton("PAGAR Y SALIR"));yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(90,service.State.Balance);Assert.AreEqual(4,service.State.Inventory.Count);
            string paid=JsonUtility.ToJson(service.State);var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            Assert.IsTrue(FindText("Fridge preparation status").text.Contains("Tocá 2"),"Checkout must explain the next action rather than only announcing stock");
            Assert.AreEqual("ELEGÍ 2 PIEZAS",GameObject.Find("PREPARAR").GetComponentInChildren<Text>().text);
            int c1=service.State.Inventory[0].Id,c2=service.State.Inventory[1].Id,r1=service.State.Inventory[2].Id,r2=service.State.Inventory[3].Id;
            // Even during Guided, two of the same paid SKU are a valid alternate mix.
            TapFood(view,view.Targets.Find(t=>t.UnitId==c1));yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(GameObject.Find("PREPARAR").GetComponent<Button>().interactable);
            Assert.AreEqual("FALTA 1 PIEZA",GameObject.Find("PREPARAR").GetComponentInChildren<Text>().text);
            TapFood(view,view.Targets.Find(t=>t.UnitId==c2));yield return new WaitForSecondsRealtime(.3f);
            Assert.IsTrue(FindButton("PREPARAR").interactable,"Tutorial must not enforce a hidden one-per-SKU quota");
            TapFood(view,view.Targets.Find(t=>t.UnitId==r1));yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(view.Targets.Find(t=>t.UnitId==r1).Selected,"Keep the documented batch quantity; extras remain stock");
            Assert.IsTrue(FindText("Fridge preparation status").text.Contains("Ya elegiste las 2"));
            Assert.AreEqual(paid,JsonUtility.ToJson(service.State),"Temporary choices/rejections must not debit or consume");
            TapVisibleButton(FindButton("CANCELAR"));yield return new WaitForSecondsRealtime(.3f);
            TapVisibleButton(FindButton("VOLVER"));yield return null;
            Assert.AreEqual(paid,JsonUtility.ToJson(service.State));
            TapVisibleButton(FindButton("PREPARAR ASADO"));yield return new WaitForSecondsRealtime(.3f);
            view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            TapFood(view,view.Targets.Find(t=>t.UnitId==c1));yield return new WaitForSecondsRealtime(.3f);
            TapFood(view,view.Targets.Find(t=>t.UnitId==r1));yield return new WaitForSecondsRealtime(.3f);
            Assert.AreEqual("IR A LA PARRILLA",FindButton("PREPARAR").GetComponentInChildren<Text>().text);
            AssertAllManagementTextFits();
            CaptureManagementFrame(game,"/tmp/asadito-stock-to-grill-ready.png");
            CaptureManagementFrame(game,"/tmp/asadito-stock-to-grill-ready-narrow.png",null,720,1600);
            TapVisibleButton(FindButton("PREPARAR"));yield return new WaitForSecondsRealtime(.5f);
            Assert.IsNull(GameObject.Find("Management HELADERA"));
            CollectionAssert.AreEqual(new[]{c1,r1},service.State.ActiveRun.Units.ConvertAll(u=>u.Id));
            CollectionAssert.AreEqual(new[]{c2,r2},service.State.Inventory.ConvertAll(u=>u.Id));
            Assert.AreEqual(90,service.State.Balance);Assert.AreEqual(280,service.State.ActiveRun.FoodCost);
            Assert.AreEqual(1f,((CanvasGroup)GetField(game,"gameplayCanvasGroup")).alpha);
            yield return CookAndPlateOrder(game,90f);TapBoard(game);yield return new WaitForSecondsRealtime(2f);
            Assert.IsNull(service.State.ActiveRun);Assert.AreEqual(2,service.State.Inventory.Count);
            var result=(EconomicResult)GetField(game,"economicResult");Assert.AreEqual(280,result.Spending);
            Assert.Greater(result.Income,0);Assert.IsTrue(result.Passed);
            ResetSaveCache();CollectionAssert.AreEqual(new[]{c2,r2},MvpSave.Load().Management.Inventory.ConvertAll(u=>u.Id));
        }

        [UnityTest]
        public IEnumerator Management_2DFridgeTracksExactUnitsCancelAndPrepare()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            var service=(ManagementService)GetField(game,"management");service.State.Balance=2000;
            ClickButton("CARNICERÍA");yield return null;
            for(int i=0;i<3;i++){AddToBasket("chorizo");yield return null;}for(int i=0;i<2;i++){AddToBasket("tira");yield return null;}
            ClickButton("PAGAR Y SALIR");yield return new WaitForSecondsRealtime(.7f);
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();Assert.IsTrue(view.IsFridge);Assert2DManagementPresentation(view);
            Assert.AreEqual(5,view.Targets.Count);Assert.AreEqual(5,service.State.Inventory.Count);Assert.AreEqual(1340,service.State.Balance);
            var ids=new HashSet<int>();
            foreach(var t in view.Targets)
            {
                Assert.IsTrue(ids.Add(t.UnitId),"Each visible unit must have a unique paid inventory ID");
                var unit=service.State.Inventory.Find(u=>u.Id==t.UnitId);Assert.IsNotNull(unit);Assert.AreEqual(unit.FoodId,t.FoodId);
                Assert.AreSame(t,view.Raycast(view.ScreenPoint(t)),"Inventory unit must be independently selectable: "+t.UnitId);
                AssertOriginalRawFoodSprite(game,t);
            }
            int chorizo=service.State.Inventory[2].Id,tira=service.State.Inventory[4].Id;
            var c=view.Targets.Find(t=>t.UnitId==chorizo);var r=view.Targets.Find(t=>t.UnitId==tira);
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge.png");
            TapFood(view,c);yield return new WaitForSecondsRealtime(.3f);TapFood(view,r);yield return new WaitForSecondsRealtime(.3f);
            Assert.IsTrue(c.Selected);Assert.IsTrue(r.Selected);Assert.AreEqual(5,service.State.Inventory.Count,"Selection previews do not consume inventory");
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge-selected.png");
            ClickButton("CANCELAR");yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(c.Selected);Assert.IsFalse(r.Selected);Assert.Less(Vector3.Distance(c.Home,c.transform.localPosition),.01f);
            Assert.IsFalse(GameObject.Find("PREPARAR").GetComponent<Button>().interactable);
            // Returning/reopening cancels only the preview, with persistent paid inventory intact.
            ClickButton("VOLVER");yield return null;ResetSaveCache();Assert.AreEqual(5,MvpSave.Load().Management.Inventory.Count);
            ClickButton("HELADERA");yield return new WaitForSecondsRealtime(.7f);view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            foreach(var t in view.Targets)Assert.IsFalse(t.Selected,"Reopening must not keep a stale preparation preview");
            TapFood(view,view.Targets.Find(t=>t.UnitId==chorizo));yield return new WaitForSecondsRealtime(.3f);
            TapFood(view,view.Targets.Find(t=>t.UnitId==tira));yield return new WaitForSecondsRealtime(.3f);
            ClickButton("PREPARAR");yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(chorizo,service.State.ActiveRun.Units[0].Id);Assert.AreEqual(tira,service.State.ActiveRun.Units[1].Id);Assert.AreEqual(3,service.State.Inventory.Count);
            Assert.IsNull(service.State.Inventory.Find(u=>u.Id==chorizo));Assert.IsNull(service.State.Inventory.Find(u=>u.Id==tira));
            ResetSaveCache();Assert.AreEqual(chorizo,MvpSave.Load().Management.ActiveRun.Units[0].Id);Assert.AreEqual(tira,MvpSave.Load().Management.ActiveRun.Units[1].Id);
            Assert.IsNull(UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>());
        }

        [UnityTest]
        public IEnumerator Management_2DFridgeFullCapacityKeepsSizeAndPicksVisibleOverlap()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            var service=(ManagementService)GetField(game,"management");service.State.Balance=3000;service.State.ManagementTutorialCompleted=true;
            ClickButton("CARNICERÍA");yield return null;
            for(int i=0;i<4;i++){AddToBasket("chorizo");yield return null;AddToBasket("tira");yield return null;}
            ClickButton("PAGAR Y SALIR");yield return new WaitForSecondsRealtime(.4f);
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            Assert.IsTrue(view.IsFridge);Assert.AreEqual(8,view.Targets.Count);Assert.AreEqual(8,service.State.Inventory.Count);Assert.AreEqual(1880,service.State.Balance);
            Assert2DManagementPresentation(view);
            var backdrop=GameObject.Find("Heladera ilustrada").GetComponent<RawImage>();
            Assert.AreEqual("Fridge_Hybrid_OpenEmptyV2",backdrop.texture.name);Assert.IsFalse(backdrop.raycastTarget);
            Assert.IsTrue(GameObject.Find("DESCARTAR").GetComponent<RectTransform>().anchorMin.x>.7f);
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge-full.png",()=>AssertFridgeFoodSize(view,game));
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge-full-narrow.png",()=>AssertFridgeFoodSize(view,game),720,1600);
            AssertAllManagementTextFits();
            var cuts=view.Targets.FindAll(t=>t.FoodId=="tira");
            // Actual Canvas sibling order must decide which exact inventory ID is visible.
            cuts[1].DisplayUV=cuts[0].DisplayUV;view.RefreshLayout();cuts[1].transform.SetAsLastSibling();Canvas.ForceUpdateCanvases();
            Assert.Greater(cuts[1].transform.GetSiblingIndex(),cuts[0].transform.GetSiblingIndex());
            Assert.AreSame(cuts[1],view.Raycast(view.ScreenPoint(cuts[1])),"The upper 2D cut must own the touch, not the hidden unit");
            TapFood(view,cuts[1]);yield return new WaitForSecondsRealtime(.3f);
            Assert.IsTrue(cuts[1].Selected);Assert.AreSame(cuts[0],view.Raycast(view.ScreenPoint(cuts[0])));
            Assert.AreEqual(8,service.State.Inventory.Count,"Selecting a preview must not consume or duplicate inventory");
            ClickButton("CANCELAR");yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(cuts[1].Selected);Assert.Less(Vector3.Distance(cuts[1].Home,cuts[1].transform.localPosition),.01f);
            // Four full-sized cuts on the prep board exercise overlap without count-based scaling.
            for(int i=0;i<cuts.Count;i++)view.MoveSelection(cuts[i],true,i);
            yield return new WaitForSecondsRealtime(.3f);
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge-four-selected.png",()=>AssertFridgeFoodSize(view,game));
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge-selected-narrow.png",()=>AssertFridgeFoodSize(view,game),720,1600);
            for(int i=0;i<cuts.Count;i++)view.MoveSelection(cuts[i],false,0);
            yield return new WaitForSecondsRealtime(.3f);AssertFridgeFoodSize(view,game);
            // Every capacity slot, including the fifth, has its own reversible visual pose.
            for(int i=0;i<view.Targets.Count;i++)view.MoveSelection(view.Targets[i],true,i);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.Greater(Vector3.Distance(view.Targets[4].Rect.localPosition,view.Targets[0].Rect.localPosition),1f,
                "The fifth prep slot must not wrap onto the first slot's exact position");
            for(int i=0;i<view.Targets.Count;i++)Assert.AreEqual(i,view.Targets[i].SelectionSlot);
            for(int i=0;i<view.Targets.Count;i++)view.MoveSelection(view.Targets[i],false,0);
            yield return new WaitForSecondsRealtime(.3f);
            // An interrupted transfer reverses cleanly without consuming an inventory ID.
            var interrupted=view.Targets[0];int interruptedId=interrupted.UnitId;
            view.MoveSelection(interrupted,true,0);yield return new WaitForSecondsRealtime(.04f);view.MoveSelection(interrupted,false,0);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(interrupted.Selected);Assert.AreEqual(-1,interrupted.SelectionSlot);
            Assert.Less(Vector3.Distance(interrupted.Home,interrupted.Rect.localPosition),.01f);
            Assert.AreEqual(interruptedId,interrupted.UnitId);Assert.IsNotNull(service.State.Inventory.Find(u=>u.Id==interruptedId));
            Assert.AreEqual(8,MvpSave.Load().Management.Inventory.Count);
        }

        private static void AssertFridgeFoodSize(ManagementFoodView view,Component game)
        {
            foreach(var target in view.Targets)
            {
                AssertOriginalRawFoodSprite(game,target);AssertFoodFootprint(game,target);
                var projected=ProjectedFoodBounds(view,target);
                Assert.That(projected.xMin,Is.GreaterThanOrEqualTo(-.001f));Assert.That(projected.xMax,Is.LessThanOrEqualTo(1.001f));
                Assert.That(projected.yMin,Is.GreaterThanOrEqualTo(-.001f));Assert.That(projected.yMax,Is.LessThanOrEqualTo(1.001f));
            }
        }

        [UnityTest]
        public IEnumerator Management_2DZeroStockAndEmptyFridgeNeverInventUnits()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            var service=(ManagementService)GetField(game,"management");service.State.ManagementTutorialCompleted=true;
            foreach(var p in service.Config.Products)service.State.Purchases.Add(new PurchaseRecord{FoodId=p.FoodId,Quantity=p.Stock-(p.FoodId=="chorizo"?5:3)});
            ClickButton("CARNICERÍA");yield return null;var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            Assert.AreEqual(4,view.Targets.Count,"Catalogue artwork is one original RAW per SKU, not inventory units");
            Assert.AreEqual(1,view.Targets.FindAll(t=>t.FoodId=="chorizo").Count);Assert.AreEqual(1,view.Targets.FindAll(t=>t.FoodId=="tira").Count);
            Assert.That(SignStock("chorizo").text,Does.Contain("Stock 5"));Assert.That(SignStock("tira").text,Does.Contain("Stock 3"));
            foreach(var target in view.Targets)Assert.AreEqual(target.FoodId,view.Raycast(view.ScreenPoint(target)).FoodId);
            AssertShopGrillSize(view,game);
            service.State.Purchases.Clear();view.ConfigureShop(1);yield return null;
            Assert.AreEqual(4,view.Targets.Count);AssertShopGrillSize(view,game);
            service.State.Purchases.Clear();foreach(var p in service.Config.Products)service.State.Purchases.Add(new PurchaseRecord{FoodId=p.FoodId,Quantity=p.Stock});
            view.ConfigureShop(1);yield return null;
            Assert.AreEqual(4,view.Targets.Count);Assert.IsFalse(GameObject.Find("PAGAR Y SALIR").GetComponent<Button>().interactable);
            Assert.IsFalse(GameObject.Find("Sumar corte chorizo").GetComponent<Button>().interactable);
            string before=JsonUtility.ToJson(service.State);TapFood(view,view.Targets.Find(v=>v.FoodId=="chorizo"));yield return null;Assert.AreEqual(before,JsonUtility.ToJson(service.State));
            Assert.That(SignStock("chorizo").text,Does.Contain("AGOTADO"));
            CaptureManagementFrame(game,"/tmp/asadito-2d-shop-empty-stock.png");
            ClickButton("HELADERA");yield return new WaitForSecondsRealtime(.7f);view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            Assert.IsEmpty(view.Targets);Assert.IsNotNull(GameObject.Find("Heladera vacía\nComprá carne para tu asado"));
            CaptureManagementFrame(game,"/tmp/asadito-2d-fridge-empty.png");
        }

        [UnityTest]
        public IEnumerator Management_2DCartDragAddsOnlyOnDropAndCancelsOtherPointers()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;ClickButton("CARNICERÍA");yield return null;
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();var service=(ManagementService)GetField(game,"management");
            Assert.IsNotNull(GameObject.Find("Carnicería ilustrada"));Assert.IsNotNull(GameObject.Find("Changuito"));Assert2DManagementPresentation(view);
            var shopSizes=new Dictionary<string,Vector2>{{"chorizo",GrillFoodSize(game,"chorizo")},{"tira",GrillFoodSize(game,"tira")}};
            AssertShopGrillSize(view,game);
            string state=JsonUtility.ToJson(service.State);var target=view.Targets.Find(t=>t.FoodId=="chorizo");
            var data=StartCartDrag(view,target,900);yield return null;
            Assert.IsTrue(view.IsDragging);Assert.IsNotNull(GameObject.Find("Dragged food preview"));
            Assert.AreEqual(target.Rect.sizeDelta,GameObject.Find("Dragged food preview").GetComponent<RectTransform>().sizeDelta);Assert.IsNotNull(GameObject.Find("TOTAL $0"));
            var extra=new PointerEventData(EventSystem.current){position=data.position,pressPosition=data.pressPosition,pointerId=901,button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(view.gameObject,extra,ExecuteEvents.pointerDownHandler);yield return null;
            Assert.IsFalse(view.IsDragging,"A second finger DOWN cancels the original drag");Assert.IsNull(GameObject.Find("Dragged food preview"));
            data.position=UiScreenPoint(view.CartDropZone.position);
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerClickHandler);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $0"),"The cancelled original pointer must not buy on later release");
            data=StartCartDrag(view,target,902);
            var wrongEnd=new PointerEventData(EventSystem.current){position=UiScreenPoint(view.CartDropZone.position),pointerId=9902,button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(view.gameObject,wrongEnd,ExecuteEvents.endDragHandler);yield return null;
            Assert.IsFalse(view.IsDragging,"An EndDrag from the wrong pointer must cancel, not commit or leave a ghost");
            Assert.IsNull(GameObject.Find("Dragged food preview"));Assert.IsNotNull(GameObject.Find("TOTAL $0"));
            data=StartCartDrag(view,target,903);data.position=view.ScreenPoint(target);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);yield return null;
            Assert.IsFalse(view.IsDragging);Assert.IsNull(GameObject.Find("Dragged food preview"));Assert.IsNotNull(GameObject.Find("TOTAL $0"));
            data=StartCartDrag(view,target,904);data.position=UiScreenPoint(view.CartDropZone.position);
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.dragHandler);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerClickHandler);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $100"),"Drop adds exactly once; duplicate release or synthesized click cannot buy again");
            Assert.AreEqual(state,JsonUtility.ToJson(service.State));Assert.IsTrue(GameObject.Find("Contenido changuito chorizo").activeSelf);
            target=view.Targets.Find(t=>t.FoodId=="tira");data=StartCartDrag(view,target,905);data.position=UiScreenPoint(view.CartDropZone.position);
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);yield return new WaitForSecondsRealtime(.4f);Assert.IsNotNull(GameObject.Find("TOTAL $280"));
            Assert.IsNull(GameObject.Find("Dragged food preview"));Assert.AreEqual(4,view.Targets.Count);AssertShop2DOverlapLayout(view);
            foreach(var piece in view.Targets)Assert.Less(Vector3.Distance(piece.HomeScale,piece.transform.localScale),.001f,"Drag/pulse must restore each piece's layout scale");
            CaptureManagementFrame(game,"/tmp/asadito-2d-cart.png");AssertAllManagementTextFits();
            ClickButton("PAGAR Y SALIR");yield return new WaitForSecondsRealtime(.7f);
            Assert.AreEqual(2,service.State.Inventory.Count);Assert.AreEqual(2,MvpSave.Load().Management.Inventory.Count);
            view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();Assert.IsTrue(view.IsFridge);CaptureManagementFrame(game,"/tmp/asadito-2d-cart-paid-fridge.png");
            foreach(var unit in view.Targets){Assert.AreSame(unit,view.Raycast(view.ScreenPoint(unit)));TapFood(view,unit);yield return new WaitForSecondsRealtime(.3f);}
            ClickButton("PREPARAR");yield return new WaitForSecondsRealtime(.5f);
            var portions=(Array)GetField(game,"portions");var sizes=(Vector2[])GetField(game,"portionVisualSizes");var images=(Image[])GetField(game,"portionImages");
            for(int i=0;i<portions.Length;i++)
            {
                var profile=(FoodCookProfile)GetField(portions.GetValue(i),"Profile");
                Assert.AreEqual(shopSizes[profile.FoodId],sizes[i],"Prepared grill must reuse the same footprint");Assert.AreEqual(sizes[i],images[i].rectTransform.sizeDelta);
            }
            SetField(game,"SimulationTimeScale",0f);LoadAllRawFoodToGrill(game,0);yield return new WaitForSecondsRealtime(.4f);
            for(int i=0;i<portions.Length;i++)Assert.AreEqual(shopSizes[((FoodCookProfile)GetField(portions.GetValue(i),"Profile")).FoodId],images[i].rectTransform.sizeDelta);
            CaptureManagementFrame(game,"/tmp/asadito-2d-grill-size-reference.png",()=>
            {
                // Offscreen portrait QA changes Canvas mode; refresh only the fixture's food placement.
                var place=game.GetType().GetMethod("SetFoodTargetPosition",BindingFlags.Instance|BindingFlags.NonPublic);
                for(int i=0;i<portions.Length;i++)place.Invoke(game,new object[]{i,new Vector2(.45f,.43f+i*.25f)});
            });
        }

        [UnityTest]
        public IEnumerator Management_2DDragRejectsButtonsMoneyAndLeavesNoCopies()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;ClickButton("CARNICERÍA");yield return null;
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();var service=(ManagementService)GetField(game,"management");
            var target=view.Targets.Find(t=>t.FoodId=="tira");var data=StartCartDrag(view,target,910);
            data.position=UiScreenPoint(FindButton("DETALLE").transform.position);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $0"));service.State.Balance=0;string before=JsonUtility.ToJson(service.State);
            data=StartCartDrag(view,target,911);data.position=UiScreenPoint(view.CartDropZone.position);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.endDragHandler);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $0"));Assert.AreEqual(before,JsonUtility.ToJson(service.State));Assert.IsFalse(GameObject.Find("PAGAR Y SALIR").GetComponent<Button>().interactable);
            service.State.Balance=10000;for(int i=0;i<30;i++){TapFood(view,target);yield return null;}
            yield return new WaitForSecondsRealtime(.4f);Assert.IsNull(GameObject.Find("Dragged food preview"));Assert.AreEqual(4,view.Targets.Count);
            Assert.AreEqual(8,service.Config.FridgeCapacity);Assert.IsEmpty(service.State.Inventory);Assert.IsTrue(float.IsFinite(target.transform.localScale.x));
            CaptureManagementFrame(game,"/tmp/asadito-2d-rapid-taps.png");
            data=StartCartDrag(view,target,912);ClickButton("Volver carnicería");yield return null;yield return null;
            Assert.IsNull(GameObject.Find("Dragged food preview"));Assert.IsNull(UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>());
        }

        [UnityTest]
        public IEnumerator Management_2DDuplicateClicksDoNotRepeatSelection()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;ClickButton("CARNICERÍA");yield return null;
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();var service=(ManagementService)GetField(game,"management");
            string state=JsonUtility.ToJson(service.State);var target=view.Targets.Find(t=>t.FoodId=="chorizo");
            var point=ExposedFoodPoint(view,target);var data=new PointerEventData(EventSystem.current){position=point,pressPosition=point,pointerId=920,button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerClickHandler);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerClickHandler);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $100"),"One pointer press must produce only one purchase preview, even if click repeats");
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerClickHandler);yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $200"),"A fresh deliberate press may add another unit of the same SKU");
            Assert.AreEqual(state,JsonUtility.ToJson(service.State),"Basket preview never spends or creates persistent inventory");
        }

        [UnityTest]
        public IEnumerator Management_2DAlphaHitTestingUsesVisiblePixelsAndSiblingOrder()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            var service=(ManagementService)GetField(game,"management");service.State.ManagementTutorialCompleted=true;
            service.State.Inventory.Add(new InventoryUnit{Id=service.State.NextUnitId++,FoodId="chorizo"});
            service.State.Inventory.Add(new InventoryUnit{Id=service.State.NextUnitId++,FoodId="chorizo"});
            // Planning was built before the fixture inserted inventory/tutorial state. Rebuild native
            // navigation controls rather than bypassing a correctly disabled guided-debut button.
            ClickButton("CARNICERÍA");yield return null;ClickButton("HELADERA");yield return new WaitForSecondsRealtime(.4f);
            var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();Assert.AreEqual(2,view.Targets.Count);
            var lower=view.Targets[0];var upper=view.Targets[1];
            lower.DisplayUV=upper.DisplayUV=new Vector2(.5f,.55f);view.RefreshLayout();
            lower.Visual.rectTransform.localRotation=upper.Visual.rectTransform.localRotation=Quaternion.identity;
            upper.transform.SetAsLastSibling();Canvas.ForceUpdateCanvases();
            Vector2 opaque=FindRawSpritePixel(upper.Visual.sprite,true),transparent=FindRawSpritePixel(upper.Visual.sprite,false);
            Vector2 opaquePoint=FoodPixelScreenPoint(upper,opaque),transparentPoint=FoodPixelScreenPoint(upper,transparent);
            Assert.AreSame(upper,view.Raycast(opaquePoint),"Opaque upper artwork must prevent selecting the hidden inventory ID");
            lower.transform.SetAsLastSibling();Canvas.ForceUpdateCanvases();
            Assert.AreSame(lower,view.Raycast(opaquePoint),"Changing only the real Canvas sibling order must change the visible hit");
            upper.transform.SetAsLastSibling();Canvas.ForceUpdateCanvases();
            Assert.IsNull(view.Raycast(transparentPoint),"Genuine transparent sprite corners must not become rectangular invisible selection targets");
            string state=JsonUtility.ToJson(service.State);
            var emptyTap=new PointerEventData(EventSystem.current){position=transparentPoint,pressPosition=transparentPoint,pointerId=930,button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(view.gameObject,emptyTap,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(view.gameObject,emptyTap,ExecuteEvents.pointerClickHandler);yield return null;
            Assert.IsFalse(lower.Selected);Assert.IsFalse(upper.Selected);Assert.AreEqual(state,JsonUtility.ToJson(service.State));
            // A lower opaque pixel revealed through the upper transparent corner is genuinely visible.
            Vector3 exposedWorld=upper.Visual.rectTransform.TransformPoint(FoodPixelLocalPoint(upper.Visual.rectTransform,transparent));
            Vector3 lowerOpaqueWorld=lower.Visual.rectTransform.TransformPoint(FoodPixelLocalPoint(lower.Visual.rectTransform,opaque));
            lower.Visual.rectTransform.position+=exposedWorld-lowerOpaqueWorld;Canvas.ForceUpdateCanvases();
            Assert.AreSame(lower,view.Raycast(transparentPoint),"Transparent upper pixels must reveal the exact lower visible unit, not choose the upper rectangle");
            // UI card/control occlusion is tested separately by the shop/sign and cart-detail regressions.
            Assert.AreEqual(2,service.State.Inventory.Count);
        }

        private static void ForcePortraitManagementCanvas(Component game)
        {
            // Device UI is portrait-only. Editor Game View may be landscape; use its height for the
            // logical Canvas while real QA captures still render at 1080x1920 and 720x1600.
            ((Canvas)GetField(game,"canvas")).GetComponent<CanvasScaler>().matchWidthOrHeight=1;
            Canvas.ForceUpdateCanvases();
        }
        private static bool TryFindExposedFoodPoint(ManagementFoodView view,ManagementFoodTarget target,out Vector2 point)
        {
            foreach(float y in new[]{.5f,.3f,.7f,.15f,.85f,.05f,.95f})
                foreach(float x in new[]{.5f,.3f,.7f,.15f,.85f,.05f,.95f})
                {
                    var uv=new Vector2(x,y);
                    if(!FoodSilhouette.Contains(target.FoodId,uv))continue; // Do not manufacture hits in transparent corners.
                    var candidate=FoodPixelScreenPoint(target,uv);
                    if(view.Raycast(candidate)==target){point=candidate;return true;}
                }
            point=Vector2.zero;return false;
        }
        private static Vector2 ExposedFoodPoint(ManagementFoodView view,ManagementFoodTarget target)
        {
            Assert.IsNotNull(target);
            if(TryFindExposedFoodPoint(view,target,out var point))return point;
            // Shop pieces sell a SKU, not an inventory ID: a completely covered stock unit may be
            // represented by another genuinely exposed piece of that SKU. Fridge IDs never fall back.
            if(!view.IsFridge)
                foreach(var other in view.Targets)
                    if(other.FoodId==target.FoodId&&TryFindExposedFoodPoint(view,other,out point))return point;
            Assert.Fail("No genuinely exposed selectable surface for "+target.FoodId+" inventory ID "+target.UnitId);
            return Vector2.zero;
        }
        private static void Assert2DManagementPresentation(ManagementFoodView view)
        {
            Assert.IsNotNull(view.Viewport);Assert.IsTrue(view.Viewport.raycastTarget);Assert.AreEqual(0f,view.Viewport.color.a);
            Assert.IsEmpty(view.GetComponentsInChildren<MeshRenderer>(true));Assert.IsEmpty(view.GetComponentsInChildren<MeshFilter>(true));
            Assert.IsEmpty(view.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(view.GetComponentsInChildren<Camera>(true));
            Assert.IsNull(GameObject.Find("Management isolated camera"));Assert.IsNull(GameObject.Find("Physical butcher world"));Assert.IsNull(GameObject.Find("Physical fridge world"));
            foreach(var food in view.Targets)
            {
                Assert.IsNotNull(food.Visual);Assert.IsFalse(food.Visual.raycastTarget,"The viewport resolves alpha and draw order without rectangle child blockers");
                Assert.AreSame(view.GetComponentInParent<Canvas>(),food.Visual.GetComponentInParent<Canvas>());
            }
        }
        private static void AssertOriginalRawFoodSprite(Component game,ManagementFoodTarget target)
        {
            Assert.IsNotNull(target.Visual);Assert.IsNotNull(target.Visual.sprite);Assert.AreEqual(target.FoodId+"_raw",target.Visual.sprite.name);
            var original=(Sprite)game.GetType().GetMethod("ManagementFoodSprite",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(game,new object[]{target.FoodId});
            Assert.AreSame(original,target.Visual.sprite,"Management must reuse the original raw grill/catalog sprite, not a regenerated relief or miniature");
            Assert.IsFalse(target.Visual.raycastTarget);Assert.IsNull(target.GetComponent<MeshCollider>());
        }
        private static void AssertFoodFootprint(Component game,ManagementFoodTarget target)
        {
            var expected=GrillFoodSize(game,target.FoodId);var actual=target.Visual.rectTransform.rect.size;
            Assert.That(actual.x,Is.EqualTo(expected.x).Within(.01f),"Food width must retain the exact grill footprint: "+target.FoodId);
            Assert.That(actual.y,Is.EqualTo(expected.y).Within(.01f),"Food height must retain the exact grill footprint: "+target.FoodId);
            Assert.AreEqual(Vector3.one,target.HomeScale,"Canvas artwork must not shrink according to stock or inventory capacity");
        }
        private static Rect ProjectedFoodBounds(ManagementFoodView view,ManagementFoodTarget target)
        {
            var viewport=view.Viewport.rectTransform;var corners=new Vector3[4];target.Visual.rectTransform.GetWorldCorners(corners);
            Vector2 min=Vector2.one*float.MaxValue,max=Vector2.one*float.MinValue;
            foreach(var corner in corners)
            {
                Vector3 local=viewport.InverseTransformPoint(corner);
                var uv=new Vector2((local.x-viewport.rect.xMin)/viewport.rect.width,(local.y-viewport.rect.yMin)/viewport.rect.height);
                min=Vector2.Min(min,uv);max=Vector2.Max(max,uv);
            }
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        private static Text SignStock(string id)=>GameObject.Find("Stock "+id).GetComponent<Text>();
        private static Vector2 GrillFoodSize(Component game,string id)=>(Vector2)game.GetType()
            .GetMethod("ManagementFoodSize",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{id});
        private static Vector2 UiScreenPoint(Vector3 worldPoint)
        {
            var canvas=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>()?.GetComponentInParent<Canvas>();
            return RectTransformUtility.WorldToScreenPoint(canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null,worldPoint);
        }
        private static void AssertShopGrillSize(ManagementFoodView view,Component game)
        {
            foreach(var target in view.Targets)
            {
                AssertOriginalRawFoodSprite(game,target);
                var sprite=target.Visual.sprite;var size=target.Rect.rect.size;
                Assert.That(size.x/size.y,Is.EqualTo(sprite.rect.width/sprite.rect.height).Within(.01f),"Cards scale the exact original RAW uniformly");
                Assert.Greater(size.x,0);Assert.Greater(size.y,0);Assert.AreEqual(Vector3.one,target.HomeScale);
            }
        }
        private static void AssertShop2DOverlapLayout(ManagementFoodView view)
        {
            foreach(var target in view.Targets)
            {
                Assert.AreSame(target,view.Raycast(ExposedFoodPoint(view,target)),"Every card has a genuine exposed food surface");
                var rect=ProjectedFoodBounds(view,target);
                Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(-.001f));Assert.That(rect.xMax,Is.LessThanOrEqualTo(1.001f));
                Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(-.001f));Assert.That(rect.yMax,Is.LessThanOrEqualTo(1.001f));
                foreach(var other in view.Targets)if(other!=target)Assert.IsFalse(rect.Overlaps(ProjectedFoodBounds(view,other)),"Four catalogue cards must not overlap each other");
            }
        }
        private static PointerEventData BeginShopGesture(Vector2 point,int pointer)
        {
            Canvas.ForceUpdateCanvases();
            var data=new PointerEventData(EventSystem.current){position=point,pressPosition=point,pointerId=pointer,button=PointerEventData.InputButton.Left,eligibleForClick=true};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Assert.IsNotEmpty(hits);
            data.pointerPress=ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject);
            data.pointerDrag=ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject);
            Assert.IsNotNull(data.pointerDrag,"A card or its quantity button must receive the drag");
            ExecuteEvents.Execute(data.pointerPress,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.beginDragHandler);data.dragging=true;
            return data;
        }
        private static Vector2 ShopGestureEnd(ManagementFoodView view,Vector2 start,float x,float y)
        {
            var rect=view.Viewport.rectTransform;
            var center=UiScreenPoint(rect.position);
            return start+UiScreenPoint(rect.TransformPoint(new Vector3(rect.rect.width*x,rect.rect.height*y,0)))-center;
        }
        private static void FinishShopGesture(PointerEventData data,Vector2 end)
        {
            data.position=end;
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.dragHandler);
            bool isClick=data.eligibleForClick; // InputSystem computes candidacy before pointer-up.
            ExecuteEvents.Execute(data.pointerPress,data,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(data.pointerDrag,data,ExecuteEvents.endDragHandler);
            if(isClick)ExecuteEvents.Execute(data.pointerPress,data,ExecuteEvents.pointerClickHandler);
        }
        private static void SwipeShopPage(ManagementFoodView view,int direction,Vector2? from=null)
        {
            var point=from??UiScreenPoint(view.Viewport.rectTransform.position);
            var data=BeginShopGesture(point,970);
            FinishShopGesture(data,ShopGestureEnd(view,point,-direction*.3f,0));
        }

        private static PointerEventData StartCartDrag(ManagementFoodView view,ManagementFoodTarget target,int pointer)
        {
            Canvas.ForceUpdateCanvases();var point=ExposedFoodPoint(view,target);
            var data=new PointerEventData(EventSystem.current){position=point,pressPosition=point,pointerId=pointer,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Assert.IsNotEmpty(hits);Assert.AreSame(view.gameObject,hits[0].gameObject);
            ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.pointerDownHandler);Assert.IsTrue(ExecuteEvents.Execute(view.gameObject,data,ExecuteEvents.beginDragHandler));return data;
        }
        private static void TapFood(ManagementFoodView view,ManagementFoodTarget target)
        {
            Assert.IsNotNull(target);Canvas.ForceUpdateCanvases();var point=ExposedFoodPoint(view,target);
            var data=new PointerEventData(EventSystem.current){position=point,pressPosition=point,pointerId=940,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Assert.IsNotEmpty(hits);Assert.AreSame(view.gameObject,hits[0].gameObject,"A UI overlay must not cover the intended food surface "+target.UnitId);
            ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerDownHandler);
            Assert.IsTrue(ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler));
        }
        private static Vector3 FoodPixelLocalPoint(RectTransform rect,Vector2 uv)=>new Vector3(
            Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,uv.x),Mathf.Lerp(rect.rect.yMin,rect.rect.yMax,uv.y),0);
        private static Vector2 FoodPixelScreenPoint(ManagementFoodTarget target,Vector2 uv)=>UiScreenPoint(
            target.Visual.rectTransform.TransformPoint(FoodPixelLocalPoint(target.Visual.rectTransform,uv)));
        private static Vector2 FindRawSpritePixel(Sprite sprite,bool opaque)
        {
            // Production atlases are intentionally non-readable. QA reads the original imported texture through
            // a temporary GPU copy rather than changing import settings or fabricating a rectangular alpha mask.
            var texture=sprite.texture;var target=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;Texture2D pixels=null;
            try
            {
                Graphics.Blit(texture,target);RenderTexture.active=target;pixels=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
                pixels.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);pixels.Apply();
                // Keep transparent samples near actual corners; opaque samples are well within the cut.
                foreach(float y in opaque?new[]{.5f,.4f,.6f,.3f,.7f}:new[]{.015f,.985f,.04f,.96f,.08f,.92f})
                    foreach(float x in opaque?new[]{.5f,.4f,.6f,.3f,.7f}:new[]{.015f,.985f,.04f,.96f,.08f,.92f})
                    {
                        var rect=sprite.textureRect;int px=Mathf.Clamp(Mathf.FloorToInt(rect.x+x*rect.width),0,texture.width-1);
                        int py=Mathf.Clamp(Mathf.FloorToInt(rect.y+y*rect.height),0,texture.height-1);float alpha=pixels.GetPixel(px,py).a;
                        if(opaque?alpha>.95f:alpha<.02f)return new Vector2(x,y);
                    }
                Assert.Fail("Original raw sprite needs a genuine "+(opaque?"opaque interior":"transparent corner")+" for the alpha regression: "+sprite.name);
                return Vector2.zero;
            }
            finally
            {
                RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);if(pixels!=null)UnityEngine.Object.Destroy(pixels);
            }
        }
        [UnityTest]
        public IEnumerator Management_CounterErrorsAreReadableAndNeverPartiallyCharge()
        {
            Component game=null;yield return LoadGameScene(value=>game=value);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            ClickButton("CARNICERÍA");yield return null;
            AddToBasket("tira");yield return null;AddToBasket("chorizo");yield return null;
            var service=(ManagementService)GetField(game,"management");
            service.State.Balance=0;AddToBasket("chorizo");yield return null;
            AssertCounterErrorAndAtomicPayment(service,"No alcanzan las monedas");
            CaptureManagementFrame(game,"/tmp/asadito-counter-no-money.png");
            CaptureManagementFrame(game,"/tmp/asadito-shop-720-no-money.png",()=>AssertAllManagementTextFits(),720,1600);
            service.State.Balance=10000;service.State.Purchases.Add(new PurchaseRecord{FoodId="tira",Quantity=service.Config.Product("tira").Stock});
            AddToBasket("chorizo");yield return null;AssertCounterErrorAndAtomicPayment(service,"Sin stock");
            service.State.Purchases.Clear();
            for(int i=0;i<service.Config.FridgeCapacity;i++)service.State.Inventory.Add(new InventoryUnit{Id=service.State.NextUnitId++,FoodId="chorizo"});
            AddToBasket("chorizo");yield return null;AssertCounterErrorAndAtomicPayment(service,"Heladera llena");
            AssertAllManagementTextFits();
        }
        private static void AssertCounterErrorAndAtomicPayment(ManagementService service,string expected)
        {
            var pay=GameObject.Find("PAGAR Y SALIR").GetComponent<Button>();Assert.IsFalse(pay.interactable);
            Assert.IsTrue(Array.Exists(GameObject.Find("Management CARNICERÍA").GetComponentsInChildren<Text>(),text=>text.text.Contains(expected)));
            string before=JsonUtility.ToJson(service.State);
            pay.onClick.Invoke(); // Bypass disabled appearance to exercise the final transaction guard.
            Assert.AreEqual(before,JsonUtility.ToJson(service.State));
        }

        [UnityTest]
        public IEnumerator Management_CounterBasketCancelsAdjustsPaysAndReloadsIntoFridge()
        {
            Component game=null;yield return LoadGameScene(value=>game=value);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            ClickButton("CARNICERÍA");yield return null;
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>());Assert.IsFalse(GameObject.Find("PAGAR Y SALIR").GetComponent<Button>().interactable);
            CaptureManagementFrame(game,"/tmp/asadito-counter-empty.png");
            int balance=MvpSave.Load().Management.Balance;
            AddToBasket("chorizo");yield return null;AddToBasket("chorizo");yield return null;AddToBasket("tira");yield return null;
            AssertAllManagementTextFits();CaptureManagementFrame(game,"/tmp/asadito-counter-cart.png");
            ClickButton("DETALLE");yield return null;ClickButton("Restar carrito chorizo");yield return null;
            Assert.IsNotNull(GameObject.Find("TOTAL $280"));
            ClickButton("VACIAR");yield return null;Assert.IsFalse(GameObject.Find("PAGAR Y SALIR").GetComponent<Button>().interactable);
            AddToBasket("tira");yield return null;ClickButton("Volver carnicería");yield return null;
            Assert.AreEqual(balance,MvpSave.Load().Management.Balance);Assert.IsEmpty(MvpSave.Load().Management.Inventory);
            ClickButton("CARNICERÍA");yield return null;Assert.IsNotNull(GameObject.Find("TOTAL $0"));
            AddToBasket("tira");yield return null;AddToBasket("chorizo");yield return null;
            ClickButton("PAGAR Y SALIR");yield return new WaitForSecondsRealtime(.7f);
            Assert.IsNotNull(GameObject.Find("Management HELADERA"));Assert.AreEqual(balance-280,MvpSave.Load().Management.Balance);Assert.AreEqual(2,MvpSave.Load().Management.Inventory.Count);
            AssertAllManagementTextFits();CaptureManagementFrame(game,"/tmp/asadito-counter-paid-fridge.png");
            ResetSaveCache();Assert.AreEqual(balance-280,MvpSave.Load().Management.Balance);Assert.AreEqual(2,MvpSave.Load().Management.Inventory.Count);
            ClickButton("Preparar tira");yield return null;ClickButton("Preparar chorizo");yield return null;
            ClickButton("PREPARAR");yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(2,MvpSave.Load().Management.ActiveRun.Units.Count);
        }

        [UnityTest]
        public IEnumerator Management_IllustratedResultsDetailsAndActionsDoNotMutateSave()
        {
            Component game=null;yield return LoadGameScene(v=>game=v);ForcePortraitManagementCanvas(game);
            yield return new WaitForSecondsRealtime(.6f);
            var screen=GetField(game,"managementScreen");
            var service=(ManagementService)GetField(game,"management");string state=JsonUtility.ToJson(service.State);
            string persisted=PlayerPrefs.GetString(SaveKey);int retries=0,nexts=0;
            SetField(game,"activeGuests",MvpLevelCatalog.CreateGuests(6));SetField(game,"guestExpressions",new[]{3,1,2,3,1});
            string detail="";for(int i=0;i<10;i++)detail+="Invitado "+i+": cocción 50 · saciedad 75 · punto 60 · gusto 90 = 68/100\n";
            foreach(int stars in new[]{0,3,1})
            {
                bool recovery=stars==1,hasNext=stars==3;
                var result=new EconomicResult{Asador=68,Economy=80,Operations=90,Overall=74,Spending=560,Waste=180,Income=120,Profit=-440,Balance=12345,Stars=stars,Passed=stars>0,Recovery=recovery,
                    Advice="Revisá el punto de cada corte antes de servir. La carne que no preparaste sigue guardada para el próximo asado."};
                screen.GetType().GetMethod("Results").Invoke(screen,new object[]{result,(Action)(()=>retries++),(Action)(()=>nexts++),hasNext,detail});yield return new WaitForSecondsRealtime(.3f);
                var root=GameObject.Find("Management ¡ASADO COMPLETADO!");Assert.IsNotNull(root);
                Assert.AreEqual("Frame_ResultPanel",GameObject.Find("Illustrated result board").GetComponent<Image>().sprite.name);
                Assert.AreEqual(Image.Type.Sliced,GameObject.Find("Illustrated result wallet").GetComponent<Image>().type);
                Assert.AreEqual("74%",GameObject.Find("GENERAL 74%").GetComponent<Text>().text);
                Assert.AreEqual("$12.345",FindText("Result balance").text);Assert.AreEqual("−$440",FindText("Result profit").text);
                Assert.AreEqual(5,Array.FindAll(root.GetComponentsInChildren<Image>(),i=>i.name.StartsWith("Result guest ")).Length);
                for(int i=0;i<3;i++)Assert.AreEqual(i<stars?new Color32(247,176,37,255):new Color32(111,99,74,255),(Color32)GameObject.Find("Management star "+i).GetComponent<Image>().color);
                AssertAllManagementTextFits();
                CaptureManagementFrame(game,"/tmp/asadito-result-board-"+stars+".png");
                CaptureManagementFrame(game,"/tmp/asadito-result-board-narrow-"+stars+".png",null,720,1600);
                Assert.AreEqual(state,JsonUtility.ToJson(service.State));Assert.AreEqual(persisted,PlayerPrefs.GetString(SaveKey));
                TapVisibleButton(FindButton("VER DETALLE"));yield return new WaitForSecondsRealtime(.3f);
                var scroll=GameObject.Find("Result detail viewport").GetComponent<ScrollRect>();
                Assert.Greater(scroll.content.rect.height,scroll.viewport.rect.height,"Long guest breakdown must scroll instead of overflowing the popup");
                Assert.IsTrue(FindText("Result detailed breakdown").text.Contains(detail));
                Assert.IsNotNull(scroll.GetComponent<RectMask2D>());
                var underlying=FindButton("OTRO ASADO");var data=new PointerEventData(EventSystem.current){position=UiScreenPoint(underlying.transform.position)};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Assert.IsNotEmpty(hits);
                Assert.AreNotSame(underlying.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),"Details must block clicks through to the result action");
                data.position=UiScreenPoint(scroll.transform.position);data.scrollDelta=new Vector2(0,-10);
                ExecuteEvents.Execute(scroll.gameObject,data,ExecuteEvents.scrollHandler);yield return null;
                Assert.Less(scroll.verticalNormalizedPosition,1f,"Native scroll input must reach the detailed breakdown");
                CaptureManagementFrame(game,"/tmp/asadito-result-details-"+stars+".png");
                TapVisibleButton(FindButton("VOLVER AL RESUMEN"));yield return null;
                Assert.IsNull(GameObject.Find("Result detail overlay"));
                Assert.AreEqual(state,JsonUtility.ToJson(service.State));Assert.AreEqual(persisted,PlayerPrefs.GetString(SaveKey));
                TapVisibleButton(FindButton(hasNext?"SIGUIENTE":"OTRO ASADO"));yield return null;
                Assert.IsNull(GameObject.Find("Management ¡ASADO COMPLETADO!"));
            }
            Assert.AreEqual(2,retries);Assert.AreEqual(1,nexts);
            Assert.AreEqual(state,JsonUtility.ToJson(service.State));Assert.AreEqual(persisted,PlayerPrefs.GetString(SaveKey));
        }

        [UnityTest]
        public IEnumerator Management_BuyPrepareCookServeAndPersistBalance()
        {
            var save = MvpSaveData.Migrate(new MvpSaveData());
            for (int completed = 1; completed < 5; completed++) save.RecordLevelResult(completed, 130, 1);
            MvpSave.Save(save);
            Component game = null;
            yield return LoadGameScene(value => game = value);
            SetField(game, "SimulationTimeScale", 120f);
            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR"); yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 5"); yield return new WaitForSecondsRealtime(.5f);
            ClickButton("IR A LA PARRILLA"); yield return new WaitForSecondsRealtime(.5f);
            Assert.IsNotNull(GameObject.Find("Management PRÓXIMO ASADO"));
            CaptureManagementFrame(game, "/tmp/asadito-management-planning.png");
            ClickButton("CARNICERÍA"); yield return null;
            var library = Resources.Load<ScriptableObject>("ManagementArt");
            Assert.IsNotNull(library, "The selected prepared management sprites must load through the reference catalog.");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>());
            CaptureManagementFrame(game, "/tmp/asadito-management-shop.png"); yield return new WaitForSecondsRealtime(.2f);
            AddToBasket("tira"); yield return null;
            AddToBasket("tira"); yield return null;
            AddToBasket("chorizo"); yield return null;
            AddToBasket("chorizo"); yield return null;
            ClickButton("PAGAR Y SALIR"); yield return new WaitForSecondsRealtime(.7f);
            Assert.AreEqual(4, MvpSave.Load().Management.Inventory.Count);
            int purchaseBalance = MvpSave.Load().Management.Balance;
            Assert.AreEqual(ManagementConfig.Load().InitialBalance - 560, purchaseBalance);
            ClickButton("Preparar tira"); yield return null;
            ClickButton("Preparar tira"); yield return null;
            ClickButton("Preparar chorizo"); yield return null;
            ClickButton("Preparar chorizo"); yield return null;
            CaptureManagementFrame(game, "/tmp/asadito-management-fridge.png"); yield return new WaitForSecondsRealtime(.2f);
            ClickButton("PREPARAR"); yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(0, MvpSave.Load().Management.Inventory.Count);
            Assert.AreEqual(4, MvpSave.Load().Management.ActiveRun.Units.Count);
            // A cold app restart must preserve paid prepared food, not silently mark it as waste.
            ResetSaveCache();
            yield return LoadGameScene(value => game = value);
            SetField(game, "SimulationTimeScale", 120f);
            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR"); yield return new WaitForSecondsRealtime(.5f);
            Assert.IsNotNull(MvpSave.Load().Management.ActiveRun);
            ClickButton("NIVEL 5"); yield return new WaitForSecondsRealtime(.5f);
            ClickButton("IR A LA PARRILLA"); yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(purchaseBalance, MvpSave.Load().Management.Balance);
            Assert.AreEqual(0, MvpSave.Load().Management.Inventory.Count);
            Assert.AreEqual(0, MvpSave.Load().Management.PendingWaste);
            Assert.IsNull(GameObject.Find("Management PRÓXIMO ASADO"), "Prepared asado resumes directly without buying twice.");
            yield return CookAndPlateOrder(game, 90f);
            TapBoard(game); yield return new WaitForSecondsRealtime(2f);
            Assert.IsNotNull(GameObject.Find("Management ¡ASADO COMPLETADO!"));
            Assert.AreEqual(0f, ((CanvasGroup)GetField(game,"gameplayCanvasGroup")).alpha, "Food overlay must not render over management results.");
            Assert.Greater(MvpSave.Load().Management.Balance, purchaseBalance);
            Assert.IsNull(MvpSave.Load().Management.ActiveRun);
            Assert.AreEqual(1, MvpSave.Load().Management.Cycle);
            Assert.AreEqual(0, MvpSave.Load().Management.FreshnessCycle);
            Canvas.ForceUpdateCanvases();
            foreach (Text label in GameObject.Find("Management ¡ASADO COMPLETADO!").GetComponentsInChildren<Text>())
                Assert.LessOrEqual(label.preferredHeight, label.rectTransform.rect.height + 1f,
                    "Management result text must not truncate: " + label.text);
            CaptureManagementFrame(game, "/tmp/asadito-management-result.png"); yield return new WaitForSecondsRealtime(.3f);
            ResetSaveCache(); Assert.Greater(MvpSave.Load().Management.Balance, purchaseBalance);
            ClickButton("OTRO ASADO"); yield return null;
            Assert.IsNotNull(GameObject.Find("Management PRÓXIMO ASADO"));
        }

        [UnityTest]
        public IEnumerator Management_RawServiceCannotUnlockOrFarmCoins()
        {
            Component game=null;yield return LoadGameScene(value=>game=value);
            yield return EnterLevelOne(game,true);SetField(game,"SimulationTimeScale",0f);
            LoadAllRawFoodToGrill(game,0);yield return null;
            for(int i=0;i<2;i++)
            {
                DragFoodToBoard(game,i,950+i);
                float started=Time.realtimeSinceStartup;
                while(ReadField<bool>(game,"platingInProgress") && Time.realtimeSinceStartup-started<3f) yield return null;
            }
            Assert.IsTrue(CanServeFromBoard(game));TapBoard(game);yield return new WaitForSecondsRealtime(2f);
            var result=(EconomicResult)GetField(game,"economicResult");
            Assert.AreEqual(0,result.Stars);Assert.IsFalse(result.Passed);Assert.Less(result.Profit,0);
            Assert.Greater(result.Waste,0);Assert.Less(result.Cooking,ManagementConfig.Load().MinimumCookingForStar);
            Assert.AreEqual(1,MvpSave.Load().MaxUnlockedLevel);
            Assert.IsNull(GameObject.Find("SIGUIENTE"));
            CaptureManagementFrame(game,"/tmp/asadito-level1-raw-result.png");
        }

        [UnityTest]
        public IEnumerator Management_DebutRecoveryAndGuideSurviveReloadWithoutResettingMoney()
        {
            var save=MvpSaveData.Migrate(new MvpSaveData());save.Management.Balance=0;MvpSave.Save(save);
            Component game=null;yield return LoadGameScene(value=>game=value);
            SetField(game,"SimulationTimeScale",120f);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            Assert.IsTrue(FindButton("HELADERA").interactable,"Guided debut must not block recovery when broke.");
            ClickButton("HELADERA");yield return null;ClickButton("CAJA DEL ASADOR");yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(0,MvpSave.Load().Management.Balance);Assert.IsTrue(MvpSave.Load().Management.ActiveRun.Recovery);
            yield return CookAndPlateOrder(game,90f);TapBoard(game);yield return new WaitForSecondsRealtime(2f);
            var result=(EconomicResult)GetField(game,"economicResult");Assert.Greater(result.Income,0);Assert.AreEqual(1,result.Stars);
            Assert.IsTrue(MvpSave.Load().Management.ManagementTutorialCompleted);
            int balance=MvpSave.Load().Management.Balance;
            ResetSaveCache();yield return LoadGameScene(value=>game=value);
            Assert.AreEqual(balance,MvpSave.Load().Management.Balance);
            yield return new WaitForSecondsRealtime(.6f);ClickButton("ENTRAR");yield return new WaitForSecondsRealtime(.5f);
            ClickButton("NIVEL 1");yield return new WaitForSecondsRealtime(.5f);ClickButton("IR A LA PARRILLA");yield return null;
            ClickButton("CARNICERÍA");yield return null;
            ClickButton("Comprar chorizo");yield return null;
            Assert.AreEqual(balance,MvpSave.Load().Management.Balance,"Preview never spends.");
            ClickButton("Volver carnicería");yield return null;ClickButton("CARNICERÍA");yield return null;
            Assert.IsFalse(GameObject.Find("PAGAR Y SALIR").GetComponent<Button>().interactable,"Leaving clears the unpaid basket.");
            CaptureManagementFrame(game,"/tmp/asadito-level1-shop.png");
        }

        private static void CaptureManagementFrame(Component game,string path,Action afterLayout=null,int width=1080,int height=1920)
        {
            Assert.Greater(width,0);Assert.Greater(height,0);
            var foods=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();
            Canvas canvas=(Canvas)GetField(game,"canvas");
            var cameraObject=new GameObject("Management QA camera");var camera=cameraObject.AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=height*.5f;camera.enabled=false;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            var target=new RenderTexture(width,height,24);camera.targetTexture=target;
            var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;var previousDistance=canvas.planeDistance;
            var previous=RenderTexture.active;Texture2D image=null;
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
                Canvas.ForceUpdateCanvases();if(foods!=null)foods.RefreshLayout();afterLayout?.Invoke();camera.Render();
                // The first real offscreen UI render may rebuild the font atlas; render again after layout.
                Canvas.ForceUpdateCanvases();if(foods!=null)foods.RefreshLayout();afterLayout?.Invoke();camera.Render();
                RenderTexture.active=target;image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previous;canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=previousDistance;
                Canvas.ForceUpdateCanvases();if(foods!=null)foods.RefreshLayout();afterLayout?.Invoke();
                camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);
                if(image!=null)UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(cameraObject);
            }
        }

        private static IEnumerator CookAndPlateOrder(Component game, float timeoutSeconds)
        {
            Array portions = (Array)GetField(game, "portions");
            float originalScale = ReadField<float>(game, "SimulationTimeScale");
            LoadAllRawFoodToGrill(game, 0);
            yield return null;

            int exposedFace = ((FoodState)GetField(portions.GetValue(0), "State")).ExposedFace;
            TapFoodPiece(game, 0, 600);
            Assert.AreEqual(exposedFace, ((FoodState)GetField(portions.GetValue(0), "State")).ExposedFace,
                "Tapping grilled food must not flip it.");

            float startedAt = Time.realtimeSinceStartup;
            while (ReadField<int>(game, "trayCount") < portions.Length && Time.realtimeSinceStartup - startedAt < timeoutSeconds)
            {
                bool platedPiece = false;
                bool nearPointBand = false;
                for (int i = 0; i < portions.Length; i++)
                {
                    object portion = portions.GetValue(i);
                    if (ReadField<bool>(portion, "OnTray")) continue;
                    FoodState state = (FoodState)GetField(portion, "State");
                    FoodCookProfile profile = (FoodCookProfile)GetField(portion, "Profile");
                    DonenessBand pointBand = FindDonenessBand(profile, Doneness.A_Punto);
                    if (state.CoreTemperatureC >= pointBand.MinimumCoreC - 2f &&
                        state.CoreTemperatureC <= pointBand.MaximumCoreC + 1f)
                        nearPointBand = true;

                    if (FoodCookingModel.GetDoneness(state, profile) != Doneness.A_Punto) continue;
                    DragFoodToBoard(game, i, 700 + i);
                    float plateStartedAt = Time.realtimeSinceStartup;
                    while (ReadField<bool>(game, "platingInProgress") && Time.realtimeSinceStartup - plateStartedAt < 3f)
                        yield return null;
                    Assert.IsTrue(ReadField<bool>(portion, "OnTray"), profile.FoodId + " must reach the serving board while at its own point.");
                    platedPiece = true;
                    break;
                }

                // Slow only near the narrow A_Punto windows so the PlayMode driver can plate each
                // concurrent cut before another piece passes its individual target.
                SetField(game, "SimulationTimeScale", nearPointBand ? Mathf.Min(originalScale, 3f) : originalScale);
                if (!platedPiece) yield return new WaitForSecondsRealtime(.01f);
            }

            SetField(game, "SimulationTimeScale", originalScale);
            Assert.AreEqual(portions.Length, ReadField<int>(game, "trayCount"),
                "Every concurrently cooking portion must independently reach its A_Punto band.");
            for (int i = 0; i < portions.Length; i++)
                Assert.IsTrue(ReadField<bool>(portions.GetValue(i), "OnTray"), "Every cooked cut must be plated.");
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
            Assert.AreEqual("ParrillaTopDownGameplay", sprite.texture.name,
                "The redesigned top-down gameplay scene should be the primary background, not the legacy fallback.");

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

        private static IEnumerator EnterLevelOne(Component game, bool capture = false)
        {
            // Menu CTAs become interactable after the short, staggered cover entrance.
            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("IR A LA PARRILLA");
            yield return new WaitForSecondsRealtime(.1f);
            yield return BuyAndPrepareCurrentOrder(game,capture);
            AssertGrillReadyWithoutCoal(game);
            Assert.That(FindText("Estado coccion").text, Does.Contain("ARRASTRÁ"));
            Assert.AreEqual(2, ((Array)GetField(game, "portions")).Length);
            Assert.AreEqual(2, ((Array)GetField(game, "activeGuests")).Length);
        }

        private static void AddToBasket(string id)
        {
            int before=MvpSave.Load().Management.Inventory.Count;
            int balance=MvpSave.Load().Management.Balance;
            ClickButton("Comprar "+id);
            Assert.AreEqual(before,MvpSave.Load().Management.Inventory.Count,"Selection never spends or creates inventory.");
            Assert.AreEqual(balance,MvpSave.Load().Management.Balance);
        }
        private static IEnumerator BuyAndPrepareCurrentOrder(Component game, bool capture = false)
        {
            var order=MvpLevelCatalog.Get(ReadField<int>(game,"currentLevelNumber"));
            Assert.IsNotNull(GameObject.Find("Management PRÓXIMO ASADO"));
            AssertAllManagementTextFits();
            if(capture) CaptureManagementFrame(game,"/tmp/asadito-level1-planning.png");
            ClickButton("CARNICERÍA");yield return null;
            if(capture) CaptureManagementFrame(game,"/tmp/asadito-level1-guided-shop.png");
            var needs=new Dictionary<string,int>();
            foreach(var id in order.FoodIds){if(!needs.ContainsKey(id))needs[id]=0;needs[id]++;}
            foreach(var pair in needs)
            {
                var service=new ManagementService(MvpSave.Load().Management,ManagementConfig.Load());
                int missing=Mathf.Max(0,pair.Value-service.Available(pair.Key));
                for(int i=0;i<missing;i++){AddToBasket(pair.Key);yield return null;}
            }
            AssertAllManagementTextFits();
            if(GameObject.Find("PAGAR Y SALIR").GetComponent<Button>().interactable) ClickButton("PAGAR Y SALIR");
            else ClickButton("HELADERA");
            yield return new WaitForSecondsRealtime(.7f);
            foreach(var id in order.FoodIds){ClickButton("Preparar "+id);yield return null;}
            AssertAllManagementTextFits();
            if(capture) CaptureManagementFrame(game,"/tmp/asadito-level1-fridge.png");
            ClickButton("PREPARAR");yield return new WaitForSecondsRealtime(.5f);
        }
        private static void AssertAllManagementTextFits()
        {
            Canvas.ForceUpdateCanvases();
            foreach(var label in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                if(label.transform.root!=null && label.GetComponentInParent<Canvas>()!=null && label.transform.parent.name.StartsWith("Management "))
                {
                    Assert.LessOrEqual(label.preferredHeight,label.rectTransform.rect.height+1f,"Truncated: "+label.text);
                    if(!string.IsNullOrEmpty(label.text)) Assert.Greater(label.cachedTextGenerator.vertexCount,0,"Empty rendered text mesh: "+label.text);
                }
        }

        private static void AssertGrillReadyWithoutCoal(Component game, bool assertTutorialStep = true)
        {
            Assert.IsNull(GameObject.Find("PRENDER CARBÓN"));
            Assert.IsNull(GameObject.Find("CARBÓN ENCENDIDO"));
            Assert.IsNull(GameObject.Find("Mapa de calor carbón 8x6"));
            Assert.IsNull(GameObject.Find("Destello de encendido"));
            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 8; x++)
                Assert.IsNull(GameObject.Find("Brasa " + x + "," + y));
            Assert.AreEqual(210f, ((GrillHeatModel)GetField(game, "grillHeat")).GetTemperatureC(), .001f);
            if (assertTutorialStep)
                Assert.That(FindText("Estado coccion").text, Does.Contain("ARRASTRÁ"));
        }

        private static DonenessBand FindDonenessBand(FoodCookProfile profile, Doneness doneness)
        {
            if (profile?.DonenessBands != null)
                foreach (DonenessBand band in profile.DonenessBands)
                    if (band.Doneness == doneness) return band;
            Assert.Fail(profile?.FoodId + " is missing its " + doneness + " temperature band.");
            return default;
        }

        private static bool CanServeFromBoard(Component game)
        {
            return (bool)game.GetType().GetProperty("CanServeFromBoard").GetValue(game);
        }

        private static void TapBoard(Component game)
        {
            Image board = (Image)GetField(game, "servingBoardImage");
            Component touch = FindComponent(board.gameObject, "Asadito.Runtime.ServingBoardTouch");
            PointerEventData pointer = MakePointer(board.rectTransform, 900);
            pointer.clickCount = 1;
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.pointerClickHandler),
                "Only the physical cutting board should receive the tap-to-serve input.");
        }

        private static void DragFoodToBoard(Component game, int index, int pointerId)
        {
            RectTransform board = ((Image)GetField(game, "servingBoardImage")).rectTransform;
            Vector2 boardPoint = RectTransformUtility.WorldToScreenPoint(null, board.position);
            DragFoodPiece(game, index, boardPoint, pointerId);
        }

        private static void DragRawFoodToGrill(Component game, int index, int pointerId, float normalizedX = .5f, float normalizedY = .5f)
        {
            RectTransform grill = (RectTransform)GetField(game, "grillAreaRect");
            Vector3 local = new Vector3((normalizedX - .5f) * grill.rect.width, (normalizedY - .5f) * grill.rect.height, 0f);
            Vector2 grillPoint = RectTransformUtility.WorldToScreenPoint(null, grill.TransformPoint(local));
            DragFoodPiece(game, index, grillPoint, pointerId);
        }

        private static void LoadAllRawFoodToGrill(Component game, int activatingIndex)
        {
            Array portions = (Array)GetField(game, "portions");
            Vector2[] grillHomes = (Vector2[])game.GetType().GetMethod("CalculateInitialFoodPositions",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            for (int i = 0; i < portions.Length; i++)
            {
                if (i == activatingIndex || !ReadField<bool>(portions.GetValue(i), "OnSourceTray")) continue;
                DragRawFoodToGrill(game, i, 100 + i, grillHomes[i].x, grillHomes[i].y);
            }
            if (ReadField<bool>(portions.GetValue(activatingIndex), "OnSourceTray"))
                DragRawFoodToGrill(game, activatingIndex, 300 + activatingIndex,
                    grillHomes[activatingIndex].x, grillHomes[activatingIndex].y);
            else if (ReadField<int>(game, "activePortion") != activatingIndex)
            {
                Assert.IsTrue((bool)game.GetType().GetMethod("SelectFoodPiece").Invoke(game, new object[] { activatingIndex }),
                    "The requested portion should become the selected interaction target.");
            }
            int level = ReadField<int>(game, "currentLevelNumber");
            for (int i = 0; i < portions.Length; i++)
            {
                object portion = portions.GetValue(i);
                Assert.IsTrue(ReadField<bool>(portion, "Started") && !ReadField<bool>(portion, "OnSourceTray"),
                    "L" + level + " must load every raw portion before cooking; failed portion index " + i + ".");
            }
            Assert.IsTrue(ReadField<bool>(game, "cooking"), "L" + level + " should cook all placed pieces while they remain on the grill.");
            Assert.AreEqual(activatingIndex, ReadField<int>(game, "activePortion"),
                "L" + level + " should activate the requested portion after emptying the raw tray.");
        }

        private static void DragActiveFoodToDifferentGrillPosition(Component game, int index, int pointerId)
        {
            Array portions = (Array)GetField(game, "portions");
            Vector2 start = ReadField<Vector2>(portions.GetValue(index), "Position");
            RectTransform grill = (RectTransform)GetField(game, "grillAreaRect");
            Vector2[] candidates = { new Vector2(.72f, .18f), new Vector2(.28f, .18f),
                new Vector2(.72f, .82f), new Vector2(.28f, .82f), new Vector2(.5f, .5f) };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (Vector2.Distance(start, candidates[i]) < .11f) continue;
                Vector3 local = new Vector3((candidates[i].x - .5f) * grill.rect.width,
                    (candidates[i].y - .5f) * grill.rect.height, 0f);
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, grill.TransformPoint(local));
                DragFoodPiece(game, index, screen, pointerId + i);
                if (Vector2.Distance(start, ReadField<Vector2>(portions.GetValue(index), "Position")) > .1f)
                    return;
            }
            Assert.Fail("The selected portion should move to at least one free touch target on the grill.");
        }

        private static void TapFoodPiece(Component game, int index, int pointerId)
        {
            RectTransform[] targets = (RectTransform[])GetField(game, "portionHitTargets");
            Component touch = FindComponent(targets[index].gameObject, "Asadito.Runtime.FoodPieceTouch");
            PointerEventData pointer = MakePointer(targets[index], pointerId);
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.pointerDownHandler),
                "The food hit target must receive pointer-down directly.");
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.pointerUpHandler),
                "A food tap must release its pointer cleanly.");
        }

        private static void DragFoodPiece(Component game, int index, Vector2 destination, int pointerId)
        {
            RectTransform[] targets = (RectTransform[])GetField(game, "portionHitTargets");
            Component touch = FindComponent(targets[index].gameObject, "Asadito.Runtime.FoodPieceTouch");
            PointerEventData pointer = MakePointer(targets[index], pointerId);
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.pointerDownHandler));
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.beginDragHandler));
            pointer.position = destination;
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.dragHandler));
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.pointerUpHandler));
            Assert.IsTrue(ExecuteEvents.Execute(touch.gameObject, pointer, ExecuteEvents.endDragHandler));
        }

        private static PointerEventData MakePointer(RectTransform target, int pointerId)
        {
            return new PointerEventData(EventSystem.current)
            {
                pointerId = pointerId,
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, target.position)
            };
        }

        private static Button FindButton(string name)
        {
            GameObject go = null;
            foreach(var candidate in UnityEngine.Object.FindObjectsByType<Button>())
                if(candidate.name==name&&candidate.transform.parent.name.StartsWith("Management ")){go=candidate.gameObject;break;}
            if(go==null)go=GameObject.Find(name);
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

        private static void AssertRectInside(RectTransform container, RectTransform child, string label)
        {
            var containerCorners = new Vector3[4];
            var childCorners = new Vector3[4];
            container.GetWorldCorners(containerCorners);
            child.GetWorldCorners(childCorners);
            for (int i = 0; i < childCorners.Length; i++)
            {
                Assert.GreaterOrEqual(childCorners[i].x, containerCorners[0].x - .1f, label + " must remain inside the popup's left edge.");
                Assert.LessOrEqual(childCorners[i].x, containerCorners[2].x + .1f, label + " must remain inside the popup's right edge.");
                Assert.GreaterOrEqual(childCorners[i].y, containerCorners[0].y - .1f, label + " must remain inside the popup's bottom edge.");
                Assert.LessOrEqual(childCorners[i].y, containerCorners[2].y + .1f, label + " must remain inside the popup's top edge.");
            }
        }

        private static void ClickButton(string name)
        {
            if(name.StartsWith("Comprar ")||name.StartsWith("Preparar "))
            {
                var view=UnityEngine.Object.FindAnyObjectByType<ManagementFoodView>();Assert.IsNotNull(view);
                string id=name.Substring(name.IndexOf(' ')+1);
                var t=view.Targets.Find(x=>x.FoodId==id&&(!view.IsFridge||!x.Selected));TapFood(view,t);return;
            }
            ClickButton(FindButton(name));
        }

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
