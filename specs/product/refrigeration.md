# Conservación — fuente canónica

Heladera básica tiene capacidad limitada en unidades/slots configurados. Vertical 1 muestra ocupación y selección; la carne guardada NO envejece todavía (`EnableFreshness=false`). Se mantiene separado ciclo de jornada del ciclo de conservación para no activar deterioro retroactivo al introducir V2.

## Vertical 2
EnableFreshness habilita avance solo al completar una jornada. Nunca reloj del dispositivo, offline ni DateTime. Edad = ciclo de conservación actual − adquisición; vida se configura por FoodId.
Fresh al adquirir, OK intermedio, ConsumeSoon al último ciclo utilizable, Spoiled al alcanzar vida. Podrida no prepara/cocina; descarte registra pérdida y nunca bloquea recovery. El modelo está probado pero no se anuncia vencimiento activo en V1.

## Vertical 5
Refrigerated → Frozen → Thawing → Ready; Spoiled como terminal descartable. Descongelar demora una jornada; nunca instantáneo ni tiempo real. Capacidad y mayor conservación se tunean al implementar, con pruebas propias; el save reserva nivel de freezer pero no implementa falsas operaciones ahora.

## Vista 100 % 2D vigente (2026-10-02)
`Fridge_Hybrid_OpenEmptyV2` conserva su nombre/ruta/GUID, pero es una ilustración 2D de heladera abierta vacía, no una arquitectura híbrida. Estantes y tabla se mantienen dentro de un frame de aspecto nativo; botones de navegación/descarte no tapan alimentos.

Cada `Image` RAW corresponde a una unidad real con su `InventoryUnit.Id`, hasta capacidad 8, repartida entre tres superficies. Comparte sprite y tamaño canónico con carnicería/parrilla, sin miniaturizar por cantidad ni al seleccionar. Orden de dibujo y máscaras alfa precomputadas seleccionan la pieza visible al superponerse; no hay collider, raycast físico, mesh, material o cámara/RenderTexture de gestión.

Toque traslada el mismo ID visualmente a la tabla mediante movimiento/escala 2D breve; otro toque, cambio de selección o CANCELAR lo devuelve al estante. La selección no consume ni reserva inventario; `PrepareUnits` valida y consume exactamente los IDs elegidos en una transacción. Cerrar/volver/reanudar no duplica ni pierde unidades. Animaciones interrumpibles se limpian con la pantalla.

Frescura, descarte confirmado, tutorial, Caja del Asador, economía, capacidad y save siguen intactos. `EnableFreshness=false`; sin reloj real/offline, freezer ni upgrades nuevos. Originales de arte y GUID de recursos reutilizados preservados. Compilación/tests/capturas de esta migración se registran por separado; aprobación artística y QA táctil/performance Android siguen pendientes.
