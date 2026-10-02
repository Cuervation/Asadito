# Manifest de animación, audio y VFX — ASADITO

Inventario de feedback de runtime existente, mayormente procedural y con corutinas Canvas. `PROVISIONAL` significa integrado, no aprobado por artista ni revisado táctilmente en dispositivo. **No existen seis clips Animator authored por cada comida**: los 18 alimentos comparten interacciones genéricas, con sprite/cocción por datos y cara actual.

| ID | Target | Implementación conectada | Runtime | Verificación / límite |
|---|---|---|---|---|
| `cover.entrance` | Wordmark, subtitle y CTA | Fade/slide/scale escalonado | `AnimateMenuEntrance`, `BuildFrontEnd` | Transition UI en PlayMode; falta revisar ritmo visual en móvil. |
| `cover.glow` | Brasa ambiente en portada | Pulso de opacidad/escala | `AnimateTitleGlow` | Activo en la portada; no shader/VFX authored. |
| `ui.button.feedback` | Botones | Tint/squash y toque sintetizado | `AsaditoButtonFeedback`, `PlaySfx(UiTap)` | Base procedural; sin QA táctil físico. |
| `ui.navigation.transition` | Selector/intro/gameplay/resultados | CanvasGroup fade/transiciones | `TransitionToLevelSelect/Intro/Gameplay`, results | PlayMode recorre el flujo y 12 niveles; menú sigue enabled bajo safe-area root. |
| `grill.heat-always-on` | Parrilla | Sin secuencia/efecto; modelo uniforme, 210 °C al iniciar | `GrillHeatModel` | EditMode/PlayMode nuevos; no visualiza zonas ni carbón. |
| `food.cooking-smoke` | Alimento sobre parrilla | Puff ligero de Canvas y ascenso/fade, solo mientras cocina | `SmokePuffs`, `FloatSmoke` | No partículas volumétricas; rendimiento de teléfono pendiente. |
| `food.raw-tray-to-grill` | Carne cruda y bandeja de aluminio procedural | Hit target táctil ampliado; al tomar, crece al tamaño de parrilla, sigue el dedo y settle al soltar sobre el fuego; fuera de zona vuelve a bandeja | `FoodPieceTouch`, `BeginFoodDrag`, `PlaceRawPortionOnGrill`, `ReturnFoodToSourceTray`, `LiftFood`, `PlaceMeat` | PlayMode está validando la transferencia; tacto físico pendiente. |
| `food.appear-place` | Porción en parrilla | Escala de aparición/contacto | `PlaceMeat` | Porciones del ciclo completo pasan en PlayMode. |
| `food.select-lift` | Porción en parrilla | Elevación, tilt y halo/sombra de selección | `BeginFoodDrag`, `LiftFood`, `UpdateFoodSelectionVisuals` | Pointer simulado, touch real no verificado. |
| `food.drag-release` | Porción en parrilla | Seguimiento pointer, clamp y asentamiento easing | `DragFood`, `EndFoodDrag`, `ReleaseFood` | Drag/touch simulado; probar bordes/aspect ratios móvil. |
| `food.flip-face` | Pieza | Squash horizontal + pequeño arco, alterna cara y recupera textura de cara expuesta | `FlipAnimation`, `FoodState` | Test de cara independiente y progresión; el frame es sprite swap (no mesh rotatoria). |
| `food.cook-visual` | Atlas/corte | Reemplazo de sprite por seis etapas térmicas por perfil | `RefreshFoodVisual` | EditMode carga seis estados de cada atlas; cambio discreto, no blend por material. |
| `food.to-table` | Comida/tabla | Drag grill→tabla, arco breve, deposita en posiciones empaquetadas con escala común por nivel y aspecto individual; tabla completa acepta un solo tap para servir | `FoodPieceTouch`, `PlatePortion`, `ServeAnimation`, `ServingBoardPortionPosition`, `ServingBoardTouch` | PlayMode valida drag/tap en la suite actual; tacto real pendiente; el ajuste de proporciones más reciente está compilado pero aún no probado en Unity. |
| `guests.reactions` | Retrato + evaluación | Retrato elegido y bounce secuencial por evaluación | `GuestReaction`, `ApplyGuestPortrait` | Perfiles/reacciones en resultados; QA de timing pendiente. |
| `results.entrance-score` | Panel y score legacy | Fade/scale + conteo ascendente | `AnimateResultsEntrance`, `AnimateResultScore` | Save/score/progression cubierto por PlayMode. |
| `results.stars` | Tres estrellas legacy | Pop escalonado/tint y cue por estrella ganada | `PopResultStar`, `PlaySfx(Star)` | Estrellas/progreso en ciclo; no prueba accesibilidad ni mezcla. |
| `audio.ui-drop-flip-plate-serve-result-star` | Acciones | Siete AudioClips determinísticamente sintetizados en memoria y `PlayOneShot` | `ProceduralSfx` | Código activo, sin cue de encendido; mezcla y volumen físico pendientes. |
| `audio.sizzle` | Cocción | Sizzle sintético loop en runtime; haptics opcionales | `BuildSizzleAudio`, `VibrateFeedback` | No es asset grabado; haptics requieren hardware. |

No se documentan gestos, expresiones o VFX que no sean código real. Todo gesto importante tiene feedback básico; requiere polish y validación visual/touch real.

## Gestión 100 % 2D — vigente (2026-10-02)
- `ManagementFoodView`: pulso/escala breve al seleccionar, preview RAW 2D temporal siguiendo al dueño del drag y feedback de incorporación al carrito; fuente restaurada tras terminar/cancelar. Sin clones 3D ni animaciones de cámara/puerta.
- Heladera abierta ilustrada: movimiento/escala Canvas corto de la unidad a la tabla y de regreso al estante; mantiene sprite/tamaño e InventoryUnit.Id. Selección reversible hasta `PrepareUnits`, sin consumo por animación.
- Corutinas sustituibles por target; cancelar/drop inválido/UI/otro dedo/cerrar/disable limpia previews y movimientos. Release del drag no duplica tap. Sonidos existentes preservados; no materiales ni partículas volumétricas.
- Validación 2D: resultados reales de compilación, pruebas/punteros/capacidad/IDs y capturas portrait se registran en [estado operativo](../current-project-state.md). Sin inferir FPS/tacto humano Android a partir de estos efectos.

## Gestión 3D inicial (histórico, 2026-10-01)
- Selección tienda: pulso/highlight 0.22s y mesh decorativo al carrito 0.32s, sonido existente. Coroutines locales reemplazables; no bloqueo de la transacción.
- Heladera: apertura de bisagra 0.55s; selección/devolución a bandeja 0.25s. Identidad de unidad intacta hasta PrepareUnits. Movimiento reversible y recolocación de slots; sin clips/rig ni animación offline.
- Cámara/meshes/materiales/RenderTexture se liberan al cerrar, sin Update de selección ni iluminación dinámica.

### Carnicería híbrida — corrección histórica de selección (2026-10-01)
Retirado vuelo shrinking de clones de comida3D. Toque/drop válidos generan solo pulso0.18s del modelo original; arrastre usa un preview UI temporal único que sigue al dedo, no bloquea raycast y se destruye al soltar/abandonar. Drop inválido cancela sin alterar carrito/monedas. En ese paso se conservaba la puerta; la heladera híbrida siguiente la reemplaza por ilustración abierta.

### Pulso/drag de mostrador perspectiva (histórico, 2026-10-01)
El pulso0.18s vuelve a HomeScale de cada pieza, no a una escala global; comenzar drag detiene el pulso y recupera esa escala. Cancelar/drop preservan tamaño/posición de fuente. Modelos posteriores no pierden perspectiva después de seleccionar. QA focal de layout/drag2/2PASS; Android pendiente.

### Heladera híbrida histórica reemplazada (2026-10-02)
Gabinete/puerta2D estáticos abiertos, no bisagra3D ni animación de puertas repetidas. Selección/devolución de comida3D0.25s conserva ancho de parrilla en destino mediante compensación de perspectiva, sin reducir tamaño por número de seleccionadas. Cancelar devuelve cada ID al estante y recupera tint de frescura. Recursos/coroutines locales liberados al cerrar. QA PlayMode dirigida5/5PASS; ritmo/tacto humano Android pendientes.

### Mini barras por comida (2026-10-02)
Dos Images Canvas nativos por pieza (track8px/fill6px), stretch al ancho real del arte, sin assets nuevos ni Animator. Llenado/color rojo-amarillo-verde-amarillo-rojo desde estado térmico por perfil, un tramo verde sostenido y señales de overshoot. Hijo del alimento sigue posición/escala de interacción, sin raycast; oculto al emplatar/reset y pausa sin avance. Unity2EditMode+3PlayMode focalesPASS; renders realesportrait inspeccionados. Touch/performance Android pendientes, no APK nueva.

### Resultado de gestión ilustrado (2026-10-02)
`ManagementResultLayout` aplica fit uniforme dentro del Canvas/safe area y entrada0.20s unscaled: fade0→1, escala0.985→1, interacción habilitada al terminar. Suscripción `Canvas.willRenderCanvases` se libera en OnDisable; no altera economía/save. Las estrellas earned/unearned se muestran inmediatamente, sin el conteo/pop de la ruta legacy. Overlay de detalle bloquea input posterior y permite scroll nativo; cierre desactiva antes de Destroy. Test focal verifica tres estados, cinco invitados, tamañosportrait y botones; tres integraciones compra/cocción/resultado/recuperaciónPASS. Ritmo/tacto humano físico pendientes; sin APK nueva.
