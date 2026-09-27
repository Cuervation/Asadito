# MVP status

## IMPLEMENTADO

- SDD liviano: alcance, sistemas, acceptance gate y dirección visual canónicos.
- Seis imágenes CONCEPT bajo `docs/art/concepts/`.
- Código del First Playable: carbón/brasas 8×6, drag, perfiles térmicos por alimento, cocción por zona/cara, bandeja, allocator, score, estrellas, save, menú/intro, tutorial, scale debug, feedback provisional y retry.
- Pruebas EditMode escritas para fuego/grilla, cocción/flip/punto (incluye perfiles por alimento), asignación, score, estrellas y progreso. La puntuación por comensal está normalizada a 0–100 y el nivel 1 a 0–200.

## PROVISIONAL

- Canvas 2D, arte/animación, humo y sizzle de runtime son provisionales.
- Balance térmico y duración requieren otra sesión de playtest; tras corregir el score falta repetir un ciclo completo en el Editor.

## NO IMPLEMENTADO

- Visual Slice de producción.
- Niveles 2–5 jugables y configuraciones de los seis perfiles; vacío/provoleta con estados de cocción propios.
- Visual Slice de producción, QA restante del acceptance gate, test de diversión y build Android.

## KNOWN ISSUES

- GitHub CLI no está autenticado; el push sigue pendiente.

## TESTS

- Unity Editor abierto: compilación OK; EditMode 9/9 Passed.
- Playtest MCP: flujo del nivel 1 y retry completados. La primera corrida detectó total fuera de rango; el código se corrigió y el resultado actualizado dio 193/200 en una verificación rápida del evaluador. Falta repetir el ciclo completo en el código corregido.
- MCP: HTTP registrado en Codex, Editor conectado; escena `SampleScene`. Consola sin errores de juego; quedó un warning del WebSocket MCP (`WebSocket is not initialised`) tras refrescar assets.
- Producto, workflow, setup Git/Engram y estado guardados en Engram; tres chunks exportados por Git Sync.

## BUILD

- Sin build Android generada.

## NEXT

- Repetir el ciclo completo del nivel 1 con score corregido, validar arrastre táctil y seguir con Visual Slice. Completar login de GitHub CLI para subir los commits a Cuervation/Asadito.
