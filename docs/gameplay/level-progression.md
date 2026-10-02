# Progresión de niveles

Fuente de verdad de runtime: [`MvpLevelCatalog.cs`](../../Assets/Asado/Scripts/Runtime/MvpLevelCatalog.cs). El orden enseña calor/punto antes de combinar cortes rápidos, gruesos, sensibles o de distinta familia. Cada `FoodIds` aporta una porción a cada invitado de ese nivel (`GuestCount == FoodIds.Length` en este MVP). El selector muestra la ilustración y solo el texto `Nivel N`; títulos de esta tabla son documentación de diseño, no datos adicionales mostrados en la tarjeta.

| Nivel | Nombre de diseño | Porciones / comensales | Contenido (IDs del catálogo) | Aprendizaje principal |
|---:|---|---:|---|---|
| 1 | El debut | 2 | `tira`, `chorizo` | Arrastrar toda la orden desde la bandeja → parrilla (cambia a tabla al vaciarse), reubicar durante la carga y cocinar/servir de a una pieza. |
| 2 | Una tanda más | 3 | `chorizo`, `tira`, `chorizo` | Manejar más piezas y mantener el ritmo de servicio. |
| 3 | Puntos distintos | 4 | `tira`, `chorizo`, `tira`, `chorizo` | Resolver varios puntos solicitados. |
| 4 | El vacío | 4 | `vacio`, `tira`, `chorizo`, `tira` | Incorporar corte grueso y lento. |
| 5 | La gran juntada | 6 | `tira`, `chorizo`, `vacio`, `provoleta`, `tira`, `chorizo` | Sincronizar cortes y queso con ritmos muy distintos. |
| 6 | Cortes finos | 5 | `entrana`, `bife_angosto`, `chinchulines`, `entrana`, `tira` | Manejar piezas finas/rápidas y grasa sobre calor. |
| 7 | Punto justo | 4 | `lomo`, `colita_cuadril`, `vacio`, `lomo` | Equilibrar carne magra sensible con cortes más lentos. |
| 8 | Achuras | 5 | `chinchulines`, `morcilla`, `morcilla_vasca`, `chorizo`, `tira` | Vigilar dorado, grasa y riesgo de rotura de morcilla. |
| 9 | Otras carnes | 5 | `matambre_cerdo`, `costillita_cerdo`, `pollo_deshuesado` ×2, `provoleta` | Cambiar de familia térmica y combinar proteína animal con queso. |
| 10 | Cortes premium | 6 | `bife_ancho`, `bife_chorizo`, `ojo_bife`, `bife_angosto`, `lomo`, `colita_cuadril` | Ajustar el manejo a perfiles vacunos de grasa/espesor diferentes. |
| 11 | Fogón criollo | 6 | `pollo_deshuesado`, `matambre_cerdo`, `solomillo_cerdo`, `costillita_cerdo`, `morcilla_vasca`, `entrana` | Combinar las familias previamente aprendidas. |
| 12 | El asado completo | 6 | `vacio`, `ojo_bife`, `entrana`, `morcilla_vasca`, `provoleta`, `pollo_deshuesado` | Síntesis de proteínas, puntos y ventanas térmicas. |

Los niveles 1–5 conservan el recorrido/tutorial original. `MvpSaveData` v3 conserva las tablas de 12 niveles y migra v1 (progreso de cinco niveles) y v2; la migración ajusta el default térmico legado de 30× a 20× sin sobrescribir un valor custom. EditMode valida cantidades/perfiles válidos y cobertura de las 18 definiciones; PlayMode cubre cooking→serving→results de L1 y que la compuerta de disponibilidad mantenga L2+ bloqueados.

**Disponibilidad actual (2026-10-02):** L1–6 son jugables secuencialmente; L7–12 siguen Disabled/candado. L2 requiere completar L1 con al menos una estrella; cada nivel siguiente requiere una estrella en todos los anteriores y estar dentro del techo de desbloqueo guardado. `MvpSaveData.IsLevelUnlocked` comprueba esos prerrequisitos, y `AsaditoGame.IsLevelAvailable` añade el límite jugable. Las tarjetas, selección directa y avance desde resultados comparten esa condición. Un `MaxUnlockedLevel` antiguo alto sin estrellas previas ya no permite saltear niveles. Se preservan sus valores/estrellas/puntajes y economía; no se modifica la migración ni se resetea la partida.
