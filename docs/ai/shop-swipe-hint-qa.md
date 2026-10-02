# QA — pista discreta de swipe (2026-10-02)

## Cambio visual
- Banda de paginación existente (.286) ahora muestra «Deslizá a los lados para ver más cortes · N / 5» con bodyfont25 y fondo verde translúcido58px.
- Se conservan exactamente viewport de cortes y ancla de carrito (.175), espacio intermedio, imágenes/precios/stock y navegación por dedo. Sin flechas, botones, animación ni nuevo arte.
- Label y fondo `raycastTarget=false`: pista decorativa, no objetivo táctil. Compra/pago/stock/guardado sin cambios.

## Evidencia
Unity6000.6.3f1 real /Users/celestino/Asadito; refresh/domainreload, Console0errores y scoped diffcheckPASS. Inicial pingtimeout se recuperó antes de pruebas.

PlayMode dirigido **1/1 PASS**,3.5924491s, job292ab024fe78430298b442c6c841240c: test existente `Management_2DShopSwipeOwnsDirectionAndNeverAddsOrSpends` extendido con copy/panel58px/noButton/nonraycast/geometría. Conserva swipes nativos ida/vuelta, límites, cancelación/multitouch/+/- sin compras accidentales y snapshots State/PlayerPrefs iguales. Texto completo cabe1080×1920 y720×1600; capturas reales inspeccionadas y copiadas en evidencia.

BaselineQA inicial de esta tarea preservado semánticamente al cierre (lectura read-only plist). No equivale al respaldo humano166/dos piezas del Editor, todavía protegido y pendiente de autorización para restauración por tarea anterior; no se rodeó restricción. Teléfono no tocado.

## Límites
No suite completa, validator contenido (sin cambios de datos), APK/build/install/commit/push ni QA física. Motorola1.5.0/code7 no incluye cambios locales posteriores; Editor queda detenido, no arrancado con saveQA como partida humana.

## Artefactos
`/Users/celestino/Documents/ChatGPT/Asadito/output/debug/shop-swipe-hint/`: baselines/staging, `playmode-results.xml`, `shop-swipe-hint-1080.png`, `shop-swipe-hint-720.png`, `playerprefs-start-qa.json`.
