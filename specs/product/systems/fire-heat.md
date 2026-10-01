# Sistema — parrilla caliente

## Regla canónica: heat always on

Al entrar al gameplay, la parrilla ya está caliente. `GrillHeatModel` entrega una temperatura uniforme configurable, con 210 °C como valor inicial y clamp de 100–300 °C. No hay acción de encendido/apagado, combustible, decaimiento, celdas de calor, brasas movibles ni UI/SFX de carbón. El área invisible de la parrilla solo define hit bounds, posición y movimiento de las piezas; no representa ni calcula calor espacial.

`AsaditoGame.Update` pasa la temperatura de `GrillHeatModel` y el tiempo simulado a `FoodCookingModel.Step` para cada pieza iniciada que permanezca sobre la parrilla. Sacar una pieza de la parrilla detiene su calentamiento; llevarla a la tabla la retira del grill. Perfiles por corte conservan diferencias de transferencia, masa/espesor, humedad, Maillard, char, riesgo y etapas específicas de provoleta.

`SimulationTimeScale` global inicia en 20 y solo acelera/ralentiza la evolución térmica; no define puntos ni reemplaza temperaturas por temporizadores. El control interno ofrece 20/25/30/35/40 únicamente en Editor/development builds.

## Verificación

- Test EditMode: 210 °C por defecto, valor configurable y clamp 100–300 °C.
- Test PlayMode: L1 entra sin encendido, no crea botón/estado/grilla/celdas de carbón; cada pieza arrastrada empieza a calentarse de inmediato con su perfil, incluso mientras quedan cortes crudos en la bandeja y mientras otras piezas cocinan.
- El barrido visual del runtime no encuentra objetos `Mapa de calor carbón 8x6` ni `Brasa x,y`.
