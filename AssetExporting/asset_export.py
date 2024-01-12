import os
import re
import subprocess
import shutil
from ffdec_image_fix import crop_all_pngs

from PIL import Image

# SCRIPT MEANT FOR WINDOWS ONLY

def scale_image(input_path, output_path, scale):
    original_image = Image.open(input_path)
    new_size = (int(original_image.width * scale[0]), int(original_image.height * scale[1]))
    scaled_image = original_image.resize(new_size)
    scaled_image.save(output_path)

ffdec_path = ''
asset_export_path = os.path.join(os.getcwd(), "AssetExporting")
original_asset_path = os.path.join(asset_export_path, "OriginalAssets")
game_asset_path = os.path.join(os.getcwd(), "Assets")

with open(os.path.join(asset_export_path, 'ffdec-path.txt'), 'r') as file:
    ffdec_path = file.read().strip()

assetmap_lines = []

with open(os.path.join(asset_export_path, 'assetmap.txt'), 'r') as file:
    for line in file:
        assetmap_lines.append(line.strip())

class AssetMapParser:
    temp_folder = os.path.join(asset_export_path, 'temp')
    crop_temp_folder = os.path.join(asset_export_path, 'croptemp')

    def __init__(self, lines):
        self.lines = lines
        self.index = 0
        self.length = len(lines)

    def parse(self):
        # Lines in here will always be of a format something -> something
        while self.index < self.length:
            self.parse_origin_file()
            self.next()

        shutil.rmtree(self.temp_folder)
        shutil.rmtree(self.crop_temp_folder)


    def parse_origin_file(self):
        line_match = re.search('(.*)->(.*)', self.get_current_line())
        origin = os.path.join(original_asset_path, line_match.group(1).strip())
        destination = os.path.join(game_asset_path, line_match.group(2).strip())
        self.next()
        while self.index < self.length and self.get_current_line().strip() != '':
            self.parse_export(origin, destination)

    def parse_export(self, origin, destination):
        export_type = self.get_current_line().strip()
        self.next()
        if export_type == 'shape':
            self.parse_shape(origin, destination)
        elif export_type == 'font':
            self.parse_font(origin, destination)

    def parse_shape(self, origin, destination):
        os.makedirs(self.temp_folder, exist_ok=True)
        os.chdir(ffdec_path)
        subprocess.run([
            'ffdec.bat',
            '-zoom',
            '10',
            '-format',
            'shape:png', 
            '-export',
            'shape',
            self.temp_folder,
            origin
        ])
        os.makedirs(self.crop_temp_folder, exist_ok=True)
        crop_all_pngs(self.temp_folder, self.crop_temp_folder)
        while '=' in self.get_current_line():
            self.parse_shape_line(self.crop_temp_folder, destination)

    def get_current_line(self):
        return self.lines[self.index]
    
    def parse_shape_line(self, origin, destination):
        line_match = re.search('(.*)=(.*)', self.get_current_line())
        destination_str = line_match.group(2).strip()
        destination_filename = destination_str
        scale = [1, 1]
        if '[' in destination_str:
            destination_filename = destination_str[:destination_str.index('[')]
            scale_uneval = destination_str[destination_str.index('[') + 1:destination_str.index(']')].split(',')
            scale = [eval(x) for x in scale_uneval]

        origin_file = os.path.join(origin, line_match.group(1).strip() + '.png')
        destination_file = os.path.join(destination, destination_filename + '.png')
        scale_image(origin_file, destination_file, scale)
        self.next()

    def parse_font(self, origin, destination):
        os.makedirs(self.temp_folder, exist_ok=True)
        os.chdir(ffdec_path)
        subprocess.run([
            'ffdec.bat',
            '-export',
            'font',
            self.temp_folder,
            origin
        ])
        while any(char.isdigit() for char in self.get_current_line()):
            self.parse_font_line(self.temp_folder, destination)
    
    def parse_font_line(self, origin, destination):
        font_number = int(self.get_current_line().strip())
        for filename in os.listdir(origin):
            if filename.startswith(str(font_number)) and filename.endswith('.ttf'):
                new_filename = filename[filename.index('_') + 1:]
                shutil.move(os.path.join(origin, filename), os.path.join(destination, new_filename))
        self.next()

    def next(self):
        self.index += 1

AssetMapParser(assetmap_lines).parse()