# Roadmap revisado — 2026-09-30

1. **Finalización del loop móvil (implementado):** interacción primaria por tap/drag sobre la comida con pinza, feedback de selección, flip, bandeja, pausa, tutorial directo y ausencia de botones por corte. El debug de tiempo no aparece en release.
2. **Contenido/jugabilidad MVP (implementado y automatizado):** 18 alimentos con perfiles diferenciados y 6 estados visuales, 12 niveles/progreso/save v3, comensales y scoring existente. Suites locales: EditMode 19/19, PlayMode 10/10; validator ampliado.
3. **Android build / install (PASS), runtime smoke (bloqueado por AVD):** APK 1.2.0 code3 ARM64 IL2CPP API36 compilado, metadata/firma v2 verificadas e instalación ADB exitosa. El AVD `Pixel 7a` es x86_64 y SwiftShader solo anuncia GLES 3.1; Unity no inicializa la Graphics API, así que no se mostró gameplay L1 ni se capturó gameplay Android del refactor. No es APK productivo firmado ni publicación.
4. **QA táctil/performance físico (abierto antes de release):** un swipe sintético del APK anterior terminó en ANR (~8 s) y no confirmó bandeja por drag; no es evidencia del APK actual. El APK actual no llegó al gameplay por limitación gráfica del AVD. Repetir drag/drop a la tabla, multi-touch, overlap, bordes, pausa y safe area con Development Build + Unity Profiler y teléfono ARM64 de gama media.
5. **Evaluación de experiencia:** playtest humano progresivo L1–L12, ritmo por alimento, tutorial, legibilidad y volumen de audio; artwork y SFX actuales son procedurales/provisionales, requieren aprobación.
6. **Release futura:** cuando owner lo autorice, activar credenciales Unity para CI remoto y revisar metadata/políticas, firmar con keystore protegido fuera de Git y solo después preparar Play Store.

No se agregan carnicería, economía, multijugador, NPCs, anuncios ni otras features fuera del MVP.
