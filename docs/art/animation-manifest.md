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
| `results.entrance-score` | Panel y score | Fade/scale + conteo ascendente | `AnimateResultsEntrance`, `AnimateResultScore` | Save/score/progression cubierto por PlayMode. |
| `results.stars` | Tres estrellas | Pop escalonado/tint y cue por estrella ganada | `PopResultStar`, `PlaySfx(Star)` | Estrellas/progreso en ciclo; no prueba accesibilidad ni mezcla. |
| `audio.ui-drop-flip-plate-serve-result-star` | Acciones | Siete AudioClips determinísticamente sintetizados en memoria y `PlayOneShot` | `ProceduralSfx` | Código activo, sin cue de encendido; mezcla y volumen físico pendientes. |
| `audio.sizzle` | Cocción | Sizzle sintético loop en runtime; haptics opcionales | `BuildSizzleAudio`, `VibrateFeedback` | No es asset grabado; haptics requieren hardware. |

No se documentan gestos, expresiones o VFX que no sean código real. Todo gesto importante tiene feedback básico; requiere polish y validación visual/touch real.

## Gestión 3D (2026-10-01)
- Selección tienda: pulso/highlight 0.22s y mesh decorativo al carrito 0.32s, sonido existente. Coroutines locales reemplazables; no bloqueo de la transacción.
- Heladera: apertura de bisagra 0.55s; selección/devolución a bandeja 0.25s. Identidad de unidad intacta hasta PrepareUnits. Movimiento reversible y recolocación de slots; sin clips/rig ni animación offline.
- Cámara/meshes/materiales/RenderTexture se liberan al cerrar, sin Update de selección ni iluminación dinámica.
