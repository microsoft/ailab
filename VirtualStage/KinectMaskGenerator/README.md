# Kinect Mask Generator

Command line tool to generate body mask images from an Azure Kinect video. It also generates
a PTS (presentation time stamp) file that can be consumed by _ffmpeg_ to create an
alpha video.

## Requirements

* OpenCV 4.3.0
  * Download [OpenCV 4.3.0](https://sourceforge.net/projects/opencvlibrary/files/4.3.0/opencv-4.3.0-vc14_vc15.exe/download)
  * Install on _3rdparty/opencv_ directory
  * Add to Path `[ROOT]\3rdparty\opencv\build\x64\vc15\bin`
* Azure Kinect SDK 1.4.0
  * [Download](https://download.microsoft.com/download/4/5/a/45aa3917-45bf-4f24-b934-5cff74df73e1/Azure%20Kinect%20SDK%201.4.0.exe) and install
  * Add to `PATH` the bin folder `C:\Program Files\Azure Kinect SDK v1.4.0\sdk\windows-desktop\amd64\release\bin`
* Azure Kinect Body Traking SDK 1.0.1
  * [Download](https://www.microsoft.com/en-us/download/details.aspx?id=100942) and install
  * Add to `PATH` the bin folder `C:\Program Files\Azure Kinect Body Tracking SDK\sdk\windows-desktop\amd64\release\bin`

## Build

The project uses CMake

## Usage

`./build/KinectmaskGenerator.exe VIDEO_INPUT_PATH MASK_OUTPUT_PATH START DURATION [COLOR_IMG_SUFFIX]`

* `START` and `DURATION` are specified in seconds
* Set `DURATION` to `-1` to process the video until the end
* If `COLOR_IMG_SUFFIX` is set, it also generates color images with the suffix provided
