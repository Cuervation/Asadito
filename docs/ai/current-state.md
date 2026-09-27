# Estado de Asadito

- **Milestone:** First Playable en integración, pendiente de compile y playtest.
- **Implementado en código:** HeatGrid/carbón, drag de brasas y alimentos, térmica por posición/cara, bandeja/servir, allocator/score/estrellas/save/retry, menú/intro, tutorial, debug scale, sizzle/haptic y pruebas EditMode.
- **Validación:** QA estático no encontró bloqueos evidentes. El último `Tundra build success` es anterior a esta integración y a los tests. Compilación actual, Console y ciclo de juego no verificados. MCP registra Asadito pero Unity no responde a comandos (`ping not answered`).
- **Contrato vigente:** cinco niveles, carbón/HeatGrid, cuatro alimentos, perfiles/allocator, cuatro factores de score, tutorial, progreso/guardado, arte/feedback. Ver specs canónicas.
- **Bloqueo operativo:** MCP Editor registra la instancia pero no entrega respuestas; compilar y operar el Editor siguen sin verificarse.
- **Repositorio:** `main` local inicializado con `origin` en Cuervation/Asadito; GitHub estaba vacío al comprobarlo. Pendiente verificar autenticación, primer push y validación de compilación.
- **Memoria:** Engram v2.2.1 instalado; Codex quedó configurado para Engram stdio MCP. Hay dos memorias iniciales y Git Sync local; las herramientas MCP estarán disponibles después de reiniciar Codex.
- **Siguientes 3 pasos:** reimportar/compilar y correr EditMode tests; playtestear nivel 1, balance/tiempo y cerrar fallos; recién después iniciar Visual Slice.
- **Specs canónicas:** [alcance](../../specs/product/mvp-scope.md), [sistemas](../../specs/product/systems/), [aceptación](../../specs/acceptance/mvp-gate.md), [arte](../../specs/visual/art-direction.md).
