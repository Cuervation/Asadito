# Agents & Skills Context Audit

**Proyecto:** ASADITO
**Corte:** 2026-09-30
**Checkout auditado:** /Users/celestino/Asadito
**HEAD:** 9a73fd8 — feat(gameplay): size foods by grill footprint
**Alcance:** diagnóstico exclusivamente. No se modificó código, settings, AGENTS, skills ni specs. Esta es la única modificación de repo.

> Seguimiento: esta auditoría conserva el diagnóstico y recomendaciones tal como estaban al corte. Su implementación documental está registrada en [`context-optimization-result.md`](context-optimization-result.md); las secciones “pendiente aprobación” describen el estado de esa fecha, no el estado actual.

## 1. Resumen ejecutivo

**Alineación con contexto progresivo: parcial, ~6/10 (valoración cualitativa, no métrica de telemetría).** La base es buena: un AGENTS corto, tres skills concisos, SDD que dice no crear documentos para cambios pequeños, validación escalonada y cero agentes obligatorios. No hay una cadena inevitable Planner → Architect → Reviewer → Tester.

La brecha real no es que las instrucciones sean enormes. No hay router ni mapa de código; el checkout real está fuera del cwd declarado para esta tarea; cuatro instrucciones aún describen el producto anterior de cinco niveles/carbón/brasas mientras specs y runtime describen 12 niveles, 18 comidas, parrilla always-hot y tabla. Esto ocasiona redescubrimiento, dudas de jerarquía y riesgo de aplicar reglas erróneas.

**Resumen solicitado**
- AGENTS encontrados: **1**, en raíz.
- Agentes especializados: **0**.
- Skills locales: **3** (~674 tokens estimados en conjunto).
- Specs: **16 Markdown** (~5.36k tokens en conjunto).
- Fortaleza principal: skills pequeños y reglas de validación honestas/proporcionales; no hay pipeline multiagente forzado.
- Mayor desperdicio potencial de tokens: falta de mapa de rutas + UI procedural dentro de AsaditoGame.cs (2.364 líneas / 121 KB); leerlo completo para un color puede costar ~25–35k tokens; rg más snippet reduce el costo a decenas de líneas. El cwd fuera del repo agrega otro paso.
- Mayor causa de latencia: reasoning xhigh en config global si sigue efectivo en tareas simples; Unity PlayMode/build/import cuando aplica. No se midió telemetría real.
- Routing actual: informal, basado en prosa/semántica; no puede garantizar contexto progresivo.
- Skills: ahorran contexto cuando aplican, lo agregan cuando se activan fuera de dominio.

**Cinco recomendaciones de mayor impacto, sin implementar:** alinear AGENTS/README/skills con el juego actual; abrir el checkout correcto en Codex; documentar un mapa corto de archivos y routing simple/media/compleja; unificar el snapshot operativo; aclarar proporcionalidad de tests/Git/Push para cambios triviales.

## 2. Objetivo y método

Se midió conceptualmente cuánto necesita Codex antes de tocar el archivo correcto y si escala contexto según complejidad. Se inspeccionaron todos los AGENTS y skills del repo, specs/docs, C# y JSON representativos, assets, Unity config, Git, pipeline CI, herramientas y configuración global de Codex/MCP pertinente.

No se inspeccionaron databases/historial privado del app, no se midió uso real de tokens ni qué fragmentos autoinyecta el runtime. Las cifras son aproximaciones por texto, no conteos del tokenizer. No se asumió que la documentación era correcta sin mirar settings/código/rutas. No se ejecutaron tests/build porque el alcance es documental y los resultados recientes están explícitamente fechados.

## 3. Estado actual del repositorio

| Dato | Estado verificado |
|---|---|
| Git root | /Users/celestino/Asadito |
| Cwd entregado a la tarea | /Users/celestino/Documents/ChatGPT/Asadito; no contiene repo Git (git rev-parse falla) |
| Branch/status | main, limpio antes de crear reporte; tracking local de origin/main |
| HEAD | 9a73fd8 del 30-09-2026; commits previos relevantes eb9e80e, 668c7e8, df081bc |
| Editor | Unity 6000.6.3f1 |
| Escena build | Assets/Scenes/SampleScene.unity; Settings habilita solo esa escena |
| Datos de contenido | JSON contiene 18 FoodIds, Resources/Art/Foods contiene 18 PNG atlas, MvpLevelCatalog define 12 niveles |
| Juego | Canvas/runtime procedural, juego cenital, comida/pinza/tabla; modelos separados para catálogo, calor, cocción, scoring y serving |
| Android config | portrait, package com.cuervation.asadito, v1.2.0/code3, min API26, target API36, ARM64 |
| Tests | 21 EditMode y 10 PlayMode declaradas |
| Último estado de test escrito | EditMode 21/21 + validator PASS; PlayMode de huellas detenido 8/10 por editor_unfocused |
| PASS anterior | PlayMode 10/10 (486.94 s) es del rediseño anterior y no verifica huellas actuales |
| APK en repo | Builds/Android/Asadito.apk existe, ignorado, 44,605,264 bytes, 27-09; no representa HEAD actual |
| Build actual | No verificado; docs dicen que no hubo rebuild para cambio de huellas |
| Release | Firma productiva/publicación fuera de scope; QA físico/táctil pendiente según docs |

La suite y validator citados provienen de docs/ai/current-state.md y mvp-status.md; no se relanzaron en esta auditoría.

## 4. Inventario

Ámbito: configuración específica de ASADITO más configuración global de Codex que puede afectar routing. No se cuentan como skills del repo todos los plugins genéricos de la app. Se excluyeron Library, Temp, Logs, caches, binarios y generados.

| Área | Cantidad/hallazgo | Cuándo entra | Tamaño/costo aproximado | Valor |
|---|---|---|---:|---|
| AGENTS | 1 root, ningún AGENTS por subcarpeta | Al operar sobre checkout correcto | 2.4 KB, ~466 tokens | Barato, parcialmente stale |
| Agentes | 0 prompts/roles/personas | Nunca desde el repo | 0 | No existe cadena |
| Skills | 3 en .agents/skills | Según coincidencia con description; comportamiento real del host no observable | 3.7 KB, ~674 tokens | Bajos si relevantes |
| Specs | 16 Markdown | Cambio de producto/sistema/visual/aceptación | 28.9 KB, ~5.36k tokens todo | Mayormente compactas |
| Docs | 16 Markdown | Estado/arquitectura/manifests/procedimientos | 62.5 KB, ~10.50k tokens todo | Útiles, status duplicado |
| Ref visual | 6 PNG en docs/art/concepts | Inspección visual explícita | No tokenizados aquí | Concepto/no runtime |
| Router/prompts | No hay .codex en repo, router, plantilla de prompt u orquestador | No aplica | 0 | Falta de navegación |
| Automatización | 1 workflow CI, 2 herramientas en Tools | CI, validator, conectar Unity | Validator 343 líneas/~2.84k tokens si se leyera entero; launcher ~73 | Validación, no routing |
| Unity MCP | Paquete embebido com.coplaydev.unity-mcp 10.2.0, wrapper documentado, URL localhost en config global | Editor/operaciones Unity | 48 herramientas expuestas en esta sesión | Reduce control manual, latencia variable |
| Engram | MCP global y .engram manifest/chunks versionados | Búsqueda selectiva de memoria | prompt global ~880 tokens + compact ~96 estimados | Evita redescubrimiento |
| Config Codex | ~/.codex/config.toml, no versionado | Modelo, reasoning, MCP/plugins globales | 2.5 KB de TOML; no es todo prompt | Model gpt-6-luna/reasoning xhigh declarados |

Los archivos .agents/skills/*/agents/openai.yaml son metadatos de UI/display del skill, no prompts de agentes.

## 5. Agents

**Cero agentes especializados encontrados.** No hay Planner, Architect, Gameplay, Visual, QA, Reviewer, Tester ni Orchestrator como archivos/roles declarados. Ninguna definición describe propósito, inputs/outputs, invocador, otras llamadas, skills, specs o rutas típicas.

El SDD usa Product/SDD, Unity Gameplay, Visual/Animation y QA como categorías de trabajo. También menciona niveles A/B/C, sin indicar modelos concretos. Nada de eso crea agentes que el sistema pueda invocar. Los YAML junto a skills son display metadata.

- ¿Hace falta que existan agentes para cambios simples? No; cero overhead actual es una fortaleza.
- ¿Se duplican entre ellos? No existen.
- ¿Podrían ser skills? Las categorías actuales ya son instrucción textual dentro del skill SDD.
- ¿Podrían eliminarse? No aplica.
- ¿Riesgo de invocación para trivial? No hay invocación codificada; solo una posible decisión del agente principal.
- ¿Autonomía? El agente principal puede editar/probar sin agentes; delegar solo cuando subtareas independientes ahorran contexto/riesgo.

La mejora es un router corto, no incorporar una plantilla de seis agentes.

## 6. Skills

Estimación de tokens: palabras × 1.35. Cada skill es condicional; el repo no contiene telemetría para demostrar cuándo el runtime los carga.

| Skill/ruta | Propósito, inputs, referencias | Activación apropiada | Tamaño | Clasificación |
|---|---|---|---:|---|
| asadito-sdd, .agents/skills/asadito-sdd/SKILL.md | SDD ligero: specs, acceptance, scope, gameplay; apunta a mvp-scope, systems, mvp-gate y Visual Bible | Cambio de regla/scope/gameplay significativo; no color definido | ~266 tokens | **ÚTIL**, pero con regla obsoleta de cinco niveles/carbón/brasas |
| asadito-unity-verify, .agents/skills/asadito-unity-verify/SKILL.md | Inspeccionar Editor/instancia/escena/hierarchy/Console; PlayMode solo acceptance afectada; remite a setup/gate | Cuando compilación, integración o prueba Unity aporta evidencia | ~232 | **ÚTIL**; caro si se usa para cambio sin efecto Unity |
| asadito-visual-mvp, .agents/skills/asadito-visual-mvp/SKILL.md | Arte nuevo/reemplazado, assets status, import Unity, lectura mobile; remite a specs/visual y asset-manifest | Arte/feedback visual real | ~176, más Visual Bible ~1.93k y asset manifest ~1.44k | **ÚTIL** para arte; **COSTOSO** si se activa en UI trivial. Dice embers/tray obsoletos |
| agents/openai.yaml de cada skill | Display name y short description | Solo metadata | 2 líneas aprox. | No es agente ni skill adicional |

No existe un skill propio de UI, Unity Gameplay, animación exclusivo, cocción/debug, Android o Testing. El skill visual puede orientar nuevos sprites, pero no mapea botones a un archivo. SDD acota el uso de documentos para cambios pequeños, aunque su activación no es un dispatcher.

**¿Ahorran más tokens que cuestan?** Sí en el dominio correspondiente: prevenir una regla de gameplay incorrecta o un arte desconectado vale más que 200 tokens. No en cualquier tarea. Para una propiedad visual pequeña, una activación semántica amplia y lectura de todo el art-manifest reduce el neto.

## 7. Specs e instrucciones

| Ruta | Tipo/función | Tamaño aproximado |
|---|---|---:|
| specs/product/mvp-scope.md | Fuente scope actual: MVP 12 niveles/18 comidas, reglas/exclusiones | ~871 tokens |
| specs/acceptance/mvp-gate.md | Gate actual | ~329 |
| specs/product/systems/fire-heat.md | Calor uniforme always-hot | ~285 |
| specs/product/systems/food-cooking.md | Modelo, perfiles térmicos, estados, huella | ~441 |
| specs/product/systems/guest-evaluation.md | Guest profiles y score | ~144 |
| specs/product/systems/serving.md | Servicio/asignación | ~131 |
| specs/product/systems/scoring.md | Score y criterios de aceptación | ~186 |
| specs/product/scoring.md | Descripción de puntuación/allocator | ~151; se solapa con systems/scoring |
| specs/product/cooking.md | Alias corto al sistema térmico y JSON | ~80 |
| specs/visual/art-direction.md | Visual Bible: estilo, UI, arte, mobile/performance | ~1,928, mayor spec |
| specs/visual/food-states.md | 6 estados térmicos/porciones | ~317 |
| specs/visual/fire-states.md | Parrilla visual sin carbón/heatmap | ~131 |
| specs/visual/animation.md | Movimiento, audio y VFX runtime | ~140 |
| specs/product/mvp.md | Redirect histórico al scope actual | ~116 |
| specs/product/gameplay-loop.md | Redirect histórico | ~59 |
| specs/acceptance/mvp.md | Redirect histórico al gate vigente | ~47 |

Total 16, 28.9 KB, ~5.36k tokens si se cargan todos. El conjunto no es gigante; el costo depende de seleccionar mal. Tres referencias históricas son muy cortas y están marcadas; product/scoring y systems/scoring se superponen.

**Otras instrucciones/documentación de entrada:** README.md (~134 tokens estimados) enlaza scope, gate, Visual Bible, status y AGENTS; UNITY_MCP_SETUP.md (~204) explica paquete/compatibilidad.

## 8. Jerarquía actual

Reconstrucción basada en archivos (la prioridad interna del host no puede probarse con el repo):

1. Políticas globales Codex + prompt actual del usuario.
2. AGENTS.md raíz si cwd se encuentra en el checkout.
3. Usuario decide; para regla de producto, specs/product + systems; aceptación en specs/acceptance/mvp-gate; estilo en specs/visual.
4. Skill aplicable como guía.
5. docs/ai, docs/architecture, manifests y README como resumen/procedimiento.
6. Código/data/assets/tests/settings como verdad del comportamiento ejecutable.

AGENTS señala specs como fuente de verdad. El skill SDD dice user > specs > acceptance/código. Es razonable solo si ambas están sincronizadas; hoy no. No existe regla local por subdirectorio.

**Always:** reglas globales de plataforma; AGENTS raíz si Codex realmente trabaja en repo.
**Condicional:** skill, spec de subsistema, manifests, Unity MCP, test/build.
**No debería ocurrir:** leer todas las specs y docs por defecto.

Cwd constatado en esta tarea es una carpeta sin Git y el checkout vive en otro path; puede impedir que los artefactos locales se descubran automáticamente. Esto es específico del entorno de trabajo auditado.

## 9. Arquitectura actual

Flujo deducible:

    Prompt
      ↓
    Config/runtime global (modelo, Engram, MCP)
      ↓
    Workspace actual (aquí, no es Git root)
      ↓
    AGENTS si se abre el repo real
      ↓
    Coincidencia informal con skills
      ↓
    Lectura manual de spec y búsqueda en repo
      ↓
    Implementación por un agente principal
      ↓
    Validator/tests/Unity MCP según criterio
      ↓
    Política Git pide commit/push de cambios validados

No se encontró el flujo Planner → Architect → Specialist → Reviewer → Tester. No hay script que reciba el prompt, elija skill, abra spec ni seleccione una suite.

## 10. Routing actual

Las descripciones de skill permiten coincidencia aproximada; no hay tabla de activación/paths. Simulación por la estructura real, no invocaciones observadas:

| Tarea de ejemplo | Skills/specs que convienen | Paths/áreas a explorar | ¿Eficiente? |
|---|---|---|---|
| A. Cambiar color de botón | Sin skill por defecto; subapartado de paleta si precisa | búsqueda label/MakeButton; AsaditoGame.cs, feedback script solo si pressed state | Puede ser 1 archivo/una búsqueda |
| B. Mover tabla al lado de parrilla | Visual; SDD solo si cambia regla | Visual Bible + asset-manifest + layout en AsaditoGame y Mesita/Tabla | Media, necesita preview |
| C. Sacar botón Servir | No skill si solo se verifica presencia; serving spec si cambia sistema | buscar label en AsaditoGame/ServingBoardTouch | En HEAD ya no existe botón SERVIR |
| D. Doble toque sirve carne | SDD + Unity Verify | systems/serving, ServingBoardTouch.cs, callback AsaditoGame, test PlayMode | Feature táctil media |
| E. Agregar corte | SDD + Visual | scope/food-cooking/food-states, JSON, modelos, atlas, nivel, tests/validator | Compleja data-driven |
| F. Nueva imagen de corte | Visual + Verify al importar | Visual Bible, asset-manifest, Resources/Art/Foods/import settings | Media de arte |
| G. Crear animación | Visual + Verify | animation spec/manifest, corutinas de AsaditoGame, scripts touch | Media |
| H. Bug complejo de cocción | SDD + Unity Verify | fire-heat/food-cooking, FoodCookingModel, FoodState, JSON, Edit/Play tests | Compleja |
| I. Build Android | Unity tooling/Verify; no skill Android local | android-release, AndroidReleaseBuild, ProjectSettings, Editor/build | Lento, alcance claro |
| J. Correr tests | Unity Verify + gate pertinente | asmdef, test files y validator | Suite/filtro según pedido |

## 11. Simulación de tarea trivial

Pedido: “Cambiá el color del botón X”.

**Ruta mínima:** repo correcto → AGENTS (~466 tokens) → rg del label/color → abrir rango local en AsaditoGame.cs → cambiar valor → diff/compilación o preview si contraste/layout cambia.

- Cero agente.
- Ningún skill por defecto. SDD no hace falta para una regla ya definida; Visual solo si cambia el sistema visual.
- Leer solo un fragmento de Visual Bible si el color requiere norma de paleta.
- No cargar food catalog, progresión, scoring, todos los docs ni gate de aceptación.
- No correr 10 PlayMode tests, build Android ni modificar docs.
- AGENTS actualmente dice “consult specs/current-state docs first”; si se interpreta amplio, suma ~1.29k tokens de current-state aunque no cambie el estado.
- La instrucción Git sugiere commit/push a main para cada unidad validada, sin excepción trivial.

AsaditoGame.cs mide 121 KB. Cargarlo entero puede aproximarse a 25–35k tokens; búsqueda dirigida + 10–30 líneas evita la mayor parte. El repo no fuerza que se cargue entero, por eso es costo potencial, no observado.

**Flujo actual posible (interpretación amplia):** ubicar repo → AGENTS + current-state/specs sin límite → skill visual por coincidencia → abrir monolito → cambio → Editor/tests completos → commit/push.

**Flujo ideal:** AGENTS una vez → localizar por label/ruta → un snippet → cambio → check proporcional → terminar. No Planner, agentes, docs, PlayMode ni push.

## 12. Simulación de tarea mediana

Pedido: mover la mesa visualmente al costado de la parrilla.

Cargar AGENTS, skill Visual; SDD solo si cambia el comportamiento. Leer la sección de composición de art-direction y asset-manifest, buscar anclajes en AsaditoGame y props existentes. Editor Game View para verificar proporción/click targets; PlayMode de serving/drag solo si el rect interactivo cambia. No leer cooking, scoring, todo el catálogo ni Android. Contexto probable 2–4k si se cargan esas referencias enteras.

## 13. Simulación de tarea compleja

Pedido: corregir un bug de cocción o agregar alimento/atlas.

AGENTS + SDD + Verify; Visual para nuevo sprite. Leer spec térmica/visual/acceptance afectada, no todo el SDD. Conectar JSON, FoodCatalog/food profile, FoodCookingModel, FoodSpriteLibrary, MvpLevelCatalog, atlas PNG, tests y validator. EditMode primero; PlayMode si cambia selección/flip/serving/progresión. Unity MCP si hace falta escena/Console; Android build solo si cambia player/config o acceptance lo requiere.

Delegar solo partes independientes cuyo contexto no se duplique; no lanzar secuencia fija de planner/architect/reviewer/tester. Si editor_unfocused, la prueba no es PASS. En complejo sí se justifica la lectura progresiva.

## 14. Estimación de contexto

| Elemento | Tokens si se lee todo | Entrada | Latencia/relectura | Valor |
|---|---:|---|---|---|
| Instrucción global Engram | ~880 + compact ~96 | Baja/media | Roundtrip; relectura baja | Media/alta |
| AGENTS raíz | ~466 | Muy baja | Baja | Alta si actual |
| Skill local | ~176–266 | Muy baja | Baja | Alta en su dominio |
| Spec de sistema | ~130–441 | Baja | Media por aliases | Alta según cambio |
| MVP scope | ~871 | Baja | Media | Alta para contenido/scope |
| Visual Bible | ~1.93k | Baja | Media | Alta en arte, no térmica |
| Asset manifest | ~1.44k | Baja | Media | Alta para asset integration |
| Current-state | ~1.29k | Baja | Alta con otros status docs | Media |
| Todas specs | ~5.36k | Media | Alta | Baja en trivial |
| Todos docs Markdown | ~10.50k | Media | Alta | Útil solo auditoría amplia |
| Docs + specs + README/setup | ~17.33k | Media/alta | Alta | No cargar por defecto |
| AsaditoGame completo | ~25–35k aprox. | Media/alta | Media | Mucho irrelevante para un color |
| PlayMode/build IL2CPP | Poco token, mucha duración | Alta/muy alta | Variable | Solo por riesgo/alcance |

## 15. Estimación de tokens

Método: palabras × 1.35. Total texto visible en docs/specs/entrada, no tokenizador exacto:

| Grupo | Archivos | Palabras | Tokens aprox. |
|---|---:|---:|---:|
| AGENTS | 1 | 345 | 466 |
| Skills | 3 | 499 | 674 |
| Specs | 16 | 3,969 | 5,358 |
| Docs Markdown | 16 | 7,774 | 10,495 |
| README + MCP setup | 2 | 250 | 338 |
| **Total** | **38** | **12,837** | **17,331** |

Seis PNG de concepto no están incluidos. La configuración global apunta a instrucción Engram ~880 tokens y compact prompt ~96, pero el runtime puede variar inyección/override. No son parte del repo. Sistema/developer/prompt/historial tampoco se incluyen.

El target “decenas o cientos de tokens antes de tocar el archivo” no está garantizado por configuración global, pero tampoco se carga automáticamente todo el repo según archivos encontrados. La mayor variable evitable es cuánto archivo/estado amplio se decide abrir.

## 16. Análisis de velocidad

| Fuente | Impacto | Evidencia |
|---|---|---|
| Razonamiento | Medio/alto para trivial si xhigh efectivo | config global lo establece; override actual no visible |
| Lectura de archivos | Bajo con rg/snippets; muy alto si abre monolito | AsaditoGame 121 KB |
| Búsqueda de repo | Medio/alto | Sin mapa y cwd fuera del root |
| AGENTS | Poco costo, impacto semántico alto | Scope antiguo |
| Skills | Bajo costo textual | References visuales pueden sumar ~3.4k |
| Specs | Bajo si una; alto si todas | ~5.36k total |
| Subagentes | Ninguno obligatorio | No hay definiciones/cadena |
| Tests | Medio/alto en PlayMode | 10 tests, última suite actual no terminó |
| Unity compile/import | Medio | Editor/asset import no medido aquí |
| Android | Alto | IL2CPP; build anterior documentada ~61.7s incremental |
| Generación visual | Alto variable | generación/import/render/legibilidad móvil |
| MCP externo | Variable | requiere Editor/puente; 48 tools expuestas |
| Git | Medio en trivial | fetch/status/commit/push puede añadir ciclo externo |
| Retries | Alto | PlayMode se frenó por pérdida de foco |
| Duplicación | Bajo/medio | tres snapshots y score specs |
| Engram | Bajo roundtrip | consulta/sync, contexto selectivo útil |

## 17. Relecturas y duplicación

- docs/ai/current-state.md (~1.29k), mvp-status.md (~1.01k) y mvp-audit.md (~1.34k) vuelven a exponer loop, contenido, tests, Android y QA con distintos horizontes. Las fechas/secciones identifican actual vs previo; no atribuir 10/10 anterior al HEAD.
- specs/product/scoring.md y systems/scoring.md se solapan; mvp-scope también resume los pesos.
- docs/gameplay/loop.md apunta a gameplay-loop, que redirige a mvp-scope/serving/gate.
- docs/art/visual-bible.md y animations.md son índices pequeños, aportan links y no son duplicación pesada.
- README abre el proyecto pero una oración de alcance es vieja.
- Los seis PNG conceptuales no se cargan salvo búsqueda/inspección visual.

## 18. Sobreingeniería

No hay pipeline de seis agentes. Sí hay ambigüedad de proceso: AGENTS recomienda consultar specs y current-state al iniciar, SDD prescribe spec→acceptance→Unity para feature significativa, y Git manda crear/pushear cambios validados sin excepción de scope. Una petición de color puede interpretarse como leer demasiado y publicar.

Simplificar: tarea trivial = edit directo + diff; spec solo si cambia regla; tests según riesgo; no delegación/build/commit/push salvo pedido o flujo realmente aprobado. En tarea grande, usar cadena mínima no serial de roles.

## 19. Información obsoleta

| Ruta | Qué dice | Efecto |
|---|---|---|
| AGENTS.md líneas 6–7 | Visual Slice, no abrir MVP Content, L2–L5 pendientes, máximo cinco niveles | Puede bloquear scope ya implementado |
| .agents/skills/asadito-sdd/SKILL.md línea 12 | First Playable de cinco niveles, carbón, preparación de fuego, movimiento de brasas | Contradice juego actual |
| .agents/skills/asadito-visual-mvp/SKILL.md línea 12 | embers y tray como interacciones core | Guía arte a affordances retiradas |
| README.md líneas 3 y 17 | objetivo actual validar L1 antes de ampliar contenido | Falso como estado actual |

Las rutas de los tres skills existen. Los redirects marcados históricos son apropiados, no instrucciones activas. No se borraron archivos.

## 20. Contradicciones

- Root AGENTS/SDD: cinco niveles, charcoal/embers, todavía no abrir contenido.
- Scope/runtime actual: 12 niveles y 18 alimentos, siempre caliente, interacción directa con carne/pinza y tabla, sin carbón/brasas.
- Visual skill conserva embers/tray.
- mvp-gate admite “arrastre o acción universal a tabla”; el flujo/documentación actual es arrastrar a tabla y doble tap sobre ella. La expresión “acción universal” podría confundirse con botón que el rediseño eliminó.
- Las pruebas y builds sí diferencian actualización actual vs anterior, aunque requieren leer la sección fechada correcta.

## 21. Fuentes de verdad

| Tema | Regla normativa | Implementación/estado |
|---|---|---|
| Scope/progresión | specs/product/mvp-scope.md | MvpLevelCatalog + JSON |
| Cocción/calor | systems/fire-heat.md + food-cooking.md | GrillHeatModel, FoodCookingModel, FoodCatalog.json |
| Serving/scoring/guests | serving/scoring/guest-evaluation | runtime + tests |
| Visual | specs/visual/art-direction.md + states | Resources/Art, docs/art manifests |
| Acceptance | specs/acceptance/mvp-gate.md | Tests + validator |
| Arquitectura | docs/architecture/mvp.md como resumen | C# + SampleScene |
| Git | AGENTS.md, con excepciones de autorización | remote/status local |
| Android | docs/android-release + settings actuales | ProjectSettings/AndroidReleaseBuild/artifact fechado |
| Estado | un snapshot actual y corto por definir | resultados con fecha/SHA |
| Routing | no hay fuente canónica | hoy texto del SDD |

## 22. Qué está bien

- No hay agentes obligatorios: reduce latencia/contexto.
- AGENTS tiene ~466 tokens; no es una instrucción larga.
- Skills combinados ~674 tokens y especializados.
- SDD aclara que cambios pequeños ya definidos no requieren nuevos documentos.
- Verify exige no mentir sobre compile/PlayMode y limita pruebas a acceptance tocada.
- Visual Bible fija estilo móvil/originalidad y evita copiar juegos de referencia.
- Catálogo y niveles centralizados; tests nombran dominios.
- CI siempre valida contenido/whitespace; Unity está condicionado por secret/license, sin fingir PASS.
- Engram pide recuperar memorias pertinentes, no copiar DB/logs/docs enteros.
- Una escena activa simplifica build.
- Redirects históricos cortos preservan enlaces.

No tocaría por eficiencia: sumar roles Planner/Reviewer a todo, borrar specs canónicas, cargar atlas en bug C#, correr build Android o suite completa para cada UI color.

## 23. Cuellos de botella

1. **Cwd desalineado (alto):** este turno apunta a carpeta no Git.
2. **Mapa/monolito (alto en UI):** búsqueda repetida y archivo 121 KB.
3. **Contrato stale (alto):** scopes/mecánicas contradictorias.
4. **Reasoning global xhigh (medio):** potencial en cambios simples, no medido.
5. **Unity tests/build (alto solo al requerirse):** PlayMode actual perdió foco; Android es lento.
6. **Git push sugerido para todo cambio (medio).**
7. **Estado repetido (bajo/medio).**
8. **Arte/Unity import (alto en tarea visual, irrelevante para lógica).**

## 24. Problemas por severidad

| Severidad | Problema / causa | Impacto / evidencia | Recomendación |
|---|---|---|---|
| ALTO | AGENTS/SDD/README stale de cinco niveles/carbón | contradice specs y runtime, puede guiar mal | corregir tras aprobación |
| ALTO-entorno | cwd fuera del repo | no hereda archivos locales por ruta esperada | abrir checkout real |
| MEDIO/ALTO | no router/mapa; AsaditoGame monolito | redescubrimiento o lectura enorme | indexar paths, buscar snippets |
| MEDIO | Visual skill stale | sugiere arte/mecánica eliminada | actualizar texto |
| MEDIO | Git pide commit/push sin umbral | efecto externo innecesario | distinguir local/commit/push solicitado |
| MEDIO | 3 status docs solapados | relectura/ambigüedad temporal | snapshot único, historia enlazada |
| MEDIO | reasoning xhigh global | puede sobregastar reasoning en trivial | valorar default menor y escalamiento |
| BAJO | scoring/loop redirects | hops de navegación pequeños | elegir autoridad y mantener enlaces |
| BAJO | script UI grande | leer entero caro, pero no requiere refactor | añadir mapa y extraction gradual |

**Críticos:** ninguno detectado. No existe automatización multiagente obligatoria ni resultado falso identificado.

## 25. Quick wins (no implementados)

1. Corregir instrucciones stale en cuatro archivos.
2. Cambiar el project/cwd de Codex a checkout real.
3. Agregar mapa de paths por dominio y complejidad.
4. Definir ruta trivial con no planner/skill/tests/doc/commit/push por defecto.
5. Un snapshot operativo compacto enlaza auditoría/status histórico.
6. Separar score product y systems como autoridad + alias.
7. No crear documento nuevo para color/offset; actualizar specs solo ante regla nueva.
8. Medir una tarea simple y otra compleja antes/después para validar reducción real.

## 26. Arquitectura ideal

Mantener un agente principal. Una matriz, no un nuevo sistema:

    prompt
      ↓
    AGENTS pequeño/veraz
      ↓
    router/matriz de complejidad
      ├─ SIMPLE: rg → skill opcional → 1–3 archivos → diff/validación mínima
      ├─ MEDIA: 1–2 skills → spec puntual → código/assets → test dirigido
      └─ COMPLEJA: spec/acceptance amplia → plan/review solo si agrega valor
                   → tests escalonados → Unity/build cuando corresponda

Clasificar por incertidumbre, sistemas tocados y riesgo, no extensión del prompt. Mantener alta autonomía para decisiones reversibles y confirmar solo efectos externos o riesgos importantes.

## 27. Routing recomendado

| Dominio | Skill/spec | Rutas clave | Check |
|---|---|---|---|
| UI color/copy | Ninguna por defecto | label/MakeButton en AsaditoGame; feedback script si necesario | diff y compile/preview si impacta |
| Layout/arte | Visual + sección art-direction/asset-manifest | AsaditoGame, Resources/Art, props/atlas | importar y ver en Editor |
| Gameplay/serving | SDD + Verify cuando altera regla | FoodPieceTouch, ServingBoardTouch, AsaditoGame | test de serving/touch |
| Nuevo corte | SDD + Visual | FoodCatalog.json, FoodCatalog, CookingModel, SpriteLibrary, LevelCatalog, assets/tests/validator | validator + EditMode + PlayMode afectado |
| Animación | Visual + animation spec/manifest | corutinas AsaditoGame, scripts touch, pinza/tabla | preview/editor + flujo relevante |
| Cooking bug | SDD + Verify | fire-heat/food-cooking + models/data/test | EditMode primero, PlayMode según interacción |
| Android | Unity tooling + android-release | Editor build script/settings/artifact | build/install/smoke solicitado |
| Tests | Verify + gate puntual | test assembly/test fixture + validator | suite o filtro afectado |

## 28. Qué conservar

AGENTS único; tres skills; specs canónicas/gate; Validator + CI condicionado claramente; Visual Bible y manifests; memoria Engram selectiva y portable; una escena build; tests con fechas/SHAs. Mantener los aliases mientras haya enlaces. No añadir subagentes seriales.

## 29. Qué modificar (pendiente aprobación)

- Actualizar AGENTS, README, asadito-sdd y asadito-visual-mvp.
- Incorporar mapa path→tarea y complejidad.
- Matizar compromiso Git para push no solicitado y tarea trivial.
- Elegir current-state o mvp-status como snapshot canónico.
- Corregir la ruta del proyecto Codex.
- Nada de esto fue cambiado en esta auditoría.

## 30. Qué fusionar

No fusionar aún. Evaluar consolidar current-state/mvp-status como resumen+registro fechado; definir una fuente canónica de scoring entre product/scoring y systems/scoring. Mantener SDD separado de Unity Verify, Visual Bible separado del asset-manifest, no borrar los redirects sin verificar enlaces.

## 31. Qué eliminar potencialmente

No se recomienda borrar nada ahora. Aliases históricos cuestan poco y preservan referencias. Corregir skills stale en lugar de borrarlos. No crear agentes que luego haya que eliminar. Limpiar docs históricos solo si se retienen fecha, SHA y evidencia.

## 32. Próximos pasos

1. Confirmar/open el Git checkout correcto en Codex.
2. Esperar aprobación antes de modificar instrucciones o routing.
3. Corregir las referencias stale.
4. Agregar mapa + reglas proporcionales.
5. Medir archivos abiertos, skills/specs, pruebas y latencia en tarea trivial y compleja.
6. Esta auditoría no ejecutó tests, validator ni build, ni commit/push.

## 33. Conclusión

ASADITO evita la sobreingeniería obligatoria pero no garantiza pocos tokens antes del primer archivo correcto. Con repo bien abierto, rg puede hallar el origen en un paso; sin mapa ni raíz correcta, las instrucciones y los paths obligan a redescubrir. Skills cortos ahorran contexto en su dominio. La prioridad es corregir precisión/ruta y mapear el código, no añadir agentes.

### Tabla ejecutiva final

| Área | Estado | Tokens | Velocidad | Problema | Prioridad |
|---|---|---|---|---|---|
| AGENTS | Uno, corto, desactualizado parcialmente | Bajo (~466) | Carga rápida, interpretación riesgosa | cinco niveles antiguos | Alta |
| Routing | Prosa semántica, sin dispatcher | Bajo formal; exploración no medida | Incierta | no mapea tarea→archivo | Alta |
| Skills | 3 locales, ~674 total | Bajo | Rápida carga condicional | 2 stale, no hay UI/cooking route específico | Alta |
| Specs | 16, ~5.36k total | Medio global | buena si selectiva | scoring/aliases solapados | Media |
| Testing | 21 Edit/10 Play | poco token, tiempo elevado | PlayMode actual sin terminar | 8/10 por foco | Media |
| Agentes | 0 | 0 | sin overhead | no hacen falta en trivial | Baja |
| Workspace | cwd fuera de Git root | — | añade un paso | descubrimiento local incierto | Alta |

### Escenario objetivo: “Cambiá el color del botón X”

1. Cargar AGENTS del checkout correcto y normas globales obligatorias.
2. Skill: ninguna; Visual solo si cambia la guía de estilo.
3. rg por label; abrir solo fragmento relevante de Assets/Asado/Scripts/AsaditoGame.cs; feedback script solo si afecta pressed state.
4. No cargar agents, catálogos, progresión, scoring, manifests irrelevantes, snapshots ni suite completa.
5. Validar diff/compilación; preview solo si cambia contraste/layout.
6. Terminar con resumen corto; sin doc, commit ni push automático salvo pedido/política explícita.

Comparación: es realizable hoy si se abre el repo correcto y se usa búsqueda acotada, pero el sistema no lo garantiza. No hace falta borrar skills: hace falta corregir instrucciones y ruta, y reducir la búsqueda a snippets.
