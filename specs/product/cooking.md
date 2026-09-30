# Cocción de gameplay

La fuente normativa térmica es [`systems/fire-heat.md`](systems/fire-heat.md) junto con [`systems/food-cooking.md`](systems/food-cooking.md); la matriz autoritativa editable es [`Assets/Asado/Resources/Definitions/FoodCatalog.json`](../../Assets/Asado/Resources/Definitions/FoodCatalog.json). La parrilla ya está caliente al comenzar (210 °C uniforme por `GrillHeatModel`) y `FoodCookProfile` impulsa estados térmicos por cara; el tiempo por sí solo no define calidad ni victoria. Todos los valores son tuning de gameplay, no instrucción de inocuidad.
