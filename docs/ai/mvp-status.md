# Estado del MVP de Asadito

## IMPLEMENTADO

- PlayMode valida el loop de servicio L1–L5; cada porción llega al punto, se sirve, puntúa/guarda y desbloquea Next; L5 vuelve a selección. Retry/reset está comprobado independientemente en L1.
- Unity batch aislado: PlayMode full suite 6/6 Passed (`/tmp/AsaditoPlayTests.current.xml`) + prueba focal de arte/fonts/glifos 1/1 (`/tmp/AsaditoVisualAssetsTest.current.xml`), EditMode 13/13 (`/tmp/AsaditoEditTests.current.xml`). Un L1 full-cycle al ×30 configurado tardó 43.8s en interacción scripted, bajo el límite de tuning de 180s; no mide deliberación humana ni device. MCP del Editor original pasó 3/3 anteriormente, pero su consola ahora no es accesible.
- SDD liviano: alcance, sistemas, gate de aceptación y dirección visual canónicos.
- Catálogo de cinco niveles con menú de selección, intro dinámica y composición de botones/porciones variable; seis perfiles de comensal. L1–L5 navegan al gameplay en Editor sin servir.
- Modelos de fuego/cocción/asignación/evaluación; vacío lento y fases térmicas de provoleta; HUD térmico, movimiento, bandeja, servicio, score, persistencia local, Retry y Next implementados en código.
- Fondo cenital, atlas térmico (16 cortes), retratos (24 cortes), wordmark Lilita One y launcher icon propio ya están integrados. PlayMode valida carga/cambios de arte, glifos españoles y entrada animada del CTA; el logo y el icono son PROVISIONALES hasta review móvil. El key art de portada se reemplazó por una pieza original coherente con el gameplay cenital y se verifica su ratio/carga; logo/cover/icono aún son PROVISIONAL sin review GUI/device. UI/VFX y revisión de resolución/touch siguen pendientes.
- Pruebas EditMode: 13/13 pasaron en Unity batch aislado; ejecución previa del Editor MCP original fue 13/13 antes de los cambios nuevos.

## PROVISIONAL

- El juego utiliza Canvas procedural 2D; el fondo cenital sigue siendo una pieza provisional. Hay cuatro estados genéricos por comida en atlas, pero no variantes por cara; el set no es final.
- Vacío/provoleta ya tienen sprites distintos; aún faltan aprobar comida y tarjetas de comensales, revisar portada/UI responsive y hacer review en device; wordmark/cover/icono están integrados pero no aprobados.
- Brasas/humo, animaciones de entrada/CTA, reacción, puntaje y transición, sizzle sintético y háptica opcional funcionan de forma básica; ninguno está aprobado visual/sonoramente en device.
- Full cycle automatizado cooking→serving→results/progression pasa en L1–L5; Retry/reset se cubre en L1. Input táctil se simula vía events, no hay QA táctil real.
- El test full L1–L5 fuerza ×1200 sólo para acelerar; un flujo L1 separado pasa al ×30 default en batch (43.8s scripted). La sesión normal de 5 niveles, duración con usuario y responsividad siguen por probar con personas/device.

## BLOQUEADO / NO VERIFICADO

- MCP vivo del Editor original inestable/no responde tras warning WebSocket; suite completa 6/6 corrió en copia aislada, no su GUI. Requiere reconexión segura para leer consola y revisar render importado.
- Sin push: `gh` no tiene sesión GitHub autenticada; se requiere `gh auth login`.
- Sin prueba de dispositivo, safe areas, rendimiento ni legibilidad portrait final.
- Arte final, VFX/sonido dedicados, revisión visual de portada/logo/icono, responsive/safe areas, perf y revisión final de Acceptance Gate pendientes.

## TESTS / EDITOR

- Unity Editor `6000.6.3f1`, escena `Assets/Scenes/SampleScene.unity`.
- Compilación/batch actual: PlayMode full suite 6 passed, 0 failed (+ foco 1/1 assets/glifos) y EditMode 13 passed, 0 failed. L1–L5 full cycles a ×1200 sólo como aceleración de test; prueba L1 independiente a ×30 default tarda 43.8s scripted.
- Runtime full cycles en batch: L1–L5 con composiciones 2/3/4/4/6 porciones; L4 incluye vacío, L5 provoleta; L1–L4 desbloquean Next, L5 vuelve a selección.
- Unity MCP: original GUI/HTTP del Editor dejó de responder luego de import/test, con warning `WebSocket is not initialised`; el proceso Editor sigue abierto y no se terminó para evitar perder estado. MCP HTTP local estuvo levantado, pero no se pudo confirmar instancia viva ni leer consola tras esa falla.

## BUILD / PUBLICACIÓN

- Unity Android build aislado con el nuevo arte completó `Succeeded` (0 errores, 1 warning); APK de 46 MB en output temporal, AAPT confirma icono adaptive en densidades y `apksigner` verifica firma debug. No se instaló/probó en teléfono; `Builds/Android/Asadito.apk` no se reemplazó (artefacto ignorado y anterior) y el package ID `com.DefaultCompany.Asadito` sigue provisional.
- HEAD local `90e39a3` (`Integrate five-level MVP and Android build support`) en `main`; cambios intencionales en gameplay/arte/tests/docs siguen sin commit. El remoto público está vacío (sin refs); `gh auth status` confirma sin sesión. El commit local no pudo subirse hasta hacer login seguro; nunca force push.

Ver [matriz integral](mvp-audit.md) para acceptance, evidencia y próximos bloqueantes.
