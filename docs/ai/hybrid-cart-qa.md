# Carnicería híbrida y changuito — QA 2026-10-01

## Implementación
Mostrador ilustrado vacío CounterV2 existente, encima cámara/RenderTexture transparente con modelos/colliders reales. Mallas nativas cerradas de relieve a partir del raw atlas de parrilla: superficie elevada, base y laterales, UV originales; no se editó ni duplicó ninguna textura de comida. Material compartido por SKU. Heladera comparte estas piezas, sin cambiar economía/guardado/preparación exactID/parrilla/térmica.

Changuito vectorial nativo UI, contenido visible y detalle ajustable. Arrastre desde physics raycast crea un único preview UI con el mismo sprite crudo; solo al soltar en canasto válido cotiza/agrega una unidad. Drop fuera/en botón/otro dedo/cerrar cancela. Release no agrega otra vez por click; toque simple queda alternativa accesible. Pago sigue BuyCart transacción atómica persistida. VACIAR cierra detalle y vuelve a habilitar acceso visual al mostrador. Sin clones3D de vuelo: solo pulso0.18s del original. RT sin MSAA y batching deshabilitado en shaders con coordenadas locales. No se confirmó causaGPU del efecto Android original; se retiró la ruta que producía clones.

## Pruebas y evidencia
- Unity6000.6.3f1, QA aislado /tmp/Asadito-concurrent-cooking-qa; import/compile y bake final PASS. Mallas2656v/2920v, total22,304 triángulos de comida con16piezas,16 MeshRenderers activos. No significa FPS medido.
- Primera corrida7focales:4/7PASS. Detectó layout basado en aspecto inicial (bounds/carteles recortados), detalle aún expandido después de vaciar y helper de test que pedía botón enabled para comprobar disabled. Corregidos antes del cierre.
- Cierre **7/7PASS**,26.769s: /tmp/asadito-hybrid-closure.xml, log /tmp/asadito-hybrid-closure.log. Física de16targets/atlas exacto/UV/geometría no plana, cambio a780×1100, tamaños de botones/legibilidad; drag cancel/drop una vez/multitouch/botón/saldo0/30taps rápidos sin acumulación; carrito ajustar/vaciar/pago/reload; saldo/stock/capacidad y atomicidad; heladera unidades exactas/cancelación/reopen/prepare; stock0/heladera vacía sin unidades ficticias.
- Capturas Unity1080×1920 reales inspeccionadas: build/qa-hybrid-cart/asadito-hybrid-counter-cart.png, asadito-hybrid-fridge.png, asadito-hybrid-rapid-taps.png. La de spam cambia saldo sintético en memoria sin refrescar HUD: no representa economía de una partida humana.
- Diff check limpio; whitespace de los assets nuevos normalizado con tokens serializados idénticos. Sin suites completas ni builds adicionales: no se modificó dominio económico/cocción.

## Límites y entrega
Cambios locales en proyecto, sin commit/push solicitados en este mensaje. Arte original y trabajo local previo preservados. No APK ni instalación nueva: Motorola conserva1.4.0/code6 de la carnicería anterior. Validación Android touch/performance de esta variante pendiente; se necesita build nueva antes de evaluarla en celular.
