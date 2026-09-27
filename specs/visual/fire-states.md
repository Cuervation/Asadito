# Estados visuales del fuego

El carbón se representa con estados graduales que acompañan `HeatGrid`/`EmberEnergy`. Deben poder leerse en pantalla móvil y no tapar la comida.

| Estado | Brasa y parrilla | Feedback |
|---|---|---|
| Apagado | Carbón gris, sin emisión | Sin sizzle |
| Encendido | Núcleos rojos aislados | Chispa breve |
| Calentando | Resplandor naranja creciente | Crepitar leve |
| Fuerte | Brasas vivas y calor concentrado | Ondulación/humo discreto |
| En descenso | Menos celdas calientes | Pulso más lento |
| Casi agotado | Puntos rojos apagándose | Lectura clara de baja energía |

El mapa térmico de celdas afecta la visualización en regiones; no crear una luz en tiempo real por celda. La hoja de estados en `docs/art/concepts/fire-states.png` es referencia, no sprite final.
