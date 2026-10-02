# Resultados ilustrados — QA focal (2026-10-02)

## Cambio y alcance
Usuario rechaza estética de ventana antigua. ManagementScreen.Results ahora usa tablero de madera/papel existente, estrellas vivas, puntaje grande, expresiones de invitados, tres métricas compactas, balance/ganancia/pérdida e importes secundarios. Consejo y acciones arcade existentes. VER DETALLE conserva el texto completo en overlay con RectMask2D/ScrollRect y bloqueo de clicks al resumen. Sin tocar fórmula, gates, Complete, compras, recompensas, inventario, migración o guardado. Intro y resultados legacy fuera del alcance.

## Implementación
- ManagementScreen.cs: composición y disclosure; rectángulos de texto Baloo de al menos2.5×fontSize para no suprimir líneas.
- ManagementResultLayout.cs: fit uniforme960×1660 (detalle1420), margen de safe area, fade/escala breve0.20s sin business state.
- AsaditoGame.cs: accesores de presentación a fuentes, nivel, invitados y expresión de resultado real.
- ManagementArt.asset: dos referencias nuevas a PNG ya preparados, no arte nuevo; ProductCard9-slice160/70/160/70. Primer borde100 recortaba remaches y los estiraba; corregido tras captura real.

## Evidencia
Unity6000.6.3f1, Editor existente, SampleScene. Compilación final e import sin errores de Console.

1. Management_IllustratedResultsDetailsAndActionsDoNotMutateSave — **PASS1/1 en XML nativo Unity**, 3 variantes0/3/1 estrellas, cinco invitados, textos completos, saldo positivo y pérdida, resumen/detalle, scroll nativo, bloqueo de click-through, volver/retry/next sin escribir prefs ni estado. El primer intento tenía referencia inválida a Assembly-CSharp desde asmdef; corregida a reflexión. Su job MCP luego perdió callbacks y declaró timeout, pero TestResults.xml nuevo (17:36:38–17:36:43Z) confirma Passed y las nueve capturas fueron generadas. No se cuenta el intento abortado como pass.
2. Management_BuyPrepareCookServeAndPersistBalance — **PASS**: compra/preparación/resume/cocción/resultado/saldo/retry.
3. Management_RawServiceCannotUnlockOrFarmCoins — **PASS**: fallo real sin estrellas ni unlock/pérdida.
4. Management_DebutRecoveryAndGuideSurviveReloadWithoutResettingMoney — **PASS**: Caja del Asador, premio reducido y saldo tras reload.

Las tres integraciones: job75f7060905aa4079ba76269e02a2c7b1, succeeded3/3,68.36s. Total cuatro pruebas únicasPASS; no full suite ni APK.

Capturas Canvas nativas1080×1920 y720×1600: éxito, fallo y recovery con cinco invitados, detalle largo desplazado y resultado realL5. Inspección: labels legibles, remaches redondos, sin overflow ni glyphs truncados en resumen. XML/capturas/backup en workspace de chat `output/debug/results-redesign/evidence/`. No es QA de touch físico ni FPS Android.

## Preservación
Baseline local capturado antes de editar.132 diffs previos no relacionados permanecieron idénticos antes de actualizar docs. Se conserva el guardado real actual (saldo166, dos unidades chorizo/tira) respaldado inmediatamente antes de editar; no se restaura el saldo90 de la sesión previa. Previsualizar el resultado anterior mediante ShowFinalScore no vuelve a ejecutar Complete ni pagar recompensa. Sin commit/push.

Cierre: prefs exactos del backup actual166 seguían idénticos tras las pruebas y tras reconstruir solo la presentación del resultado original. Capturas finales1080×1920 y Foldable960×2658 inspeccionadas (`result-final.png`, `result-foldable.png`). Simulator existente mostrado/enfocado, Unity foreground en Play; no recompensa repetida ni reinicio de partida. Scoped diff checkPASS; sin cambios PNG.
