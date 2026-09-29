# Progresión de niveles

Fuente de verdad de runtime: [`MvpLevelCatalog.cs`](../../Assets/Asado/Scripts/Runtime/MvpLevelCatalog.cs). El orden enseña calor/punto antes de combinar cortes rápidos, gruesos, sensibles o de distinta familia. Cada `FoodIds` aporta una porción a cada invitado de ese nivel (`GuestCount == FoodIds.Length` en este MVP). El selector muestra la ilustración y solo el texto `Nivel N`; títulos de esta tabla son documentación de diseño, no datos adicionales mostrados en la tarjeta.

| Nivel | Nombre de diseño | Porciones / comensales | Contenido (IDs del catálogo) | Aprendizaje principal |
|---:|---|---:|---|---|
| 1 | El debut | 2 | `tira`, `chorizo` | Tutorial; primero aprender a iniciar y gestionar la parrilla. |
| 2 | Zonas de calor | 3 | `chorizo`, `tira`, `chorizo` | Colocar piezas en distintas zonas y priorizar tiempos. |
| 3 | Puntos distintos | 4 | `tira`, `chorizo`, `tira`, `chorizo` | Resolver varios puntos solicitados. |
| 4 | El vacío | 4 | `vacio`, `tira`, `chorizo`, `tira` | Incorporar corte grueso y lento. |
| 5 | La gran juntada | 6 | `tira`, `chorizo`, `vacio`, `provoleta`, `tira`, `chorizo` | Sincronizar cortes y queso con ritmos muy distintos. |
| 6 | Cortes finos | 5 | `entrana`, `bife_angosto`, `chinchulines`, `entrana`, `tira` | Manejar piezas finas/rápidas y grasa sobre calor. |
| 7 | Punto justo | 4 | `lomo`, `colita_cuadril`, `vacio`, `lomo` | Equilibrar carne magra sensible con cortes más lentos. |
| 8 | Achuras | 5 | `chinchulines`, `morcilla`, `morcilla_vasca`, `chorizo`, `tira` | Vigilar dorado, grasa y riesgo de rotura de morcilla. |
| 9 | Otras carnes | 5 | `matambre_cerdo`, `costillita_cerdo`, `pollo_deshuesado` ×2, `provoleta` | Cambiar de familia térmica y combinar proteína animal con queso. |
| 10 | Cortes premium | 6 | `bife_ancho`, `bife_chorizo`, `ojo_bife`, `bife_angosto`, `lomo`, `colita_cuadril` | Ajustar zonas a perfiles vacunos de grasa/espesor diferentes. |
| 11 | Fogón criollo | 6 | `pollo_deshuesado`, `matambre_cerdo`, `solomillo_cerdo`, `costillita_cerdo`, `morcilla_vasca`, `entrana` | Combinar las familias previamente aprendidas. |
| 12 | El asado completo | 6 | `vacio`, `ojo_bife`, `entrana`, `morcilla_vasca`, `provoleta`, `pollo_deshuesado` | Síntesis de proteínas, zonas, puntos y ventanas térmicas. |

Los niveles 1–5 conservan el recorrido/tutorial original. `MvpSaveData` v2 extiende las tablas de estrellas/puntajes a 12 filas y migra guardados v1 (cinco niveles) sin perder su progreso previo. EditMode valida que los 12 niveles tengan cantidades/perfiles válidos y que el conjunto cubra las 18 definiciones; PlayMode recorre cooking→serving→results/desbloqueo→volver para los doce.
