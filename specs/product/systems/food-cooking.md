# Sistema — alimento y cocción

## Estado del modelo

`FoodState` mantiene temperatura de centro, temperatura superficial por cara, humedad, progreso Maillard, `Char`, `FatRendered`, `SplitRisk` y exposición de las dos caras. `FoodCookingModel.Step` genérico toma `FoodCookProfile`, la temperatura uniforme de `GrillHeatModel` (210 °C inicial), tiempo simulado y si está en grill; los valores térmicos no se usan para inocuidad. No existen zonas térmicas ni brasas que mover.

## Catálogo y etapas

El JSON `Resources/Definitions/FoodCatalog.json` (schema 2) describe 18 entidades únicas. Tasa de transferencia superficial/central, cooling, humedad, Maillard, char, grasa, umbrales, masa térmica, espesor, calor preferido/fuerte, split risk, balance de cara y bandas de punto están definidos por alimento. `UsesCheeseStages` delega solo la lectura térmica del queso a estados propios; los otros cortes usan el motor común.

El catálogo schema 2 también define escala cenital/huella mediante un multiplicador de área relativo al sprite actual de chorizo (unidad 1.0). El aspecto original de cada recorte se conserva; la misma huella dimensiona placement/solapamiento y límites del drag, mientras que el hit target añade sólo un margen táctil moderado. Este dimensionamiento es visual y espacial: no altera masa, espesor, transferencia térmica ni scoring.

Cada PNG vertical de seis filas aporta RAW, WARMING, BROWNING, IDEAL, OVERCOOKED, BURNT; recortes se crean en `FoodSpriteLibrary`, se cargan por ID lazy y se cachean para presentación. Un `FoodDefinition` clonado no comparte arrays mutables.

## Punto y caras

Las cinco bandas (`Jugoso` a `Bien cocido`) se balancean por perfil; por ejemplo el chorizo, queso, morcilla, cerdo y pollo no heredan idénticos umbrales. Por decisión del usuario (2026-09-30), se cocina de un solo lado: tap selecciona, sin voltear. El modelo de dos caras permanece por compatibilidad, pero la interacción no cambia la cara expuesta. Stage ideal se comunica por arte más estado; la puntuación evalúa calidad/punto por temperatura/variables configuradas.

## Verificación

`FoodCatalogTests` verifica IDs, diferencias/perfiles válidos, copias aisladas, seis sprites por atlas, y simulaciones que alcanzan Warming/Browning/Ideal/Overcooked/Burnt. Hay test de riesgo de rotura de morcilla y progresión/uso de los 18 alimentos. PlayMode comprueba caras independientes/flip y ciclo completo de los 12 niveles. Validador externo revisa assets/dimensiones/transparencia.

## Diez etapas runtime (2026-09-30)

RAW → EARLY_WARMING → WARMING → EARLY_BROWNING → BROWNING → IDEAL → DRYING → OVERCOOKED → CHARRING → BURNT. Se conservan los seis dibujos originales; cuatro composiciones al 50% entre etapas adyacentes aportan transiciones visuales intermedias, sin alterar proporciones ni perfiles térmicos. Provoleta usa sus propios umbrales. Los diez estados no cambian las cinco bandas de punto solicitadas por los comensales.

## Barra fina por pieza (2026-10-02)
Cada corte sobre la parrilla lleva una mini barra horizontal debajo, del ancho de su arte y grosor8px (relleno6px) en Canvas1080. Relleno y color evolucionan rojo→amarillo→verde→amarillo→rojo por estado/profile individual, no tiempo fijo ni pieza seleccionada. Verde referencia banda A_Punto; queso usa etapa Ideal con dorado. Char/sequedad anticipan advertencia y quemado llena rojo. Ayuda de juego, no regla de puntuación ni indicador de inocuidad. Se oculta en crudos/tabla, acompaña drag, no toma eventos táctiles y reinicia/pausa con la comida.
