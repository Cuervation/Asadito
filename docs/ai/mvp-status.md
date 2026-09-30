# Estado MVP ASADITO — 2026-09-30

> Registro fechado de verificación y pendientes; para comportamiento operativo actual usar [`docs/current-project-state.md`](../current-project-state.md). Los resultados pertenecen al alcance/commit anotado en cada sección.

## Actualización de proporciones físicas de alimentos (2026-09-30)

- `FoodCatalog.json` schema 2: `chorizo` permanece en huella 1.0 con su rect legado (236×176×0.92); los otros 17 tamaños se definen en `FootprintAreaMultiplier` y usan el aspecto de su atlas.
- `FoodFootprintLayout` calcula size sin estirar, packea por área real, comprueba límites/solapamientos del drag y genera hit targets basados en el tamaño visible. Nivel inicial respeta los mismos rectángulos; los slots del board escalan uniformemente para conservar el aspecto.
- EditMode actual 21/21 PASS; validator actual PASS (18 IDs, 108 atlas frames, layouts L1–L12 y prueba de capacidad Chorizo/Vacío); `git diff --check` PASS. Comparativa Unity Editor 1080×1920: `/tmp/asadito-food-scale-comparison.png`.
- PlayMode del cambio **no verificado**: tras 8 de 10 tests el Editor reportó `editor_unfocused` en el test normal L1–L12, sin fallas reportadas; se detuvo/limpió ese job. El 10/10 documentado abajo fue antes de este cambio. No se generó APK nueva ni se revisó en teléfono.

## Implementado

### Actualización 10bis — layout e interacción física

- Fondo vertical gameplay actualizado, parrilla cenital grande a la izquierda; prop `MesitaAsador` a la derecha con `TablaAsador` separada como objeto hijo e interactivo. Los archivos y metadatos de import se conservan en `Resources/Art/`.
- Quitados los botones de acción de carne `DAR VUELTA`, `BANDEJA` y `SERVIR`. El segundo tap al alimento activo voltea; arrastrar al rect de tabla emplata; doble tap sobre tabla llena sirve. Pinza acompaña el arco grill→tabla; seis slots 3×2 con variación determinista.
- HUD sin barra de cocción, estado contextual en texto; tutorial bajo la zona de parrilla; debug se mantiene sólo para builds de desarrollo y se ocultó manualmente para la captura de presentación.
- Captura de Unity Editor Game View a 1080×1920: `/tmp/Asadito-gameplay-layout-final.png`. No representa Android ni hardware físico.
- Tests del código del rediseño: EditMode 19/19 PASS; PlayMode 10/10 PASS (486.94 s) en Unity 6000.6.3f1; validator y consola chequeados. El detalle sigue abajo/actualiza con la última ejecución de este sprint.
- No se reconstruyó APK para este cambio: el APK previo `/tmp/Asadito-always-hot-1.2.0-arm64.apk` no incluye el layout actualizado; verificar compilación/instalación Android tras el commit si el release requiere esta UI.

- Parrilla always-hot uniforme a 210 °C; se eliminaron carbón/encendido, combustible, heat-grid/brasas táctiles, glow de gameplay, SFX de ignition y tutorial asociado. Cocción de alimentos, caras/flip, Maillard, humedad, char y perfiles siguen conectados.
- Tocar comida directamente selecciona; hitbox invisible ampliada, pinza ilustrada abierta/cerrada, tap repetido voltea; arrastrar a tabla física y doble tap para servirla. No quedan botones de acción DAR VUELTA/BANDEJA/SERVIR ni botones individuales por corte.
- Catálogo data-driven de 18 alimentos, 108 estados visuales y progresión de 12 niveles; pause/settings mínimos, scoring, save y debug oculto fuera de Editor/development.
- Specs, arquitectura y manifests documentan la regla y los assets conectados.

## Verificación previa al ajuste de escala (rediseño 10bis)

- EditMode Unity 6000.6.3f1: **19/19 PASS** (`/tmp/asadito-always-hot-editmode.xml`).
- PlayMode Unity 6000.6.3f1: **10/10 PASS** (486.94 s), incluye cocinar/servir todos los niveles L1–L12; XML en `/Users/celestino/Library/Application Support/Cuervation/Asadito/TestResults.xml`.
- Validator `python3 Tools/validate_food_content.py`: **PASS**, 18 perfiles, 108 sprites, 12 cartas, progresión y recursos runtime.
- Console Unity al final de la suite: 0 errors/warnings.

## Pendiente antes de declarar release/QA físico

- Build actual del refactor: Android ARM64 IL2CPP, API36, package/version/code verificados; firma v2 debug. APK `/tmp/Asadito-always-hot-1.2.0-arm64.apk` (53 MiB); `aapt`, `unzip` y `apksigner` pasaron.
- Instalación ADB en `Asadito_Pixel_7a_API_36` pasó, pero el AVD es x86_64, no ARM64. Unity no pudo inicializar Graphics API (SwiftShader anuncia GLES 3.1 y el player intenta ES 3.2/3.1); no alcanzó menú ni gameplay y no hay captura Android de gameplay. Evidencia del error: `/tmp/asadito-qa/02-after-hardware-warning.png`.
- El ANR histórico de swipe pertenece al APK previo al refactor; no se declara ni resuelto ni reproducido en la nueva build, porque ésta no llegó a renderizar en el AVD.
- Faltan pruebas con dedos/hardware físico (drag/multitouch/notch/navbar), audio/haptics real, rendimiento sostenido y sign-off humano de arte.
- GitHub Actions del push `df081bc`: workflow `36664620467` concluyó `success`; validator/whitespace PASS, job Unity EditMode/PlayMode `skipped` (no se atribuye como ejecución remota).

## QA local completado en la actualización 10bis anterior

- EditMode 19/19 PASS y PlayMode 10/10 PASS (incluye L1–L12); `Tools/validate_food_content.py` PASS; consola Unity limpia al final de tests.
- `git diff --check`: **PASS**; commit principal `df081bc` se empujó a `origin/main`. La ejecución CI resultante está documentada en esta actualización.

API36 es el target configurado para Android 16 y cubre el requisito actual de envíos a Google Play desde el 31-08-2026 ([documentación oficial](https://support.google.com/googleplay/android-developer/answer/11926878?hl=es)). Build de validación debug; no hay firma productiva/publicación.

Ver [auditoría](mvp-audit.md), [estado técnico](current-state.md) y [build Android](../android-release.md).
