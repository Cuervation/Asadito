# QA Vertical 1 — 2026-10-01

Unity6000.6.3f1, checkout aislado `/tmp/Asadito-concurrent-cooking-qa`, sources sincronizados desde repo. Editor principal MCP bloqueado por estado previo `tests_running` y ping sin responder; no se cerró ni borró su Library.

## Evidencia ejecutada
- Compilación inicial real encontró CS1061 en importer preparado antes; corregido con TextureImporterSettings.
- Focal económico inicial12/13: corrigió avance de ciclo de frescura; nunca activar reloj real.
- EditMode broad inicial37/38: assertion de roster antiguo esperaba seis invitados en L5 ahora de cuatro; prueba trasladada a L12, donde sigue comprobando seis perfiles distintos.
- Cierre EditMode **39/39 PASS**,3.23s: `/tmp/asadito-management-editmode-closure.xml` y `.log`.
- Cierre PlayMode **13/13 PASS**,191.26s: `/tmp/asadito-management-playmode-final.xml` y `.log`. Incluye core, gestos/cocción simultánea, navegación, selector/progreso/save, arte y nuevo loop de gestión.
- Focal post-ajuste de legibilidad **1/1 PASS**,29.62s: `/tmp/asadito-management-legibility.xml` y `.log`. Confirma compra4unidades/costo560/saldo440, extracción a run, cocción real, servicio, income/saldo persistido, retry vuelve a planificación; además preferredHeight dentro de contenedores.
- `Tools/validate_food_content.py` **PASS**; `/tmp/asadito-management-content.log`.
- `git diff --check` PASS.

## Visual real renderizado
Capturas con cámara/RenderTexture1080×1920 durante PlayMode (ScreenCapture simple en batch no exportó imágenes):
- `/tmp/asadito-management-planning.png`
- `/tmp/asadito-management-shop.png`
- `/tmp/asadito-management-fridge.png`
- `/tmp/asadito-management-result.png`
Se revisaron. Primeras pasadas detectaron contorno oscuro excesivo, food canvasoverride encima del resultado y ASADOR/SALDO truncados al cambiar fuente. Corregidos y flujo/legibilidad repetidos. No se usó mockup HTML como evidencia Unity.

## Reinicio y validación final
La revisión final del save detectó que transición genérica al selector abandonaba unidades pagadas tras reiniciar la app. Abandono movido a salida explícita de cocina/pausa o inicio de otro pedido. Reanudar el mismo nivel conserva las unidades/costo y comienza cocción fría, sin segunda compra.
Focal de compra/preparación → reload real de escena/cache/save → reanudación → cocción/resultado **1/1 PASS**,32.70s: `/tmp/asadito-management-resume.xml`. La suite PlayMode general se repitió tras esta corrección transversal: **13/13 PASS**,191.26s, xml final arriba. Business models/datos sin cambios adicionales: EditMode39/39 sigue cubriendo sus contratos.

## Android/límites
Build inicial Succeeded; se hizo una **recompilación incremental necesaria**, no builds por tweaks, después de la corrección de reanudación. Build final ARM64/IL2CPP **Succeeded**, `/tmp/asadito-management-android-final.log`.
APK final conservada en `/Users/celestino/Asadito/build/Asadito-management-vertical1-20261001.apk` (65MiB; no se versiona binario en Git).
`aapt`: com.cuervation.asadito,1.2.0/code3,min26/target36,arm64-v8a. `apksigner verify`: esquema v2 PASS, certificado Android Debug (no firma de distribución).
SHA256: `0041a67fd6c4c1d1de4720fc04d6d21a9f5334eadcd7fbcc83341d2882fc52c4`.
 ADB solo detecta emulator-5554 x86_64; no hay teléfono ARM64 conectado. No instalar buildARM64 en AVD incompatible ni afirmar QA táctil físico. La UI queda provisional hasta playtesting humano/performance/safe area real. Sin publicación ni firma de distribución.
