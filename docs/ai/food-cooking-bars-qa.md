# Mini barras de cocción — QA 2026-10-02

## Pedido e implementación
Barrita bien fina del largo de cada alimento: rojo→amarillo→tramo verde→amarillo→rojo. Dos Images nativos por pieza, track8px/fill6px a1080 de referencia; ancho anclado al arte real, no al hitbox. Separación5px debajo del borde del arte. Sin textos/iconos/porcentajes o assets nuevos. Fill inicial2.5% rojo para no quedar totalmente invisible; avanza izquierda→derecha y el relleno cambia color. Visible solo mientras la pieza está en parrilla, no en fuente cruda/tabla; no intercepta raycasts y sigue drag/animación.

FoodCookingProgress es lectura pura, usa la temperatura de centro y banda A_Punto por FoodCookProfile, más char/humedad para advertir daño; no elapsed-time, selección global ni variables nuevas de save. Verde45–60% del gauge; amarillos25% y78%, extremos rojos. Provoleta sigue GetProvoletaStage y necesita dorado para verde54–66; el gap66–69 posterior a Ideal avanza, no vuelve a lectura cruda. Los umbrales del modelo, perfiles, puntuación y preferencias de invitados no cambian. Verde es referencia de juego, no garantía de puntaje/preferencia ni inocuidad.

## Validación proporcional real
Unity6000.6.3f1 en copiaQA ya importada /tmp/Asadito-concurrent-cooking-qa; Editor principal intacto, sin suite completa/build Android.
- **2/2EditModePASS**,0.1238755s: CookingProgress_UsesEachFoodProfileAndHasRedYellowGreenYellowRedWindow y CookingProgress_FollowsRealThermalStepsMonotonicallyAndHandlesDrying.18perfiles, extremos/tramo verde/ambos amarillos, lectura sin mutación, temperatura equivalente distinta tira/chorizo, queso sin dorar y post-Ideal, quemado/sequedad y500pasos térmicos reales por alimento sin retroceso.
- **3/3PlayModePASS**,18.9958673s:
  - FoodCookingBars_AreThinFoodWidthIndependentAndNeverBlockDrag: largo/grosor por corte, no raycasts, estado independiente rojo/verde/quemado, progreso de ambas piezas por Step real mientras solo una está seleccionada, pausa, arrastre, emplatado de una conserva barra de otra, reset oculta y recupera rojo inicial.
  - FoodPieces_AreVisibleDirectTouchTargetsAndNoFoodNameButtonsRemain: interacción, tamaño, fuente, transferencia y cocción concurrente conservados.
  - AluminumTrayDrag_StartsCooking_AndBoardTapServes: drag/fuente/tabla/servicio y contención previos intactos.
- Compilación Unity real incluida; git diff --checkPASS. Renders Unity1080×1920 rojo/verde y quemado/verde inspeccionados. Estados controlados del fixture para visualizar colores, simulación congelada solo en captura; en el mismo test también se verifica progreso con Step real. Reposicionamiento portrait solo en callbackQA para evitar coordenadas cacheadas del aspecto del Editor, sin cambio jugable.
- XML/log/PNG preservados en build/qa-food-cooking-bars/ (ignorados por Git).

## Límites
Cambios locales posteriores al commit6238ae1. No nuevo commit/push/APK/install solicitado. Celular conserva1.4.0/code6 anterior sin las vistas híbridas ni barras; revisión humana del grosor/tacto/notch/FPS física pendiente.
