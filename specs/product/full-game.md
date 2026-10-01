# ASADITO — dirección canónica

ASADITO es un juego móvil de gestión de asados argentinos donde planificás la juntada, comprás y administrás la carne, mejorás tu patio y demostrás tu habilidad cocinando para que todos se vayan contentos.

## Loop
Próximo asado → invitados/necesidades → heladera → carnicería → compra → inventario → preparación → bandeja → estación → tabla → servicio → satisfacción → economía → monedas → mejoras → próximo asado.
La gestión determina lo que llega al fuego; cocinar sigue siendo habilidad táctil. No se reemplaza el motor térmico.

## Alcance por vertical
1. Compra básica, saldo, heladera, selección, cocina existente, resultado/recompensa y save migrado. Solo chorizo/tira.
2. Frescura por jornadas, vencimiento y desperdicio.
3. Promociones con decisiones de capacidad/saldo/conservación.
4. Mejoras funcionales y visibles del patio.
5. Freezer con descongelación por ciclo.
6. Asador/cruz; luego disco y horno de barro.

Cada vertical necesita flujo completo, pruebas de reglas, integración y evidencia visual antes de ampliar alcance. La primera no promete freezer, eventos ni upgrades activos.

## Fuentes de verdad
- [Economía/score/recovery](economy.md)
- [Inventario/preparación/save](inventory.md)
- [Carnicería/stock/promos](butcher-shop.md)
- [Conservación](refrigeration.md)
- [Capítulos/tutorial](progression.md)
- [Mejoras](upgrades.md)
- [Eventos](game-events.md)
- Cocción, invitados y servicio: specs existentes en `systems/`; FoodCatalog conserva identidad, visuales y perfiles.
Datos de tuning numérico: `ManagementConfig.json`; pedidos del capítulo: `ChapterOneLevels.json`.

## Límites absolutos
Sin carbón, encendido/apagado, brasas manipulables, energía, vidas, timers reales/offline, multiplayer, NPCs caminando, anuncios, monetización ni compras reales.
Mobile-first: objetos, cards, iconos y acciones breves; no un panel administrativo. Mantener botones arcade actuales y arte de patio. No nuevos frameworks, microservicios ni event bus genérico. No Update por unidad de inventario.

## Historia
`mvp-scope.md` y auditorías fechadas conservan decisiones del prototipo: sus exclusiones de gestión/economía/métodos NO definen el futuro del producto. Esta spec reemplaza aquella dirección, no borra sus evidencias.
