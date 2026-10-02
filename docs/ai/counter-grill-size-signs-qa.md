# Carnicería: tamaño de parrilla y carteles con soporte — QA 2026-10-01

## Pedido y alcance
El usuario rechazó achicar las comidas para acomodar 48 piezas. Se conserva el tamaño de la parrilla, se admite superposición y se reemplazan las etiquetas planas por carteles como la referencia de comercio: fondo blanco, encabezado verde, precio rojo grande con contorno y soporte metálico. La foto es referencia de estilo, no de precios: chorizo $100 y tira $180. Stock24 por SKU, heladera8, saldo650 y reglas de compra intactos.

## Implementación
- AsaditoGame.ManagementFoodSize reutiliza portionVisualSizes del alimento, ya ajustadas para parrilla/bandeja/tabla. Fallback de catálogo usa FoodFootprintLayout y las mismas superficies, sin una escala especial del mostrador.
- Referencia de nivel1: chorizo185.71×71.71 y tira265.20×100.43 unidades UI. El ancho proyectado del relieve3D se calibra en la profundidad media con sus bounds completos (incluye altura, perspectiva y giro de cámara), no con un ancho fijo por celda. Fondo/frente conservan perspectiva natural; la altura visible proviene del volumen inclinado, no de distorsionar el atlas.
- Hasta48 instancias reales: por SKU dos columnas/seis filas y una segunda capa apoyada. Stock no cambia la escala. En safe areas distintas las piezas de borde se desplazan hacia adentro de la superficie, sin achicarse.
- Mismos atlas raw y mallas de relieve existentes. Colliders de silueta MeshCollider en mostrador: una caja alrededor del recorte transparente tapaba superficies expuestas de otras piezas. Heladera conserva sus BoxColliders y selección exacta por InventoryUnit.Id.
- Carteles Canvas nativos independientes: marco/papel claro, banda verde, nombre, precio rojo grande con Outline, POR PIEZA, stock/llevás, pinza/poste/base metálicos. La fuente mantiene altura≥2.5× y overflow, como el resto de gestión. Collider del cartel bloquea compras de comida oculta detrás; no es botón.
- Drag preview utiliza el mismo footprint de parrilla. Pulso/drag restauran HomeScale; QuoteCart/BuyCart/guardado y botones existentes no cambian.

## Validación proporcional
Unity6000.6.3f1 en /tmp/Asadito-concurrent-cooking-qa aislado, sin cerrar el Editor principal. No suite completa ni Android build.
- **3/3 PlayMode focales PASS** de implementación,v4/12.895s. Cierre final con comparación de ancho al1% y captura de las dos piezas sobre la parrilla: **3/3 PASS**,13.052s, /tmp/asadito-counter-large-signs-final.xml y .log.
  - Management_3DModelsReceivePhysicsTapsWithoutCards:48 modelos/atlas/UV/shaders/colliders,24 por SKU, profundidad/capas/bounds, vista780×1100 y resize, tamaño contra la medida real de parrilla, texto/números de carteles y toque sin compra oculta, toques repetidos/pulso/detalle/raycast. Muestra26–35 superficies físicamente expuestas según viewport; no exige que todos los centros sean visibles en una pila.
  - Management_3DZeroStockAndEmptyFridgeNeverInventUnits:5+3 restantes muestran8 modelos/carteles correctos; reponer a48 no cambia HomeScale. Stock0 elimina todas las piezas y muestra AGOTADO; heladera vacía no inventa unidades.
  - Management_HybridCartDragAddsOnlyOnDropAndCancelsSafely: cancelar/segundo dedo/drop agrega una sola unidad sin débito previo; preview con tamaño canónico, carrito $280, pago/persistencia de dos unidades, selección exacta de heladera y preparación. Los controles preparados conservan ambos footprints antes y después de arrastrar a la parrilla. La captura de referencia congela la simulación solo dentro del test y reubica las piezas en el centro tras cambiar el Canvas a cámara portrait, para no mostrar posiciones cacheadas del aspecto del Editor; no cambia código jugable.
- Anchos proyectados de referencia medidos:185.7153 /185.71286 y265.2119 /265.20; también pasan en viewport estrecho (185.7125 y265.1993).
-66,912 triángulos de comida,48 MeshRenderers activos, atlas/materiales compartidos. RenderTexture cap1200×1200/MSAA1. No representa FPS ni rendimiento físico medido.
- Primeras capturas detectaron texto invisible por ascenders y un error de calibración de tamaño. Se corrigieron antes del cierre, además de mover las piezas de borde hacia el interior y usar colisión de silueta.
- Capturas reales Unity1080×1920 inspeccionadas: mostrador inicial, carrito $280, detalle y referencia de parrilla en build/qa-counter-grill-signs/. Evidencia XML/log preservada en la misma carpeta.

## Límites
Solo cambios locales de proyecto. No APK, instalación, commit ni push nuevos en este pedido. Motorola conserva1.4.0/code6 anterior sin híbrido/changuito ni este diseño. QA táctil/performance Android y aprobación estética humana quedan pendientes.
