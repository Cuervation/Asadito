# Sistema — alimento y cocción

## Estado del modelo

`FoodState` mantiene temperatura de centro, temperatura superficial por cara, humedad, progreso Maillard, `Char`, `FatRendered`, `SplitRisk` y exposición de las dos caras. `FoodCookingModel.Step` genérico toma `FoodCookProfile`, la temperatura uniforme de `GrillHeatModel` (210 °C inicial), tiempo simulado y si está en grill; los valores térmicos no se usan para inocuidad. No existen zonas térmicas ni brasas que mover.

## Catálogo y etapas

El JSON `Resources/Definitions/FoodCatalog.json` (schema 1) describe 18 entidades únicas. Tasa de transferencia superficial/central, cooling, humedad, Maillard, char, grasa, umbrales, masa térmica, espesor, calor preferido/fuerte, split risk, balance de cara y bandas de punto están definidos por alimento. `UsesCheeseStages` delega solo la lectura térmica del queso a estados propios; los otros cortes usan el motor común.

Cada PNG vertical de seis filas aporta RAW, WARMING, BROWNING, IDEAL, OVERCOOKED, BURNT; recortes se crean en `FoodSpriteLibrary`, se cargan por ID lazy y se cachean para presentación. Un `FoodDefinition` clonado no comparte arrays mutables.

## Punto y caras

Las cinco bandas (`Jugoso` a `Bien cocido`) se balancean por perfil; por ejemplo el chorizo, queso, morcilla, cerdo y pollo no heredan idénticos umbrales. En el paso se calienta la cara expuesta; flip alterna cara y conserva estado. Stage ideal se comunica por arte más estado; la puntuación evalúa calidad/punto por temperatura/variables configuradas.

## Verificación

`FoodCatalogTests` verifica IDs, diferencias/perfiles válidos, copias aisladas, seis sprites por atlas, y simulaciones que alcanzan Warming/Browning/Ideal/Overcooked/Burnt. Hay test de riesgo de rotura de morcilla y progresión/uso de los 18 alimentos. PlayMode comprueba caras independientes/flip y ciclo completo de los 12 niveles. Validador externo revisa assets/dimensiones/transparencia.
