from PIL import Image
import os

# Keeping this script if anyone needs it, it's not integral to the project.

# This is a script that you can use to fix the FFDec image exporting bug, where
# it always adds a 1px border to the right and bottom of the image.
# Make sure to install Pillow library and replace the output and input directories (it will)
# fetch all the pngs inside it and crop them, not changing the original

# Directory with the PNG files that you exported with FFDEC
INPUT_DIR = ""

# Directory to save the files
OUTPUT_DIR = ""

def crop_image(input_path, output_path):
    image = Image.open(input_path)
    original_width, original_height = image.size
    cropped_image = image.crop((0, 0, original_width - 1, original_height - 1))
    cropped_image.save(output_path)
    print("Cropped image: " + input_path)

def get_png_files(directory):
    all_files = os.listdir(directory)
    png_files = [file for file in all_files if file.lower().endswith(".png")]
    return png_files

def crop_all_pngs(input_dir, output_dir):
    all_images = get_png_files(input_dir)
    for image in all_images:
        input_path = os.path.join(input_dir, image)
        output_path = os.path.join(output_dir, image)
        crop_image(input_path, output_path)

if __name__ == "__main__":
    crop_all_pngs(INPUT_DIR, OUTPUT_DIR)
