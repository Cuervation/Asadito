# Estado del MVP — finalización y QA (2026-09-29)

## Implementado

- Comida como interacción primaria: toque/drag directamente sobre cada pieza y target táctil transparente mínimo de 190×180; selección unificada, lift/halo/sombra y pinza animada; movimiento/settle, flip, bandeja por botón universal o drag-to-tray. Sin botones nombrados por alimento.
- Flujo L1–L12, 18 alimentos data-driven con perfil térmico distinto, 18 atlas y seis estados progresivos c/u (108 sprites), cara independiente, fuego/carbón, bandeja, comensales y scoring 40/30/20/10.
- Ritmo inicial ajustado de 30× a **20×** después de medir ventanas A_Punto en cada corte rápido: mínimo ~1.9 s para provoleta y máximo ~18.1 s para vacío a grilla de 210 °C; conserva entraña rápida y vacío lento. Guardado v3 migra los defaults previos sin sobrescribir elecciones custom.
- Menú pausa móvil (continuar, retry, selector, SFX y vibración), tutorial actualizado a manipulación directa, control debug solo en Editor/development.
- Audio sizzle/SFX procedural con volumen/pausa; safe area y dimensiones revisadas en simulación previa. Estos elementos aún necesitan validación humana/física.
- Validator ampliado verifica catálogo, 108 frames y consistencia alfa/silueta, niveles, perfiles y recursos de runtime.

## Verificación

- Unity 6000.6.3f1 local: EditMode **20/20 PASS**; PlayMode **10/10 PASS**; tras el ajuste idempotente del tutorial se repitieron ambas suites: `/tmp/asadito-final-rerun-editmode.xml` y `/tmp/asadito-final-rerun-playmode.xml`.
- `python3 Tools/validate_food_content.py`: PASS, 18 perfiles, 108 estados coherentes, 12 tarjetas y progresión completa.
- `git diff --check`: se repetirá después de la actualización de auditoría/documentación.
- Build Android de validación **PASS**: `/tmp/Asadito-mvp-1.2.0-arm64-final.apk`, ARM64 IL2CPP, API36, 1.2.0/code3; `aapt` configurado, firma debug; instalado y lanzado en emulador Pixel 7a API36. Se mostró L1, fuego y selección directa; captura `/tmp/Asadito-mvp-gameplay-selected.png`. Se eliminó spam de Bloom quitando referencia URP no usada.
- **Riesgo bloqueante de QA pendiente:** el último swipe ADB del APK final produjo ANR input (~8 s) y no confirmó drag-to-tray. Traza apunta a espera `glUnmapBufferAEMU_enc` en renderer QEMU/Android x86-64 con APK ARM64 traducido; origen no resuelto ni se atribuye solo al emulador. No declarar MVP release-ready hasta validar drag continuo/emplatado y profiler en teléfono ARM64. AssetPackManager opcional reporta ClassNotFound al inicio, sin crash fatal observado.
- Emulador no certifica rendimiento, batería, tacto humano, audio/haptics, cutout ni navbar físico. No se afirma QA física; la imagen demuestra solo un estado de interacción visible, no recorrido completo.
- GitHub Actions [36592407375](https://github.com/Cuervation/Asadito/actions/runs/36592407375): static validator y whitespace PASS; Unity workflow SKIPPED por licencia/secretos no provistos por owner.

## Límites pendientes antes de un release de tienda

- Teléfono real y Unity Profiler; prueba con dedos de arrastre y drop a bandeja, brasas, multitoque, solapes, gestos y safe areas.
- Revisión humana de artwork/audio y balance real del juego completo.
- Firma y publicación no están en el scope ni se generaron credenciales productivas.

Ver [auditoría](mvp-audit.md), [estado detallado](current-state.md) y [build Android](../android-release.md).
