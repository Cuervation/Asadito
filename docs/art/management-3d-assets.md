# Recursos de gestión 3D — producción inicial

| Recurso | Uso | Geometría |
|---|---|---|
| Resources/Management3D/chorizo.asset | Compra y unidad en heladera | 561 vértices, tripa curva, extremos atados y vetas |
| Resources/Management3D/tira.asset | Compra y unidad en heladera | 975 vértices, tira irregular, grasa/vetas y cuatro costillas |
| Resources/ManagementSurface.shader | Superficie pintada compartida | Color de vértice, microtextura procedural suave, luz fija/destello; una pasada |
| Resources/ManagementGlass.shader | Vitrina | Alpha/Fresnel ligero; una pasada, sin sombras dinámicas |

Assets nativos optimizados, no primitivas de comida ni planos con imágenes. Authoring determinista en ManagementMeshes; regeneración explícita por menú **Asadito/Bake Management 3D Food Models** / Editor.ManagementModelBake.Bake, preservando GUID al rebake. Blender no está instalado; no dependencias nuevas ni herramienta externa obligatoria. Mesh assets son autoritativos en runtime; factory idéntica queda como fallback/control de extensión, no un placeholder distinto.

Scenery de mostrador/heladera authored en Unity con props biselados, madera/metal/vidrio, bandejas, carteles físicos, estantes y bisagra. Props opacos fijos combinados; puerta, comida y sombras de contacto independientes. Materiales/mesh compartidos entre instancias, sin material nuevo por unidad; highlight usa MaterialPropertyBlock. Encuadre adaptativo al ancho útil (validado780×1100, guard contra tamaño0 transitorio). RenderTexture máximo1200×1200, depth16, MSAA2, sin HDR; cámara/recursos locales liberados al cerrar. No Update de negocio ni deterioro/frescura offline.

Registro RegisterFoodModel permite agregar modelos del catálogo antes de habilitar su producto. No se activaron nuevos cortes, freezer ni upgrades. La capacidad configura slots/estantes; la tienda pagina pares cuando existan más modelos registrados.

Biblioteca PNG original y CounterV2 previo preservados, sin borrar GUID/archivos. ManagementArt solo enlaza patio/moneda para planificación/resultados/HUD; fondos/poses/heladera 2D anteriores ya no entran al player por esa referencia. FoodCatalog/atlas de cocción siguen intactos.

Estado: modelos y recursos de producción de esta primera implementación. Capturas reales y pruebas en [QA](../ai/management-3d-qa.md); la aprobación humana de estilo y el profiling táctil en Android son verificaciones independientes, todavía pendientes, no sustituidos por pruebas automáticas.
