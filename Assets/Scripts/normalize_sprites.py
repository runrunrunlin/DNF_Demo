"""
normalize_sprites.py

将 sprite sheet 中各帧角色的高度统一，输出到新文件夹，不覆盖原图。

流程：
  1. 按配置切割每张 sprite sheet 为独立帧
  2. 检测每帧非透明像素的边界框（角色实际占用区域）
  3. 取所有帧中最大的角色高度作为统一目标高度
  4. 等比缩放每帧，使角色高度 == 目标高度
  5. 将缩放后的角色居中放置在固定尺寸画布上
  6. 重新拼合为 sprite sheet 并输出
"""

from PIL import Image
import os

# ── 配置区（按需修改） ──────────────────────────────────────────────────────────

# 输入根目录（包含 Idle / Run / Attack 等子文件夹）
SPRITES_ROOT = r"C:\Users\Run\Documents\GitHub\DNF_Demo\Assets\Sprites"

# 输出根目录（不会修改原图）
OUTPUT_ROOT = r"C:\Users\Run\Documents\GitHub\DNF_Demo\Assets\Sprites_Normalized"

# 每帧输出画布尺寸（宽, 高），单位像素
FRAME_CANVAS = (256, 256)

# Sprite sheet 布局配置：{ 子文件夹名: (行数, 列数, 实际帧数) }
# 实际帧数 < 行*列 时，末尾多余格子视为空白
SHEETS = {
    "Idle":   (2, 2, 4),   # 648×1024,  2×2 grid
    "Run":    (3, 3, 9),   # 1524×1650, 3×3 grid
    "Attack": (3, 3, 8),   # 768×768,   3×3 grid，最后一格空
    # "Hurt": (?, ?, ?),   # 如需处理 Hurt 文件夹，在此添加
}

# ── 工具函数 ───────────────────────────────────────────────────────────────────

def get_content_bbox(img: Image.Image):
    """返回图像中非透明像素的边界框 (left, upper, right, lower)，全透明返回 None。"""
    alpha = img.split()[3]
    return alpha.getbbox()


def slice_sheet(sheet: Image.Image, rows: int, cols: int, total_frames: int):
    """将 sprite sheet 等分切割为独立帧列表。"""
    sw, sh = sheet.size
    fw, fh = sw // cols, sh // rows
    frames = []
    for r in range(rows):
        for c in range(cols):
            if len(frames) >= total_frames:
                break
            box = (c * fw, r * fh, (c + 1) * fw, (r + 1) * fh)
            frames.append(sheet.crop(box))
    return frames


def assemble_sheet(frames: list, rows: int, cols: int, frame_size: tuple) -> Image.Image:
    """将帧列表重新拼合为 sprite sheet。"""
    fw, fh = frame_size
    sheet = Image.new("RGBA", (cols * fw, rows * fh), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        r, c = divmod(i, cols)
        sheet.paste(frame, (c * fw, r * fh))
    return sheet


def normalize_frame(
    frame: Image.Image,
    target_char_height: int,
    canvas_size: tuple,
) -> Image.Image:
    """
    裁剪角色区域 → 等比缩放至目标高度 → 居中放置在 canvas_size 画布上。
    若缩放后宽度超出画布，改为按宽度缩放。
    """
    bbox = get_content_bbox(frame)
    if bbox is None:
        return Image.new("RGBA", canvas_size, (0, 0, 0, 0))

    char = frame.crop(bbox)
    cw, ch = char.size
    canvas_w, canvas_h = canvas_size

    # 按高度缩放
    scale = target_char_height / ch
    new_w = round(cw * scale)
    new_h = target_char_height

    # 宽度超出画布时改按宽度缩放
    if new_w > canvas_w:
        scale = canvas_w / cw
        new_w = canvas_w
        new_h = round(ch * scale)

    char_scaled = char.resize((max(new_w, 1), max(new_h, 1)), Image.LANCZOS)

    canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    ox = (canvas_w - new_w) // 2
    oy = (canvas_h - new_h) // 2
    canvas.paste(char_scaled, (ox, oy))
    return canvas


# ── 主流程 ─────────────────────────────────────────────────────────────────────

def main():
    # ── Pass 1：切帧，统计最大角色高度 ──
    sheet_data = {}   # folder -> (filename, frames, rows, cols)
    max_char_height = 0

    for folder, (rows, cols, total_frames) in SHEETS.items():
        sprite_dir = os.path.join(SPRITES_ROOT, folder)
        if not os.path.isdir(sprite_dir):
            print(f"[SKIP] 文件夹不存在: {sprite_dir}")
            continue

        pngs = [f for f in os.listdir(sprite_dir) if f.lower().endswith(".png")]
        if not pngs:
            print(f"[SKIP] {folder} 内没有 PNG 文件")
            continue
        if len(pngs) > 1:
            print(f"[WARN] {folder} 内有多个 PNG，使用第一个: {pngs[0]}")

        filename = pngs[0]
        sheet = Image.open(os.path.join(sprite_dir, filename)).convert("RGBA")
        frames = slice_sheet(sheet, rows, cols, total_frames)

        for frame in frames:
            bbox = get_content_bbox(frame)
            if bbox:
                char_h = bbox[3] - bbox[1]
                max_char_height = max(max_char_height, char_h)

        sheet_data[folder] = (filename, frames, rows, cols)
        print(f"[读取] {folder}/{filename}  ({sheet.width}×{sheet.height}, {len(frames)} 帧)")

    if not sheet_data:
        print("没有找到任何 sprite sheet，退出。")
        return

    # 目标高度不超过画布高度
    target_height = min(max_char_height, FRAME_CANVAS[1])
    print(f"\n所有帧中最大角色高度: {max_char_height}px")
    print(f"统一目标高度 (已限制在画布内): {target_height}px")
    print(f"输出帧画布尺寸: {FRAME_CANVAS[0]}×{FRAME_CANVAS[1]}\n")

    # ── Pass 2：归一化并输出 ──
    os.makedirs(OUTPUT_ROOT, exist_ok=True)

    for folder, (filename, frames, rows, cols) in sheet_data.items():
        normalized = [normalize_frame(f, target_height, FRAME_CANVAS) for f in frames]
        sheet_out = assemble_sheet(normalized, rows, cols, FRAME_CANVAS)

        out_dir = os.path.join(OUTPUT_ROOT, folder)
        os.makedirs(out_dir, exist_ok=True)
        out_path = os.path.join(out_dir, filename)
        sheet_out.save(out_path, "PNG")

        out_w = cols * FRAME_CANVAS[0]
        out_h = rows * FRAME_CANVAS[1]
        print(f"[输出] {folder}/{filename}  ({out_w}×{out_h}, {len(normalized)} 帧)")

    print("\n完成。原图未修改。")
    print(f"输出目录: {OUTPUT_ROOT}")


if __name__ == "__main__":
    main()
