# Auditoría MVP / QA — 2026-09-29

| Gate | Estado | Evidencia / límites |
|---|---|---|
| Fuente de verdad | PASS | Checkout Unity `/Users/celestino/Asadito`, `main`, remote `origin`; scope actual prevalece sobre notas heredadas de la visual slice. |
| Interacción con alimentos | PASS automatizable | Sin botones por nombre/corte; hit target transparente con tamaño táctil ampliado y test de filtro de hit para pieza más cercana. Tap y drag alimentan flujo único de selección; pinza/feedback, movimiento con clamp/settle, flip explícito y retirar/arrastrar a bandeja. PlayMode cubre multi-input y flujo directo. |
| Debug en release | PASS por código / build por confirmar | Control de escala se crea solo cuando `Debug.isDebugBuild`; tests distinguen Editor/dev y release. Confirmar luego en APK no-development que no exista control visible. |
| Comida y perfiles térmicos | PASS automatizable, tuneado | 18 FoodIds únicos, perfiles distintos, JSON y conexión runtime; calor/capacidad/espesor/grasa/humedad/Maillard/char/split/queso verificados por catálogo/tests. A 20× y 210 °C, ventana A_Punto medida por un paso de simulación va ~1.9–18.1 s; esto valida thresholds de código, no diversión observada en test humano. |
| Atlas / estados | PASS técnico, PROVISIONAL artístico | 18 atlas RGBA × 6 estados =108 sprites. Auditoría Python de PNG/alpha, frame no vacío, similitud de silueta adyacente e inventario; contact sheets inspeccionados para identidad, orientación y progresión. Sin frame cortado observado; no equivale a sign-off artístico/táctil humano. |
| Niveles/progreso/save | PASS automatizable | 12 tarjetas/definiciones; todos los alimentos en L1–L12; flujo progresivo PlayMode, estrellas/puntajes, Retry, Next, save/migración. Guardado v3 migra v2 y v1; scale 30→20 en v2 solamente si era el default anterior. |
| Cara/flip | PASS automatizable | Cooking state por cara independiente; flip conserva ambas, activa cara opuesta y hace transición 2D; tests de cara y runtime. |
| Guests/scoring | PASS automatizable | Seis perfiles conservan edad/peso/apetito/punto/favoritos/gustados/rechazados; score 40/30/20/10; allocator y scoring tests. No se alteró la regla de diseño. |
| Pausa / tutorial / opciones | PASS automatizable | Continuar, reiniciar, selector, SFX ON/OFF y vibración ON/OFF; pausa congela simulación/audio, Retry restablece alimento/parrilla. Tutorial contextual menciona contacto directo, pinza, calor, flip y bandeja. |
| safe area / resoluciones | PASS implementado; QA físico pendiente | Tests y sesión previa revisaron 1080×2400, 720×1600, 720×1280, inset de cutout simulado 126 px, Screen.safeArea. No se probó navbar/gestos de un teléfono físico ni todos los modelos/notches. |
| EditMode | PASS | **20 passed / 0 failed**, Unity 6000.6.3f1; retest `/tmp/asadito-final-rerun-editmode.xml`. Incluye tuning de ventana a 20×, perfiles/atlas, progreso, serving/scoring y migración. |
| PlayMode | PASS | **10 passed / 0 failed**, Unity 6000.6.3f1; retest `/tmp/asadito-final-rerun-playmode.xml`. Recorre niveles, directo sobre comida, drag, bandeja, retry/next/save, pausa/debug y gameplay normal. No sustituye playtest humano. |
| Validator / diff check | PASS | Corrida final `python3 Tools/validate_food_content.py`: 18 perfiles únicos, 108 estados visuales/coherentes, 12 ilustraciones, catálogo en L1–L12 y recursos obligatorios. `git diff --check` también ejecutado con éxito antes de entrega/commit. |
| Android build/config | PASS | Unity 6000.6.3f1 `Succeeded`: `/tmp/Asadito-mvp-1.2.0-arm64-final.apk`, 52,615,236 bytes. `aapt` confirma package `com.cuervation.asadito`, version 1.2.0/code3, min26, target/compile36, ARM64 y actividad Unity portrait. Firma debug, sin release keys. |
| Instalación/uso emulador | PASS parcial, drag abierto | Instalado/abierto en emulador Pixel 7a API36 1080×2400. Se llegó a portada, selector, intro L1, se prendió carbón y tap directo seleccionó chorizo; evidencia `/tmp/Asadito-mvp-gameplay-selected.png`. `Renderer2D.asset` dejó de cargar shader Bloom sin uso; en logcat posterior no hay warnings Bloom. Un ADB swipe a bandeja no confirmó drop y acabó con ANR; el estado final del flujo no se considera verificado. |
| Rendimiento / ANR | RIESGO ABIERTO | Se evita repintar HeatGrid completo en cada frame y reordenar pinza por cada `OnDrag`, pero el último swipe sintético provocó ANR de input de ~8 s. Trace: Android 16 x86-64/QEMU con APK ARM64 traducido, thread esperando `glUnmapBufferAEMU_enc`/QEMU GL; causa no determinada, no atribuir exclusivamente al emulador. Requiere Development Build/Unity Profiler y repetición en teléfono ARM64 real; FPS/memoria no medidos con validez. |
| Audio / arte | PROVISIONAL | SFX y sizzling procedural conectados, volumen configurable y crossfade loop revisados en código. Falta escucha en dispositivo/auriculares, medición subjetiva y aprobación visual humana. |
| CI remoto Unity | PASS parcial documentado | [Run 36592407375](https://github.com/Cuervation/Asadito/actions/runs/36592407375): validator y whitespace check corrieron; job GameCI SKIPPED porque owner no habilitó licencia/secretos. No se falsifican tests remotos. |
| Firma/publicación | Fuera de alcance | APK de validación con firma debug solamente. Sin keystore productivo, Play Console ni publicación. |

## Riesgos/acciones post-MVP

1. Probar con manos/dedos y multitoque real la selección, drag sostenido, brasas, flip, drag al plato, overlaps y bordes; validar en 720×1280 hasta 1080×2400 con barra gestual/notch real.
2. Capturar profiler Android de Development Build en gama media para investigar ANR anterior, CPU/GC/frame pacing, memoria de texturas/audio y heat-grid; no inferir rendimiento de SwiftShader.
3. Playtest humano para medir ritmo/ventana por alimento, progreso L1–L12, tutorial, audio y legibilidad; targets de cocción son gameplay, no recomendación sanitaria.
4. Habilitar secreto Unity del owner para GameCI cuando sea apropiado; luego repetir Unity suites remotas.
5. Revisar arte/audio y signing solo con recursos y autorización de release.
