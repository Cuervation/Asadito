# Configuración y build Android

`Assets/Asado/Scripts/Editor/AndroidReleaseBuild.cs` contiene un ajuste y build repetibles. El método de configuración establece:

| Campo | Valor validado |
|---|---|
| Company | `Cuervation` |
| Product | `Asadito` |
| Application ID | `com.cuervation.asadito` |
| Version / versionCode | `1.3.0` / `4` |
| Orientación | Portrait; manifiesto empaquetado anuncia `screenOrientation=portrait` |
| SDK | min 26, target/compile 36 (Android 16) |
| Arquitectura / backend | ARM64 / IL2CPP |
| Icono | Android genera XML `adaptive-icon` usando `AsaditoAppIcon` (adaptación cuadrada de la parrilla cenital y la carne de `PortadaAsadito`); sin firma productiva |

Unity API: ejecutar desde CLI `-executeMethod Asadito.Editor.AndroidReleaseBuild.BuildAndroidValidation`; predeterminado `/tmp/Asadito-expanded-android.apk` (override vía `ASADITO_APK_PATH`). `Builds/` no se incluye por ser artefacto.

Build de validación del baseline anterior (2026-09-29): `/tmp/Asadito-mvp-1.2.0-arm64-final.apk` mostraba carbón y **no valida este refactor**. Sus capturas de gameplay/swipe y el ANR de input son evidencia histórica; no deben atribuirse ni extrapolarse a la build siguiente.

Build actual del refactor (2026-09-30): Unity 6000.6.3f1 `Succeeded` en el segundo intento; el primero se canceló durante IL2CPP postprocess tras ~239 s y el incremental terminó en 61.7 s. APK `/tmp/Asadito-always-hot-1.2.0-arm64.apk` (53 MiB); `aapt` verificó `com.cuervation.asadito`, versión 1.2.0/code 3, min API 26 y target/compile API 36; `unzip` confirmó solo `arm64-v8a`; `apksigner` verifica el scheme v2. Instalación con ADB: `Success` en el AVD `Asadito_Pixel_7a_API_36` (1080×2400, Android 16/API36).

Límite de smoke test: pese a instalar, el AVD es una imagen **x86_64**, no un teléfono Pixel ARM64. Al abrir el APK ARM64 mostró el aviso de incompatibilidad del emulador y Unity no pudo inicializar `Unity Engine Graphics API`; con SwiftShader la imagen solo anuncia OpenGL ES 3.1, mientras Unity intenta crear contextos ES 3.2/3.1. Por ello **no** se verificaron menú, gameplay, toque, drag, flip o servicio en esta build, y no existe una captura de gameplay Android nueva. La pantalla del bloqueo gráfico quedó en `/tmp/asadito-qa/02-after-hardware-warning.png` solo como evidencia diagnóstica. La build está compilada, firmada para validación e instalada, pero el smoke de runtime requiere teléfono ARM64 compatible; no se publicó ni se usaron claves productivas.

Desde el 31-08-2026 Google Play requiere que nuevas aplicaciones y actualizaciones apunten a API36 o superior ([requisito oficial de Google Play](https://support.google.com/googleplay/android-developer/answer/11926878?hl=es)). Esta configuración cubre el umbral target, pero **no** implica certificación de política, release firmado, QA de teléfono físico ni publicación. El paquete actual usa firma debug. Claves productivas deben permanecer fuera del repo.

Build de icono parrilla (2026-09-30): Unity 6000.6.3f1 `Succeeded`; APK ARM64 `/tmp/Asadito-app-icon-grill-20260930.apk` (60 MiB). `aapt` confirmó `com.cuervation.asadito`, 1.2.0/code3, min26 y target/compile36, y que el launcher usa XML `adaptive-icon` con `ic_launcher_background`/`ic_launcher_foreground`; `apksigner` verificó firma v2 de validación. El teléfono no estaba conectado en esta sesión, así que no se instaló ni se validó el aspecto del launcher en dispositivo físico. La compilación se hizo en una copia aislada porque el Editor Unity abierto bloquea una segunda instancia del mismo proyecto.

Build e instalación de `d6a5004` (2026-09-30): validator PASS; Unity EditMode **25/25 PASS** y PlayMode **11/11 PASS**. La compilación ARM64/IL2CPP con Unity 6000.6.3f1 terminó `Succeeded`: `/tmp/Asadito-d6a5004-1.2.0-arm64.apk` (60 MiB). `aapt` confirmó `com.cuervation.asadito`, 1.2.0/code3, min API26/target+compile API36; `unzip` confirma `arm64-v8a`; `apksigner verify --verbose` confirma firma v2. `adb install -r` devolvió `Success` en Motorola Edge 60 Fusion `ZY22MBNWRB` (Android 16/API36), sin desinstalar ni limpiar los datos; se inició `UnityPlayerGameActivity`. Screenshot `/tmp/asadito-d6a5004-level1-intro.png`: inicio → selector con niveles 2+ Disabled/candado → popup compacto de Nivel 1. No se ejecutó la partida ni se verificaron drag/cocción/toques en esta pasada; el AVD conectado es x86_64 e incompatible con el player ARM64 para smoke gráfico.

Build de los cambios pendientes de navegación/UI (2026-09-30): `python3 Tools/validate_food_content.py` PASS; Unity EditMode **25/25 PASS** y PlayMode **12/12 PASS** (incluye volver desde gameplay, eliminación de PAUSA, popup y separación título–tarjetas). Unity 6000.6.3f1 Android ARM64/IL2CPP terminó `Succeeded`: `/tmp/Asadito-all-pending-1.2.0-arm64-20260930.apk` (60 MiB). `aapt` verificó `com.cuervation.asadito` 1.2.0/code3, min API26/target+compile API36; `unzip` confirma solamente `arm64-v8a`; `apksigner verify --verbose` valida firma v2. APK local de validación; no instalada en el teléfono en esta solicitud.

Instalación de la APK `20979f4` (2026-09-30): `adb install -r --no-streaming` devolvió `Success` en Motorola Edge 60 Fusion `ZY22MBNWRB` (Android 16/API36), conservando datos. `UnityPlayerGameActivity` quedó en primer plano con el menú principal visible (PID 21581 al verificar). Captura `/tmp/asadito-motorola-20979f4.png` (1220×2712). El transporte ADB del teléfono desapareció al intentar continuar la navegación, después de confirmar instalación y arranque; no se verificó el selector ni gameplay en esta pasada.

## Gestión desde L1 (2026-10-01, actual)
Unity6000.6.3f1, ARM64/IL2CPP Succeeded en una única compilación final. APK local65MiB `build/Asadito-1.3.0-management-level1-20261001.apk`, version1.3.0/code4,min26,target36; metadata/ABI/firma v2 debug verificadas. SHA256 `6876a9ab20d112c3b257654c540c588248afc5efc369ac04d1021e891604cc61`. Dominio53/53PASS, focalgestión3/3PASS y recorridoL1–6 1/1PASS; detalle/evidencia en [QA](ai/management-level1-qa.md). ADB no detecta dispositivos al cierre, por lo que esta build no se instaló ni se probó táctilmente en teléfono. Mantener datos con `adb install -r` cuando vuelva a estar conectado; no desinstalar ni limpiar PlayerPrefs. Sin publicación/firma productiva.
