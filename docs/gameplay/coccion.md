# Cocción de gameplay

El motor `FoodCookingModel` avanza temperatura central/superficial, humedad, Maillard, char, grasa rendida y SplitRisk con transferencia térmica propia por perfil JSON. Cada cara guarda su exposición térmica; al voltear vuelve visible/procesable el estado previo de la otra cara. Las seis etapas de arte (RAW → WARMING → BROWNING → IDEAL → OVERCOOKED → BURNT) no sustituyen puntuación térmica. Provoleta resuelve estados específicos de queso. Todo coeficiente y °C son tuning arcade, no guía de seguridad alimentaria.

Especificación: [`food-cooking.md`](../../specs/product/systems/food-cooking.md); matriz y perfiles: [`food-catalog.md`](food-catalog.md).

## Ajuste vigente — 2026-09-30

Se desactiva dar vuelta por ahora. Tap sólo selecciona y no hay penalización por la cara inactiva. Cada alimento tiene diez etapas runtime (Raw, EarlyWarming, Warming, EarlyBrowning, Browning, Ideal, Drying, Overcooked, Charring, Burnt): seis dibujos existentes y cuatro transiciones superpuestas al 50%. No cambia la cantidad de lugares en parrilla ni las cinco bandas de punto del pedido. Este ajuste reemplaza las referencias anteriores a flip y seis etapas runtime.
