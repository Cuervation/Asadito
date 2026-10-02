# Recursos de gestión 3D — registro histórico

> **Histórico, reemplazado el 2026-10-02 por la decisión definitiva 100 % 2D.** Las mallas, shaders, generadores y bake descritos aquí se retiran del runtime/proyecto al migrar sus consumidores. Sus métricas, pruebas y capturas corresponden a las versiones anteriores, no validan la2D actual. Arquitectura/auditoría vigente: [gestión 2D](../architecture/management-2d.md). Se conserva el texto siguiente como evidencia, no como instrucciones para regenerar 3D.


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

## Carnicería híbrida/changuito (2026-10-01)
- Mostrador ilustrado vacío CounterV2 existente, recorte UV de presentación (sin editar PNG) y capa transparente3D. La versión procedural completa queda histórica, no activa en tienda.
- Nuevos chorizo-grill-relief.asset / tira-grill-relief.asset: malla cerrada de relieve modelada a partir del alpha del raw atlas original, con UV de la fila cruda. Se usan los mismos archivos Textures de la parrilla sin copias/editarlos. Bake explícito, ningún GetPixel ni textura CPU-readable en runtime. Volumen real/top/lados/base, no plano ni SpriteRenderer.
- Material ManagementFoodTexture compartido por SKU; superficie original preiluminada más sombreado leve del volumen. Heladera comparte estos mismos modelos y texturas.
- Changuito vectorial UI nativo de canasto/rejilla/manija/ruedas, mismos botones existentes. Un preview de arrastre UI del raw sprite, sin objetos3D de vuelo repetidos; RT sin MSAA y transparente en tienda.
- Validación táctil/performance Android de esta variante pendiente; screenshots Unity no sustituyen dispositivo físico.

### Distribución perspectiva (2026-10-01)
Tienda: cámara perspectiva FOV60°, mirada elevada (~60°) y roll5°; todos los modelos sobre plano horizontal compartido, tres filas separadas3–3–2 por grupo. Ray/plane layout con escala uniforme adaptativa tras resize y carteles billboard de profundidad fija. Se conservan mallas/UV/materiales. Heladera continúa ortográfica. Evidencia local y límites en [QA perspectiva](../ai/counter-perspective-qa.md).

### Cantidad triplicada (2026-10-01)
Stock aprobado24 porSKU, hasta 48 modelos reales en tienda. Cuatro columnas/seis filas de profundidad por grupo reemplazan3–3–2; count limitado al stock real, sin relleno cuando se agota. Escala/espacios adaptados sobre la misma cámara/plano. Recursos compartidos intactos,66,912triángulos/48MeshRenderers activos. [QA](../ai/counter-triple-quantity-qa.md); performance físico pendiente.

## Corrección de escala y carteles comerciales (2026-10-01)
Las48 instancias dejan de ser mosaicos miniaturizados: reutilizan la medida canónica de cada corte en la parrilla, con capas apoyadas/superposición permitida por el usuario. Calibración de bounds completos en profundidad media y contención por desplazamiento; fuente raw/mallas/UV existentes no se modifican. Colliders de silueta para comida del mostrador, cajas de heladera intactas. Presupuesto de geometría sigue66,912 triángulos/48 MeshRenderers; profiling físico pendiente.

Carteles realizados con Canvas/Text/Image nativos: papel/marco, banda verde, precio rojo grande con contorno, pinza/poste/base metálicos y stock/llevás. No se generaron PNG ni se reemplazaron botones. Texto usa contrato de ascenders/overflow de gestión y un collider impide comprar comida detrás del cartel. Evidencia y límites en [QA de escala/carteles](../ai/counter-grill-size-signs-qa.md); pendiente aprobación estética humana.

## Heladera híbrida histórica (2026-10-02)
Reemplaza el gabinete/puerta/props3D históricos por Fridge_Hybrid_OpenEmptyV2 ilustrado vacío. Solo alimentos3D reales del inventario: mismo raw atlas/malla de relieve/material de tienda/parrilla, sin duplicar PNG de comida. Cámara perspectiva FOV60°, roll0°, clear transparente, RT cap1200/MSAA1/depth16. Mallas nativas y materiales compartidos; ninguna textura readable ni muestreo CPU runtime.

Tres cortes por superficie de heladera (capacidad actual8). Ancho proyectado calibrado a ManagementFoodSize de cada alimento tanto en estante como tabla de preparación; nunca shrink de selección. Volumen conserva perspectiva y colisión de silueta por MeshCollider. Raycast al frente de la pila preserva ID exacto. Movimiento reversible0.25s con compensación de escala, tint de frescura al devolver. Fridge2D está abierta desde el primer frame, sin bisagra ficticia.

Host RectTransform central separado del frame AspectRatioFitter: FitInParent no puede expandir la heladera sobre toda la pantalla. Descartar/comprar/recovery fuera del interior y sin solaparse. Cinco PlayMode focales PASS y rendersportrait reales revisados; [QA](../ai/hybrid-fridge-qa.md). Sin APK ni profiling de teléfono en esta entrega; aprobación humana pendiente.
