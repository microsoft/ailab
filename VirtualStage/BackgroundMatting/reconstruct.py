import os


def reconstruct_all_video(videos, output_dir, suffix, outputs_list):
    print(f"Reconstructing {len(videos)} output videos")
    for i, video in enumerate(videos):
        reconstruct_video(video, output_dir, suffix, outputs_list)


def reconstruct_video(video, output_dir, suffix, outputs_list):
    # video not split
    out_path = os.path.join(output_dir, os.path.basename(video))

    for o in outputs_list:
        if not os.path.exists(f"{out_path}_{o}{suffix}.mp4"):

            # set up output timestamp file
            write_output_timestamp_file(
                video, out_path + suffix, os.path.basename(video) + suffix, o,
            )

            outp = (out_path + suffix).replace("\\", "/")
            code = os.system(
                f"ffmpeg -f concat -i {os.path.join(outp, 'timestampfile.txt')} "
                f"-vcodec libx265 -x265-params lossless=1 -crf 0 -pix_fmt yuv420p "
                f"{out_path}_{o}{suffix}.mp4"
                " > bg_matting_logs.txt 2>&1"
            )
            if code != 0:
                exit(code)

    print(f" Reconstructed {video} output")


def reconstruct_all_color(videos, output_dir, suffix):
    print(f"Reconstructing {len(videos)} original color videos")
    for i, video in enumerate(videos):
        out_path = os.path.join(output_dir, os.path.basename(video))
        if not os.path.exists(f"{out_path}_color{suffix}.mp4"):
            # set up output timestamp file
            write_input_timestamp_file(video)

            outp = video.replace("\\", "/")
            code = os.system(
                f"ffmpeg -f concat -i {os.path.join(outp, 'timestampfile_color.txt')} "
                f"-vcodec libx265 -x265-params lossless=1 -crf 0 -pix_fmt yuv420p "
                f"{out_path}_color{suffix}.mp4"
                " > bg_matting_logs.txt 2>&1"
            )
            if code != 0:
                exit(code)


def write_output_timestamp_file(input, output, output_dir, output_suffix):
    ts_in = open(os.path.join(input, "timestampfile.txt"), "rt")
    ts_out = open(os.path.join(output, "timestampfile.txt"), "wt")
    for line in ts_in:
        ts_out.write(
            line.replace("file ", "file " + output_dir + "/").replace(
                "out", output_suffix
            )
        )
    ts_in.close()
    ts_out.close()


def write_input_timestamp_file(input):
    ts_in = open(os.path.join(input, "timestampfile.txt"), "rt")
    ts_out = open(os.path.join(input, "timestampfile_color.txt"), "wt")
    for line in ts_in:
        ts_out.write(
            line.replace("file ", "file " + os.path.basename(input) + "/").replace(
                "out", "img"
            )
        )
    ts_in.close()
    ts_out.close()
