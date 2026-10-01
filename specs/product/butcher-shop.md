# Carnicería — fuente canónica

Pantalla real de compra de chorizo y tira, FoodCatalog para identidad/arte. `ProductEconomy`: FoodId, precio base, stock controlado, nivel de unlock y duración. Precio actual usa contrato Promotion. Stock disponible = stock de configuración − compras de jornada, reproducible sin RNG. Cambia al completar asado, no al entrar/salir de la tienda.
Antes de comprar se conoce pedido, invitados, hambre aproximada, puntos y favoritos desde planificación. Compra de una o más unidades valida desbloqueo, stock, capacidad, saldo y run inactivo antes de mutar. UI comunica error y mantiene saldo/inventario sin cambio; compra válida ofrece sonido/botón animado y saldo actualizado.

## Promos preparadas, no activadas en V1
Contrato de cotización cubre porcentaje, BuyNPayM (2x1/3x2 y restos), pack, oferta del día y liquidación como descuento, premium como producto sin descuento. No activar todas al comienzo. Origen/ventana/variantes premium se agregan con Vertical 3 y sus datos; no hay eventos aleatorios ahora.
Una promoción NO ignora capacidad ni frescura. Comprar excedente ocupa slots e inmoviliza saldo; no debería ser siempre óptimo. Tutorial controla stock, never imposibilita continuar por un evento obligatorio.

## Mostrador con carrito (desde nivel 1)
Vitrina ilustrada con bandejas de cortes tocables, cartel de nombre/precio, stock y cantidad seleccionada. FoodCatalog provee sprites y ProductEconomy precios/unlocks/stock. No se duplican alimentos como productos independientes ni se hornean precios en el arte.

- Tocar un corte suma una unidad; highlight, pulso breve, sonido y aviso +1. La selección es libre también en L1; el pedido orienta, no fuerza compras.
- Carrito visible: cantidades, subtotales, total, +/− y VACIAR. Saldo visible en HUD. No debita ni reserva stock/inventario hasta pagar.
- PAGAR Y SALIR es una confirmación única del carrito completo y conduce a HELADERA con aviso de éxito. QuoteCart valida sin mutar; BuyCart revalida todo antes del único débito, entrega todas las unidades y registra stock. UI guarda el agregado una vez tras éxito.
- Carrito vacío o inválido deshabilita pago; mensaje explícito por saldo, stock, capacidad o run activo. Un callback de pago igualmente revalida: jamás entrega una parte del pedido si falla otra.
- VOLVER/cancelar y navegación a heladera descartan solo el carrito sin pagar; reabrir empieza vacío. La mercadería previamente comprada sigue intacta. Carrito es efímero, no nuevo campo del save.
- Inventario/prepare/recovery y economía permanecen; no promociones activadas. Heladera/Caja del Asador siguen accesibles si no se puede completar el pedido.
- Descarte de inventario conserva confirmación y no se habilita en la guía inicial.
