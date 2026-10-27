#!/usr/bin/env python3
"""Deterministically generate the original visual and audio assets for EnergoKey.

The script uses only Pillow, NumPy and Python's standard library.  It does not
download or embed third-party artwork, and it deliberately does not generate
anything that can be mistaken for a Unity Editor or gameplay screenshot.
"""

from __future__ import annotations

import argparse
import hashlib
import math
import wave
from pathlib import Path
from typing import Callable, Iterable, Sequence

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont


SEED = 0xE11E_6E7
SAMPLE_RATE = 44_100
SUPER_SAMPLE = 4

INK = (229, 248, 255, 255)
MUTED = (142, 174, 193, 255)
NAVY = (5, 14, 27, 255)
NAVY_2 = (9, 27, 44, 255)
PANEL = (13, 31, 48, 242)
PANEL_EDGE = (47, 78, 101, 255)
CYAN = (67, 217, 255, 255)
CYAN_LIGHT = (166, 244, 255, 255)
CYAN_DARK = (12, 94, 126, 255)
GOLD = (255, 216, 77, 255)
GOLD_LIGHT = (255, 246, 171, 255)
ORANGE = (255, 153, 44, 255)
RED = (255, 76, 91, 255)
RED_DARK = (132, 24, 43, 255)
GREEN = (73, 227, 143, 255)
GREEN_LIGHT = (181, 255, 215, 255)
GREEN_DARK = (20, 111, 75, 255)

FONT_REGULAR_CANDIDATES = (
    "/System/Library/Fonts/Supplemental/Arial.ttf",
    "/System/Library/Fonts/Supplemental/Arial Unicode.ttf",
    "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
)
FONT_BOLD_CANDIDATES = (
    "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
    "/System/Library/Fonts/Supplemental/Arial Unicode.ttf",
    "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
)


def find_font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = FONT_BOLD_CANDIDATES if bold else FONT_REGULAR_CANDIDATES
    for candidate in candidates:
        path = Path(candidate)
        if path.is_file():
            return ImageFont.truetype(str(path), size=size)
    return ImageFont.load_default()


def rgba_gradient(
    size: tuple[int, int],
    start: Sequence[int],
    end: Sequence[int],
    *,
    horizontal: bool = False,
) -> Image.Image:
    width, height = size
    count = width if horizontal else height
    t = np.linspace(0.0, 1.0, count, dtype=np.float32)
    start_array = np.array(start, dtype=np.float32)
    end_array = np.array(end, dtype=np.float32)
    line = start_array[None, :] * (1.0 - t[:, None]) + end_array[None, :] * t[:, None]
    if horizontal:
        array = np.repeat(line[None, :, :], height, axis=0)
    else:
        array = np.repeat(line[:, None, :], width, axis=1)
    return Image.fromarray(np.clip(array, 0, 255).astype(np.uint8), "RGBA")


def add_radial_light(
    image: Image.Image,
    center: tuple[float, float],
    radius: float,
    color: Sequence[int],
    strength: float,
) -> None:
    width, height = image.size
    yy, xx = np.ogrid[:height, :width]
    distance = np.sqrt((xx - center[0]) ** 2 + (yy - center[1]) ** 2)
    alpha = np.clip(1.0 - distance / radius, 0.0, 1.0) ** 2
    alpha *= strength
    overlay = np.zeros((height, width, 4), dtype=np.uint8)
    overlay[..., :3] = np.array(color[:3], dtype=np.uint8)
    overlay[..., 3] = np.clip(alpha * 255, 0, 255).astype(np.uint8)
    image.alpha_composite(Image.fromarray(overlay, "RGBA"))


def add_vignette(image: Image.Image, strength: float = 0.55) -> None:
    width, height = image.size
    yy, xx = np.ogrid[:height, :width]
    nx = (xx - width / 2.0) / (width / 2.0)
    ny = (yy - height / 2.0) / (height / 2.0)
    distance = np.sqrt(nx * nx + ny * ny)
    alpha = np.clip((distance - 0.3) / 0.8, 0.0, 1.0) ** 1.8
    overlay = np.zeros((height, width, 4), dtype=np.uint8)
    overlay[..., 3] = np.clip(alpha * strength * 255, 0, 255).astype(np.uint8)
    image.alpha_composite(Image.fromarray(overlay, "RGBA"))


def save_png(image: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path, format="PNG", optimize=False, compress_level=9)


def downsample(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    return image.resize(size, Image.Resampling.LANCZOS)


def clear_faint_alpha(image: Image.Image, threshold: int = 8) -> Image.Image:
    """Make near-invisible antialiasing tails fully transparent."""
    if "A" not in image.getbands():
        return image
    alpha = image.getchannel("A").point(lambda value: 0 if value < threshold else value)
    image.putalpha(alpha)
    return image


def scaled_points(points: Iterable[tuple[float, float]], scale: float) -> list[tuple[int, int]]:
    return [(round(x * scale), round(y * scale)) for x, y in points]


def glow_shape(
    image: Image.Image,
    painter: Callable[[ImageDraw.ImageDraw], None],
    color: tuple[int, int, int, int],
    radius: float,
) -> None:
    glow = Image.new("RGBA", image.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(glow)
    painter(draw)
    glow = glow.filter(ImageFilter.GaussianBlur(radius=radius))
    tint = Image.new("RGBA", image.size, color)
    tint.putalpha(glow.getchannel("A"))
    image.alpha_composite(tint)


def regular_polygon(
    center: tuple[float, float], radius: float, sides: int, start_angle: float = -math.pi / 2
) -> list[tuple[float, float]]:
    cx, cy = center
    return [
        (
            cx + math.cos(start_angle + index * math.tau / sides) * radius,
            cy + math.sin(start_angle + index * math.tau / sides) * radius,
        )
        for index in range(sides)
    ]


def star_polygon(
    center: tuple[float, float], outer: float, inner: float, points: int
) -> list[tuple[float, float]]:
    cx, cy = center
    result: list[tuple[float, float]] = []
    for index in range(points * 2):
        radius = outer if index % 2 == 0 else inner
        angle = -math.pi / 2 + index * math.pi / points
        result.append((cx + math.cos(angle) * radius, cy + math.sin(angle) * radius))
    return result


def create_background(size: tuple[int, int] = (1280, 720)) -> Image.Image:
    image = rgba_gradient(size, NAVY, (8, 29, 45, 255))
    add_radial_light(image, (260, 220), 470, CYAN, 0.24)
    add_radial_light(image, (1090, 390), 520, GREEN, 0.12)

    circuit = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(circuit)
    width, height = size
    for x in range(0, width + 1, 64):
        draw.line((x, 0, x, height), fill=(57, 119, 146, 22), width=1)
    for y in range(0, height + 1, 64):
        draw.line((0, y, width, y), fill=(57, 119, 146, 20), width=1)

    rng = np.random.default_rng(SEED)
    for index in range(28):
        x = int(rng.integers(-64, width - 100))
        y = int(rng.integers(36, height - 36))
        run = int(rng.integers(90, 260))
        rise = int(rng.choice((-64, -32, 32, 64)))
        points = [(x, y), (x + run, y), (x + run + abs(rise), y + rise)]
        alpha = 34 if index % 4 else 56
        color = (67, 217, 255, alpha) if index % 3 else (73, 227, 143, alpha)
        draw.line(points, fill=color, width=2, joint="curve")
        px, py = points[-1]
        draw.ellipse((px - 4, py - 4, px + 4, py + 4), outline=color, width=2)

    image.alpha_composite(circuit)
    add_vignette(image, 0.67)
    image.putalpha(255)
    return image


def create_player(size: tuple[int, int] = (256, 256)) -> Image.Image:
    scale = SUPER_SAMPLE
    width, height = size
    high = Image.new("RGBA", (width * scale, height * scale), (0, 0, 0, 0))
    cx, cy = high.width / 2, high.height / 2
    outer = min(high.size) * 0.39

    glow_shape(
        high,
        lambda d: d.ellipse((cx - outer, cy - outer, cx + outer, cy + outer), fill=(255, 255, 255, 210)),
        (24, 193, 255, 150),
        24 * scale,
    )
    draw = ImageDraw.Draw(high)
    hull = regular_polygon((cx, cy), outer, 12)
    draw.polygon(hull, fill=(10, 47, 66, 255), outline=CYAN_LIGHT, width=5 * scale)
    draw.ellipse(
        (cx - outer * 0.72, cy - outer * 0.72, cx + outer * 0.72, cy + outer * 0.72),
        fill=(16, 104, 135, 255),
        outline=CYAN,
        width=6 * scale,
    )
    draw.ellipse(
        (cx - outer * 0.43, cy - outer * 0.43, cx + outer * 0.43, cy + outer * 0.43),
        fill=CYAN,
    )
    draw.ellipse(
        (cx - outer * 0.25, cy - outer * 0.30, cx + outer * 0.16, cy + outer * 0.10),
        fill=(213, 253, 255, 235),
    )
    nose = [
        (cx, cy - outer * 1.08),
        (cx - outer * 0.27, cy - outer * 0.58),
        (cx + outer * 0.27, cy - outer * 0.58),
    ]
    draw.polygon(nose, fill=CYAN_LIGHT, outline=INK)
    return clear_faint_alpha(downsample(high, size))


def create_crystal(size: tuple[int, int] = (256, 256)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    width, height = high.size
    points = scaled_points(((0.50, 0.06), (0.82, 0.34), (0.67, 0.87), (0.50, 0.97), (0.33, 0.87), (0.18, 0.34)), width)
    # scaled_points above assumes a square and intentionally uses width for both axes.
    glow_shape(
        high,
        lambda d: d.polygon(points, fill=(255, 255, 255, 220)),
        (255, 195, 35, 165),
        22 * scale,
    )
    draw = ImageDraw.Draw(high)
    draw.polygon(points, fill=GOLD, outline=GOLD_LIGHT, width=4 * scale)
    top = points[0]
    left = points[1]
    right = points[-1]
    lower = points[3]
    center = (width // 2, round(height * 0.52))
    draw.polygon((top, right, center), fill=GOLD_LIGHT)
    draw.polygon((top, left, center), fill=(255, 188, 47, 255))
    draw.polygon((right, lower, center), fill=(255, 171, 37, 255))
    draw.polygon((left, lower, center), fill=(255, 132, 35, 255))
    draw.line((top, center, lower), fill=(255, 249, 190, 220), width=3 * scale)
    draw.ellipse(
        (round(width * 0.39), round(height * 0.20), round(width * 0.49), round(height * 0.32)),
        fill=(255, 255, 232, 220),
    )
    return clear_faint_alpha(downsample(high, size))


def create_hazard(size: tuple[int, int] = (256, 256)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    width, height = high.size
    center = (width / 2, height / 2)
    outer = min(high.size) * 0.45
    points = star_polygon(center, outer, outer * 0.64, 12)
    glow_shape(
        high,
        lambda d: d.polygon(points, fill=(255, 255, 255, 205)),
        (255, 50, 73, 140),
        21 * scale,
    )
    draw = ImageDraw.Draw(high)
    draw.polygon(points, fill=RED_DARK, outline=(255, 137, 144, 255), width=4 * scale)
    r = outer * 0.60
    draw.ellipse(
        (center[0] - r, center[1] - r, center[0] + r, center[1] + r),
        fill=RED,
        outline=(255, 184, 185, 255),
        width=4 * scale,
    )
    bolt = scaled_points(
        ((0.53, 0.18), (0.34, 0.53), (0.49, 0.53), (0.42, 0.82), (0.68, 0.43), (0.53, 0.43)),
        width,
    )
    draw.polygon(bolt, fill=(255, 244, 220, 255))
    return downsample(high, size)


def draw_lock(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], color: tuple[int, int, int, int]) -> None:
    x0, y0, x1, y1 = box
    width = x1 - x0
    stroke = max(2, round(width * 0.10))
    draw.arc((x0 + width * 0.20, y0, x1 - width * 0.20, y0 + width * 0.73), 180, 360, fill=color, width=stroke)
    draw.rounded_rectangle((x0, y0 + width * 0.37, x1, y1), radius=width * 0.10, fill=color)
    key_color = (20, 38, 49, 255)
    cx = (x0 + x1) / 2
    draw.ellipse((cx - width * 0.07, y0 + width * 0.55, cx + width * 0.07, y0 + width * 0.69), fill=key_color)
    draw.rectangle((cx - width * 0.025, y0 + width * 0.66, cx + width * 0.025, y0 + width * 0.81), fill=key_color)


def create_exit(opened: bool, size: tuple[int, int] = (320, 320)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    width, height = high.size
    margin = round(width * 0.10)
    outer_box = (margin, margin, width - margin, height - margin)
    active = GREEN if opened else (116, 139, 154, 255)
    glow = (52, 255, 150, 145) if opened else (255, 86, 91, 65)
    glow_shape(
        high,
        lambda d: d.rounded_rectangle(outer_box, radius=width * 0.10, outline=(255, 255, 255, 220), width=10 * scale),
        glow,
        19 * scale,
    )
    draw = ImageDraw.Draw(high)
    draw.rounded_rectangle(outer_box, radius=width * 0.10, fill=(9, 31, 43, 255), outline=active, width=8 * scale)
    inner_margin = round(width * 0.24)
    inner_box = (inner_margin, inner_margin, width - inner_margin, height - inner_margin)
    if opened:
        draw.rounded_rectangle(inner_box, radius=width * 0.07, fill=(23, 98, 74, 220), outline=GREEN_LIGHT, width=5 * scale)
        for index in range(4):
            inset = round(index * width * 0.035)
            color = (73, 227, 143, 220 - index * 42)
            draw.rounded_rectangle(
                (inner_box[0] + inset, inner_box[1] + inset, inner_box[2] - inset, inner_box[3] - inset),
                radius=width * 0.05,
                outline=color,
                width=max(scale, (5 - index) * scale),
            )
        arrow = scaled_points(((0.42, 0.39), (0.62, 0.50), (0.42, 0.61)), width)
        draw.line(arrow, fill=GREEN_LIGHT, width=8 * scale, joint="curve")
    else:
        draw.rounded_rectangle(inner_box, radius=width * 0.07, fill=(42, 52, 62, 255), outline=(151, 166, 176, 255), width=5 * scale)
        draw.line((width * 0.25, height * 0.25, width * 0.75, height * 0.75), fill=(178, 68, 76, 210), width=6 * scale)
        draw.line((width * 0.75, height * 0.25, width * 0.25, height * 0.75), fill=(178, 68, 76, 210), width=6 * scale)
        draw_lock(
            draw,
            (round(width * 0.38), round(height * 0.34), round(width * 0.62), round(height * 0.67)),
            (225, 231, 233, 255),
        )
    return downsample(high, size)


def create_ui_button(size: tuple[int, int] = (640, 192)) -> Image.Image:
    scale = SUPER_SAMPLE
    high_size = (size[0] * scale, size[1] * scale)
    high = Image.new("RGBA", high_size, (0, 0, 0, 0))
    margin = 10 * scale
    box = (margin, margin, high.width - margin, high.height - margin)
    glow_shape(
        high,
        lambda d: d.rounded_rectangle(box, radius=34 * scale, fill=(255, 255, 255, 180)),
        (36, 178, 226, 80),
        11 * scale,
    )
    fill = rgba_gradient(high_size, (25, 70, 93, 255), (15, 45, 65, 255), horizontal=True)
    mask = Image.new("L", high_size, 0)
    ImageDraw.Draw(mask).rounded_rectangle(box, radius=34 * scale, fill=255)
    high.alpha_composite(Image.composite(fill, Image.new("RGBA", high_size, (0, 0, 0, 0)), mask))
    draw = ImageDraw.Draw(high)
    draw.rounded_rectangle(box, radius=34 * scale, outline=CYAN, width=5 * scale)
    draw.line((70 * scale, 42 * scale, (size[0] - 70) * scale, 42 * scale), fill=(159, 242, 255, 120), width=2 * scale)
    return clear_faint_alpha(downsample(high, size))


def create_ui_panel(size: tuple[int, int] = (720, 440)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    margin = 12 * scale
    box = (margin, margin, high.width - margin, high.height - margin)
    glow_shape(
        high,
        lambda d: d.rounded_rectangle(box, radius=32 * scale, fill=(255, 255, 255, 145)),
        (22, 145, 188, 55),
        15 * scale,
    )
    draw = ImageDraw.Draw(high)
    draw.rounded_rectangle(box, radius=32 * scale, fill=PANEL, outline=PANEL_EDGE, width=4 * scale)
    draw.rounded_rectangle(
        (margin + 12 * scale, margin + 12 * scale, high.width - margin - 12 * scale, high.height - margin - 12 * scale),
        radius=24 * scale,
        outline=(90, 153, 181, 80),
        width=2 * scale,
    )
    draw.line((52 * scale, 58 * scale, (size[0] - 52) * scale, 58 * scale), fill=(67, 217, 255, 72), width=2 * scale)
    return clear_faint_alpha(downsample(high, size))


def create_life_icon(size: tuple[int, int] = (128, 128)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    width, height = high.size
    points = []
    for index in range(240):
        angle = math.tau * index / 240.0
        x = 16.0 * math.sin(angle) ** 3
        y = 13.0 * math.cos(angle) - 5.0 * math.cos(2 * angle) - 2.0 * math.cos(3 * angle) - math.cos(4 * angle)
        points.append((width / 2 + x * width / 42.0, height * 0.48 - y * height / 38.0))
    glow_shape(high, lambda d: d.polygon(points, fill=(255, 255, 255, 210)), (255, 70, 94, 135), 10 * scale)
    draw = ImageDraw.Draw(high)
    draw.polygon(points, fill=RED, outline=(255, 191, 197, 255), width=3 * scale)
    draw.ellipse((width * 0.32, height * 0.22, width * 0.44, height * 0.34), fill=(255, 222, 224, 205))
    return downsample(high, size)


def create_lock_icon(size: tuple[int, int] = (128, 128)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    glow_shape(
        high,
        lambda d: d.rounded_rectangle((28 * scale, 40 * scale, 100 * scale, 108 * scale), radius=10 * scale, fill=(255, 255, 255, 190)),
        (255, 190, 50, 100),
        9 * scale,
    )
    draw = ImageDraw.Draw(high)
    draw_lock(draw, (28 * scale, 22 * scale, 100 * scale, 108 * scale), GOLD)
    return downsample(high, size)


def create_pause_icon(size: tuple[int, int] = (128, 128)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    width, height = high.size
    draw = ImageDraw.Draw(high)
    draw.ellipse((8 * scale, 8 * scale, width - 8 * scale, height - 8 * scale), fill=(10, 45, 63, 245), outline=CYAN, width=4 * scale)
    draw.rounded_rectangle((38 * scale, 32 * scale, 55 * scale, 96 * scale), radius=5 * scale, fill=CYAN_LIGHT)
    draw.rounded_rectangle((73 * scale, 32 * scale, 90 * scale, 96 * scale), radius=5 * scale, fill=CYAN_LIGHT)
    return downsample(high, size)


def create_arrow_icon(direction: str, size: tuple[int, int] = (128, 128)) -> Image.Image:
    scale = SUPER_SAMPLE
    high = Image.new("RGBA", (size[0] * scale, size[1] * scale), (0, 0, 0, 0))
    width, height = high.size
    draw = ImageDraw.Draw(high)
    draw.ellipse((8 * scale, 8 * scale, width - 8 * scale, height - 8 * scale), fill=(10, 45, 63, 232), outline=(67, 217, 255, 195), width=4 * scale)
    arrow = scaled_points(((0.50, 0.20), (0.78, 0.52), (0.62, 0.52), (0.62, 0.82), (0.38, 0.82), (0.38, 0.52), (0.22, 0.52)), width)
    draw.polygon(arrow, fill=CYAN_LIGHT, outline=INK)
    rotation = {"up": 0, "left": 90, "down": 180, "right": 270}[direction]
    if rotation:
        high = high.rotate(rotation, resample=Image.Resampling.BICUBIC, expand=False)
    return downsample(high, size)


def create_arrows_overview(icons: dict[str, Image.Image], size: tuple[int, int] = (384, 384)) -> Image.Image:
    result = Image.new("RGBA", size, (0, 0, 0, 0))
    icon_size = 112
    placements = {
        "arrow_up": (136, 8),
        "arrow_left": (16, 132),
        "arrow_down": (136, 256),
        "arrow_right": (256, 132),
    }
    for name, position in placements.items():
        result.alpha_composite(icons[name].resize((icon_size, icon_size), Image.Resampling.LANCZOS), position)
    core = Image.new("RGBA", (92, 92), (0, 0, 0, 0))
    core_draw = ImageDraw.Draw(core)
    core_draw.ellipse((4, 4, 88, 88), fill=(13, 40, 57, 238), outline=(67, 217, 255, 150), width=3)
    core_draw.ellipse((34, 34, 58, 58), fill=CYAN)
    result.alpha_composite(core, (146, 146))
    return result


def rounded_mask(size: tuple[int, int], radius: int) -> Image.Image:
    mask = Image.new("L", size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, size[0] - 1, size[1] - 1), radius=radius, fill=255)
    return mask


def create_release_icon(background: Image.Image, player: Image.Image, crystal: Image.Image) -> Image.Image:
    size = (512, 512)
    crop = background.resize((910, 512), Image.Resampling.LANCZOS).crop((199, 0, 711, 512))
    crop = crop.filter(ImageFilter.GaussianBlur(1.4))
    add_radial_light(crop, (256, 280), 290, CYAN, 0.26)
    draw = ImageDraw.Draw(crop)
    draw.rounded_rectangle((15, 15, 496, 496), radius=104, outline=(113, 232, 255, 180), width=9)
    draw.line((78, 406, 204, 406, 243, 366, 435, 366), fill=(73, 227, 143, 100), width=5)
    hero = player.resize((300, 300), Image.Resampling.LANCZOS)
    crop.alpha_composite(hero, (88, 154))
    gem = crystal.resize((142, 142), Image.Resampling.LANCZOS)
    crop.alpha_composite(gem, (288, 66))
    mask = rounded_mask(size, 108)
    output = Image.new("RGBA", size, (0, 0, 0, 0))
    output.paste(crop, (0, 0), mask)
    return output


def fit_text(draw: ImageDraw.ImageDraw, text: str, max_width: int, start_size: int, *, bold: bool = True) -> ImageFont.ImageFont:
    size = start_size
    while size >= 10:
        font = find_font(size, bold=bold)
        bounds = draw.textbbox((0, 0), text, font=font, stroke_width=0)
        if bounds[2] - bounds[0] <= max_width:
            return font
        size -= 2
    return find_font(10, bold=bold)


def create_promo(
    background: Image.Image,
    player: Image.Image,
    crystal: Image.Image,
    hazard: Image.Image,
    exit_open: Image.Image,
) -> Image.Image:
    size = (1280, 720)
    image = background.resize(size, Image.Resampling.LANCZOS).copy()
    add_radial_light(image, (640, 410), 520, CYAN, 0.12)
    draw = ImageDraw.Draw(image)

    draw.rounded_rectangle((76, 64, 374, 112), radius=24, fill=(10, 34, 51, 225), outline=(78, 181, 213, 180), width=2)
    badge_font = find_font(20, bold=True)
    draw.text((225, 88), "2D TOP-DOWN ARCADE", font=badge_font, fill=CYAN_LIGHT, anchor="mm")

    title_font = fit_text(draw, "ЭНЕРГОКЛЮЧ", 1040, 104, bold=True)
    draw.text((640, 170), "ЭНЕРГОКЛЮЧ", font=title_font, fill=INK, anchor="mm", stroke_width=3, stroke_fill=(9, 54, 72, 255))
    subtitle_font = find_font(34, bold=False)
    draw.text((640, 243), "Собери энергию. Открой выход.", font=subtitle_font, fill=(177, 219, 233, 255), anchor="mm")

    floor = Image.new("RGBA", size, (0, 0, 0, 0))
    floor_draw = ImageDraw.Draw(floor)
    floor_draw.ellipse((172, 536, 1104, 682), fill=(0, 0, 0, 110))
    floor_draw.line((225, 590, 504, 590, 608, 488, 988, 488), fill=(67, 217, 255, 80), width=4)
    floor_draw.line((300, 635, 720, 635, 808, 548, 1050, 548), fill=(73, 227, 143, 57), width=3)
    image.alpha_composite(floor.filter(ImageFilter.GaussianBlur(4)))

    image.alpha_composite(exit_open.resize((300, 300), Image.Resampling.LANCZOS), (862, 337))
    image.alpha_composite(player.resize((332, 332), Image.Resampling.LANCZOS), (190, 325))
    image.alpha_composite(crystal.resize((190, 190), Image.Resampling.LANCZOS), (593, 328))
    image.alpha_composite(hazard.resize((150, 150), Image.Resampling.LANCZOS), (708, 506))
    image.putalpha(255)
    return image


def feature_card(
    canvas: Image.Image,
    box: tuple[int, int, int, int],
    heading: str,
    text: str,
    accent: tuple[int, int, int, int],
) -> None:
    draw = ImageDraw.Draw(canvas)
    draw.rounded_rectangle(box, radius=26, fill=(16, 35, 52, 255), outline=(58, 91, 112, 255), width=2)
    x0, y0, x1, _ = box
    draw.rounded_rectangle((x0 + 22, y0 + 22, x0 + 34, y0 + 106), radius=6, fill=accent)
    draw.text((x0 + 58, y0 + 26), heading, font=find_font(28, bold=True), fill=INK)
    draw.multiline_text((x0 + 58, y0 + 69), text, font=find_font(21), fill=MUTED, spacing=7)


def create_product_page_mockup(icon: Image.Image, promo: Image.Image) -> Image.Image:
    size = (1440, 1800)
    image = rgba_gradient(size, (5, 15, 27, 255), (10, 28, 43, 255))
    add_radial_light(image, (170, 170), 630, CYAN, 0.15)
    add_radial_light(image, (1260, 1500), 730, GREEN, 0.08)
    draw = ImageDraw.Draw(image)

    draw.rounded_rectangle((70, 54, 1370, 122), radius=26, fill=(14, 32, 47, 245), outline=(49, 78, 96, 255), width=2)
    draw.text((104, 88), "КОНЦЕПТ-СТРАНИЦА ПРОДУКТА", font=find_font(22, bold=True), fill=CYAN_LIGHT, anchor="lm")
    draw.text((1338, 88), "НЕ СКРИНШОТ ИГРЫ", font=find_font(18, bold=True), fill=(255, 185, 108, 255), anchor="rm")

    icon_small = icon.resize((212, 212), Image.Resampling.LANCZOS)
    image.alpha_composite(icon_small, (92, 164))
    draw.text((340, 188), "ЭНЕРГОКЛЮЧ", font=find_font(69, bold=True), fill=INK)
    draw.text((343, 277), "2D top-down arcade • collect-and-exit", font=find_font(27), fill=CYAN_LIGHT)
    draw.text((343, 327), "Соберите все кристаллы, избегайте опасностей\nи откройте энергетический выход.", font=find_font(26), fill=MUTED, spacing=10)
    draw.rounded_rectangle((1046, 214, 1326, 296), radius=20, fill=(30, 137, 90, 255), outline=GREEN_LIGHT, width=2)
    draw.text((1186, 255), "УЧЕБНЫЙ ПРОЕКТ", font=find_font(22, bold=True), fill=(233, 255, 243, 255), anchor="mm")

    promo_preview = promo.resize((1240, 698), Image.Resampling.LANCZOS)
    preview_mask = rounded_mask(promo_preview.size, 32)
    framed = Image.new("RGBA", promo_preview.size, (0, 0, 0, 0))
    framed.paste(promo_preview, (0, 0), preview_mask)
    image.alpha_composite(framed, (100, 438))
    draw.rounded_rectangle((99, 437, 1341, 1137), radius=34, outline=(89, 154, 179, 230), width=3)
    draw.rounded_rectangle((104, 1071, 461, 1119), radius=22, fill=(7, 20, 31, 220))
    draw.text((282, 1095), "РЕКЛАМНЫЙ КЛЮЧЕВОЙ АРТ", font=find_font(17, bold=True), fill=CYAN_LIGHT, anchor="mm")

    feature_card(image, (100, 1192, 493, 1390), "3 уровня", "Нарастающая сложность\nи открытие по прогрессу", CYAN)
    feature_card(image, (523, 1192, 916, 1390), "Особое правило", "Выход активируется только\nпосле сбора кристаллов", GOLD)
    feature_card(image, (946, 1192, 1340, 1390), "Мобильный HUD", "Экранные кнопки, жизни,\nсчёт и пауза", GREEN)

    draw.rounded_rectangle((100, 1446, 1340, 1688), radius=30, fill=(12, 29, 45, 245), outline=(53, 83, 103, 255), width=2)
    draw.text((140, 1490), "О ПРОЕКТЕ", font=find_font(28, bold=True), fill=INK)
    body = (
        "«Энергоключ» — компактная учебная аркада, где игрок управляет\n"
        "голубым энергоядром, собирает кристаллы и избегает красных опасных зон.\n"
        "Закрытый выход даёт понятную обратную связь, а открытый портал завершает уровень."
    )
    draw.multiline_text((140, 1540), body, font=find_font(25), fill=MUTED, spacing=12)
    draw.text((720, 1742), "Version 1.0.0 • Графика и звук созданы для учебного проекта", font=find_font(20), fill=(114, 155, 177, 255), anchor="mm")
    image.putalpha(255)
    return image


def envelope(length: int, attack: float, release: float) -> np.ndarray:
    attack_samples = max(1, int(SAMPLE_RATE * attack))
    release_samples = max(1, int(SAMPLE_RATE * release))
    result = np.ones(length, dtype=np.float64)
    result[:attack_samples] = np.linspace(0.0, 1.0, attack_samples, endpoint=False)
    result[-release_samples:] *= np.linspace(1.0, 0.0, release_samples, endpoint=True)
    return result


def oscillator(frequency: float | np.ndarray, duration: float, phase: float = 0.0) -> np.ndarray:
    count = int(round(duration * SAMPLE_RATE))
    if np.isscalar(frequency):
        time = np.arange(count, dtype=np.float64) / SAMPLE_RATE
        return np.sin(math.tau * float(frequency) * time + phase)
    frequency_array = np.asarray(frequency, dtype=np.float64)
    if frequency_array.size != count:
        frequency_array = np.interp(
            np.linspace(0.0, 1.0, count),
            np.linspace(0.0, 1.0, frequency_array.size),
            frequency_array,
        )
    phase_array = math.tau * np.cumsum(frequency_array) / SAMPLE_RATE + phase
    return np.sin(phase_array)


def tone(frequency: float, duration: float, amplitude: float = 1.0) -> np.ndarray:
    signal = oscillator(frequency, duration)
    signal += 0.22 * oscillator(frequency * 2.0, duration, phase=0.12)
    return signal * envelope(signal.size, 0.012, min(0.09, duration * 0.38)) * amplitude


def place_note(buffer: np.ndarray, note: np.ndarray, start_seconds: float) -> None:
    start = int(round(start_seconds * SAMPLE_RATE))
    end = min(buffer.size, start + note.size)
    if end > start:
        buffer[start:end] += note[: end - start]


def create_sound(name: str) -> np.ndarray:
    rng = np.random.default_rng(SEED + sum(ord(character) for character in name))
    if name == "collect":
        duration = 0.34
        count = int(duration * SAMPLE_RATE)
        sweep = np.linspace(620.0, 1450.0, count)
        signal = 0.72 * oscillator(sweep, duration) + 0.25 * oscillator(sweep * 1.5, duration, phase=0.4)
        signal *= envelope(count, 0.006, 0.12)
    elif name == "error":
        duration = 0.46
        count = int(duration * SAMPLE_RATE)
        sweep = np.linspace(260.0, 145.0, count)
        base = oscillator(sweep, duration)
        grit = np.tanh(base * 2.8)
        tremolo = 0.72 + 0.28 * oscillator(11.0, duration)
        signal = grit * tremolo * envelope(count, 0.005, 0.10)
    elif name == "damage":
        duration = 0.52
        count = int(duration * SAMPLE_RATE)
        time = np.arange(count, dtype=np.float64) / SAMPLE_RATE
        low = oscillator(np.linspace(150.0, 62.0, count), duration)
        noise = rng.normal(0.0, 1.0, count)
        decay = np.exp(-time * 9.2)
        signal = (0.82 * low + 0.34 * noise) * decay * envelope(count, 0.002, 0.08)
    elif name == "victory":
        duration = 1.48
        signal = np.zeros(int(duration * SAMPLE_RATE), dtype=np.float64)
        for start, frequency in ((0.00, 523.25), (0.22, 659.25), (0.44, 783.99), (0.68, 1046.50)):
            place_note(signal, tone(frequency, 0.44, 0.78), start)
        for frequency in (523.25, 659.25, 783.99):
            place_note(signal, tone(frequency, 0.66, 0.30), 0.80)
    elif name == "defeat":
        duration = 1.32
        signal = np.zeros(int(duration * SAMPLE_RATE), dtype=np.float64)
        for start, frequency in ((0.00, 392.00), (0.27, 329.63), (0.54, 261.63), (0.82, 196.00)):
            place_note(signal, tone(frequency, 0.47, 0.72), start)
        count = signal.size
        signal += 0.10 * rng.normal(0.0, 1.0, count) * np.exp(-np.arange(count) / SAMPLE_RATE * 2.4)
        signal *= envelope(count, 0.008, 0.18)
    elif name == "button":
        duration = 0.14
        count = int(duration * SAMPLE_RATE)
        click = oscillator(np.linspace(520.0, 760.0, count), duration)
        signal = click * np.exp(-np.arange(count) / SAMPLE_RATE * 22.0) * envelope(count, 0.001, 0.03)
    else:
        raise ValueError(f"Unknown sound: {name}")

    peak = float(np.max(np.abs(signal)))
    if peak > 0:
        signal = signal / peak * 0.86
    return np.clip(signal, -1.0, 1.0)


def save_wav(signal: np.ndarray, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    pcm = np.round(signal * 32767.0).astype("<i2")
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(pcm.tobytes())


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def generate(root: Path) -> list[Path]:
    art_dir = root / "Assets" / "Resources" / "Art" / "Final"
    audio_dir = root / "Assets" / "Resources" / "Audio"
    release_dir = root / "Release"
    art_dir.mkdir(parents=True, exist_ok=True)
    audio_dir.mkdir(parents=True, exist_ok=True)
    release_dir.mkdir(parents=True, exist_ok=True)

    assets: dict[str, Image.Image] = {
        "background": create_background(),
        "player": create_player(),
        "crystal": create_crystal(),
        "hazard": create_hazard(),
        "exit_closed": create_exit(False),
        "exit_open": create_exit(True),
        "ui_button": create_ui_button(),
        "ui_panel": create_ui_panel(),
        "life": create_life_icon(),
        "lock": create_lock_icon(),
        "pause": create_pause_icon(),
    }
    for direction in ("up", "down", "left", "right"):
        assets[f"arrow_{direction}"] = create_arrow_icon(direction)
    assets["arrows"] = create_arrows_overview(assets)

    outputs: list[Path] = []
    for name, image in assets.items():
        output = art_dir / f"{name}.png"
        save_png(image, output)
        outputs.append(output)

    release_icon = create_release_icon(assets["background"], assets["player"], assets["crystal"])
    icon_path = release_dir / "icon_512.png"
    save_png(release_icon, icon_path)
    outputs.append(icon_path)

    promo = create_promo(
        assets["background"],
        assets["player"],
        assets["crystal"],
        assets["hazard"],
        assets["exit_open"],
    )
    promo_path = release_dir / "promo_1280x720.png"
    save_png(promo, promo_path)
    outputs.append(promo_path)

    product_page = create_product_page_mockup(release_icon, promo)
    product_path = release_dir / "product_page_mockup.png"
    save_png(product_page, product_path)
    outputs.append(product_path)

    for sound_name in ("collect", "error", "damage", "victory", "defeat", "button"):
        sound_path = audio_dir / f"{sound_name}.wav"
        save_wav(create_sound(sound_name), sound_path)
        outputs.append(sound_path)

    return outputs


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--root",
        type=Path,
        default=Path(__file__).resolve().parents[1],
        help="EnergoKey project root (defaults to the parent of Tools).",
    )
    args = parser.parse_args()
    root = args.root.expanduser().resolve()
    outputs = generate(root)
    print(f"Generated {len(outputs)} original assets under: {root}")
    for path in outputs:
        print(f"{path.relative_to(root)}\t{sha256(path)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
