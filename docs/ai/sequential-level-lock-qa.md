# Bloqueo secuencial — QA (2026-10-02)

## Regla y cambio
L2 exige aprobarL1 (al menos1estrella); cada siguiente exige aprobar todos los anteriores, además del techo guardado y límite jugableL1–6. `MvpSaveData.IsLevelUnlocked` comprueba el prerrequisito; `AsaditoGame.IsLevelAvailable` centraliza selector, selección directa y SIGUIENTE. Un MaxUnlockedLevel legado alto sin estrellas previas ya no saltea niveles. Consulta sin mutaciones: conserva datos de unlock/estrellas/puntajes, monedas/inventario/settings y runs. Sin cambio de versión de save/scoring/térmica/economía.

## Evidencia
- Rojo antes del fix: PlayMode `36dc39c182ee47e6af77b45518971140`; MaxUnlockedLevel6/estrellaL1=0 dejóNIVEL2 enabled; expectedFalse,actualTrue.
- Compile/import sin errores; scoped C# diff-check PASS.
- EditMode `ServingAndProgressTests` **11/11PASS**,0.842s,job `7413a916acd64878b8da18b88450d98f`. Incluye2regresiones nuevas: legacy/huecos/preservación y unlock/reload/retry fallido.
- PlayMode dirigido4casos: job `97c0c3a83acf49f896e8b5f43c15777d`,3/4PASS,63.361s. Pasaron cocina/servicio/desbloqueo real deL1; compra/preparación/cocción/guardadoL5; arte/UI del catálogo. El caso nuevo falló en el helper FindButton, que exige interactable=true y no sirve para buscar una tarjeta correctamente bloqueada.
- Se corrigió **solo el lookup del nuevo test**, usando Button directo al inspeccionar disabled y manteniendo tap/raycast nativo al habilitarNIVEL2. Rerun focal `fe6373ed8c174f43870ba81959c6aab5`: **1/1PASS**,4.650s. Verifica candado/Disabled, rechazo de SelectLevel y PlayNextLevel, fracaso0estrellas tras reload, aprobación1estrella→desbloqueo, y tap nativo→introL2; L3 permanece bloqueado.
- Cobertura final **4casos PlayMode únicos verificados (3+1)**; no se afirma un único barrido4/4. Sin repetir3pasados pues solo cambió el lookup del caso nuevo.
- Un filtro EditMode con namespace errado ejecutó0casos: **no cuenta comoPASS**. Primer intento PlayMode4casos abortó con NullReference del Unity Test Framework/ExitPlayModeTask; **no cuenta comoPASS**. Se limpió job/InitTestScene propio y se restauró la copia de prefs de esta tarea antes del rerun limpio.

## Preservación / entrega
PlayerPrefs del Editor se comparan con `output/debug/sequential-level-lock/playerprefs-before.json`; no reset de partida ni sobrescritura de actividad nueva tras devolver control. Fuente local verificada; **sin nueva APK/install/commit/push**. El teléfono conserva1.5.0/code7 anterior a este bloqueo reforzado; requiere build/update posterior si se solicita.

## Archivos
- `Assets/Asado/Scripts/Runtime/MvpSaveData.cs`: disponibilidad por cadena de estrellas.
- `Assets/Asado/Scripts/AsaditoGame.cs`: guardia central.
- `Assets/Tests/EditMode/ServingAndProgressTests.cs`:2regresiones de progreso.
- `Assets/Tests/PlayMode/FirstPlayableFlowTests.cs`: regresión nativa y2fixtures con completados reales, no flags aislados.
- Evidencia local: `/Users/celestino/Documents/ChatGPT/Asadito/output/debug/sequential-level-lock/`:diffs,redJSON,XML11/11,3/4,1/1 y copia actual de prefs/result.
