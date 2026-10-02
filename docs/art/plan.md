# Estado visual — Asadito

> Dirección vigente 100 % 2D: [arte](../../specs/visual/art-direction.md), [gestión 2D](../architecture/management-2d.md) y [estado operativo](../current-project-state.md). Inventario/evidencia siguiente conserva la fase MVP anterior; sus tests/APK no validan la migración 2D actual.

El inventario vigente y sus límites están en [Visual Bible](../../specs/visual/art-direction.md), [asset manifest](asset-manifest.md), [animation manifest](animation-manifest.md) y [food catalog](../gameplay/food-catalog.md). `PROVISIONAL` significa conectado y verificable, pero sin aprobación artística/táctil final.

## Integrado en la fase MVP (histórico)

- Portada vertical semi-cartoon top-down, wordmark propio Lilita One, bajada `el sabor Argentino`, repasador con bandera integrada a la tela, CTA `ENTRAR`/`SALIR` centrados e iguales.
- Fondo de parrilla top-down en gameplay, retratos de seis invitados × cuatro expresiones y 10 iconos vectoriales de UI.
- Catálogo visual de 18 atlas RGBA, seis estados cada uno: 108 recortes dinámicos no-null, importados comprimidos para Android; cada estado térmico se proyecta durante cocción.
- Doce tarjetas ilustradas independientes; el selector conserva solo la etiqueta `Nivel N`, sin datos secundarios.
- Canvas procedural integrado para portada, navegación, botones, tap/drag/place/flip/retirar a tabla/servir, invitados, resultados y estrellas; pinza/tabla PNG ilustradas, parrilla uniformemente caliente; siete one-shots sintéticos y sizzle loop, sin ignition/embers.
- La especificación [animation manifest](animation-manifest.md) distingue con precisión motion real procedural compartido de lo que no existe: clips authored por corte o simulación visual física.

## Validación técnica de la fase MVP (histórica)

Unity 6000.6.3f1: EditMode 20/20 y PlayMode 10/10; las pruebas recorren los doce niveles, no solo L1–L5. `Tools/validate_food_content.py` confirma 18 perfiles distintos, 108 sprites, 12 tarjetas. APK de validación Android ARM64 IL2CPP 1.2.0/code3, min API26/target API36/portrait compilado e instalado en emulador Pixel 7a API36. L1, fuego y selección directa de chorizo vistos; captura `/tmp/Asadito-mvp-gameplay-selected.png`. Un swipe ADB final produjo ANR y no confirmó drop al plato; no se declara validado el drag en dispositivo. Arte, tacto, notch, perf/audio físicos todavía sin aprobación. Evidencia y límites en [current state](../ai/current-state.md), [auditoría](../ai/mvp-audit.md) y [Android release](../android-release.md).

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
