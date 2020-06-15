import os


def fixed_split(videos, thresholds, mask_suffix, overlap=0):
    print(f"Splitting {len(videos)} videos vertically by a fixed threshold")

    for i, video in enumerate(videos):
        if i >= (len(thresholds)) or not thresholds[i]:
            continue

        try:
            os.makedirs(video + "_up")
            os.makedirs(video + "_dw")
        except FileExistsError:
            continue

        threshold = int(thresholds[i])
        iup_region = f"iw:{threshold + overlap}:0:0"
        idw_region = f"iw:ih-{threshold}+{overlap}:0:{threshold - overlap}"

        # crop color images
        code = os.system(
            f"ffmpeg -i {os.path.join(video, '%04d_img.png')} "
            f'-filter:v "crop={iup_region}" '
            f"{os.path.join(video+'_up', '%04d_img.png')}"
            " > split_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)
        code = os.system(
            f"ffmpeg -i {os.path.join(video, '%04d_img.png')} "
            f'-filter:v "crop={idw_region}" '
            f"{os.path.join(video+'_dw', '%04d_img.png')}"
            " > split_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)

        # crop mask images
        code = os.system(
            f"ffmpeg -i {os.path.join(video, '%04d')}{mask_suffix}.png "
            f'-filter:v "crop={iup_region}" '
            f"{os.path.join(video+'_up', '%04d')}{mask_suffix}.png"
            " > split_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)
        code = os.system(
            f"ffmpeg -i {os.path.join(video, '%04d')}{mask_suffix}.png "
            f'-filter:v "crop={idw_region}" '
            f"{os.path.join(video+'_dw', '%04d')}{mask_suffix}.png"
            " > split_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)

        # crop background image
        code = os.system(
            f"ffmpeg -y -i {video+'.png'} "
            f"-filter:v \"crop={iup_region}\" {video+'_up.png'}"
            " > split_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)
        code = os.system(
            f"ffmpeg -y -i {video+'.png'} "
            f'-filter:v "crop={idw_region}" '
            f"{video+'_dw.png'}"
            " > split_logs.txt 2>&1"
        )
        if code != 0:
            exit(code)

        print(f" Splitted {video} ({i+1}/{len(videos)})")


def fixed_merge(videos, factors, output_dir, suffix, outputs_list, overlap=0):
    print(f"Reconstructing {len(videos)} output images")
    for i, video in enumerate(videos):
        if i < (len(factors)) and factors[i]:
            # video split, merging
            out_path = os.path.join(output_dir, os.path.basename(video)).replace(
                "\\", "/"
            )

            try:
                os.makedirs(out_path + suffix)
            except FileExistsError:
                continue

            outpup = (out_path + "_up" + suffix).replace("\\", "/")
            outpdw = (out_path + "_dw" + suffix).replace("\\", "/")

            for o in outputs_list:
                code = os.system(
                    f"ffmpeg -i {outpup}/%04d_{o}.png -i {outpdw}/%04d_{o}.png "
                    f'-filter_complex "[0:0]crop=iw:ih-{overlap}:0:0[v0];'
                    f"[1:0]crop=iw:ih-{overlap}:0:{overlap}[v1];"
                    f'[v0][v1]vstack" '
                    f"{out_path + suffix}/%04d_{o}.png -hide_banner"
                    " > merge_logs.txt"
                )
                if code != 0:
                    exit(code)

            print(f" Merged {video} ({i+1}/{len(videos)})")
