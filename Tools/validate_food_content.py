#!/usr/bin/env python3
"""Static CI audit for food profiles, six-frame atlases, progression, and required art."""
import hashlib
import json
import math
import re
import struct
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "Assets/Asado/Resources/Definitions/FoodCatalog.json"
ATLAS_DIR = ROOT / "Assets/Asado/Resources/Art/Foods/States"
LEVEL_DIR = ROOT / "Assets/Asado/Resources/Art/LevelCards"
LEVEL_SOURCE = ROOT / "Assets/Asado/Scripts/Runtime/MvpLevelCatalog.cs"
REQUIRED_IDS = {
    "tira", "chorizo", "vacio", "provoleta", "entrana", "colita_cuadril", "lomo",
    "bife_ancho", "bife_angosto", "bife_chorizo", "ojo_bife", "chinchulines", "morcilla",
    "morcilla_vasca", "matambre_cerdo", "costillita_cerdo", "solomillo_cerdo", "pollo_deshuesado",
}


def png_header(path: Path):
    with path.open("rb") as stream:
        header = stream.read(29)
    if header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
        raise AssertionError(f"{path.relative_to(ROOT)} is not a valid PNG")
    width, height, bit_depth, color_type, compression, filtering, interlace = struct.unpack(">IIBBBBB", header[16:29])
    return width, height, bit_depth, color_type, interlace


def audit_atlas_frames(path: Path, width: int, height: int):
    """Decode alpha only; check every frame is visible and silhouettes stay aligned."""
    chunks = []
    with path.open("rb") as stream:
        stream.read(8)
        while True:
            size_bytes = stream.read(4)
            if not size_bytes:
                break
            size = struct.unpack(">I", size_bytes)[0]
            chunk_type = stream.read(4)
            payload = stream.read(size)
            stream.read(4)  # CRC
            if chunk_type == b"IDAT":
                chunks.append(payload)
            elif chunk_type == b"IEND":
                break

    rgba = zlib.decompress(b"".join(chunks))
    stride = width * 4
    frame_height = height // 6
    prior_alpha = bytearray(width)
    frame_counts = [0] * 6
    frame_bounds = [[width, -1, frame_height, -1] for _ in range(6)]
    # Coarse occupancy masks make silhouette continuity checks quick and deterministic.
    sample_step_x = max(1, width // 64)
    sample_step_y = max(1, frame_height // 64)
    sample_width = (width + sample_step_x - 1) // sample_step_x
    sample_height = (frame_height + sample_step_y - 1) // sample_step_y
    masks = [bytearray(sample_width * sample_height) for _ in range(6)]
    offset = 0

    def paeth(left, above, upper_left):
        estimate = left + above - upper_left
        dl, da, du = abs(estimate - left), abs(estimate - above), abs(estimate - upper_left)
        return left if dl <= da and dl <= du else above if da <= du else upper_left

    for y in range(height):
        filter_type = rgba[offset]
        offset += 1
        raw_row = rgba[offset:offset + stride]
        offset += stride
        alpha_row = bytearray(width)
        frame = y // frame_height
        in_frame_y = y - frame * frame_height
        for x in range(width):
            value = raw_row[x * 4 + 3]
            left = alpha_row[x - 1] if x else 0
            above = prior_alpha[x]
            upper_left = prior_alpha[x - 1] if x else 0
            if filter_type == 1:
                value += left
            elif filter_type == 2:
                value += above
            elif filter_type == 3:
                value += (left + above) // 2
            elif filter_type == 4:
                value += paeth(left, above, upper_left)
            elif filter_type != 0:
                raise AssertionError(f"{path.name} has unsupported PNG filter {filter_type}")
            alpha = value & 0xFF
            alpha_row[x] = alpha
            if alpha <= 12:
                continue
            frame_counts[frame] += 1
            bounds = frame_bounds[frame]
            bounds[0] = min(bounds[0], x)
            bounds[1] = max(bounds[1], x)
            bounds[2] = min(bounds[2], in_frame_y)
            bounds[3] = max(bounds[3], in_frame_y)
            if x % sample_step_x == sample_step_x // 2 and in_frame_y % sample_step_y == sample_step_y // 2:
                sx, sy = x // sample_step_x, in_frame_y // sample_step_y
                masks[frame][sy * sample_width + sx] = 1
        prior_alpha = alpha_row

    min_pixels = width * frame_height * 0.02
    for frame, count in enumerate(frame_counts):
        assert count >= min_pixels, f"{path.name} frame {frame + 1} is blank/cropped (alpha pixels={count})"
        x_min, x_max, y_min, y_max = frame_bounds[frame]
        assert x_min <= x_max and y_min <= y_max, f"{path.name} frame {frame + 1} has no visible silhouette"
        assert x_max - x_min >= width * 0.12, f"{path.name} frame {frame + 1} is unexpectedly narrow"
    for frame in range(5):
        left, right = masks[frame], masks[frame + 1]
        intersection = sum(1 for a, b in zip(left, right) if a and b)
        union = sum(1 for a, b in zip(left, right) if a or b)
        assert union and intersection / union >= 0.60, (
            f"{path.name} frames {frame + 1}→{frame + 2} change silhouette/orientation abruptly "
            f"(coarse IoU {intersection / union if union else 0:.2f})"
        )
    return frame_counts


def audit_progression(food_ids):
    source = LEVEL_SOURCE.read_text(encoding="utf-8")
    definitions = re.findall(
        r'Create\((\d+),\s*"([^"]+)",\s*new\[\]\s*\{([^}]*)\},\s*([0-9.]+)f?\)', source
    )
    assert len(definitions) == 12, f"Expected 12 explicit level definitions, found {len(definitions)}"
    seen = set()
    present_foods = set()
    for expected, (number, title, raw_foods, amount) in enumerate(definitions, 1):
        level = int(number)
        assert level == expected, f"Level progression must be contiguous; expected L{expected}, got L{level}"
        assert title.strip(), f"L{level} needs a title"
        ids = re.findall(r'"([a-z0-9_]+)"', raw_foods)
        assert ids and all(food_id in food_ids for food_id in ids), f"L{level} references missing food IDs: {ids}"
        assert len(ids) <= 6, f"L{level} exceeds the mobile MVP order size"
        assert float(amount) > 0, f"L{level} portion amount must be positive"
        present_foods.update(ids)
        seen.add(level)
    assert seen == set(range(1, 13)), "All levels 1–12 must be present exactly once"
    assert present_foods == food_ids, f"Every food must appear in progression; missing {sorted(food_ids - present_foods)}"
    return [[food_id for food_id in re.findall(r'"([a-z0-9_]+)"', raw_foods)]
            for _, (_, _, raw_foods, _) in enumerate(definitions)]


def try_pack(area, sizes, gap):
    """Deterministic fixed-orientation MaxRects equivalent to FoodFootprintLayout.TryPack."""
    free = [[-area[0] / 2 + gap / 2, -area[1] / 2 + gap / 2, area[0] - gap, area[1] - gap]]
    centers = [None] * len(sizes)
    order = sorted(range(len(sizes)), key=lambda i: (-sizes[i][0] * sizes[i][1], i))
    for index in order:
        width, height = sizes[index]
        padded = (width + gap, height + gap)
        best = None
        for slot_index, (x, y, sw, sh) in enumerate(free):
            dx, dy = sw - padded[0], sh - padded[1]
            if dx < -0.001 or dy < -0.001:
                continue
            score = (min(dx, dy), max(dx, dy), slot_index)
            if best is None or score < best[0]:
                best = (score, slot_index, (x, y, padded[0], padded[1]))
        if best is None:
            return None
        _, _, used = best
        ux, uy, uw, uh = used
        centers[index] = (ux + gap / 2 + width / 2, uy + gap / 2 + height / 2)
        next_free = []
        for x, y, sw, sh in free:
            if x >= ux + uw or x + sw <= ux or y >= uy + uh or y + sh <= uy:
                next_free.append([x, y, sw, sh])
                continue
            candidates = [
                [x, y, sw, uy - y], [x, uy + uh, sw, y + sh - (uy + uh)],
                [x, y, ux - x, sh], [ux + uw, y, x + sw - (ux + uw), sh],
            ]
            next_free.extend(rect for rect in candidates if rect[2] > 0.001 and rect[3] > 0.001)
        free = []
        for i, rect in enumerate(next_free):
            x, y, w, h = rect
            if any(i != j and other[0] <= x and other[1] <= y and other[0] + other[2] >= x + w and other[1] + other[3] >= y + h
                   for j, other in enumerate(next_free)):
                continue
            free.append(rect)
    return centers


def audit_transparent_sprite(resource_path: str, expected_size):
    path = ROOT / "Assets/Asado/Resources" / resource_path
    assert path.is_file(), f"Missing generated gameplay sprite: {resource_path}"
    width, height, bit_depth, color_type, _ = png_header(path)
    assert (width, height) == expected_size, f"Unexpected dimensions for {resource_path}: {width}x{height}"
    assert bit_depth == 8 and color_type == 6, f"{resource_path} must be 8-bit RGBA with alpha"
    meta = Path(str(path) + ".meta")
    assert meta.is_file(), f"Missing Unity import settings: {meta.relative_to(ROOT)}"
    settings = meta.read_text(encoding="utf-8")
    assert re.search(r"(?m)^  textureType: 8$", settings), f"{resource_path} must import as Sprite"
    assert re.search(r"(?m)^    enableMipMap: 0$", settings), f"{resource_path} must not generate mipmaps"
    assert re.search(r"(?m)^  spriteMode: 2$", settings), f"{resource_path} must have a full-rect Sprite subasset"


def main():
    data = json.loads(CATALOG.read_text(encoding="utf-8"))
    foods = data.get("Foods", [])
    ids = [food.get("Id", "") for food in foods]
    assert data.get("SchemaVersion") == 2, "Unsupported FoodCatalog schema"
    reference = data.get("VisualReference") or {}
    assert reference == {"FoodId": "chorizo", "LegacyRectWidth": 236.0, "LegacyRectHeight": 176.0, "LegacyDisplayScale": 0.92}, (
        "The exact legacy chorizo sizing must remain the catalog's visual reference"
    )
    assert len(foods) == 18 and set(ids) == REQUIRED_IDS, f"Expected all 18 required foods, got {len(ids)}"
    assert len(set(i.casefold() for i in ids)) == len(ids), "FoodIds must be case-insensitively unique"
    signatures = set()
    atlas_hashes = set()
    numeric_positive = (
        "CoreTransferRate", "SurfaceTransferRate", "CoolingRate", "MoistureLossRate", "MaillardRate",
        "CharRate", "FatRenderRate", "MaillardStartsAtC", "CharStartsAtC", "ThermalMass", "ThicknessCm",
        "PreferredHeatC", "StrongHeatThresholdC",
    )
    for food in foods:
        food_id = food["Id"]
        profile = food.get("Profile") or {}
        bands = profile.get("DonenessBands") or []
        crop = food.get("SpriteCrop") or {}
        area = food.get("FootprintAreaMultiplier")
        assert food.get("DisplayName") and food.get("Category"), f"{food_id} needs a name and category"
        assert isinstance(area, (int, float)) and math.isfinite(area) and area > 0, f"{food_id} needs a positive physical footprint"
        assert "DisplayScale" not in food, f"{food_id} still uses legacy per-cut DisplayScale"
        assert profile.get("FoodId") == food_id, f"{food_id} profile id mismatch"
        assert len(bands) == 5, f"{food_id} needs exactly five ordered doneness bands"
        assert [b.get("Doneness") for b in bands] == list(range(5)), f"{food_id} doneness bands must cover all five states in order"
        assert all(
            math.isfinite(b["MinimumCoreC"]) and math.isfinite(b["MaximumCoreC"])
            and b["MaximumCoreC"] >= b["MinimumCoreC"]
            for b in bands
        ), f"{food_id} invalid temperature band"
        for previous, current in zip(bands, bands[1:]):
            assert current["MinimumCoreC"] > previous["MinimumCoreC"], f"{food_id} doneness bands must progress upward"
        for key in numeric_positive:
            value = profile.get(key)
            assert isinstance(value, (int, float)) and math.isfinite(value) and value > 0, f"{food_id}.{key} must be finite and positive"
        for key in ("SplitRiskRate", "FaceBalanceWeight"):
            value = profile.get(key)
            assert isinstance(value, (int, float)) and math.isfinite(value) and 0 <= value <= 1, f"{food_id}.{key} must be between 0 and 1"
        assert isinstance(profile.get("UsesCheeseStages"), bool), f"{food_id}.UsesCheeseStages must be explicit"
        assert 0 <= crop.get("XMin", -1) < crop.get("XMax", 0) <= 1, f"{food_id} invalid horizontal sprite crop"
        assert 0 <= crop.get("YMin", -1) < crop.get("YMax", 0) <= 1, f"{food_id} invalid vertical sprite crop"
        signature = tuple(profile.get(k) for k in (
            "CoreTransferRate", "SurfaceTransferRate", "MoistureLossRate", "MaillardRate", "CharRate",
            "FatRenderRate", "ThermalMass", "ThicknessCm", "PreferredHeatC", "StrongHeatThresholdC", "SplitRiskRate"))
        assert None not in signature and signature not in signatures, f"Missing or cloned thermal profile: {food_id}"
        signatures.add(signature)
        atlas = ATLAS_DIR / f"{food_id}.png"
        assert atlas.is_file(), f"Missing cooking atlas: {food_id}"
        assert png_header(atlas) == (1024, 1536, 8, 6, 0), f"{food_id} atlas must be a transparent 1x6 RGBA PNG"
        assert len(audit_atlas_frames(atlas, 1024, 1536)) == 6, f"{food_id} must contain six visible stage frames"
        digest = hashlib.sha256(atlas.read_bytes()).digest()
        assert digest not in atlas_hashes, f"Duplicated food atlas: {food_id}"
        atlas_hashes.add(digest)

    cards = [LEVEL_DIR / f"Nivel{i}.png" for i in range(1, 13)]
    for card in cards:
        assert card.is_file(), f"Missing representative card: {card.name}"
        assert png_header(card)[:2] == (1536, 1024), f"Unexpected aspect/resolution for {card.name}"
    audit_progression(set(ids))
    assert next(food["FootprintAreaMultiplier"] for food in foods if food["Id"] == "chorizo") == 1.0
    assert next(food["FootprintAreaMultiplier"] for food in foods if food["Id"] == "vacio") >= 2.5
    assert next(food["FootprintAreaMultiplier"] for food in foods if food["Id"] == "matambre_cerdo") >= 2.5

    by_id = {food["Id"]: food for food in foods}
    atlas_width, atlas_height, *_ = png_header(ATLAS_DIR / "chorizo.png")
    reference_food = by_id[reference["FoodId"]]
    reference_aspect = ((reference_food["SpriteCrop"]["XMax"] - reference_food["SpriteCrop"]["XMin"]) * atlas_width /
                        ((reference_food["SpriteCrop"]["YMax"] - reference_food["SpriteCrop"]["YMin"]) * atlas_height / 6))
    ref_bounds = (reference["LegacyRectWidth"] * reference["LegacyDisplayScale"],
                  reference["LegacyRectHeight"] * reference["LegacyDisplayScale"])
    ref_size = (min(ref_bounds[0], ref_bounds[1] * reference_aspect), 0)
    ref_size = (ref_size[0], ref_size[0] / reference_aspect)

    def visual_size(food):
        crop = food["SpriteCrop"]
        aspect = ((crop["XMax"] - crop["XMin"]) * atlas_width /
                  ((crop["YMax"] - crop["YMin"]) * atlas_height / 6))
        area = ref_size[0] * ref_size[1] * food["FootprintAreaMultiplier"]
        width = math.sqrt(area * aspect)
        return width, area / width

    level_ids = audit_progression(set(ids))
    for level, food_ids in enumerate(level_ids, 1):
        sizes = [visual_size(by_id[food_id]) for food_id in food_ids]
        centers = try_pack((640, 900), sizes, 16)
        assert centers is not None, f"L{level} portions do not physically fit without overlap"
        for i, (cx, cy) in enumerate(centers):
            width, height = sizes[i]
            assert -320 <= cx - width / 2 and cx + width / 2 <= 320 and -450 <= cy - height / 2 and cy + height / 2 <= 450
            for j in range(i + 1, len(centers)):
                ox, oy = centers[j]
                ow, oh = sizes[j]
                assert not (abs(cx - ox) < (width + ow) / 2 + 15.9 and abs(cy - oy) < (height + oh) / 2 + 15.9), (
                    f"L{level} packed portions {i + 1} and {j + 1} overlap"
                )
    assert try_pack((640, 900), [visual_size(by_id["chorizo"])] * 10, 8) is not None, "Ten chorizos should fit"
    assert try_pack((640, 900), [visual_size(by_id["vacio"])] * 10, 8) is None, "Ten vacíos must not fit"
    required_resources = (
        "Art/AsaditoAppIcon.png", "Art/AsaditoLogo.png", "Art/PortadaAsadito.png",
        "Art/ParrillaTopDownStylized.png", "Art/ParrillaTopDownGameplay.png", "Art/GuestPortraitAtlas.png", "Fonts/LilitaOne-Regular.ttf",
        "Fonts/Baloo2-Regular.ttf", "Fonts/Baloo2-Medium.ttf", "Fonts/Baloo2-SemiBold.ttf",
        "Fonts/Baloo2-Bold.ttf", "Fonts/Baloo2-ExtraBold.ttf",
    )
    resources = ROOT / "Assets/Asado/Resources"
    for resource in required_resources:
        assert (resources / resource).is_file(), f"Missing required runtime resource: {resource}"
    audit_transparent_sprite("Art/Tools/PinzaParrilleraOpen.png", (2172, 724))
    audit_transparent_sprite("Art/Tools/PinzaParrilleraClosed.png", (2172, 724))
    audit_transparent_sprite("Art/Props/TablaAsador.png", (1536, 1024))
    audit_transparent_sprite("Art/Props/MesitaAsador.png", (1086, 1448))
    game_source = (ROOT / "Assets/Asado/Scripts/AsaditoGame.cs").read_text(encoding="utf-8")
    for runtime_connection in (
        'LoadSingleSpriteResource("Art/Tools/PinzaParrilleraOpen")',
        'LoadSingleSpriteResource("Art/Tools/PinzaParrilleraClosed")',
        'LoadSingleSpriteResource("Art/Props/TablaAsador")',
        'Resources.Load<Texture2D>("Art/Props/MesitaAsador")',
        'MakeImage("Pinza parrillera ilustrada"',
        'MakeImage("Mesita auxiliar de asador"',
        'MakeImage("Tabla de asador"',
        'AddComponent<ServingBoardTouch>()',
    ):
        assert runtime_connection in game_source, f"Generated visual is not connected to gameplay: {runtime_connection}"
    assert 'MakeButton("DAR VUELTA"' not in game_source, "Food flipping must not use a named-food action button"
    assert 'MakeButton("BANDEJA"' not in game_source, "Plating must be direct food drag to the visible board"
    assert 'MakeButton("SERVIR"' not in game_source, "Serving must be a double-tap on the physical board"
    assert 'FoodFootprintLayout.CalculateVisualSize' in game_source, "Per-food footprint sizes must be connected to the rendered portions"
    assert 'FoodFootprintLayout.TryPack' in game_source, "Initial food positions must be packed by physical footprint"
    assert 'FoodFootprintLayout.Overlaps' in game_source, "Dragging must prevent physical overlaps"
    assert 'FoodFootprintLayout.GetTouchTargetSize' in game_source, "Mobile hit targets must be derived from visible food size"
    all_atlases = {path.stem for path in ATLAS_DIR.glob("*.png")}
    assert all_atlases == REQUIRED_IDS, f"Unexpected cooking atlas set: missing={sorted(REQUIRED_IDS-all_atlases)}, extra={sorted(all_atlases-REQUIRED_IDS)}"
    print(f"Food content OK: {len(foods)} unique profiles, {len(foods) * 6} visible/coherent cooking frames, physical footprints and non-overlapping layouts for L1–L12, {len(cards)} level illustrations, runtime art/fonts present.")


if __name__ == "__main__":
    main()
