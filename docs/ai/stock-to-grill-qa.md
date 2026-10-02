# Compra → preparación → parrilla — QA 2026-10-02

## Diagnóstico observado

El save real del reporte tenía saldo90, cuatro unidades pagadas (IDs1,2 chorizo100; IDs3,4 tira180), compras2+2 y ningún ActiveRun. `CanPrepareUnits(1,[1,3])` daba true. Se reprodujo el picking por superficie RAW con EventSystem en el contexto real del Simulator: una pieza dejaba PREPARAR deshabilitado; un chorizo y una tira habilitaban la acción. No había pérdida de compra ni run fantasma.

Dos problemas: la navegación sólo ofrecía HELADERA (el pago sólo anunciaba que guardó stock), y el tutorial imponía una cuota oculta por SKU. Cualquier corte fuera del pedido tenía Required=0 y quedaba rechazado incluso con tabla vacía; dos chorizos también se rechazaban. El aviso erróneo decía que la bandeja estaba completa.

La especificación de inventario conserva **cantidad del pedido** y permite cambiar la mezcla/asumir consecuencias culinarias. No se cambió ese contrato ni se consume toda la heladera automáticamente.

## Cambios

- Planificación: PREPARAR ASADO abre selección; carnicería y heladera siguen accesibles.
- Pago: explica cuántas piezas elegir y que el resto queda guardado.
- Heladera: instrucciones persistentes y CTA contextual ELEGÍ N PIEZAS / FALTA N / IR A LA PARRILLA.
- Tutorial: acepta cortes comprados y mezclas repetidas, no cuotas por SKU. Podridas y transacción inválida siguen rechazadas; batch exacto y preparación atómica por IDs siguen igual.
- Sin cambios de arte, precios, saldo, save schema, térmica o scoring.

## Pruebas actuales

Unity6000.6.3f1, escena SampleScene; compilación/Console sin errores al cierre.

1. **RED real antes de corregir**: `Management_2DStockingNonOrderFoodPaysAndReloadsExactRawUnits` extendido falló: "A paid non-order cut must not be rejected by the debut tutorial". Job `dbcd59201121489a8ba20aaa56d28324`.
2. **PASS1/1**, 29.276s, job `d7eaa8130e6047bd988ccae6f9ea70c7`: `Management_StockedDebutHasClearNativeRouteToGrillAndKeepsExtras`. Compra2chorizos+2tiras ($560, saldo90), aviso claro, input nativo/alpha real, selección duplicada admitida, batch lleno explicado, cancel/reabrir sin consumo, CTA con raycast al botón real, preparación IDs1+3, sobrantes2+4, cocción/servicio/ingreso y reload conservando sobrantes.
3. **PASS5/5**, 82.692s, job `b5ceb25cf8e648af96ec65b0e6c8c892`:
   - `Management_2DStockingNonOrderFoodPaysAndReloadsExactRawUnits`: lomo comprado, selección/input nativo, preparación pagada y cold resume sin recobro/desperdicio.
   - `Management_2DFridgeTracksExactUnitsCancelAndPrepare`: picking de IDs, cancelación, vuelta/reapertura, preparación/persistencia exacta.
   - `Management_2DFridgeFullCapacityKeepsSizeAndPicksVisibleOverlap`: capacidad8, tamaños RAW originales y capas expuestas.
   - `Management_BuyPrepareCookServeAndPersistBalance`: L5 compra/preparación/cocción/servicio/reward/resume.
   - `Management_DebutRecoveryAndGuideSurviveReloadWithoutResettingMoney`: recovery L1, ingresos/tutorial/reload.

**Total6 casos únicos PlayMode PASS.** No suite completa, tests EditMode nuevos, APK/build/install ni commit/push solicitados o ejecutados en este arreglo local.

Una corrida intermedia de2 tests abortó en infraestructura Unity Test Framework1.8.0 (`PlayModeRunTask.cs:52`, ExitPlayModeTask), tras completar1. No se contabiliza como aprobada. Se restableció PlayerPrefs desde backup exacto, se limpió sólo su escena InitTestScene huérfana y se repitieron los casos en jobs completados.

## Visual e input

Capturas Canvas nativas1080×1920 y720×1600 revisadas: instrucciones, comida, tabla, CTA y botones sin recortes. Planificación L5 con4 invitados revisada. El helper de captura usa cámara/RT transitorios sólo en tests, no arquitectura runtime3D.

Advertencia para QA del Simulator: un raycast MCP desde callback Editor usa Screen1637×914, aunque Canvas y Device.Screen sean960×2658, y puede devolver0hits. En `Canvas.willRenderCanvases` del player Screen960×2658 y top hit=Heladera2D. No confundir ese contexto Editor con fallo de input del usuario. [Clases simuladas de Unity](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/cross-platform-features/device-simulator/simulated-classes).

## Progreso y entrega local

Antes de pruebas: backups exactos de PlayerPrefs y de todos los diffs en el workspace de chat `output/debug/shop-to-grill/`. Después de pruebas se restauró exactamente el PlayerPrefs original antes de Play; no se confirmó ni consumió comida en el save real por comandos QA. Se comprobó el nuevo CTA con selección temporal por input nativo.

Después de mostrar el Simulator se observó nueva actividad completada: saldo166, ciclo1, inventario IDs2(chorizo)+3(tira), reward76 y pérdida280, run null. Se guardó snapshot nuevo y **no se volvió a restaurar el backup viejo**, para no borrar avances posteriores. Editor queda Play activo/sin pausa, Simulator abierto con resultado, Console0errores. No prueba FPS ni touch real en Android.

Revisión incremental contra baseline previo:134 diffs ajenos, incluidos importer metadata, barras de cocción y ProjectSettings, idénticos byte a byte. Archivos mezclados retienen cambios previos; sólo se añadieron/corrigieron rutas, reglas UI y pruebas de este flujo. `git diff --check` focal PASS.
