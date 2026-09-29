# Estado visual — Asadito

El inventario vigente y sus límites están en [Visual Bible](../../specs/visual/art-direction.md), [asset manifest](asset-manifest.md), [animation manifest](animation-manifest.md) y [food catalog](../gameplay/food-catalog.md). `PROVISIONAL` significa conectado y verificable, pero sin aprobación artística/táctil final.

## Integrado

- Portada vertical semi-cartoon top-down, wordmark propio Lilita One, bajada `el sabor Argentino`, repasador con bandera integrada a la tela, CTA `ENTRAR`/`SALIR` centrados e iguales.
- Fondo de parrilla top-down en gameplay, retratos de seis invitados × cuatro expresiones y 11 iconos vectoriales de UI.
- Catálogo visual de 18 atlas RGBA, seis estados cada uno: 108 recortes dinámicos no-null, importados comprimidos para Android; cada estado térmico se proyecta durante cocción.
- Doce tarjetas ilustradas independientes; el selector conserva solo la etiqueta `Nivel N`, sin datos secundarios.
- Canvas procedural integrado para portada, navegación, botones, brasa/calor/humo, drag/place/flip/retirar/servir, invitados, resultados y estrellas; ocho one-shots sintéticos y sizzle loop.
- La especificación [animation manifest](animation-manifest.md) distingue con precisión motion real procedural compartido de lo que no existe: clips authored por corte o simulación visual física.

## Validación técnica disponible

Unity 6000.6.3f1: EditMode 18/18 y PlayMode 6/6; los tests recorren los doce niveles, no solo L1–L5. `Tools/validate_food_content.py` confirma 18 perfiles distintos, 108 sprites, 12 tarjetas. APK Android ARM64 IL2CPP (1.1.0, min API 26, target API 36, portrait) compilado, verificado e instalado/lanzado en emulador Pixel 7a API 36. La portada se capturó en ese emulador; no equivale a certificar tacto, notch, perf/audio físicos ni aprobación artística. Evidencia y limitaciones en [current state](../ai/current-state.md) y [Android release](../android-release.md).

## Próximas mejoras, sin bloquear el MVP técnico

1. Probar gestos y zonas táctiles con dedos en un teléfono y distintas relaciones de aspecto/safe areas.
2. Revisar legibilidad, contraste, crops y ritmo de animación con evaluación visual humana; todo arte sigue provisional.
3. Medir FPS, memoria, transparencias, audio y háptica en hardware físico de baja/media gama.
4. Incorporar una credencial Unity válida a GitHub Actions para ejecutar suites Unity de forma remota; el workflow ya separa validación estática de ese requisito.
5. Hacer balance/playtest humano de dificultad de 12 niveles y revisar firma productiva solo cuando exista autorización/material del owner.

## Límites de producción

- No imitar logos, sprites o personajes de juegos de referencia: se toma solo un lenguaje casual de lectura rápida.
- Assets propios/creados para Asadito y fuentes Lilita One/Baloo 2 con OFL incluida. VFX y audio son soluciones procedurales provisionales.
- Las seis fases térmicas son cambios discretos de sprite; cada lado guarda su estado, pero el flip anima un intercambio/arqueo 2D y no rota geometría 3D.
- No se genera ni publica una release firmada. `Builds/Android` no se reemplaza; el APK validado temporal vive fuera del repositorio.
