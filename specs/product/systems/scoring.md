# Sistema — puntuación

Cada porción servida se evalúa en cuatro dimensiones explícitas: `CookingQuality` (40%), `Satiety` (30%), `DonenessMatch` (20%) y `FoodPreference` (10%). Son pesos baseline centralizados y tunables; curvas, topes y tolerancias también son parámetros de balance.

El resultado por comensal se normaliza a 0–100; el total de nivel se normaliza a 0–200 en First Playable con dos comensales y escala según el número/configuración de comensales en niveles posteriores. El desglose o la reacción debe comunicar al menos el factor de mayor impacto, de modo que el jugador pueda entender cómo mejorar.

## Verificación

- El desglose identifica las cuatro dimensiones y permite comprobar qué factor alteró el resultado.
- El mismo estado, preferencias y parámetros producen la misma puntuación.
- La UI de nivel 1 informa resultados individuales y total 0–200; retry reinicia estado, asignaciones y puntaje.

## Gestión desde nivel1
ASADOR se agrega como media de estos componentes, general60/25/15 con economía/operación. Estrellas exigen pedido completo por identidad/cantidad y mínimos independientes de cocina/saciedad/asador: ver [servicio](serving.md) y [economía](../economy.md). Cocción cruda reduce CookingQuality según primer punto del perfil; daño sigue medido individualmente. El mínimo de cocción considera todas las piezas servidas, incluso las no asignadas por no ser pedidas; un comensal sin su pedido no convierte una pieza bien cocida en cruda. El general alto no abre progreso si no cumplió el pedido o esos mínimos: dinero, cocción o gusto no compensan una sustitución.

No cambia la fórmula de ingresos del asado pagado, incluso cuando falla el pedido, ni el multiplicador/tope de estrellas de recovery y su regla de ingreso cero si no aprueba. Compra/preparación libre y persistencia conservan el contrato existente; no se modifica saldo previo ni se requiere migración.
