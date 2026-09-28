# Runtime Asset Manifest — Asadito MVP

Inventario honesto del estado integrable. Los únicos estados permitidos son `FINAL`, `PROVISIONAL` y `BLOCKED`; un concepto no cuenta como asset de runtime. La carpeta `Resources` se usa porque el prototipo crea su Canvas durante `AsaditoGame.Start`.

| AssetId | Name | Type | Purpose | RuntimePath | Source | UsedBy | Status | Validation |
|---|---|---|---|---|---|---|---|---|
| `art.gameplay.grill` | `ParrillaTopDownGameplay.png` | Background texture | Fondo vertical cenital de gameplay | `Resources/Art/ParrillaTopDownGameplay` | Generado para este proyecto | `AsaditoGame.Start` → `Parrilla cenital ilustrada` | PROVISIONAL | Importada y cargada como Sprite/Texture durante Play Mode (941×1672); composición cenital comprobada en Editor. La imagen es fotográfica y no satisface la dirección estilizada final; la captura disponible es Game View 640×360 horizontal, no QA móvil portrait. |
| `art.menu.cover` | `PortadaAsadito.png` | Key art texture | Fondo de portada del menú | `Resources/Art/PortadaAsadito` | Generado para este proyecto | `AsaditoGame.BuildFrontEnd` | PROVISIONAL | Existe y la ruta runtime está conectada; sin validación final de dispositivo/build ni aprobación visual final. |
| `art.menu.patio-fallback` | `PatioParrilla.png` | Background texture | Fallback de portada/fondo si faltan key arts | `Resources/Art/PatioParrilla` | Arte preexistente del proyecto | `AsaditoGame.Start` / `BuildFrontEnd` | PROVISIONAL | Recurso existente; no es el nuevo fondo principal de gameplay. |
| `art.icon.app` | `AsaditoAppIcon.png` | Android adaptive app icon | Identidad del ejecutable móvil | — | Generado para este proyecto | `PlayerSettings` → Android Adaptive (6 tamaños, 2 capas por tamaño) | PROVISIONAL | PNG cuadrado 1254×1254 asignado a las seis densidades; AAPT confirma que el APK incluye `application-icon-*`. Estilo fotográfico, no validado en launcher/device. |
| `food.chorizo.raw` | `ChorizoCrudoCutout.png` | Food sprite | Chorizo crudo y representación runtime | `Resources/Art/ChorizoCrudoCutout` | Generado para este proyecto | `AsaditoGame.MakeSausage` | PROVISIONAL | Cargado en runtime; los cambios de cocción actuales son principalmente tintes, no una secuencia ilustrada de estados. |
| `food.tira.raw` | `TiraAsadoCruda.png` | Food sprite | Tira de asado cruda | `Resources/Art/TiraAsadoCruda` | Arte preexistente del proyecto | `AsaditoGame.MakeFoodVisual` | PROVISIONAL | Ruta de recurso integrada; falta set visual de estados/caras y QA de lectura en teléfono. |
| `food.vacio.raw` | `BifeCrudo.png` | Food sprite | Representación actual de vacío | `Resources/Art/BifeCrudo` | Arte genérico preexistente, no específico de vacío | `AsaditoGame.MakeFoodVisual` | PROVISIONAL | Recurso visible en el flujo L4/L5; no hay ilustración específica ni estados dedicados para vacío. |
| `food.provoleta` | Provoleta geométrica | Procedural Canvas visual | Pieza circular para L5 | — | Formas `Image`/`Sprite` generadas en código | `AsaditoGame.MakeFoodVisual` | PROVISIONAL | Botón y pieza aparecen en runtime L5; no hay asset de comida ni estados animados/materiales finales. |
| `ui.guest-portraits` | Avatar geométrico único | Procedural UI | Identidad y reacción resumida de comensales | — | `AsaditoGame.DrawAvatar` | HUD y pantalla de resultado | BLOCKED | No existen seis retratos individuales; avatar actual es una única forma provisional. |
| `ui.font.display` | Lilita One Regular | TTF | Marca/títulos | `Resources/Fonts/LilitaOne-Regular` | Google Fonts OFL 1.1; licencia incluida | Canvas `UnityEngine.UI.Text` | PROVISIONAL | TTF y licencia incluidos; ruta configurada. Falta QA de legibilidad y escala en teléfono real. |
| `ui.font.body` | Baloo 2 Regular/Medium/SemiBold/Bold/ExtraBold | TTF family | Texto de UI | `Resources/Fonts/Baloo2-*` | Google Fonts OFL 1.1; licencia incluida | Canvas `UnityEngine.UI.Text` | PROVISIONAL | Cinco pesos y licencia incluidos; el runtime usa TTF Legacy Text; falta QA de escala/tildes en dispositivo. |
| `ui.screens` | Canvas runtime | Procedural UI | Menú, selección, intro, HUD, resultado, retry/next | — | `AsaditoGame.BuildInterface` / `BuildFrontEnd` | `SampleScene` en Play Mode | PROVISIONAL | Flujos Menu→Select→Intro→Gameplay y controles dinámicos L1–L5 observados en Editor; no se verificó el loop completo ni resolución/touch de dispositivo. |
| `vfx.embers` | Mapa 8×6 y resplandor | Procedural UI | Mostrar intensidad/distribución térmica | — | `AsaditoGame.BuildHeatGrid` / `AnimateEmbers` | HUD de gameplay | PROVISIONAL | Colores de celdas siguen energía de brasas; falta dirección artística y rendimiento móvil. |
| `vfx.smoke` | Humo procedural | Procedural UI | Feedback durante cocción | — | Corutinas `SmokePuffs` / `FloatSmoke` | HUD de gameplay | PROVISIONAL | Círculos Canvas temporales, no partículas/VFX de producción. |
| `audio.sizzle` | Sizzle procedural | AudioClip generado en runtime | Sonido loop de cocción | — | Ruido filtrado determinista desde código | `AsaditoGame.BuildSizzleAudio` | PROVISIONAL | Clip en memoria, sin archivo de audio ni mezcla/audio QA en móvil. |
| `animation.gameplay` | Acciones de cocción | Coroutines | Colocar, voltear, retirar, reaccionar y contar score | — | Código procedural | `AsaditoGame` | PROVISIONAL | Existen transiciones básicas; no hay clips Animator ni QA visual de timing en dispositivo. Ver [animation manifest](animation-manifest.md). |
| `concepts.art-direction` | Conceptos en `docs/art/concepts/` | Concept art | Referencia, no runtime | — | Varias | No consumidos por runtime | BLOCKED | Los conceptos sirven de referencia solamente; no cuentan como assets implementados ni aprobados. |

## Licencias de tipografía

- **Lilita One Regular**: Google Fonts, `ofl/lilitaone`, OFL 1.1. Texto de licencia en `Assets/Asado/Resources/Fonts/OFL-LilitaOne.txt`.
- **Baloo 2**: pesos estáticos 400/500/600/700/800 derivados de la fuente variable oficial `ofl/baloo2`, OFL 1.1. Texto de licencia en `Assets/Asado/Resources/Fonts/OFL-Baloo2.txt`.

## Bloqueantes de cierre visual

- Reemplazar el fondo fotográfico y el look de tintes/formas por un set coherente casual-premium estilizado, preservando lectura y rendimiento móvil.
- Producir cuatro alimentos reconocibles con estados/caras, retratos individualizados para seis perfiles, iconografía de resultados y feedback de fuego.
- Rediseñar el icono en el estilo final; completar audio, VFX/motion y validar safe areas, interacción táctil y legibilidad en device/build.
