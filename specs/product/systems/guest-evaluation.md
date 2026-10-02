# Sistema — comensales y evaluación

`GuestProfile` usa edad, peso, apetito individual, `PreferredDoneness`, `RequestedFoodId` y listas Favorite/Liked/Disliked. `GuestAmountTuning` calcula objetivos por grupo etario con modificador de peso y apetito; género no se consulta. Seis perfiles (Ana, Tito, Luz, Beto, Mora y Rulo) se reutilizan progresivamente y difieren en objetivos, punto y alimentos preferidos/rechazados.

`MvpLevelCatalog.CreateGuests` copia el pedido canónico `FoodIds[i]` del nivel a `RequestedFoodId` del comensal `i`, sin alterar sus preferencias. Pedido, hambre y punto son dimensiones distintas: una comida favorita no reemplaza la solicitada y los gramos objetivo siguen describiendo saciedad, no identidad del corte. Los pedidos se crean desde datos existentes; no se agregan campos persistidos ni migración.

La puntuación pondera cooking 40, satiety 30, doneness 20, preference 10; métricas 0–100 y pesos normalizados. Score de comida neutral 40, liked 70, favorite 100, disliked 0. Seis perfiles no implican que cada nivel tenga seis comensales: cada menú varía entre dos y seis invitados según porciones.

Ver `GuestAndScoring.cs` + `MvpLevelCatalog.cs`; no es consejo nutricional.
