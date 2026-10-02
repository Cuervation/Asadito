# Asadito — Visual Bible

## Norte de arte

Asadito es un juego argentino de cocina casual premium: patio/quincho cálido, comida protagonista y lectura instantánea en móvil. Usar ilustraciones y sprites **100 % 2D** cartoon, colores cálidos, contornos limpios, formas reconocibles e iluminación pintada; no foto-realismo, ni caricatura infantil. Identidad propia: ritual compartido del asado, hierro y madera de patio, vocabulario rioplatense y pequeños acentos celeste/verde solo cuando ayuden a orientar.

**Regla de producción:** la acción y el estado de cocción siempre ganan a la decoración. En toda parrilla de gameplay/key art, usar composición ilustrada cenital/top-down (sin vista lateral/oblicua dominante), con rejilla de hierro grafito, superficie de madera y alimentos legibles desde arriba; sin celdas/grid térmico, masa naranja ni affordances de carbón. Composición fija vertical, siluetas limpias y contraste suficiente. Mantener Canvas UI como presentación definitiva 2D; no rehacer el gameplay ni el alcance del MVP para acomodar arte.

## Decisión definitiva — 100 % 2D (2026-10-02)
Todos los elementos de juego se representan con ilustraciones, sprites, capas/transparencias y motion 2D. Sombras pintadas, perspectiva ilustrada y animaciones que sugieren profundidad están permitidas; modelos, geometría/colliders 3D, cámaras/RenderTextures de mundos de gestión y efectos volumétricos no. La referencia Pocket Chef se limita al acabado casual premium, nunca a copiar identidad/recursos.

Carnicería conserva CounterV2; heladera conserva `Fridge_Hybrid_OpenEmptyV2` abierta (el nombre histórico del PNG no define la técnica). Ambos usan el sprite RAW original y tamaño por corte de la parrilla. Orden visual y máscaras alfa seleccionan alimentos expuestos incluso apilados; carteles/UI bloquean lo que tapan. Fuentes, botones arcade, catálogo, retratos, escenarios y animaciones aprovechables se reutilizan, no se regeneran por lotes ni se reemplazan por placeholders.

La arquitectura vigente se describe en [gestión 2D](../../docs/architecture/management-2d.md). Las secciones fechadas 3D/híbridas de esta página son solo historia, reemplazadas por esta decisión. Legibilidad/safe area y respuesta táctil Android prevalecen sobre decoración; QA física y aprobación artística no se presumen a partir de tests Unity.

## Paleta de trabajo

| Rol | Color de referencia | Uso |
|---|---|---|
| Tinta carbón | `#25221C` | contornos, texto oscuro, parrilla |
| Crema | `#FFF0D1` | texto claro, paneles y fondos de UI |
| Brasa | `#E95B32` | fuego, acción primaria y alerta |
| Dorado | `#F3AE48` | foco perfecto, acento y premio |
| Oro arcade | `#FFD33A` → `#FFA60D` | botones primarios elevados |
| Coral arcade | `#FF644E` → `#ED312B` | volver/salir y acciones secundarias |
| Tinta de contorno | `#301713` | borde y sombra de botones/títulos |
| Madera | `#8A5234` | parrilla, superficies y bandeja |
| Verde salvia | `#66815D` | estado positivo, hierbas y apoyo |
| Carne cruda | `#B94F46` | estado crudo; nunca usar el color como único indicador |

Usar colores cálidos con saturación moderada; fondo más oscuro y suave que la comida. Contraste de texto mínimo de lectura sobre móvil. Error no se codifica solo en rojo: añadir palabra/ícono/animación.

## Brand

- Wordmark propio `Asadito` (sin coma): Lilita One con trazo chunky/redondeado, cara blanca, contorno casi negro marcado y sombra oscura corta; usar mayúscula inicial. La referencia de juegos casuales solo inspira tipografía, contorno, sombra y jerarquía; no imitar ningún logo, personaje ni composición reconocible.
- La portada muestra la línea secundaria exacta `el sabor Argentino` debajo del wordmark, con Lilita One blanca y contorno oscuro. La identidad argentina va integrada a un repasador de tela dentro del key art: tres franjas celeste/blanco/celeste y un Sol de Mayo pequeño que sigue los pliegues; no añadir un estandarte, mástil ni cuadrado ondeando por separado. Mantener título y repasador en el aire libre del tercio superior; no cubrir la parrilla ni los alimentos.
- Icono de launcher: símbolo original simple de chorizo sobre parrilla, medallón crema/rojo y glow de brasa; sin texto ni escena completa. El icono adaptativo usa el mismo asset en los slots Android existentes.
- No tomar formas, composición, personajes, lettering o íconos identificables de Pocket Chef ni de otro juego.

## UI / tipografía

- **Portada:** key art vertical con parrilla/comida como héroe, `Asadito` legible arriba y sin tapar la parrilla, línea `el sabor Argentino` inmediatamente debajo con separación vertical corta, y un repasador dentro de la ilustración con el motivo argentino integrado a su tela. No crear una bandera animada/superpuesta en UI. Título y bajada usan Lilita One chunky, cara blanca y contorno/sombra oscuros. CTA `ENTRAR` y `SALIR` tienen las mismas dimensiones y ambas etiquetas centradas geométricamente en sus botones; color oro para entrar, coral para salir. Entrar conserva el flujo existente de introducción/tutorial. `Application.Quit()` en Player; salir de Play Mode en Unity Editor.
- **Selector de niveles:** reemplazar la lista de botones con información por una grilla visual de postales clickeables, dos columnas, cada una con ilustración propia y solo la etiqueta `Nivel 1`…`Nivel 12`. No mostrar nombre descriptivo, comensales, platos, estrellas, candados ni subtítulo por ahora; el bloqueo de niveles conserva su lógica y solo cambia la disponibilidad táctil.
- **Tipografía:** Lilita One Regular para wordmark, títulos cortos y etiquetas de botón, en blanco con contorno/sombra tinta; Baloo 2 para texto corrido e instrucciones legibles (pesos 400/500/600/700/800). Las licencias OFL están dentro del proyecto. Canvas Legacy carga TTF desde `Resources` (no TMP); no se generan TMP assets mientras no se use TextMeshPro. Nunito queda como fallback futuro, no instalado sin evidencia de legibilidad insuficiente. Revisar ñ, tildes, números y tamaños mínimos en teléfono.
- **Botones:** todos los botones de UI usan silueta arcade de esquinas moderadas, cara dorada degradada, marco tinta y relieve/sombra inferior; volver/salir usan coral-rojo. Etiqueta grande Lilita One blanca delineada y escalada con la altura del botón para conservar jerarquía entre CTA, controles de gameplay y tarjetas. Feedback de brillo/escala corto y accesible; deshabilitados conservan un estado apagado inequívoco.
- **Tarjetas/paneles:** crema o carbón con radio amable, agrupación y encabezado inequívoco. HUD no debe tapar parrilla, alimento ni tabla; safe area en contenido de juego.

## Style, color y formas

- Casual premium con ilustraciones 2D cartoon, siluetas claras, contornos limpios, sombras y luz cálida pintadas; nunca hiperrealismo fotográfico ni infantilización.
- Paleta: tinta carbón `#25221C`, crema `#FFF0D1`, brasa `#E95B32`, dorado `#F3AE48`, madera `#8A5234`, salvia `#66815D` y carne cruda `#B94F46`. Escenario menos contrastado que comida/UI.
- Botones, bandejas, platos y tarjetas usan radios amplios, volumen/sombra suave y contraste alto; evitar detalles muy finos, exceso de brillo y saturación.

## Food, mundo, fuego y guests

- Patio/quincho: señales argentinas sutiles y cálidas (hierbas, madera, faroles, hierro); fondo en profundidad con detalle limitado. La parrilla debe contrastar del patio y conservar lectura en pantallas pequeñas.
- Parrilla: fondo de juego top-down con rejilla oscura, hierro/grafito y quincho de madera cálida pero poco saturada. El heat model es uniforme e invisible. El alimento conserva prioridad visual; el humo de cocción es escaso y ligero. La pinza de metal/madera se comunica con sprites PNG originales abiertos/cerrados; la tabla es una superficie de madera ilustrada y el drop target, invisible, coincide con su área visual.
- **Food:** silueta reconocible primero; jugosidad, brillo y textura pintada simplificada. Tira, chorizo, vacío y provoleta se distinguen por forma, no solo color. El catálogo integra 18 atlas individuales (uno por alimento), seis etapas térmicas cada uno (RAW, WARMING, BROWNING, IDEAL, OVERCOOKED, BURNT; 108 sprites dinámicos). Conservar los estados y animaciones existentes de cocción; no añadir controles de dar vuelta, bandeja o servir. El cambio visual es discreto por sprite, no blend de material.
- **Guests:** caricatura suave casual premium, identidad y expresión legibles; texto/ícono acompaña la reacción. `GuestPortraitAtlas.png` ahora aporta 24 retratos raster ilustrados semi-cartoon (seis perfiles × cuatro expresiones); el runtime cambia retrato individual en reacción/resultados y pasa PlayMode. Recorte/escala inspeccionados en GameView portrait; falta validación de device.
- **VFX:** humo ligero de círculos Canvas por cocción y glow de portada. No hay VFX/animación de encendido, carbón o distribución de calor. Son recursos baratos/provisionales, no humo volumétrico ni sistema final de partículas; evaluar legibilidad/rendimiento móvil antes de sumar efectos.
- Bandeja, pinza y superficies: madera/metal coherentes con parrilla; utilería mínima y funcional. Feedback `PERFECTO`, advertencia, estrellas y puntaje con animación breve, sin cubrir alimento/órdenes.

## Motion, sonido y rendimiento

Animar para confirmar entrada, selección, arrastre, incorporación al carrito, traslado/devolución de heladera, retiro, servicio, cambio térmico y reacción: movimiento corto, asentamiento elástico moderado, brillo pulsado suave; respetar pausa/legibilidad. No usar cámara movediza, partículas densas ni efectos que oculten el estado. Conservar el pipeline actual, animaciones Canvas livianas, transparencias contenidas y texturas comprimibles; sin iluminación/efectos volumétricos, modelos ni cámaras/RenderTextures de mundos 3D; probar vertical y safe areas.

## Portabilidad de la inspiración

La referencia de interfaz adjunta inspira solo rasgos generales de UI arcade (tipografía chunky delineada, botones dorados/rojos elevados, contraste vivo). No copiar su pantalla de ajustes, textos de configuración, etiqueta de versión, layout, assets, logo, iconografía ni composición identificable. Mantener el key art, vocabulario, distribución móvil y mundo cálido propios de Asadito.

## Evolución visual de gestión y patio — biblioteca preparada (2026-10-01)

La biblioteca de progresión debe sentirse del mismo mundo, con cartoon estilizado y materiales pintados/simplificados; no subirla a fotorrealismo. La carnicería puede ser frontal/3⁄4 y vertical; toda parrilla, mesa y estación de cocción destinada a la acción debe ser cenital y legible en portrait.

- **Patio modular:** componer cinco niveles visuales sobre un solo fondo de patio. Superponer módulos de parrilla, heladera, mesada/isla, freezer, mesa, macetero, luces, asador/cruz, disco y horno; desbloquear no reemplaza el lugar por otra locación.
- **Gestión desacoplada:** cards, popups, ribbons, stickers, contenedores de resultados y stock son marcos vacíos/reutilizables. Precio, nombre, porcentaje, unidades, monedas, satisfacción y métricas se renderizan como texto dinámico; no hornear datos mutables ni botones dentro de imágenes.
- **Comida en almacenamiento:** utilizar `FoodCatalog`/sus atlas existentes para los slots abiertos de heladera, inventario y frescura, en vez de mantener imágenes duplicadas por carne y tier. Los overlays FRESCA/OK/CONSUMIR PRONTO/PODRIDA comunican tanto etiqueta/ícono como color.
- **Moneda:** símbolo Asadito único y reconocible en tamaño HUD; reutilizar la misma moneda individual para HUD y precios, con grupos/pilas y cuatro frames discretos para feedback.
- **Personaje de tienda:** seis poses iniciales del mismo atlas cubren neutral, saludo, compra, oferta, agotado y felicitación. Una ilustración complementaria de sorpresa/evento está preparada, pero su identidad visual debe armonizarse antes de conectarla a la misma persona. Las poses son frames estáticos, no un rig.
- **Estaciones futuras:** parrillas tier 1–3, asador/cruz, disco y horno en top-down; estados separados para composición/animación. El horno abierto con bandeja es la vista activa con comida. No sumar carbón interactivo, ignición ni apagar la parrilla caliente actual.
- **Importación y alcance:** PNG nuevos como sprites individuales, centro/100 PPU, filtro bilinear, clamp, sin mipmaps y compresión ETC2 HQ; fondos limitados a 2048 y sprites a 512. Mantenerlos fuera de `Resources` hasta que una pantalla/sistema los consuma para no inflar la build presente. Cuatro sprites de gestión están conectados (patio, moneda, CounterV2 y FridgeV2); el resto de la biblioteca permanece preparado; ver [manifest completo](../../docs/art/management-asset-manifest.md).

### Auditoría de conservación

En la fase visual 2026-10-01 se conservaron los 18 atlas de alimentos (108 estados), las 12 level cards, retratos, escenarios, props, logo e icono actuales. Se observó que los niveles 1–5 y 6–12 difieren algo en estilo, pero no se regeneraron por lotes: cualquier armonización será una decisión separada después de revisar referencias y uso.

## Estado visual al 2026-09-29

El runtime continúa en Canvas/2D procedural y el arte es PROVISIONAL: conectado y automatizable, pero pendiente de aprobación humana y QA físico. `ParrillaTopDownStylized.png` aporta arte cenital; 18 atlas nuevos proveen 108 etapas por alimento; `GuestPortraitAtlas.png` contiene 24 retratos (6 identidades × 4 expresiones). `AsaditoLogo.png` usa Lilita One como wordmark; `AsaditoAppIcon.png` se exporta como icono adaptativo Android. `PortadaAsadito.png` es key art top-down y sitúa la bandera en un repasador. El selector muestra doce postales con solo `Nivel N`.

EditMode valida 18/18 perfiles y seis sprites, PlayMode recorre L1–L12; APK Android ARM64/API36 compila, instala y renderiza la portada en emulador. Ninguna de esas comprobaciones automatizadas equivale a touch humano, notch/safe-area física, rendimiento real ni revisión final de contraste/crops. El fondo `ParrillaTopDownGameplay.png` y `PatioParrilla.png` se retienen como fallback conectado. UI, VFX, sonido, transiciones térmicas y motion son procedurales/provisionales. Ver [plan](../../docs/art/plan.md), [manifest](../../docs/art/asset-manifest.md) y [Android](../../docs/android-release.md).

## Gestión 3D — primera implementación (2026-10-01, histórica)
Carnicería y heladera usan geometría tridimensional real, cámara fija elevada y UI superpuesta segura. Chorizo curvo con nudos de tripa; tira irregular con grasa, vetas y cuatro huesos. Mesh assets nativos de Unity, colores de vértice y microtextura procedural en shader móvil compartido. Vidrio de una pasada, sombras de contacto geométricas, props fijos combinados; built-in renderer actual (no migración a URP). Modelos de producción inicial, no primitivas visibles/billboards como alimento; aprobación estética humana y medición de teléfono siguen siendo gates independientes. Biblioteca 2D original preservada para cocina, menú y futuras referencias.

## Carnicería híbrida — decisión histórica reemplazada (2026-10-01)
Por feedback del usuario, escenario/mostrador ilustrado2D vacío con comida3D volumétrica usando exactamente el raw atlas de parrilla. Misma silueta/color/textura reconocible en cocina, tienda e inventario; no comida geométrica genérica. Changuito vectorial visible y arrastre táctil para agregar antes de pagar. Sin vuelos repetidos ni texturas duplicadas; UI mantiene botones actuales. Heladera pasa también a escenario2D abierto vacío con los mismos modelos/texturas y medidas de comida; la puerta3D anterior queda histórica. Ver decisión siguiente.

## Heladera híbrida — decisión histórica reemplazada (2026-10-02)
Fondo ilustrado vacío de heladera abierta con interior amplio y tabla frontal, transparente/versionado y sin alimentos/texto horneados. Solo las unidades reales del inventario son3D, usando atlas raw/mallas de carnicería/parrilla y su tamaño por corte. Al superponerse, los colliders de silueta y profundidad seleccionan la pieza expuesta. Fondo y overlay comparten un frame de aspecto nativo contenido en el panel central, sin estirar o invadir footer. Botones arcade existentes intactos; descarte fuera del área de alimentos. Arte PROVISIONAL hasta aprobación humana/dispositivo.
