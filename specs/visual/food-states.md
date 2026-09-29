# Estados visuales de alimentos

Seis estados por atlas/corte: RAW, WARMING, BROWNING, IDEAL, OVERCOOKED, BURNT. `FoodCookVisualStage` usa esa taxonomía común, mientras que cada uno de los 18 PNGs tiene forma/arte propio y `FoodCookProfile` dicta transiciones; provoleta dispone de sub-etapas de queso. No todo estado adicional requiere clip dedicado.

| Etapa | Lectura prevista |
|---|---|
| RAW | Superficie fresca, color específico del corte y silueta reconocible. |
| WARMING | Cambio gradual de temperatura y pérdida de aspecto frío/crudo. |
| BROWNING | Dorado/Maillard y marcas de fuego con aceite/brillo dependiente de corte. |
| IDEAL | Punto apetitoso; contraste visible vs estados vecinos. |
| OVERCOOKED | Oscurecimiento/pérdida humedad sin perder identidad de forma. |
| BURNT | Carbonización marcada, lectura distinta y no confundir con dorado. |

La cara expuesta decide el sprite de esa pieza; cada `FoodState` conserva ambas exposiciones al hacer flip. Ver [matriz de atlas/perfiles](../../docs/gameplay/food-catalog.md). Arte original/generado de este proyecto; sigue PROVISIONAL hasta aprobación visual y teléfono real.
