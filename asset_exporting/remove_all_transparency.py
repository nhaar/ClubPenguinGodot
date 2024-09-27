# removes all transparent outlines in all pngs inside image_test/transparent and sends to image_test/transparentout

from transparency_remover import remove_transparent_outline
from processors import process_directory

process_directory("transparent", "transparentout", remove_transparent_outline)