# Alcance de producto — MVP expandido de Asadito

## Alcance

El MVP implementado contiene doce niveles progresivos y un catálogo data-driven de 18 alimentos (siete cortes vacunos adicionales, tres achuras/embutidos, tres cortes porcinos y pollo deshuesado, además de tira, chorizo, vacío y provoleta). El primer nivel conserva First Playable: dos comensales, una porción de chorizo y una de tira, cocción, armado/servicio de bandeja, evaluación y retry. Los niveles posteriores introducen calor espacial y puntos distintos, luego vacío/provoleta, cortes finos, achuras, cerdo/ave y cortes premium antes del asado completo. El contenido por nivel está en `MvpLevelCatalog`; el menú enseña solo `Nivel N`.

La experiencia enseña a leer y manejar un fuego de carbón, cocinar alimentos por sus estados térmicos, asignar porciones a comensales y servir. El jugador puede dar vuelta las piezas, retirar y servir cuando decida. Cada cara conserva su propia cocción. `SimulationTimeScale` inicia en 20: el QA térmico a 30× dejaba ventanas móviles demasiado estrechas en varios cortes rápidos; el valor se eligió para permitir una reacción táctil deliberada sin sustituir la cocción por timers. Las opciones de tuning 20/25/30/35/40 son independientes del nivel y solo el control interno aparece en Editor/development builds. Las metas de duración siguen siendo objetivos de tuning, no condiciones de victoria ni timers fijos de cocción.

El MVP incluye menú principal, intro de nivel, tutorial, progresión local de estrellas, resultados, Retry/Next, guardado versionado y seis invitados diferenciados por edad/peso/apetito/punto/preferencias. No incluye economía.

| Nivel | Introducción de gameplay | Invitados/porciones |
|---|---|---:|
| 1 — El debut | Chorizo + tira; tutorial | 2 |
| 2 — Zonas de calor | Reparto sobre calor espacial | 3 |
| 3 — Puntos distintos | Diferentes preferencias de cocción | 4 |
| 4 — El vacío | Corte grueso y más lento | 4 |
| 5 — La gran juntada | Provoleta y sincronización de cortes | 6 |
| 6 — Cortes finos | Entraña, bife angosto, chinchulines | 5 |
| 7 — Punto justo | Lomo, colita de cuadril y vacío | 4 |
| 8 — Achuras | Chinchulines y dos morcillas con riesgo de rotura | 5 |
| 9 — Otras carnes | Cerdo y pollo deshuesado | 5 |
| 10 — Cortes premium | Bife ancho, de chorizo, ojo de bife y otros | 6 |
| 11 — Fogón criollo | Combinación de aves, cerdo, achura y vacuno | 6 |
| 12 — El asado completo | Combinación final de proteínas/cortes | 6 |

## Sistemas requeridos

- Fuego de carbón con `HeatGrid` configurable, inicialmente 8×6, con campos `Heat`/`EmberEnergy` y consulta del calor por región ocupada por cada alimento.
- Catálogo `FoodCatalog.json` → `FoodDefinition`/`FoodCookProfile`; motor térmico genérico y ajustable por datos. Cada atlas integrado presenta RAW, WARMING, BROWNING, IDEAL, OVERCOOKED y BURNT.
- Estado de alimento `FoodState`: temperatura de centro/superficie, humedad, progreso Maillard, carbonización (`Char`), grasa rendida (`FatRendered`) y estado térmico independiente de las dos caras para volteo.
- Parámetros de punto por alimento con rangos calibrables. El punto depende del estado de cocción, no del temporizador de estados visuales.
- Preferencias y puntos solicitados por seis perfiles; edad/peso/apetito ajustan cantidad objetivo. El género no determina el apetito.
- `ServingAllocator` prioriza cubrir a cada comensal antes de repetir y optimiza preferencias/punto; scoring ponderado por `CookingQuality`, `Satiety`, `DonenessMatch` y `FoodPreference`.
- Safe area con `Screen.safeArea` para el contenido de juego; controles, cards y HUD requieren QA manual táctil en dispositivo.

## Fuera del MVP

Carnicería, economía/monedas/compras, multijugador, caminar o navegación de NPC y métodos de cocción distintos de la parrilla de carbón. También quedan fuera la gestión de restaurante, inventario de combustible y selección libre de recetas/cortes por el jugador.

## Autoridad

Las reglas canónicas están en [`fire-heat.md`](systems/fire-heat.md), [`food-cooking.md`](systems/food-cooking.md), [`guest-evaluation.md`](systems/guest-evaluation.md), [`serving.md`](systems/serving.md), [`scoring.md`](systems/scoring.md) y los gates en [`mvp-gate.md`](../acceptance/mvp-gate.md). Conteo/perfiles de alimentos en [`food-catalog.md`](../../docs/gameplay/food-catalog.md); progresión por nivel en [`level-progression.md`](../../docs/gameplay/level-progression.md) y runtime en `Assets/Asado/Scripts/Runtime/MvpLevelCatalog.cs`.
