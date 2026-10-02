# ASADITO — estado operativo actual

**2026-10-02.** Dirección canónica: [full-game](../specs/product/full-game.md). No usar informes históricos como instrucciones actuales. Baseline previo conservado en [historia 2026-09-30](ai/operational-state-20260930-history.md).

## Jugable
- Primer capítulo de 12 asados; **L1–6** disponibles secuencialmente según estrellas. Partida nueva empieza solo con L1; cards restantes Disabled/candado. L7–12 siguen bloqueados hasta implementar sus verticales. Se conservan unlock/stars/scores antiguos.
- **Todos L1–6:** pedido → carnicería → compra chorizo/tira → heladera/inventario → selección → preparación → bandeja → parrilla → tabla → servicio → evaluación/recompensa/saldo persistido.
- Wallet real, stock por jornada, capacidad limitada, selección de cantidades, descarte/pérdida, reward único y Caja del Asador sin soft-lock. Reiniciar cocina conserva el mismo run; un reinicio de app permite reanudarlo desde su nivel sin recomprar. Solo abandono explícito cuenta carne preparada como pérdida.
- L1 compra guiada chorizo/tira con monedas reales; L2 cantidad; L3 gustos/puntos; L4 ganancia/pérdidas; L5 inventario existente; L6 desafío autónomo5comensales. Tutorial/tips persistidos sin repetir; heladera/recovery accesible aun con saldo cero.
- Saldo inicial650 solo en partidas nuevas; precios100/180 y reward80+150/invitado modulado por rendimiento. Bonus L4 retirado sin quitar/pagar recursos históricos.
- Resultado separa ASADOR/GESTIÓN/OPERACIÓN, general, estrellas, costo utilizado, desperdicio, ingresos, ganancia y saldo. Pesos60/25/15 con mínimos55ASADOR/50cocción peorpieza/65saciedad; dos/tresestrellas requieren70/85ASADOR. Crudo/quemado no se aprueba por gestión perfecta. Compra conjunta confirmada por carrito y descarte confirmado; reparto justo normalizado por saciedad/gusto/punto.

## Contratos conservados
- Una escena portrait1080×1920, safe area, input directo; sin carbón/encendido/apagado ni botón PAUSA visible. VOLVER con flecha; pausa interna por app/Escape durante cocina.
- Todos los cortes colocados se cocinan simultáneamente por perfil individual; selección no pausa otros. Un solo lado, sin girar por tap. Fuente y tabla contienen huellas completas y apilan solo overflow; último emplatado al frente. Tap en tabla completa sirve.
- FoodCatalog18 alimentos y diez estados runtime por alimento; no se exponen los 18 desde el comienzo.

## Arquitectura/datos
- `ManagementConfig.json`: economía, capacidad, precios, stock, porciones, rewards, pesos y gates; `ChapterOneLevels.json`: pedidos. Catálogo de alimentos sigue siendo fuente de identidad/perfiles/arte.
- `ManagementState`/`ManagementService`/`Wallet`: reglas y transacciones independientes; `ManagementScreen`: mostrador con vitrina/bandejas tocables y carrito temporal (+/−/vaciar/total), checkout único PAGAR Y SALIR hacia heladera; vistas event-driven; AsaditoGame adapta preparación/resultados sin reescribir térmica.
- Save v5, migración aditiva desde v1–4; normaliza ActiveRun fantasma vacío materializado por JsonUtility sin eliminar runs preparados reales. Balance/inventario/run/equipamiento/ciclos persistidos; progreso/settings retenidos.
- Cuatro sprites de gestión conectados por referencias en `Resources/ManagementArt.asset`: patio, moneda, CounterV2 y FridgeV2;88 restantes de la biblioteca fuera del runtime. Botones arcade existentes intactos.

## Preparado, no activado
- Frescura derivada/modelo probado; `EnableFreshness=false`, sin tiempo real/offline. Ciclo de conservación separado de jornada.
- Cotización de promos probada, sin ofertas activas aún. Freezer/upgrades/estaciones/eventos solo diseño y campos mínimos de save; no acciones falsas.
- Roadmap Verticales2–6 en [roadmap](roadmap.md); reglas por dominio enlazadas desde full-game.

## QA de esta entrega
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

## Carnicería híbrida/changuito — implementación vigente (2026-10-01)
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

### Tamaño de parrilla y carteles comerciales — corrección vigente (2026-10-01)
- Reemplaza la miniaturización/separación estricta anterior:48 instancias según stock24 por SKU, comida con footprints reales de parrilla (L1 chorizo185.71×71.71/tira265.20×100.43), dos capas apoyadas con superposición permitida y perspectiva natural. Bordes se desplazan al interior, no se achican.
- Carteles nativos blancos con banda verde, precio rojo grande/contorno y pinza/poste/base metálicos; $100/$180 intactos. Texto sin truncar, stock/llevás visibles y cartel bloquea compra oculta.
- MeshColliders de silueta solo en mostrador; heladera conserva cajas/IDs exactos. Drag preview comparte medida de parrilla; pulso y compra/pago/guardado intactos.
- **3/3 PlayMode focales PASS** (cierre13.052s, ancho al1%): tamaño/paridad de controles preparados,48 modelos/stock parcial/agotado, safe area780×1100, texto/raycast/pulso/drag/pago/reload/preparación. Renders reales inspeccionados;66,912 triángulos/48 renderers, sin FPS físico. [QA](ai/counter-grill-size-signs-qa.md).
- Sin APK/install/commit/push nuevos; celular sigue1.4.0 anterior. Falta build/instalación si se solicita y QA táctil/performance humano.

## Heladera híbrida — implementación vigente (2026-10-02)
- Escenario ilustrado2D vacío Fridge_Hybrid_OpenEmptyV2 (1254RGBA, originalTier1 preservado) con comida volumétrica3D compartida con tienda/parrilla. No gabinete/puertas opacos3D que oculten alimentos ni arte duplicado de comida.
- Hasta8 unidades reales por ID en tres superficies; ancho proyectado canónico por corte en estantes y tabla, sin miniaturizar selección. MeshCollider/silueta y raycast de profundidad seleccionan el corte expuesto cuando hay superposición. Cancelar/volver/preparar preservan economía e identidad exacta; freshness desactivada sigue igual.
- Frame de aspecto nativo dentro de host central; footer no tapa la tabla. DESCARTAR se mueve afuera del escenario, al lado de COMPRAR o CAJA DEL ASADOR cuando corresponde; mismos botones arcade. ManagementArt tiene4 referencias activas: patio, moneda, CounterV2 y FridgeV2.
- Unity6000.6.3f1 real, **5/5 PlayMode focales PASS** (23.963s): capacidad8, tamaño al1%, picking de superposición visible, cancelación/IDs exactos/persistencia/preparación; regresiones stock/carnicería/changuito y texto/targets. Capturas reales1080×1920 inspeccionadas. [QA](ai/hybrid-fridge-qa.md).
- Cierre Git autorizado: incluye cambios pendientes de carnicería híbrida, stock24/SKU, tamaño de parrilla/carteles y heladera. No APK/build/install nuevos; Motorola conserva1.4.0/code6 previo sin estas vistas. Aprobación artística/touch/performance Android pendientes.
