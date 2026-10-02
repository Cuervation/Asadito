# Carnicería — fuente canónica

## Entorno híbrido (decisión vigente)
La carnicería utiliza la ilustración **vacía** ButcherShop_CounterV2 como escenario/mostrador2D y comida volumétrica3D encima. La heladera también utiliza un escenario2D vacío con comida3D y selección por collider de silueta; ver [conservación](refrigeration.md). Parrilla/input/térmica no cambian. Esta decisión del usuario reemplaza la exigencia anterior de mostrador íntegramente3D.

Chorizo/tira comparten exactamente el atlas raw de la parrilla, mapeado sobre mallas nativas con superficie elevada, silueta, paredes laterales y base (no quads/billboards). Modelos/texturas/materiales compartidos también en heladera, sin otro catálogo. Hasta veinticuatro unidades por SKU según stock (stock diario inicial24 por producto, aprobado por el usuario); stock0 vacía el mostrador y marca AGOTADO, sin carne ficticia dibujada en el fondo.

Las piezas conservan la medida canónica de la parrilla; no se miniaturizan por cantidad ni se normalizan al mismo ancho entre productos. Se admite superposición en capas apoyadas, manteniendo perspectiva y contención en la superficie; la compra corresponde a la silueta expuesta del alimento, no a cajas invisibles que cubran otro corte.

Carteles de precio con soporte/pinza/base metálicos, papel blanco y marco, banda verde con nombre y números rojos grandes con contorno oscuro (referencia comercial del usuario). Precios de ProductEconomy por pieza, stock/llevás dinámicos; tocar el cartel no compra comida escondida detrás. Changuito visible con contenido/resumen y detalle expandible. Arrastrar desde un collider real y soltar sobre el canasto agrega una unidad si QuoteCart lo permite; fuera del canasto, sobre botones o cancelando/abandonando no agrega. Solo un puntero activo, preview UI temporal único del mismo arte; se limpia al soltar/cerrar. Toque simple sigue como alternativa accesible, pulso/sonido sin clones3D de vuelo.

## Compra y carrito
- Pedido/invitados/puntos/favoritos se conocen desde planificación. Changuito inferior expandible con cantidades, subtotales, total, +/− y VACIAR; saldo visible. Tocar añade una unidad solo si QuoteCart permite la canasta propuesta.
- Cotización no debita ni reserva stock/inventario. PAGAR Y SALIR revalida saldo, stock, capacidad, desbloqueos y run inactivo mediante BuyCart antes de mutar; nunca cobra/entrega parcialmente. La UI persiste una vez tras éxito y abre la heladera ilustrada ya abierta con aviso.
- Volver o ir a heladera cancela solamente la canasta sin pagar. Las compras persistentes siguen intactas; carrito no es nuevo campo de save.
- Stock = configuración menos compras de jornada; cambia al completar asado, no al entrar/salir. Precios de ProductEconomy, no precios propios de prefabs.
- Tutorial orienta sin forzar compras; Caja del Asador evita bloqueo cuando no se puede completar pedido. Descarte confirmado sigue restringido en guía inicial.

## Promos preparadas, no activadas
Promotion mantiene porcentaje, BuyNPayM, pack, oferta diaria, liquidación y premium. Ninguna promoción activa en esta vertical; nunca ignora capacidad/frescura. No RNG ni eventos obligatorios nuevos.
