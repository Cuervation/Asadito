# ASADITO — estado operativo actual

**2026-10-02. Asadito es 100 % 2D.** Dirección canónica: [full-game](../specs/product/full-game.md). No usar informes históricos como instrucciones actuales. Baseline previo conservado en [historia 2026-09-30](ai/operational-state-20260930-history.md).

## Jugable
- Primer capítulo de 12 asados; **L1–6** disponibles secuencialmente: **L2 exige completar L1 con al menos1estrella**, y cada siguiente exige aprobar todos los anteriores. Un MaxUnlockedLevel legado alto no saltea esa condición. Partida nueva empieza solo con L1; cards restantes Disabled/candado. L7–12 siguen bloqueados hasta implementar sus verticales. Se conservan los datos de unlock/stars/scores antiguos, monedas e inventario; no se borran para aplicar el bloqueo.
- **Todos L1–6:** pedido → carnicería → compra libre del catálogo de18 alimentos → heladera/inventario → selección → preparación → bandeja → parrilla → tabla → servicio → evaluación/recompensa/saldo persistido.
- Wallet real, stock por jornada, capacidad limitada, selección de cantidades, descarte/pérdida, reward único y Caja del Asador sin soft-lock. Reiniciar cocina conserva el mismo run; un reinicio de app permite reanudarlo desde su nivel sin recomprar. Solo abandono explícito cuenta carne preparada como pérdida.
- L1 compra guiada chorizo/tira con monedas reales; L2 cantidad; L3 gustos/puntos; L4 ganancia/pérdidas; L5 inventario existente; L6 desafío autónomo5comensales. Tutorial/tips persistidos sin repetir; heladera/recovery accesible aun con saldo cero.
- Saldo inicial650 solo en partidas nuevas; precios100/180 y reward80+150/invitado modulado por rendimiento. Bonus L4 retirado sin quitar/pagar recursos históricos.
- **Pedido obligatorio por comensal:** cada invitado pide el FoodId canónico de su posición en el nivel, independiente de favoritos. Sólo recibe ese corte; faltantes/sustituciones/extras impiden estrellas y avance, aun con excelente cocción o gestión. Comprar/preparar otros cortes sigue permitido; no se cuenta el stock de heladera como servido ni se modifica el pedido por lo preparado.
- Resultado separa ASADOR/GESTIÓN/OPERACIÓN, general, estrellas, costo utilizado, desperdicio, ingresos, ganancia y saldo. Pesos60/25/15 con mínimos55ASADOR/50cocción peorpieza/65saciedad; dos/tresestrellas requieren70/85ASADOR. Crudo/quemado no se aprueba por gestión perfecta. Compra conjunta confirmada por carrito y descarte confirmado; reparto justo normalizado por saciedad/gusto/punto.

## Contratos conservados
- Por pedido del usuario, los gramos/porción objetivo por comensal se ocultan por ahora en planificación; siguen visibles su corte pedido, punto y gustos. El cálculo interno de apetito/saciedad no cambia.
- Una escena portrait1080×1920, safe area, input directo; sin carbón/encendido/apagado ni botón PAUSA visible. VOLVER con flecha; pausa interna por app/Escape durante cocina.
- Todos los cortes colocados se cocinan simultáneamente por perfil individual; selección no pausa otros. Un solo lado, sin girar por tap. Fuente y tabla contienen huellas completas y apilan solo overflow; último emplatado al frente. Tap en tabla completa sirve.
- FoodCatalog18 alimentos y diez estados runtime por alimento; no se exponen los 18 desde el comienzo.

## Arquitectura/datos
- `ManagementConfig.json`: economía, capacidad, precios, stock, porciones, rewards, pesos y gates; `ChapterOneLevels.json`: pedidos. Catálogo de alimentos sigue siendo fuente de identidad/perfiles/arte.
- `ManagementState`/`ManagementService`/`Wallet`: reglas y transacciones independientes; `ManagementScreen`: navegación, carrito temporal (+/−/vaciar/total), selección por IDs y checkout único PAGAR Y SALIR hacia heladera; `ManagementFoodView`/`Asadito.Management2D`: imágenes Canvas, picking por sibling order/máscaras alfa precomputadas y animaciones breves. Sin estado de negocio duplicado ni mundo/cámara/RenderTexture/material/mesh/collider 3D de gestión. AsaditoGame adapta preparación/resultados sin reescribir térmica. [Arquitectura y auditoría](architecture/management-2d.md).
- Save v5, migración aditiva desde v1–4; normaliza ActiveRun fantasma vacío materializado por JsonUtility sin eliminar runs preparados reales. Balance/inventario/run/equipamiento/ciclos persistidos; progreso/settings retenidos.
- Siete sprites de gestión conectados por referencias en `Resources/ManagementArt.asset`: patio, moneda, CounterV2, FridgeV2, Butcher_Neutral y los marcos ResultPanel/ProductCard;85 restantes de la biblioteca fuera de esa referencia runtime. Botones arcade existentes intactos.

## Preparado, no activado
- Frescura derivada/modelo probado; `EnableFreshness=false`, sin tiempo real/offline. Ciclo de conservación separado de jornada.
- Cotización de promos probada, sin ofertas activas aún. Freezer/upgrades/estaciones/eventos solo diseño y campos mínimos de save; no acciones falsas.
- Roadmap Verticales2–6 en [roadmap](roadmap.md); reglas por dominio enlazadas desde full-game.

## Migración definitiva 2D — vigente (2026-10-02)
- CounterV2 y Fridge_Hybrid_OpenEmptyV2 existentes se reutilizan; comida RAW exacta de parrilla mediante Images Canvas; tarjetas de catálogo escalan uniformemente el RAW y heladera/parrilla conservan tamaño canónico por corte, sin PNG duplicados ni regeneración masiva. El nombre Hybrid del PNG se conserva por estabilidad de recurso, no describe arquitectura.
- Carnicería atendida: **18 alimentos con precio disponibles desde L1**, cuatro tarjetas2×2 por página y cinco páginas (4+4+4+4+2), ahora navegables con deslizamiento horizontal en ambos sentidos, sin flechas inferiores; su espacio se conserva sobre el carrito. CounterV2 y Butcher_Neutral existentes, sprites RAW idénticos a heladera/parrilla. Precio/stock/cantidad, carrito persistente entre páginas, detalle solo de productos elegidos y pago único. Selección solo cotiza.
- Compra para stockear libremente, aunque el alimento no forme parte del pedido actual: **no se reserva presupuesto ni se bloquea compra para asegurar el objetivo**. El pedido mostrado no restringe la compra: sólo se valida saldo real, stock y capacidad8. Su cumplimiento sí es obligatorio al servir para aprobar el asado. Chorizo$100/tira$180 y saldo inicial650 conservados; nuevos precios configurados en ManagementConfig. Inventario y compras antiguos conservan sus IDs/costos.
- Constructor de MvpSaveData ya no carga Resources durante serialización; Migrate/RecordLevelResult dimensionan estrellas/puntajes al catálogo vigente sin perder filas.
- Carnicería nueva: **EditMode46/46 PASS; PlayMode12/12 PASS + cierre focal1/1 PASS; validator contenido y scoped diff check PASS**. Capturas Canvas1080×1920,720×1600 y liveFoldable960×2658 revisadas; Play/Simulator local abiertos. [QA del catálogo](ai/shop-four-cuts-qa.md). Sin APK/build/install ni commit/push nuevos.
- Heladera: hasta8 imágenes por InventoryUnit.Id real, estantes/tabla ilustrados; selección/devolución breve y reversible, preparación exacta por IDs. Recuperación/descarte/tutorial/save y frescura deshabilitada intactos.
- Compra→asado corregido: planificación ofrece **PREPARAR ASADO**; pago explica cómo elegir las piezas; heladera muestra **ELEGÍ N / FALTA N / IR A LA PARRILLA**. Se conserva cantidad del pedido (L1=2), pero también el tutorial acepta cualquier corte comprado o mezcla repetida; sólo consume los IDs confirmados, el resto sigue stock. **6 pruebas únicas PlayMode focales PASS**, incluida reproducción nativa de compra2chorizos+2tiras/saldo90, preparación/cocción/servicio con sobrantes y lomo comprado→preparado→resume. [QA del paso a parrilla](ai/stock-to-grill-qa.md).
- Resultados rediseñados como **tablero ilustrado**: estrellas/puntaje general protagonistas, expresiones de invitados, ASADOR/GESTIÓN/OPERACIÓN compactos, saldo separado de ganancia/pérdida y consejo. **VER DETALLE** abre desglose completo con scroll y bloqueo de clicks por detrás. Usa marcos existentes, escalado uniforme safe-area y entrada0.20s; no cambia scoring, recompensas ni guardado. **4 pruebas PlayMode focales PASS** (una por XML nativo aunque el job MCP perdió callbacks y declaró timeout; tres integraciones confirmadas por MCP), capturas1080×1920/720×1600 inspeccionadas. [QA del resultado](ai/results-illustrated-qa.md). Sin APK/commit/push nuevo.
- Picking por orden de hermanos y alfa precomputada, no física3D. Puntero dueño único; cancelar/drop fuera/UI/otro dedo/cierre no añade unidades. Preview temporal y corutinas se limpian sin consumir inventario.
- Retirados meshes 3D exclusivos de gestión, generadores, shaders y bake de Asadito; se preservan terceros, Unity MCP, herramientas generales y metadata/GUID de arte reutilizado.
- **Unity6000.6.3f1: compile/import PASS; ManagementTests41/41 EditMode PASS; nueve PlayMode focales9/9 PASS (69.116s) y cierre visual/drag/capacidad3/3 PASS (16.974s); validator contenido PASS.** Capturas reales1080×1920/720×1600 revisadas; [evidencia y fallos corregidos](ai/management-2d-qa.md). No suite completa, APK/instalación ni FPS/touch/performance físico medidos.
- **Android2D1.5.0/code7 generado e instalado (2026-10-02):** ARM64/IL2CPP,APK70.18MiB,metadata/SDK26–36/portrait/firma debugv2 verificadas. ValidatorPASS; EditMode72/72PASS; PlayMode28/29 inicial+recorridoL1–6focal1/1PASS (no único29/29). Motorola ZY22MBNWRB actualizado con `install -r`,datos conservados y Activity Status ok. Menú visual pendiente: pantallaOFF/bloqueada y USB desconectado antes del cierre. No compras/partida física/publicación/commit/push. [QA Android vigente](ai/android-2d-1.5.0-qa.md). Aprobación artística/touch/notch/performance humana pendientes.

- **Bloqueo secuencial reforzado (2026-10-02):** MaxUnlockedLevel legado sin estrellas ya no habilitaL2. EditMode11/11PASS y4casos PlayMode únicos verificados(3+1); preservación de progreso y tap nativo→L2 tras aprobarL1. [QA](ai/sequential-level-lock-qa.md). Solo fuente local: APK1.5.0/code7 instalada todavía no incluye este refuerzo. Sin APK/install/commit/push nuevos.

- **Presentación de nivel ilustrada (2026-10-02):** reemplaza popup gris/glass por marco de madera/papel existente, título verde Lilita, retratos reales de comensales, pedido con sprites originales y CTA arcade dorado. Flecha vuelve al selector sin iniciar/abandonar ni gastar. Ajuste uniforme safe-area y encuadre local del borde de atlas sin modificar arte compartido. PlayMode focal **4/4 PASS** + cierre de encuadre **2/2 PASS**; capturas L1/L3/L6 en1080×1920 y720×1600, saldo/progreso exactos. [QA](ai/intro-illustrated-qa.md). Solo proyecto local: APK1.5.0/code7 instalada todavía no incluye esta presentación ni el bloqueo secuencial posterior. Sin APK/install/commit/push nuevos.

- **Carnicería con swipe (2026-10-02):** deslizar a izquierda avanza y derecha vuelve, una página por gesto y límites sin wrap. Funciona desde comida/fondo/tarjeta y +/−; taps cortos conservan cantidad, arrastre vertical de comida conserva drop al carrito. Gesto corto, cancelación, otro dedo y puntero equivocado no agregan. Flechas inferiores retiradas, tarjetas/carrito y banda de separación intactos; indicador actualizado a «Deslizá a los lados para ver más cortes · N / 5», con franja verde translúcida fina, texto crema discreto y sin raycast ni botones. Cierre focal1/1PASS y capturas1080/720 revisadas; [QA del indicio](ai/shop-swipe-hint-qa.md). **4 casos únicos PlayMode PASS (3+1)**, Console0errores y capturas1080×1920/720×1600 revisadas. [QA](ai/shop-swipe-qa.md). Sin APK/install/commit/push nuevos. El runner abortado dejó un save temporal en Editor: respaldo fresco original ($166/dos piezas) intacto, restauración pendiente de aprobación del usuario; celular no afectado.

- **Identidad del pedido (2026-10-02):** asignación compatible por comensal y verificación de IDs/cantidades desde el catálogo contra las unidades realmente preparadas. Falta de pedido puntúa cero y muestra el corte faltante, sin confundirlo con mala cocción. Sin cambios de arte, precios, recompensa de fallo pagado ni save v5. Unity compile/Console0errores; EditMode **72/72 PASS en dos fixtures afectados** y **cuatro PlayMode únicos PASS (2 iniciales + cierre nativo2/2,163.035s)**. Cierre confirma dos morcillas bien cocidas no cumplen tira/chorizo, no estrellas/avance, stock correcto guardado intacto; recorrido positivoL1–6 PASS. Los últimos dos se confirman por XML nativo tras desconexión MCP, no por job MCP. Capturas1080/720 y texto legible; [registro](ai/requested-food-qa.md). Sólo fuente local, no APK/install/commit/push nuevo. Se conserva la advertencia anterior sobre restauración del respaldo humano del Editor pendiente de aprobación.

## Historia de implementación y QA
Las secciones siguientes conservan resultados de sus respectivas versiones; ninguna afirmación 3D/híbrida describe la arquitectura vigente.

## Evidencia histórica — gestión desde L1 (2026-10-01)
- Unity6000.6.3f1 real en checkoutQA ya importado; **EditMode53/53 PASS**, focal gestión **3/3 PASS** y recorrido pagado de los seis niveles **1/1 PASS** (153.12s). Validator contenido PASS. Broad12/13 inicial encontró truncadoL6; corregido y probado en cierre del recorrido. Evidencia exacta: [QA gestión desdeL1](ai/management-level1-qa.md).
- Renders1080×1920 de guíaL1/compra/heladera/resultados revisados. Font ascenders con espacio2.5x y overflow, detalle260px sin reducir fuente; test comprueba geometría/texto.
- Se conserva aislamiento del Editor principal; no se modifica Library del usuario.
- Android1.3.0/code4 ARM64/IL2CPP **Succeeded**, APK65MiB en `build/Asadito-1.3.0-management-level1-20261001.apk`; ABI/metadata/firma v2 debug verificados. Luego se instaló por ADB en Motorola ZY22MBNWRB; menú visible1.3.0 confirmado. APK aún no incluye el rediseño posterior del mostrador/carrito.

El release público requiere playtesting humano y QA táctil/performance ARM64. Esta vertical no equivale a toda la versión definitiva del juego.

## Rediseño posterior: mostrador con carrito (2026-10-01)
- Vitrina/bandejas tocables con precios, highlight/pulso, stock y carrito visible: cantidades, subtotales, total, +/−/vaciar. Selección libre; pedido orientativo. Reutiliza fondo/food/arcade sprites sin nuevos PNG.
- QuoteCart/BuyCart mantienen reglas en ManagementService: selección no debita, pago conjunto revalida y muta una sola vez. Éxito persiste y lleva a heladera; cancelar descarta únicamente el carrito.
- Compilación Unity real y ManagementTests **34/34 PASS**. Cuatro casos PlayMode focalizados aprobados en corridas proporcionales: canasta/cancelación/persistencia/preparación, L5 compra/cocina/servicio/reload, recovery y errores/raycast real. Renders portrait revisados; detalle en [QA mostrador](ai/butcher-counter-cart-qa.md).
- En el pedido de UX no se ejecutó suite completa/build Android. Cierre posterior solicitado: validator PASS, EditMode60/60 y PlayMode17/17PASS; APK1.3.1/code5 ARM64 Succeeded e instalada en Motorola ZY22MBNWRB con install -r sin limpiar datos. Menú real muestra Mostrador y carrito. QA táctil humana/performance del mostrador permanece pendiente; [evidencia Android](android-release.md).

## Vitrina abundante — rediseño posterior (2026-10-01)
- Carnicería cambia de dos grandes cards a escena ilustrada de mostrador compartido. Fondo nuevo CounterV2 sin carne horneada; hasta8 instancias tocables por SKU (16 con stock inicial), sombras/inclinaciones/variación suave. Etiquetas compactas por grupo con precio/stock/badge.
- Carrito reducido456→256px; cantidades/subtotales/total, quitar/sumar, vaciar y pago existentes. Actualización en sitio conserva targets y animaciones: rebote0.22s/vuelo0.36s; no nuevas reglas ni unidades persistentes.
- Horizontal scroll para futura vitrina con más de2 cortes, tickets asociados; vertical scroll en resumen sin tapar CTA. Datos vigentes conservan solo2 productos.
- Compile/import Unity real y **cuatro PlayMode focales PASS** en dos corridas (3/3+1/1):16 targets/raycast/clicks, no rebuild, contador/total/VFX/legibilidad, checkout/errores/persistencia/prepare y scroll con4 productos sintéticos/restaurados. Evidencia [QA](ai/abundant-counter-qa.md).
- No APK/build/instalación nueva ni commit/push en este pedido. Motorola conserva1.3.1 del mostrador anterior; nuevo rediseño solo en proyecto. Se preservan cinco archivos locales del release previo.

## Gestión 3D — implementación histórica (2026-10-01)
- Carnicería: mostrador refrigerado3D, vidrio/bandejas, hasta16 modelos por stock, precios/stock/selección dinámicos; compra directa por physics raycast. Carrito compacto expandible, pago atómico existente.
- Heladera: puerta animada, estantes y un modelo por InventoryUnit.Id real. Selección reversible a bandeja; PrepareUnits valida todos los IDs únicos y consume exactamente esos. Legacy Prepare/recovery/save y parrilla Canvas/térmica intactos.
- Meshes nativos chorizo561v/tira975v y shaders compartidos de producción inicial. Sin sprites/primitivas de comida en gestión, sin nuevos paquetes. Static props combinados, RT cap1200×1200/depth16/MSAA2, lifetime local;18,912 food triangles/42 MeshRenderers activos en vitrina completa.
- EditMode66/66PASS; PlayMode20/20PASS y cierre final3/3PASS; capturas realesportrait revisadas. [QA](ai/management-3d-qa.md), [recursos](art/management-3d-assets.md).
- Identidad1.4.0/code6; Android ARM64/IL2CPP Succeeded, APK63.55MiB en build/Asadito-1.4.0-management-3d-20261001.apk, metadata/ABI/firma v2 debug verificadas. No instalación ni profiling/touch humano físico en este pedido. Aprobación estética humana pendiente, no interacciones simuladas ni inventario ficticio.

### Instalación Gestión 3D en celular (2026-10-01)
- APK1.4.0/code6 actualizada con install -r en Motorola ZY22MBNWRB, sin borrar datos, y abierta en primer plano. Menú real confirma v1.4.0 Gestión3D. Ver [evidencia Android](android-release.md); gameplay/touch3D/performance físico siguen pendientes. Sin commit/push nuevo en este pedido.

## Carnicería híbrida/changuito — implementación histórica (2026-10-01)
- Mostrador ilustrado vacío CounterV2 y comida3D con textura raw exacta de parrilla (relieve nativo2656v/2920v); heladera comparte esas piezas, resto de escenario/puerta y economía/cocción intactos.
- Changuito visible, drag al canasto agrega una unidad validada; cancelar/otro dedo/botón/fuera no agrega, pagar persiste transacción conjunta. Preview UI temporal único; retirados clones de vuelo. UV layout recalculado al cambiar viewport, VACIAR colapsa detalle.
- **7/7 PlayMode focales PASS** y capturas Unity reales revisadas;22,304foodtriangles/16MeshRenderers. [QA](ai/hybrid-cart-qa.md). No FPS físico medido.
- Sin APK/instalación/commit/push nuevos en este pedido: teléfono conserva1.4.0 anterior sin híbrido/changuito. Próximo paso build e instalación si se solicita.

### Mostrador con perspectiva — ajuste anterior (2026-10-01)
- Piezas sobre plano común bajo cámara perspectiva, filas escalonadas3–3–2 por SKU; diagonales, profundidad/tamaño relativo y espacios acompañan CounterV2. Carteles conservan legibilidad y pulso/drag recuperan HomeScale individual.
- **2/2 PlayMode focales PASS** (7.928s), bounds sin superposición,16targets independientes, viewport780×1100 y drag/pago/reload; capturas realesportrait inspeccionadas. [QA](ai/counter-perspective-qa.md).
- Solo proyecto local; no APK/instalación/commit/push nuevos. Teléfono sigue1.4.0 anterior.

### Mostrador completo — cantidad triplicada anterior (2026-10-01)
- Usuario aprobó24 unidades comprables de chorizo y24 de tira por jornada; hasta48piezas iniciales visibles, antes16. Cuatro columnas/seis filas de profundidad porSKU, separadas sobre plano/cámara perspectiva previa; stock parcial/0 reduce/vacía exposición sin relleno ficticio.
- Precios100/180, saldo inicial650, capacidad de heladera8, rewards/inventario/guardado/cocción intactos. Seleccionar cotiza, solo pagar debita y entrega.
- **3 casos únicosPlayMode+3EditMode focales PASS**:48centros físicos/UV/geometría/márgenes/viewport780×1100, pulso/drag/pago/reload, stock5+3 y0, economíaStock24/23 y atomicidad. Renders realesportrait inspeccionados;66,912foodtriangles/48renderers compartidos, sin FPS físico medido. [QA](ai/counter-triple-quantity-qa.md).
- No APK/install/commit/push nuevos; celular sigue1.4.0 anterior sin híbrido ni cantidad triplicada.

### Tamaño de parrilla y carteles comerciales — corrección histórica (2026-10-01)
- Reemplaza la miniaturización/separación estricta anterior:48 instancias según stock24 por SKU, comida con footprints reales de parrilla (L1 chorizo185.71×71.71/tira265.20×100.43), dos capas apoyadas con superposición permitida y perspectiva natural. Bordes se desplazan al interior, no se achican.
- Carteles nativos blancos con banda verde, precio rojo grande/contorno y pinza/poste/base metálicos; $100/$180 intactos. Texto sin truncar, stock/llevás visibles y cartel bloquea compra oculta.
- MeshColliders de silueta solo en mostrador; heladera conserva cajas/IDs exactos. Drag preview comparte medida de parrilla; pulso y compra/pago/guardado intactos.
- **3/3 PlayMode focales PASS** (cierre13.052s, ancho al1%): tamaño/paridad de controles preparados,48 modelos/stock parcial/agotado, safe area780×1100, texto/raycast/pulso/drag/pago/reload/preparación. Renders reales inspeccionados;66,912 triángulos/48 renderers, sin FPS físico. [QA](ai/counter-grill-size-signs-qa.md).
- Sin APK/install/commit/push nuevos; celular sigue1.4.0 anterior. Falta build/instalación si se solicita y QA táctil/performance humano.

## Heladera híbrida — implementación histórica reemplazada (2026-10-02)
- Escenario ilustrado2D vacío Fridge_Hybrid_OpenEmptyV2 (1254RGBA, originalTier1 preservado) con comida volumétrica3D compartida con tienda/parrilla. No gabinete/puertas opacos3D que oculten alimentos ni arte duplicado de comida.
- Hasta8 unidades reales por ID en tres superficies; ancho proyectado canónico por corte en estantes y tabla, sin miniaturizar selección. MeshCollider/silueta y raycast de profundidad seleccionan el corte expuesto cuando hay superposición. Cancelar/volver/preparar preservan economía e identidad exacta; freshness desactivada sigue igual.
- Frame de aspecto nativo dentro de host central; footer no tapa la tabla. DESCARTAR se mueve afuera del escenario, al lado de COMPRAR o CAJA DEL ASADOR cuando corresponde; mismos botones arcade. ManagementArt tiene4 referencias activas: patio, moneda, CounterV2 y FridgeV2.
- Unity6000.6.3f1 real, **5/5 PlayMode focales PASS** (23.963s): capacidad8, tamaño al1%, picking de superposición visible, cancelación/IDs exactos/persistencia/preparación; regresiones stock/carnicería/changuito y texto/targets. Capturas reales1080×1920 inspeccionadas. [QA](ai/hybrid-fridge-qa.md).
- Cierre Git autorizado: incluye cambios pendientes de carnicería híbrida, stock24/SKU, tamaño de parrilla/carteles y heladera. No APK/build/install nuevos; Motorola conserva1.4.0/code6 previo sin estas vistas. Aprobación artística/touch/performance Android pendientes.

## Mini barras de cocción por pieza (2026-10-02)
- Cada comida sobre parrilla muestra una barra propia debajo: track8px/fill6px en referencia1080, largo igual al Image real del corte (no hitbox), acompañando drag/animación. Sin porcentajes ni controles nuevos; oculta en bandeja cruda/tabla y no intercepta input.
- Rojo→amarillo→tramo verde→amarillo→rojo, avance por temperatura/estado de esa pieza y perfil individual, no cronómetro ni selección. Verde corresponde a A_Punto de cada perfil; provoleta exige su estado Ideal existente. Quemado/sequedad invalidan verde aunque el centro esté a temperatura objetivo. No garantiza preferencias de todos los invitados ni modifica scoring/térmica.
- Helper read-only FoodCookingProgress; pausa/reset/emplatado conservan comportamiento. Unity focal2/2EditMode+3/3PlayModePASS, capturasportrait reales revisadas; [QA](ai/food-cooking-bars-qa.md).
- Cambios locales posteriores a6238ae1, sin commit/push/APK/instalación nuevos. Celular aún1.4.0/code6 previo a híbrido/barras; QA táctil física pendiente.
