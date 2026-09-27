# Sistema — asignación y servicio

`ServingAllocator` asigna porciones cocinadas a objetivos de comensales, en este orden lexicográfico: (1) evitar hambre/cumplir `TargetFoodAmount`; (2) maximizar `FoodPreference`; (3) maximizar `DonenessMatch`. Las preferencias distinguen alimentos favoritos, gustados y rechazados. Las prioridades son deterministas para un mismo estado y configuración.

El jugador conserva el control de colocar, voltear y retirar cada pieza, preparar la bandeja y decidir cuándo servirla. Servir cierra la tanda y produce evaluación; no se exige duración mínima. El nivel 1 presenta dos porciones y dos comensales. Los niveles 2–5 pueden elevar gradualmente escala y variedad según su configuración.

## Verificación

- Si hay una asignación que cumple más objetivos/preferencias, el allocator la prefiere a otra que no los cumple, aunque esta tenga calidad marginalmente mayor.
- Con objetivos igualmente cubiertos, la asignación favorece mayor cobertura/saciedad y luego calidad/punto.
- El servicio puede iniciarse al criterio del jugador y genera un resultado para cada comensal y uno agregado.
