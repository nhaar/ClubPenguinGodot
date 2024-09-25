# crops all images that are in the image_test/crop directory
# places it inside image_test/cropout

import os
from ffdec_image_fix import crop_all_pngs

script_dir = os.path.dirname(os.path.realpath(__file__))
crop_dir = os.path.join(script_dir, "..", "image_test", "crop")
out_dir = os.path.join(crop_dir, "..", "cropout")


os.makedirs(out_dir, exist_ok=True)
crop_all_pngs(crop_dir, out_dir)