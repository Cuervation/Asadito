# Sistema — comensales y objetivos

Cada `GuestProfile` contiene `Id`, `Name`, `Age`, `Weight`, `Appetite`, `PreferredDoneness`, `FavoriteFoods`, `LikedFoods` y `DislikedFoods`. `TargetFoodAmount = AgeBase × BodySizeModifier × AppetiteModifier`; la tabla o función de `AgeBase`, los modificadores y unidades son parametrizables. El modelo no deriva apetito, preferencias ni reglas de puntuación del género.

La saciedad se calcula como una función parametrizable de unidades/raciones asignadas y apetito/capacidad del perfil. La función y sus parámetros permiten calibrar rendimientos distintos por alimento y necesidad por comensal. La saciedad objetivo es explícita para que servicio y puntuación puedan medir cumplimiento.

Cada nivel declara el conjunto de comensales, porciones disponibles, preferencias y metas. First Playable (nivel 1) tiene dos comensales y chorizo y tira de asado como porciones predeterminadas; la asignación fija inicial satisface las preferencias de corte descritas en el flujo legado mientras el modelo general soporta preferencias configuradas.

## Verificación

- Un perfil permite parametrizar apetito/capacidad, preferencias, punto y objetivo de saciedad.
- Cambiar género no cambia el resultado de ninguna fórmula.
- El nivel puede expresar metas verificables por comensal y preferencias por tipo de alimento.
