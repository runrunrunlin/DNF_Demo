"""
recolor_hurt_sprites.py

把 Assets/Sprites/Hurt/ 里所有 PNG 的黑色像素（R、G、B 均 < 50）
替换成红色（255, 0, 0），透明度保持不变，覆盖原文件。
"""

from PIL import Image
import os
import numpy as np

HURT_DIR = r"C:\Users\Run\Documents\GitHub\DNF_Demo\Assets\Sprites\Hurt"
BLACK_THRESHOLD = 50  # R、G、B 三通道都低于此值才视为黑色

def recolor_png(path: str):
    img = Image.open(path).convert("RGBA")
    data = np.array(img, dtype=np.uint8)

    r, g, b, a = data[:, :, 0], data[:, :, 1], data[:, :, 2], data[:, :, 3]

    # 黑色像素：三通道都低于阈值，且不完全透明
    mask = (r < BLACK_THRESHOLD) & (g < BLACK_THRESHOLD) & (b < BLACK_THRESHOLD) & (a > 0)

    data[mask, 0] = 255  # R
    data[mask, 1] = 0    # G
    data[mask, 2] = 0    # B
    # alpha 保持不变

    Image.fromarray(data, "RGBA").save(path, "PNG")
    return mask.sum()

def main():
    if not os.path.isdir(HURT_DIR):
        print(f"[错误] 文件夹不存在: {HURT_DIR}")
        return

    pngs = [f for f in os.listdir(HURT_DIR) if f.lower().endswith(".png")]
    if not pngs:
        print(f"[跳过] {HURT_DIR} 里没有 PNG 文件")
        return

    for filename in pngs:
        path = os.path.join(HURT_DIR, filename)
        count = recolor_png(path)
        print(f"[处理] {filename}  →  {count} 个像素已改为红色")

    print(f"\n完成，共处理 {len(pngs)} 个文件（已覆盖原文件）。")

if __name__ == "__main__":
    main()
