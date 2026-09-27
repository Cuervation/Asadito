# MVP status

## IMPLEMENTADO

- SDD liviano: alcance, sistemas, acceptance gate y dirección visual canónicos.
- Seis imágenes CONCEPT bajo `docs/art/concepts/`.
- Código actualizado del First Playable: carbón/brasas 8×6, drag, cocción por zona/cara, bandeja, allocator, score, estrellas, save, menú/intro, tutorial, scale debug, feedback provisional y retry.
- Pruebas EditMode para fuego/grilla, cocción/flip/punto, asignación, score, estrellas y progreso.

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
- El proyecto está en proceso de bootstrap de GitHub; compilación, tests y playtest siguen pendientes.

## TESTS

- Suite EditMode escrita, no ejecutada.
- Compilación actual no verificada; el último `Tundra build success` es anterior a la integración.
- Contrato y estado del proyecto guardados en Engram y exportados por Git Sync.

## BUILD

- Sin build Android generada.

## NEXT

- Reimportar/compilar y ejecutar la suite y el acceptance del nivel 1. No avanzar milestone hasta cerrar ese gate.
