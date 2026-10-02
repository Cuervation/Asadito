# Carnicería con deslizamiento horizontal — QA 2026-10-02

## Solicitud y alcance
Usuario mantiene la carnicería actual y pide navegar cortes con el dedo a ambos lados, quitar flechas inferiores y conservar ese espacio sobre el carrito.

ManagementFoodView distingue desplazamiento horizontal de arrastre vertical de comida. Horizontal dominante al superar12unidades del Canvas: bloquea intención y elimina preview de comida. Al soltar supera max44/10%ancho: izquierda avanza/derecha vuelve una página; clamp sin wrap. Movimiento corto/vertical sobre fondo no cambia página. Vertical sobre comida mantiene drag/drop validado al carrito. Ninguno de los gestos compra automáticamente.

Relay en +/− mantiene taps nativos y permite swipe desde los botones, incluidos deshabilitados. Captura PointerEventData del dueño; cancelar/segundo dedo suprime eligibleForClick antes de pointer-up. Esto importa porque Button y drag comparten GameObject y Input System calcula isClick antes de OnPointerUp. No overlay bloqueando botones.

ManagementScreen elimina las dos flechas, conserva anclas de tarjetas(.025,.31) a(.975,.75), carrito(.5,.175)/altura224 y label de paginacióny.286; sólo amplía ancho del texto Deslizá/página/18cortes. Arte, RAW de18alimentos, cantidades/cotización/pago/economía/progreso intactos. No modificación de PNG/metadatos/escena.

## Resultados proporcionales
Unity6000.6.3f1 principal, SampleScene, target Android.
- Primera corrida job0ba107587a8f4c97a5fa67c53490c15f abortó TestFramework con NullReferenceException/ExitPlayModeTask;0casos entregados, no PASS. Se limpió sólo el job y su InitTestScene752e9b6a-2893-4380-a70b-bceaabc48aa4 creado por esa corrida.
- Corrida job3570538d4730460d91801072c6b37542: **3/4PASS**,16.6340834s. Pasaron Management_2DShopPagesAllOriginalFoodsAndKeepsCart (18RAW exactos/5páginas/cart persistente/prices/layout), Management_2DCartDragAddsOnlyOnDropAndCancelsOtherPointers, Management_2DDragRejectsButtonsMoneyAndLeavesNoCopies. Nuevo test falló sólo por usar FindButton que exige interactable=true para el botón − deshabilitado; lookup directo corrigió harness, no runtime.
- Job70631c5605b44d94b1e7c8451059c486 perdió inicialización durante reload:0casos, no PASS; clear_stuck y retry sin recargar.
- Cierre job3334e31898cf43dfa373fed49cce375b: **1/1PASS**,3.3509877s, Management_2DShopSwipeOwnsDirectionAndNeverAddsOrSpends. Verifica primera/última página, ambos sentidos, fondo/comida/botones, taps nativos, swipe corto, gesto horizontal que cruza carrito, cancelación de + antes de begin-drag, segundo dedo sobre +, puntero incorrecto, Escape/cancel y cerrar página. Guarda cantidades y estado/PlayerPrefs idénticos dentro del fixture.
- Total **4 casos únicos aprobados3+1**, no un barrido único4/4. Console0errores y scoped diff --check PASS. Renders1080×1920 y720×1600; inspección final estrecha confirma mismo arte, cuatro cortes, indicador y espacio sobre carro sin flechas.

## Guardado del Editor
El respaldo fresco playerprefs-before.json contiene la partida previa ($166,dos unidades stock, score77). Tras fallo del runner el Editor conservó estado QA temporal ($370,run de pruebaL1), distinto del respaldo. No se escribieron datos en el teléfono. Una restauración automática genérica fue rechazada por auto-review por posible pérdida de progreso posterior; no se ejecutó ni se eludió. Se pidió autorización al usuario para restaurar sólo este backup del Editor; al escribir este documento está pendiente. Mantener Unity detenido hasta resolver y no afirmar PlayerPrefs original intacto en Editor.

## Evidencia y límites
/Users/celestino/Documents/ChatGPT/Asadito/output/debug/shop-swipe:
- playmode-initial-results.xml —3PASS+fallo harness.
- playmode-final-swipe-results.xml —cierre1PASS.
- shop-first-page-1080.png,shop-swipe-1080.png,shop-swipe-720.png —renders Canvas nativos sin retoque.
- *-before.cs,*-scoped.patch,diff-before.patch,status-before.txt —scope/baseline.
- playerprefs-before.json,playerprefs-after-runner.json —respaldo local protegido; no importar al teléfono.

No fullsuite/EditMode/validator/build/install/commit/push para cambio local. APK instalada1.5.0/code7 no incorpora este swipe, intro nuevo ni guard secuencial posterior. Touch/performance físico y aprobación humana pendientes.
