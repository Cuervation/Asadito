# Arquitectura actual

- `AsaditoGame` sigue siendo el coordinador del ciclo/UI procedural; se preserva para minimizar riesgo al MVP y se separó el gameplay visual bajo `gameplayRoot`, además de catálogos/presentación y SFX.
- `GrillHeatModel` provee calor uniforme siempre activo (210 °C inicial, sin carbón, combustible, heat grid, brasas ni acciones de encendido); `FoodCookingModel` conserva temperatura por cara, centro, humedad, Maillard, carbonización y perfiles data-driven.
- `FoodPieceTouch` enruta tap/drag directo de los sprites de comida al flujo común de selección; hit targets invisibles ampliados, pointer dueño único, flip universal y drop target transparente encima de `TablaAsador`. Los PNG de pinza abierta/cerrada siguen al alimento y comunican el agarre.
- `FoodCatalog.json` carga definiciones serializables por `FoodCatalog`; `FoodDefinition` contiene identidad/recorte/perfil, clona arrays/bandas defensivamente. `FoodCookingModel` ejecuta un algoritmo común sobre `FoodCookProfile`; comportamiento de queso se habilita con flag de datos. No agrega código por ID al añadir comida.
- `FoodSpriteLibrary` crea seis recortes desde atlas vertical de seis celdas; cargador de presentación usa atlas por ID lazy. Import settings aplica compresión/tamaño móvil para atlases y cards.
- `MvpLevelCatalog` configura 12 niveles y seis guest profiles; `MvpSaveData` esquema 3 conserva progreso anterior y migra el default histórico de SimulationTimeScale sin pisar escalas personalizadas.
- `ServingAllocator` determinista y codicioso: un invitado no recibe segunda porción hasta que cada invitado elegible haya recibido una; después compara cobertura, preferencias y punto.
- Unity crea Canvas en runtime en escena SampleScene; mantiene UI procedural Legacy Text e ilustración como Resources; `Screen.safeArea` aplica área usable.
- Motion/VFX/audio permanecen livianos y procedural para assets independientes menores; costes están controlados por 18 atlas a resolución móvil, sprites cachés y 12 thumbnails a 512.
