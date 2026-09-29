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

- EditMode **18/18 PASS** y PlayMode **6/6 PASS** ejecutados con Unity 6000.6.3f1; PlayMode completa los doce niveles y comprobación de seis estados/face.
- `Tools/validate_food_content.py`: PASS (18 definiciones/perfiles, 108 sprites, 12 cartas). `git diff --check`: PASS antes del commit.
- APK Android construye correctamente, pasa inspección `aapt`/firma debug `apksigner`, se instala y lanza en el emulador Pixel 7a Android 16/API36. Screenshot de portada disponible.
- No se afirma QA física. Emulador muestra la portada; la interacción manual/táctil no se considera certificada. Retención de assets, nombres, estados y futuras pruebas en `docs/gameplay/food-catalog.md` y `docs/art/*manifest.md`.

## PROVISIONAL / fuera de gate automatizado

Arte y audio integrados pero no aprobados profesionalmente. No hay clips Animator authored únicos para cada comida; las animaciones son interacciones genéricas compartidas con arte/perfiles diferentes. Las temperaturas son tuning de gameplay, **no** consejo de inocuidad ni modelo científico. Sin teléfono físico, firma de distribución, check Play Console o publicación.

Ver [auditoría y evidencia](mvp-audit.md), [build Android](../android-release.md), [catálogo completo](../gameplay/food-catalog.md).
