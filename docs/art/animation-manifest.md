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
| `tongs.select-grip` | Pinza ilustrada | Desplaza pinza a pieza; alterna PNG abierto/cerrado; sigue drag | `PositionTongsAtFood`, `AnimateTongsGrip`, `DragFood` | Sprite connections/PlayMode nuevos; tacto humano pendiente. |
| `food.appear-place` | Porción en parrilla | Escala de aparición/contacto | `PlaceMeat` | Porciones del ciclo completo pasan en PlayMode. |
| `food.select-lift` | Porción/pinza | Elevación, tilt, selección/lift | `BeginFoodDrag`, `LiftFood` | Pointer simulado, touch real no verificado. |
| `food.drag-release` | Porción/pinza | Seguimiento pointer y asentamiento easing | `DragFood`, `EndFoodDrag`, `ReleaseFood` | Drag/touch simulado; probar bordes/aspect ratios móvil. |
| `food.flip-face` | Pieza | Squash horizontal + pequeño arco, alterna cara y recupera textura de cara expuesta | `FlipAnimation`, `FoodState` | Test de cara independiente y progresión; el frame es sprite swap (no mesh rotatoria). |
| `food.cook-visual` | Atlas/corte | Reemplazo de sprite por seis etapas térmicas por perfil | `RefreshFoodVisual` | EditMode carga seis estados de cada atlas; cambio discreto, no blend por material. |
| `food.to-table` | Comida/tabla | Traslado con arco y escala a slot distribuidos sobre el asset | `PlatePortion`, `ServeAnimation`, `TrayPortionPosition` | Flujo de servicio y resultados en PlayMode. |
| `guests.reactions` | Retrato + evaluación | Retrato elegido y bounce secuencial por evaluación | `GuestReaction`, `ApplyGuestPortrait` | Perfiles/reacciones en resultados; QA de timing pendiente. |
| `results.entrance-score` | Panel y score | Fade/scale + conteo ascendente | `AnimateResultsEntrance`, `AnimateResultScore` | Save/score/progression cubierto por PlayMode. |
| `results.stars` | Tres estrellas | Pop escalonado/tint y cue por estrella ganada | `PopResultStar`, `PlaySfx(Star)` | Estrellas/progreso en ciclo; no prueba accesibilidad ni mezcla. |
| `audio.ui-drop-flip-plate-serve-result-star` | Acciones | Siete AudioClips determinísticamente sintetizados en memoria y `PlayOneShot` | `ProceduralSfx` | Código activo, sin cue de encendido; mezcla y volumen físico pendientes. |
| `audio.sizzle` | Cocción | Sizzle sintético loop en runtime; haptics opcionales | `BuildSizzleAudio`, `VibrateFeedback` | No es asset grabado; haptics requieren hardware. |

No se documentan gestos, expresiones o VFX que no sean código real. Todo gesto importante tiene feedback básico; requiere polish y validación visual/touch real.
