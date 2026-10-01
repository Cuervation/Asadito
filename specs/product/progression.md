# Progresión — fuente canónica

Los 12 niveles son Capítulo 1, no el juego entero. Desbloqueo secuencial por al menos una estrella; cards futuras permanecen Disabled con candado. El gate numérico de entregas completas está en ManagementConfig. V1 habilita L1–6; L7–12 no se liberan hasta tener sus verticales reales. Se retienen todos los scores/unlocks históricos.

| Nivel | Enseñanza |
|---|---|
|1|El debut: 2 invitados, compra guiada 1 chorizo/1 tira con monedas iniciales → heladera → selección → cocina → cobro|
|2|Más invitados: 3 comensales, calcular cantidades; sugerencia de pedido, compra autónoma|
|3|A gusto del cliente: 4 comensales, elegir cortes/puntos según preferencias|
|4|Cuidá tus monedas: 4 comensales, ingreso/costo/ganancia y pérdidas; sin bonus nuevo|
|5|El buen administrador: 4 comensales, revisar y usar inventario antes de comprar|
|6|Tu primer desafío: 5 comensales, ciclo completo sin asistencia obligatoria|
|7|Heladera/conservación ampliada (V2)|
|8|Frescura (V2)|
|9|Promo (V3)|
|10|Capacidad (V3/V4)|
|11|Primera mejora (V4)|
|12|Integrador|

Pedidos L1–6 usan únicamente chorizo/tira. Pedidos L7–12 todavía retienen contenido histórico provisional y deben rebalancearse cuando se implemente su enseñanza, no presentarse como definitivos.
Comensales conservan edad/tamaño/apetito para hambre y favoritos/gustados/rechazados/punto; género nunca determina consumo. Cantidades authored son porciones jugables, no guía nutricional.

Capítulos futuros: Aprender → Gestionar (freezer/promos/grandes grupos) → Asador (cruz/piezas largas) → Maestro del fuego (múltiples estaciones/eventos). Unlock alimentos: inicio chorizo/tira; temprano vacío/morcilla/provoleta; medio entraña/achuras/cerdo/pollo; avanzado lomos/bifes/premium. No exponer los 18 al principio.

## Tutorial persistente
L1 usa mensajes breves por acción, limita la primera compra a lo que falta del pedido y la primera preparación a esa mezcla. No regala unidades ni obliga a recomprar inventario existente. Una vez servido/cobrado L1, `ManagementTutorialCompleted` evita repetir la guía y permite preparación libre. Los tips de L2–5 se muestran una vez mediante `LearningTipsSeen`; L6 no tiene guía obligatoria. Los usuarios v4 con jornadas de gestión completadas no repiten la guía; `TutorialCompleted` de cocina antigua no implica conocer la compra.
