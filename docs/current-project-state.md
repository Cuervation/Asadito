# ASADITO — current operational state

**Checked:** 2026-09-30. Canonical quick reference for active behavior; verify code/data when changing it. Dated QA detail: [`docs/ai/current-state.md`](ai/current-state.md), [`docs/ai/mvp-status.md`](ai/mvp-status.md).

- **Scope:** 12 progressive levels, 18 data-driven foods, ten runtime visual cooking stages each (six authored atlas rows plus four intermediate blends). L1 is the tutorial, not the content limit.
- **Loop:** one portrait scene; runtime UI in `Assets/Asado/Scripts/AsaditoGame.cs`. Grill starts uniformly hot (210 °C); no ignition, charcoal, fuel or movable embers.
- **Input:** raw food starts on a generated aluminum tray, automatically packed by visible footprint; overflow stacks only when needed. Invisible mobile touch targets may overlap and resolve to the nearest piece. Drag each cut to the grill; the tray remains until emptied, then vanishes and the serving board appears in the exact same 380×253.3 rect, center, and 3:2 aspect. Food stays at the same unscaled size on raw tray, grill, and board; the board packs full-size pieces and stacks only overflow. While loading, placed grill pieces can be repositioned and wait without heating; after the source is empty, cook one active piece at a time. Tap grilled food to select (no flipping for now) and drag to board; single-tap the full board to serve. Tongs are removed from the active HUD.
- **Core data:** `FoodCatalog.json`/`FoodCatalog`, `FoodCookingModel`/`FoodState`, `GrillHeatModel` and `MvpLevelCatalog`. See [`repo-map.md`](repo-map.md).
- **Out of scope:** economy/shop, restaurant management, multiplayer, walking NPCs, other appliances and charcoal management.
- **Latest documented QA:** validator PASS (18 foods / 108 states / 12 levels); Unity EditMode **23/23 PASS**; focused PlayMode surface state and direct-touch/reposition **1/1 PASS each**; normal PlayMode progression L1–L12 **1/1 PASS** (388.47 s). Current-turn test XML and details: [`docs/ai/current-state.md`](ai/current-state.md). No APK or device screenshot was produced, so visual QA on a phone remains pending. Scoped `git diff --check` is recorded for this change; unrelated modified `.png.meta` files previously caused the repository-wide check to report trailing whitespace.

Update this page when live behavior changes. Detailed rules stay in the one relevant spec; test/build evidence stays dated in `docs/ai/`.

- **Current change (2026-09-30):** ten cooking stages, single-sided input; previous QA numbers above predate this change. Removed the inactive-face quality penalty. Isolated Roslyn compilation passed for runtime, gameplay and both test assemblies; scoped whitespace check passed. Unity refresh was blocked by an existing `tests_running` job; EditMode/PlayMode execution and device verification remain pending.

- **Level selector (2026-09-30):** locked cards remain visible but non-interactable, with dimmed artwork/label, dark overlay and centered padlock. Unlocking restores normal visuals and removes the padlock. Focused selector/visual PlayMode test **1/1 PASS** (`/tmp/asadito-lock-playmode.xml`); no new APK built for this UI change.

- **Intro de nivel (2026-09-30):** la tarjeta con “Nivel N · título”, pedido e “IR A LA PARRILLA” vive ahora en popup centrado; el selector queda detrás con oscurecimiento. PlayMode focal verifica contenedor, jerarquía del título/CTA y el flujo a gameplay: **1/1 PASS** (`/tmp/asadito-intro-popup-visual.xml`). Cambio local, APK no actualizada.

- **Despliegue popup intro (2026-09-30):** build Android ARM64 validada `Succeeded`, APK `/tmp/Asadito-popup-20260930.apk`; instalada con `Success` en Motorola Edge 60 Fusion `ZY22MBNWRB` y AVD Pixel `emulator-5554`, Unity activity/PID activa en ambos. La prueba focal que verifica la tarjeta, el título/CTA y flujo de inicio pasó 1/1. Build no firmada para distribución.

- **Instalación teléfono (2026-09-30):** popup blanco compacto `Asadito-popup-white-20260930.apk` reinstalado con ADB `Success` en el Motorola Edge 60 Fusion `ZY22MBNWRB`; Unity foreground verificado, PID 15752, package 1.2.0/code3.

- **Refinamiento popup (2026-09-30):** ancho reducido otro 10% a 774 UI, altura 1000; fondo blanco al 70% de opacidad (30% transparente). Título/pedido/objetivo/CTA mantienen alineación central con anchos dentro de tarjeta, fila comensales icono+texto despejada y CTA reducido a 500×108. Test focal 1/1 PASS (`/tmp/asadito-popup-30transparent.xml`). En ese momento el ajuste aún era local; la APK instalada se actualizó en el siguiente paso.

- **Estilo popup 8 — vidrio esmerilado (2026-09-30):** se agregó un borde claro, reflejo translúcido y acento cálido sobre la tarjeta blanca existente. Se conserva el botón dorado arcade de “IR A LA PARRILLA” para respetar el lenguaje visual actual; no se agregó desenfoque real del fondo. Prueba PlayMode focal 1/1 PASS (`/tmp/asadito-glass-popup-test.xml`); cambio local, sin rebuild ni instalación en el teléfono.

- **APK popup vidrio en teléfono (2026-09-30):** build Android ARM64 Unity 6000.6.3f1 `Succeeded`, `/tmp/Asadito-frosted-popup-20260930.apk` (60 MiB); `aapt` confirmó `com.cuervation.asadito` 1.2.0/code3, min API 26/target 36, `apksigner` validó esquema v2. Instalación `Success` y Unity foreground/PID 6648 en Motorola Edge 60 Fusion `ZY22MBNWRB`; screenshot del selector y popup en `/tmp/asadito-frosted-popup-motorola.png`. Prueba focal 1/1 PASS; build de validación no firmada para distribución.
