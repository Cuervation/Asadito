# Inventario y preparación — fuente canónica

## Unidad
`InventoryUnit`: Id estable, FoodId, costo realmente pagado, ciclo de adquisición, origen recovery. La identidad/nombre/perfil NO se duplican: FoodCatalog. Una unidad ocupa un slot; cantidades se derivan contando unidades. Promos reparten costo exacto con resto entre unidades.
`ManagementState` guarda balance, ciclos, unidades, compras de la jornada, estado de equipamiento y ActiveRun. Frescura se deriva de edad + vida por producto; no guardar estados duplicados.

## Transiciones
Comprar → heladera. Tocar unidades 3D individuales → bandeja de preparación; volver a tocar o CANCELAR devuelve la selección sin consumir inventario. Confirmar → extraer únicamente las unidades seleccionadas y persistir ActiveRun → construir las porciones existentes de gameplay. Carne no seleccionada permanece guardada. Cantidad preparada inicial = cantidad del pedido; se puede cambiar mezcla chorizo/tira y asumir consecuencias de gustos/puntos.
No cocinar carne podrida ni consumir la misma unidad dos veces. Confirmación atómica: o todas las unidades seleccionadas existen o nada se consume. Mientras hay run, no comprar/descartar simultáneamente.
Abandonar un asado preparado registra el costo como pérdida pendiente. Reiniciar su cocción usa el mismo run sin duplicar inventario. Tras cierre inesperado se conserva run; volver a elegir su nivel reanuda las mismas unidades preparadas sin comprar otra vez. La cocción reinicia fría (no snapshot térmico en V1), igual que retry. Solo VOLVER desde gameplay/pausa o iniciar otro asado abandona y registra pérdida; entrar al selector tras abrir la app NO descarta carne. El siguiente pedido sigue siendo posible mediante recovery.
Descartar una unidad registra pérdida persistente y pendiente de resultado. No destruir progreso para recuperar una partida.

## Save
MvpSave v5 migra v1–4 conservando estrellas, mejores scores, unlock y settings. Gestión ausente se inicializa una sola vez con tuning; nunca rellenar monedas en cada load. Arrays se normalizan como antes; ids próximos superan los existentes. JSON roundtrip preserva run/inventario/balance. PlayerPrefs es el almacenamiento local existente; no backend ni sincronización nueva.

## Futuro
Lotes grandes, storage freezer/thawing y reservas se añaden solo al implementar su vertical; no copiar catálogos térmicos ni crear inventario paralelo por pantalla.

## Representación 3D vigente
Heladera con puerta animada, interior y estantes; cada modelo corresponde exactamente a InventoryUnit.Id. Sin unidades decorativas ficticias. Posiciones automáticas (cuatro slots por estante), capacidad configurable y modelos compartidos con carnicería. Selección temporal reversible, sin reserva ni modificación del save hasta confirmar. PrepareUnits/CanPrepareUnits validan IDs únicos, existentes, no podridos, desbloqueados y run inactivo; consumen exactamente esos IDs en una sola transacción. Prepare por SKU conserva FIFO y delega a la misma transacción. Carne no elegida permanece; preparación/reanudación/recovery/resultados conservan contratos anteriores.

Heladera vacía comunica compra/recovery, no botones de SKU como sustituto de mercadería. Frescura queda representable por datos/materiales sin avanzar ciclos ni activar deterioro offline. Upgrades/freezer/thawing no se activan.
