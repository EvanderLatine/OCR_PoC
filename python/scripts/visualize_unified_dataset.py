"""
Скрипт для визуализации и проверки объединенного датасета Emirates ID.
"""
import json
import os
import random
from pathlib import Path
from typing import Dict, List
import cv2
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as patches
from collections import defaultdict

# Базовый путь к проекту
BASE_PATH = Path(r"C:\Miral\OCR_PoC")
DATASET_PATH = BASE_PATH / "data" / "eid-field-boxes"

# Цвета для каждой категории (RGB normalized)
CATEGORY_COLORS = {
    0: (0.0, 0.0, 1.0),      # id_number - синий
    1: (0.0, 1.0, 0.0),      # name - зеленый
    2: (1.0, 0.0, 0.0),      # date_of_birth - красный
    3: (1.0, 1.0, 0.0),      # sex - желтый
    4: (1.0, 0.0, 1.0),      # nationality - фиолетовый
    5: (0.0, 1.0, 1.0),      # expiry_date - голубой
    6: (0.5, 0.5, 0.0),      # issue_date - оливковый
    7: (0.0, 0.5, 0.5),      # issuing_place - темно-голубой
    8: (0.5, 0.0, 0.5),      # employer - темно-фиолетовый
    9: (1.0, 0.5, 0.0)       # mrz - оранжевый
}

def load_dataset_info() -> Dict:
    """Загрузка информации о датасете."""
    info_file = DATASET_PATH / "dataset_info.json"
    with open(info_file, 'r', encoding='utf-8') as f:
        return json.load(f)

def load_annotations(split: str) -> Dict:
    """Загрузка аннотаций для указанного разбиения."""
    ann_file = DATASET_PATH / split / "_annotations.coco.json"
    with open(ann_file, 'r', encoding='utf-8') as f:
        return json.load(f)

def visualize_image_with_boxes(image_path: Path, annotations: List[Dict], categories: Dict, save_path: Path = None):
    """Визуализация изображения с боксами."""
    # Загрузка изображения
    img = cv2.imread(str(image_path))
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    
    # Создание фигуры
    fig, ax = plt.subplots(1, figsize=(12, 8))
    ax.imshow(img_rgb)
    
    # Отрисовка боксов
    for ann in annotations:
        cat_id = ann['category_id']
        cat_name = categories[str(cat_id)]['name']
        bbox = ann['bbox']
        x, y, w, h = bbox
        
        # Получение цвета для категории
        color = CATEGORY_COLORS.get(cat_id, (0, 0, 0))
        
        # Создание прямоугольника
        rect = patches.Rectangle(
            (x, y), w, h,
            linewidth=2,
            edgecolor=color,
            facecolor='none'
        )
        ax.add_patch(rect)
        
        # Добавление подписи
        ax.text(
            x, y - 5, cat_name,
            color=color,
            fontsize=10,
            weight='bold',
            bbox=dict(boxstyle='round,pad=0.3', facecolor='white', alpha=0.8)
        )
    
    ax.set_title(f"Изображение: {image_path.name}")
    ax.axis('off')
    
    if save_path:
        plt.savefig(save_path, bbox_inches='tight', dpi=150)
        plt.close()
    else:
        plt.show()

def analyze_dataset_statistics(split: str):
    """Анализ статистики датасета."""
    print(f"\n=== Анализ разбиения: {split} ===")
    
    # Загрузка аннотаций
    data = load_annotations(split)
    
    # Общая статистика
    print(f"Количество изображений: {len(data['images'])}")
    print(f"Количество аннотаций: {len(data['annotations'])}")
    print(f"Количество категорий: {len(data['categories'])}")
    
    # Статистика по категориям
    category_counts = defaultdict(int)
    category_boxes_per_image = defaultdict(list)
    
    # Создание маппинга изображений
    image_map = {img['id']: img for img in data['images']}
    
    # Подсчет аннотаций
    for ann in data['annotations']:
        cat_id = ann['category_id']
        img_id = ann['image_id']
        category_counts[cat_id] += 1
        category_boxes_per_image[img_id].append(cat_id)
    
    # Вывод статистики по категориям
    print("\nСтатистика по категориям:")
    for cat in data['categories']:
        cat_id = cat['id']
        cat_name = cat['name']
        count = category_counts[cat_id]
        print(f"  {cat_name}: {count} боксов")
    
    # Анализ источников изображений
    source_counts = defaultdict(int)
    for img in data['images']:
        # Определение источника по префиксу имени файла
        filename = img['file_name']
        if filename.startswith('eid_back_detection_'):
            source_counts['back_detection'] += 1
        elif filename.startswith('eid_front_detection_'):
            source_counts['front_detection'] += 1
        elif filename.startswith('eid_front_segmentation_'):
            source_counts['front_segmentation'] += 1
    
    print("\nИсточники изображений:")
    for source, count in source_counts.items():
        print(f"  {source}: {count} изображений")
    
    # Среднее количество боксов на изображение
    if len(category_boxes_per_image) > 0:
        avg_boxes = sum(len(boxes) for boxes in category_boxes_per_image.values()) / len(category_boxes_per_image)
        print(f"\nСреднее количество боксов на изображение: {avg_boxes:.2f}")

def visualize_random_samples(split: str, n_samples: int = 5):
    """Визуализация случайных примеров из датасета."""
    print(f"\nВизуализация {n_samples} случайных изображений из {split}...")
    
    # Загрузка данных
    dataset_info = load_dataset_info()
    data = load_annotations(split)
    
    # Создание маппингов
    image_map = {img['id']: img for img in data['images']}
    annotations_by_image = defaultdict(list)
    for ann in data['annotations']:
        annotations_by_image[ann['image_id']].append(ann)
    
    # Выбор случайных изображений
    available_images = [img_id for img_id in annotations_by_image.keys() if img_id in image_map]
    sample_image_ids = random.sample(available_images, min(n_samples, len(available_images)))
    
    # Создание директории для сохранения визуализаций
    vis_dir = DATASET_PATH / "visualizations" / split
    vis_dir.mkdir(parents=True, exist_ok=True)
    
    # Визуализация каждого изображения
    for idx, img_id in enumerate(sample_image_ids):
        img_info = image_map[img_id]
        img_path = DATASET_PATH / split / img_info['file_name']
        annotations = annotations_by_image[img_id]
        
        if img_path.exists():
            save_path = vis_dir / f"sample_{idx + 1}_{img_info['file_name']}"
            visualize_image_with_boxes(img_path, annotations, dataset_info['categories'], save_path)
            print(f"  Сохранено: {save_path.name}")
        else:
            print(f"  Изображение не найдено: {img_path}")

def main():
    """Основная функция."""
    print("=== Анализ объединенного датасета Emirates ID ===")
    
    # Анализ каждого разбиения
    for split in ['train', 'valid', 'test']:
        analyze_dataset_statistics(split)
    
    # Визуализация примеров из каждого разбиения
    print("\n=== Визуализация примеров ===")
    for split in ['train', 'valid', 'test']:
        visualize_random_samples(split, n_samples=3)
    
    print("\nАнализ завершен!")
    print(f"Визуализации сохранены в: {DATASET_PATH / 'visualizations'}")

if __name__ == "__main__":
    main()