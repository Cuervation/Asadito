# Gestión 100 % 2D — QA final (2026-10-02)

## Checkout y alcance
- Checkout real `/Users/celestino/Asadito`, rama `main`, baseline `6238ae1`; remoto verificado antes de empezar. El directorio de chat solo contiene arte/documentos.
- Cambios locales previos de mini barras de cocción preservados y excluidos del commit de migración, también en los archivos compartidos. Sin cambio a configuración económica, catálogo, service/state/wallet/save, niveles/pedidos, escenas, paquetes ni terceros/MCP.
- Auditoría y contratos: [arquitectura](../architecture/management-2d.md). `ManagementWorldView` sustituido por `ManagementFoodView`, assembly `Asadito.Management2D`, dentro del Canvas. Ninguna cámara/RenderTexture/malla/collider/material/raycast físico 3D de gestión queda en runtime.
- Reutilizados CounterV2, Fridge_Hybrid_OpenEmptyV2, FoodSpriteLibrary/atlas RAW y medidas reales de parrilla, fuentes/botones/retratos/escenarios existentes. Preservados GUID/meta de recursos reutilizados; retirada únicamente geometría/shaders/generadores/bake de Asadito sin referencias. Máscaras alfa 128×64 de los 18 RAW: 18 KiB decodificados, sin hacer readable los atlas ni regenerar imágenes.

## Pruebas realmente ejecutadas
Unity **6000.6.3f1** del proyecto real, Editor abierto vía Unity MCP; no APK ni suite completa.

| Comprobación | Resultado | Alcance |
|---|---|---|
| Import/compilación C# y Console | PASS, sin errores de compilación | Assembly 2D, UI, tests; tipos actuales comprobados en Editor. |
| `Tools/validate_food_content.py` | PASS | 18 perfiles, 108 frames, huellas/layouts L1–L12 y 12 postales. |
| `ManagementTests` EditMode | **41/41 PASS**, 1.5269 s | Cotización/pago único, errores atómicos, stock24/precios, capacidad8, IDs/preparación, frescura deshabilitada, recovery, resultados y persistencia/modelo. |
| Ocho tests `Management_2D*` + `Management_BuyPrepareCookServeAndPersistBalance` | **9/9 PASS**, 69.1160 s | Stock48/partial/0, arte RAW exacto/tamaño, alfa/sibling order/carteles, clicks duplicados, drag/drop/cancel/otro puntero/UI, IDs/capacidad/selección interrumpible/reload, preparación/parrilla y compra→cocción simultánea→tabla→servicio→saldo guardado. |
| Cierre tras ajustar capas gráficas de botones a safe width | **3/3 PASS**, 16.9740 s | Shop/safe-area/cart detail, drag/pago/parrilla, heladera8/overlap/quinta selección; mismos alimentos, sin reejecutar suite completa. |

Evidencia local: `build/qa-management-2d/editmode-management-41.xml`, `playmode-final-9.xml`, `playmode-final-layout-3.xml`, `content-validator.txt` y PNG. No se versionan builds/logs/PNG grandes en este commit.

## Fallos encontrados y resueltos
- Primera corrida 5/8: los nuevos fixtures confundían centros ocultos con superficies visibles y no reconstruían el botón guiado tras inyectar inventario. Ajustados para tocar píxeles opacos expuestos/IDs exactos y reconstruir navegación nativa; no se añadieron hit boxes invisibles ni se saltaron reglas.
- Tras estabilizar portrait: descubierto target invisible de parrilla que interceptaba el botón − del carrito por el Canvas con overrideSorting. `IsFoodTargetClosest` ahora rechaza raycasts cuando gestión está abierta; la regresión real de EventSystem pasa.
- Precio/soportes opacos bloquean piezas tapadas. Slots de preparación no repiten posición al llegar a la quinta pieza. Arranque de drag restaura pulso; otro puntero cancela, release repetido no agrega.
- Capturas estrechas detectaron bordes de botones recortados: rects y capas arcade sliced ahora adaptan el ancho seguro sin reducir alimentos ni cambiar fuentes/altura. UI se actualiza al cambiar dimensiones/datos, no se reconstruye por tap.
- MCP conservaba un job huérfano de sesión anterior: se limpió y se importó la fuente nueva. Una inicialización fallida durante importación no se contó como PASS. Re-serialización incidental de meta/settings Unity se excluyó, conservando originales.

## Capturas revisadas
Renders **reales de UI Unity** a 1080×1920 y **720×1600**, mediante cámara ortográfica/RenderTexture temporal solo de QA (no mundo/runtime de gestión):
- `asadito-2d-shop-full.png`, `shop-narrow.png`: 48 piezas a tamaño de parrilla, contornos/stock/precios legibles, superposición real, sin comida ficticia ni recorte de cortes.
- `asadito-2d-cart-expanded.png`, `cart.png`: resumen +/−, total, contenido visible y CTA existentes; raycast del detalle verificado.
- `asadito-2d-fridge-full.png`, `fridge-full-narrow.png`, `fridge-four-selected.png`, `fridge-selected-narrow.png`: ocho IDs reales, estantes/tabla, selección reversible, alimentos completos y footer accesible dentro del ancho seguro.
- `asadito-2d-grill-size-reference.png`: mismos sprites/medidas en la parrilla cenital, tabla lateral y barra local previa preservada; sin botones flip/bandeja/servir ni encendido/carbon/apagado nuevos.
- Stock0 y heladera vacía también capturados. Planificación/resultados del recorrido pagado conservan arte2D existente.

## Límites honestos
No se midieron FPS, memoria/performance, multitouch humano, notch/safe-area físicos ni se instaló Android. La ausencia de mundos 3D reduce componentes/recursos por diseño, no constituye una medición en teléfono. Motorola conserva la APK1.4.0 histórica; push Git no la actualiza. Aprobación estética humana y QA Android física siguen pendientes. Historial híbrido/3D conservado como evidencia, no arquitectura vigente.
