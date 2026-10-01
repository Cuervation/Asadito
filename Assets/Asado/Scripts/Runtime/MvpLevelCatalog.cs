using System;

namespace Asadito.Runtime
{
    [Serializable]
    public sealed class MvpLevelDefinition
    {
        public int Number;
        public string Title;
        public string LearningGoal;
        public int GuestCount;
        public string[] FoodIds;
        public float[] PortionAmounts;
    }

    /// <summary>Data-driven first chapter; management unlocks progressively while later verticals remain gated.</summary>
    public static class MvpLevelCatalog
    {
        // Current release gate: Chapter 1 exposes the complete cooking/basic-management arc; later verticals remain locked.
        public static int MaxPlayableLevel => ManagementConfig.Load().PlayableLevels;

        [Serializable] private sealed class LevelData { public MvpLevelDefinition[] Levels; }
        private static MvpLevelDefinition[] cached;
        private static MvpLevelDefinition[] Levels
        {
            get
            {
                if (cached == null)
                {
                    var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Definitions/ChapterOneLevels");
                    if (asset == null) throw new InvalidOperationException("ChapterOneLevels missing");
                    cached = UnityEngine.JsonUtility.FromJson<LevelData>(asset.text).Levels;
                }
                return cached;
            }
        }

        private static readonly GuestProfile[] Roster =
        {
            NewGuest("ana", "ANA", 29, 56f, .38f, Doneness.Jugoso,
                new[] { "tira", "ojo_bife" }, new[] { "pollo_deshuesado", "provoleta" }, new[] { "morcilla" }),
            NewGuest("tito", "TITO", 51, 84f, .46f, Doneness.A_Punto,
                new[] { "chorizo", "morcilla" }, new[] { "vacio", "entrana", "morcilla_vasca" }, new[] { "provoleta" }),
            NewGuest("luz", "LUZ", 17, 52f, .31f, Doneness.A_PuntoMas,
                new[] { "provoleta", "lomo" }, new[] { "pollo_deshuesado", "costillita_cerdo" }, new[] { "chinchulines" }),
            NewGuest("beto", "BETO", 68, 95f, .43f, Doneness.Cocido,
                new[] { "vacio", "costillita_cerdo" }, new[] { "bife_ancho", "matambre_cerdo" }, new[] { "pollo_deshuesado" }),
            NewGuest("mora", "MORA", 34, 48f, .34f, Doneness.Jugoso,
                new[] { "entrana", "morcilla_vasca" }, new[] { "tira", "provoleta", "solomillo_cerdo" }, new[] { "morcilla" }),
            NewGuest("rulo", "RULO", 41, 72f, .52f, Doneness.A_Punto,
                new[] { "bife_chorizo", "chinchulines" }, new[] { "solomillo_cerdo", "chorizo", "ojo_bife" }, new[] { "vacio" })
        };

        public static int Count => Levels.Length;
        public static int MaxGuestCount => Roster.Length;

        public static MvpLevelDefinition Get(int levelNumber)
        {
            int index = levelNumber - 1;
            if (index < 0 || index >= Levels.Length)
                throw new ArgumentOutOfRangeException(nameof(levelNumber), "Levels are numbered 1 through " + Levels.Length + ".");
            MvpLevelDefinition source = Levels[index];
            return new MvpLevelDefinition
            {
                Number = source.Number,
                Title = source.Title,
                LearningGoal = source.LearningGoal,
                GuestCount = source.GuestCount,
                FoodIds = (string[])source.FoodIds.Clone(),
                PortionAmounts = (float[])source.PortionAmounts.Clone()
            };
        }

        public static GuestProfile[] CreateGuests(int levelNumber)
        {
            int count = Get(levelNumber).GuestCount;
            var result = new GuestProfile[count];
            var tuning = new GuestAmountTuning { AdultAgeBase = .24f, SeniorAgeBase = .21f, ChildAgeBase = .15f };
            for (int i = 0; i < count; i++)
            {
                GuestProfile source = Roster[i];
                var guest = new GuestProfile
                {
                    Id = source.Id,
                    Name = source.Name,
                    Age = source.Age,
                    Weight = source.Weight,
                    Appetite = source.Appetite,
                    PreferredDoneness = source.PreferredDoneness
                };
                guest.FavoriteFoods.AddRange(source.FavoriteFoods);
                guest.LikedFoods.AddRange(source.LikedFoods);
                guest.DislikedFoods.AddRange(source.DislikedFoods);
                guest.CalculateTarget(tuning);
                result[i] = guest;
            }
            return result;
        }

        private static GuestProfile NewGuest(string id, string name, int age, float weight, float appetite, Doneness point,
            string[] favorite, string[] liked, string[] disliked)
        {
            var guest = new GuestProfile { Id = id, Name = name, Age = age, Weight = weight, Appetite = appetite, PreferredDoneness = point };
            guest.FavoriteFoods.AddRange(favorite);
            guest.LikedFoods.AddRange(liked);
            guest.DislikedFoods.AddRange(disliked);
            return guest;
        }
    }
}
