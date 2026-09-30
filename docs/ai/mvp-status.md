# Estado MVP ASADITO — 2026-09-30

## Implementado

- Parrilla always-hot uniforme a 210 °C; se eliminaron carbón/encendido, combustible, heat-grid/brasas táctiles, glow de gameplay, SFX de ignition y tutorial asociado. Cocción de alimentos, caras/flip, Maillard, humedad, char y perfiles siguen conectados.
- Tocar comida directamente selecciona la pieza; hitbox invisible ampliada, pinza ilustrada abierta/cerrada y movimiento/flip; tabla de asador ilustrada como destino por drag o acción universal. No existen botones individuales por corte.
- Catálogo data-driven de 18 alimentos, 108 estados visuales y progresión de 12 niveles; pause/settings mínimos, scoring, save y debug oculto fuera de Editor/development.
- Specs, arquitectura y manifests documentan la regla y los assets conectados.

## Verificación local

- EditMode Unity 6000.6.3f1: **19/19 PASS** (`/tmp/asadito-always-hot-editmode.xml`).
- PlayMode Unity 6000.6.3f1: **10/10 PASS** (486.61 s), incluye cocinar/servir todos los niveles L1–L12; XML en `/Users/celestino/Library/Application Support/Cuervation/Asadito/TestResults.xml`.
- Validator `python3 Tools/validate_food_content.py`: **PASS**, 18 perfiles, 108 sprites, 12 cartas, progresión y recursos runtime.
- Console Unity al final de la suite: 0 errors/warnings.

## Pendiente antes de declarar release/QA físico

- Build actual del refactor: Android ARM64 IL2CPP, API36, package/version/code verificados; firma v2 debug. APK `/tmp/Asadito-always-hot-1.2.0-arm64.apk` (53 MiB); `aapt`, `unzip` y `apksigner` pasaron.
- Instalación ADB en `Asadito_Pixel_7a_API_36` pasó, pero el AVD es x86_64, no ARM64. Unity no pudo inicializar Graphics API (SwiftShader anuncia GLES 3.1 y el player intenta ES 3.2/3.1); no alcanzó menú ni gameplay y no hay captura Android de gameplay. Evidencia del error: `/tmp/asadito-qa/02-after-hardware-warning.png`.
- El ANR histórico de swipe pertenece al APK previo al refactor; no se declara ni resuelto ni reproducido en la nueva build, porque ésta no llegó a renderizar en el AVD.
- Faltan pruebas con dedos/hardware físico (drag/multitouch/notch/navbar), audio/haptics real, rendimiento sostenido y sign-off humano de arte.
- El resultado del CI remoto se verificará luego del push; no se atribuye PASS al pipeline Unity remoto si está limitado por licencia/secrets del owner.

## QA local completado

- EditMode 19/19 PASS y PlayMode 10/10 PASS (incluye L1–L12); `Tools/validate_food_content.py` PASS; consola Unity limpia al final de tests.
- `git diff --check`, commit/push: registrar resultado final en el cierre de esta ejecución.

API36 es el target configurado para Android 16 y cubre el requisito actual de envíos a Google Play desde el 31-08-2026 ([documentación oficial](https://support.google.com/googleplay/android-developer/answer/11926878?hl=es)). Build de validación debug; no hay firma productiva/publicación.

Ver [auditoría](mvp-audit.md), [estado técnico](current-state.md) y [build Android](../android-release.md).
