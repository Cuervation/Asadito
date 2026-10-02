# Conservación — fuente canónica

Heladera básica tiene capacidad limitada en unidades/slots configurados. Vertical 1 muestra ocupación y selección; la carne guardada NO envejece todavía (`EnableFreshness=false`). Se mantiene separado ciclo de jornada del ciclo de conservación para no activar deterioro retroactivo al introducir V2.

## Vertical 2
EnableFreshness habilita avance solo al completar una jornada. Nunca reloj del dispositivo, offline ni DateTime. Edad = ciclo de conservación actual − adquisición; vida se configura por FoodId.
Fresh al adquirir, OK intermedio, ConsumeSoon al último ciclo utilizable, Spoiled al alcanzar vida. Podrida no prepara/cocina; descarte registra pérdida y nunca bloquea recovery. El modelo está probado pero no se anuncia vencimiento activo en V1.

## Vertical 5
Refrigerated → Frozen → Thawing → Ready; Spoiled como terminal descartable. Descongelar demora una jornada; nunca instantáneo ni tiempo real. Capacidad y mayor conservación se tunean al implementar, con pruebas propias; el save reserva nivel de freezer pero no implementa falsas operaciones ahora.

## Vista híbrida vigente (2026-10-02)
Heladera2D abierta/vacía con estantes y tabla de preparación ilustrados; solo la comida tiene geometría3D. Usa los mismos raw atlas, mallas y medidas canónicas por alimento de la carnicería/parrilla, sin achicar por cantidad ni al seleccionar. Cámara transparente en viewport con aspecto nativo; UI/botones no tapan los estantes.

Una pieza por InventoryUnit.Id real, hasta la capacidad8 actual, en tres superficies de almacenamiento. MeshCollider respeta la silueta y el raycast elige la superficie visible más cercana cuando hay superposición, no una caja invisible de otra unidad. Toque selecciona/deposita en la tabla, devolución/cancelación son reversibles; PrepareUnits consume exactamente los IDs elegidos. Sin simulación Rigidbody ni catálogo paralelo. Stock de carnicería24 por alimento no aumenta capacidad de heladera.

Originales de arte conservados; Fridge_Hybrid_OpenEmptyV2 es el escenario versionado activo. Frescura, economía, recovery, inventario y save no cambian. Aprobación artística y QA táctil/performance Android pendientes.
