using NUnit.Framework;
using UnityEngine;

namespace Asadito.Tests
{
    public sealed class HeatAndCookingTests
    {
        [Test]
        public void CookingProgress_UsesEachFoodProfileAndHasRedYellowGreenYellowRedWindow()
        {
            Assert.AreEqual(Asadito.Runtime.FoodCookingProgress.Red,Asadito.Runtime.FoodCookingProgress.ColorAt(0));
            Assert.AreEqual(Asadito.Runtime.FoodCookingProgress.Yellow,Asadito.Runtime.FoodCookingProgress.ColorAt(.25f));
            Assert.AreEqual(Asadito.Runtime.FoodCookingProgress.Green,Asadito.Runtime.FoodCookingProgress.ColorAt(.52f));
            Assert.AreEqual(Asadito.Runtime.FoodCookingProgress.Yellow,Asadito.Runtime.FoodCookingProgress.ColorAt(.78f));
            Assert.AreEqual(Asadito.Runtime.FoodCookingProgress.Red,Asadito.Runtime.FoodCookingProgress.ColorAt(1));
            foreach(var food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                var profile=food.Profile;float min=54f,max=66f;
                if(!profile.UsesCheeseStages)foreach(var band in profile.DonenessBands)
                    if(band.Doneness==Asadito.Runtime.Doneness.A_Punto){min=band.MinimumCoreC;max=band.MaximumCoreC;}
                var state=new Asadito.Runtime.FoodState{CoreTemperatureC=20f};
                Assert.AreEqual(0,Asadito.Runtime.FoodCookingProgress.Value(state,profile),food.Id);
                state.CoreTemperatureC=(min+max)*.5f;
                state.SetCurrentFace(new Asadito.Runtime.FoodFaceState{SurfaceTemperatureC=150f,Maillard=.2f});
                string before=JsonUtility.ToJson(state);
                float progress=Asadito.Runtime.FoodCookingProgress.Value(state,profile);
                Assert.That(progress,Is.InRange(Asadito.Runtime.FoodCookingProgress.GreenStart,Asadito.Runtime.FoodCookingProgress.GreenEnd),food.Id);
                Assert.AreEqual(Asadito.Runtime.FoodCookingProgress.Green,Asadito.Runtime.FoodCookingProgress.ColorAt(progress),food.Id);
                Assert.AreEqual(before,JsonUtility.ToJson(state),"Read-only gauge must not change thermal state");
                state.CoreTemperatureC=max+10f;
                Assert.Greater(Asadito.Runtime.FoodCookingProgress.Value(state,profile),Asadito.Runtime.FoodCookingProgress.GreenEnd,food.Id);
                state.CoreTemperatureC=(min+max)*.5f;
                state.SetCurrentFace(new Asadito.Runtime.FoodFaceState{Char=.8f,Maillard=.3f});
                Assert.AreEqual(1f,Asadito.Runtime.FoodCookingProgress.Value(state,profile),"Burnt surface overrides green core: "+food.Id);
            }
            var tira=Asadito.Runtime.FoodCookingModel.CreateProfile("tira");
            var chorizo=Asadito.Runtime.FoodCookingModel.CreateProfile("chorizo");
            var sameCore=new Asadito.Runtime.FoodState{CoreTemperatureC=57f};
            Assert.Greater(Asadito.Runtime.FoodCookingProgress.Value(sameCore,tira),Asadito.Runtime.FoodCookingProgress.Value(sameCore,chorizo));
            var cheese=new Asadito.Runtime.FoodState{CoreTemperatureC=60f};
            Assert.Less(Asadito.Runtime.FoodCookingProgress.Value(cheese,Asadito.Runtime.FoodCookingModel.CreateProfile("provoleta")),Asadito.Runtime.FoodCookingProgress.GreenStart,"Cheese must brown before green");
            cheese.SetCurrentFace(new Asadito.Runtime.FoodFaceState{Maillard=.2f});cheese.CoreTemperatureC=66f;
            float idealEnd=Asadito.Runtime.FoodCookingProgress.Value(cheese,Asadito.Runtime.FoodCookingModel.CreateProfile("provoleta"));
            cheese.CoreTemperatureC=67f;
            Assert.Greater(Asadito.Runtime.FoodCookingProgress.Value(cheese,Asadito.Runtime.FoodCookingModel.CreateProfile("provoleta")),idealEnd,"Cheese's post-ideal gap must warn, not jump back to undercooked");
        }

        [Test]
        public void CookingProgress_FollowsRealThermalStepsMonotonicallyAndHandlesDrying()
        {
            foreach(var food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                var profile=food.Profile;var state=new Asadito.Runtime.FoodState{CoreTemperatureC=20f};
                state.SetCurrentFace(new Asadito.Runtime.FoodFaceState{SurfaceTemperatureC=20f});float previous=0;
                for(int step=0;step<500;step++)
                {
                    Asadito.Runtime.FoodCookingModel.Step(state,profile,210f,.25f,true);
                    float progress=Asadito.Runtime.FoodCookingProgress.Value(state,profile);
                    Assert.That(progress,Is.InRange(0f,1f),food.Id);Assert.GreaterOrEqual(progress+.00001f,previous,food.Id);
                    previous=progress;
                }
                Assert.Greater(previous,.95f,food.Id+" must eventually reach the red end");
            }
            var dry=new Asadito.Runtime.FoodState{CoreTemperatureC=57f,Moisture=.2f};
            Assert.Greater(Asadito.Runtime.FoodCookingProgress.Value(dry,Asadito.Runtime.FoodCookingModel.CreateProfile("tira")),Asadito.Runtime.FoodCookingProgress.GreenEnd);
            Assert.AreEqual(0f,Asadito.Runtime.FoodCookingProgress.Value(null,null));
        }

        [Test]
        public void CookingVisuals_HaveTenDistinctAtlasCompositions()
        {
            var compositions = new System.Collections.Generic.HashSet<string>();
            foreach (Asadito.Runtime.FoodCookVisualStage stage in System.Enum.GetValues(typeof(Asadito.Runtime.FoodCookVisualStage)))
            {
                Asadito.Runtime.FoodCookingModel.GetVisualBlend(stage, out int row, out float blend);
                Assert.That(row, Is.InRange(0, 5));
                Assert.IsTrue(blend == 0f || (blend == .5f && row < 5));
                compositions.Add(row + ":" + blend);
            }
            Assert.AreEqual(10, compositions.Count);
        }

        [Test]
        public void CookingVisuals_ResolveAllTenThermalStagesForEveryFood()
        {
            foreach (var food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                var profile = food.Profile;
                var observed = new System.Collections.Generic.HashSet<Asadito.Runtime.FoodCookVisualStage>();
                float[] cores = profile.UsesCheeseStages
                    ? new[] { 20f, 25f, 35f, 42f, 50f, 60f, 65f, 72f, 74f, 80f }
                    : new[] { 20f, 25f, 30f, 35f, 40f, profile.DonenessBands[0].MinimumCoreC + 1f, 70f, 75f, 80f, 85f };
                float[] surface = { 20f, 30f, 45f, 120f, 140f, 160f, 180f, 190f, 210f, 220f };
                float[] maillard = { 0f, 0f, 0f, .025f, .05f, .2f, .3f, .4f, .5f, .8f };
                float[] chars = { 0f, 0f, 0f, 0f, 0f, 0f, .2f, .35f, .55f, .8f };
                for (int n = 0; n < 10; n++)
                {
                    var state = new Asadito.Runtime.FoodState { CoreTemperatureC = cores[n] };
                    state.SetCurrentFace(new Asadito.Runtime.FoodFaceState { SurfaceTemperatureC = surface[n], Maillard = maillard[n], Char = chars[n] });
                    observed.Add(Asadito.Runtime.FoodCookingModel.GetVisualStage(state, profile));
                }
                Assert.AreEqual(10, observed.Count, food.Id);
            }
        }

        [Test]
        public void GrillHeatModel_IsAlwaysHotAndClampsConfiguredTemperature()
        {
            var grill = new Asadito.Runtime.GrillHeatModel();
            Assert.AreEqual(210f, grill.GetTemperatureC(), .001f);
            grill.TemperatureC = 250f;
            Assert.AreEqual(250f, grill.GetTemperatureC(), .001f);
            grill.TemperatureC = 500f;
            Assert.AreEqual(300f, grill.GetTemperatureC(), .001f);
            grill.TemperatureC = -10f;
            Assert.AreEqual(100f, grill.GetTemperatureC(), .001f);
        }

        [Test]
        public void FoodCooking_RespondsToAlwaysHotGrill_AndTracksBurnOnExposedSide()
        {
            var state = new Asadito.Runtime.FoodState { CoreTemperatureC = 20f, SurfaceTemperatureC = 20f };
            var profile = new Asadito.Runtime.FoodCookProfile
            {
                SurfaceTransferRate = 1f,
                CoreTransferRate = 1f,
                MaillardRate = .1f,
                CharRate = .2f,
                MaillardStartsAtC = 100f,
                CharStartsAtC = 150f
            };

            Asadito.Runtime.FoodCookingModel.Step(state, profile, new Asadito.Runtime.GrillHeatModel().GetTemperatureC(), 5f, true);
            Asadito.Runtime.FoodFaceState firstSide = state.CurrentFace;
            Assert.Greater(state.CoreTemperatureC, 20f);
            Assert.Greater(firstSide.Maillard, 0f);
            Assert.Greater(firstSide.Char, 0f);
            Assert.AreEqual(0f, state.Faces[1].Char);

            state.Flip();
            Asadito.Runtime.FoodCookingModel.Step(state, profile, new Asadito.Runtime.GrillHeatModel().GetTemperatureC(), 2.5f, true);
            Assert.Greater(state.CurrentFace.Char, 0f);
            Assert.Greater(state.Faces[0].Char, 0f);
        }

        [Test]
        public void DonenessMatch_UsesConfiguredThermalBand()
        {
            var state = new Asadito.Runtime.FoodState { CoreTemperatureC = 57f };
            var profile = new Asadito.Runtime.FoodCookProfile();
            Assert.AreEqual(100f, Asadito.Runtime.FoodCookingModel.EvaluateDonenessMatch(state, profile, Asadito.Runtime.Doneness.A_Punto));
            Assert.Less(Asadito.Runtime.FoodCookingModel.EvaluateDonenessMatch(state, profile, Asadito.Runtime.Doneness.Jugoso), 100f);
        }

        [Test]
        public void FoodProfiles_UseDistinctDonenessBandsForChorizoAndTira()
        {
            var chorizo = Asadito.Runtime.FoodCookingModel.CreateProfile("chorizo");
            var tira = Asadito.Runtime.FoodCookingModel.CreateProfile("tira");

            Assert.AreEqual("chorizo", chorizo.FoodId);
            Assert.AreEqual("tira", tira.FoodId);
            Assert.AreNotEqual(tira.DonenessBands[0].MinimumCoreC, chorizo.DonenessBands[0].MinimumCoreC);
            Assert.AreNotEqual(tira.CoreTransferRate, chorizo.CoreTransferRate);
            Assert.AreEqual(.025f, tira.CoreTransferRate, .0001f);
            Assert.AreEqual(.055f, chorizo.CoreTransferRate, .0001f);
            Assert.AreEqual(71f, chorizo.DonenessBands[1].MinimumCoreC);
            Assert.AreEqual(76f, chorizo.DonenessBands[1].MaximumCoreC);
        }

        [Test]
        public void FoodProfiles_VacioHeatsMoreSlowly_AndProvoletaHasNamedPhases()
        {
            var vacio = Asadito.Runtime.FoodCookingModel.CreateProfile("vacio");
            var tira = Asadito.Runtime.FoodCookingModel.CreateProfile("tira");
            var cheese = Asadito.Runtime.FoodCookingModel.CreateProfile("provoleta");
            Assert.Less(vacio.CoreTransferRate, tira.CoreTransferRate);
            Assert.Less(tira.CoreTransferRate, Asadito.Runtime.FoodCookingModel.CreateProfile("chorizo").CoreTransferRate);
            Assert.Greater(cheese.CoreTransferRate, tira.CoreTransferRate);
            Assert.AreEqual(Asadito.Runtime.ProvoletaCookingStage.Cold, Asadito.Runtime.FoodCookingModel.GetProvoletaStage(new Asadito.Runtime.FoodState { CoreTemperatureC = 20f }));
            Assert.AreEqual(Asadito.Runtime.ProvoletaCookingStage.Softening, Asadito.Runtime.FoodCookingModel.GetProvoletaStage(new Asadito.Runtime.FoodState { CoreTemperatureC = 40f }));
            Assert.AreEqual(Asadito.Runtime.ProvoletaCookingStage.Browning, Asadito.Runtime.FoodCookingModel.GetProvoletaStage(new Asadito.Runtime.FoodState { CoreTemperatureC = 50f, Maillard = .1f }));
            Assert.AreEqual(Asadito.Runtime.ProvoletaCookingStage.Ideal, Asadito.Runtime.FoodCookingModel.GetProvoletaStage(new Asadito.Runtime.FoodState { CoreTemperatureC = 60f, Maillard = .2f }));
            Assert.AreEqual(Asadito.Runtime.ProvoletaCookingStage.Failed, Asadito.Runtime.FoodCookingModel.GetProvoletaStage(new Asadito.Runtime.FoodState { CoreTemperatureC = 72f }));
            Assert.AreEqual(Asadito.Runtime.ProvoletaCookingStage.Burnt, Asadito.Runtime.FoodCookingModel.GetProvoletaStage(new Asadito.Runtime.FoodState { CoreTemperatureC = 65f, Char = .8f }));
            Assert.Greater(cheese.SurfaceTransferRate, tira.SurfaceTransferRate);
        }

    }
}
