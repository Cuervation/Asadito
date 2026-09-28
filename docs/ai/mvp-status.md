# Estado del MVP de Asadito

## IMPLEMENTADO

- PlayMode valida el loop de servicio L1–L5; cada porción llega al punto, se sirve, puntúa/guarda y desbloquea Next; L5 vuelve a selección. Retry/reset está comprobado independientemente en L1.
- Unity batch aislado tras integrar iconografía y motion procedurales: PlayMode full 6/6 Passed (`/tmp/AsaditoFinalPlayTests.xml`) y EditMode 13/13 (`/tmp/AsaditoFinalEditTests.xml`). Un L1 full-cycle al ×30 tardó 44s scripted, bajo el límite de tuning de 180s; no mide deliberación humana ni device. Auditoría cargó dos escenas, cero prefabs, revisó 14 componentes (0 scripts faltantes) y encontró 0 GUIDs serializados irresolubles. MCP enumera proyecto/instancia y recuperó `SampleScene`, pero no responde estable para estado/consola/jerarquía.
- SDD liviano: alcance, sistemas, gate de aceptación y dirección visual canónicos.
- Catálogo de cinco niveles con menú de selección, intro dinámica y composición de botones/porciones variable; seis perfiles de comensal. L1–L5 navegan al gameplay en Editor sin servir.
- Modelos de fuego/cocción/asignación/evaluación; vacío lento y fases térmicas de provoleta; HUD térmico, movimiento, bandeja, servicio, score, persistencia local, Retry y Next implementados en código.
- Fondo cenital, atlas térmico (16 cortes), retratos (24 cortes), wordmark Lilita One, launcher icon, food icons y set propio de system/action icons están integrados. PlayMode valida sprites, cinco flujos, iconos de comida en Level Select/Intro/Results, glifos y movimiento básico. Arte/íconos siguen PROVISIONALES hasta review en GUI/device; UI responsive, VFX/audio de producción y touch/performance siguen pendientes.
- Pruebas EditMode actuales: 13/13 pasaron en Unity batch aislado; PlayMode actual 6/6.

## PROVISIONAL

- El juego utiliza Canvas procedural 2D; el fondo cenital sigue siendo una pieza provisional. Hay cuatro estados genéricos por comida en atlas, pero no variantes por cara; el set no es final.
- Vacío/provoleta ya tienen sprites distintos; aún faltan aprobar comida y tarjetas de comensales, revisar portada/UI responsive y hacer review en device; wordmark/cover/icono están integrados pero no aprobados.
- Iconografía UI de comida/guest/lock/stars/navigation/actions ya integrada como atlas más sprites vectoriales procedurales; queda PROVISIONAL hasta review y accesibilidad en device. Algunos detalles secundarios aún son texto/Unicode.
- Brasas/humo, entrada/CTA, ignition, tween de celdas, lift/release/flip, bandeja, reacción, transición/score, sizzle y háptica funcionan de forma básica; ninguno está aprobado visual/sonoramente en device.
- Full cycle automatizado cooking→serving→results/progression pasa en L1–L5; Retry/reset se cubre en L1. Input táctil se simula vía events, no hay QA táctil real.
- El test full L1–L5 fuerza ×1200 sólo para acelerar; un flujo L1 separado pasa al ×30 default en batch (43.8s scripted). La sesión normal de 5 niveles, duración con usuario y responsividad siguen por probar con personas/device.

## BLOQUEADO / NO VERIFICADO

- MCP vivo del Editor original inestable: refresh recuperó el servidor y `get_active` devolvió `SampleScene`, pero `editor/state`/consola fallaron y `get_hierarchy` se desconectó. Suite corrió en copia aislada, no GUI. Se preservó el Editor abierto sin reinicio; falta reabrir/reconectar para leer consola, jerarquía y render importado.
- Sin push: `gh` no tiene sesión GitHub autenticada; se requiere `gh auth login`.
- Sin prueba de dispositivo, safe areas, rendimiento ni legibilidad portrait final.
- Arte final, VFX/sonido dedicados, revisión visual de portada/logo/icono, responsive/safe areas, perf y revisión final de Acceptance Gate pendientes.

## TESTS / EDITOR

- Unity Editor `6000.6.3f1`, escena `Assets/Scenes/SampleScene.unity`.
- Compilación/batch actual: PlayMode full suite 6/6 (`/tmp/AsaditoFinalPlayTests.xml`) y EditMode 13/13 (`/tmp/AsaditoFinalEditTests.xml`). L1–L5 full cycles a ×1200 sólo como aceleración de test; prueba L1 independiente a ×30 default tarda 44s scripted.
- Runtime full cycles en batch: L1–L5 con composiciones 2/3/4/4/6 porciones; L4 incluye vacío, L5 provoleta; L1–L4 desbloquean Next, L5 vuelve a selección.
- Unity MCP: HTTP enumera `Asadito@65a4fad638bbc94c` y project/info confirma proyecto/versión; active scene `SampleScene` sí leyó, pero state/read_console/hierarchy/windows/cameras siguen fallando con timeout/desconexión. No se terminó el Editor para no arriesgar estado; no se pudo inspeccionar consola, jerarquía ni render.

## BUILD / PUBLICACIÓN

- Unity Android rebuild aislado posterior a UI/motion completó `Succeeded` (0 errores, 1 warning C++ no bloqueante); APK temporal 47 MB en `/tmp/AsaditoFinalVisualUI.apk`, firma Debug v2 verificada. No se instaló/probó en teléfono; `Builds/Android/Asadito.apk` no se reemplazó y `com.DefaultCompany.Asadito` sigue provisional.
- Último commit local de implementación/corrección visual `45f5984` (`art: integrate MVP UI icons and feedback motion`); incluye iconografía UI y motion core. El remoto no expone refs; push normal debe reintentarse tras cierre docs y fallará sin credenciales HTTPS (`gh auth status` sin sesión). Requiere `gh auth login`; nunca force push.

Ver [matriz integral](mvp-audit.md) para acceptance, evidencia y próximos bloqueantes.
