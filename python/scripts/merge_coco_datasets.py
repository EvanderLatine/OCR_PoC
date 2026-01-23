"""
Скрипт для объединения трех COCO датасетов Emirates ID с унифицированными названиями классов.
"""
import json
import os
import shutil
from pathlib import Path
from typing import Dict, List, Tuple
import logging

# Настройка логирования
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Базовый путь к проекту
BASE_PATH = Path(r"C:\Miral\OCR_PoC")
DATA_PATH = BASE_PATH / "data"
OUTPUT_PATH = DATA_PATH / "eid-field-boxes"

# Пути к датасетам
DATASETS = {
    "eid_back_detection": DATA_PATH / "EID back fields object detection",
    "eid_front_detection": DATA_PATH / "EID front fields object detection",
    "eid_front_segmentation": DATA_PATH / "EID front fields segmantation"
}

# Унифицированная схема классов
UNIFIED_CATEGORIES = {
    0: {"name": "id_number", "supercategory": "identification"},
    1: {"name": "name", "supercategory": "personal_info"},
    2: {"name": "date_of_birth", "supercategory": "personal_info"},
    3: {"name": "sex", "supercategory": "personal_info"},
    4: {"name": "nationality", "supercategory": "personal_info"},
    5: {"name": "expiry_date", "supercategory": "dates"},
    6: {"name": "issue_date", "supercategory": "dates"},
    7: {"name": "issuing_place", "supercategory": "document_info"},
    8: {"name": "employer", "supercategory": "personal_info"},
    9: {"name": "mrz", "supercategory": "machine_readable"}
}

# Маппинг старых категорий на новые для каждого датасета
CATEGORY_MAPPINGS = {
    "eid_back_detection": {
        # MRZ-Employer-Issuing_Place-DOB (0) - игнорируем, это группировка
        1: 2,  # DOB -> date_of_birth
        2: 8,  # Employer -> employer
        3: 5,  # Expiry-Date -> expiry_date
        4: 7,  # Issuing-Place -> issuing_place
        5: 9,  # MRZ -> mrz
        6: 3   # Sex -> sex
    },
    "eid_front_detection": {
        # ID-Number-Name-D (0) - игнорируем, это группировка
        1: 2,  # DOB -> date_of_birth
        2: 0,  # ID_Number -> id_number
        3: 6,  # Issue_Expiry_date -> issue_date (будем разделять позже если нужно)
        4: 1,  # Name -> name
        5: 4,  # Nationality -> nationality
        6: 3   # Sex -> sex
    },
    "eid_front_segmentation": {
        # id (0) - игнорируем, неясно что это
        1: 0,  # cin -> id_number
        2: 2,  # date of birth -> date_of_birth
        3: 5,  # exp date -> expiry_date
        4: 1,  # fullname -> name
        5: 6,  # iss date -> issue_date
        6: 4,  # nationality -> nationality
        7: 3   # sex -> sex
    }
}

def load_coco_json(file_path: Path) -> Dict:
    """Загрузка COCO JSON файла."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            return json.load(f)
        logger.info(f"Успешно загружен файл: {file_path}")
    except Exception as e:
        logger.error(f"Ошибка при загрузке файла {file_path}: {e}")
        raise

def merge_datasets(splits: List[str] = ["train", "valid", "test"]) -> None:
    """Объединение датасетов с унифицированными категориями."""
    
    # Создание выходной директории
    OUTPUT_PATH.mkdir(parents=True, exist_ok=True)
    logger.info(f"Создана выходная директория: {OUTPUT_PATH}")
    
    for split in splits:
        logger.info(f"\nОбработка разбиения: {split}")
        
        # Инициализация объединенного датасета
        merged_data = {
            "info": {
                "year": "2025",
                "version": "1.0",
                "description": "Unified Emirates ID fields dataset combining detection and segmentation annotations",
                "contributor": "Yaroslav Kazakov",
                "url": "",
                "date_created": "2025-07-16"
            },
            "licenses": [{
                "id": 1,
                "url": "https://creativecommons.org/licenses/by/4.0/",
                "name": "CC BY 4.0"
            }],
            "categories": [{"id": cat_id, **cat_info} for cat_id, cat_info in UNIFIED_CATEGORIES.items()],
            "images": [],
            "annotations": []
        }
        
        # Создание директории для изображений
        split_dir = OUTPUT_PATH / split
        split_dir.mkdir(exist_ok=True)
        
        # Счетчики
        image_id_offset = 0
        annotation_id_offset = 0
        
        # Статистика
        stats = {name: {"images": 0, "annotations": 0, "categories": {}} for name in DATASETS}
        
        # Обработка каждого датасета
        for dataset_name, dataset_path in DATASETS.items():
            logger.info(f"  Обработка датасета: {dataset_name}")
            
            # Путь к аннотациям
            ann_file = dataset_path / split / "_annotations.coco.json"
            if not ann_file.exists():
                logger.warning(f"    Файл аннотаций не найден: {ann_file}")
                continue
            
            # Загрузка аннотаций
            data = load_coco_json(ann_file)
            
            # Маппинг старых ID изображений на новые
            image_id_map = {}
            
            # Обработка изображений
            for img in data["images"]:
                old_img_id = img["id"]
                new_img_id = old_img_id + image_id_offset
                image_id_map[old_img_id] = new_img_id
                
                # Создание нового имени файла с префиксом датасета
                old_filename = img["file_name"]
                new_filename = f"{dataset_name}_{old_filename}"
                
                # Копирование изображения
                src_image = dataset_path / split / old_filename
                dst_image = split_dir / new_filename
                
                if src_image.exists():
                    shutil.copy2(src_image, dst_image)
                    
                    # Обновление информации об изображении
                    img["id"] = new_img_id
                    img["file_name"] = new_filename
                    merged_data["images"].append(img)
                    stats[dataset_name]["images"] += 1
                else:
                    logger.warning(f"    Изображение не найдено: {src_image}")
            
            # Обработка аннотаций
            category_mapping = CATEGORY_MAPPINGS[dataset_name]
            for ann in data["annotations"]:
                old_cat_id = ann["category_id"]
                
                # Пропускаем категории, которых нет в маппинге
                if old_cat_id not in category_mapping:
                    continue
                
                new_cat_id = category_mapping[old_cat_id]
                
                # Обновление аннотации
                new_ann = ann.copy()
                new_ann["id"] = ann["id"] + annotation_id_offset
                new_ann["image_id"] = image_id_map.get(ann["image_id"], ann["image_id"])
                new_ann["category_id"] = new_cat_id
                
                merged_data["annotations"].append(new_ann)
                stats[dataset_name]["annotations"] += 1
                
                # Обновление статистики по категориям
                cat_name = UNIFIED_CATEGORIES[new_cat_id]["name"]
                stats[dataset_name]["categories"][cat_name] = stats[dataset_name]["categories"].get(cat_name, 0) + 1
            
            # Обновление смещений
            image_id_offset = max([img["id"] for img in merged_data["images"]], default=0) + 1
            annotation_id_offset = max([ann["id"] for ann in merged_data["annotations"]], default=0) + 1
        
        # Сохранение объединенных аннотаций
        output_file = split_dir / "_annotations.coco.json"
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(merged_data, f, indent=2, ensure_ascii=False)
        
        logger.info(f"  Сохранены объединенные аннотации: {output_file}")
        logger.info(f"  Всего изображений: {len(merged_data['images'])}")
        logger.info(f"  Всего аннотаций: {len(merged_data['annotations'])}")
        
        # Вывод статистики
        logger.info("\n  Статистика по датасетам:")
        for dataset_name, dataset_stats in stats.items():
            logger.info(f"\n    {dataset_name}:")
            logger.info(f"      Изображений: {dataset_stats['images']}")
            logger.info(f"      Аннотаций: {dataset_stats['annotations']}")
            logger.info(f"      Категории:")
            for cat_name, count in sorted(dataset_stats['categories'].items()):
                logger.info(f"        {cat_name}: {count}")

def create_dataset_info():
    """Создание файла с информацией о датасете."""
    info = {
        "dataset_name": "Unified Emirates ID Fields Dataset",
        "version": "1.0",
        "description": "Combined dataset from three sources with unified category names",
        "categories": UNIFIED_CATEGORIES,
        "sources": list(DATASETS.keys()),
        "splits": ["train", "valid", "test"],
        "creation_date": "2025-01-17"
    }
    
    info_file = OUTPUT_PATH / "dataset_info.json"
    with open(info_file, 'w', encoding='utf-8') as f:
        json.dump(info, f, indent=2, ensure_ascii=False)
    
    logger.info(f"\nСоздан файл с информацией о датасете: {info_file}")

def main():
    """Основная функция."""
    logger.info("Начало объединения датасетов Emirates ID")
    
    try:
        # Объединение датасетов
        merge_datasets()
        
        # Создание файла с информацией
        create_dataset_info()
        
        logger.info("\nОбъединение датасетов успешно завершено!")
        logger.info(f"Результаты сохранены в: {OUTPUT_PATH}")
        
    except Exception as e:
        logger.error(f"Ошибка при объединении датасетов: {e}")
        raise

if __name__ == "__main__":
    main()