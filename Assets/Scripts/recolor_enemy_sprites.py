from pathlib import Path
import numpy as np
from PIL import Image

MAPPINGS = [
    ("Assets/Sprites/Idle",    "Assets/Sprites/Enemy_Idle"),
    ("Assets/Sprites/Run",     "Assets/Sprites/Enemy_Run"),
    ("Assets/Sprites/Attack",  "Assets/Sprites/Enemy_Attack"),
    ("Assets/Sprites/Attack2", "Assets/Sprites/Enemy_Attack2"),
]

PURPLE = (150, 0, 200)
BLACK_THRESHOLD = 50


def recolor(src: Path, dst: Path) -> None:
    img = Image.open(src).convert("RGBA")
    data = np.array(img, dtype=np.uint8)

    r, g, b, a = data[..., 0], data[..., 1], data[..., 2], data[..., 3]
    mask = (r < BLACK_THRESHOLD) & (g < BLACK_THRESHOLD) & (b < BLACK_THRESHOLD) & (a > 0)

    data[mask, 0] = PURPLE[0]
    data[mask, 1] = PURPLE[1]
    data[mask, 2] = PURPLE[2]

    Image.fromarray(data, "RGBA").save(dst)


def main() -> None:
    base = Path(__file__).resolve().parents[2]

    for src_rel, dst_rel in MAPPINGS:
        src_dir = base / src_rel
        dst_dir = base / dst_rel

        if not src_dir.exists():
            print(f"[skip] source folder not found: {src_dir}")
            continue

        dst_dir.mkdir(parents=True, exist_ok=True)

        for src_file in sorted(src_dir.glob("*.png")):
            dst_file = dst_dir / src_file.name
            recolor(src_file, dst_file)
            print(f"[ok] {src_file.relative_to(base)} -> {dst_file.relative_to(base)}")


if __name__ == "__main__":
    main()
