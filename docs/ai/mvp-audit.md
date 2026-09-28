# ASADITO — MVP Audit (2026-09-27)

| Gate | Estado | Evidencia / pendiente |
|---|---|---|
| Compilación en Unity | PASS | Unity 6000.6.3f1; Editor recargó los scripts. |
| EditMode | PASS | 13/13, 0 failed; job `efa711f9e8304581b9bf40eb589c7841`. |
| Suite PlayMode | BLOCKED | Última ejecución informó 0 tests; no hay cobertura PlayMode efectiva. |
| Nivel 1: entrada a gameplay | PASS | Menu→Level Select→Intro→Gameplay observado en Editor. |
| Catálogo y shell niveles 1–5 | PASS | Botones/porciones dinámicos observados: L1 2, L2 3, L3 4, L4 4 (vacío), L5 6 (incluye provoleta). |
| Cooking→serve→results para L1–L5 | PARTIAL | Implementado en código; sin recorrido completo reproducible de todos los niveles ni test automatizado. |
| Retry / Next / progreso guardado | UNVERIFIED | Código presente; no verificado tras un ciclo completo por nivel. |
| Mecánicas táctiles y duración | UNVERIFIED | No hubo prueba en dispositivo ni calibración 2–3 / 5–7 min. |
| Dirección visual final | BLOCKED | Parrilla top-down está integrada pero es fotográfica; comida, guest avatars y UI siguen mayormente procedural/provisionales. |
| Audio, animación, VFX | PARTIAL | Sizzle sintético, humo, glow, flip/plate/reaction y háptica básica; falta producción y QA. |
| Icono de app | PARTIAL | Adaptive Android conectado en 6 densidades/12 capas, recurso presente en APK; arte fotográfico y sin launcher/device QA. |
| Android build | PASS* | `Builds/Android/Asadito.apk`, arm64, ~43 MB; ZIP/firma Android Debug válidos, 0 errores/1 warning Android diagnostics. Sin QA de dispositivo/distribución; package ID sigue `com.DefaultCompany.Asadito`. |
| GitHub push | BLOCKED | `gh` sin autenticar; no se pudo subir la rama. |
| Unity MCP | PASS | HTTP con una instancia activa; `editor/state` devolvió `ready_for_tools=true`, no stale. Llamadas de recursos y herramientas completadas. |

**Resultado:** `MVP PARTIAL`. No marcar completo hasta pasar los gates de acceptance en Editor/device, validar el APK en dispositivo y publicar el estado solicitado. No se modificaron escenas ni progreso guardado: tras detener Play Mode el save local sigue en L1, 0 estrellas/puntajes. La navegación creó Canvas/objetos temporales en Play Mode; la captura local está bajo `Assets/Screenshots/` e ignorada por Git. El APK en `Builds/Android/` también está ignorado por Git. `PASS*` indica una salida existente/verificada que conserva warnings pendientes.

## Próximas prioridades (máximo 3)
1. Completar y automatizar el loop en todos los niveles, preservar/revisar progresión y ejecutar PlayMode real.
2. Sustituir el arte fotográfico/procedural por el set estilizado (parrilla, cuatro alimentos/estados, seis retratos, resultados, icono) y hacer QA portrait/touch.
3. Probar el APK en un teléfono, luego autenticar `gh` y publicar commits coherentes.
