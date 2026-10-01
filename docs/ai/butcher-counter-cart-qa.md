# Carnicería mostrador/carrito — QA 2026-10-01

## Auditoría y alcance
Baseline `3867cb0`, main limpio, economía de compras desdeL1. ManagementScreen usaba dos cards y confirmación por unidad; ManagementService.Buy validaba una transacción individual. ManagementState/savev5 ya contiene wallet, inventario, purchases y preparedrun. Fondo ButcherShop_Background ya ofrece una carnicería argentina ilustrada/vitrina; cinco sprites de gestión conectados y FoodCatalog para cortes crudos. No fue necesaria regeneración de arte ni cambio económico/config.

## Implementación
- Vitrina UI nativa sobre ilustración existente; dos bandejas de los productos desbloqueados con área táctil, precios dinámicos, stock, cantidades y highlight. Pulso0.22s y sonido existente por selección.
- Carrito efímero en ManagementScreen: +/−/vaciar, subtotales, total, saldo y aviso. Volver/ir a heladera cancela selección sin cargos. Pedido orienta también L1 sin bloquear compras libres.
- QuoteCart no muta; BuyCart revalida todos los cortes/fondos/capacidad/unlocks/run, único débito, costos e IDs deterministas y registros de stock. Buy individual delega y conserva errores cortos/promos existentes. No nuevas promos activadas.
- PAGAR Y SALIR guarda el agregado una vez y abre heladera con aviso✓; preparación/cocina/recovery conservados.
- Savev5 normaliza únicamente ActiveRun sentinel Level0/FoodCost0/sin unidades: JsonUtility podía reconstruir null como objeto vacío y bloquear operaciones después de recargar. Runs preparados/recovery reales conservados.

## Evidencia ejecutada
Unity6000.6.3f1 en checkout QA importado `/tmp/Asadito-concurrent-cooking-qa`; Editor principal/Library no intervenidos. Source Assets sincronizados.

| Check | Evidencia | Resultado |
|---|---|---|
| Compile + EditMode ManagementTests | `/tmp/asadito-counter-edit-closure.xml`, log homónimo | **34/34 PASS**,0.1266s; cotización/compra, saldo/stock/capacidad/unlock/run activo, atomicidad, cantidades inválidas/overflow, revalidación, costos, save/migración/persistencia y reglas previas |
| L5 compra→heladera→cocina→servicio→reload/recompensa | `Management_BuyPrepareCookServeAndPersistBalance`, `/tmp/asadito-counter-play.xml` | **PASS**; selección4sin cargos, checkout560, preparedrun preservado y reward real |
| Canasta/cancelar/vaciar/restar/compra/preparación + recovery | `/tmp/asadito-counter-play-final.xml` | **2/2 PASS**,33.35s; frío cache/save y guía persistida |
| Cierre visual + canasta con código final | `/tmp/asadito-counter-visual-closure.xml` | **1/1 PASS**,4.791s; capture doble render para refrescar atlas dinámico de fonts y evitar previsualización borrosa |
| Compilación final + errores y raycast | `/tmp/asadito-counter-errors-final.xml` | **1/1 PASS**,3.676s; primer hit real en cada corte es su Button; saldo/stock/heladera explícitos, pago deshabilitado y callback revalida sin mutación |
| Diff whitespace | `git diff --check` | PASS |
| Renders | `/Users/celestino/Asadito/build/qa-counter/` | Empty/cart/paid-fridge/no-money1080×1920 revisados visualmente; texto/acciones dentro de portrait |

Las corridas iniciales no fueron todos PASS: EditMode33/34 reveló ActiveRun vacío al recargar (persistencia corregida); PlayMode1/3 tenía dos asserts de botón Disabled que llamaban un helper que exigía interactable (driver corregido, ambos pasan en cierre). Primer raycast de errores reveló MakePanel.raycastTarget=false (tray corregida explícitamente, rerun1/1PASS). Las pruebas de callback solas no prueban que el toque llegue; ahora se verifica el hit real. No se repitió suite completa por cambio local.

## Archivos modificados
- `Assets/Asado/Scripts/ManagementScreen.cs` — representación e interacción/cart.
- `Assets/Asado/Scripts/Runtime/ManagementService.cs` — quote/checkout atómico y compatibilidad Buy.
- `Assets/Asado/Scripts/Runtime/MvpSaveData.cs` — normalización sentinel vacío.
- `Assets/Tests/EditMode/ManagementTests.cs` y `Assets/Tests/PlayMode/FirstPlayableFlowTests.cs` — pruebas focales y adaptadores al nuevo checkout.
- `specs/product/butcher-shop.md`, `specs/acceptance/management-vertical1-gate.md`, `docs/art/management-asset-manifest.md`, `docs/current-project-state.md` — contratos/estado.
- Este reporte.

## Límites
No nueva APK ni instalación por este pedido de UX: celular aún tiene1.3.0 previa. No afirmar QA táctil/performance físico ni suite completa. Renders de Unity y GraphicRaycaster automatizado no reemplazan playtesting humano. Arte/UI conserva estado PROVISIONAL hasta aprobación. Catálogo actual tiene dos cortes; más productos exigirán layout/scroll apropiado, no reducir su legibilidad automáticamente.
