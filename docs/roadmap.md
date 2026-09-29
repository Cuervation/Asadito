# Roadmap revisado

1. **Contenido / progresión (implementado):** 18 perfiles y seis estados visuales por alimento, 12 niveles ilustrados, tabla de [catálogo jugable](gameplay/food-catalog.md) y [progresión](gameplay/level-progression.md).
2. **Regresión / Android (validado localmente):** EditMode/PlayMode, validator de contenido, build ARM64 API36 e instalación/arranque en emulador; evidencia en [`mvp-audit`](ai/mvp-audit.md). GitHub CI corre validator, Unity tests requieren activar secrets de licencia.
3. **QA humana/móvil:** revisar interacción/touch, safe area, escalas/aspect ratios, texto, rendimiento/audio en Android real y aprobar calidad/consistencia del arte generado.
4. **Release prep:** probar launcher/icono adaptativo, metadatos de privacidad/política y firmar con keystore protegido; no subir secretos al repo ni publicar automáticamente.
5. **Iteración post gate:** segmentar el coordinador UI/gameplay si el siguiente ciclo lo justifica; añadir clips/VFX/audio profesionales según feedback visual, presupuesto y dirección artística.

No hay metas de carnicería/economía/multiplayer en este ciclo.
