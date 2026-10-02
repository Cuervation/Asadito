# Vertical 1 — aceptación

- Save v1–4 migra a v5 y conserva progreso/settings; balance/inventario no se reinicializan.
- L1–6 conectan loop de gestión desde el primer asado, L7+ siguen locked.
- Pedido antes de comprar: invitados, hambre, puntos/gustos, cantidades.
- Chorizo/tira: compra real descontando saldo/stock, heladera limitada; error transaccional sin mutación.
- Selección explícita y preparación sin duplicar unidades. No cocinar podrida; conservación aún inactiva en V1.
- Toda porción en parrilla cocina simultáneamente con perfil propio; tabla/servicio reutilizados.
- Resultado distingue asador/gestión/operación y costos/reward/saldo; reward único y progreso guardados juntos.
- Recovery con saldo cero, faltante o capacidad/stock sin salida; reducción explícita, no bloqueo.
- Reiniciar app conserva unidades preparadas y permite reanudación fría sin recomprar. Salir explícitamente de gameplay registra pérdida de preparación; no recompensa gratuita por retry ni duplicado de inventory.
- Import/compilación Unity, tests dominio relacionados, suite razonable core y PlayMode end-to-end.
- Renders portrait revisados (texto legible, sin food overlay sobre resultado). Android una vez al estabilizar; device solo si disponible.
- Specs por dominio + estado/roadmap actualizados. Sin activar verticales futuras ni funciones prohibidas.

- L1: selección de un chorizo y una tira en el mostrador, confirmación conjunta PAGAR Y SALIR, descuento real, heladera/selección/preparación, cocina/servicio y primer ingreso; tutorial no se repite tras completarlo.
- Todos los seis niveles: ingresos/costos/reward/progreso persistidos; L7–12 continúan bloqueados.
- Gestión perfecta con crudo/quemado o hambre no gana estrella; tiers superiores exigen mejor ASADOR.
- Savev4 conserva balance, inventario, unidades preparadas, estrellas y bonus histórico; no reinicia ni repaga bonus.

## Mostrador / carrito
- Seleccionar, sumar, restar, vaciar y cancelar no modifican wallet/inventario/stock/IDs.
- Checkout valida canasta completa por saldo, stock, capacidad, unlock y run activo antes de mutar. Revalida tras una previsualización.
- Compra exitosa persiste unidades/costos y saldo; conduce a heladera y permite preparar. Recarga conserva unidades y no crea un ActiveRun vacío que bloquee operaciones.
- Precio y total legibles en portrait, cortes tocables con feedback y botones arcade existentes.

## Migración definitiva 100 % 2D (2026-10-02)
- Pantallas activas de gestión en Canvas, sin modelos/MeshRenderer/MeshCollider/raycast físico ni cámaras, materiales o RenderTextures de mundos 3D. Recursos exclusivos 3D de Asadito sin referencias retirados; terceros/MCP y GUID reutilizados preservados.
- Mostrador completo24+24, parcial y agotado refleja stock real; todos los cortes usan RAW/tamaño por alimento de parrilla, abundantes sin miniaturización por cantidad.
- Picking por orden visual y silueta alfa: tocar superposición elige lo expuesto, transparente deja pasar y cartel/UI no compra alimento oculto.
- Tap suma una unidad cotizada; drag al carrito suma exactamente una. Cancelación, fuera, botón, segundo dedo, cierre y disable no compran ni dejan previews. Release de drag no duplica tap.
- Nombre/precio/stock/llevás/total dinámicos, contenido y detalle de carrito, cantidades/VACIAR/checkout único conservados; selección no debita y pago revalida/persiste atómicamente.
- Heladera máxima8: una imagen por ID real, selección/devolución/cambio/cancelación reversible, `PrepareUnits` consume IDs exactos sin duplicar ni perder unidades al volver/reabrir/reanudar.
- Compra → heladera → parrilla → tabla → servicio/persistencia conserva contratos. Frescura offline, freezer y nuevos controles de comida siguen deshabilitados.
- Compile/import y tests focalizados por bloque; ampliar solo por evidencia. Capturas reales Unity 1080×1920 y portrait más estrecho revisadas: alimentos/carteles/buttons accesibles, sin recortes.
- Registrar pruebas y límites reales; screenshots Unity no equivalen a FPS/touch/safe area medidos en teléfono. Sin APK por cada cambio.
