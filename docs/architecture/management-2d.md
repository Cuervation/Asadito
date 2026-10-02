# Gestión — arquitectura 100 % 2D (2026-10-02)

## Decisión y límites
Asadito es definitivamente 2D: ilustraciones/sprites/capas y animaciones Canvas. Perspectiva/sombras pintadas no implican geometría. No hay modelos, cámaras/RenderTextures de mundos de gestión, mallas/colliders/raycast físicos 3D, materiales dedicados ni efectos volumétricos. El proyecto conserva su Canvas/pipeline, terceros, MCP y herramientas generales; no introduce framework, event bus ni catálogo paralelo.

## Responsabilidades
- `ManagementConfig`/`ManagementState`/`ManagementService`: fuente existente de economía, inventario, IDs, reglas, transacciones y progresión. `MvpSaveData` v5 conserva el mismo contrato; no requiere migración de datos por cambiar gráficos.
- `ManagementScreen`: navegación, carrito no persistido, selección reversible de IDs, carteles/resumen y llamadas a `QuoteCart`/`BuyCart`/`PrepareUnits`/recovery/descarte. Seleccionar no debita ni reserva; pagar revalida todo y persiste una única transacción.
- `Assets/Asado/Scripts/Management2D/ManagementFoodView.cs`, assembly `Asadito.Management2D`: targets de presentación con `Image` RAW, disposición/capas, carteles, hit testing y motion interrumpible. `Selected`/posición son estado visual temporal, nunca una copia del inventario.
- `ManagementWidthFit`: límites de ancho de safe area por cambios de Canvas; capas arcade sliced siguen sus rects. Sin reconstruir la vista en cada gesto.
- `AsaditoGame`/`FoodSpriteLibrary`: sprites RAW del mismo atlas y `ManagementFoodSize` del footprint real de parrilla; cocción simultánea, tabla/servicio, sonidos y feedback siguen existentes.

## Dibujo e interacción
Carnicería: catálogo completo de18 alimentos, cuatro tarjetas2×2 por página, cinco páginas y flechas con límites. La tarjeta muestra una ilustración del SKU RAW (no una unidad de inventario), nombre/precio/stock/cantidad y +/−. Stock0 conserva la ilustración pero indica AGOTADO, deshabilita + y toda compra vuelve a validar reglas. Se reutilizan CounterV2 y Butcher_Neutral existentes. Las tarjetas escalan el mismo Sprite uniformemente a su superficie; no duplican arte ni alteran la escala física de parrilla/heladera.

ManagementScreen mantiene un único carrito temporal mientras cambia la página del ManagementFoodView. El detalle reordena únicamente filas elegidas y permite ajustar/quitar; salir cancela el carrito no pagado. La tienda permite stockear cualquier producto desde L1 sin requisitos del pedido ni presupuesto reservado: el objetivo orienta la preparación/servicio, no restringe la compra. QuoteCart/BuyCart existentes siguen validando saldo/stock/capacidad/estado de run atómicamente.

Heladera: Fridge_Hybrid_OpenEmptyV2 abierta/vacía, frame con aspecto nativo, estantes y tabla visibles. El nombre Hybrid del recurso se conserva por estabilidad de ruta/GUID; no implica 3D. Un target por InventoryUnit.Id real, capacidad 8; tocar mueve ese mismo ID a tabla/de vuelta, sin consumo hasta PrepareUnits.

UI recibe eventos en la raíz de presentación; la selección recorre el orden de dibujo inverso y comprueba máscaras alfa precomputadas del RAW/crop. Transparentes dejan pasar; carteles/controles/overlays interceptan lo que tapan. `FoodSilhouette` usa `Assets/Asado/Resources/Definitions/FoodHitMasks.json`, generado desde el alfa RAW por `Tools/generate_food_hit_masks.py` (18 máscaras binarias de 128×64, sin arte nuevo). Los atlas permanecen comprimidos/no-readable: no GetPixel runtime ni dependencia de alphaHitTestMinimumThreshold sobre textura CPU-readable.

Cada gesto tiene un puntero dueño. Tap suma una unidad validada; drag crea un preview 2D único, no intercepta raycasts y solo drop sobre el canasto añade. Otro dedo, cancelación, release fuera/sobre UI, cierre o disable cancelan sin compra; release de drag no duplica click. Fuente/resting scale se recuperan. Movimiento/pulso/alpha breves y cancelables; reemplazar corutinas o cerrar limpia previews sin afectar monedas/IDs.

## Auditoría y decisiones de recursos
| Grupo | Decisión |
|---|---|
| CounterV2, Fridge_Hybrid_OpenEmptyV2, patio y moneda | Reutilizar las referencias de ManagementArt, sumando Butcher_Neutral existente (cinco en total); preservar archivos/meta/GUID y aspectos nativos. |
| FoodCatalog, 18 atlas/108 estados, FoodSpriteLibrary | Reutilizar RAW exacto y medidas por corte; no duplicar/regenerar catálogo. |
| Parrilla/tabla, retratos, 12 postales, fuentes/botones | Conservar presentación 2D y contratos táctiles actuales; sin botones de flip/bandeja/servir ni carbón/ignición. |
| ManagementWorldView/assembly Management3D | Sustituir por ManagementFoodView/Management2D sin compatibilidad 3D ni negocio duplicado. |
| Resources/Management3D: chorizo/tira y *-grill-relief | Retirar cuatro mesh assets exclusivos de Asadito y metadata tras quitar consumidores. |
| ManagementMeshes, ManagementFoodRelief, Editor/ManagementModelBake | Retirar generadores y bake 3D sin consumidores. |
| ManagementSurface, ManagementGlass, ManagementFoodTexture shaders | Retirar shaders exclusivos de gestión 3D y metadata; no materiales 3D residuales. |
| Unity MCP, paquetes, Tools/validate_food_content.py, editor general | Preservar; son ajenos a los recursos 3D exclusivos. |

La auditoría encontró cargas 3D por nombre en la vista/bake y referencias del assembly en tests; no consumidores serializados por GUID de los recursos retirados. La escena base ya tiene cámara ortográfica y luz 2D sin volumen; Camera en cálculo de coordenadas de UI no es cámara de mundo 3D. El atlas non-readable obliga a evitar muestreo alfa runtime; la máscara precomputada es metadato pequeño reutilizable, no otro catálogo.

## Contratos conservados
Precios originales chorizo100/tira180 y promociones preparadas; catálogo económico extendido a18 SKU con unlock1 y stock24/SKU, capacidad8, saldo inicial650, niveles/unlocks/estrellas, pedidos/reward único, tutorial, descarte y Caja del Asador. MvpSaveData inicializa sus arrays vacíos sin Unity APIs (seguro durante serialización); Migrate/RecordLevelResult dimensionan al catálogo vigente. Save v5/run/inventario/equipamiento/settings compatibles; selección/cancelación/cierre no duplica ni pierde inventario. EnableFreshness=false, sin tiempo real/offline, freezer/upgrades futuros desactivados.

## Verificación y alcance de evidencia
Compilación/importación, tests focalizados de presentación/punteros/stock/capacidad/IDs/cart checkout/persistencia y flujo compra → cocina → tabla → servicio. Capturas reales Unity 1080×1920 y portrait estrecho, sin recortes y con textos/targets legibles. Resultados de la carnicería paginada en [QA catálogo atendido](../ai/shop-four-cuts-qa.md); resultados originales y límites en [QA 2D](../ai/management-2d-qa.md) y [estado operativo](../current-project-state.md).

La ausencia de cámara/RT/meshes/materiales de gestión simplifica recursos, pero no se declara mejora numérica de FPS/memoria sin medir. Screenshots/tests Unity no sustituyen touch/notch/performance/aprobación artística Android. Sin APK por bloque. [Historial 3D](../art/management-3d-assets.md), QA fechadas y builds anteriores se conservan como evidencia de sus versiones, no arquitectura vigente.
