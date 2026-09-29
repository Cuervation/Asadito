# Evaluación y puntuación

Cada `GuestScore` agrupa métricas en escala 0–100. `ScoreConfig` pesos iniciales: calidad de cocción 40%, saciedad 30%, punto deseado 20%, preferencia de comida 10%; el total ponderado se divide por suma de pesos y se acota a 0–100.

`ServingAllocator.GetFoodPreferenceScore`: rechazado=0, neutral=40, gustado=70, favorito=100. El valor favorito ahora sí alcanza máximo. `EvaluateDonenessMatch` devuelve 100 dentro de banda propia y decrece 8 puntos por °C fuera de rango. Las estrellas/resultados agregan puntajes por comensal, explicitan progreso y persisten en save.

La asignación de plato no es idéntica al score final: distribuye primera porción entre invitados, prioriza meta/cobertura, preferencia y punto determinísticamente; puntuación posterior recompensa calidad/mesa cubierta. Ver `GuestAndScoring.cs`, `ServingAllocator.cs` y tests.
