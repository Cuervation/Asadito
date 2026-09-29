# Manifest de assets de runtime — ASADITO

Inventario de contenido fuente en `Resources`; las rutas Unity omiten extensión donde corresponda. `PROVISIONAL` = integrado y validable pero sin aprobación artística/física final. Assets raster originales o generados para este proyecto; las fuentes tienen licencia OFL adjunta. No se reclama producción comercial para audio/VFX.

| Familia | Cantidad | Fuentes/runtime | Integración y verificación | Estado |
|---|---:|---|---|---|
| Comidas: atlas térmico | 18 atlas PNG RGBA × 6 estados = 108 sprites runtime | `Assets/Asado/Resources/Art/Foods/States/<FoodId>.png`; cada 1024×1536: seis filas | `FoodCatalog.json` resuelve ID/crop; `FoodSpriteLibrary` recorta/cacha RAW/WARMING/BROWNING/IDEAL/OVERCOOKED/BURNT; validator comprueba dimens., alfa, IDs y perfiles; EditMode carga 6 etapas | PROVISIONAL |
| Ilustración de nivel | 12 PNG 1536×1024 | `Resources/Art/LevelCards/Nivel1`…`Nivel12` | Una ilustración por nivel; cards importadas a cap 512/ETC2; selector expone solo Nivel N | PROVISIONAL |
| Retratos invitados | Atlas 1024×1536, 6 perfiles × 4 expresiones = 24 cortes | `Resources/Art/GuestPortraitAtlas` | Retratos neutral/feliz/muy feliz/decepcionado para seis guests; presentación de resultado usa corte por perfil/estado | PROVISIONAL |
| Parrilla principal | 941×1672 | `Resources/Art/ParrillaTopDownStylized` | Imagen de fondo principal top-down en gameplay | PROVISIONAL |
| Portada | 941×1672 | `Resources/Art/PortadaAsadito` | Fondo key art top-down, repasador de bandera impreso en tela; marca/títulos/CTA de UI separados | PROVISIONAL |
| Fallbacks de fondo | varios PNG preexistentes | `ParrillaTopDownGameplay`, `PatioParrilla` | Retenidos por fallback en código; no son el camino visual principal | LEGACY retenido |
| Wordmark | PNG transparente 1536×480 + Lilita One | `Resources/Art/AsaditoLogo`, `Resources/Fonts/LilitaOne-Regular` | Header/logo/menu. TTF dispone de `OFL-LilitaOne.txt` | PROVISIONAL |
| Fuentes UI | Lilita One; Baloo 2 en 5 pesos | `Resources/Fonts/*` | Legacy `UnityEngine.UI.Text`; ambos paquetes de licencia OFL incluidos | PROVISIONAL |
| Icono adaptativo | PNG original 1254×1254 y slots de launcher existentes | `AsaditoAppIcon`; `ProjectSettings/ProjectSettings.asset` | Configuración anterior conservada; revisitar launcher de APK después del build | PROVISIONAL |
| Iconografía UI | 11 marcas vectoriales generadas/cacheadas como sprites 96×96 | `AsaditoUiIcons.Get` | flame/tray/stars/back/exit/warning/retry/next/flip y guests, código propio | PROVISIONAL |
| VFX visuales | Procedural Canvas | `AsaditoGame`: brasa/glow, transición calor, humo/fade | Suficientes señales básicas; no shader/pipeline VFX dedicado | PROVISIONAL |
| SFX one-shot | 8 clips mono sintetizados en memoria | `Assets/Asado/Scripts/Runtime/ProceduralSfx.cs` | UI, Ignite, FoodDrop, Flip, Plate, Serve, Result, Star | PROVISIONAL |
| Sizzle / haptics | loop sintetizado y vibración de plataforma condicional | `AsaditoGame` | Activa durante cocción y cuando soportado/permitido | PROVISIONAL |
| UI | canvas y componentes creados en runtime | `AsaditoGame` | Main, selección, intro, HUD, resultados/save/Retry/Next; referencias usan `Resources` | PROVISIONAL |

## Auditoría de inventario visual conectado

| Recurso | Inventario completo | Estados/uso | Runtime conectado | Verificado |
|---|---|---|---|---|
| Food atlas | `tira`, `chorizo`, `vacio`, `provoleta`, `entrana`, `colita_cuadril`, `lomo`, `bife_ancho`, `bife_angosto`, `bife_chorizo`, `ojo_bife`, `chinchulines`, `morcilla`, `morcilla_vasca`, `matambre_cerdo`, `costillita_cerdo`, `solomillo_cerdo`, `pollo_deshuesado` (18/18) | RAW, WARMING, BROWNING, IDEAL, OVERCOOKED, BURNT para cada ID | Sí: `FoodCatalog.json` → `FoodSpriteLibrary` → gameplay | Sí: 108/108 sprites no-null y transición/profile coverage EditMode |
| Guest portraits | `ana`, `tito`, `luz`, `beto`, `mora`, `rulo` (6/6) | Neutral, feliz, muy feliz, decepcionado (24 recortes) | Sí: diálogo/reacción y resultado por invitado | Sí: PlayMode comprueba el flujo y la suite valida atlas/reacciones |
| Nivel ilustrado | `Nivel1`…`Nivel12` (12/12) | Una imagen exclusiva por nivel; el UI sobreimprime solo `Nivel N` | Sí: grilla del selector | Sí: dimensiones/archivos por validator y desbloqueo por PlayMode |
| Marca/fondos | `AsaditoLogo`, `AsaditoAppIcon`, `PortadaAsadito`, `ParrillaTopDownStylized`; fallbacks conservados: `ParrillaTopDownGameplay`, `PatioParrilla` | Wordmark/icono; portada top-down con repasador argentino; fondo de parrilla | Sí: portada, launcher y gameplay/fallback | Sí: Android APK exporta adaptive icon; arranque renderizado en emulador |
| Iconos UI | Guest, Locked, Star, Flame, Tray, Retry, Next, Flip, Back, Exit, Warning (11/11) | Marcas vectoriales rasterizadas/cacheadas a 96×96 | Sí: UI de intro, juego, resultados y acciones | Sí: escenas/tests de flujo y carga de iconos |
| VFX | Glow/pulso de portada, transición de calor, pulso/desplazamiento de brasa, humo Canvas/fade, feedback de contacto y cambios visuales térmicos | Procedural y ligero; sin partículas volumétricas | Sí: portada y gameplay | Sí: tests de gameplay/render de estados; rendimiento físico pendiente |
| Motion | Catálogo completo en [animation manifest](animation-manifest.md): entrada/CTA, botones, navegación, fuego, brasas/humo, comida(place/lift/drag/flip/cook/tray), reacciones y resultados/estrellas | Corutinas/canvas y sprites por datos; animaciones de comida genéricas compartidas, no clips authored por corte | Sí: integradas en runtime | Sí: 6 PlayMode; inspección táctil humana pendiente |

## Comprobaciones y límites

- `python Tools/validate_food_content.py`: 18 perfiles únicos, 108 representaciones de seis estados y 12 ilustraciones. El test verifica cada atlas y que todas las fases térmicas son alcanzables. La tabla foodId, nombre, categoría y parámetros es [`docs/gameplay/food-catalog.md`](../gameplay/food-catalog.md).
- Import settings fijan atlases de comida a 768 px (`ETC2_RGBA8`, HQ, no mips/readable) y tarjetas a 512 px. Los atlas previos de 4/6 alimentos/estados y tres PNG legacy de cortes se borraron después de comprobar que no quedaran referencias runtime/serializadas ni GUID en escenas/prefabs. Fondos que siguen conectados como fallback se conservan.
- La forma de PNG confirma alpha/dimensiones, no gusto, coherencia fina entre todos los frames, contraste a escala, arte final ni QA de notch. Todo arte nuevo sigue `PROVISIONAL`.
- `Resources` es carga apropiada para la UI existente creada en runtime, pero catálogo muy grande futuro debe evaluar Addressables; no hace falta introducirlo para 18 alimentos.

## Licencias

- Lilita One y Baloo 2: Google Fonts OFL 1.1; conservar textos de licencia en `Resources/Fonts/`.
- Arte generado para ASADITO no incluye personajes, logos ni sprites de terceros; archivos de licencias externos no incluidos para los PNG creados.
