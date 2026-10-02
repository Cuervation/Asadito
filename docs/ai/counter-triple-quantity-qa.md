# Carnicería: cantidad triplicada — QA 2026-10-01

## Pedido y alcance
Mostrador más completo, triple cantidad. Usuario confirmó explícitamente que también quiere stock comprable24 por alimento:24 chorizos +24 tiras,48 piezas visibles iniciales, antes8+8. Precios100/180, monedas iniciales650, heladera8, rewards, inventario/guardado/preparación y cocción permanecen sin cambios.

## Implementación
- ManagementConfig.json: Stock24 para ambos productos; sin migración/creación de unidades ni débito por mostrar comida.
- ManagementWorldView: límite24 por SKU, count=min(límite, Stock real). Cuatro columnas/seis filas de profundidad por grupo, sobre el plano horizontal común; cámara perspectiva previa intacta. Escala uniforme adaptativa.078, separación de filas.052, pendiente/ángulos suaves y crecimiento por profundidad. Mallas/atlas/materiales compartidos originales.
- Stock cero vacía la exposición; el mostrador no rellena unidades ficticias. Toques/drag cotizan cada unidad por las reglas existentes; se debita y crea inventario solo al pagar. Pulsos/drag siguen restaurando HomeScale individual.
- Presupuesto de48piezas:66,912 triángulos de comida/48 MeshRenderers activos, shaders y texturas compartidos, RT cap1200 sin MSAA. Límite de test75k tris/60 renderers; no es medición de FPS físico.

## Validación proporcional
Unity6000.6.3f1 en checkoutQA aislado /tmp/Asadito-concurrent-cooking-qa, sin cerrar ni reimportar el Editor principal.
- Primera implementación **3/3 PlayMode PASS**,11.483s: /tmp/asadito-counter-triple-first.xml.48 targets exactos/UV originales/modelos volumétricos, viewport780×1100 y resize, bounds sin superposición, drag cancel/drop una vez/pago/reload y stock0/heladera vacía.
- Ajuste final de tamaño/espaciado (solo geometría de layout): **2/2 PlayMode PASS**,8.077s, /tmp/asadito-counter-triple-polish.xml y .log. Cada una de48 piezas tiene su propio centro físico accesible, misma altura de apoyo, bounds separados y contenidos; cartelesStock24, toques/drag y checkout intactos.
- Economía focal **3/3 EditMode PASS**,0.079s, /tmp/asadito-counter-triple-economy.xml y .log: Stock24/Stock23 tras compra1+1, saldo370 después de$280 y dos unidades únicas; atomicidad por exceder stock/capacidad y revalidación al agotarse. El caso sintético que compra todo el stock con descuento100% configura capacidad suficiente solo dentro del test; producción conserva8.
- Extensión de stock parcial/agotado **1/1 PlayMode PASS**,4.186s, /tmp/asadito-counter-triple-partial.xml y .log:5 chorizos+3tiras restantes muestran exactamente8targets y carteles5/3; cada centro físico accesible. Al agotar ambos productos desaparecen piezas, cartelesAGOTADO y heladera vacía sin unidades ficticias. Total final:3 casos únicosPlayMode+3EditMode aprobados en corridas focales, no suite completa.
- Capturas realesUnity1080×1920 inspeccionadas: build/qa-counter-triple/asadito-counter-triple-initial.png y asadito-counter-triple-cart.png. Las48 piezas están separadas y dentro de la superficie; fondo menor/frente mayor, carteles y changuito legibles. XML focales guardados en esa misma carpeta.

## Límites
Sin APK, instalación, commit ni push nuevos solicitados. Teléfono mantiene1.4.0/code6 anterior sin híbrido/changuito; requiere nueva build para mostrar estos cambios. Tacto/performance Android y aprobación estética humana de esta densidad pendientes. No suites completas ni build por este cambio local.
