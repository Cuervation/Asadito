# Carnicería — fuente canónica

Pantalla real de compra de chorizo y tira, FoodCatalog para identidad/arte. `ProductEconomy`: FoodId, precio base, stock controlado, nivel de unlock y duración. Precio actual usa contrato Promotion. Stock disponible = stock de configuración − compras de jornada, reproducible sin RNG. Cambia al completar asado, no al entrar/salir de la tienda.
Antes de comprar se conoce pedido, invitados, hambre aproximada, puntos y favoritos desde planificación. Compra de una o más unidades valida desbloqueo, stock, capacidad, saldo y run inactivo antes de mutar. UI comunica error y mantiene saldo/inventario sin cambio; compra válida ofrece sonido/botón animado y saldo actualizado.

## Promos preparadas, no activadas en V1
Contrato de cotización cubre porcentaje, BuyNPayM (2x1/3x2 y restos), pack, oferta del día y liquidación como descuento, premium como producto sin descuento. No activar todas al comienzo. Origen/ventana/variantes premium se agregan con Vertical 3 y sus datos; no hay eventos aleatorios ahora.
Una promoción NO ignora capacidad ni frescura. Comprar excedente ocupa slots e inmoviliza saldo; no debería ser siempre óptimo. Tutorial controla stock, never imposibilita continuar por un evento obligatorio.
