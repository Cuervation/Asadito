# Estado del MVP de Asadito

## IMPLEMENTADO

- PlayMode valida el loop de servicio L1–L5; cada porción llega al punto, se sirve, puntúa/guarda y desbloquea Next; L5 vuelve a selección. Retry/reset está comprobado independientemente en L1.
- Unity batch aislado tras actualizar portada y limpiar URP: PlayMode full 6/6 Passed (`/tmp/AsaditoPlayTests.final2.xml`) y EditMode 13/13 (`/tmp/AsaditoEditTests.final.xml`). Un L1 full-cycle al ×30 tardó 44s scripted, bajo el límite de tuning de 180s; no mide deliberación humana ni device. Auditoría cargó dos escenas, cero prefabs, revisó 14 componentes (0 scripts faltantes) y encontró 0 GUIDs serializados irresolubles. MCP del Editor original enumera instancia/proyecto pero state/console/hierarchy no responden; no hay QA viva.
- SDD liviano: alcance, sistemas, gate de aceptación y dirección visual canónicos.
- Catálogo de cinco niveles con menú de selección, intro dinámica y composición de botones/porciones variable; seis perfiles de comensal. L1–L5 navegan al gameplay en Editor sin servir.
- Modelos de fuego/cocción/asignación/evaluación; vacío lento y fases térmicas de provoleta; HUD térmico, movimiento, bandeja, servicio, score, persistencia local, Retry y Next implementados en código.
- Fondo cenital, atlas térmico (16 cortes), retratos (24 cortes), wordmark Lilita One y launcher icon propio ya están integrados. PlayMode valida carga/cambios de arte, glifos españoles y entrada animada del CTA; el logo y el icono son PROVISIONALES hasta review móvil. El key art de portada se reemplazó por una pieza original coherente con el gameplay cenital y se verifica su ratio/carga; logo/cover/icono aún son PROVISIONAL sin review GUI/device. UI/VFX y revisión de resolución/touch siguen pendientes.
- Pruebas EditMode: 13/13 pasaron en Unity batch aislado; ejecución previa del Editor MCP original fue 13/13 antes de los cambios nuevos.

## PROVISIONAL

- El juego utiliza Canvas procedural 2D; el fondo cenital sigue siendo una pieza provisional. Hay cuatro estados genéricos por comida en atlas, pero no variantes por cara; el set no es final.
- Vacío/provoleta ya tienen sprites distintos; aún faltan aprobar comida y tarjetas de comensales, revisar portada/UI responsive y hacer review en device; wordmark/cover/icono están integrados pero no aprobados.
- Falta un set de iconos UI dedicados para comida, stars/locks/navigation/settings; hoy algunos indicadores son texto, Unicode o geometría Canvas. Es un gap de arte/UI (no de referencias ni del flujo).
- Brasas/humo, animaciones de entrada/CTA, reacción, puntaje y transición, sizzle sintético y háptica opcional funcionan de forma básica; ninguno está aprobado visual/sonoramente en device.
- Full cycle automatizado cooking→serving→results/progression pasa en L1–L5; Retry/reset se cubre en L1. Input táctil se simula vía events, no hay QA táctil real.
- El test full L1–L5 fuerza ×1200 sólo para acelerar; un flujo L1 separado pasa al ×30 default en batch (43.8s scripted). La sesión normal de 5 niveles, duración con usuario y responsividad siguen por probar con personas/device.

## BLOQUEADO / NO VERIFICADO

- MCP vivo del Editor original inestable/no responde: state/console hacen timeout, windows/cameras reportan plugin desconectado aun después de telemetry ping. Suite y prueba focal corrieron en copia aislada, no su GUI. Se preservó el Editor abierto sin reinicio; falta reconexión para leer consola, jerarquía y render importado.
- Sin push: `gh` no tiene sesión GitHub autenticada; se requiere `gh auth login`.
- Sin prueba de dispositivo, safe areas, rendimiento ni legibilidad portrait final.
- Arte final, VFX/sonido dedicados, revisión visual de portada/logo/icono, responsive/safe areas, perf y revisión final de Acceptance Gate pendientes.

## TESTS / EDITOR

- Unity Editor `6000.6.3f1`, escena `Assets/Scenes/SampleScene.unity`.
- Compilación/batch actual: PlayMode full suite 6 passed, 0 failed (+ foco 1/1 assets/glifos) y EditMode 13 passed, 0 failed. L1–L5 full cycles a ×1200 sólo como aceleración de test; prueba L1 independiente a ×30 default tarda 43.8s scripted.
- Runtime full cycles en batch: L1–L5 con composiciones 2/3/4/4/6 porciones; L4 incluye vacío, L5 provoleta; L1–L4 desbloquean Next, L5 vuelve a selección.
- Unity MCP: HTTP enumera `Asadito@65a4fad638bbc94c` y project/info confirma proyecto/versión; state/read_console/get_active/get_hierarchy/windows/cameras fallan con timeout o plugin desconectado tras telemetry ping. No se terminó el Editor para no arriesgar estado; no se pudo inspeccionar consola, jerarquía ni render.

## BUILD / PUBLICACIÓN

- Unity Android build aislado con portada cenital y renderer URP limpio completó `Succeeded` (0 errores, 1 warning); APK temporal de 47 MB, AAPT confirma icono adaptive y `apksigner` verifica firma debug v2. No se instaló/probó en teléfono; `Builds/Android/Asadito.apk` no se reemplazó (artefacto ignorado) y `com.DefaultCompany.Asadito` sigue provisional.
- HEAD local `5bb029c` (`Polish Asadito visual identity slice`) en `main`; commit local completado. El push `origin/main` falló porque no hay credenciales HTTPS disponibles (`gh auth status` sin sesión). Tras `gh auth login`, reintentar push normal; nunca force push.

Ver [matriz integral](mvp-audit.md) para acceptance, evidencia y próximos bloqueantes.
