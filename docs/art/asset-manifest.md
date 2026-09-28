# Asset Manifest

| Asset | Estado | Uso |
|---|---|---|
| `Assets/Asado/Resources/Art/PatioParrilla.png` | PROVISIONAL | Fondo 2D temporal; reemplazar/adaptar para presentación 3D |
| `Assets/Asado/Resources/Art/PortadaAsadito.png` | PROVISIONAL | Key art vertical original con parrilla/patio, sin texto horneado; portada runtime la usa como fondo; falta QA final/import tras el último reemplazo |
| `Assets/Asado/Resources/Art/AsaditoAppIcon.png` | PROVISIONAL | Propuesta de icono original sin texto; `ProjectSettings.asset` aún no registra iconos de plataforma ni se validó en build/dispositivo |
| `Assets/Asado/Resources/Art/BifeCrudo.png` | PROVISIONAL | Referencia visual previa, fuera del First Playable actual |
| `Assets/Asado/Resources/Art/TiraAsadoCruda.png` | PROVISIONAL | Sprite 2D temporal; revisar al migrar la presentación a 3D |
| `Assets/Asado/Resources/Art/ChorizoCrudoCutout.png` | PROVISIONAL | Sprite generado para el First Playable; el tinte acompaña la cocción, reemplazar al cerrar Visual Slice |
| `Assets/Asado/Resources/Fonts/LilitaOne-Regular.ttf` | FINAL | Fuente display para marca/títulos en Canvas legacy; recursos Unity y tildes/números revisados visualmente |
| `Assets/Asado/Resources/Fonts/Baloo2-{Regular,Medium,SemiBold,Bold,ExtraBold}.ttf` | FINAL | Familia UI de cinco pesos estáticos para Canvas legacy; cargas y texto español revisados en Editor |
| Retrato, plato y bandeja UI | PROVISIONAL | Formas de Canvas temporales; retratos no tienen ilustración de personaje todavía |
| UI runtime de portada/gameplay | PROVISIONAL | `ENTRAR`/`SALIR`, título Lilita One con contorno/sombra, botones redondeados, transición y HUD mobile-first; captura portrait del Editor comprobó legibilidad/posición, faltan device QA, tutorial de todas las acciones y polish visual |
| Parrilla/brasas 3D, pinza, madera y patio | TODO | Assets runtime compatibles con la dirección 3D móvil |
| Tira, chorizo, vacío y provoleta 3D con estados | TODO | Modelos/materiales por alimento y caras |
| UI de menú, tutorial, niveles, estrellas y resultado | TODO | UI vertical con safe areas |
| `docs/art/concepts/gameplay-key-art.png` | CONCEPT | Key art vertical 3D de gameplay en patio/quincho argentino |
| `docs/art/concepts/fire-states.png` | CONCEPT | Hoja de 6 estados del carbón encendido |
| `docs/art/concepts/chorizo-cooking-states.png` | CONCEPT | Hoja de 6 estados de cocción del chorizo |
| `docs/art/concepts/tira-cooking-states.png` | CONCEPT | Hoja de 6 estados de cocción de tira de asado |
| `docs/art/concepts/result-screen.png` | CONCEPT | Concepto de pantalla de resultado con comensales, hambre, punto, preferencias y score |
| `docs/art/concepts/grill-progression.png` | CONCEPT | Progresión de parrillas, con variantes futuras solo de referencia |

Los únicos estados permitidos son `FINAL`, `PROVISIONAL`, `CONCEPT` y `TODO`. CONCEPT no es un asset de runtime.

## Tipografía — fuentes y licencias

- **Lilita One Regular** (`LilitaOne-Regular.ttf`): fuente original de [Google Fonts, `ofl/lilitaone`](https://github.com/google/fonts/tree/23e54b51ddffbc7713c583748e3bd86f62b1fa4a/ofl/lilitaone). Licencia OFL 1.1 incluida en `Assets/Asado/Resources/Fonts/OFL-LilitaOne.txt`.
- **Baloo 2 Regular/Medium/SemiBold/Bold/ExtraBold**: instancias estáticas (pesos 400/500/600/700/800) derivadas de la fuente variable oficial `ofl/baloo2/Baloo2[wght].ttf` en [el mismo commit de Google Fonts](https://github.com/google/fonts/tree/23e54b51ddffbc7713c583748e3bd86f62b1fa4a/ofl/baloo2), con fontTools 4.66.0; licenciado bajo OFL 1.1, texto incluido en `Assets/Asado/Resources/Fonts/OFL-Baloo2.txt`.
- Se conservan únicamente los TTF requeridos en `Assets/Asado/Resources/Fonts/` para carga por `Resources.Load<Font>`; no se generan assets SDF de TextMeshPro porque la UI actual usa `UnityEngine.UI.Text`. Estado PROVISIONAL hasta importación y revisión tipográfica dentro de Unity.
