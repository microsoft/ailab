import time

import os

from app_parser import get_parser
from prepare_data import prepare_videos
from reconstruct import reconstruct_all_video, reconstruct_all_color
from fixed_threshold import fixed_split, fixed_merge

start = time.time()

# parse arguments
args = get_parser().parse_args()

video_start = int(args.start)
video_duration = args.duration
EXT = "." + args.input_extension
if args.kinect_mask:
    EXT = ".mkv"
path = args.input
original_videos = [
    os.path.join(path, f[:-4]) for f in os.listdir(path) if f.endswith(EXT)
]
mask_suffix = "_masksDL"
output_suffix = f"_{args.name}"
if args.kinect_mask:
    mask_suffix = "_masksAK"

outputs = args.output_types.split(",")

op_default = "erode,3,10;dilate,5,5;blur,31,0"

# Prepare the videos to process
prepare_videos(original_videos, EXT, video_start, video_duration, args.kinect_mask)

# Split videos as specified by thesholds
overlap = 5
thresholds = [""] * len(original_videos)
if args.fixed_threshold is not None:
    thresholds = args.fixed_threshold.split(",")

    fixed_split(original_videos, thresholds, mask_suffix, overlap=overlap)

videos = []
for i, video in enumerate(original_videos):
    if i >= (len(thresholds)) or not thresholds[i]:
        videos.append(video)
    else:
        videos.append(video + "_up")
        videos.append(video + "_dw")

# Prepare morphological operations that will be applied to masks
mask_ops = args.mask_ops.split(":")
defined = len(mask_ops)
for i in range(len(videos)):
    if i < defined:
        if mask_ops[i] == "":
            # default value
            mask_ops[i] = op_default
        elif mask_ops[i] == "-":
            # no operations
            mask_ops[i] = ""
    else:
        mask_ops.append(op_default)
print(f"Morph ops to apply: {mask_ops}")

# Do the inference
print(f" Doing inference on {len(videos)} input videos")
from background_matting_image import inference  # noqa: E402

for i, video in enumerate(videos):
    fixed_back = video + ".png"
    out_path = os.path.join(args.output_dir, os.path.basename(video) + output_suffix)
    if not os.path.exists(out_path):
        inference(
            out_path,
            video,
            sharpen=args.sharpen,
            target_back=args.background,
            mask_ops=mask_ops[i],
            back=fixed_back,
            mask_suffix=mask_suffix,
            outputs=outputs,
        )
        print(f"Inference done on {video} ({i+1}/{len(videos)})")

# Merge the outputs of the videos that were splited
if args.fixed_threshold is not None:
    fixed_merge(
        original_videos,
        thresholds,
        args.output_dir,
        output_suffix,
        outputs,
        overlap=overlap,
    )

# Generate output videos
reconstruct_all_video(original_videos, args.output_dir, output_suffix, outputs)
reconstruct_all_color(original_videos, args.output_dir, output_suffix)

end = time.time()

elapsed = end - start
elapsed_str = f"{elapsed} seconds"
if elapsed > 60:
    elapsed /= 60
    elapsed_str = f"{elapsed} minutes"
if elapsed > 60:
    elapsed /= 60
    elapsed_str = f"{elapsed} hours"
print(f"Total elapsed time: {elapsed_str}")
