import os

def get_png_files(directory):
    all_files = os.listdir(directory)
    png_files = [file for file in all_files if file.lower().endswith(".png")]
    return png_files

def apply_to_all_in_directory(input_dir, output_dir, function):
    all_images = get_png_files(input_dir)
    for image in all_images:
        input_path = os.path.join(input_dir, image)
        output_path = os.path.join(output_dir, image)
        function(input_path, output_path)

def process_directory(path_in, path_out, function):
    script_dir = os.path.dirname(os.path.realpath(__file__))
    crop_dir = os.path.join(script_dir, "..", "image_test", path_in)
    out_dir = os.path.join(crop_dir, "..", path_out)


    os.makedirs(out_dir, exist_ok=True)
    apply_to_all_in_directory(crop_dir, out_dir, function)