# Plan visual MVP — Asadito

Fuente de dirección y gates: [Visual Bible](../../specs/visual/art-direction.md), [estados de comida](../../specs/visual/food-states.md), [estados de fuego](../../specs/visual/fire-states.md) y [animación](../../specs/visual/animation.md). Una imagen/concepto no se considera integrado ni `FINAL` hasta import, validación en Editor y legibilidad portrait.

## Auditoría breve

- **Se conserva por ahora:** key art del patio `PatioParrilla.png` (tiene buena atmósfera y ya sirve de fondo gameplay, pero su tratamiento pictórico no termina de alcanzar el norte 3D casual); estados/tokens térmicos y la arquitectura runtime del First Playable.
- **Rehacer al avanzar el slice:** portada oscura genérica sin Salir; UI rectangular y tipografía de sistema; retratos geométricos; tira y chorizo planos; parrilla/brasas como rejilla UI; feedback sin assets cohesivos.
- **Nuevos básicos:** imagen hero propia para portada, botón de entrada/salida funcional, sistema de componentes de UI redondeados, key art 3D estilizado/lighting coherente.

## Orden de actualización

1. Portada y marca: usar `PortadaAsadito.png`; titular real y botones UI (no texto horneado en bitmap), Entrar al flujo de intro, Salir por plataforma.
2. Sistema UI reusable: forma/redondeo, paleta, tamaños, feedback, safe area y tipografía con soporte español.
3. Gameplay HUD + resultado: jerarquía, estados accesibles, tarjetas, score/estrellas; no cambiar reglas de juego.
4. Parrilla/brasas: plano cenital legible, gradiente térmico, animación de brasa/humo económica.
5. Chorizo y tira: sprites/modelos por estado y cara; aprovechar `ChorizoCrudoCutout.png` solo mientras siga provisional.
6. Bandeja/pinza/madera y fondo quincho estilizado; después retratos y expresiones.
7. Provoleta, vacío y level select cuando correspondan al scope; no crear contenido de niveles futuros dentro del MVP.

**Dependencias:** escoger fuente licenciada antes del polish final; componentes UI y guías portrait antes de rehacer pantallas; silueta/material de cada comida antes de estados de cocción; la parrilla visual debe respetar la lectura del `HeatGrid` existente. No importar un paquete gráfico genérico sin revisar licencias/peso.

## Estado/alcance de esta actualización

Implementación incremental sobre `AsaditoGame`/`SampleScene`: portada nueva con key art, título de marca, acciones `ENTRAR`/`SALIR`, botones redondeados/feedback, transición, fuentes OFL y app icon Android; se sumó count-up de score y feedback de botones manteniendo la simulación. La carga tipográfica y portada/intro se revisaron en Play Mode, pero el bridge MCP dejó de responder tras recompilar y el intercambio final de arte no está revalidado; las pruebas 9/9 son anteriores a la última pasada. Comida, fuego, guest cards, level select y Results siguen pendientes/provisionales. Ver [manifest de assets](asset-manifest.md) para estados; no declarar lo no probado como final.
