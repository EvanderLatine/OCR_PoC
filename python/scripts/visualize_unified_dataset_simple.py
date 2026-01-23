"""
Упрощенный скрипт для анализа объединенного датасета Emirates ID.
"""
import json
from pathlib import Path
from collections import defaultdict
from typing import Dict, List

# Базовый путь к проекту
BASE_PATH = Path(r"C:\Miral\OCR_PoC")
DATASET_PATH = BASE_PATH / "data" / "eid-field-boxes"

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

def analyze_dataset_statistics(split: str):
    """Анализ статистики датасета."""
    print(f"\n{'='*50}")
    print(f"Анализ разбиения: {split.upper()}")
    print(f"{'='*50}")
    
    # Загрузка аннотаций
    data = load_annotations(split)
    
    # Общая статистика
    print(f"\nОбщая статистика:")
    print(f"  • Количество изображений: {len(data['images'])}")
    print(f"  • Количество аннотаций: {len(data['annotations'])}")
    print(f"  • Количество категорий: {len(data['categories'])}")
    
    # Статистика по категориям
    category_counts = defaultdict(int)
    category_boxes_per_image = defaultdict(list)
    annotations_by_source = defaultdict(lambda: defaultdict(int))
    
    # Создание маппинга изображений
    image_map = {img['id']: img for img in data['images']}
    
    # Подсчет аннотаций
    for ann in data['annotations']:
        cat_id = ann['category_id']
        img_id = ann['image_id']
        category_counts[cat_id] += 1
        category_boxes_per_image[img_id].append(cat_id)
        
        # Определение источника
        if img_id in image_map:
            filename = image_map[img_id]['file_name']
            if filename.startswith('eid_back_detection_'):
                source = 'back_detection'
            elif filename.startswith('eid_front_detection_'):
                source = 'front_detection'
            elif filename.startswith('eid_front_segmentation_'):
                source = 'front_segmentation'
            else:
                source = 'unknown'
            
            cat_name = next((cat['name'] for cat in data['categories'] if cat['id'] == cat_id), 'unknown')
            annotations_by_source[source][cat_name] += 1
    
    # Вывод статистики по категориям
    print("\nСтатистика по категориям:")
    total_boxes = 0
    for cat in sorted(data['categories'], key=lambda x: x['id']):
        cat_id = cat['id']
        cat_name = cat['name']
        cat_supercat = cat['supercategory']
        count = category_counts[cat_id]
        total_boxes += count
        print(f"  • {cat_name:<20} ({cat_supercat:<20}): {count:>4} боксов")
    print(f"  {'─'*60}")
    print(f"  {'ИТОГО:':<43} {total_boxes:>4} боксов")
    
    # Анализ источников изображений
    source_counts = defaultdict(int)
    for img in data['images']:
        filename = img['file_name']
        if filename.startswith('eid_back_detection_'):
            source_counts['back_detection'] += 1
        elif filename.startswith('eid_front_detection_'):
            source_counts['front_detection'] += 1
        elif filename.startswith('eid_front_segmentation_'):
            source_counts['front_segmentation'] += 1
    
    print("\nИсточники изображений:")
    for source, count in sorted(source_counts.items()):
        print(f"  • {source:<20}: {count:>3} изображений")
    
    # Детальная статистика по источникам
    print("\nДетальная статистика по источникам и категориям:")
    for source in sorted(annotations_by_source.keys()):
        print(f"\n  {source}:")
        for cat_name, count in sorted(annotations_by_source[source].items()):
            print(f"    • {cat_name:<20}: {count:>3}")
    
    # Среднее количество боксов на изображение
    if len(category_boxes_per_image) > 0:
        avg_boxes = sum(len(boxes) for boxes in category_boxes_per_image.values()) / len(category_boxes_per_image)
        min_boxes = min(len(boxes) for boxes in category_boxes_per_image.values())
        max_boxes = max(len(boxes) for boxes in category_boxes_per_image.values())
        print(f"\nСтатистика боксов на изображение:")
        print(f"  • Среднее: {avg_boxes:.2f}")
        print(f"  • Минимум: {min_boxes}")
        print(f"  • Максимум: {max_boxes}")

def analyze_category_coverage():
    """Анализ покрытия категорий по всем разбиениям."""
    print(f"\n{'='*50}")
    print("АНАЛИЗ ПОКРЫТИЯ КАТЕГОРИЙ")
    print(f"{'='*50}")
    
    dataset_info = load_dataset_info()
    all_categories = {int(k): v['name'] for k, v in dataset_info['categories'].items()}
    
    coverage_by_split = {}
    
    for split in ['train', 'valid', 'test']:
        data = load_annotations(split)
        category_counts = defaultdict(int)
        
        for ann in data['annotations']:
            category_counts[ann['category_id']] += 1
        
        coverage_by_split[split] = category_counts
    
    # Вывод таблицы покрытия
    print(f"\n{'Категория':<20} {'Train':>10} {'Valid':>10} {'Test':>10} {'Всего':>10}")
    print("─" * 65)
    
    for cat_id in sorted(all_categories.keys()):
        cat_name = all_categories[cat_id]
        train_count = coverage_by_split['train'].get(cat_id, 0)
        valid_count = coverage_by_split['valid'].get(cat_id, 0)
        test_count = coverage_by_split['test'].get(cat_id, 0)
        total_count = train_count + valid_count + test_count
        
        print(f"{cat_name:<20} {train_count:>10} {valid_count:>10} {test_count:>10} {total_count:>10}")
    
    # Общие итоги
    print("─" * 65)
    total_train = sum(coverage_by_split['train'].values())
    total_valid = sum(coverage_by_split['valid'].values())
    total_test = sum(coverage_by_split['test'].values())
    grand_total = total_train + total_valid + total_test
    
    print(f"{'ИТОГО:':<20} {total_train:>10} {total_valid:>10} {total_test:>10} {grand_total:>10}")

def save_summary_report():
    """Сохранение сводного отчета о датасете."""
    report_path = DATASET_PATH / "dataset_summary.txt"
    
    # Перенаправление вывода в файл
    import sys
    original_stdout = sys.stdout
    
    with open(report_path, 'w', encoding='utf-8') as f:
        sys.stdout = f
        
        print("ОБЪЕДИНЕННЫЙ ДАТАСЕТ EMIRATES ID - СВОДНЫЙ ОТЧЕТ")
        print("=" * 70)
        print(f"Дата создания: 2025-01-17")
        print(f"Версия: 1.0")
        print("=" * 70)
        
        # Анализ каждого разбиения
        for split in ['train', 'valid', 'test']:
            analyze_dataset_statistics(split)
        
        # Анализ покрытия категорий
        analyze_category_coverage()
        
        print("\n" + "=" * 70)
        print("Отчет сохранен успешно!")
    
    sys.stdout = original_stdout
    print(f"\nСводный отчет сохранен в: {report_path}")

def main():
    """Основная функция."""
    print("\n" + "="*70)
    print("АНАЛИЗ ОБЪЕДИНЕННОГО ДАТАСЕТА EMIRATES ID")
    print("="*70)
    
    # Анализ каждого разбиения
    for split in ['train', 'valid', 'test']:
        analyze_dataset_statistics(split)
    
    # Анализ покрытия категорий
    analyze_category_coverage()
    
    # Сохранение сводного отчета
    save_summary_report()
    
    print("\n" + "="*70)
    print("Анализ завершен успешно!")
    print("="*70)

if __name__ == "__main__":
    main()