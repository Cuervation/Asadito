# Auditoría de expansión — 2026-09-29

| Gate | Estado | Evidencia / límites |
|---|---|---|
| Fuente de verdad Git/Unity | Revisado | Checkout `/Users/celestino/Asadito`, `main`, remote `origin`; remoto GitHub autenticado. El directorio alias Codex `Documents/ChatGPT/Asadito` no es checkout. |
| Catálogo / perfiles | PASS | 18 IDs requeridos, todas las firmas térmicas distintas; JSON validados también en runtime. Cortes/profile/motor separados; solo provoleta tiene regla específica declarada por data flag. |
| Art de comida / seis estados | PASS automatizable | 18 atlas 1024×1536 RGBA, 6 frames cada uno =108 recortes runtime no-null; EditMode comprueba todos y los estados clave. PROVISIONAL visualmente. |
| Progresión/niveles | PASS | 12 niveles, 12 ilustraciones PNG; save v2/migración v1; PlayMode cooking→serving→results/unlock→selector final L1–L12. |
| Cara/flip | PASS automatizable | Estado y sprites separados por cara; PlayMode verifica ideal↔raw↔ideal tras flip. |
| Serving/score/guests | PASS automatizable | Allocator reparto justo, prioridad cobertura/gusto/punto; scoring ponderado 40/30/20/10 con inputs 0–100; seis guests distintos; favorito=100. |
| Safe area / capas | PASS emulado, pendiente físico | Probado `Screen.safeArea` en emulador API36 con cutout alto Android simulado (inset superior 126 px, 1080×2400); selector y gameplay quedan dentro. No hubo teléfono físico ni navbar/gestos reales. |
| Escalas / legibilidad | PASS con ajuste | Revisados 1080×2400, 720×1600 y 720×1280. Tarjetas, títulos y botones caben; en 720×1280 el feedback de cocción quedaba demasiado cerca del botón carbón. Se subió el ancla vertical de `.342` a `.36` y se verificó el espacio en 720×1280 y 1080×2400. Texto auxiliar queda compacto en la resolución menor. |
| Motion/VFX/audio | Integrado, PROVISIONAL | Lista real runtime en `docs/art/animation-manifest.md`. Procedural; sin clips por tipo de comida, partículas production o mezcla audio aprobada. |
| EditMode | PASS | **18 passed / 0 failed**; Unity 6000.6.3f1, repetido tras el ajuste de viewport; XML `/tmp/Asadito-mobile-qa-editmode.xml`. |
| PlayMode | PASS | **6 passed / 0 failed**; los 12 niveles completos, retry/UI/progresión, atlas/6 estados por cara; repetido tras el ajuste, XML `/tmp/Asadito-mobile-qa-playmode.xml`. |
| Content/diff check | PASS | Validator imprime 18 unique profiles,108 cooking sprites,12 level illustrations; `git diff --check` PASS. |
| Android | PASS build/install | Unity 6000.6.3f1, build Android ARM64 IL2CPP `Succeeded`, APK ~50MB; `com.cuervation.asadito` 1.1.0 code2, min26, target36, portrait; firma debug. APK final (cambio menor de layout) reinstalado y ejecutado en Pixel 7a emulator API36. No release signing/Play publish. |
| GitHub CI | PASS parcial | Run [36592407375](https://github.com/Cuervation/Asadito/actions/runs/36592407375): validator de contenido y `git diff --check` PASS. Job Unity omitido por gate de licencia no habilitado. Un primer run detectó checkout superficial sin `HEAD^`; se corrigió en `6925c2b` y el rerun quedó PASS. |
| Touch / flujo de juego | PASS parcial en emulador | ADB touchscreen taps completaron portada → selector → intro → encender carbón → seleccionar dos piezas → bandeja 2/2 → servir → resultados (126/200, 2 estrellas). `input swipe` no confirmó arrastre de brasas/comida; no conectado teléfono físico. Esto no certifica gestos humanos/táctiles. |
| Rendimiento / recursos | No concluyente | Snapshot en 1080×2400: app PSS ≈432 MiB, RSS ≈583 MiB; buffers gráficos importados ≈91 MiB. QEMU con SwiftShader consumió 621–709% CPU host; `gfxinfo` no dio muestras Unity (solo 60 Hz nominal). No inferir FPS ni autonomía: falta perfil en teléfono real. El control visible `DEBUG ×30` corre cocción a 30× y acelera demasiado la sesión manual. |
| Git / push | PASS | `dfa0072` (`feat(content): expand food catalog and progression`) y `6925c2b` (`fix(ci): fetch history for patch validation`) publicados a `origin/main`; rama sincronizada y worktree limpio al 2026-09-29. |

## Bloqueos reales no técnicos

QA en teléfono real y validación del arrastre (brasas/comida) siguen pendientes; ADB solo certifica el recorrido por taps del emulador. La cifra de rendimiento está contaminada por SwiftShader y el costo del editor/host. Para ejecutar GameCI remoto, el owner debe habilitar `UNITY_CI_LICENSE_READY=true` y secretos de licencia de Unity. Firma productiva/publicación están fuera de autorización. Ninguna de esas cosas bloqueó tests/build locales.
