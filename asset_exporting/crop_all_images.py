# crops all images that are in the image_test/crop directory
# places it inside image_test/cropout

from ffdec_image_fix import crop_image
from processors import process_directory

process_directory("crop", "cropout", crop_image)