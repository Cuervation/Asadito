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
        public IEnumerator FirstPlayable_CookServePersistsResultButKeepsLevelTwoLocked()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game, "SampleScene must start the Asadito runtime controller.");
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 1200f); // Accelerate simulation only for deterministic test duration.

            yield return EnterLevelOne(game);
            AssertGrillReadyWithoutCoal(game);

            yield return CookAndPlate(game, 0, "tira", 25f, true);
            yield return CookAndPlate(game, 1, "chorizo", 25f, true);

            Assert.IsTrue(CanServeFromBoard(game), "Serving unlocks on the physical board after every portion is plated.");
            TapBoard(game);
            yield return new WaitForSecondsRealtime(1.8f);

            Assert.NotNull(GameObject.Find("Fin de nivel"), "Serving must reach the result screen.");
            Text resultScore = GameObject.Find("Resultado puntos").GetComponent<Text>();
            Assert.That(resultScore.text, Does.Contain("PUNTOS"));
            Assert.IsNotNull(GameObject.Find("REINTENTAR"));
            Button next = GameObject.Find("BLOQUEADO").GetComponent<Button>();
            Assert.IsFalse(next.interactable, "Level 2 stays unavailable until it is approved for play.");
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
        }

        [UnityTest]
        public IEnumerator FirstPlayable_RetryResetsFoodAndTable()
        {
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 1200f);

            yield return EnterLevelOne(game);
            AssertGrillReadyWithoutCoal(game);
            yield return CookAndPlate(game, 0, "tira", 25f, true);
            yield return CookAndPlate(game, 1, "chorizo", 25f, true);
            TapBoard(game);
            yield return new WaitForSecondsRealtime(1.8f);

            Button retry = FindButton("REINTENTAR");
            Assert.IsTrue(retry.interactable);
            ClickButton(retry);
            yield return new WaitForSecondsRealtime(.1f);

            GrillHeatModel grill = (GrillHeatModel)GetField(game, "grillHeat");
            Assert.AreEqual(210f, grill.GetTemperatureC(), .001f, "Retry keeps the always-hot grill ready for the next tanda.");
            Assert.AreEqual(0, ReadField<int>(game, "trayCount"));
            Assert.IsNull(GameObject.Find("Fin de nivel"));
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
            Assert.AreEqual(-1, ReadField<int>(game, "activePortion"), "The loading step doesn't begin cooking before the source tray is empty.");
            Assert.IsFalse(ReadField<bool>(game, "cooking"));
            Assert.IsTrue(rawSurface.gameObject.activeInHierarchy);
            Assert.IsFalse(boardSurface.gameObject.activeInHierarchy);
            Vector2 waitingPosition = ReadField<Vector2>(portions.GetValue(0), "Position");
            DragActiveFoodToDifferentGrillPosition(game, 0, 45);
            Assert.Greater(Vector2.Distance(waitingPosition, ReadField<Vector2>(portions.GetValue(0), "Position")), .1f,
                "A loaded piece can still be repositioned while the remaining raw order is transferred.");
            float waitingTemperature = ((FoodState)GetField(portions.GetValue(0), "State")).CoreTemperatureC;
            TapFoodPiece(game, 1, 43);
            Assert.IsFalse(ReadField<bool>(portions.GetValue(1), "Started"), "Tapping raw food must not transfer it to the grill.");
            DragRawFoodToGrill(game, 1, 44);
            Assert.IsTrue(ReadField<bool>(portions.GetValue(1), "Started"));
            Assert.IsFalse(rawSurface.gameObject.activeInHierarchy, "The aluminum tray disappears after its last piece leaves.");
            Assert.IsTrue(boardSurface.gameObject.activeInHierarchy, "The board replaces the tray in the same spot.");
            Assert.AreEqual(1, ReadField<int>(game, "activePortion"));
            yield return new WaitForSecondsRealtime(.3f);
            Assert.Greater(((FoodState)GetField(portions.GetValue(1), "State")).CoreTemperatureC, 20f,
                "Cooking begins only after all raw pieces are on the grill.");
            Assert.That(((FoodState)GetField(portions.GetValue(0), "State")).CoreTemperatureC,
                Is.EqualTo(waitingTemperature).Within(.01f), "Unselected portions keep their size and wait their turn on the grill.");
            Assert.That(Vector3.Distance(images[0].rectTransform.localScale, Vector3.one), Is.LessThan(.001f),
                "Selection feedback must not permanently enlarge food on the grill.");
            Assert.IsFalse((bool)game.GetType().GetMethod("SelectFoodPiece").Invoke(game, new object[] { -1 }),
                "Out-of-range/empty selections must be harmless.");
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

            ClickButton("PAUSA");
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

            ClickButton("PAUSA");
            ClickButton("VOLVER A NIVELES");
            yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.NotNull(GameObject.Find("Seleccion de nivel"), "Pause should return to the level selector.");
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
            float initialCore = state.CoreTemperatureC;
            TapFoodPiece(game, 0, 71);
            Assert.IsFalse(ReadField<bool>(portion, "Started"), "Tapping raw meat leaves it on the source tray.");
            DragRawFoodToGrill(game, 1, 72);
            Assert.IsTrue(((Image)GetField(game, "rawTrayImage")).gameObject.activeInHierarchy,
                "The source tray must stay while it still contains raw food.");
            Assert.IsFalse(((Image)GetField(game, "servingBoardImage")).gameObject.activeInHierarchy,
                "The serving board cannot replace a tray that still holds food.");
            Assert.That(state.CoreTemperatureC, Is.EqualTo(initialCore).Within(.01f),
                "A piece waits without heat while the order is still being transferred.");
            DragRawFoodToGrill(game, 0, 73);
            Assert.IsTrue(ReadField<bool>(portion, "Started"));
            Assert.IsFalse(((Image)GetField(game, "rawTrayImage")).gameObject.activeInHierarchy);
            Assert.IsTrue(((Image)GetField(game, "servingBoardImage")).gameObject.activeInHierarchy);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.Greater(state.CoreTemperatureC, initialCore, "Food must heat directly without an ignition step.");
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
            levelSave.MaxUnlockedLevel = 2;
            levelGame.GetType().GetMethod("RefreshLevelCards", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(levelGame, null);
            Assert.IsFalse(levelTwo.interactable, "Persisted progression must not expose Level 2 before it is approved.");
            Assert.IsTrue(levelTwo.transform.Find("Candado nivel 2").gameObject.activeSelf);
            Assert.IsTrue(levelTwo.transform.Find("Disabled nivel 2").gameObject.activeSelf);
            levelGame.GetType().GetMethod("SelectLevel", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(levelGame, new object[] { 2 });
            Assert.AreEqual(1, ReadField<int>(levelGame, "currentLevelNumber"), "Direct selection must honor the same lock as the disabled card.");
            Assert.IsNull(GameObject.Find("Seleccion ayuda"),
                "The level-selection screen should not add a secondary block of descriptive copy.");
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            Assert.AreEqual("Asadito UI Icon Guest", GameObject.Find("Icon comensales intro").GetComponent<Image>().sprite.name);
            Assert.AreEqual("tira_ideal", GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<Image>().sprite.name);
            Assert.AreEqual("chorizo_ideal", GameObject.Find("Icon comida intro CHORIZO").GetComponent<Image>().sprite.name);
            Transform introPopup = GameObject.Find("Popup nivel").transform;
            RectTransform introPopupRect = introPopup.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(640f, 660f), introPopupRect.rect.size,
                "The centered level intro should be compact around its content, not nearly screen-sized.");
            Assert.AreEqual(.58f, introPopup.GetComponent<Image>().color.a, .001f, "The white popup should be more translucent than the previous 30%-transparent version.");
            Assert.NotNull(introPopup.GetComponent<Outline>(), "Frosted glass should have a subtle bright rim.");
            Assert.NotNull(introPopup.Find("Reflejo vidrio popup"), "The translucent card should have a soft glass sheen.");
            Assert.NotNull(introPopup.Find("Icono fuego vidrio"), "Glass style should use the small centered warm accent.");
            Assert.NotNull(introPopup.Find("Separador menu popup"), "The order and instructions need a clear visual divider.");
            Assert.AreEqual(introPopup, GameObject.Find("Intro título").transform.parent);
            Assert.AreEqual(introPopup, GameObject.Find("IR A LA PARRILLA").transform.parent);
            Text popupTitle = GameObject.Find("Intro título").GetComponent<Text>();
            Text popupGuests = GameObject.Find("Intro comensales").GetComponent<Text>();
            Text popupMenu = GameObject.Find("Intro menu").GetComponent<Text>();
            Text popupObjective = GameObject.Find("Intro objetivo").GetComponent<Text>();
            Assert.GreaterOrEqual(popupTitle.fontSize, 48, "The compact title must remain readable on a phone.");
            Assert.GreaterOrEqual(popupGuests.fontSize, 32, "The guest count must remain comfortably legible.");
            Assert.GreaterOrEqual(popupMenu.fontSize, 26, "Order copy must not be tiny.");
            Assert.GreaterOrEqual(popupObjective.fontSize, 28, "Instructions must be comfortably legible.");
            Assert.LessOrEqual(popupTitle.preferredHeight, popupTitle.rectTransform.rect.height + 1f, "The level title must fit without clipping.");
            Assert.LessOrEqual(popupGuests.preferredHeight, popupGuests.rectTransform.rect.height + 1f, "The guest count must fit without clipping.");
            Assert.LessOrEqual(popupMenu.preferredHeight, popupMenu.rectTransform.rect.height + 1f, "The order must fit without clipping.");
            Assert.LessOrEqual(popupObjective.preferredHeight, popupObjective.rectTransform.rect.height + 1f, "The instructions must fit without clipping.");
            Assert.AreEqual(new Vector2(128f, 96f), GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<RectTransform>().sizeDelta,
                "Food illustrations should stay distinct while fitting the compact card.");
            Button introStart = FindButton("IR A LA PARRILLA");
            Assert.AreEqual(new Vector2(460f, 88f), introStart.GetComponent<RectTransform>().rect.size,
                "The primary action should fit the compact popup without dominating it.");
            AssertRectInside(introPopupRect, popupTitle.rectTransform, "Title");
            AssertRectInside(introPopupRect, popupGuests.rectTransform, "Guest count");
            AssertRectInside(introPopupRect, popupMenu.rectTransform, "Order");
            AssertRectInside(introPopupRect, popupObjective.rectTransform, "Instructions");
            AssertRectInside(introPopupRect, introStart.GetComponent<RectTransform>(), "Primary action");
            AssertRectInside(introPopupRect, GameObject.Find("Icon comida intro TIRA DE ASADO").GetComponent<RectTransform>(), "Food icon");
            var objectiveCorners = new Vector3[4];
            var ctaCorners = new Vector3[4];
            popupObjective.rectTransform.GetWorldCorners(objectiveCorners);
            introStart.GetComponent<RectTransform>().GetWorldCorners(ctaCorners);
            Assert.Greater(objectiveCorners[0].y, ctaCorners[1].y,
                "The instructions need breathing room above the CTA and must not overlap it.");
            Assert.AreEqual("Asadito UI Icon Next", introStart.transform.Find("Icono accion IR A LA PARRILLA").GetComponent<Image>().sprite.name);
            Assert.AreEqual("Arcade Gold Button", ((Image)introStart.targetGraphic).sprite.name,
                "The selected glass style must preserve the app's current CTA button styling.");
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
            yield return CookAndPlate(game, 0, "tira", 100f, true);
            yield return CookAndPlate(game, 1, "chorizo", 60f, true);
            Assert.IsTrue(CanServeFromBoard(game));
            TapBoard(game);
            yield return new WaitForSecondsRealtime(2f);
            Assert.NotNull(GameObject.Find("Fin de nivel"));

            double elapsedSeconds = (DateTime.UtcNow - started).TotalSeconds;
            Assert.LessOrEqual(elapsedSeconds, 180d, "Automated L1 should remain under the 2–3 minute First Playable tuning ceiling at 20x.");
            Assert.Greater(ReadField<float>(game, "SimulationTimeScale"), 0f);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Mvp_OnlyLevelOneIsPlayableButProgressIsStillSaved()
        {
            ResetSaveCache();
            MvpSave.Save(new MvpSaveData());
            Component game = null;
            yield return LoadGameScene(value => game = value);
            Assert.NotNull(game);
            AssertGameplayGrillArtLoaded(game);
            SetField(game, "SimulationTimeScale", 120f); // Accelerate 6× while retaining the thermal window at the editor's background frame rate.

            yield return new WaitForSecondsRealtime(.6f);
            ClickButton("ENTRAR");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("NIVEL 1");
            yield return new WaitForSecondsRealtime(.45f);
            ClickButton("IR A LA PARRILLA");
            yield return new WaitForSecondsRealtime(.45f);

            int[] expectedPortions = { 2 };
            for (int level = 1; level <= expectedPortions.Length; level++)
            {
                Assert.AreEqual(level, ReadField<int>(game, "currentLevelNumber"));
                Array portions = (Array)GetField(game, "portions");
                Array guests = (Array)GetField(game, "activeGuests");
                Assert.AreEqual(expectedPortions[level - 1], portions.Length, "L" + level + " order size must match its MVP definition.");
                Assert.AreEqual(expectedPortions[level - 1], guests.Length, "Each portion must serve a guest in L" + level + ".");

                AssertGrillReadyWithoutCoal(game, assertTutorialStep: level == 1);
                for (int portionIndex = 0; portionIndex < portions.Length; portionIndex++)
                    yield return CookAndPlateByIndex(game, portionIndex, 35f);

                Assert.IsTrue(CanServeFromBoard(game), "All L" + level + " portions must be plated before service.");
                TapBoard(game);
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
                Assert.GreaterOrEqual(result.StarsByLevel[level - 1], 1, "L" + level + " must persist the earned star.");
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
                    Assert.GreaterOrEqual(result.MaxUnlockedLevel, 2,
                        "The save may retain progression while the current playable-level gate remains closed.");
                    Button next = GameObject.Find("BLOQUEADO").GetComponent<Button>();
                    Assert.IsFalse(next.interactable, "Finishing L1 must not make L2 playable in this release.");
                }
            }
        }

        private static IEnumerator CookAndPlateByIndex(Component game, int index, float timeoutSeconds)
        {
            Array portions = (Array)GetField(game, "portions");
            object portion = portions.GetValue(index);
            FoodState state = (FoodState)GetField(portion, "State");
            FoodCookProfile profile = (FoodCookProfile)GetField(portion, "Profile");
            LoadAllRawFoodToGrill(game, index);
            yield return null;

            float elapsed = 0f;
            while (state.CoreTemperatureC < 38f && elapsed < timeoutSeconds)
            {
                yield return new WaitForSecondsRealtime(.01f);
                elapsed += .01f;
            }
            Assert.GreaterOrEqual(state.CoreTemperatureC, 38f, profile.FoodId + " must warm before flipping.");
            int exposedFace = state.ExposedFace;
            TapFoodPiece(game, index, 600 + index);
            Assert.AreEqual(exposedFace, state.ExposedFace, profile.FoodId + " repeated taps must not flip food.");

            elapsed = 0f;
            while (FoodCookingModel.GetDoneness(state, profile) != Doneness.A_Punto && elapsed < timeoutSeconds)
            {
                yield return new WaitForSecondsRealtime(.01f);
                elapsed += .01f;
            }
            Assert.AreEqual(Doneness.A_Punto, FoodCookingModel.GetDoneness(state, profile), profile.FoodId + " must reach a valid point band.");
            DragFoodToBoard(game, index, 700 + index);
            float plateElapsed = 0f;
            while (ReadField<int>(game, "trayCount") < index + 1 && plateElapsed < 3f)
            {
                yield return new WaitForSecondsRealtime(.05f);
                plateElapsed += .05f;
            }
            Assert.IsTrue(ReadField<bool>(portion, "OnTray"), profile.FoodId + " must reach the serving tray.");
            Assert.AreEqual(index + 1, ReadField<int>(game, "trayCount"));
            SetField(game, "SimulationTimeScale", 120f);
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
            AssertGrillReadyWithoutCoal(game);
            Assert.That(FindText("Estado coccion").text, Does.Contain("ARRASTRÁ"));
            Assert.AreEqual(2, ((Array)GetField(game, "portions")).Length);
            Assert.AreEqual(2, ((Array)GetField(game, "activeGuests")).Length);
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

        private static IEnumerator CookAndPlate(Component game, int index, string foodId, float timeoutSeconds, bool flip)
        {
            string foodName = FoodCatalog.Get(foodId).DisplayName;
            LoadAllRawFoodToGrill(game, index);
            Array portions = (Array)GetField(game, "portions");
            object portion = portions.GetValue(index);
            FoodState state = (FoodState)GetField(portion, "State");
            FoodCookProfile profile = (FoodCookProfile)GetField(portion, "Profile");
            Assert.IsTrue(ReadField<bool>(portion, "Started"));
            Vector2 startingPosition = ReadField<Vector2>(portion, "Position");

            // Exercise the same public handlers used by the touch-drag component.
            DragActiveFoodToDifferentGrillPosition(game, index, 400 + index);
            Assert.Greater(Vector2.Distance(startingPosition, ReadField<Vector2>(portion, "Position")), .1f,
                "Dragging the portion must change its normalized grill position.");
            if (index == 0) Assert.That(FindText("Tutorial contextual").text, Does.Contain("TABLA"));

            if (flip)
            {
                float elapsedBeforeFlip = 0f;
                while (state.CoreTemperatureC < 38f && elapsedBeforeFlip < timeoutSeconds)
                {
                    yield return new WaitForSecondsRealtime(.01f);
                    elapsedBeforeFlip += .01f;
                }
                Assert.GreaterOrEqual(state.CoreTemperatureC, 38f, foodName + " should start warming before flip.");
                int faceBefore = state.ExposedFace;
                TapFoodPiece(game, index, 600 + index);
                Assert.AreEqual(faceBefore, state.ExposedFace, foodName + " repeated taps must keep cooking on one side.");
            }

            float elapsed = 0f;
            while (FoodCookingModel.GetDoneness(state, profile) != Doneness.A_Punto && elapsed < timeoutSeconds)
            {
                yield return new WaitForSecondsRealtime(.01f);
                elapsed += .01f;
            }
            Assert.AreEqual(Doneness.A_Punto, FoodCookingModel.GetDoneness(state, profile), foodName + " should reach its configured point band.");
            Assert.Greater(state.CoreTemperatureC, 0f);

            DragFoodToBoard(game, index, 700 + index);
            float plateElapsed = 0f;
            while (ReadField<int>(game, "trayCount") < index + 1 && plateElapsed < 3f)
            {
                yield return new WaitForSecondsRealtime(.05f);
                plateElapsed += .05f;
            }
            Assert.IsTrue(ReadField<bool>(portion, "OnTray"), foodName + " should be on the tray before service.");
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
                    "Once the tray is empty, the requested portion should become the one active cooking piece.");
            }
            int level = ReadField<int>(game, "currentLevelNumber");
            for (int i = 0; i < portions.Length; i++)
            {
                object portion = portions.GetValue(i);
                Assert.IsTrue(ReadField<bool>(portion, "Started") && !ReadField<bool>(portion, "OnSourceTray"),
                    "L" + level + " must load every raw portion before cooking; failed portion index " + i + ".");
            }
            Assert.IsTrue(ReadField<bool>(game, "cooking"), "L" + level + " should cook the selected piece after loading the tray.");
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
