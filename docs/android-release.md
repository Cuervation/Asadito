# Configuración y build Android

`Assets/Asado/Scripts/Editor/AndroidReleaseBuild.cs` contiene un ajuste y build repetibles. El método de configuración establece:

| Campo | Valor validado |
|---|---|
| Company | `Cuervation` |
| Product | `Asadito` |
| Application ID | `com.cuervation.asadito` |
| Version / versionCode | `1.2.0` / `3` |
| Orientación | Portrait; manifiesto empaquetado anuncia `screenOrientation=portrait` |
| SDK | min 26, target/compile 36 (Android 16) |
| Arquitectura / backend | ARM64 / IL2CPP |
| Icono | Android genera XML `adaptive-icon` con imagen de `AsaditoAppIcon`; sin firma productiva |

Unity API: ejecutar desde CLI `-executeMethod Asadito.Editor.AndroidReleaseBuild.BuildAndroidValidation`; predeterminado `/tmp/Asadito-expanded-android.apk` (override vía `ASADITO_APK_PATH`). `Builds/` no se incluye por ser artefacto.

Build de validación actualizado 2026-09-29: Unity 6000.6.3f1 `Succeeded`; `/tmp/Asadito-mvp-1.2.0-arm64-final.apk`, 52,615,236 bytes (~50 MiB). `aapt` confirma applicationId, version/code, min/target/compile SDK y ABI ARM64; `apksigner` verifica scheme v2; se instaló y abrió en emulador Pixel 7a Android 16/API36 (1080×2400). Portada → selector → intro L1 → fuego → tap directo sobre chorizo fueron vistos; captura de menú del APK final `/tmp/Asadito-mvp-final-menu.png`; captura de pieza seleccionada `/tmp/Asadito-mvp-gameplay-selected.png`. Ajuste de `Assets/Settings/Renderer2D.asset` quitó warnings repetidos de Bloom no disponible. Límite importante: una prueba ADB de swipe en el APK final terminó en ANR de input (~8 s) y el drop no se confirmó. Trace de la VM x86-64/QEMU muestra espera GL traducida (`glUnmapBufferAEMU_enc`); origen no determinado, por eso drag sostenido/emplatado/performance no son PASS. Próximo paso de release: Development Build + Unity Profiler y repetición en teléfono ARM64 real. Firma debug; no se publicó.

Desde el 31-08-2026 Google Play requiere que nuevas aplicaciones y actualizaciones apunten a API36 o superior ([requisito oficial de Google Play](https://support.google.com/googleplay/android-developer/answer/11926878?hl=es)). Esta configuración cubre el umbral target, pero **no** implica certificación de política, release firmado, QA de teléfono físico ni publicación. Claves productivas deben permanecer fuera del repo.
