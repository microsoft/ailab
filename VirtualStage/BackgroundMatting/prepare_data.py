import os


def prepare_videos(
    videos, extension, start, duration, kinect_mask=True, width=1920, height=1080
):
    video_start_secs = start % 60
    video_start_mins = start // 60
    print(f"Dumping frames and segmenting {len(videos)} input videos")
    for i, video in enumerate(videos):
        try:
            os.makedirs(video)
        except FileExistsError:
            continue

        print(f"Dumping frames from {video} ({i+1}/{len(videos)})...")
        ffmpeg_duration = ""
        if duration != "-1":
            ffmpeg_duration = f"-t {duration}"
        code = os.system(
            f"ffmpeg -y -ss 00:{video_start_mins:02}:{video_start_secs:02}.000 "
            f"-vsync 0 "
            f"-i {video}{extension} -vf scale={width}:{height} "
            f"-map 0:0 {ffmpeg_duration} {video}/%04d_img.png -hide_banner"
            f" > bg_matting_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)

        print(f"Segmenting frames...")
        if kinect_mask:
            code = os.system(
                f"KinectMaskGenerator.exe {video}{extension} {video} {start} {duration}"
                f" > segmentation_logs_{i}.txt 2>&1"
            )
            if code != 0:
                exit(code)
        else:
            code = os.system(
                f"python segmentation_deeplab.py -i {video}"
                f" > segmentation_logs_{i}.txt 2>&1"
            )
            if code != 0:
                exit(code)

        print(f"Extracting background...")
        code = os.system(
            f"ffmpeg -y -i {video}{extension} -vf scale={width}:{height} "
            f"-map 0:0 -ss 00:00:02.000 -vframes 1 {video}.png -hide_banner"
            " > bg_matting_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)
