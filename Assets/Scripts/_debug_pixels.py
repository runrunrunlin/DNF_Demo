from pathlib import Path
import numpy as np
from PIL import Image

base = Path(__file__).resolve().parents[2]
path = base / "Assets/Sprites/Attack/sprite-no-bg-256x256-8f-sheet.png"

img  = Image.open(path).convert("RGBA")
data = np.array(img, dtype=np.uint8)

r, g, b, a = data[..., 0], data[..., 1], data[..., 2], data[..., 3]

visible = a > 0
print(f"总可见像素: {visible.sum()}")

for lo in [50, 80, 100, 120, 150]:
    mask = (r > lo) & (g > lo) & (b > lo) & (a > 0)
    print(f"R/G/B > {lo}: {mask.sum()} 像素")
