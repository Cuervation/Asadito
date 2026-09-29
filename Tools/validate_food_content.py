#!/usr/bin/env python3
"""Static CI checks for the authored food catalog, atlas completeness, and level card art."""
import hashlib
import json
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "Assets/Asado/Resources/Definitions/FoodCatalog.json"
ATLAS_DIR = ROOT / "Assets/Asado/Resources/Art/Foods/States"
LEVEL_DIR = ROOT / "Assets/Asado/Resources/Art/LevelCards"
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


def main():
    data = json.loads(CATALOG.read_text(encoding="utf-8"))
    foods = data.get("Foods", [])
    ids = [food.get("Id", "") for food in foods]
    assert data.get("SchemaVersion") == 1, "Unsupported FoodCatalog schema"
    assert len(foods) == 18 and set(ids) == REQUIRED_IDS, f"Expected all 18 required foods, got {len(ids)}"
    assert len(set(i.casefold() for i in ids)) == len(ids), "FoodIds must be case-insensitively unique"
    signatures = set()
    atlas_hashes = set()
    for food in foods:
        food_id = food["Id"]
        profile = food.get("Profile") or {}
        bands = profile.get("DonenessBands") or []
        crop = food.get("SpriteCrop") or {}
        assert food.get("DisplayName") and food.get("Category"), f"{food_id} needs a name and category"
        assert profile.get("FoodId") == food_id, f"{food_id} profile id mismatch"
        assert len(bands) >= 5, f"{food_id} needs five doneness bands"
        assert all(b["MaximumCoreC"] >= b["MinimumCoreC"] for b in bands), f"{food_id} invalid temperature band"
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
        digest = hashlib.sha256(atlas.read_bytes()).digest()
        assert digest not in atlas_hashes, f"Duplicated food atlas: {food_id}"
        atlas_hashes.add(digest)

    cards = [LEVEL_DIR / f"Nivel{i}.png" for i in range(1, 13)]
    for card in cards:
        assert card.is_file(), f"Missing representative card: {card.name}"
        assert png_header(card)[:2] == (1536, 1024), f"Unexpected aspect/resolution for {card.name}"
    print(f"Food content OK: {len(foods)} unique profiles, {len(foods) * 6} cooking sprites, {len(cards)} level illustrations.")


if __name__ == "__main__":
    main()
