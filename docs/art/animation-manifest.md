# Animation Manifest — First Playable

Estado: `FINAL`, `PROVISIONAL`, `CONCEPT` o `TODO`.

| Interacción | Estado | Implementación actual | Criterio de DONE |
|---|---|---|---|
| Embers (glow/pulse) | PROVISIONAL | Resplandor UI pulsante | Brasa comunica intensidad y cambia con el mapa térmico |
| Ceniza/chispas | TODO | — | Partículas limitadas, legibles y ligeras en móvil |
| Colocar alimento | PROVISIONAL | Escala de entrada; sin pinza/contacto | Grab con pinza, descenso, contacto, sizzle y humo |
| Cocinar/surface states | PROVISIONAL | Tinte uniforme por dosis térmica | Transición gradual de superficie, humedad, Maillard y char |
| Mover pieza | TODO | — | Grab, traslado espacial, release y contacto con zona destino |
| Flip | PROVISIONAL | Compresión horizontal de ~0.3 s | 0.5–0.8 s; lift, rotation, cara opuesta visible, contact, sizzle y feedback |
| Retirar a bandeja | PROVISIONAL | Desplazamiento/fade de ~0.42 s | Levantar, trasladar y depositar; pieza queda visible en bandeja |
| Servir bandeja | TODO | — | Transición breve de bandeja/cámara y señal clara de cierre |
| Reacción de comensales | PROVISIONAL | Un rebote del retrato y texto conjunto | Entrada/expresión por comensal con score legible |
| Resultado/estrellas | TODO | Resultado con puntos, sin estrellas | Conteo animado, estrellas, motivo y botón retry |
| Audio/haptic | TODO | No conectado | Feedback de fuego, sizzle, pinza/metal, bandeja, positivo/negativo; haptic optativo y configurable |

Toda animación debe reforzar estado o acción, no ocultar el alimento. La implementación puede ser procedural; no requiere Animator si el resultado visual cumple acceptance.
