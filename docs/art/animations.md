# Animaciones, audio y feedback

La implementación de hoy usa corutinas Canvas y sprites/runtime procedural, no sets de clips `Animator` ni clips específicos para cada corte. La matriz detallada de triggers, responsables, evidencias y límites está en [Animation Manifest](animation-manifest.md); la cobertura de cada alimento queda en [food catalog](../gameplay/food-catalog.md).

Hay feedback para interacción con comida (aparecer, seleccionar, levantar, arrastrar, soltar, voltear/cambiar cara, cocinar por sprite, retirar/bandeja, servir), fuego/brasa/humo, transiciones/UI, reacciones de comensales, score y estrellas. Ocho cues one-shot y un sizzle se sintetizan en memoria; la háptica es condicional. Todo queda provisional hasta probar touch, legibilidad y mezcla en móvil real.
