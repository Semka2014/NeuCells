from PIL import Image, ImageDraw, ImageFont
import imageio
import os
import numpy as np

def generate_image_from_colors(colors):
    num_colors = len(colors)
    size = int(num_colors ** 0.5)
    # Calculate the image size considering 1 pixel distance between squares
    width = (size * 6) + (size - 1)
    height = (size * 6) + (size - 1)
    # Round up the image size to the nearest multiple of 16
    width = ((width + 15) // 16) * 16
    height = ((height + 15) // 16) * 16
    image = Image.new("RGB", (width, height), color='black')

    draw = ImageDraw.Draw(image)
    font = ImageFont.load_default()  # You can choose a different font if you prefer
    
    pixels = image.load()
    for i in range(size):
        for j in range(size):
            color = tuple(map(int, colors[i * size + j].split("/")))
            # Calculate the starting coordinates of the current square
            x_start = i * 7 + i  # Add i to account for the 1-pixel distance between squares
            y_start = j * 7 + j  # Add j to account for the 1-pixel distance between squares
            # Fill the square with color
            for x_offset in range(6):
                for y_offset in range(6):
                    # Ensure the index is within the image bounds
                    if x_start + x_offset < width and y_start + y_offset < height:
                        pixels[x_start + x_offset, y_start + y_offset] = color

    # Add text to the image
    draw.text((5, 5), simulation_number, fill="white", font=font)

    return image

# Read colors from the file
with open("save.txt", "r") as file:
    lines = file.readlines()[1:]  # Ignore the first line

# Ask user for simulation number
simulation_number = input("Введите номер симуляции: ")

# Create a video and add frames every 100 frames
output_name = f'simulation_{simulation_number}.mp4'  # Define the name for the output video

# Delete the video file before writing new frames
if os.path.exists(output_name):
    os.remove(output_name)

with imageio.get_writer(output_name, format='mp4', mode='I', fps=24) as writer:
    frames = []  # List to store frames
    total_images = len(lines)
    for i, line in enumerate(lines):
        colors = line.strip().split("-")
        image = generate_image_from_colors(colors)
        
        # Convert PIL image to numpy array for imageio
        frame = np.array(image)
        
        # Add the frame to the list of frames
        frames.append(frame)
        
        # If the number of frames reaches 100 or it's the last frame
        if (i + 1) % 100 == 0 or (i + 1) == total_images:
            # Print progress
            print(f"Progress: {(i + 1) / total_images * 100:.2f}%")
            # Add frames to the video
            for frame in frames:
                writer.append_data(frame)
            # Clear the list of frames
            frames = []
