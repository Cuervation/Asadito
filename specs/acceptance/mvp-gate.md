# Gate de aceptación — MVP

## First Playable: nivel 1

- Al iniciar hay dos comensales con perfiles/preferencias y dos porciones predeterminadas: chorizo y tira de asado.
- Se puede encender/preparar carbón, mover brasas, observar calor por región, colocar/mover/voltear piezas, retirarlas a bandeja y servir cuando el jugador elija.
- El `HeatGrid` inicia en 8×6 y ofrece `Heat` y `EmberEnergy`; dimensión configurable.
- El carbón conserva energía al mover brasas entre celdas, decae al quemar y una zona movida cambia el calor muestreado; las celdas muestran visualmente la distribución.
- Cada pieza expone los campos de `FoodState` requeridos: temperaturas de centro/superficie, humedad, Maillard, `Char`, `FatRendered` y caras diferenciadas.
- El punto coincide con rangos configurables por alimento y deriva del estado, no de timer. `SimulationTimeScale` inicia en 30; DEBUG ofrece 20/25/30/35/40 en cualquier nivel.
- La asignación procura objetivos y preferencias con las prioridades de `ServingAllocator`; la evaluación incluye las cuatro dimensiones y muestra puntaje individual y total sobre 200.
- Retry restaura piezas, fuego/estado de tanda, bandeja, asignación y puntaje.
- La duración de 2–3 minutos es objetivo de tuning, no condición de victoria.
- El control DEBUG permite elegir 20×, 25×, 30×, 35× y 40× y persiste la selección; la sesión nueva inicia en 30× sin preferencia previa.
- En pantalla táctil se puede arrastrar una pieza por el mapa térmico, voltearla y soltarla en la bandeja; soltar fuera de la parrilla o bandeja devuelve la pieza a la parrilla.
- Menú principal e intro preceden la partida. El tutorial contextual avanza al detectar acciones, y luego se reduce tras completar el nivel 1 por primera vez.
- El flujo verificable es MAIN MENU → LEVEL INTRO → PRENDER/PREPARAR → MOVER BRASAS → COLOCAR CARNE → COCINAR → MOVER → VOLTEAR → RETIRAR → BANDEJA → SERVIR → EVALUAR → RESULTADO → RETRY. Incluye tutorial, estrellas/progreso y guardado local básico.

## Gate visual y audio

- Dirección 3D estilizada/semi-cartoon casual premium coherente en patio/parrilla/comida/UI, composición vertical y legible en teléfono; el key art de portada actual no alcanza aún este estilo y safe area/performance requieren mobile QA.
- Comida distingue estados crudo, calentando, dorando, punto objetivo, pasado y quemado; las caras mantienen progreso propio al voltear.
- Fuego comunica estados y variación regional del `HeatGrid`; feedback de colocar, mover, voltear, retirar, servir y reacción de comensales.
- Sizzle, humo y señal háptica (cuando la plataforma lo soporta y el setting está activo) acompañan interacciones sin tapar la lectura térmica.
- Resultado comunica score, estrellas y un motivo entendible. Audio básico de carbón/sizzle/metal y háptica optativa cuando sea viable.
- Concepts no cuentan como assets de runtime ni como verificación visual aprobada. Estados vigentes en [`../visual/`](../visual/art-direction.md) y manifests en [`../../docs/art/`](../../docs/art/asset-manifest.md).

## Evidencia de actualización visual (2026-09-28)

- Lilita One + wordmark propio, Baloo 2 cinco pesos estáticos (OFL/licencias) y launcher icon original ya están integrados; el runtime usa Unity Legacy Text, por eso no se generan TMP Font Assets. PlayMode verifica fuentes, glifos españoles (ñ, tildes, signos, ×, · y números), loading de logo/icono y gating de entrada durante animación.
- El full-cycle batch PlayMode L1–L5 pasa 6/6 y el EditMode 13/13; se prueban carga/intercambio del atlas de 16 comidas, 24 portraits/result expression, resultados y navegación. Esto no valida composición visual de GUI, contraste mínimo por device, input táctil físico ni build Android posterior al rediseño del icono.
- Mantener milestone Visual Slice / `MVP PARTIAL` hasta revisar Editor GUI, portada, safe areas, performance y arte en dispositivo.

## MVP completo: niveles 1–5

- Se pueden configurar cinco niveles progresivos según la tabla de [`mvp-scope.md`](../product/mvp-scope.md); las opciones DEBUG globales son 20, 25, 30, 35 y 40.
- Una sesión completa apunta a 5–7 minutos como objetivo de tuning.
- El modelo general de perfiles, fuego, alimentos, asignación y evaluación sirve a los cinco niveles sin fórmulas basadas en género.
- Menú → selección muestra título, comensales, menú, progreso/estrellas y bloquea los niveles aún no desbloqueados; cada nivel disponible abre una intro que corresponde a su configuración real.
- Cada nivel se puede jugar hasta servir una bandeja válida, evaluar asignaciones y mostrar resultados/estrellas; Retry resetea la tanda y Next abre el nivel que acaba de desbloquearse. El último nivel vuelve a selección.
- L1–L5 se prueban de punta a punta tanto con pruebas automatizadas PlayMode donde sea razonable como con una revisión táctil/responsive en dispositivo; no basta con confirmar que se construye la lista de porciones.
- El alcance excluye carnicería, economía, multijugador, caminar/navegación de NPC y cocciones diferentes de parrilla de carbón.

## Criterios de consistencia

- Duraciones y escalas son metas/configuración de ritmo, no condiciones de victoria ni timers de punto.
- Los rangos de doneness se calibran por alimento y deben ser distinguibles en juego; valores finales se ajustan mediante balance.
- Las cuatro dimensiones de score y prioridades del allocator son observables/reproducibles con iguales entradas.

## Evidencia automatizada (2026-09-28)

- PlayMode batch 6/6 y EditMode batch 13/13 en copia Unity temporal `/tmp/AsaditoValidation.QP0xpX`.
- Un L1 full-cycle (incluye UI, movimiento/flip, cocción por estado, bandeja, resultados/save y Retry) pasa a la escala default ×30 en 43.2s de acciones scripted; no incluye deliberación humana.
- Otra prueba completa cooking→serve→results/progression para L1–L5; usa ×1200 solo para acelerar la prueba y no certifica el ritmo normal ni el tacto de dispositivo.
- El MCP GUI del Editor original dejó de responder tras un warning WebSocket; no se verificó su consola/jerarquía/render posterior ni se probó teléfono.
