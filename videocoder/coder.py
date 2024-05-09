from PIL import Image, ImageDraw, ImageFont
import imageio
import os
import numpy as np

def generate_image_from_colors(colors, size_x, size_y):
    width = (size_x * 6) + (size_x - 1)
    height = (size_y * 6) + (size_y - 1)
    width = ((width + 15) // 16) * 16
    height = ((height + 15) // 16) * 16
    image = Image.new("RGB", (width, height), color='black')
    draw = ImageDraw.Draw(image)
    font = ImageFont.load_default()
    pixels = image.load()

    # Предварительно вычисляем координаты начала каждого квадрата
    square_coordinates = [(i * 7 + i, j * 7 + j) for i in range(size_x) for j in range(size_y)]

    for i in range(size_x):
        for j in range(size_y):
            color = tuple(map(int, colors[i * size_y + j]))
            x_start, y_start = square_coordinates[i * size_y + j]

            # Минимизируем операции во вложенных циклах
            for x_offset in range(6):
                for y_offset in range(6):
                    x, y = x_start + x_offset, y_start + y_offset
                    if x < width and y < height:
                        pixels[x, y] = color

    return image

# Читаем цвета из файла
with open(".save", "rb") as file:
    byte_array = bytearray(file.read())

# Получаем размеры в клетках
size_x = int(input("Ширина: "))
size_y = int(input("Высота: "))

# Создаем видео и добавляем кадры каждые 100 кадров
output_name = 'simulation.mp4'

if os.path.exists(output_name):
    os.remove(output_name)

with imageio.get_writer(output_name, format='mp4', mode='I', fps=24) as writer:
    frames = []
    total_images = len(byte_array) // (size_x * size_y * 3)

    for i in range(total_images):
        colors = []
        for j in range(size_x * size_y):
            offset = (i * size_x * size_y * 3) + (j * 3)
            color = byte_array[offset:offset + 3]
            colors.append(color)

        image = generate_image_from_colors(colors, size_x, size_y)
        frame = np.array(image)
        frames.append(frame)

        if (i + 1) % 100 == 0 or (i + 1) == total_images:
            print(f"Прогресс: {(i + 1) / total_images * 100:.2f}%")

            for frame in frames:
                writer.append_data(frame)

            frames = []
