# Carnicería: vitrina abundante — QA 2026-10-01

## Resultado y alcance
Se reemplazaron las dos grandes tarjetas por una superficie ilustrada continua de metal y vidrio. Con stock inicial hay 16 piezas táctiles (hasta ocho por SKU), con posiciones, sombras y giros variados. Los cartelitos muestran nombre, precio, stock y cantidad seleccionada. El carrito pasó de 456 a 256 unidades de alto; total y acciones quedan fuera de sus filas. Seleccionar actualiza la escena sin reconstruirla, con pulso, +1, sonido existente y vuelo decorativo al carrito. Más de dos productos habilitan scroll horizontal de vitrina y vertical de filas.

Wallet, ManagementConfig, ManagementService, cotización, compra atómica, stock, capacidad, inventario y persistencia no cambiaron. Las piezas visuales no duplican productos ni reservan inventario. Stock cero presenta una referencia deshabilitada con AGOTADO.

## Archivos de esta tarea
- `Assets/Asado/Scripts/ManagementScreen.cs`: escena, interacción, carrito compacto y scroll.
- `Assets/Asado/Scripts/Editor/ManagementArtImportSettings.cs`: importación del nuevo fondo.
- `Assets/Asado/Resources/ManagementArt.asset`: referencia de CounterV2.
- `Assets/Asado/Art/Management/ButcherShop/ButcherShop_CounterV2.png` y `.meta`: nuevo fondo; original conservado.
- `Assets/Tests/PlayMode/FirstPlayableFlowTests.cs`: dos pruebas nuevas de raycasts/actualización y extensibilidad.
- `specs/product/butcher-shop.md`, `docs/art/asset-manifest.md`, `docs/art/management-asset-manifest.md`, `docs/current-project-state.md`, este documento.

Se preservaron los cambios de entrega anteriores en AsaditoGame.cs, AndroidReleaseBuild.cs, ProjectSettings.asset, android-release.md y current-project-state.md; no se hizo commit ni push.

## Validación proporcional
Unity 6000.6.3f1, proyecto QA importado `/tmp/Asadito-concurrent-cooking-qa`, sin cerrar el Editor principal.

- `/tmp/asadito-abundant-counter-flow.xml`: 2/2 PASS, 7.0336 s (flujo y errores).
- `/tmp/asadito-abundant-counter-closure.xml`: 3/3 PASS, 10.6135 s (flujo, errores y selección real).
- `/tmp/asadito-abundant-counter-scroll.xml`: 1/1 PASS, 2.8197 s (scroll con cuatro productos sintéticos, configuración restaurada al finalizar).
- Logs respectivos con extensión `.log`. Cuatro casos distintos aprobados; compilación/importación incluidas en estas ejecuciones, no suite completa.
- Raycaster comprobó los 16 centros táctiles. Dos eventos reales de pointer click verificaron cantidad, total, conservación del root, animación y limpieza. Textos comprobados sin recorte de altura.
- Compra/cancelación/ajustes, saldo/stock/capacidad, ausencia de cargos parciales y recarga hacia heladera verificados por los dos casos existentes.
- `python3 Tools/validate_food_content.py`: PASS, 18 perfiles, 108 frames, footprints/layouts L1–L12 y arte/fuentes presentes.
- `git diff --check`: PASS.

Captura Unity 1080×1920 inspeccionada: `/Users/celestino/Asadito/build/qa-abundant-counter/asadito-abundant-counter-cart.png`. En el mismo directorio: vacío, compra en heladera y saldo insuficiente. Son previews ignoradas por Git, no assets del juego.

## Arte generado
Modo: ImageGen integrado, edición de referencia en dos pasadas; primera composición descartada por mostrador demasiado bajo. Fondo final PROVISIONAL pendiente de aprobación humana: PNG RGB 941×1672, importado FullRect, máximo 2048, sin mipmaps, ETC2 HQ. Biblioteca conserva 90 PNG originales y agrega este PNG (91 total, seis referencias, 85 preparados).

Prompt final exacto:

> Edit target is the attached ASADITO illustrated empty butcher shop background. Keep warm stylized painted casual-game materials, empty single shared metal display floor, wooden base, glass side framing and warm lamp. CRITICAL COMPOSITION FIX: the first image has far too much empty wall and the counter is too low and shallow. Move and enlarge the whole counter upwards dramatically. Top edge of glass counter should start at EXACTLY about 24% of image height, not 48%. Usable open display floor must fill x=8%-92% and y=31%-66% of image height, as one broad continuous metal surface viewed at an elevated front-three-quarter angle. This clear surface will hold many large interactive food sprites. Front lip crosses at about y=70%; wooden front occupies y=71%-94%. Keep only a compressed shallow slice of shop wall at y=0%-23% with subdued cream tiles and lamp/leafy corners. Scale may peek above counter to right. The counter is the HERO taking almost 70% of the image, not background scenery. No meat, no hanging sausages, no individual trays, no product boxes, no cards, no text, no button graphics, no people, no logos. Do not retain the empty wall from the previous composition. Portrait game background, polished chunky 3D illustrative style, never photorealistic. This is production backdrop so obey the counter/floor coordinates.

## Pendiente
No se generó ni instaló APK nueva en esta tarea: el Motorola conserva la versión anterior 1.3.1/code5 con el mostrador de dos tarjetas. Falta aprobación visual humana y QA física de legibilidad/rendimiento; los productos adicionales se probaron de forma sintética y no se habilitaron comercialmente. No hay fallos focales pendientes.
