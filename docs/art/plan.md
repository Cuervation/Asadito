# Plan visual MVP — Asadito

Fuente de dirección/gates: [Visual Bible](../../specs/visual/art-direction.md), [estados de comida](../../specs/visual/food-states.md), [estados de fuego](../../specs/visual/fire-states.md) y [animación](../../specs/visual/animation.md). Concepto o asset integrado no equivale a `FINAL`: debe pasar import, inspección visual, responsive/táctil y device QA.

## Slice visual ya integrado

- Fuentes estáticas Lilita One y Baloo 2 400/500/600/700/800, con licencias OFL; Canvas runtime usa Unity Legacy Text, no TMP.
- Portada vertical atmosférica con parrilla cenital ortográfica, logo propio Lilita One, botones `ENTRAR`/`SALIR`, estado de espera accesible durante la animación escalonada, botones redondeados/feedback, brillo ambiental y transición a selección.
- Icono adaptativo Android rediseñado como chorizo sobre parrilla; se conservó GUID y configuración de las seis densidades/12 capas.
- Parrilla cenital ilustrada de gameplay, 16 estados de comida por atlas y 24 retratos de seis identidades/cuatro expresiones; atlas activos en runtime.
- Level select, intro, HUD, resultados, score/estrellas/reacciones y navegación L1–L5 continúan sobre Canvas procedural; se verifican loops automáticos sin alterar sistemas de juego durante el polish visual.

## Plan restante (prioridad MVP)

1. Reconnectar el Editor MCP sin cerrar la instancia abierta; revisar portada/scene/console, encuadres y botones en layout portrait. El MCP HTTP reporta la sesión pero falla ping y consola (`Unity session not ready`).
2. Inspeccionar en GUI el nuevo key art top-down ya revisado como PNG, wordmark/icono a tamaño real, CTA/SALIR, safe-area y crops Android; ajustar solo si el render lo requiere.
3. Revisión de pantallas: level cards con jerarquía/info legible, iconos propios de comidas/navegación/estrellas/locks, HUD sin tapar parrilla, intro, score y feedback individual. Corregir layouts solo con evidencia de render, no reescribir gameplay.
4. Elevar atlas de comida de cuatro intercambios visuales a estados intermedios/por cara y ampliar fuego/brasa/tray/pinzas/VFX; conservar el modelo térmico/scoring y mostrar claramente su estado real.
5. Medir build/dispositivo (resoluciones, touch, rendimiento/transparencias, audio/haptics); completar arte/animaciones solo tras aprobar lectura y balance de feedback.

## Validación actual

Unity 6000.6.3f1, copia aislada del proyecto: PlayMode full L1–L5 con portada y Renderer2D limpio 6/6 Passed (`/tmp/AsaditoPlayTests.final2.xml`); EditMode 13/13 (`/tmp/AsaditoEditTests.final.xml`); referencia scan: 2 escenas, 0 prefabs, 14 componentes, 0 missing scripts y 0 GUIDs irresolubles. L1 ×30 dura ~44 s scripted, no en playtest humano. MCP del Editor original sigue desconectado, por lo que batch no se presenta como inspección en GUI/device. Rebuild Android posterior a la portada/limpieza: 47 MB, 0 errores/1 warning, AAPT adaptive resource y firma Debug v2; falta launcher/touch QA real y no se reemplazó `Builds/Android`.

## Restricciones

- No crear niveles, escenas, GameObjects ni contenido gameplay fuera del MVP ni variar simulación, scoring, allocator, guests o progression por motivo visual.
- No copiar Pocket Chef ni juegos/artistas de referencia; usar solo el estándar de calidad/claridad casual mobile.
- No importar paquetes visuales grandes ni fuentes/familias adicionales sin justificación y licencia. Nunito no se instala salvo que legibilidad móvil demuestre que Baloo 2 no alcanza.
- Consultar [Asset Manifest](asset-manifest.md) y [Animation Manifest](animation-manifest.md); los estados permitidos son `FINAL`, `PROVISIONAL` y `BLOCKED`.
