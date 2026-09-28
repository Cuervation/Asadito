# Animation Manifest — Asadito MVP

Estados: `FINAL`, `PROVISIONAL`, `BLOCKED`. Las animaciones actuales son principalmente corutinas/Canvas; no hay biblioteca final de clips `Animator`.

| Interacción | Estado | Implementación actual | Criterio de cierre |
|---|---|---|---|
| Brasas / calor | PROVISIONAL | Resplandor ambiental pulsante; color de celdas ligado a energía de carbón | Llama/brasas estilizadas expresivas y legibles, ligadas al HeatGrid; rendimiento comprobado en móvil |
| Ceniza / chispas | BLOCKED | Sin partículas dedicadas | Añadir VFX escaso, liviano y no obstructivo |
| Colocar alimento | PROVISIONAL | Entrada con escala breve (~0,22 s); sizzle al iniciar | Grab/contacto/apoyo visual y sonoro con estado térmico legible |
| Cocción / estados | PROVISIONAL | Tinte general por Maillard/Char; punto y temperatura en HUD | Estados ilustrados por alimento y cara (crudo→calentando→dorado→ideal→pasado/quemado) inequívocos |
| Mover pieza | PROVISIONAL | Drag directo sobre HeatGrid; pinza geométrica sigue el pointer | Validar touch, trayectoria y límites en dispositivo; mejorar peso/feedback sin tapar comida |
| Voltear | PROVISIONAL | Actualiza la cara térmica; compresión horizontal ~0,3 s y giro aleatorio pequeño | Levantar, rotar y aterrizar con lectura clara de la cara opuesta, sonido y feedback |
| Retirar a bandeja | PROVISIONAL | Desplazamiento/reducción hasta la bandeja (~0,42 s) | Mejorar agarre/depósito y comprobar que cada porción queda ordenada/visible |
| Servir | PROVISIONAL | Solo se habilita con toda la bandeja; evalúa y bloquea interacción | Confirmación de cierre y transición clara a resultados |
| Reacción de comensales | PROVISIONAL | Rebote único de avatar genérico (~0,38 s), pausa y resultados | Reacciones individuales según satisfacción, con retratos/personas legibles |
| Resultado / score | PROVISIONAL | Conteo del puntaje (~0,58 s), muestra estrellas y botones Retry/Next o Niveles | Composición visual final, motivo entendible y navegación/progreso QA |
| Audio / háptica | PROVISIONAL | Sizzle sintético en memoria; vibración opcional en plataformas móviles | Efectos específicos y mezcla configurable; verificar haptics y respetar settings |

Toda animación debe confirmar estado/acción sin ocultar pieza, lectura térmica u órdenes. El contenido debe respetar pausa, safe area y rendimiento de móvil.
