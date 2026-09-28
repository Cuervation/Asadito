# Estado del MVP de Asadito

## IMPLEMENTADO

- SDD liviano: alcance, sistemas, gate de aceptación y dirección visual canónicos.
- Catálogo de cinco niveles con menú de selección, intro dinámica y composición de botones/porciones variable; seis perfiles de comensal. L1–L5 navegan al gameplay en Editor sin servir.
- Modelos de fuego/cocción/asignación/evaluación; vacío lento y fases térmicas de provoleta; HUD térmico, movimiento, bandeja, servicio, score, persistencia local, Retry y Next implementados en código.
- Arte top-down de parrilla añadido como fondo y cargado en Play Mode; tipografías y assets existentes tienen rutas runtime.
- Pruebas EditMode: 13/13 pasaron en Unity Editor.

## PROVISIONAL

- El juego utiliza Canvas procedural 2D; el nuevo fondo de parrilla es fotográfico y no cumple el arte casual-premium estilizado de la dirección visual.
- Comida cambia principalmente por tintes/formas; no hay set completo de estados/caras. Vacío reutiliza un bife genérico; provoleta es geométrica. El avatar es único, no seis retratos.
- Humo, fuego, animaciones, sizzle sintético y háptica opcional están implementados de forma básica, no aprobados visualmente/sonoramente en device.
- Flujo de selección→intro→gameplay de L1–L5 observado; no se terminó cooking→serving→results→retry/next para cada nivel, ni QA táctil.

## BLOQUEADO / NO VERIFICADO

- Sin suite PlayMode efectiva: el resultado disponible de PlayMode fue 0 tests.
- Sin push: `gh` no tiene sesión GitHub autenticada; se requiere `gh auth login`.
- Sin prueba de dispositivo, safe areas, rendimiento ni legibilidad portrait final.
- Arte final, VFX/sonido dedicados, rediseño del icono ya conectado y revisión final de Acceptance Gate pendientes.

## TESTS / EDITOR

- Unity Editor `6000.6.3f1`, escena `Assets/Scenes/SampleScene.unity`.
- Compilación del proyecto: OK; última suite EditMode completa: 13 passed, 0 failed (`efa711f9e8304581b9bf40eb589c7841`).
- Runtime observado a través de MCP: nivel 1, 2, 3, 4 y 5; composiciones dinámicas 2/3/4/4/6 porciones según catálogo, L4 vacío, L5 provoleta.
- Unity MCP HTTP: una instancia conectada, `ready_for_tools=true` y estado no stale tras el build. La consola de la última compilación quedó con 1 warning Android no bloqueante (diagnostics data requiere Full/SymbolTable para stacktraces), 0 errores. Durante la primera build larga hubo warnings WebSocket MCP transitorios; las herramientas HTTP responden.

## BUILD / PUBLICACIÓN

- APK Android arm64 regenerado con icono adaptive en `Builds/Android/Asadito.apk` (aprox. 43 MB), ZIP/firma Android Debug válidos, 0 errores de build/1 warning. AAPT confirma recurso de icono; package ID `com.DefaultCompany.Asadito` es provisional. No se generó AAB ni se validó en dispositivo; APK local ignorado por Git.
- Commit `5078cb3` (`Integrate five-level MVP and Android build support`) está en `main` y el árbol de trabajo quedó limpio; todavía no se publicó en `origin` porque `gh` no tiene una sesión GitHub autenticada (`gh auth login`). Sin autenticación el remoto no anunció refs, así que no fue posible verificar si hay divergencia remota.

Ver [matriz integral](mvp-audit.md) para acceptance, evidencia y próximos bloqueantes.
