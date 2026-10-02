# Carnicería y heladera 3D — QA 2026-10-01

## Implementación
Cámaras fijas aisladas en layer30 y RenderTexture, con viewport táctil real que traduce eventos UI a raycast físico. Mostrador refrigerado madera/metal/vidrio, bandejas, múltiples meshes según stock y carteles físicos dinámicos. Carrito compacto expandible: cantidades/subtotales/total, +/−/vaciar/pago. Cotización previa limita taps inválidos, checkout reutiliza transacción atómica existente.

Heladera 3D con puerta0.55s, estantes automáticos, cada objeto asociado a InventoryUnit.Id real. Selección temporal a bandeja0.25s, devolución/cancelación sin mutar inventario; preparación consume exactamente IDs únicos mediante PrepareUnits. API Prepare por SKU preserva FIFO y delega. Guarda ActiveRun y no duplica inventario/recompensas. Recovery y parrilla Canvas/térmica intactos; deterioro offline, freezer y upgrades no activados.

Modelos nativos de producción: chorizo561v, tira975v (curvatura/nudos y grasa/vetas/cuatro costillas). Meshes y materiales compartidos, colores pintados/microtextura shader, no primitivas ni sprites de comida en las vistas nuevas. Blender ausente; authoring Unity sin paquetes extra. Detalle y límites: [recursos](../art/management-3d-assets.md).

## Evidencia
- Unity6000.6.3f1, checkout QA importado `/tmp/Asadito-concurrent-cooking-qa`; Editor principal no cerrado ni Library reimportada por esta tarea.
- Inicial ManagementTests34/34PASS: `/tmp/asadito-3d-domain.xml` (compilación inicial y compatibilidad).
- Focal3D inicial2/3 encontró driver de VOLVER fuera de scope. Corregido driver; capturas vacías detectadas visualmente por recreación de RT al cambiar Canvas, corregidas re-renderizando tras layout. Primera compilación de pruebas requirió assembly separado; segunda renombró variable de driver. No fueron regresiones de economía.
- Cierre focal previo3/3PASS,13.970s: `/tmp/asadito-3d-visual-repair.xml`, colliders/modelos/repetición de taps, cancelación, IDs exactos, vacío/stock0.
- Release EditMode66/66PASS,3.316s: `/tmp/asadito-3d-release-edit.xml`; incluye seis casos nuevos de identidad y rechazo atómico por duplicado/inexistente/unlock/podrido/run activo.
- PlayMode completo20/20PASS,386.769s: `/tmp/asadito-3d-release-play.xml`; canasta/errores/saldo/stock/capacidad, paid flow/reload/recovery/raw/recompensa única y cocina/servicio actuales (incluye flujo pagado L1/L5; no recorrido pagado completo L1–6 en esta ejecución).
- Cierre visual final3/3PASS,13.965s: `/tmp/asadito-3d-final-views.xml`, tras soporte de carteles/base de heladera y retiro de refs2D. Todos16 centros físicos, overlay de detalle, selección/retiro por ID y cancel/reload/prepare. Presupuesto medido18,912 triángulos de comida,42 MeshRenderers activos; targets de UI132px de referencia y envelope44dp-equivalente para piezas finas. No equivale a profiling FPS.
- Cierre adicional de formato móvil2/2PASS,10.505s: `/tmp/asadito-3d-tall-viewport-closure.xml`. Bounds de las16 piezas completos en área780×1100; cámara adapta ancho y descarta rects transitorios0 para evitar proyecciónNaN detectada en primer intento. El cambio es local de cámara; suites previas no rerun, Android regenerado incrementalmente.
- Food validator PASS; 18 perfiles/108 frames/layouts L1–L12; sprites/perfiles de cocina sin cambio. Diff whitespace PASS.

Capturas Unity portrait1080×1920 (no mockups): `/tmp/asadito-3d-shop.png`, `/tmp/asadito-3d-cart-expanded.png`, `/tmp/asadito-3d-fridge.png`, `/tmp/asadito-3d-fridge-selected.png`, `/tmp/asadito-3d-shop-empty-stock.png`, `/tmp/asadito-3d-fridge-empty.png`. Copias estables en build/qa-management-3d al cerrar.

## Archivos principales
- Scripts/ManagementScreen.cs: gestión3D y carrito/retiro físicos; UI segura bajo contentRoot existente.
- Scripts/Management3D/: assembly, view/raycast/lifetime, autoría de meshes.
- Resources/Management3D/: dos meshes de producción y metadata; dos Resources shaders.
- Scripts/Editor/ManagementModelBake.cs: rebake explícito conservando GUID.
- Runtime/ManagementService.cs: CanPrepareUnits/PrepareUnits; transacción exacta, sin nuevo save.
- Tests/EditMode/ManagementTests.cs, Tests/PlayMode/FirstPlayableFlowTests.cs y asmdef: identidad, errores, taps/overlays/persistencia/cocina/lifetime.
- ManagementArt.asset: solo patio/moneda; referencias 2D retiradas sin borrar archivos.
- AsaditoGame.cs, AndroidReleaseBuild.cs, ProjectSettings.asset: identidad de versión1.4.0/code6.
- Specs de carnicería/inventario/dirección artística; docs recursos/motion/estado/Android y este QA.

## Límites y Git
Arte de producción inicial, no placeholders; falta aprobación estética humana y profiling/touch/notch en Android físico. Sin carnicero3D opcional ni long-press secundario. Extensión de modelos disponible, solo dos cortes comerciales activos.

APK1.4.0/code6 ARM64/IL2CPP Succeeded en build inicial, con regeneración incremental final tras el ajuste de formato móvil; paquete com.cuervation.asadito, min26/target+compile36, firma v2 debug verificada. Tamaño66,634,050 bytes (~63.55MiB), SHA256 `02231b37289a072a8202e103b27c17da9ebcf9ce2569eb810b36ae070071e93c`. Artefacto `/Users/celestino/Asadito/build/Asadito-1.4.0-management-3d-20261001.apk`; log inicial `/tmp/asadito-3d-android.log`; log final `/tmp/asadito-3d-android-final.log`.

Alcance de Git: código/meshes/shaders/tests/specs/documentación3D y versión1.4.0; staging parcial de documentos compartidos conserva las notas2D previas sin empaquetarlas. No instalación ni compras en el teléfono del usuario solicitadas en este pedido. Notas/PNG/importer del mostrador2D previo preservados fuera del commit. Ver HEAD de main y confirmación remota en el informe final; APK/previews ignorados por Git.
