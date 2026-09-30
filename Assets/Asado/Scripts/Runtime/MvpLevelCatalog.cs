using System;

namespace Asadito.Runtime
{
    [Serializable]
    public sealed class MvpLevelDefinition
    {
        public int Number;
        public string Title;
        public int GuestCount;
        public string[] FoodIds;
        public float[] PortionAmounts;
    }

    /// <summary>Progressive 12-level menu: teach direct food handling and turning before mixing cuts, offal and proteins.</summary>
    public static class MvpLevelCatalog
    {
        private static readonly MvpLevelDefinition[] Levels =
        {
            Create(1, "EL DEBUT", new[] { "tira", "chorizo" }, .065f),
            Create(2, "UNA TANDA MÁS", new[] { "chorizo", "tira", "chorizo" }, .060f),
            Create(3, "PUNTOS DISTINTOS", new[] { "tira", "chorizo", "tira", "chorizo" }, .060f),
            Create(4, "EL VACÍO", new[] { "vacio", "tira", "chorizo", "tira" }, .060f),
            Create(5, "LA GRAN JUNTADA", new[] { "tira", "chorizo", "vacio", "provoleta", "tira", "chorizo" }, .060f),
            Create(6, "CORTES FINOS", new[] { "entrana", "bife_angosto", "chinchulines", "entrana", "tira" }, .057f),
            Create(7, "PUNTO JUSTO", new[] { "lomo", "colita_cuadril", "vacio", "lomo" }, .060f),
            Create(8, "ACHURAS", new[] { "chinchulines", "morcilla", "morcilla_vasca", "chorizo", "tira" }, .055f),
            Create(9, "OTRAS CARNES", new[] { "matambre_cerdo", "costillita_cerdo", "pollo_deshuesado", "pollo_deshuesado", "provoleta" }, .055f),
            Create(10, "CORTES PREMIUM", new[] { "bife_ancho", "bife_chorizo", "ojo_bife", "bife_angosto", "lomo", "colita_cuadril" }, .052f),
            Create(11, "FOGÓN CRIOLLO", new[] { "pollo_deshuesado", "matambre_cerdo", "solomillo_cerdo", "costillita_cerdo", "morcilla_vasca", "entrana" }, .052f),
            Create(12, "EL ASADO COMPLETO", new[] { "vacio", "ojo_bife", "entrana", "morcilla_vasca", "provoleta", "pollo_deshuesado" }, .050f)
        };

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
                GuestCount = source.GuestCount,
                FoodIds = (string[])source.FoodIds.Clone(),
                PortionAmounts = (float[])source.PortionAmounts.Clone()
            };
        }

        private static MvpLevelDefinition Create(int number, string title, string[] foods, float amount)
        {
            var amounts = new float[foods.Length];
            for (int i = 0; i < amounts.Length; i++) amounts[i] = amount;
            return new MvpLevelDefinition { Number = number, Title = title, GuestCount = foods.Length, FoodIds = foods, PortionAmounts = amounts };
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
