from pathlib import Path
import numpy as np
from PIL import Image

FOLDERS = [
    "Assets/Sprites/Attack",
    "Assets/Sprites/Enemy_Attack",
]

WHITE_THRESHOLD = 230


def process(path: Path) -> None:
    img  = Image.open(path).convert("RGBA")
    data = np.array(img, dtype=np.uint8)

    r, g, b, a = data[..., 0], data[..., 1], data[..., 2], data[..., 3]
    mask = (r > 100) & (g > 100) & (b > 100) & (a > 0)
    data[mask, 3] = 0

    Image.fromarray(data, "RGBA").save(path)
    print(f"[ok] {path.name}  ({mask.sum()} pixels cleared)")


def main() -> None:
    base = Path(__file__).resolve().parents[2]

    for folder_rel in FOLDERS:
        folder = base / folder_rel
        if not folder.exists():
            print(f"[skip] not found: {folder}")
            continue

        pngs = sorted(folder.glob("*.png"))
        if not pngs:
            print(f"[skip] no PNG files in {folder}")
            continue

        for png in pngs:
            process(png)


if __name__ == "__main__":
    main()
