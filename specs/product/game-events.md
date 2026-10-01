# Eventos — diseño preparado

No se ejecutan eventos en Vertical 1. Tutorial siempre determinista y stock controlado.
V3+: datos de evento con Id, nivel/condiciones elegibles, payload (extra guest, falta de stock, oferta, pedido especial, premium, preferencia, juntada especial), ventana por jornada y seed opcional. Resolver una vez al preparar la jornada; guardar elección si hay aleatoriedad. Nunca rerollear abriendo UI ni modificar sorpresivamente carne ya comprada sin información.
Pruebas de elegibilidad, determinismo, aplicación única y recovery son gate antes de activar. No event bus enorme ni RNG global oculto.
