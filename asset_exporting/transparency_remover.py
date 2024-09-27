from PIL import Image

def crop_top_transparency(image):
    while True:
        pixels = image.load()
        # if all squares are transparent in top row
        if all([pixels[x, 0] == (0, 0, 0, 0) for x in range(image.size[0])]):
            image = image.crop((0, 1, image.size[0], image.size[1]))
        else:
            break
    return image

def crop_bottom_transparency(image):
    while True:
        pixels = image.load()
        # if all squares are transparent in bottom row
        if all([pixels[x, image.size[1] - 1] == (0, 0, 0, 0) for x in range(image.size[0])]):
            image = image.crop((0, 0, image.size[0], image.size[1] - 1))
        else:
            break
    return image

def crop_left_transparency(image):
    while True:
        pixels = image.load()
        # if all squares are transparent in left column
        if all([pixels[0, y] == (0, 0, 0, 0) for y in range(image.size[1])]):
            image = image.crop((1, 0, image.size[0], image.size[1]))
        else:
            break
    return image

def crop_right_transparency(image):
    while True:
        pixels = image.load()
        # if all squares are transparent in right column
        if all([pixels[image.size[0] - 1, y] == (0, 0, 0, 0) for y in range(image.size[1])]):
            image = image.crop((0, 0, image.size[0] - 1, image.size[1]))
        else:
            break
    return image

def remove_transparent_outline(image_path, output_path):
    image = Image.open(image_path)
    image = crop_top_transparency(image)
    image = crop_right_transparency(image)
    image = crop_left_transparency(image)
    image = crop_bottom_transparency(image)
    image.save(output_path)