# Sistema — puntuación

Cada porción servida se evalúa en cuatro dimensiones explícitas: `CookingQuality` (40%), `Satiety` (30%), `DonenessMatch` (20%) y `FoodPreference` (10%). Son pesos baseline centralizados y tunables; curvas, topes y tolerancias también son parámetros de balance.

El resultado por comensal se normaliza a 0–100; el total de nivel se normaliza a 0–200 en First Playable con dos comensales y escala según el número/configuración de comensales en niveles posteriores. El desglose o la reacción debe comunicar al menos el factor de mayor impacto, de modo que el jugador pueda entender cómo mejorar.

## Verificación

- El desglose identifica las cuatro dimensiones y permite comprobar qué factor alteró el resultado.
- El mismo estado, preferencias y parámetros producen la misma puntuación.
- La UI de nivel 1 informa resultados individuales y total 0–200; retry reinicia estado, asignaciones y puntaje.
