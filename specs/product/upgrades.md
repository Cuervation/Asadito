# Mejoras y estaciones — diseño preparado

No hay compras de upgrades activas en Vertical 1. Save reserva FridgeTier/FreezerTier/GrillTier. Costos y efectos vendrán como datos con V4, no botones sin efecto.
Cada mejora debe ser visible y funcional: heladera amplía capacidad, freezer conserva, parrilla amplía superficie, mesada prepara, patio/mesa/luces muestran progreso, asador/cruz habilitan mecánicas/cortes. Carnicería evoluciona stock/variedad/ofertas/premium.
Ruta estaciones: parrilla → asador/cruz → disco → horno de barro. Motor actual FoodCookingModel/FoodState se reutiliza; crear contrato de estación solo cuando exista segunda implementación y necesidad demostrada. No inventar un framework de calor hoy.
Arte preparado en `docs/art/management-asset-manifest.md`; referencias runtime solo para sprites de sistemas implementados. Home patio diegético es una evolución a evaluar con playtesting, no una promesa activa de navegación caminando.
