# Presentación de nivel ilustrada — QA 2026-10-02

## Alcance
El usuario rechaza el popup NIVEL1 · EL DEBUT y pide alinearlo con la estética vigente. Se cambia sólo la presentación y su navegación de salida; no scoring, pedido, cantidad, precio, economía, progreso, inventario, save ni preparación.

- AsaditoGame: tarjeta720×780 con Frame_ProductCard sliced (madera/papel/hojas), fondo oscurecido cálido, badge NIVEL y título separado Lilita sobre verde, GuestPortraitAtlas para comensales reales y alimentos originales de FoodSpriteLibrary en su estado ideal. CTA dorado conserva StartLevel/PLANIFICAR ASADO y los sprites arcade existentes.
- Flecha coral64×64 vuelve al selector sin iniciar ni abandonar un run.
- ManagementResultLayout: parámetro animate=true por defecto; intro usa false porque se construye inactiva y su padre gestiona el fade. Los resultados mantienen la entrada original. Este archivo ya era untracked antes de este trabajo; no reemplazar trabajo previo por ausencia de diff Git.
- RectMask2D local ajustado a la altura dibujada preserveAspect oculta cuatro unidades del borde inferior de cada celda de atlas. Corrige fragmentos de fila vecina, sin cambiar PNG, crops compartidos, GUID/import ni tamaño de carne en parrilla/heladera.

## Validación proporcional
Unity6000.6.3f1 en /Users/celestino/Asadito, SampleScene, target Android.
1. Implementación inicial: compile/console0errores, PlayMode **4/4 PASS**,18.6782672s, job09c1368f3c0c439e82ce52ffd4d1f482:
   - Intro_IllustratedCardFitsOrdersAndKeepsNavigationAndSave
   - VisualAssets_CatalogFoodsExposeSixThermalStatesPerFace
   - Management_IllustratedResultsDetailsAndActionsDoNotMutateSave
   - Progression_LevelTwoRequiresCompletedLevelOneEvenWithLegacyUnlock
2. Tras encuadre local final: **2/2 PASS**,9.5904633s, job32731e0203b64842827acf8bafa27f01, intro+VisualAssets anteriores; Console0errores. No se repitieron resultados/progresión sin modificación de sus contratos.
3. Tests usan fixture con save QA legítimamente desbloqueado, taps nativos de ENTRAR/NIVEL1,3,6/VOLVER/CTA. Verifican retratos, assets, jerarquía, textos completos, bounds de tarjeta/targets, original sprites y no CanvasGroup invisible bloqueando. CTA final abre PRÓXIMO ASADO, no compra ni prepara.
4. Renders Canvas1080×1920 y720×1600 para L1/L3/L6. Inspección visual final L1grande yL6estrecho: título, caras, pedido, cantidades yCTA completos; fragmentos eliminados sin recortar comida principal. Es evidencia Editor, no screenshot del teléfono ni aprobación artística humana.
5. Scoped git diff --check de runtime/tests PASS. PlayerPrefs asadito.mvp.save idéntico al backup fresco de esta tarea tras teardown; no se restaura backup de sesiones anteriores.

Durante el cierre hubo ping/reconexión del bridge MCP con Editor sin foco; las llamadas abortadas no cuentan como tests. Traer el Editor existente al frente permitió la corrida final2/2; no se reinició ni creó otra instancia.

## Evidencia
Directorio absoluto /Users/celestino/Documents/ChatGPT/Asadito/output/debug/intro-popup-redesign:
- playmode-results.xml — corrida inicial4/4.
- playmode-polish-results.xml — cierre2/2.
- before-1080.png, intro-level1-1080.png, intro-level1-720.png, intro-level3-1080.png, intro-level6-720.png — antes y capturas finales sin retoque.
- playerprefs-before.json — backup fresco privado de Editor; no importar al teléfono.
- diff-before.patch, diff-after.patch, layout-scoped.patch — auditoría incluyendo layout previo reconstruido desde lectura inicial (no snapshot binario).

## Estado de entrega
Unity principal queda en Play, Simulator enfocado y nuevo intro L1 EL DEBUT visible, sin iniciar el asado. Captura real final intro-live-final.png inspeccionada; PlayerPrefs sigue idéntico al backup fresco de esta tarea.

## Límites
No full suite/EditMode/validator/build/install solicitados para este cambio local. No commit/push. APK instalada1.5.0/code7 sigue anterior al nuevo popup y al bloqueo secuencial local. QA Android táctil/performance y aprobación estética humana pendientes.
