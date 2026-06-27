from pathlib import Path
import numpy as np
from PIL import Image

GRAY = (150, 150, 150)


def main() -> None:
    base    = Path(__file__).resolve().parents[2]
    src_dir = base / "Assets/Sprites/Hurt"
    dst_dir = base / "Assets/Sprites/Death"

    if not src_dir.exists():
        print(f"[skip] source folder not found: {src_dir}")
        return

    pngs = sorted(src_dir.glob("*.png"))
    if not pngs:
        print(f"[skip] no PNG files in {src_dir}")
        return

    src_file = pngs[0]
    dst_dir.mkdir(parents=True, exist_ok=True)
    dst_file = dst_dir / src_file.name

    img  = Image.open(src_file).convert("RGBA")
    data = np.array(img, dtype=np.uint8)

    mask = data[..., 3] > 0  # alpha > 0
    data[mask, 0] = GRAY[0]
    data[mask, 1] = GRAY[1]
    data[mask, 2] = GRAY[2]

    Image.fromarray(data, "RGBA").save(dst_file)
    print(f"[ok] {src_file.relative_to(base)} -> {dst_file.relative_to(base)}")


if __name__ == "__main__":
    main()
