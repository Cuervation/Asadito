# Estado del MVP — expansión de catálogo (2026-09-29)

## Implementado

- 18 alimentos únicos desde JSON data-driven; motor térmico genérico por perfil, con bandas, masa, espesor, tasas, humedad, Maillard, char, grasa, split risk y comportamiento de queso configurables.
- 18 PNG RGBA de 1024×1536, cada uno con seis estados, 108 representaciones recortadas/cacheadas por el runtime; import Android mobile-safe.
- 12 niveles ilustrados y progresión; IDs de las 18 comidas incluidos progresivamente; save v2 migra filas del save v1 de cinco niveles.
- Seis perfiles de comensal con edad/peso/apetito/punto y favoritos/gustados/rechazados. Preferencia 0/40/70/100, favorito=100; allocator distribuye una porción por persona antes de dar segundas.
- Cara independiente, parrilla 8×6, brasa y calor local, drag/flip/retiro/tray/servicio, score/estrellas/progreso/tutorial/Retry/Next.
- Canvas de gameplay aislado en `gameplayRoot`; overlays y menú bajo safe area permanecen activos. Audio 8 cues procedural + sizzle y feedback visual procedural de comida/fire/UI/guests/resultados.
- CI de contenido y suite opcional Unity preparada; script repetible Android ARM64/IL2CPP API36.

## Verificación actual

- EditMode **18/18 PASS** y PlayMode **7/7 PASS** ejecutados con Unity 6000.6.3f1; PlayMode completa los doce niveles, seis estados/cara y verifica que `Update` no cancela el tween de calor.
- `Tools/validate_food_content.py`: PASS (18 definiciones/perfiles, 108 sprites, 12 cartas). GitHub Actions run [36592407375](https://github.com/Cuervation/Asadito/actions/runs/36592407375): validator y whitespace-check PASS. El job Unity remoto se omitió por el gate de licencia desactivado; ambas suites Unity sí pasaron localmente.
- APK Android ARM64 IL2CPP construye correctamente e instala en emulador Pixel 7a Android 16/API36. El recorrido ADB por taps llegó a resultados Nivel 1 (126/200, 2 estrellas); un gesto de arrastre en la build actual reprodujo un input-dispatch ANR (MOVE sin respuesta por 6.4 s) en SwiftShader, por lo que el touch y el rendimiento quedan bloqueados para investigación. Un drag de comida/bandeja solo funcionó en build anterior.
- No se afirma QA física ni éxito del gesto tras el cambio. Falta perfilar Development Build con Unity Profiler y validar en teléfono real. Retención de assets, nombres, estados y futuras pruebas en `docs/gameplay/food-catalog.md` y `docs/art/*manifest.md`.

## PROVISIONAL / fuera de gate automatizado

Arte y audio integrados pero no aprobados profesionalmente. No hay clips Animator authored únicos para cada comida; las animaciones son interacciones genéricas compartidas con arte/perfiles diferentes. Las temperaturas son tuning de gameplay, **no** consejo de inocuidad ni modelo científico. Sin teléfono físico, firma de distribución, check Play Console o publicación.

Ver [auditoría y evidencia](mvp-audit.md), [build Android](../android-release.md), [catálogo completo](../gameplay/food-catalog.md) y [progresión](../gameplay/level-progression.md).
