# Gestión desde el debut — auditoría y QA (2026-10-01)

## Auditoría / decisiones
Baseline `b7d6fe9`, árbol limpio en `/Users/celestino/Asadito`. Nueva decisión de producto del usuario reemplaza el gate anterior L5. Revisados router/skills, estado, config/pedidos, gestión, save, serving/score, vistas y pruebas. Se reutilizan Wallet/ManagementService/ManagementScreen/arte existente y toda la cocción/touch/packing; no hay nuevas dependencias ni agentes.

Problemas encontrados:
- Gates `ManagementFromLevel=5` y `CoinsFromLevel=4` daban carne gratuita L1–4.
- Cocción cruda casi100 en CookingQuality porque solo medía daño; gestión perfecta y umbral bajo podían aprobar un mal asado.
- Asignación por déficit absoluto daba la tira de Ana a Tito aunque se comprara/cocinara según sus preferencias. Ahora usa saciedad normalizada, gusto/punto ponderados, conservando primera porción para todos.
- Bonus L4 viejo necesitaba retirarse sin reentregar ni quitar saldo previo.
- Tutorial nuevo debía persistir independientemente del tutorial de cocina legacy y permitir recuperación con saldo cero.

Implementación: compra confirmada desde L1, saldo650 solo para nuevas partidas, precio100/180; pedidos authored100g/140g coherentes con unidades compradas; recompensa80+150porinvitado modulada por mínimo de general/asador/cocción, recovery50% y sin cobro si fracasa. Pesos60/25/15 más mínimos Asador55/cada pieza50/saciedad65 y caps2/3por70/85. Desperdicio cocinado registra costo sin doble descuento. Savev5 preserva v1–4, inventario, run y progreso; tutorial/tips no reiterativos. Version1.3.0/code4 visible en portada. Frescura/promos/upgrades siguen inactivos.

L1 guía breve de pedido/compra/heladera; L2 cantidad3; L3 gustos/puntos4; L4 ganancia/pérdidas4; L5 aprovechamiento inventario4; L6 desafío autónomo5. L7–12 locked, conserva unlock histórico.

## Evidencia y selección proporcional
QA en checkout temporal ya importado `/tmp/Asadito-concurrent-cooking-qa`, sin tocar Library del Editor principal. Sources/data sincronizados. No se atribuyen logs previos a cambios nuevos.
- Compilación real y EditMode cierre **53/53 PASS** (3.39s): `/tmp/asadito-level1-editmode-closure.xml` + `.log`. Compra/capacidad/stock/insuficiencia/promos/preparación/waste/recovery, gates culinarios, progresión pagada1–6, migraciónv4 conrun/bonus/progreso, repeated reward/retained inventory y reparto.
- Validator contenido **PASS**: `/tmp/asadito-level1-content-final.log` (18 perfiles,108frames,12cards/layouts).
- Broad PlayMode inicial **12/13 PASS** (320.64s): `/tmp/asadito-level1-playmode.xml`; única falla: texto de detalleL6 superaba200px (220.84). Ampliado260, sin achicar la fuente. Otros tests core/gestos/navegación/retry/concurrencia aprobados.
- Focal L1–6 posterior recorrió compras/cocción/servicio/reward/persistencia/tip/legibilidad en los seis niveles; falló al consultar NIVEL7 disabled mediante helper que exige botón interactuable. Corregida assertion, sin cambiar gate del juego.
- Focal nuevo recovery/servicio crudo encontró dos defectos del driver: VOLVER homónimo de gameplay (ahora usa nombre específico Volver carnicería) y espera0.3s menor que animación de plato0.42s (ahora espera estado real). No se cambió interacción del usuario para acomodar tests.

## Cierre
Cierre gestión **3/3 PASS**,67.93s: `/tmp/asadito-level1-management-closure.xml` + `.log`. Incluye compra/preparación→coldreload sin recompra→cocción/servicio/reward/retry, rescateL1 saldo0→tutorial persistido→cancelaciónpreview, servicio crudo→0estrellas/noNext/ganancia negativa.
Cierre de recorridoL1–6 **1/1 PASS**,153.12s: `/tmp/asadito-level1-six-levels-closure.xml` + `.log`. Compra real, cocción real, cobro, tutorial/progreso, legibilidad y L7 Disabled comprobados. La revisión final agrega assertion de mesh de texto no vacío.
Renders Unity1080×1920: `/tmp/asadito-level1-planning.png`, `-guided-shop.png`, `-fridge.png`, `-raw-result.png`; generales `/tmp/asadito-management-{planning,shop,fridge,result}.png`. Se revisaron; ajuste de ascenders con altura2.5×/overflow conserva tamaño de fuente y muestra encabezados antes recortados.
APK única final ARM64/IL2CPP **Succeeded**: `/tmp/asadito-level1-android.log`. Copia local65MiB `/Users/celestino/Asadito/build/Asadito-1.3.0-management-level1-20261001.apk` (ignorada por Git). `aapt`: com.cuervation.asadito1.3.0/code4, min26/target36, soloarm64-v8a. `apksigner`: firma v2 verificada (debug).
SHA256 `6876a9ab20d112c3b257654c540c588248afc5efc369ac04d1021e891604cc61`.
Sources/data C#/JSON originales y QA comparados byte a byte sin diferencias antes del build. No se repitió build Android por cambios menores: una sola compilación final tras cierre de código.
ADB sin dispositivos tanto al comenzar build como al verificar APK; no instalación ni smoke físico de esta versión. Versión/flujo identificados en portada para distinguirla de1.2.0. Sin publicación ni firma productiva. QA humano/performance sigue siendo requisito de release comercial.
