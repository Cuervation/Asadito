# Configuración y build Android

`Assets/Asado/Scripts/Editor/AndroidReleaseBuild.cs` contiene un ajuste y build repetibles. El método de configuración establece:

| Campo | Valor validado |
|---|---|
| Company | `Cuervation` |
| Product | `Asadito` |
| Application ID | `com.cuervation.asadito` |
| Version / versionCode | `1.1.0` / `2` |
| Orientación | Portrait; manifiesto empaquetado anuncia `screenOrientation=portrait` |
| SDK | min 26, target/compile 36 (Android 16) |
| Arquitectura / backend | ARM64 / IL2CPP |
| Icono | Android genera XML `adaptive-icon` con imagen de `AsaditoAppIcon`; sin firma productiva |

Unity API: ejecutar desde CLI `-executeMethod Asadito.Editor.AndroidReleaseBuild.BuildAndroidValidation`; predeterminado `/tmp/Asadito-expanded-android.apk` (override vía `ASADITO_APK_PATH`). `Builds/` no se incluye por ser artefacto.

Build verificado 2026-09-29: Unity 6000.6.3f1 `Succeeded`; APK ~52 MB; `aapt` confirma package/version/minSdk/targetSdk/ARM64; `apksigner` valida firma de depuración; APK se instaló y ejecutó en emulador Pixel 7a Android 16/API36. Captura portada: `/tmp/Asadito-android-final-normalized.png`. No se publicó.

Desde el 31-08-2026 Google Play requiere que nuevas aplicaciones y actualizaciones apunten a API36 o superior ([requisito oficial de Google Play](https://support.google.com/googleplay/android-developer/answer/11926878?hl=es)). Esta configuración cubre el umbral target, pero **no** implica certificación de política, release firmado, QA de teléfono físico ni publicación. Claves productivas deben permanecer fuera del repo.
