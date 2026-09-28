# Asadito — Visual Bible

## Norte de arte

Asadito es un juego argentino de cocina casual premium: patio/quincho cálido, comida protagonista y lectura instantánea en móvil. Usar escenarios y comida 3D estilizados de formas suaves, materiales pintados/simplificados y luz de atardecer; no foto-realismo, ni caricatura infantil. Identidad propia: ritual compartido del asado, hierro y madera de patio, vocabulario rioplatense y pequeños acentos celeste/verde solo cuando ayuden a orientar.

**Regla de producción:** la acción y el estado de cocción siempre ganan a la decoración. Cámara fija, composición vertical, siluetas limpias y contraste suficiente. Mantener Canvas UI mientras la transición visual sea incremental; no rehacer el gameplay ni el alcance del MVP para acomodar arte.

## Paleta de trabajo

| Rol | Color de referencia | Uso |
|---|---|---|
| Tinta carbón | `#25221C` | contornos, texto oscuro, parrilla |
| Crema | `#FFF0D1` | texto claro, paneles y fondos de UI |
| Brasa | `#E95B32` | fuego, acción primaria y alerta |
| Dorado | `#F3AE48` | foco perfecto, acento y premio |
| Madera | `#8A5234` | parrilla, superficies y bandeja |
| Verde salvia | `#66815D` | estado positivo, hierbas y apoyo |
| Carne cruda | `#B94F46` | estado crudo; nunca usar el color como único indicador |

Usar colores cálidos con saturación moderada; fondo más oscuro y suave que la comida. Contraste de texto mínimo de lectura sobre móvil. Error no se codifica solo en rojo: añadir palabra/ícono/animación.

## UI, marca y navegación

- **Portada:** key art vertical con parrilla/comida como héroe, título legible arriba, CTA `ENTRAR` dominante y `SALIR` secundario. Entrar conserva el flujo existente de introducción/tutorial. `Application.Quit()` en Player; salir de Play Mode en Unity Editor.
- **Tipografía:** Lilita One para marca/display y Baloo 2 para UI, ambas OFL con licencias incluidas y tildes/números comprobados; el Canvas Legacy carga TTF desde `Resources` (no TMP). Evitar textos largos, versales compactas y tipografías display para instrucciones.
- **Botones:** grandes, con esquinas suaves, un verbo, estado normal/hover/tap/deshabilitado; primaria naranja/dorada, secundaria madera/crema. Feedback de color/escala corto y accesible.
- **Tarjetas/paneles:** crema o carbón con radio amable, agrupación y encabezado inequívoco. HUD no debe tapar parrilla, alimento ni bandeja; safe area en contenido de juego.
- **Marca:** lettering de `ASADITO` tipográfico y propio; sin calcar logo, disposición, íconos o forma de botones de otra obra. Tono cercano, rioplatense y apetitoso.

## Mundo, fuego y comida

- Patio/quincho: señales argentinas sutiles y cálidas (hierbas, madera, faroles, hierro); fondo en profundidad con detalle limitado. La parrilla debe contrastar del patio y conservar lectura en pantallas pequeñas.
- Parrilla: geometría simple de hierro, rejilla y brasero; brasas con núcleo dorado y borde naranja/rojo. Humo/chispas escasos y ligeros por rendimiento. Distribución de calor y estado deben seguir legibles incluso sin partículas.
- Comida: silueta reconocible primero; jugosidad/brillo y textura pintada simplificada. Tira, chorizo, vacío y provoleta deben distinguirse por forma, no solo por color. Estados crudo, cocción, punto ideal y quemado cambian valores/superficie y tienen señal visual distinta por cara.
- Comensales: retratos expresivos y memorables, caricatura suave sin realismo facial; perfiles, gustos y satisfacción en tarjetas simples con texto/íconos accesibles. En el First Playable los avatares son UI geométrica provisional.
- Bandeja, pinza y superficies: madera/metal coherentes con parrilla; utilería mínima y funcional. Feedback `PERFECTO`, advertencia, estrellas y puntaje con animación breve, sin cubrir alimento/órdenes.

## Motion, sonido y rendimiento

Animar únicamente para confirmar entrada, volteo, retiro, servicio, cambio térmico y reacción: movimiento corto, asentamiento elástico moderado, brillo pulsado suave; respetar pausa/legibilidad. No usar cámara movediza, partículas densas ni efectos que oculten el estado. Mantener URP móvil, luz controlada, transparencias reducidas y texturas comprimibles; probar vertical y safe areas.

## Portabilidad de la inspiración

La referencia es solo calidad/claridad de juegos casuales de cocina. No reproducir assets, logo, personajes, layout, iconografía ni composición identificable de Pocket Chef ni de terceros. La identidad de Asadito proviene del quincho, el asado compartido y tono argentino.

## Estado visual MVP al 2026-09-27

El First Playable continúa en Canvas/2D procedural y no es aún el objetivo 3D estilizado final. La nueva imagen `PortadaAsadito.png` es key art provisional para dar personalidad a la pantalla inicial, no modelo/runtime 3D ni prueba de aprobación visual. `PatioParrilla.png` queda provisional y separado de la portada. Comida individual, guest portraits, escenas y pantallas secundarias aún requieren producción visual dedicada; ver [plan](../../docs/art/plan.md) y [manifest](../../docs/art/asset-manifest.md).
