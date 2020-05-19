import argparse


def get_parser():
    """Returns an arguments parser"""
    parser = argparse.ArgumentParser(description="Background Matting on Videos.")
    parser.add_argument(
        "-n", "--name", type=str, required=True, help="Name of processing."
    )
    parser.add_argument(
        "-i", "--input", type=str, required=True, help="Path to videos folder."
    )
    parser.add_argument(
        "-o",
        "--output_dir",
        type=str,
        required=True,
        help="Directory to save the output results.",
    )
    parser.add_argument(
        "-s",
        "--start",
        type=str,
        default="00",
        help="Start point to process (in seconds). Default 0.",
    )
    parser.add_argument(
        "-d",
        "--duration",
        type=str,
        default="-1",
        help="Duration of the processed video (in seconds). Set to '-1' to process till"
        " the end of the video. Default '-1'.",
    )

    parser.add_argument(
        "-pt",
        "--proportional_threshold",
        type=str,
        default=None,
        help="Comma separated list to set which videos must be split in two according"
        " to a proportional threshold before processing. For example, for two videos "
        "where we want to split the first one (alphabetical order) at the person half,"
        ' set this parameter to "0.5" or "0.5,',
    )
    parser.add_argument(
        "-ft",
        "--fixed_threshold",
        type=str,
        default=None,
        help="Colon separated list to set which videos must be split in two by a fixed"
        " threshold before processing. For example, for two videos where we want to "
        "split the first one (alphabetical order) by the line 640, set this parameter "
        'to "640" or "640:".',
    )
    parser.add_argument(
        "-shp",
        "--sharpen",
        type=bool,
        default=False,
        help="Enable sharpen original color image to help on noise removal. Default to "
        "False.",
    )
    parser.add_argument(
        "-mask_ops",
        "--mask_ops",
        type=str,
        default="",
        help="Morphological operations to apply to masks for each video. Format "
        "[erode|dilate],KernelSize,Iterations;...;blur,KernelSize,SigmaX:[...]. Default"
        ' "erode,3,10;dilate,5,5;blur,31,0" for each video. Specify "-" to not use any '
        "morphological operation on the mask.",
    )
    parser.add_argument(
        "-o_types",
        "--output_types",
        type=str,
        default="out,matte",
        help="Output types generation separated by comma. Valid values are out,matte,"
        'compose,fg. Default "out,matte".',
    )
    parser.add_argument(
        "-bg",
        "--background",
        type=str,
        help="Path to background video directory for compose output.",
    )
    parser.add_argument(
        "--kinect_mask",
        dest="kinect_mask",
        action="store_true",
        help="Enable the use of azure kinect mask (Default).",
    )
    parser.add_argument(
        "--no_kinect_mask",
        dest="kinect_mask",
        action="store_false",
        help="Disble the use of azure kinect mask.",
    )
    parser.set_defaults(kinect_mask=True)
    parser.add_argument(
        "-i_ext",
        "--input_extension",
        type=str,
        default="mp4",
        help="Input videos extension. Only if not using Azure Kinect videos.",
    )

    return parser
