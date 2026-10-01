# Auditoría baseline de Vertical 1 — 2026-10-01

Baseline main bda0f9f. Inspección de fuentes antes de integrar gestión: repo-map, AGENTS/skills, estado actual, FoodCatalog/model/serving/score/levels/save y consumidores UI/arte/tests.

|Área|Hallazgo|Decisión|
|---|---|---|
|Arquitectura|AsaditoGame mezcla UI/cocina, datos FoodCatalog aislados|Nuevos servicios runtime + ManagementScreen; adapter limitado|
|Niveles|12 authored, gate de uno, recetas premium tempranas|Capítulo de 12; L1–6 básicos y gate por config; futuros provisionales|
|Térmico|Perfiles individuales, cocción simultánea, 10 estados, un lado|Reutilizar sin reescribir calor|
|Invitados/score|Edad/peso/apetito, preferencias y cuatro componentes|Conservar; score gestión separado y media normalizada|
|Save|v3 progreso/settings, no inventario|Migración v4 aditiva, active run y reward único|
|UI|Canvas portrait1080×1920, botones arcade, input directo|Pantallas event-driven dentro safe area; mantener botones|
|Arte|90 sprites preparados fuera Resources|Catálogo de referencias explícitas para selección V1; no cargar freezer/estaciones futuras|
|Tests|Edit/PlayMode cubren core; assertions gate1 obsoletas|Actualizar contratos de progresión, agregar reglas y flujo gestión|
|Docs|MVP excluye economía/gestión/métodos, roadmap con flip obsoleto|Nueva autoridad full-game + specs por dominio; preservar historia marcada|

Descubrimiento durante compilación real: importer preparado anteriormente usaba propiedades inexistentes de TextureImporter (CS1061). Corregido a TextureImporterSettings; la validación estática previa no acreditaba compilación Editor.
Editor principal MCP responde tests_running y ping no contestado; validación aislada reutiliza /tmp/Asadito-concurrent-cooking-qa sin modificar Library del usuario.
