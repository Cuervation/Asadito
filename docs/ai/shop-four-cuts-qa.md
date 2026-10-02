# Carnicería atendida: catálogo completo / compra para stock — QA (2026-10-02)

## Pedido y alcance
El usuario eligió la atención cercana y pidió cuatro alimentos por pantalla, flechas para todos los cortes y exactamente las imágenes de parrilla/heladera. Autorizó precios para todos y compra libre para stockear, incluso si luego no alcanza para el objetivo del asado. Implementado en el Unity real /Users/celestino/Asadito; no sólo el prototipo HTML.

## Implementación
-18 SKU, cinco páginas4+4+4+4+2; precio/stock/cantidad y +/− nativos, tap/drag al changuito, carrito persistente entre páginas y detalle filtrado a productos elegidos. Back/fridge cancelan preview; PAGAR Y SALIR revalida y persiste una transacción.
- Todas las ilustraciones alimentarias son el mismo Sprite RAW del atlas FoodSpriteLibrary usado en heladera/parrilla. Tarjetas escalan uniformemente; no se agregaron PNG ni se tocaron alimentos. Clerk Butcher_Neutral existente agregado al asset de referencias.
- Saldo inicial650, capacidad8, stock24/SKU, rewards/frescura/gates/recetas/cocción sin cambios. Precios chorizo100/tira180 preservados; todos UnlockLevel1. No reserva del presupuesto del pedido ni condición de coincidencia con el objetivo.
- MvpSaveData constructor: arrays vacíos sin Resources.Load; migración y registro mantienen tamaño dinámico y progreso v5 compatible. Corrige excepción al recargar dominio del Editor.

## Precios por pieza (monedas del juego)
| Alimento | Precio |
|---|---:|
| Chorizo |100|
| Tira de asado |180|
| Vacío |220|
| Provoleta |140|
| Entraña |240|
| Colita de cuadril |230|
| Lomo |320|
| Bife ancho |270|
| Bife angosto |260|
| Bife de chorizo |280|
| Ojo de bife |300|
| Chinchulines |110|
| Morcilla |90|
| Morcilla vasca |120|
| Matambre de cerdo |190|
| Costillita de cerdo |160|
| Solomillo de cerdo |210|
| Pollo deshuesado |170|

## Verificación
- Unity6000.6.3f1, import/compile sin errores de fuente; EditMode46/46 PASS (ManagementTests44 y dos casos de guardado/progreso). Incluye18 SKU únicos/precio positivo/unlock1, compra lomo×2 en L1 por640 dejando10, persistencia exacta de IDs, rechazo de compra del pedido por saldo insuficiente sin mutación, constructor seguro y migración de progreso.
- PlayMode12/12 PASS (105.987s), job06bb375dac0c4621bcdf2f972e01a869. Rechequeo final de paginado/RAW/carrito/pulso/textos1/1 PASS (4.645s), job4e0afc01f54e46d1ab4571f45d1dd5c6. Cubre cinco páginas/18 sprites, precios/cantidades, compra no-pedido y recarga, stock0/heladera vacía, drag/cancelación/otro dedo/duplicados, errores atómicos, detalle/vaciar/pago/preparación/IDs, capacidad8, compra→cocción→servicio/reward/guardado y recuperación/tutorial.
- Tools/validate_food_content.py PASS:18 perfiles,108 frames, footprints y layouts L1–12, postales/arte/fuentes. Scoped git diff --check PASS;126 diffs previos permanecen idénticos, y todas las líneas añadidas preexistentes en tests/estado permanecen. Dos InitTestScene huérfanas creadas por estas corridas fueron eliminadas con AssetDatabase.
- Capturas del Canvas nativo a1080×1920 y720×1600, las cinco páginas y detalle/errores/stock0/heladera. Cámara/RT temporal sólo en fixture QA; no arquitectura runtime de cámara de gestión. Text fit y tamaños táctiles verificados en pruebas; revisión visual detectó aviso encima de TU CARRITO y se corrigió. Último ancho del aviso limitado a62% para no tapar flechas estrechas. Cambio de página restablece escala del cart pulse interrumpido. Capturas finales en output/integration/shop-four-cuts del workspace de chat, incluyendo carniceria-unity-live.png y carniceria-unity-foldable.png.
- Incidencias del runner: corrida inicial falló por helper FindButton que exige interactable al verificar flechas Disabled; corregida prueba. Dos corridas abortaron/orphan por Unity TestRunner PlayModeRunTask NullReference, sin contar como PASS; limpiado job, SampleScene recuperada y Editor traído al frente con open -a. No cambios de paquetes/runner.

## Límites y entrega
No APK/build/install, commit ni push nuevos en este pedido. Cambios previos de cooking bars y122 importers .meta preservados, no incluidos como trabajo de esta integración. La elección de precios es una primera configuración ajustable de gameplay, no balance final probado. Touch/notch/performance/aprobación artística Android física pendientes. Simulador abierto sobre SampleScene en Play sin pausa, nueva carnicería ejercitada y capturada; al cierre la pantalla estaba en planificación y no se forzó nueva navegación. dispositivo Foldable960×2658 existente, captura del Canvas live adicional revisada. Saldo mostrado es el real de la sesión (sin monedas demo agregadas para la captura).
