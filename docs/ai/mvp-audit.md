# Auditoría MVP / QA — 2026-09-30

| Gate | Estado | Evidencia / límite |
|---|---|---|
| Composición 10bis | PASS visual en Editor / dispositivo pendiente | Fondo top-down con grill grande a izquierda; `MesitaAsador` a derecha; `TablaAsador` hija, drop y doble toque restringidos al rect visible. Captura `/tmp/Asadito-gameplay-layout-final.png` a 1080×1920. La prueba no es emulador ni teléfono físico y no sustituye QA de safe area. |
| Acciones físicas gameplay | PASS automatizable en 10bis anterior; proporciones pendientes | Se quitaron DAR VUELTA/BANDEJA/SERVIR; FoodPieceTouch selecciona, tap al mismo alimento voltea, drag a la tabla deposita; ServingBoardTouch sirve tras doble tap cuando completa. El 10/10 EditMode19/19 de esa ejecución corresponde al estado anterior; el flujo revisado con tamaños nuevos no se completó por el bloqueo de foco documentado abajo. |
| Mesa / slots | PASS técnico, arte provisional | Mesita generada original con alpha, tabla separada como hija. Slots deterministas en 2×3 para seis porciones; los tests de niveles incluyen pedidos hasta seis. Revisión artística y ergonomía en device siguen pendientes. |
| Base y alcance | PASS | HEAD inicial verificado: `f0a6126bca756f7f49ed5fdeacf56ce755d12962`, `main`. Se retira la manipulación de carbón sin expandir sistemas. |
| Calor | PASS | `GrillHeatModel` entrega 210 °C uniforme al entrar; no existe requisito `IsLit`, combustible, decaimiento ni mapa térmico. `FoodCookingModel` y estados por cara permanecen activos. |
| Interacción directa | PASS automatizable | `FoodPieceTouch` conduce tap/drag a una sola selección; pruebas PlayMode de tap directo, dueño de pointer/multitouch, drag, flip y emplatado. Sin botones por nombre de comida. |
| Props de parrilla | PASS técnico / arte provisional | La pinza abierta/cerrada y `TablaAsador.png` se cargan como sprites runtime; test PlayMode comprueba asset/conexión; test de drag valida emplatado. Evaluación artística y tacto humano siguen pendientes. |
| Perfil/atlas de comida | PASS técnico | 18 FoodIds/perfiles y 108 sprites (RAW/WARMING/BROWNING/IDEAL/OVERCOOKED/BURNT). Validator/auditoría técnica y tests validan dimensiones, alpha y disponibilidad por ID; no es aprobación visual profesional. |
| Proporciones / huella física | PASS estático y EditMode; runtime PlayMode pendiente | Catálogo schema 2 ancla Chorizo a 1.0 y asigna área relativa individual a los 18 cortes; aspecto original preservado, colocación MaxRects para L1–L12 y bloqueo de overlaps; test de capacidad confirma 10 chorizos sí / 10 vacíos no. Hitbox deriva del sprite con margen móvil. Comparativa estática Unity Editor `/tmp/asadito-food-scale-comparison.png` (no sesión jugable). Drag/hitbox manuales siguen pendientes. |
| Progresión/save | PASS automatizable | PlayMode cocina y sirve L1–L12, revisa resultados, estrellas, unlock/Next, regresa a selector; EditMode valida save/migración/allocator/scoring. |
| Pause/tutorial/settings | PASS automatizable en 10bis anterior | La suite PlayMode previa cubrió pause/resume/restart/selector, sonido/vibración y onboarding de comida/pinza. No se revalidó en esta actualización de escalas. |
| Debug | PASS por código/config | `CONTROL DEBUG` sólo se construye en Editor/development (`Debug.isDebugBuild`); APK Android compilado como release no-development. La UI del player no pudo inspeccionarse en el AVD por incompatibilidad gráfica. |
| EditMode | PASS en esta actualización | 21/21 en Unity 6000.6.3f1; incluye área/aspecto Chorizo y pack/capacidad grill. XML generado por Unity MCP. |
| PlayMode | PENDIENTE para proporciones | La ejecución actual llegó hasta 8/10 y se atascó al perder foco el Editor en el test normal L1–L12 (`editor_unfocused`); job limpiado, sin fallas reportadas. El 10/10 / 486.94 s de `/Users/celestino/Library/Application Support/Cuervation/Asadito/TestResults.xml` es del código previo y no valida este cambio. |
| Validator | PASS en esta actualización | `python3 Tools/validate_food_content.py`: 18 perfiles, 108 estados, 12 cartas, huellas/layout L1–L12, capacidad y recursos runtime. |
| Console Unity | PASS de compilación, con límite | Tras refresh, Unity no reportó errores de compilación. El bridge MCP sí registró una advertencia WebSocket `Unexpected receive error: WebSocket is not initialised`; no atribuir 0 advertencias a esta sesión. |
| Android build | PASS | Unity 6000.6.3f1 compiló APK ARM64 IL2CPP; package/version/code, min26/target36, ABI ARM64 y firma v2 verificadas (`/tmp/Asadito-always-hot-1.2.0-arm64.apk`, 53 MiB). |
| Android install | PASS | ADB instaló correctamente en AVD `Asadito_Pixel_7a_API_36` (API36, 1080×2400); esto confirma empaquetado/instalación, no compatibilidad física ARM64. |
| Android runtime smoke | BLOQUEADO POR AVD | Imagen de AVD x86_64; al abrir el APK ARM64, Unity reportó que no pudo inicializar Unity Engine Graphics API. SwiftShader expone GLES 3.1, incompatible con los contextos ES 3.2/3.1 solicitados por Unity. No hubo menú, gameplay ni captura de gameplay. |
| Drag continuo / ANR | NO VERIFICADO EN NUEVO APK | El ANR de swipe/drop no confirmado corresponde al APK anterior. La nueva build no llegó a gameplay en este emulador, por lo que el cambio de parrilla no lo confirma ni lo reproduce. Repetir con teléfono ARM64 y profiler. |
| Dispositivo físico / arte y audio | PENDIENTE | No se ha validado touch humano, notch/navbar reales, rendimiento sostenido en ARM64, SFX/haptics en dispositivo ni aprobación visual final. |
| CI posterior al push | PASS PARCIAL | GitHub Actions `36664620467` concluyó `success`; job “Validate authored content” (validator + `git diff --check`) PASS. El job “Unity EditMode and PlayMode” quedó `skipped`; no cuenta como prueba remota. Tests locales Unity 19/19 y 10/10 sí corrieron. [Ejecución](https://github.com/Cuervation/Asadito/actions/runs/36664620467). |
| Signing/publicación | Fuera de alcance | Se usa firma debug de validación; no hay keystore productivo ni publicación. |

## Riesgos / siguiente QA

1. Construir APK nuevo con el rediseño y repetir runtime smoke en teléfono ARM64: abrir, recorrer Nivel 1 con tap, drag continuo, flip por segundo tap, drop a tabla, doble toque, resultados, Retry y pausa.
2. Capturar evidencia visual Android del gameplay con pieza seleccionada, pinza, tabla y parrilla top-down; la captura Editor presente solo valida la composición de referencia y el AVD histórico solo contiene bloqueo gráfico.
3. Si swipe causa ANR en un dispositivo compatible, obtener trace/profiler y medir memoria/FPS/GC antes de atribuirlo.
4. Probar notch/nav gesture, audio/haptics y sign-off artístico con teléfono real; tests automatizados no lo sustituyen.
5. Revisar CI remoto posterior al push; Unity CI puede permanecer omitido por licencia/secrets sin que eso invalide tests locales.
