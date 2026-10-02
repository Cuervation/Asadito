# Carnicería — fuente canónica

## Entorno 3D (decisión vigente)
Carnicería y heladera son escenarios tridimensionales reales con cámaras fijas elevadas y UI móvil superpuesta. La parrilla conserva su Canvas, input y motor térmico. No cards de productos ni sprites planos como mercadería final.

Mostrador de madera/metal con vidrio de bajo costo, bandejas y modelos independientes de chorizo/tira. Varias unidades según stock (hasta ocho por SKU visible); stock cero deja la bandeja vacía y cartel AGOTADO. Modelos comparten identidad FoodCatalog, geometría/materiales entre tienda e inventario, sin duplicar catálogo económico. Más cortes se registran por fábrica/modelo y se paginan, sin inventar variantes de venta.

Cartel físico dinámico: nombre, precio, stock y cantidad en carrito desde ManagementConfig/ManagementService, nunca horneados en modelos. Toque mediante raycast real desde viewport táctil; overlays interceptan su propia área. Pulso/highlight, vuelo decorativo breve y sonido existente; entrada no bloquea taps posteriores.

## Compra y carrito
- Pedido/invitados/puntos/favoritos se conocen desde planificación. Carrito compacto inferior expandible con cantidades, subtotales, total, +/− y VACIAR; saldo visible. Tocar añade una unidad solo si QuoteCart permite la canasta propuesta.
- Cotización no debita ni reserva stock/inventario. PAGAR Y SALIR revalida saldo, stock, capacidad, desbloqueos y run inactivo mediante BuyCart antes de mutar; nunca cobra/entrega parcialmente. La UI persiste una vez tras éxito y abre heladera con puerta animada y aviso.
- Volver o ir a heladera cancela solamente la canasta sin pagar. Las compras persistentes siguen intactas; carrito no es nuevo campo de save.
- Stock = configuración menos compras de jornada; cambia al completar asado, no al entrar/salir. Precios de ProductEconomy, no precios propios de prefabs.
- Tutorial orienta sin forzar compras; Caja del Asador evita bloqueo cuando no se puede completar pedido. Descarte confirmado sigue restringido en guía inicial.

## Promos preparadas, no activadas
Promotion mantiene porcentaje, BuyNPayM, pack, oferta diaria, liquidación y premium. Ninguna promoción activa en esta vertical; nunca ignora capacidad/frescura. No RNG ni eventos obligatorios nuevos.
