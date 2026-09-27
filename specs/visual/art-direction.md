# Dirección visual — Asadito

## Norte

Juego 3D estilizado semirrealista, cálido y apetitoso, con identidad de patio/quincho argentino. La comida debe sentirse reconocible y sabrosa, no infantil ni fotorealista costosa. Cámara fija en tres cuartos, composición vertical para móvil y parrilla siempre legible.

## Paleta y materiales

Carbón oscuro y hierro gastado para la parrilla; brasas naranja/rojo; madera cálida; vegetación y patio discretos; carne con rojos, dorados y tostados naturales; UI crema de alto contraste. Materiales simples con silueta y valores claros. La lectura del calor y punto tiene prioridad sobre detalle decorativo.

## Escena y rendimiento

URP móvil, cámara ortográfica o perspectiva suave fija, una luz principal, sombras limitadas y pocas partículas. Mantener UI Canvas para legibilidad mientras se migra gradualmente la escena de 2D a 3D. No exigir postprocesado, múltiples luces en tiempo real ni personajes animados complejos.

## Concepts

Las seis imágenes de [`../../docs/art/concepts/`](../../docs/art/concepts/) son referencias CONCEPT; no son assets integrados ni criterios de aprobación visual final. El [manifest de assets](../../docs/art/asset-manifest.md) registra estado y uso.

## DONE visual

Cada estado comunica acción y resultado a tamaño de teléfono; alimento y fuego se distinguen sin depender solo del color; UI respeta safe areas; la escena mantiene rendimiento móvil estable y unidad de estilo. Ver [estados](food-states.md), [fuego](fire-states.md), [animaciones](animation.md) y [gate](../acceptance/mvp-gate.md).
