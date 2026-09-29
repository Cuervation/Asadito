# Auditoría de expansión — 2026-09-29

| Gate | Estado | Evidencia / límites |
|---|---|---|
| Fuente de verdad Git/Unity | Revisado | Checkout `/Users/celestino/Asadito`, `main`, remote `origin`; remoto GitHub autenticado. El directorio alias Codex `Documents/ChatGPT/Asadito` no es checkout. |
| Catálogo / perfiles | PASS | 18 IDs requeridos, todas las firmas térmicas distintas; JSON validados también en runtime. Cortes/profile/motor separados; solo provoleta tiene regla específica declarada por data flag. |
| Art de comida / seis estados | PASS automatizable | 18 atlas 1024×1536 RGBA, 6 frames cada uno =108 recortes runtime no-null; EditMode comprueba todos y los estados clave. PROVISIONAL visualmente. |
| Progresión/niveles | PASS | 12 niveles, 12 ilustraciones PNG; save v2/migración v1; PlayMode cooking→serving→results/unlock→selector final L1–L12. |
| Cara/flip | PASS automatizable | Estado y sprites separados por cara; PlayMode verifica ideal↔raw↔ideal tras flip. |
| Serving/score/guests | PASS automatizable | Allocator reparto justo, prioridad cobertura/gusto/punto; scoring ponderado 40/30/20/10 con inputs 0–100; seis guests distintos; favorito=100. |
| Safe area / layers | PASS en código, no QA físico | `Screen.safeArea` actualiza anchors; gameplay group separado de overlays/selector. Emulador portrait; falta notch/gestos/touch humano. |
| Motion/VFX/audio | Integrado, PROVISIONAL | Lista real runtime en `docs/art/animation-manifest.md`. Procedural; sin clips por tipo de comida, partículas production o mezcla audio aprobada. |
| EditMode | PASS | **18 passed / 0 failed**; Unity 6000.6.3f1, XML `/tmp/Asadito-EditMode-final.xml`. |
| PlayMode | PASS | **6 passed / 0 failed**; los 12 niveles completos, retry/UI/progresión, atlas/6 estados por cara; XML `/tmp/Asadito-PlayMode-passing.xml`. |
| Content/diff check | PASS | Validator imprime 18 unique profiles,108 cooking sprites,12 level illustrations; `git diff --check` PASS. |
| Android | PASS build/install | Unity 6000.6.3f1, APK ~52MB; `com.cuervation.asadito` 1.1.0 code2, min26, target36, portrait, ARM64 IL2CPP; AAPT metadata+adaptive icon XML, debug signer; instalado/lanzado en Pixel 7a emulator API36. No release signing/Play publish. |
| GitHub CI | Preparada | Content/diff job cada push/PR. Unity GameCI job requiere variable `UNITY_CI_LICENSE_READY=true` y tres secretos de licencia; no verificaría suites en Actions si la licencia falta. |
| Touch/física/arte final | Parcial / no certificado | Captura portada del emulador confirma render startup; el screenshot no es prueba de tacto, notch ni rendimiento de teléfono real. |
| Git push | Pendiente de cierre | GH auth está disponible; hacer commit lógico(s), luego push normal sin force y comprobar `origin/main`. |

## Bloqueos reales no técnicos

Prueba física solo requiere conectar/autorizar un teléfono si se desea QA humano. Unity CI remoto requiere credencial/licencia del owner en secrets. Firma productiva/publicación están fuera de autorización. Ninguna de esas cosas bloqueó tests/build locales.
