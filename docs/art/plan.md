# Plan visual MVP — Asadito

Fuente de dirección/gates: [Visual Bible](../../specs/visual/art-direction.md), [estados de comida](../../specs/visual/food-states.md), [estados de fuego](../../specs/visual/fire-states.md) y [animación](../../specs/visual/animation.md). Concepto o asset integrado no equivale a `FINAL`: debe pasar import, inspección visual, responsive/táctil y device QA.

## Slice visual ya integrado

- Fuentes estáticas Lilita One y Baloo 2 400/500/600/700/800, con licencias OFL; Canvas runtime usa Unity Legacy Text, no TMP.
- Portada vertical atmosférica con logo propio Lilita One, botones `ENTRAR`/`SALIR`, estado de espera accesible durante la animación escalonada, botones redondeados/feedback, brillo ambiental y transición a selección.
- Icono adaptativo Android rediseñado como chorizo sobre parrilla; se conservó GUID y configuración de las seis densidades/12 capas.
- Parrilla cenital ilustrada de gameplay, 16 estados de comida por atlas y 24 retratos de seis identidades/cuatro expresiones; atlas activos en runtime.
- Level select, intro, HUD, resultados, score/estrellas/reacciones y navegación L1–L5 continúan sobre Canvas procedural; se verifican loops automáticos sin alterar sistemas de juego durante el polish visual.

## Plan restante (prioridad MVP)

1. Reconnectar el Editor MCP sin cerrar la instancia abierta; revisar portada/scene/console, encuadres y botones en layout portrait. El MCP HTTP reporta la sesión pero falla ping y consola (`Unity session not ready`).
2. Inspeccionar en GUI el nuevo key art (derivado del estilo de parrilla gameplay), wordmark/icono a tamaño real, CTA/SALIR, safe-area y crops Android; ajustar solo si el render lo requiere.
3. Revisión de pantallas: level cards con jerarquía/info legible, HUD sin tapar parrilla, pantalla intro, score/estrellas/feedback individual. Corregir layouts solo con evidencia de render, no reescribir gameplay.
4. Elevar atlas de comida de cuatro intercambios visuales a estados intermedios/por cara y ampliar fuego/brasa/tray/pinzas/VFX; conservar el modelo térmico/scoring y mostrar claramente su estado real.
5. Medir build/dispositivo (resoluciones, touch, rendimiento/transparencias, audio/haptics); completar arte/animaciones solo tras aprobar lectura y balance de feedback.

## Validación actual

Unity 6000.6.3f1, copia aislada del proyecto: PlayMode full L1–L5 6/6 Passed; prueba enfocada de logo/icono/fuentes/CTA y glifos españoles 1/1 Passed; EditMode 13/13 Passed (`/tmp/AsaditoEditTests.current.xml`). L1 ×30 dura ~43,8 s en interacción scripted, no en playtest humano. MCP del Editor original sigue no-ready, por lo que ningún resultado de batch se presenta como inspección visual en GUI/device. El APK de validación Android aislado compila el key art/icono nuevos (46 MB, AAPT adaptive resource, firma debug); `Builds/Android` no se reemplazó y falta launcher/touch QA real.

## Restricciones

- No crear niveles, escenas, GameObjects ni contenido gameplay fuera del MVP ni variar simulación, scoring, allocator, guests o progression por motivo visual.
- No copiar Pocket Chef ni juegos/artistas de referencia; usar solo el estándar de calidad/claridad casual mobile.
- No importar paquetes visuales grandes ni fuentes/familias adicionales sin justificación y licencia. Nunito no se instala salvo que legibilidad móvil demuestre que Baloo 2 no alcanza.
- Consultar [Asset Manifest](asset-manifest.md) y [Animation Manifest](animation-manifest.md); los estados no validados permanecen `PROVISIONAL`, `CONCEPT` o `TODO`.
