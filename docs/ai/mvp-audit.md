# ASADITO — MVP Audit (2026-09-28)

| Gate | Estado | Evidencia / pendiente |
|---|---|---|
| Compilación en Unity | PASS (Editor real) | Código actualizado importado/compilado por el Editor 6000.6.3f1; EditMode 13/13 y PlayMode 6/6 pasaron desde el Editor conectado. Build Android anterior a los últimos fixes C#. |
| EditMode | PASS (Editor real) | 13/13 pruebas pasaron en el Editor conectado (job `afc695658d5845e99c6d85d49ee56ef1`). |
| PlayMode | PASS (Editor real) | Suite 6/6 pasó en el Editor conectado (job `7d2f3e02417249ffb3d29eedf5b949d0`), incluye loop L1–L5, Retry, UI, iconos, ignition, retratos y assets. |
| Nivel 1: loop completo | PASS | Entrar→selección/intro→brasas→drag/flip→cocción→bandeja→servir→resultado/save/unlock; Retry resetea. A ×30 scripted duró 43.8s. |
| Catálogo y shell niveles 1–5 | PASS | Botones/porciones dinámicos observados: L1 2, L2 3, L3 4, L4 4 (vacío), L5 6 (incluye provoleta). |
| Cooking→serve→results para L1–L5 | PASS (acelerado) | Test cocina cada menú, sirve, guarda/puntúa y desbloquea Next L1→L5; L5 vuelve a selección. Usa ×1200 de test y no certifica ritmo normal. |
| Retry / Next / progreso guardado | PARTIAL | Unlock/score/Next pasa en todos; Retry/reset y tutorial/save se validan en L1, no hay Retry separado en L2–L5. |
| Mecánicas táctiles y duración | PARTIAL | `ExecuteEvents` valida pointer/drag; L1 ×30 scripted bajo 180s, pero falta tacto real, deliberación humana y safe-area device. |
| Dirección visual final | PARTIAL | Portada, niveles, intro, gameplay y resultados se inspeccionaron en GameView portrait 540×960; iconos CTA/back/retry quedaron dentro de sus botones. Se conservan los assets/GUIDs y tests. Falta QA en dispositivo: safe area, touch, legibilidad/rendimiento y aprobación final. |
| Audio, animación, VFX | PARTIAL | Sizzle sintético, humo, glow y secuencias procedurales básicas de ignition, tween de brasas, lift/release, flip, tray, reacción, transición de resultados y score count; faltan polish/QA móvil, audio/VFX de producción y blending. |
| Icono de app | PARTIAL | PNG 1254×1254 actualizado a pictograma propio y GUID adaptativo conservado en 6 densidades/12 capas; el APK de validación temporal sí contiene el icono actualizado; falta launcher/device QA y publicar build desde checkout oficial. |
| Android build | PARTIAL | APK anterior (`/tmp/AsaditoFinalVisualUI.apk`) 47 MB compiló con 0 errores/1 warning no bloqueante y firma Debug v2, pero precede los fixes C# recientes. No instalado/probado en teléfono ni copiado a `Builds/Android`. |
| Referencias Unity | PASS (aislado) | Se eliminaron siete GUIDs colgantes en campos URP 2D obsoletos de `Assets/Settings/Renderer2D.asset`. Tras el cambio, scan de referencias GUID serializadas en Assets: 0 unresolved; Unity batch abrió 2 escenas/0 prefabs y revisó 14 componentes, con 0 missing scripts (`/tmp/AsaditoReferenceAudit.final.log`). |
| GitHub push | BLOCKED | Los cambios de MVP permanecen en `main` local. Push normal se reintentó y falló con `fatal: could not read Username for 'https://github.com': terminal prompts disabled`; `gh auth status` confirma que no hay sesión. Requiere `gh auth login` y push normal; no se hizo force push. |
| Unity MCP | PASS (consola intermitente) | HTTP enumera `Asadito@65a4fad638bbc94c`; state, escena y jerarquía respondieron. Unity importó código/arte y corrió ambas suites; Editor quedó idle en `SampleScene`, no dirty. Consola: 1 warning `WebSocket is not initialised` originado en `WebSocketTransportClient`; sin errores de app/compilación. |

**Resultado:** `MVP PARTIAL`. Gates de esta matriz: 8/15 completos, 6 parciales y 1 bloqueado por autenticación GitHub. Los loops y ambas suites pasan en el Editor real. Se hizo review del GameView portrait; siguen pendientes dispositivo real, safe area/touch/performance, audio/VFX final y APK recompilado con los últimos fixes. Los tests capturan/restauran `PlayerPrefs`; el ciclo L1–L5 usa ×1200 solo para acelerar test. Capturas/Builds y copia temporal son artefactos locales ignorados.

## Próximas prioridades (máximo 3)
1. Resolver QA de dispositivo: portada/UI en safe areas, legibilidad, touch y rendimiento; completar motion/VFX/audio final y generar/probar APK vigente.
2. Hacer una última lectura de consola Unity MCP (su conexión respondió pero fue intermitente).
3. Tras el login autenticado de GitHub, subir `main` mediante push normal (sin force push).
