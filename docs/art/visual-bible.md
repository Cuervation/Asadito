# Visual Bible de Asadito

La dirección canónica, paleta, UI, comida, mundo y rendimiento están en [`specs/visual/art-direction.md`](../../specs/visual/art-direction.md). El inventario de gestión/progresión listo para una fase futura está en [management-asset-manifest.md](management-asset-manifest.md); está preparado, **no conectado**. La biblioteca se ve en la [hoja de contacto](management-library-preview.png). El plan incremental auditable está en [plan.md](plan.md). Consultar también [estados de alimentos](../../specs/visual/food-states.md), [estados de fuego](../../specs/visual/fire-states.md), [animación](../../specs/visual/animation.md) y los manifests de [assets runtime](asset-manifest.md) y [animaciones](animation-manifest.md).

## Extensión visual de gestión (2026-10-01)

- La nueva biblioteca conserva el estilo cálido, cartoon estilizado y casual-premium; no reutiliza logos, personajes ni UI identificable de otros juegos.
- Carnicería en portrait; puestos, heladeras y estaciones utilizables en vista top-down donde comparten espacio con gameplay. Los textos, precios, cantidades y porcentajes son dinámicos, nunca baked en el PNG.
- El patio progresa mediante módulos sobre el **mismo fondo**, no mediante cinco escenarios independientes: parrilla, mesada, heladera, freezer, cruz/asador, disco, horno, mesa, hierbas y luces.
- Las imágenes nuevas están fuera de `Resources` y todas las referencias runtime permanecen intactas. La economía, carnicería, inventario, frescura, freezer, mejoras y eventos todavía no están implementados.
- Los elementos del atlas actual de comida se mantienen como fuente para productos/slots de heladera; no generar duplicados de cada alimento por tier o frescura.
- Reservar la revisión final de identidad del personaje de sorpresa/evento y escala real de UI para antes de conectar los assets.
