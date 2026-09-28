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

- Dirección casual premium coherente en patio/parrilla/comida/UI, composición vertical y legible en teléfono. Menú, selección, intro, gameplay y resultados se revisaron en GameView portrait 540×960; la composición cabe y los CTA/iconos quedaron alineados. Safe area, contraste, touch y rendimiento aún requieren dispositivo real.
- Comida distingue estados crudo, calentando, dorando, punto objetivo, pasado y quemado; las caras mantienen progreso propio al voltear.
- Fuego comunica estados y variación regional del `HeatGrid`; feedback de colocar, mover, voltear, retirar, servir y reacción de comensales.
- Ignition, movimiento de brasas, grab/lift/release/contact de comida y transición de servir→evaluación tienen respuesta animada distinguible; no solo cambio instantáneo de valores/colores.
- Sizzle, humo y señal háptica (cuando la plataforma lo soporta y el setting está activo) acompañan interacciones sin tapar la lectura térmica.
- Resultado comunica score, estrellas y un motivo entendible. Audio básico de carbón/sizzle/metal y háptica optativa cuando sea viable.
- Level Select, Intro y Results muestran iconos propios de chorizo/tira/vacío/provoleta, y la UI requerida (estrellas, locks, flechas, calor, bandeja/servir/retry/next) no queda reducida a labels/Unicode o rectángulos Unity sin identidad.
- Concepts no cuentan como assets de runtime ni como verificación visual aprobada. Estados vigentes en [`../visual/`](../visual/art-direction.md) y manifests en [`../../docs/art/`](../../docs/art/asset-manifest.md).

## Evidencia de actualización visual (2026-09-28)

- Lilita One + wordmark propio, Baloo 2 cinco pesos estáticos (OFL/licencias), launcher icon, comida UI (atlas) y 11 marcas system/action procedurales ya están integrados; el runtime usa Unity Legacy Text, por eso no se generan TMP Font Assets. La portada actual (941×1672) se generó con el fondo cenital de gameplay como referencia estilística; GuestPortraitAtlas se restilizó manteniendo sus 24 slots. Ambos conservan GUID/dimensiones; PlayMode verifica fuentes, glifos, logo, carga de portada, atlas/expresiones de invitados, iconos y gating de entrada.
- Después de los fixes visuales y de ciclo, Unity Editor real pasó PlayMode 6/6 y EditMode 13/13. Build Android del commit `c0a2835`: 0 errores/1 warning no bloqueante de Debug Symbols/Diagnostics Data; APK 52.98 MB, package `com.DefaultCompany.Asadito`, firma Debug v2 verificada (`/tmp/Asadito-MVP-c0a2835.apk`). El SDK integrado provee `adb`, pero no hay teléfono conectado ni Emulator instalado. Scan GUID previo: 0 unresolved; batch validó 2 escenas/0 prefabs/14 componentes/0 scripts faltantes.
- Mantener `MVP PARTIAL`: faltan instalación y QA físico (safe area/touch/performance/launcher), audio/VFX/arte final, package ID y firma de distribución definitivos, y revisión final del Acceptance Gate.

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

- PlayMode del Editor actual 6/6 (job `7d2f3e02417249ffb3d29eedf5b949d0`) y EditMode 13/13 (job `afc695658d5845e99c6d85d49ee56ef1`).
- Un L1 full-cycle (incluye UI, movimiento/flip, cocción por estado, bandeja, resultados/save y Retry) pasa a la escala default ×30 en 44s de acciones scripted; no incluye deliberación humana.
- Otra prueba completa cooking→serve→results/progression para L1–L5; usa ×1200 solo para acelerar la prueba y no certifica el ritmo normal ni el tacto de dispositivo.
- El scan de assets detecta 0 GUIDs irresolubles tras limpiar campos URP obsoletos y Unity batch abre 2 escenas/0 prefabs con 0 scripts faltantes; esto no sustituye revisar referencias/render en GUI.
- El MCP enumera la instancia y responde a estado, escena y jerarquía; `read_console` identifica el warning de build no bloqueante. `adb devices -l` no enumera dispositivos; Android Emulator no está instalado. No se probó teléfono.
