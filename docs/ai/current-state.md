# Estado de Asadito

- **Milestone:** First Playable en integración; compilación Unity y 9 tests EditMode pasan en copia temporal del proyecto. Pendientes validación en la instancia abierta y playtest.
- **Implementado en código:** HeatGrid/carbón, drag de brasas y alimentos, térmica por posición/cara con bandas por alimento, bandeja/servir, allocator/score/estrellas/save/retry, menú/intro, tutorial, debug scale, sizzle/haptic y pruebas EditMode.
- **Validación:** auditoría estática encontró y corrigió que chorizo heredara las bandas de tira. Unity 6000.6.3f1 compiló el proyecto y ejecutó los 9 tests EditMode con resultado Passed en una copia temporal aislada. La instancia abierta todavía no responde al ping del MCP; escena y consola siguen sin leerse. Falta playtest del loop.
- **Contrato vigente:** cinco niveles, carbón/HeatGrid, cuatro alimentos, perfiles/allocator, cuatro factores de score, tutorial, progreso/guardado, arte/feedback. Ver specs canónicas.
- **Visual:** Canvas 2D y sizzle sintético son provisionales; el Visual Slice aún no empezó. La auditoría marcó 3D, estados/animaciones y audio de carbón/metal como trabajo visual pendiente.
- **Repositorio:** contrato actualizado: repo oficial Cuervation/Asadito, `main`; commits pequeños y push normal autorizados, sin force push. `origin` ya apunta correctamente; el push sigue pendiente de completar inicio de sesión seguro de GitHub CLI.
- **Memoria:** Engram v2.2.1 instalado, integrado en la configuración de Codex y sincronizado en `.engram/`. La sesión actual requiere reinicio para cargar su MCP.
- **Siguientes 3 pasos:** reimportar/compilar y correr EditMode tests; playtestear nivel 1, balance/tiempo y cerrar fallos; recién después iniciar Visual Slice.
- **Specs canónicas:** [alcance](../../specs/product/mvp-scope.md), [sistemas](../../specs/product/systems/), [aceptación](../../specs/acceptance/mvp-gate.md), [arte](../../specs/visual/art-direction.md).
