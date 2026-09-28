# ASADITO — MVP Audit (2026-09-28)

| Gate | Estado | Evidencia / pendiente |
|---|---|---|
| Compilación en Unity | PASS (aislado) | Unity 6000.6.3f1 compila PlayMode/EditMode y completa build Android (0 errores); el Editor GUI original no está ready para inspección MCP. |
| EditMode | PASS (aislado) | 13/13, 0 failed (`/tmp/AsaditoEditTests.current.xml`), incluye grilla/fuego, cocción, allocator, score/progreso. |
| PlayMode | PASS (aislado) | Suite 6/6 en copia temporal Unity (`/tmp/AsaditoValidation.QP0xpX`, XML `/tmp/AsaditoPlayTests.current.xml`), incluye loop L1–L5, L1 a ×30, Retry, logo/icono/CTA y sprites/retratos; verificación focal posterior 1/1 de tipografías/glifos (`/tmp/AsaditoVisualAssetsTest.current.xml`). |
| Nivel 1: loop completo | PASS | Entrar→selección/intro→brasas→drag/flip→cocción→bandeja→servir→resultado/save/unlock; Retry resetea. A ×30 scripted duró 43.8s. |
| Catálogo y shell niveles 1–5 | PASS | Botones/porciones dinámicos observados: L1 2, L2 3, L3 4, L4 4 (vacío), L5 6 (incluye provoleta). |
| Cooking→serve→results para L1–L5 | PASS (acelerado) | Test cocina cada menú, sirve, guarda/puntúa y desbloquea Next L1→L5; L5 vuelve a selección. Usa ×1200 de test y no certifica ritmo normal. |
| Retry / Next / progreso guardado | PASS (parcial en Retry) | Unlock/score/Next pasa en todos; Retry/reset y tutorial/save se validan en L1, no hay Retry separado en L2–L5. |
| Mecánicas táctiles y duración | PARTIAL | `ExecuteEvents` valida pointer/drag; L1 ×30 scripted bajo 180s, pero falta tacto real, deliberación humana y safe-area device. |
| Dirección visual final | PARTIAL | Parrilla cenital, atlas comida 16 sprites (4×4), 24 retratos (6×4), wordmark Lilita One, Baloo 2 y launcher icon original integrados; faltan review GUI/device y crop/responsive de la nueva portada, UI, estados adicionales de comida y VFX. |
| Audio, animación, VFX | PARTIAL | Sizzle sintético, humo, glow, flip/plate/reaction y háptica básica; falta producción y QA. |
| Icono de app | PARTIAL | PNG 1254×1254 actualizado a pictograma propio y GUID adaptativo conservado en 6 densidades/12 capas; el APK de validación temporal sí contiene el icono actualizado; falta launcher/device QA y publicar build desde checkout oficial. |
| Android build | PASS (aislado) | Unity Android CLI build actualizado en copia aislada: 46 MB, 0 errors/1 warning; AAPT confirma icon adaptive y apksigner Debug válido. No instalado/probado en teléfono ni copiado a `Builds/Android` ignorado. |
| GitHub push | BLOCKED | El remoto oficial público está vacío (sin refs); `gh auth status` da `You are not logged into any GitHub hosts`. Commit local se puede hacer; para publicar hace falta login seguro y push normal a `main`. |
| Unity MCP | UNVERIFIED | HTTP enumera `Asadito@65a4fad638bbc94c`, versión/proyecto correctos, pero `editor/state` y `read_console` devuelven `Unity session not ready (ping not answered)` incluso luego de encolar telemetry ping; no se pudo revisar consola/scene/render. El proceso Unity sigue abierto y no se reinició para preservar posible estado. |

**Resultado:** `MVP PARTIAL`. Los cinco loops completos ahora pasan automáticamente, pero siguen pendientes asset gate visual comercial, review del Editor GUI, build Android actualizado y device QA. Los tests capturan/restauran `PlayerPrefs`; el ciclo L1–L5 usa ×1200 solo para acelerar test. Capturas/Builds y copia temporal son artefactos locales ignorados.

## Próximas prioridades (máximo 3)
1. Reconnectar la instancia del Editor MCP, revisar consola/jerarquía/render y aprobar o corregir portada y layouts responsive.
2. Completar motion/VFX/audio, revisar safe areas/touch/performance y generar/probar Android en dispositivo.
3. Commit/push al remoto oficial al poder comparar y autenticar, sin force push.
