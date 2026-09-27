# Sistema — fuego y calor

## Regla canónica

El fuego del MVP es carbón. Su estado espacial se representa en un `HeatGrid` configurable, inicializado en 8×6 celdas. Cada celda expone `Heat` (calor disponible para transferencia) y `EmberEnergy` (energía retenida por las brasas). No se simulan inventario ni compra de combustible.

Cada alimento consulta el calor agregado/ponderado de la región de grilla que ocupa; la consulta debe permitir que distintas posiciones reciban intensidades diferentes. La transferencia desde esa región actualiza `FoodState` según el modelo de cocción. La resolución del grid es configurable para balance y desempeño; 8×6 es el valor inicial verificable.

`SimulationTimeScale` global inicia en 30 y es configurable para tuning. El menú DEBUG ofrece valores 20/25/30/35/40 independientemente del nivel. La escala solo acelera o ralentiza la evolución temporal de la simulación: no define punto ni sustituye estados térmicos por temporizadores.

## Verificación

- Puede configurarse la dimensión del grid y el inicio por defecto es 8×6.
- Una consulta ligada a la región de un alimento entrega calor afectado por sus celdas, no por una única temperatura global obligatoria.
- `Heat` y `EmberEnergy` están disponibles por celda.
- `SimulationTimeScale` inicia en 30 y las cinco opciones DEBUG pueden elegirse en cualquier nivel.
