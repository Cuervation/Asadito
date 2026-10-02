# Animaciones, audio y feedback

La implementación de hoy usa corutinas Canvas y sprites/runtime procedural, no sets de clips `Animator` ni clips específicos para cada corte. La matriz detallada de triggers, responsables, evidencias y límites está en [Animation Manifest](animation-manifest.md); la cobertura de cada alimento queda en [food catalog](../gameplay/food-catalog.md).

Gestión 2D agrega selección/pulso, preview de arrastre al carrito y traslado/devolución reversible de unidades de heladera; estos efectos pertenecen a `ManagementFoodView`, no a modelos/cámaras3D. Pruebas actuales se registran aparte en el estado operativo.

Hay feedback para interacción con comida (aparecer, seleccionar, levantar, arrastrar, soltar, voltear/cambiar cara, cocinar por sprite, llevar a tabla, servir), humo de cocción, transiciones/UI, reacciones de comensales, score y estrellas. Siete cues one-shot y un sizzle se sintetizan en memoria; no hay cue/animación de encendido. La háptica es condicional. Todo queda provisional hasta probar touch, legibilidad y mezcla en móvil real.
