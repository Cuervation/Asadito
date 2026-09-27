# MVP status

## IMPLEMENTADO

- SDD liviano: alcance, sistemas, acceptance gate y dirección visual canónicos.
- Seis imágenes CONCEPT bajo `docs/art/concepts/`.
- Código del First Playable: carbón/brasas 8×6, drag, perfiles térmicos por alimento, cocción por zona/cara, bandeja, allocator, score, estrellas, save, menú/intro, tutorial, scale debug, feedback provisional y retry.
- Pruebas EditMode escritas para fuego/grilla, cocción/flip/punto (incluye perfiles por alimento), asignación, score, estrellas y progreso.

## PROVISIONAL

- La compilación aislada de runtime y scripts del juego pasó con Roslyn incluido en Unity 6000.6.3f1; falta confirmar importación/compilación dentro del Editor y Play Mode.
- Presentación Canvas 2D; arte/animación, humo y sizzle de runtime son provisionales.
- Balance térmico/score no se validó con playtest.

## NO IMPLEMENTADO

- Visual Slice de producción.
- Niveles 2–5 y configuraciones propias de los seis perfiles; vacío/provoleta con estados de cocción distintos.
- QA del acceptance gate, test de diversión y build Android.

## KNOWN ISSUES

- Unity MCP registra la instancia pero las operaciones de Editor fallan con `ping not answered`.
- Tests y playtest siguen pendientes; el push requiere completar el inicio de sesión de GitHub CLI.

## TESTS

- Suite EditMode escrita, no ejecutada.
- Compilación aislada de los scripts del First Playable: OK. Compilación/importación del Editor y tests: pendientes.
- Producto, workflow, setup Git/Engram y estado guardados en Engram; tres chunks exportados por Git Sync.

## BUILD

- Sin build Android generada.

## NEXT

- Restaurar la conexión del Editor; confirmar compilación, ejecutar la suite y probar el acceptance del nivel 1. Completar autenticación de GitHub CLI y subir commits locales. No iniciar Visual Slice hasta cerrar First Playable.
