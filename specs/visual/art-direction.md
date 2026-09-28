# Asadito — Visual Bible

## Norte de arte

Asadito es un juego argentino de cocina casual premium: patio/quincho cálido, comida protagonista y lectura instantánea en móvil. Usar escenarios y comida 3D estilizados de formas suaves, materiales pintados/simplificados y luz de atardecer; no foto-realismo, ni caricatura infantil. Identidad propia: ritual compartido del asado, hierro y madera de patio, vocabulario rioplatense y pequeños acentos celeste/verde solo cuando ayuden a orientar.

**Regla de producción:** la acción y el estado de cocción siempre ganan a la decoración. En toda parrilla de gameplay/key art, usar cámara cenital/top-down (recta u ortográfica levemente elevada, sin vista lateral/oblicua dominante), con rejilla, brasas, calor y alimentos legibles desde arriba. Cámara fija, composición vertical, siluetas limpias y contraste suficiente. Mantener Canvas UI mientras la transición visual sea incremental; no rehacer el gameplay ni el alcance del MVP para acomodar arte.

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

## Brand

- Wordmark propio `ASADITO`: Lilita One con crema, contorno carbón, extrusión terracota/sombra y acento pequeño de brasa; el lettering se mantiene legible en tamaño chico.
- Icono de launcher: símbolo original simple de chorizo sobre parrilla, medallón crema/rojo y glow de brasa; sin texto ni escena completa. El icono adaptativo usa el mismo asset en los slots Android existentes.
- No tomar formas, composición, personajes, lettering o íconos identificables de Pocket Chef ni de otro juego.

## UI / tipografía

- **Portada:** key art vertical con parrilla/comida como héroe, título legible arriba, CTA `ENTRAR` dominante y `SALIR` secundario. Entrar conserva el flujo existente de introducción/tutorial. `Application.Quit()` en Player; salir de Play Mode en Unity Editor.
- **Tipografía:** Lilita One Regular se reserva para wordmark y display; Baloo 2 se usa en UI con pesos instalados 400/500/600/700/800. Las licencias OFL están dentro del proyecto. Canvas Legacy carga TTF desde `Resources` (no TMP); no se generan TMP assets mientras no se use TextMeshPro. Nunito queda como fallback futuro, no instalado sin evidencia de legibilidad insuficiente. Revisar ñ, tildes, números y tamaños mínimos en teléfono.
- **Botones:** grandes, con esquinas suaves, un verbo, estado normal/hover/tap/deshabilitado; primaria naranja/dorada, secundaria madera/crema. Feedback de color/escala corto y accesible.
- **Tarjetas/paneles:** crema o carbón con radio amable, agrupación y encabezado inequívoco. HUD no debe tapar parrilla, alimento ni bandeja; safe area en contenido de juego.

## Style, color y formas

- Casual premium con mundo 3D estilizado/semi-cartoon, materiales pintados simples, siluetas claras y luz cálida; nunca hiperrealismo fotográfico ni infantilización.
- Paleta: tinta carbón `#25221C`, crema `#FFF0D1`, brasa `#E95B32`, dorado `#F3AE48`, madera `#8A5234`, salvia `#66815D` y carne cruda `#B94F46`. Escenario menos contrastado que comida/UI.
- Botones, bandejas, platos y tarjetas usan radios amplios, volumen/sombra suave y contraste alto; evitar detalles muy finos, exceso de brillo y saturación.

## Food, mundo, fuego y guests

- Patio/quincho: señales argentinas sutiles y cálidas (hierbas, madera, faroles, hierro); fondo en profundidad con detalle limitado. La parrilla debe contrastar del patio y conservar lectura en pantallas pequeñas.
- Parrilla: geometría simple de hierro, rejilla y brasero; brasas con núcleo dorado y borde naranja/rojo. Humo/chispas escasos y ligeros por rendimiento. Distribución de calor y estado deben seguir legibles incluso sin partículas.
- **Food:** silueta reconocible primero; jugosidad, brillo y textura pintada simplificada. Tira, chorizo, vacío y provoleta se distinguen por forma, no solo color. `FoodStateAtlas.png` contiene 4 etapas ilustradas por producto (16 sprites), seleccionadas gradualmente por umbral térmico; todavía no es blend de material ni seis estados/caras independientes.
- **Guests:** caricatura suave casual premium, identidad y expresión legibles; texto/ícono acompaña la reacción. `GuestPortraitAtlas.png` aporta seis perfiles por cuatro expresiones; el runtime cambia retrato individual en reacción/resultados, validado por PlayMode, aún sin aprobación de recorte en Editor/teléfono.
- **VFX:** pulso de brasas/color ligado a energía; humo ligero de círculos Canvas y glow de portada. Son recursos baratos/provisionales, no humo volumétrico ni sistema final de partículas; evaluar legibilidad/rendimiento móvil antes de sumar efectos.
- Bandeja, pinza y superficies: madera/metal coherentes con parrilla; utilería mínima y funcional. Feedback `PERFECTO`, advertencia, estrellas y puntaje con animación breve, sin cubrir alimento/órdenes.

## Motion, sonido y rendimiento

Animar únicamente para confirmar entrada, volteo, retiro, servicio, cambio térmico y reacción: movimiento corto, asentamiento elástico moderado, brillo pulsado suave; respetar pausa/legibilidad. No usar cámara movediza, partículas densas ni efectos que oculten el estado. Mantener URP móvil, luz controlada, transparencias reducidas y texturas comprimibles; probar vertical y safe areas.

## Portabilidad de la inspiración

La referencia es solo calidad/claridad de juegos casuales de cocina. No reproducir assets, logo, personajes, layout, iconografía ni composición identificable de Pocket Chef ni de terceros. La identidad de Asadito proviene del quincho, el asado compartido y tono argentino.

## Estado visual MVP al 2026-09-28

El runtime continúa en Canvas/2D procedural y aún no satisface el norte casual-premium completo. `ParrillaTopDownStylized.png` (941×1672) es ilustración cenital original; `FoodStateAtlas.png` aporta 16 cortes (4 comidas × 4 etapas) y `GuestPortraitAtlas.png` 24 cortes (6 identidades × 4 expresiones), activos en runtime. `AsaditoLogo.png` usa Lilita One tratada como wordmark original; `AsaditoAppIcon.png` actualiza la insignia Android sin cambiar sus GUID slots. `PortadaAsadito.png` se regeneró en cámara top-down ortográfica, con parrilla rectangular y zonas reservadas para logo/CTA, y su PNG fue revisado directamente; aún falta verificar crop/contraste ya montado en GUI/teléfono. PlayMode valida carga/uso y flujos, pero no reemplaza inspección de pantallas, safe-area/touch QA ni aprobación en teléfono. El fondo fotográfico anterior de gameplay queda como fallback. UI, humo/brasa, sonido, food-state transitions y motion siguen provisionales. Ver [plan](../../docs/art/plan.md) y [manifest](../../docs/art/asset-manifest.md).
