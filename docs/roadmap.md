# Roadmap revisado — 2026-09-29

1. **Finalización del loop móvil (implementado):** interacción primaria por tap/drag sobre la comida con pinza, feedback de selección, flip, bandeja, pausa, tutorial directo y ausencia de botones por corte. El debug de tiempo no aparece en release.
2. **Contenido/jugabilidad MVP (implementado y automatizado):** 18 alimentos con perfiles diferenciados y 6 estados visuales, 12 niveles/progreso/save v3, comensales y scoring existente. Ritmo inicial 20× tuneado frente a medición de ventanas térmicas. Suites locales: EditMode 20/20, PlayMode 10/10; validator ampliado.
3. **Android build / humo visual (PASS parcial):** APK 1.2.0 code3 ARM64 IL2CPP API36 compilado/instalado; se mostró gameplay L1 y tap directo a pieza. Se quitó configuración Bloom URP sin uso. No es APK productivo firmado ni publicación.
4. **QA táctil/performance físico (abierto antes de release):** el último swipe sintético en emulador terminó en ANR (~8 s) y no confirmó bandeja por drag; la traza muestra espera QEMU GL pero no determina causa. Repetir drag/drop a bandeja y brasas, multi-touch, overlap, bordes, pausa y safe area con Development Build + Unity Profiler y teléfono ARM64 de gama media. Emulador/SwiftShader no mide rendimiento útil.
5. **Evaluación de experiencia:** playtest humano progresivo L1–L12, ritmo por alimento, tutorial, legibilidad y volumen de audio; artwork y SFX actuales son procedurales/provisionales, requieren aprobación.
6. **Release futura:** cuando owner lo autorice, activar credenciales Unity para CI remoto y revisar metadata/políticas, firmar con keystore protegido fuera de Git y solo después preparar Play Store.

No se agregan carnicería, economía, multijugador, NPCs, anuncios ni otras features fuera del MVP.
