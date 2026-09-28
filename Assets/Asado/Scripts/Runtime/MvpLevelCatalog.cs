using System;
using System.Collections.Generic;

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

    /// <summary>Small, explicit content table for the five MVP grill levels.</summary>
    public static class MvpLevelCatalog
    {
        private static readonly MvpLevelDefinition[] Levels =
        {
            Create(1, "EL DEBUT", new[] { "tira", "chorizo" }, .065f),
            Create(2, "ZONAS DE CALOR", new[] { "chorizo", "tira", "chorizo" }, .06f),
            Create(3, "PUNTOS DISTINTOS", new[] { "tira", "chorizo", "tira", "chorizo" }, .06f),
            Create(4, "EL VACÍO", new[] { "vacio", "tira", "chorizo", "tira" }, .06f),
            Create(5, "LA GRAN JUNTADA", new[] { "tira", "chorizo", "vacio", "provoleta", "tira", "chorizo" }, .06f)
        };

        public static int Count => Levels.Length;

        public static MvpLevelDefinition Get(int levelNumber)
        {
            int index = levelNumber - 1;
            if (index < 0 || index >= Levels.Length)
                throw new ArgumentOutOfRangeException(nameof(levelNumber), "MVP levels are numbered 1 through 5.");

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
            var roster = new[]
            {
                NewGuest("ana", "ANA", Doneness.Jugoso, "tira", "vacio"),
                NewGuest("tito", "TITO", Doneness.A_Punto, "chorizo", "provoleta"),
                NewGuest("luz", "LUZ", Doneness.A_PuntoMas, "vacio", "tira"),
                NewGuest("beto", "BETO", Doneness.Cocido, "chorizo", "tira"),
                NewGuest("mora", "MORA", Doneness.Jugoso, "tira", "provoleta"),
                NewGuest("rulo", "RULO", Doneness.A_Punto, "provoleta", "vacio")
            };
            int guestCount = Get(levelNumber).GuestCount;
            var result = new GuestProfile[guestCount];
            var tuning = new GuestAmountTuning { AdultAgeBase = .2f };
            for (int i = 0; i < guestCount; i++)
            {
                GuestProfile source = roster[i];
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
                guest.CalculateTarget(tuning);
                result[i] = guest;
            }
            return result;
        }

        private static GuestProfile NewGuest(string id, string name, Doneness point, string favorite, string liked)
        {
            var guest = new GuestProfile { Id = id, Name = name, Age = 24 + id.Length * 4, Weight = 62f, Appetite = .35f, PreferredDoneness = point };
            guest.FavoriteFoods.Add(favorite);
            guest.LikedFoods.Add(liked);
            return guest;
        }
    }
}
