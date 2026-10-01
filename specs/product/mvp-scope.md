> **Documento histórico del MVP de cocción.** Desde 2026-10-01 la dirección futura es [full-game](full-game.md); exclusiones de gestión/economía/métodos aquí no limitan el nuevo producto. Las mecánicas existentes siguen sujetas a sus specs de sistema.

# Alcance de producto — MVP expandido de Asadito

## Alcance

El MVP implementado contiene doce niveles progresivos y un catálogo data-driven de 18 alimentos (siete cortes vacunos adicionales, tres achuras/embutidos, tres cortes porcinos y pollo deshuesado, además de tira, chorizo, vacío y provoleta). El primer nivel conserva First Playable: dos comensales, una porción de chorizo y una de tira, cocción, armado/servicio de tabla, evaluación y retry. Los niveles posteriores enseñan más pedidos, puntos distintos, vacío/provoleta, cortes finos, achuras, cerdo/ave y cortes premium antes del asado completo; la parrilla mantiene calor uniforme siempre activo. El contenido por nivel está en `MvpLevelCatalog`; el menú enseña solo `Nivel N`.

La experiencia enseña a tocar y manipular directamente alimentos con pinza, cocinarlos por sus estados térmicos, asignar porciones a comensales y servir en una tabla de asador. La parrilla ya está caliente al entrar; no se prende/apaga carbón ni se manipulan brasas. Cada cara conserva su propia cocción. `SimulationTimeScale` inicia en 20; las opciones de tuning 20/25/30/35/40 aparecen solo en Editor/development builds. Las metas de duración siguen siendo objetivos de tuning, no condiciones de victoria ni timers fijos de cocción.

El MVP incluye menú principal, intro de nivel, tutorial, progresión local de estrellas, resultados, Retry/Next, guardado versionado y seis invitados diferenciados por edad/peso/apetito/punto/preferencias. No incluye economía.

| Nivel | Introducción de gameplay | Invitados/porciones |
|---|---|---:|
| 1 — El debut | Chorizo + tira; tutorial | 2 |
| 2 — Una tanda más | Selección y manejo de más piezas | 3 |
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

- `GrillHeatModel` siempre activo con temperatura uniforme configurable (210 °C por defecto); el área de interacción de parrilla es invisible y sirve solo para touch/drag.
- Tap directo sobre comida, hit targets invisibles ampliados, pinza ilustrada con sprites abiertos/cerrados y drag-to-table sobre una tabla de asador.
- Catálogo `FoodCatalog.json` → `FoodDefinition`/`FoodCookProfile`; motor térmico genérico y ajustable por datos. Cada atlas integrado presenta RAW, WARMING, BROWNING, IDEAL, OVERCOOKED y BURNT.
- Estado de alimento `FoodState`: temperatura de centro/superficie, humedad, progreso Maillard, carbonización (`Char`), grasa rendida (`FatRendered`) y estado térmico independiente de las dos caras para volteo.
- Parámetros de punto por alimento con rangos calibrables. El punto depende del estado de cocción, no del temporizador de estados visuales.
- Preferencias y puntos solicitados por seis perfiles; edad/peso/apetito ajustan cantidad objetivo. El género no determina el apetito.
- `ServingAllocator` prioriza cubrir a cada comensal antes de repetir y optimiza preferencias/punto; scoring ponderado por `CookingQuality`, `Satiety`, `DonenessMatch` y `FoodPreference`.
- Safe area con `Screen.safeArea` para el contenido de juego; controles, cards y HUD requieren QA manual táctil en dispositivo.

## Fuera del MVP

Carnicería, economía/monedas/compras, multijugador, caminar o navegación de NPC y métodos de cocción distintos de la parrilla caliente. También quedan fuera gestión de restaurante, combustible y selección libre de recetas/cortes por el jugador.

## Autoridad

Las reglas canónicas están en [`fire-heat.md`](systems/fire-heat.md), [`food-cooking.md`](systems/food-cooking.md), [`guest-evaluation.md`](systems/guest-evaluation.md), [`serving.md`](systems/serving.md), [`scoring.md`](systems/scoring.md) y los gates en [`mvp-gate.md`](../acceptance/mvp-gate.md). Conteo/perfiles de alimentos en [`food-catalog.md`](../../docs/gameplay/food-catalog.md); progresión por nivel en [`level-progression.md`](../../docs/gameplay/level-progression.md) y runtime en `Assets/Asado/Scripts/Runtime/MvpLevelCatalog.cs`.
