import os
import random
from PIL import Image

# Define directories
base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
train_dir = os.path.join(base_dir, 'data', 'eid-field-boxes', 'train')
output_dir = os.path.join(base_dir, 'data', 'eid-pdf')

# List all files in train_dir
all_files = [f for f in os.listdir(train_dir) if f.endswith(('.jpg', '.png'))]

# Separate front and back images
front_images = [f for f in all_files if 'front' in f.lower() or 'page_1' in f.lower()]
back_images = [f for f in all_files if 'back' in f.lower() or 'page_2' in f.lower()]

# Generate 200 random PDFs
for i in range(200):
    # Randomly select front and back
    front_file = random.choice(front_images)
    back_file = random.choice(back_images)
    
    front_path = os.path.join(train_dir, front_file)
    back_path = os.path.join(train_dir, back_file)
    
    # Open images
    front_img = Image.open(front_path)
    back_img = Image.open(back_path)
    
    # Save as multi-page PDF
    pdf_path = os.path.join(output_dir, f'document_{i+1}.pdf')
    front_img.save(pdf_path, 'PDF', save_all=True, append_images=[back_img])
    
print('Generated 200 PDF documents.') 