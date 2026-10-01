# Conservación — fuente canónica

Heladera básica tiene capacidad limitada en unidades/slots configurados. Vertical 1 muestra ocupación y selección; la carne guardada NO envejece todavía (`EnableFreshness=false`). Se mantiene separado ciclo de jornada del ciclo de conservación para no activar deterioro retroactivo al introducir V2.

## Vertical 2
EnableFreshness habilita avance solo al completar una jornada. Nunca reloj del dispositivo, offline ni DateTime. Edad = ciclo de conservación actual − adquisición; vida se configura por FoodId.
Fresh al adquirir, OK intermedio, ConsumeSoon al último ciclo utilizable, Spoiled al alcanzar vida. Podrida no prepara/cocina; descarte registra pérdida y nunca bloquea recovery. El modelo está probado pero no se anuncia vencimiento activo en V1.

## Vertical 5
Refrigerated → Frozen → Thawing → Ready; Spoiled como terminal descartable. Descongelar demora una jornada; nunca instantáneo ni tiempo real. Capacidad y mayor conservación se tunean al implementar, con pruebas propias; el save reserva nivel de freezer pero no implementa falsas operaciones ahora.
