from email.policy import default

import torch
import os
import time
import cv2
import argparse
import numpy as np
from fontTools.mtiLib import build

from matplotlib import pyplot as plt
from PIL import Image
from include.sam2.automatic_mask_generator import SAM2AutomaticMaskGenerator
from include.sam2.build_sam import build_sam2
from include.sam2.sam2_image_predictor import SAM2ImagePredictor

OUTPUT_DIR = './outputs'
HSV_RANGE = (np.array([84, 0, 0]), np.array([225, 10, 85]))
L_RANGE = (np.array([0]), np.array([204]))
G_RANGE = (np.array([160]))

def show_original_anns(anns, borders=True):
    if len(anns) == 0:
        return
    sorted_anns = sorted(anns, key=(lambda x: x['area']), reverse=True)
    ax = plt.gca()
    ax.set_autoscale_on(False)

    img = np.ones((sorted_anns[0]['segmentation'].shape[0], sorted_anns[0]['segmentation'].shape[1], 4))
    img[:, :, 3] = 0
    for ann in sorted_anns:
        m = ann['segmentation']
        color_mask = np.concatenate([np.random.random(3), [0.5]])
        img[m] = color_mask
        if borders:
            import cv2
            contours, _ = cv2.findContours(m.astype(np.uint8), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            # Try to smooth contours
            contours = [cv2.approxPolyDP(contour, epsilon=0.01, closed=True) for contour in contours]
            cv2.drawContours(img, contours, -1, (0, 0, 1, 0.4), thickness=1)

    ax.imshow(img)

def show_mask(mask, ax, random_color=False, borders = True):
    if random_color:
        color = np.concatenate([np.random.random(3), np.array([0.6])], axis=0)
    else:
        color = np.array([30/255, 144/255, 255/255, 0.6])
    h, w = mask.shape[-2:]
    mask = mask.astype(np.uint8)
    mask_image =  mask.reshape(h, w, 1) * color.reshape(1, 1, -1)
    if borders:
        import cv2
        contours, _ = cv2.findContours(mask,cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        # Try to smooth contours
        contours = [cv2.approxPolyDP(contour, epsilon=0.01, closed=True) for contour in contours]
        mask_image = cv2.drawContours(mask_image, contours, -1, (1, 1, 1, 0.5), thickness=2)
    ax.imshow(mask_image)

def show_points(coords, labels, ax, marker_size=375):
    pos_points = coords[labels==1]
    neg_points = coords[labels==0]
    ax.scatter(pos_points[:, 0], pos_points[:, 1], color='green', marker='*', s=marker_size, edgecolor='white', linewidth=1.25)
    ax.scatter(neg_points[:, 0], neg_points[:, 1], color='red', marker='*', s=marker_size, edgecolor='white', linewidth=1.25)

def show_box(box, ax):
    x0, y0 = box[0], box[1]
    w, h = box[2] - box[0], box[3] - box[1]
    ax.add_patch(plt.Rectangle((x0, y0), w, h, edgecolor='green', facecolor=(0, 0, 0, 0), lw=2))

def show_masks(image, masks, scores, point_coords=None, box_coords=None, input_labels=None, borders=True):
    for i, (mask, score) in enumerate(zip(masks, scores)):
        plt.figure(figsize=(10, 10))
        plt.imshow(image)
        show_mask(mask, plt.gca(), borders=borders)
        if point_coords is not None:
            assert input_labels is not None
            show_points(point_coords, input_labels, plt.gca())
        if box_coords is not None:
            show_box(box_coords, plt.gca())
        if len(scores) > 1:
            plt.title(f"Mask {i+1}, Score: {score:.3f}", fontsize=18)
        plt.axis('off')
        plt.show()

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
    parser.add_argument("-a", "--automatic_segmentation", default='y')
    parser.add_argument("-ip", "--input_points", nargs='+')
    parser.add_argument("-il", "--input_labels", nargs='+')
    parser.add_argument("-prefix", "--prefix", type=str, default="")
    parser.add_argument("-postfix", "--postfix", type=str, default="output")
    parser.add_argument("-d", "--dest", type=str, default="")
    parser.add_argument("-e", "--ext", type=str, default="")
    parser.add_argument("-p", "--usePostProcess", type=bool, default=True, help='Define to use post process image')
    parser.add_argument("-pps", "--points_per_side", type=int, default=128)
    parser.add_argument("-ppb", "--points_per_batch", type=int, default=32)
    parser.add_argument("-pit", "--pred_iou_thresh", type=float, default=0.0)
    parser.add_argument("-sst", "--stability_score_thresh", type=float, default=1.0)
    parser.add_argument("-sso", "--stability_score_offset", type=float, default=1.0)
    parser.add_argument("-mt", "--mask_threshold", type=float, default=0.0)
    parser.add_argument("-bnt", "--box_nms_thresh", type=float, default=1.0)
    parser.add_argument("-cnl", "--crop_n_layers", type=int, default=2)
    parser.add_argument("-cnt", "--crop_nms_thresh", type=float, default=0.7)
    parser.add_argument("-cor", "--crop_overlap_ratio", type=float, default=512/1500)
    parser.add_argument("-cnp", "--crop_n_points_downscale_factor", type=int, default=1)
    parser.add_argument("-mmr", "--min_mask_region_area", type=int, default=-1)
    args = parser.parse_args()

    file = args.file
    destination = args.dest
    pre_name = args.prefix
    post_name = args.postfix
    ext = args.ext

    automatic_segmentation = True if args.automatic_segmentation == 'y' else False
    use_post_process = args.usePostProcess
    points_per_side = args.points_per_side
    points_per_batch = args.points_per_batch
    pred_iou_thresh = args.pred_iou_thresh
    stability_score_thresh = args.stability_score_thresh
    stability_score_offset = args.stability_score_offset
    mask_threshold = args.mask_threshold
    box_nms_thresh = args.box_nms_thresh
    crop_n_layers = args.crop_n_layers
    crop_nms_thresh = args.crop_nms_thresh
    crop_overlap_ratio = args.crop_overlap_ratio
    crop_n_points_downscale_factor = args.crop_n_points_downscale_factor
    min_mask_region_area = args.min_mask_region_area

    input_points = args.input_points
    input_labels = args.input_labels

    filename = ""

    if pre_name == "" and destination == "":
        filename = f"{str(file).split("/")[-1].split(".")[0]}_{post_name}.{str(file).split(".")[-1]}"

    else:
        if pre_name != "": filename = f"{pre_name}_{str(file).split("/")[-1]}"
        else: filename = f"{destination}.{ext}"

    image = Image.open(f'{file}').convert('RGB')
    min_area = min(image.width, image.height)
    resized_image = image.resize((512, 512))

    sam2_checkpoint = './checkpoints/sam2.1_hiera_large.pt'
    model_cfg = './configs/sam2.1/sam2.1_hiera_l.yaml'

    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

    if automatic_segmentation:
        cv_image = cv2.imread(f'{file}')
        cv_image = cv2.cvtColor(cv_image, cv2.COLOR_BGR2GRAY)

        original_height, original_width = cv_image.shape[:2]
        aspect_ratio = original_width / original_height
        new_width = 512
        new_height = int(512 / aspect_ratio)

        cv_image = cv2.resize(cv_image, (new_width, new_height), interpolation=cv2.INTER_CUBIC)
        cv_image = cv2.rotate(cv_image, cv2.ROTATE_90_COUNTERCLOCKWISE)

        image = np.array(resized_image)

        model = build_sam2(model_cfg, sam2_checkpoint, device=device, apply_postprocessing=False)
        mask_generator = SAM2AutomaticMaskGenerator(
            model=model,
            points_per_side=points_per_side,
            points_per_batch=points_per_batch,
            pred_iou_thresh=pred_iou_thresh,
            stability_score_thresh=stability_score_thresh,
            stability_score_offset=stability_score_offset,
            mask_threshold=mask_threshold,
            box_nms_thresh=box_nms_thresh,
            crop_n_layers=crop_n_layers,
            crop_nms_thresh=crop_nms_thresh,
            crop_overlap_ratio=crop_overlap_ratio,
            crop_n_points_downscale_factor=crop_n_points_downscale_factor,
            point_grids=None,
            min_mask_region_area=min_mask_region_area
        )

        mask = mask_generator.generate(image)

        plt.figure(figsize=(20, 20))
        plt.imshow(image)

        if use_post_process:
            show_anns(mask, cv_image)

        else:
            show_original_anns(mask)

        plt.axis('off')
        plt.savefig(f'{OUTPUT_DIR}/{filename}', dpi=300)
        plt.show()

    else:
        model = build_sam2(model_cfg, sam2_checkpoint, device=device)
        predictor = SAM2ImagePredictor(model)
        predictor.set_image(resized_image)

        assert len(input_points) % 2 == 0

        points = []
        labels = []

        for p in range(0, len(input_points), 2):
            points.append([float(input_points[p]), float(input_points[p+1])])

        for label in input_labels:
            labels.append(float(label))

        assert len(points) == len(labels)

        points = np.array(points)
        labels = np.array(labels)

        masks, scores, logits = predictor.predict(
            point_coords=points,
            point_labels=labels,
            multimask_output=True
        )

        sorted_ind = np.argsort(scores)[::-1]
        masks = masks[sorted_ind]
        scores = scores[sorted_ind]
        logits = logits[sorted_ind]

        plt.figure(figsize=(20, 20))
        plt.imshow(resized_image)

        show_masks(resized_image, masks, scores, point_coords=points, input_labels=labels, borders=True)

        plt.axis('off')
        plt.savefig(f'{OUTPUT_DIR}/{filename}', dpi=300)
        plt.show()
