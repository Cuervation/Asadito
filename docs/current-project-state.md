# ASADITO — estado operativo actual

**2026-10-01.** Dirección canónica: [full-game](../specs/product/full-game.md). No usar informes históricos como instrucciones actuales. Baseline previo conservado en [historia 2026-09-30](ai/operational-state-20260930-history.md).

## Jugable
- Primer capítulo de 12 asados; **L1–6** disponibles secuencialmente según estrellas. Partida nueva empieza solo con L1; cards restantes Disabled/candado. L7–12 siguen bloqueados hasta implementar sus verticales. Se conservan unlock/stars/scores antiguos.
- L1–3: cocina simple/cantidad/gustos; L4 presenta monedas y bonus inicial único. L5–6: pedido → carnicería → compra chorizo/tira → heladera/inventario → selección → preparación → bandeja → parrilla → tabla → servicio → evaluación/recompensa/saldo persistido.
- Wallet real, stock por jornada, capacidad limitada, selección de cantidades, descarte/pérdida, reward único y Caja del Asador sin soft-lock. Reiniciar cocina conserva el mismo run; un reinicio de app permite reanudarlo desde su nivel sin recomprar. Solo abandono explícito cuenta carne preparada como pérdida.
- Resultado separa ASADOR/GESTIÓN/OPERACIÓN, general, estrellas, costo utilizado, desperdicio, ingresos, ganancia y saldo. Cocina/comensales conservan los cuatro componentes existentes.

## Contratos conservados
- Una escena portrait1080×1920, safe area, input directo; sin carbón/encendido/apagado ni botón PAUSA visible. VOLVER con flecha; pausa interna por app/Escape durante cocina.
- Todos los cortes colocados se cocinan simultáneamente por perfil individual; selección no pausa otros. Un solo lado, sin girar por tap. Fuente y tabla contienen huellas completas y apilan solo overflow; último emplatado al frente. Tap en tabla completa sirve.
- FoodCatalog18 alimentos y diez estados runtime por alimento; no se exponen los 18 desde el comienzo.

## Arquitectura/datos
- `ManagementConfig.json`: economía, capacidad, precios, stock, porciones, rewards, pesos y gates; `ChapterOneLevels.json`: pedidos. Catálogo de alimentos sigue siendo fuente de identidad/perfiles/arte.
- `ManagementState`/`ManagementService`/`Wallet`: reglas y transacciones independientes; `ManagementScreen`: vistas event-driven; AsaditoGame adapta preparación/resultados sin reescribir térmica.
- Save v4, migración aditiva desde v1–3. Balance/inventario/run/equipamiento/ciclos persistidos; progreso/settings retenidos.
- Cinco sprites de gestión conectados por referencias en `Resources/ManagementArt.asset`; 85 restantes fuera del runtime. Botones arcade existentes intactos.

## Preparado, no activado
- Frescura derivada/modelo probado; `EnableFreshness=false`, sin tiempo real/offline. Ciclo de conservación separado de jornada.
- Cotización de promos probada, sin ofertas activas aún. Freezer/upgrades/estaciones/eventos solo diseño y campos mínimos de save; no acciones falsas.
- Roadmap Verticales2–6 en [roadmap](roadmap.md); reglas por dominio enlazadas desde full-game.

## QA de esta entrega
- Validator de contenido PASS (18 perfiles,108 frames,12cards/layouts).
- Compilación/importación Unity6000.6.3f1 real; EditMode **39/39 PASS**; PlayMode general **13/13 PASS** (191.26s); flujo gestión/legibilidad/reinicio final **1/1 PASS** (32.70s), incluye compras/preparación/cocción/servicio/reload y prevención de texto truncado.
- Renders1080×1920 de planificación/carnicería/heladera/resultado revisados; corregidos canvas de comida encima de resultados y métricas de fuentes.
- Editor principal MCP continúa en `tests_running`/ping sin respuesta; QA en checkout temporal aislado sin cambiar Library del usuario. Evidencia y límites: [QA Vertical1](ai/management-vertical1-qa.md).
- Android ARM64/IL2CPP **Succeeded**; APK en `build/Asadito-management-vertical1-20261001.apk`, firma debug v2/metadata/ABI verificados. Hubo una recompilación incremental necesaria tras corregir reanudación del save. Sin teléfono ARM64 accesible por ADB (solo AVDx86_64); no instalación ni QA físico afirmados.

El release público requiere playtesting humano y QA táctil/performance ARM64. Esta vertical no equivale a toda la versión definitiva del juego.
