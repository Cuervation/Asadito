# Economía — fuente canónica

## Vertical 1
Saldo entero persistente; todas las compras consumen monedas reales del save. `Wallet` impide gastos negativos o superiores al saldo. Compra/inventario/stock se mutan en una sola transacción validada y se guardan juntos.
Tuning único: `Assets/Asado/Resources/Definitions/ManagementConfig.json`: saldo inicial, precios/stock, capacidad, recompensas, límites de recovery, pesos y penalizaciones. No precios por alimento en UI.

## Evaluación
ASADOR conserva CookingQuality/Satiety/DonenessMatch/FoodPreference; su media se normaliza a 0–100. GESTIÓN compara desperdicio con costo usado y penaliza suavemente capital inmovilizado en sobrantes (no trata carne guardada como podrida). OPERACIÓN mide pérdida/descarte. General combina pesos configurados, inicialmente 60/25/15.
Ingreso = (base + invitados × recompensa por invitado) × factor de rendimiento interpolado entre mínimo y máximo × factor recovery. Rendimiento = mínimo de general, asador y cocción de la peor pieza. Dificultad inicial se refleja en tamaño del pedido; bonus/objetivos específicos se incorporarán como datos, no excepciones en UI.
Ganancia del asado = ingreso − costo de unidades preparadas − pérdida pendiente. La compra de sobrantes ya debitó el saldo; no se vuelve a debitar al resultado. La UI muestra saldo, gasto utilizado, desperdicio, ingresos, ganancia y score separados. Costos de stock retenido no se confunden con gasto de este asado.
Recompensa solo si hay ActiveRun; completar elimina el run. Nunca reclamar dos veces. Guardar recompensa y progreso en el mismo agregado. El abandono consume carne ya preparada y registra pérdida, no devuelve dinero ni paga.
Todos los niveles1–6 compran/preparan unidades pagadas y cobran al completar. No hay carne gratuita ni bonus L4 nuevo. Se conserva `IntroRewardGranted` histórico sin pagar ni retirar nada. Volver a jugar no restablece saldo: hay que utilizar inventario propio o comprar nuevamente.

### Mínimos culinarios y tuning inicial
Pesos general60/25/15 conservados, pero al menos1estrella requiere Asador≥55, cocción de cada pieza servida≥50 y saciedad media≥65. Dos estrellas requieren Asador≥70; tres≥85, además del general configurado. Cocción incorpora avance hacia el primer punto válido del perfil: crudo no recibe100 por no estar dañado. Carne muy cruda/quemada cuenta como pérdida de su costo realmente pagado, ya incluido en carne utilizada; no se descuenta dos veces en ganancia. Reacciones y consejo explican el principal problema.
Saldo nuevo650; precio chorizo100/tira180; ingreso máximo80+150porinvitado, mínimo20% según rendimiento. Pedido L1 cuesta280, saldo postcompra370; un buen asado obtiene margen moderado. L6 sugerido cuesta660. Balance anterior nunca se cambia a650. El margen depende de corte/score; llenar heladera de tiras no siempre es óptimo.

## Caja del Asador
Disponible si falta carne y no se puede completar el pedido por dinero, stock o capacidad. Usa primero unidades válidas propias y completa SOLO las faltantes; comienza directamente el pedido (sin excedente en heladera ni monedas regaladas). Recompensa reducida y máximo de estrellas configurable. Si no cumple mínimos culinarios no cobra; puede volver a usar ayuda. Nunca limita cantidad de recuperaciones ni bloquea jugar. Si hay stock/balance/capacidad suficiente no se ofrece ayuda. El jugador también puede elegir una combinación más barata mediante preparación libre.

## Futuro
Upgrades y bonus por objetivos se añaden al mismo archivo de configuración cuando haya funcionalidad. No simular compras de mejoras inexistentes. Calibrar inflación y margen con playtesting humano; el resultado de la primera vertical es tuning inicial, no balance definitivo.
