# Animation Manifest — Asadito

Inventario runtime. Estados: `FINAL`, `PROVISIONAL`, `BLOCKED`. Las animaciones son corutinas/procedurales en el Canvas; no hay biblioteca final de clips `Animator`. `PROVISIONAL` indica que funciona en batch/Editor donde se detalla, pero falta revisión visual/táctil en dispositivo.

| AnimationId | Target | Trigger | Implementation | RuntimePath | Status | Validation |
|---|---|---|---|---|---|---|
| `cover.brand.entrance` | Logo y subtítulo de portada | Carga de portada | Fade + subida + escala, escalonado, ~0,34 s | `AsaditoGame.AnimateMenuEntrance` | PROVISIONAL | PlayMode valida logo cargado; revisar composición y timing en teléfono |
| `cover.cta.entrance` | ENTRAR / SALIR | Carga de portada | Fade/slide/scale escalonado; raycast deshabilitado hasta completar | `BuildFrontEnd` → `AnimateMenuEntrance` | PROVISIONAL | PlayMode confirma ENTRAR bloqueado durante entrada y activo después |
| `cover.ambient.glow` | Resplandor de brasa en key art | Portada activa | Pulso de opacidad/escala de baja amplitud | `AsaditoGame.AnimateTitleGlow` | PROVISIONAL | Corutina runtime; falta inspección visual/móvil |
| `ui.button.press` | Botones redondeados | Pointer down / selección | Color tint Unity + squash/feedback corto | `AsaditoButtonFeedback` | PROVISIONAL | Disponible en botones runtime; sin verificación táctil en dispositivo |
| `ui.serve.ready` | SERVIR | Bandeja completa | Pulso sutil mientras sea interactuable | `AsaditoButtonFeedback` | PROVISIONAL | Habilitación comprobada al completar bandeja; legibilidad por revisar en móvil |
| `ui.screen.transition` | Menu, selección, intro, gameplay | Navegación | Cross-fade con `CanvasGroup`, ~0,38 s | `TransitionToLevelSelect` / `TransitionToIntro` / `TransitionToGameplay` | PROVISIONAL | Tests PlayMode recorren selección/intro/gameplay en L1–L5 |
| `fire.ember.pulse` | Resplandor/brasas del mapa térmico | Fuego encendido | Variación procedural ligada a energía del `HeatGrid` | `AsaditoGame.AnimateEmbers` / `RefreshHeatGridVisuals` | PROVISIONAL | Modelos/test de calor pasan; no perfilado ni review gráfico en teléfono |
| `fire.smoke.puff` | Zona de parrilla | Cocción/humo | Círculos Canvas livianos con ascenso y fade | `SmokePuffs` / `FloatSmoke` | PROVISIONAL | Sin humo volumétrico; densidad y rendimiento móvil pendientes |
| `food.place` | Porción | Selección/colocación | Entrada con escala breve (~0,22 s) + sizzle sintético | `PlaceMeat` | PROVISIONAL | Pointer PlayMode y flujo de cocción L1–L5 pasan |
| `food.cook.state` | Sprite de comida | Cambios de temperatura/Maillard/char | Swap de atlas por cuatro bandas térmicas con tinte auxiliar | `RefreshFoodVisual` | PROVISIONAL | PlayMode valida los 16 sprites y swaps; transición no es shader blend y faltan seis estados por especie |
| `food.drag` | Porción y pinza | Drag pointer | Posición directa en heat grid; pinza acompaña el pointer | `FoodPieceTouch` / `OnFoodDrag` | PROVISIONAL | Handlers/eventos simulados pasan; faltan touch real y límites en varios aspect ratios |
| `food.flip` | Porción / cara expuesta | DAR VUELTA | Compresión horizontal (~0,3 s), giro leve y alternancia de cara | `FlipAnimation` | PROVISIONAL | Cambio de cara verificado en flujo PlayMode; revisar lectura visual/dispositivo |
| `food.to-tray` | Porción y bandeja | Retirar porción | Traslado/reducción (~0,42 s) al hueco de bandeja | `PlatePortion` | PROVISIONAL | Asignación/conteo verificados L1–L5; falta revisar espacio visual y feedback |
| `guest.reaction` | Retrato individual y texto de invitado | Evaluación de servicio | Retrato por identidad/expresión + rebote breve (~0,38 s) por comensal | `GuestReaction` / `ApplyGuestPortrait` | PROVISIONAL | PlayMode comprueba sprites y expresión individual en resultados L1–L5 |
| `results.score.count` | Puntaje | Entrada a resultados | Conteo ascendente (~0,58 s) | `AnimateResultScore` | PROVISIONAL | Resultado/score persistido y loop de niveles verificados en batch |
| `results.star.feedback` | Estrellas/resultados | Evaluación de estrellas | Pop/escala breve por umbral logrado | `ShowFinalScore` / `AnimateResultScore` | PROVISIONAL | Estrellas y save comprobados; sin revisión de ritmo/sonido en dispositivo |
| `results.portraits` | Retratos de resultados | Se muestra resultado | Grupo activo recibe sprite con su expresión de evaluación | `BuildResultGuestPortraits` | PROVISIONAL | Presencia, expresión y limpieza al avanzar comprobadas en PlayMode L1–L5 |
| `audio.sizzle.haptic` | Audio/háptica opcional | Inicio de cocción / acciones | Sizzle sintético en memoria; háptica condicional de plataforma | `BuildSizzleAudio` / `VibrateFeedback` | PROVISIONAL | El audio/haptic requiere validación física, mezcla/settings y build móvil |

Toda animación debe confirmar acción/estado sin tapar parrilla, porción, pedidos ni estado térmico. El contenido debe respetar safe area, pausa, accesibilidad y rendimiento móvil. Lo que aún necesite VFX dedicado, blending de material, clips/sonido o device QA sigue `PROVISIONAL` o `BLOCKED`, no `FINAL`.
