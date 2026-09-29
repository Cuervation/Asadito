# Roadmap revisado

1. **Contenido / progresión (implementado):** 18 perfiles y seis estados visuales por alimento, 12 niveles ilustrados, tabla de [catálogo jugable](gameplay/food-catalog.md) y [progresión](gameplay/level-progression.md).
2. **Regresión / Android (validado localmente):** EditMode/PlayMode, validator de contenido, build ARM64 API36 e instalación/arranque en emulador; evidencia en [`mvp-audit`](ai/mvp-audit.md). GitHub CI corre validator, Unity tests requieren activar secrets de licencia.
3. **QA humana/móvil (parcial):** se probaron los taps hasta resultados, tres resoluciones y un cutout simulado; se corrigió el espaciado del feedback. El heat grid ahora evita repintar 48 celdas cada frame y pasó su regresión PlayMode, pero un swipe ADB posterior volvió a reproducir un ANR de input-dispatch en el emulador SwiftShader; su causa raíz sigue abierta. Pendiente Unity Profiler/Development Build, teléfono real, gestos táctiles, FPS/memoria en hardware y aprobación humana de arte/audio. DEBUG ×30 no es bug: escala inicial 30 y ciclos DEBUG 20/25/30/35/40 están declarados en specs.
4. **Release prep:** probar launcher/icono adaptativo, metadatos de privacidad/política y firmar con keystore protegido; no subir secretos al repo ni publicar automáticamente.
5. **Iteración post gate:** segmentar el coordinador UI/gameplay si el siguiente ciclo lo justifica; añadir clips/VFX/audio profesionales según feedback visual, presupuesto y dirección artística.

No hay metas de carnicería/economía/multiplayer en este ciclo.
