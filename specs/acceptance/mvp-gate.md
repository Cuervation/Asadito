# Gate de aceptación — MVP expandido

## First Playable y contenido

- Menú → nivel → intro → parrilla con HeatGrid 8×6, brasas y calor espacial; arrastrar/posicionar/voltear comida, estado independiente por cara, retirar a bandeja, servir, evaluar, guardar, retry/next.
- 18 IDs únicos con perfil térmico configurable y completo, 6 representaciones visuales por alimento (108 sprite recortados de 18 atlas), sin recurso obligatorio faltante; perfiles son tuning de gameplay y no reglas sanitarias.
- 12 niveles progresivos enseñan zonas/puntos/cortes/achuras/cerdo/ave/premium y asado combinado; unlock y migración de progreso preexistente.
- 6 comensales diferenciados en edad/peso/apetito/preferencias/punto; género fuera de fórmulas. Asignador no debe saltear invitados por repetición antes de asignar primera porción a todos.
- Scoring 0–100 y ponderado (40/30/20/10) con food preference 0/40/70/100, favorito máximo.
- Safe area aplicado al Canvas; `gameplayRoot` deactivado sin apagar overlays/menú.

## Automatización y Android

- `Tools/validate_food_content.py`, EditMode y PlayMode deben pasar, diff-check limpio; todos los perfiles llegan a las etapas clave, flip/tray/serve/score/progresión tienen regresión.
- Unity build Android ARM64 + instalación/arranque del APK en emulador; orientación vertical, package/version/target/min SDK revisados. Publicación/signing de release está fuera de scope.
- Physical-phone notch/gestures, touch, thermal/performance, artwork approval, duration and audio mix son gates humanos aún no completados.

## Feedback/runtime

Implementaciones runtime básicas/procedurales de interacción, fire/ember/smoke, guest reactions, results score/stars y eight synthesized cues exist. Asset concepts alone are not completion; `docs/art/*manifest` reporta integración/provisionalidad.
