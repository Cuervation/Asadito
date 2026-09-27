# MVP status

## IMPLEMENTADO

- SDD liviano: alcance, sistemas, acceptance gate y dirección visual canónicos.
- Seis imágenes CONCEPT bajo `docs/art/concepts/`.
- Código del First Playable: carbón/brasas 8×6, drag, perfiles térmicos por alimento, cocción por zona/cara, bandeja, allocator, score, estrellas, save, menú/intro, tutorial, scale debug, feedback provisional y retry.
- Pruebas EditMode escritas para fuego/grilla, cocción/flip/punto (incluye perfiles por alimento), asignación, score, estrellas y progreso.

## PROVISIONAL

- Compilación del proyecto y 9 tests EditMode pasan con Unity 6000.6.3f1 en una copia temporal; la instancia abierta y Play Mode aún requieren validación.
- Presentación Canvas 2D; arte/animación, humo y sizzle de runtime son provisionales.
- Balance térmico/score no se validó con playtest.

## NO IMPLEMENTADO

- Visual Slice de producción.
- Niveles 2–5 y configuraciones propias de los seis perfiles; vacío/provoleta con estados de cocción distintos.
- QA del acceptance gate, test de diversión y build Android.

## KNOWN ISSUES

- Unity MCP registra la instancia pero las operaciones de Editor fallan con `ping not answered`.
- Playtest sigue pendiente; el push requiere completar el inicio de sesión de GitHub CLI.

## TESTS

- Suite EditMode: 9/9 Passed en copia temporal (`/tmp/asadito-first-playable-validation-results.xml`).
- Compilación Unity del proyecto en la copia: OK; importación de la instancia abierta y prueba de juego en Editor: pendientes.
- Producto, workflow, setup Git/Engram y estado guardados en Engram; tres chunks exportados por Git Sync.

## BUILD

- Sin build Android generada.

## NEXT

- Restaurar la conexión del Editor; confirmar compilación, ejecutar la suite y probar el acceptance del nivel 1. Completar autenticación de GitHub CLI y subir commits locales. No iniciar Visual Slice hasta cerrar First Playable.
