# Animación y feedback runtime

Motion es procedural y vive dentro de `AsaditoGame`, no hay secuencias authored por corte. Acciones y tiempos/responsables se inventarían en [`docs/art/animation-manifest.md`](../../docs/art/animation-manifest.md). Comida: aparece en bandeja de aluminio cruda, se arrastra a parrilla con lift/settle, flip con squash/arco/cambio de cara, se arrastra a la tabla con escala proporcional y se sirve con un toque; la pinza ilustrada queda fuera del MVP activo por ahora. No existe animación de encendido/celdas/brasas. UI/resultados: entrada/transiciones, botones con sfx, reacciones de invitados, score count, pop escalonado de estrellas. Audio: cues sintéticos de acciones de comida más sizzle/haptics configurables.

Mantener efectos breves, aditivos al gameplay y legibles en mobile; no claim de VFX shader/clip individual si no existe. Estado actual PROVISIONAL; falta accesibilidad/touch perf en device.
