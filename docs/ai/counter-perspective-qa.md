# Mostrador: distribución y perspectiva — QA 2026-10-01

## Alcance
Pedido local: acomodar la comida sobre el mostrador con perspectiva, sin columnas verticales iguales. Se conserva CounterV2, la textura cruda original de parrilla, los modelos/colliders reales, ocho instancias por SKU según stock, changuito, botones y transacciones existentes. No nuevos assets raster ni cambios en economía, inventario o cocción.

## Implementación
- Cámara de tienda perspectiva FOV60°, elevada e inclinada (~60°), roll5° acorde a la bandeja ilustrada. Heladera conserva cámara ortográfica y comportamiento previo.
- Rayos del viewport intersectan el plano horizontal y=0: todas las piezas reposan sobre una misma superficie, no sobre un plano paralelo a la cámara. Tres filas escalonadas 3–3–2 por SKU, separación, pendiente hacia la derecha y pequeñas variaciones de ángulo/tamaño. Las piezas traseras se ven menores por profundidad real.
- Layout recalculado tras resize/captura; escala uniforme adaptativa para pantallas angostas y límite para viewports anchos del Editor. Margen adicional a la derecha. Carteles billboard a profundidad fija, tamaño ajustado al FOV/aspecto para conservar su legibilidad.
- HomeScale por pieza: pulso y drag restauran su escala propia. Iniciar drag detiene el pulso anterior; no altera tamaño ni crea copias del modelo fuente.

## Evidencia y correcciones
Unity6000.6.3f1 en QA aislado /tmp/Asadito-concurrent-cooking-qa, sin cerrar/reimportar el Editor principal.
- Iteraciones focales detectaron margen superior demasiado justo, bounds superpuestos en el viewport ancho de Editor y margen derecho insuficiente en 780×1100. Se ajustaron composición, encuadre/tamaño adaptativo y margen antes del cierre. La comparación front/back usa crecimiento mínimo5% (perspectiva natural), no una ampliación artificial fija10%.
- Cierre **2/2 PlayMode PASS**, **7.928s**, /tmp/asadito-counter-perspective-closure.xml y .log:
  - Management_3DModelsReceivePhysicsTapsWithoutCards:16 centros físicos seleccionan exactamente su pieza; misma altura de apoyo, sin superposición de bounds proyectados, crecimiento delantero respecto del fondo; piezas enteras en viewport780×1100 y al restaurarlo; pulso recupera HomeScale, atlas/mallas/budget, botones y detalle siguen válidos.
  - Management_HybridCartDragAddsOnlyOnDropAndCancelsSafely: cancelar/fuera/otro dedo no agrega; drop al canasto agrega una vez; dos piezas por$280, pago/reload con dos unidades reales; drag/pulso sin cambios permanentes de escala.
- Capturas Unity reales1080×1920 inspeccionadas: build/qa-counter-perspective/asadito-counter-perspective-initial.png y asadito-counter-perspective-cart.png. Comida contenida en la superficie, separada, con diagonales/volumen/profundidad visibles; etiquetas y changuito legibles. Resultado XML conservado en la misma carpeta.
- Diff whitespace limpio; QA usa exactamente ManagementWorldView y tests actuales. No suite completa ni build por ser una corrección visual/input local.

## Límites
Sin APK/instalación/commit/push nuevos solicitados. Motorola conserva1.4.0/code6 anterior sin este rediseño híbrido. Capturas/pruebas de Editor no sustituyen aprobación estética humana ni validación táctil/performance Android; generar e instalar nueva APK cuando se solicite.
