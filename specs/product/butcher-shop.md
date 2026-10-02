# Carnicería — fuente canónica

## Entorno 100 % 2D (decisión definitiva, 2026-10-02)
La carnicería conserva **ButcherShop_CounterV2** vacío como mostrador ilustrado y representa toda la comida mediante `Image`/sprites Canvas. No hay modelos, mallas, colliders físicos, cámaras de gestión, materiales ni RenderTextures de mundos 3D. La heladera comparte la presentación 2D y los mismos alimentos; ver [conservación](refrigeration.md).

Chorizo/tira usan exactamente el sprite RAW que entrega `FoodSpriteLibrary` a la parrilla, sin duplicar ni redibujar el catálogo. Hasta veinticuatro unidades por SKU según stock real (actualmente 24 por producto y jornada); stock 0 vacía el grupo y muestra AGOTADO, sin carne ficticia en el fondo. El carrito solo cotiza: las piezas representan el stock disponible, no una reserva persistente.

Las piezas conservan la medida canónica de la parrilla y las proporciones por alimento. La cantidad no determina una miniaturización uniforme. Se distribuyen en capas 2D abundantes sobre las superficies ilustradas, con superposición natural y contención. Orden de hermanos Canvas y máscaras alfa precomputadas eligen la silueta visible superior; las zonas transparentes no ocultan otras piezas. No se requiere hacer los atlas CPU-readable.

Carteles comerciales: papel blanco/banda verde, nombre, precio rojo grande, stock y llevás; todos los valores provienen de datos vivos. Su área de UI bloquea cualquier compra de carne tapada por el cartel. Changuito, contenido visible, resumen expandible, +/−, VACIAR y PAGAR Y SALIR permanecen.

Toque directo agrega una unidad únicamente si `QuoteCart` acepta la propuesta. Arrastre usa un preview RAW 2D temporal con feedback de escala/movimiento; solo soltar sobre el canasto sin una UI interceptora puede agregar una unidad. Cancelar, soltar afuera, cerrar, deshabilitar o intervenir otro dedo cancela el gesto sin compra. Un puntero dueño; ningún segundo dedo completa la operación y el release del drag no se cuenta además como tap. Animaciones cortas e interrumpibles limpian previews y restauran el alimento fuente sin clones ni consumo de inventario.

## Compra y carrito
- Pedido/invitados/puntos/favoritos se conocen desde planificación. Changuito inferior expandible con cantidades, subtotales, total, +/− y VACIAR; saldo visible. Tocar añade una unidad solo si QuoteCart permite la canasta propuesta.
- Cotización no debita ni reserva stock/inventario. PAGAR Y SALIR revalida saldo, stock, capacidad, desbloqueos y run inactivo mediante BuyCart antes de mutar; nunca cobra/entrega parcialmente. La UI persiste una vez tras éxito y abre la heladera ilustrada ya abierta con aviso.
- Volver o ir a heladera cancela solamente la canasta sin pagar. Las compras persistentes siguen intactas; carrito no es nuevo campo de save.
- Stock = configuración menos compras de jornada; cambia al completar asado, no al entrar/salir. Precios de ProductEconomy, no precios propios de prefabs.
- Tutorial orienta sin forzar compras; Caja del Asador evita bloqueo cuando no se puede completar pedido. Descarte confirmado sigue restringido en guía inicial.

## Promos preparadas, no activadas
Promotion mantiene porcentaje, BuyNPayM, pack, oferta diaria, liquidación y premium. Ninguna promoción activa en esta vertical; nunca ignora capacidad/frescura. No RNG ni eventos obligatorios nuevos.
