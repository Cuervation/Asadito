# Arquitectura mínima

`AsaditoGame` contiene estado de la tanda, pedidos, cocción, puntuación, UI y feedback en un solo componente para facilitar la iteración inicial. La escena aporta cámara y el objeto raíz. Un instalador de Editor corre una vez para configurar sprites y conectar el componente; el juego crea su Canvas al entrar en Play Mode.

Separar datos de pedidos o sistemas de carne cuando haya más niveles. Reemplazar ilustración y piezas por sprites importados manteniendo las acciones de juego.
