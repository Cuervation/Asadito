# ASADITO — MVP Audit (2026-09-28)

| Gate | Estado | Evidencia / pendiente |
|---|---|---|
| Compilación en Unity | PASS (aislado) | Unity 6000.6.3f1 compila PlayMode/EditMode y completa build Android (0 errores); el Editor GUI original no está ready para inspección MCP. |
| EditMode | PASS (aislado) | 13/13, 0 failed (`/tmp/AsaditoFinalEditTests.xml`), incluye grilla/fuego, cocción, allocator, score/progreso. |
| PlayMode | PASS (aislado) | Suite actual 6/6 en copia temporal Unity (`/tmp/AsaditoValidation.QP0xpX`, XML `/tmp/AsaditoFinalPlayTests.xml`), incluye loop L1–L5, L1 a ×30, Retry, logo/CTA, iconos de comida/UI, cue de ignition, resultados y sprites/retratos. |
| Nivel 1: loop completo | PASS | Entrar→selección/intro→brasas→drag/flip→cocción→bandeja→servir→resultado/save/unlock; Retry resetea. A ×30 scripted duró 43.8s. |
| Catálogo y shell niveles 1–5 | PASS | Botones/porciones dinámicos observados: L1 2, L2 3, L3 4, L4 4 (vacío), L5 6 (incluye provoleta). |
| Cooking→serve→results para L1–L5 | PASS (acelerado) | Test cocina cada menú, sirve, guarda/puntúa y desbloquea Next L1→L5; L5 vuelve a selección. Usa ×1200 de test y no certifica ritmo normal. |
| Retry / Next / progreso guardado | PARTIAL | Unlock/score/Next pasa en todos; Retry/reset y tutorial/save se validan en L1, no hay Retry separado en L2–L5. |
| Mecánicas táctiles y duración | PARTIAL | `ExecuteEvents` valida pointer/drag; L1 ×30 scripted bajo 180s, pero falta tacto real, deliberación humana y safe-area device. |
| Dirección visual final | PARTIAL | Parrilla/portada cenital, atlas comida 16 sprites (4×4), 24 retratos (6×4), wordmark Lilita One, Baloo 2, launcher icon, iconos de comida y 11 marcas UI procedurales ya integrados; PlayMode valida su uso en selección, intro, acciones y resultados. Falta review GUI/device, responsive, más bandas/estados visuales y polish VFX. |
| Audio, animación, VFX | PARTIAL | Sizzle sintético, humo, glow y secuencias procedurales básicas de ignition, tween de brasas, lift/release, flip, tray, reacción, transición de resultados y score count; faltan polish/QA móvil, audio/VFX de producción y blending. |
| Icono de app | PARTIAL | PNG 1254×1254 actualizado a pictograma propio y GUID adaptativo conservado en 6 densidades/12 capas; el APK de validación temporal sí contiene el icono actualizado; falta launcher/device QA y publicar build desde checkout oficial. |
| Android build | PASS (aislado) | Rebuild Android posterior a UI/motion: APK 47 MB, `Succeeded`, 0 errores/1 warning C++ no bloqueante; AAPT confirma package y `apksigner` verifica debug scheme v2 (`/tmp/AsaditoFinalVisualUI.apk`). No instalado/probado en teléfono ni copiado a `Builds/Android` ignorado. |
| Referencias Unity | PASS (aislado) | Se eliminaron siete GUIDs colgantes en campos URP 2D obsoletos de `Assets/Settings/Renderer2D.asset`. Tras el cambio, scan de referencias GUID serializadas en Assets: 0 unresolved; Unity batch abrió 2 escenas/0 prefabs y revisó 14 componentes, con 0 missing scripts (`/tmp/AsaditoReferenceAudit.final.log`). |
| GitHub push | BLOCKED | Último commit local de implementación `45f5984` en `main`; el remoto no expone refs. Push normal de `main` reintentado tras la actualización docs y bloqueado: `fatal: could not read Username for 'https://github.com': terminal prompts disabled`. Hace falta login seguro (`gh auth login`) y reintentar push normal; no se hizo force push. |
| Unity MCP | UNVERIFIED | HTTP enumera `Asadito@65a4fad638bbc94c`; `project/info` confirma Unity 6000.6.3f1. `manage_scene.get_active` devolvió `SampleScene`; refresh recuperó el servidor una vez, pero state/console fallaron por ping/timeouts y `get_hierarchy` se desconectó. No se pudo revisar consola/render en vivo. El Editor no se terminó para preservar estado. |

**Resultado:** `MVP PARTIAL`. Gates de esta matriz: 8/15 completos, 5 parciales, 1 bloqueado por autenticación GitHub, 1 no verificable por desconexión MCP. Los cinco loops pasan automáticamente; siguen pendientes asset gate visual comercial, review del Editor GUI y device QA. El APK Android actualizado compila, pero no está probado en teléfono. Los tests capturan/restauran `PlayerPrefs`; el ciclo L1–L5 usa ×1200 solo para acelerar test. Capturas/Builds y copia temporal son artefactos locales ignorados.

## Próximas prioridades (máximo 3)
1. Reconnectar la instancia del Editor MCP, revisar consola/jerarquía/render y aprobar o corregir portada y layouts responsive.
2. Completar motion/VFX/audio, revisar safe areas/touch/performance y generar/probar Android en dispositivo.
3. Tras el login autenticado de GitHub, subir `main` mediante push normal (sin force push).
