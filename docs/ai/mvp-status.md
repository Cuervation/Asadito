# Estado del MVP de Asadito

## IMPLEMENTADO

- PlayMode valida el loop de servicio L1–L5; cada porción llega al punto, se sirve, puntúa/guarda y desbloquea Next; L5 vuelve a selección. Retry/reset está comprobado independientemente en L1.
- Unity Editor real, tras fixes de caché de sprites en Domain Reload desactivado, teardown de cooking y anclaje de iconos: PlayMode full 6/6 y EditMode 13/13 pasaron; también L1 ×30 y los ciclos L1–L5. Revisión visual de GameView portrait 540×960 cubrió menú, niveles, intro, gameplay y resultados sample. No mide dispositivo/touch/safe area ni deliberación humana.
- SDD liviano: alcance, sistemas, gate de aceptación y dirección visual canónicos.
- Catálogo de cinco niveles con menú de selección, intro dinámica y composición de botones/porciones variable; seis perfiles de comensal. L1–L5 navegan al gameplay en Editor sin servir.
- Modelos de fuego/cocción/asignación/evaluación; vacío lento y fases térmicas de provoleta; HUD térmico, movimiento, bandeja, servicio, score, persistencia local, Retry y Next implementados en código.
- Fondo cenital ilustrado y portada semi-cartoon top-down coherente; retratos 6×4 mantienen identidades y expresiones. Portada y atlas conservan tamaño/GUID; PlayMode pasa con assets actuales. Preview del Editor en vertical inspeccionado; todo sigue PROVISIONAL hasta dispositivo real: safe area, responsive, VFX/audio final y touch/performance pendientes.
- Pruebas EditMode actuales: 13/13 pasaron en Unity batch aislado; PlayMode actual 6/6.

## PROVISIONAL

- El juego utiliza Canvas procedural 2D; el fondo cenital sigue siendo una pieza provisional. Hay cuatro estados genéricos por comida en atlas, pero no variantes por cara; el set no es final.
- Vacío/provoleta ya tienen sprites distintos; aún faltan aprobar comida y tarjetas de comensales, revisar portada/UI responsive y hacer review en device; wordmark/cover/icono están integrados pero no aprobados.
- Iconografía UI de comida/guest/lock/stars/navigation/actions ya integrada como atlas más sprites vectoriales procedurales; queda PROVISIONAL hasta review y accesibilidad en device. Algunos detalles secundarios aún son texto/Unicode.
- Brasas/humo, entrada/CTA, ignition, tween de celdas, lift/release/flip, bandeja, reacción, transición/score, sizzle y háptica funcionan de forma básica; ninguno está aprobado visual/sonoramente en device.
- Full cycle automatizado cooking→serving→results/progression pasa en L1–L5; Retry/reset se cubre en L1. Input táctil se simula vía events, no hay QA táctil real.
- El test full L1–L5 fuerza ×1200 sólo para acelerar; un flujo L1 separado pasa al ×30 default en batch (43.8s scripted). La sesión normal de 5 niveles, duración con usuario y responsividad siguen por probar con personas/device.

## BLOQUEADO / NO VERIFICADO

- MCP vivo del Editor original inestable: en el estado actual HTTP lista la instancia y entrega `project/info`, pero `editor/state`, `get_active` y `read_console` fallan por ping no respondido/sesión desconectada; no hay confirmación de escena/consola/render vivo. Suite corre en copia aislada, no GUI. Se preservó el Editor abierto sin reinicio; falta reconectar para revisar consola, jerarquía y render importado.
- Sin push: `gh` no tiene sesión GitHub autenticada; se requiere `gh auth login`.
- Sin prueba de dispositivo, safe areas, rendimiento ni legibilidad portrait final.
- Android build posterior a los fixes actuales y QA en dispositivo real pendientes; revisión visual final del launcher, arte/VFX/audio de producción y aceptación del gate MVP pendientes.

## TESTS / EDITOR

- Unity Editor `6000.6.3f1`, escena `Assets/Scenes/SampleScene.unity`.
- Editor conectado actual: PlayMode full suite 6/6 (job `7d2f3e02417249ffb3d29eedf5b949d0`) y EditMode 13/13 (job `afc695658d5845e99c6d85d49ee56ef1`). L1–L5 full cycles a ×1200 sólo aceleran test; prueba L1 ×30 tarda 44s scripted.
- Runtime full cycles en batch: L1–L5 con composiciones 2/3/4/4/6 porciones; L4 incluye vacío, L5 provoleta; L1–L4 desbloquean Next, L5 vuelve a selección.
- Unity MCP: HTTP enumera `Asadito@65a4fad638bbc94c`; `editor/state`, escena activa y jerarquía respondieron. Editor idle, `SampleScene`, no dirty. Consola: 1 warning del `WebSocketTransportClient` (`WebSocket is not initialised`), sin errores de app/compilación; las herramientas HTTP responden. Las suites corrieron en este Editor y se inspeccionó GameView portrait.

## BUILD / PUBLICACIÓN

- Unity Android rebuild previo a los fixes C# completó `Succeeded` (0 errores, 1 warning C++ no bloqueante); APK temporal 47 MB en `/tmp/AsaditoFinalVisualUI.apk`, firma Debug v2 verificada. Rebuild vigente e instalación/prueba en teléfono pendientes; `Builds/Android/Asadito.apk` no se reemplazó y `com.DefaultCompany.Asadito` sigue provisional.
- El arte/UI/motion y documentación de estado están versionados localmente en `main`; el push normal sigue bloqueado por falta de credenciales HTTPS (`gh auth status` sin sesión). Requiere `gh auth login`; nunca force push.

Ver [matriz integral](mvp-audit.md) para acceptance, evidencia y próximos bloqueantes.
