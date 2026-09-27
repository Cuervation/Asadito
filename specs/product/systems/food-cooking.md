# Sistema — alimento y cocción

## Estado

Cada pieza mantiene `FoodState` con temperatura de centro, temperatura de superficie, humedad, progreso Maillard, `Char` (carbonización), `FatRendered` (grasa rendida) y estado/lado de cada cara para el volteo. Los nombres de variables son identificadores del modelo; las magnitudes y ecuaciones quedan balanceables.

El calor de la región de parrilla y la transferencia del alimento impulsan la evolución. La cara expuesta cambia al voltear; fuera del fuego la pieza puede conservar calor y seguir cocinándose según el modelo. Maillard, carbonización, humedad y grasa rendida aportan señales y/o calidad, sin imponer una receta temporal.

## Punto de cocción

Cada alimento define umbrales configurables de centro y reglas visuales asociadas. Las categorías compartidas de `PreferredDoneness` son jugoso, a punto, a punto+, cocido y bien cocido. Bandas iniciales de temperatura central (°C): jugoso 50–54, a punto 55–59, a punto+ 60–64, cocido 65–69, bien cocido 70–76. Son bandas base de gameplay, calibrables y con override por alimento; chorizo y provoleta pueden definir bandas apropiadas a sus estados/materiales en vez de heredar literalmente carne entera. Para evaluar, cada alimento expone intervalos ordenados con tolerancias editables y transiciones legibles. El rango objetivo recibe el mejor match; la calidad decrece gradualmente al alejarse.

La tanda inicial usa el perfil base para tira y un override de chorizo a 68–70 / 71–73 / 74–76 / 77–80 / 81–85 °C, con transferencia central y carbonización más rápidas. Son valores de tuning del juego, pendientes de playtest.

El punto se calcula desde estado térmico y señales acumuladas pertinentes; nunca desde tiempo transcurrido como condición suficiente. No hay timer de estado ni bloqueo de servicio por duración.

## Verificación

- El volteo cambia qué cara recibe calor y conserva estado diferenciado de ambos lados.
- Temperaturas, humedad, Maillard, `Char` y `FatRendered` evolucionan o se conservan de forma observable según exposición y retiro.
- Al variar la escala de simulación cambia el ritmo, no los umbrales de punto.
- Cambiar rangos de punto de un alimento no obliga a cambiar los rangos de otro.
