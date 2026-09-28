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

- Push remoto pendiente: `gh` no tiene sesión autenticada. Se necesita `gh auth login` antes del push normal; nunca force push.
- Sin push: `gh` no tiene sesión GitHub autenticada; se requiere `gh auth login`.
- Sin prueba de dispositivo, safe areas, rendimiento ni legibilidad portrait final.
- Sin teléfono conectado ni Emulator instalado: QA física de safe areas, touch, launcher y rendimiento pendiente. Requiere conectar/autorizar dispositivo o instalar un Emulator por separado.
- VFX/audio/arte final y aceptación del gate MVP pendientes; el build actual sí está generado, pero no se instaló.

## TESTS / EDITOR

- Unity Editor `6000.6.3f1`, escena `Assets/Scenes/SampleScene.unity`.
- Editor conectado actual: PlayMode full suite 6/6 (job `7d2f3e02417249ffb3d29eedf5b949d0`) y EditMode 13/13 (job `afc695658d5845e99c6d85d49ee56ef1`). L1–L5 full cycles a ×1200 sólo aceleran test; prueba L1 ×30 tarda 44s scripted.
- Runtime full cycles en batch: L1–L5 con composiciones 2/3/4/4/6 porciones; L4 incluye vacío, L5 provoleta; L1–L4 desbloquean Next, L5 vuelve a selección.
- Unity MCP: HTTP enumera `Asadito@65a4fad638bbc94c`; `editor/state`, escena activa y jerarquía respondieron. Editor idle, `SampleScene`, no dirty. Suites corrieron en el Editor y se inspeccionó GameView portrait. Consola: warning MCP `WebSocket is not initialised` y warning Android Diagnostics Data/Debug Symbols; 0 errores de app/compilación.

## BUILD / PUBLICACIÓN

- Unity Android build actual del commit `c0a2835` completó `Succeeded` en 421 s, 0 errores/1 warning de Diagnostics Data; APK 52.98 MB, package `com.DefaultCompany.Asadito`, firma Debug v2 verificada. `adb` está disponible desde el SDK integrado, pero no hay dispositivo conectado ni Emulator instalado; instalación/device QA pendientes. `Builds/Android/Asadito.apk` no se reemplazó; package ID sigue provisional.
- El arte/UI/motion y documentación de estado están versionados localmente en `main`; el push normal sigue bloqueado por falta de credenciales HTTPS (`gh auth status` sin sesión). Requiere `gh auth login`; nunca force push.

Ver [matriz integral](mvp-audit.md) para acceptance, evidencia y próximos bloqueantes.
