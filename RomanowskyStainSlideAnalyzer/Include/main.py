import torch
import os
import time
import cv2
import argparse
import numpy as np

from matplotlib import pyplot as plt
from PIL import Image
from include.sam2.automatic_mask_generator import SAM2AutomaticMaskGenerator
from include.sam2.build_sam import build_sam2

OUTPUT_DIR = './outputs'
HSV_RANGE = (np.array([84, 0, 0]), np.array([225, 10, 85]))
L_RANGE = (np.array([0]), np.array([204]))
G_RANGE = (np.array([160]))

def show_anns(anns, original_image, borders=True):
    if len(anns) == 0:
        return

    sorted_anns = sorted(anns, key=(lambda x: x['area']), reverse=True)
    ax = plt.gca()
    ax.set_autoscale_on(False)

    img = np.ones((sorted_anns[0]['segmentation'].shape[0], sorted_anns[0]['segmentation'].shape[1], 4))
    img[:, :, 3] = 0

    for ann in sorted_anns:
        m = ann['segmentation']
        lower_l, upper_l = L_RANGE
        x, y, w, h = ann['bbox']
        x, y, w, h = int(x), int(y), int(w) if w > 0.0 else 1, int(h) if h > 0.0 else 1

        l_value = original_image[y:y+h, x:x+w].copy()

        if len(l_value) == 0: continue

        try:
            is_in_range = cv2.inRange(l_value, lower_l, upper_l)
            indices = np.argwhere(is_in_range == 255)

            if len(indices) > 0:
                color_mask = np.concatenate([np.random.random(3), [0.5]])
                img[m] = color_mask

                if borders:
                    contours, _ = cv2.findContours(m.astype(np.uint8), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
                    contours = [cv2.approxPolyDP(contour, epsilon=0.01, closed=True) for contour in contours]
                    cv2.drawContours(img, contours, -1, (0, 0, 1, 0.4), thickness=1)

        except cv2.error as e:
            print(e)
            continue

    ax.imshow(img)


if __name__ == '__main__':
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    torch.autocast("cuda", dtype=torch.bfloat16).__enter__()

    parser = argparse.ArgumentParser()
    parser.add_argument("-f", "--file", required=True, help='Target Image File')
    parser.add_argument("-d", "--dest", required=True, help='Destination File Name')
    args = parser.parse_args()

    file = args.file
    destination = args.dest

    image = Image.open(f'./{file}').convert('RGB')
    min_area = min(image.width, image.height)
    resized_image = image.resize((512, 512))

    cv_image = cv2.imread(f'./{file}')
    cv_image = cv2.cvtColor(cv_image, cv2.COLOR_BGR2GRAY)

    original_height, original_width = cv_image.shape[:2]
    aspect_ratio = original_width / original_height
    new_width = 512
    new_height = int(512 / aspect_ratio)

    cv_image = cv2.resize(cv_image, (new_width, new_height), interpolation=cv2.INTER_CUBIC)
    cv_image = cv2.rotate(cv_image, cv2.ROTATE_90_COUNTERCLOCKWISE)

    image = np.array(resized_image)
    sam2_checkpoint = './checkpoints/sam2.1_hiera_large.pt'
    model_cfg = './configs/sam2.1/sam2.1_hiera_l.yaml'

    model = build_sam2(model_cfg, sam2_checkpoint, device=device, apply_postprocessing=False)
    mask_generator = SAM2AutomaticMaskGenerator(
        model=model,
        points_per_side=128,
        points_per_batch=32,
        pred_iou_thresh=0.0,
        stability_score_thresh=1.0,
        stability_score_offset=1.0,
        mask_threshold=0.0,
        box_nms_thresh=1.0,
        crop_n_layers=2,
        crop_nms_thresh=0.7,
        crop_overlap_ratio=512 / 1500,
        crop_n_points_downscale_factor=1,
        point_grids=None,
        min_mask_region_area=-1
    )

    mask = mask_generator.generate(image)

    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

    plt.figure(figsize=(20, 20))
    sort_start = time.time()
    plt.imshow(image)
    show_anns(mask, cv_image)
    sort_end = time.time()
    plt.axis('off')
    plt.savefig(f'{OUTPUT_DIR}/{destination}.png', dpi=300)
    plt.show()
