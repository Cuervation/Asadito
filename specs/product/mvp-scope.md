# Alcance de producto — MVP de Asadito

## Alcance

El MVP contiene cinco niveles progresivos de asado. First Playable es el nivel 1: dos comensales, una porción de chorizo y una de tira de asado, cocción en parrilla, armado/servicio de bandeja, evaluación y retry. Los niveles 2–5 amplían gradualmente comensales, porciones y variedad usando el mismo ciclo central; el balance puede ajustar cantidades y cortes por nivel.

La experiencia enseña a leer y manejar un fuego de carbón, cocinar alimentos por sus estados térmicos, asignar porciones a comensales y servir. El jugador puede dar vuelta las piezas, retirarlas y servir cuando decida. `SimulationTimeScale` global inicia en 30; es configurable para tuning. Las opciones DEBUG 20/25/30/35/40 son independientes del nivel. Durar 2–3 minutos en nivel 1 y 5–7 minutos en el juego completo son objetivos de tuning, no requisitos de victoria ni timers de cocción.

El MVP incluye menú principal, intro de nivel, tutorial, progresión, estrellas y guardado local básico, todo de alcance simple. No incluye economía.

| Nivel | Contenido |
|---|---|
| 1 — El debut | 2 personas, chorizo + tira, tutorial |
| 2 | 3 personas, mayor cantidad, introduce zonas térmicas |
| 3 | 4 personas, preferencias de punto distintas |
| 4 | 4 personas, introduce vacío y cocción más lenta |
| 5 | 6 personas, introduce provoleta y requiere sincronizar comportamientos distintos |

## Sistemas requeridos

- Fuego de carbón con `HeatGrid` configurable, inicialmente 8×6, con campos `Heat` y `EmberEnergy` y consulta de calor por región ocupada por cada alimento.
- Estado de alimento `FoodState`: temperaturas de centro y superficie, humedad, progreso Maillard, carbonización (`Char`), grasa rendida (`FatRendered`) y caras/lados para soportar volteo.
- Parámetros de punto por alimento con intervalos jugables y calibrables. El punto depende del estado de cocción, nunca de un temporizador de estado.
- Preferencias y puntos solicitados por comensal; perfil y saciedad parametrizables sin asociar reglas a género.
- `ServingAllocator` que prioriza la asignación para cumplir los objetivos de comensales antes de optimizar calidad excedente.
- Evaluación por porción y total basada en `CookingQuality`, `Satiety`, `DonenessMatch` y `FoodPreference`.

## Fuera del MVP

Carnicería, economía/monedas/compras, multijugador, caminar o navegación de NPC, y métodos de cocción distintos de la parrilla de carbón. También quedan fuera gestión de restaurante, inventario de combustible, recetas y selección de cortes por el jugador.

## Autoridad

Las reglas canónicas están en [`fire-heat.md`](systems/fire-heat.md), [`food-cooking.md`](systems/food-cooking.md), [`guest-evaluation.md`](systems/guest-evaluation.md), [`serving.md`](systems/serving.md), [`scoring.md`](systems/scoring.md) y los gates en [`mvp-gate.md`](../acceptance/mvp-gate.md). Las antiguas specs de MVP, loop, cocción, scoring y aceptación quedan como referencias históricas, no como fuentes normativas.
