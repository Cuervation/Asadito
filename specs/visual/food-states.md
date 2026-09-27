# Estados visuales de alimentos

Los estados son legibles y graduales, derivados de `FoodState`; el tiempo de animación puede suavizar la transición, pero no decide el punto.

| Estado | Lectura visual | Señales secundarias |
|---|---|---|
| Crudo | Color/material propios del corte, húmedo | Sin humo de cocción |
| Calentando | Superficie pierde brillo crudo | Sizzle suave, vapor escaso |
| Dorando | Maillard visible y desigual por cara | Grasa/brillo; humo leve |
| Punto objetivo | Dorado apetitoso, centro acorde al corte | Feedback positivo discreto |
| Pasado | Menos humedad, tono más oscuro | Humo más denso, reacción negativa |
| Quemado | Char negro localizado, silueta aún clara | Humo corto, feedback inequívoco |

Chorizo, tira, vacío y provoleta tienen materiales y transiciones propios; no se usa un mismo tinte plano. El volteo conserva el estado por cara. La tira muestra hueso/grasa y provoleta evita parecer carne.
