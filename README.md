# ASADITO

Juego móvil **100 % 2D** de gestión de asados argentinos: planificación, compras, inventario y habilidad cocinando para satisfacer invitados.

Repositorio: [Cuervation/Asadito](https://github.com/Cuervation/Asadito), branch `main`. Unity6000.6.3f1, URP, portrait mobile.

## Orientación
- Dirección canónica: [full-game](specs/product/full-game.md), con specs por dominio enlazadas.
- Estado jugable y límites: [estado actual](docs/current-project-state.md).
- Implementación: [repo-map](docs/repo-map.md), [gestión 2D](docs/architecture/management-2d.md), [AGENTS](AGENTS.md).
- Aceptación actual: [Vertical1](specs/acceptance/management-vertical1-gate.md).
- Evidencia fechada: [QA Vertical1](docs/ai/management-vertical1-qa.md).
- MCP/Editor: [setup](UNITY_MCP_SETUP.md).

Vertical1 conecta compra de chorizo/tira → heladera → preparación → cocina/servicio → resultado económico → saldo persistente. Tutorial/progresión gradual; futuras verticales en [roadmap](docs/roadmap.md).

La presentación utiliza ilustraciones, sprites y animaciones Canvas; no hay mundos de gestión 3D. Los atlas de alimentos, fuentes y botones existentes se conservan.

Las specs/auditorías históricas del MVP y de gestión 3D/híbrida conservan evidencia del prototipo, no describen la arquitectura vigente. No carbón/encendido/apagado, energía/vidas, tiempo offline, monetización ni multiplayer.
