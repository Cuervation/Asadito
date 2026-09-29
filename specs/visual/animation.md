# Animación y feedback runtime

Motion es procedural y vive dentro de `AsaditoGame`, no hay secuencias authored por corte. Acciones y tiempos/responsables se inventarían en [`docs/art/animation-manifest.md`](../../docs/art/animation-manifest.md). Comida: apariciones/selección, lift/drag/release/place, flip con squash/arco/cambio de cara, transferencia a bandeja y servicio. Parrilla: entrada del fuego, transición de celdas, glow de brasas, brasas animadas y humo. UI/resultados: entrada/transiciones, botones con sfx, reacciones de invitados, score count, pop escalonado de estrellas. Audio: cues sintéticos más sizzle/haptics configurables.

Mantener efectos breves, aditivos al gameplay y legibles en mobile; no claim de VFX shader/clip individual si no existe. Estado actual PROVISIONAL; falta accesibilidad/touch perf en device.
