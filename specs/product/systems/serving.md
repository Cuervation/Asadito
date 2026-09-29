# Sistema — asignación y servicio

`ServingAllocator` determina, en orden, que los invitados reciban una primera porción antes de repetición, luego maximiza déficit cubierto, tier alimenticio (rechazado/neutral/gustado/favorito) y coincidencia de punto. Empates son deterministas por la ordenación de IDs. Tras servir, la evaluación calcula por invitado cocción, saciedad, doneness y preferencia en 0–100. `FoodPreference` asigna 0/40/70/100; favorito sí vale 100.

Cada nivel define `FoodIds` y cantidad por porción; servicio retira desde bandeja, cierra la tanda y presenta puntuación/estrellas, save y Retry/Next. El allocator prioriza reparto justo sin saltar invitados con menos platos. No aplica género a objetivos.
