# MVP status

## IMPLEMENTADO

- SDD liviano: alcance, sistemas, acceptance gate y dirección visual canónicos.
- Seis imágenes CONCEPT bajo `docs/art/concepts/`.
- Código del First Playable: carbón/brasas 8×6, drag, perfiles térmicos por alimento, cocción por zona/cara, bandeja, allocator, score, estrellas, save, menú/intro, tutorial, scale debug, feedback provisional y retry.
- Pruebas EditMode escritas para fuego/grilla, cocción/flip/punto (incluye perfiles por alimento), asignación, score, estrellas y progreso.

## PROVISIONAL

- El loop actualizado aún no se importó/compiló en Unity ni pasó Play Mode.
- Presentación Canvas 2D; arte/animación, humo y sizzle de runtime son provisionales.
- Balance térmico/score no se validó con playtest.

## NO IMPLEMENTADO

- Visual Slice de producción.
- Niveles 2–5 y configuraciones propias de los seis perfiles; vacío/provoleta con estados de cocción distintos.
- QA del acceptance gate, test de diversión y build Android.

## KNOWN ISSUES

- Unity MCP registra la instancia pero las operaciones de Editor fallan con `ping not answered`.
- Compilación, tests y playtest siguen pendientes; el primer push espera autorización de GitHub CLI.

## TESTS

- Suite EditMode escrita, no ejecutada.
- Compilación actual no verificada; el último `Tundra build success` es anterior a la integración.
- Producto, workflow, setup Git/Engram y estado guardados en Engram; tres chunks exportados por Git Sync.

## BUILD

- Sin build Android generada.

## NEXT

- Restaurar la conexión del Editor; compilar, ejecutar la suite y probar el acceptance del nivel 1. No iniciar Visual Slice hasta cerrar First Playable.
