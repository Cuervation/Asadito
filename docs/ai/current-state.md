# Estado de Asadito

- **Milestone:** First Playable en integración; scripts de runtime compilan aislados con Roslyn de Unity. Pendientes importación/compilación del Editor, tests y playtest.
- **Implementado en código:** HeatGrid/carbón, drag de brasas y alimentos, térmica por posición/cara con bandas por alimento, bandeja/servir, allocator/score/estrellas/save/retry, menú/intro, tutorial, debug scale, sizzle/haptic y pruebas EditMode.
- **Validación:** auditoría estática encontró y corrigió que chorizo heredara las bandas de tira. Roslyn de Unity compiló correctamente los modelos runtime y scripts actuales del juego; las bandas nuevas y sus tests todavía no se ejecutaron. La importación/compilación del Editor no está confirmada. El MCP HTTP registra Asadito, pero llamadas al Editor vencen por timeout o sesión desconectada; escena y consola siguen sin leerse.
- **Contrato vigente:** cinco niveles, carbón/HeatGrid, cuatro alimentos, perfiles/allocator, cuatro factores de score, tutorial, progreso/guardado, arte/feedback. Ver specs canónicas.
- **Visual:** Canvas 2D y sizzle sintético son provisionales; el Visual Slice aún no empezó. La auditoría marcó 3D, estados/animaciones y audio de carbón/metal como trabajo visual pendiente.
- **Repositorio:** contrato actualizado: repo oficial Cuervation/Asadito, `main`; commits pequeños y push normal autorizados, sin force push. `origin` ya apunta correctamente; el push sigue pendiente de completar inicio de sesión seguro de GitHub CLI.
- **Memoria:** Engram v2.2.1 instalado, integrado en la configuración de Codex y sincronizado en `.engram/`. La sesión actual requiere reinicio para cargar su MCP.
- **Siguientes 3 pasos:** reimportar/compilar y correr EditMode tests; playtestear nivel 1, balance/tiempo y cerrar fallos; recién después iniciar Visual Slice.
- **Specs canónicas:** [alcance](../../specs/product/mvp-scope.md), [sistemas](../../specs/product/systems/), [aceptación](../../specs/acceptance/mvp-gate.md), [arte](../../specs/visual/art-direction.md).
