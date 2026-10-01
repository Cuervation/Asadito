# Economía — fuente canónica

## Vertical 1
Saldo entero persistente; todas las compras consumen monedas reales del save. `Wallet` impide gastos negativos o superiores al saldo. Compra/inventario/stock se mutan en una sola transacción validada y se guardan juntos.
Tuning único: `Assets/Asado/Resources/Definitions/ManagementConfig.json`: saldo inicial, precios/stock, capacidad, recompensas, límites de recovery, pesos y penalizaciones. No precios por alimento en UI.

## Evaluación
ASADOR conserva CookingQuality/Satiety/DonenessMatch/FoodPreference; su media se normaliza a 0–100. GESTIÓN compara desperdicio con costo usado y penaliza suavemente capital inmovilizado en sobrantes (no trata carne guardada como podrida). OPERACIÓN mide pérdida/descarte. General combina pesos configurados, inicialmente 60/25/15.
Ingreso = (base + invitados × recompensa por invitado) × factor general interpolado entre mínimo y máximo × factor recovery. Dificultad inicial se refleja en tamaño del pedido; bonus/objetivos específicos se incorporarán como datos, no excepciones en UI.
Ganancia del asado = ingreso − costo de unidades preparadas − pérdida pendiente. La compra de sobrantes ya debitó el saldo; no se vuelve a debitar al resultado. La UI muestra saldo, gasto utilizado, desperdicio, ingresos, ganancia y score separados. Costos de stock retenido no se confunden con gasto de este asado.
Recompensa solo si hay ActiveRun; completar elimina el run. Nunca reclamar dos veces. Guardar recompensa y progreso en el mismo agregado. El abandono consume carne ya preparada y registra pérdida, no devuelve dinero ni paga.
Nivel 4 presenta una recompensa inicial única; niveles anteriores no son una granja gratuita de monedas.

## Caja del Asador
Disponible si falta carne y no se puede completar el pedido por dinero, stock o capacidad. Usa primero unidades válidas propias y completa SOLO las faltantes; comienza directamente el pedido (sin excedente en heladera ni monedas regaladas). Recompensa reducida y máximo de estrellas configurable. Nunca limita cantidad de recuperaciones ni bloquea jugar. Si hay stock/balance/capacidad suficiente no se ofrece ayuda. El jugador también puede elegir una combinación más barata mediante preparación libre.

## Futuro
Upgrades y bonus por objetivos se añaden al mismo archivo de configuración cuando haya funcionalidad. No simular compras de mejoras inexistentes. Calibrar inflación y margen con playtesting humano; el resultado de la primera vertical es tuning inicial, no balance definitivo.
