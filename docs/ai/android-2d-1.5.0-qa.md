# Android 2D 1.5.0 — QA (2026-10-02)

## Alcance e identidad
APK local de validación de la fuente vigente 100 %2D, con catálogo completo/precios/stock libre, preparación corregida, resultados ilustrados y mini barras por comida. Conserva los cambios locales previos; no commit/push ni publicación.

- Unity6000.6.3f1; Android IL2CPP/ARM64, Portrait.
- `com.cuervation.asadito`,1.5.0/code7, min26/target+compile36.
- APK `/Users/celestino/Asadito/build/Asadito-1.5.0-2d-20261002.apk`.
- 73,590,830bytes (~70.18MiB), SHA256 `166fc8db7ac8183345cd4bd83a1e51d6564827bac9eab2bc89d48ba56e75aa73`.
- Firma debug v2 verificada; cert SHA256 `d2fc25710ab22385b7c6979159c0ef647e91672aa9f6a8672916e52df2e28b5e`, igual a1.4.0. Sin clave productiva.

## Pruebas y build
- Validator PASS:18perfiles/108frames/12niveles y layouts/art/fonts completos.
- EditMode completo72/72PASS (3.103s), job `da02651d4bce4e8883cd47152262406b`.
- PlayMode completo inicial28/29PASS (360.017s), job `c82727ed064a48e99f0de0fa42dca01b`. El helper forzado del test de progresión no encontró el siguiente IR A LA PARRILLA.
- Se fortaleció **solo** `Mvp_TutorialProgressIsSavedAndUnlocksNextAsado`: tap nativo/raycast de SIGUIENTE, interactabilidad de CanvasGroup, nivel siguiente e intro visible. Sin cambio runtime para este fallo.
- Primer intento focal abortado por NullReference del runner/ExitPlayModeTask antes de ejecutar tests; **no** cuenta como PASS. Se limpió el job atascado y únicamente su InitTestScene; se restauró la copia de prefs de esta tarea.
- Reintento limpio1/1PASS (157.261s), job `85cc72e9f2894113b493ad06fa0704e8`, recorreL1–6 con taps nativos. Cobertura final:28PlayMode ya pasados sin cambios cubiertos +1focal pasado; **no** se afirma un único barrido29/29.
- Build job `build-1d2282bb05`: Succeeded434.221s,0errors/1warning. Warning: Diagnostics Data requiere símbolos para stacktraces. El reporte817.66MB incluye salidas auxiliares, **no** el tamaño de la APK.
- Aapt/manifiesto/zip/apksigner verificaron identidad, SDK, portrait, soloarm64-v8a y firma.
- Fuente de build:HEAD `2093bd06c326ed73c254ecaa776a7a98320336b2` + cambios locales. Manifiesto505archivos/hash `90ebece324e832fae91e4e3cf47318a14a67b868a4ce3b588cddd90e5f977bb3`.
- Auditoría post-build: fuente/arte/settings sin cambios respecto al manifiesto; únicamente desaparecen JSON+meta PerformanceTest y carpetaResources vacía por cleanup del paquete de tests. Delta propio: identidad1.5/code7, test nativo y configuración UnityConnect m_Enabled0→1 (incluida en build). No se revierten cambios ajenos.
- Diff-check C# del release/test PASS. No se atribuye un diff-check global limpio: metadata de importadores preexistente y m_SubKind generado por Unity tienen whitespace; no se reescriben masivamente.

## Motorola / conservación de datos
- Motorola Edge60Fusion `ZY22MBNWRB`, Android16/API36.
- `adb install -r --no-streaming`: **Success**, actualiza1.4.0/code6→1.5.0/code7.
- AppId10362 y firstInstallTime2026-10-01 11:39:10 permanecen iguales; sin desinstalar/pm clear/copiar prefs del Editor al teléfono.
- `am start -S -W`: Status ok, UnityPlayerGameActivity, PID23218. Capturas inicialmente negras: powerDozing, screenOFF y keyguardshowing; no se afirma menú visible hasta desbloquear.
- Log de inicio: AssetPackManagerClassNotFoundException ya conocido; no demuestra crash. No compras/preparación/partida para gastar recursos del usuario.
- PlayerPrefs del Editor idénticos a copia de esta tarea al terminar tests/build; se devuelve presentación previa sin completar/recompensar otro asado.

## Límites
El USB volvió a desaparecer mientras se esperaba el desbloqueo. Pendiente cierre visual del menú al desbloquear; touch/notch/gestos/performance/duración/audio y aprobación estética requieren QA humana. No gameplay completo en teléfono ni publicación.

## Evidencia local
`/Users/celestino/Documents/ChatGPT/Asadito/output/debug/android-1.5.0/`: XML de suites/reintento, manifiesto/hash de fuentes, artifact.json, diff-before/after, baseline de prefs/result y phone-before/after-package/install/launch/foreground/startup-log. La copia de prefs no debe usarse para sobrescribir actividad nueva del usuario.
