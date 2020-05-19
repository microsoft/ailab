#pragma comment(lib, "k4a.lib")
#define IMG_OFFSET 0

#include <iostream>

#include <sys/types.h>
#include <sys/stat.h>

#include <k4a/k4a.h>
#include <k4arecord/playback.h>
#include <k4abt.h>

#include <opencv2/opencv.hpp>
#include <opencv2/core.hpp>
#include <opencv2/core/utility.hpp>
#include <opencv2/highgui.hpp>

#include <opencv2/imgproc.hpp>
#include <fstream>

using namespace cv;

struct stat info;

#define VERIFY(result, error)                                                                        \
  if (result != K4A_RESULT_SUCCEEDED)                                                                \
  {                                                                                                  \
    printf("%s \n - (File: %s, Function: %s, Line: %d)\n", error, __FILE__, __FUNCTION__, __LINE__); \
    exit(1);                                                                                         \
  }

#define WAIT_VERIFY(result, error)                                                                   \
  if (result != K4A_WAIT_RESULT_SUCCEEDED)                                                           \
  {                                                                                                  \
    printf("%s \n - (File: %s, Function: %s, Line: %d)\n", error, __FILE__, __FUNCTION__, __LINE__); \
    exit(1);                                                                                         \
  }

static cv::Mat color_to_opencv(const k4a_image_t &im);
static cv::Mat depth_to_opencv(const k4a_image_t &im);
static cv::Mat index_to_opencv(const k4a_image_t &im);

const uint64_t default_interval = 66666;
uint64_t last_duration;
uint64_t current_interval;

struct SyncronizationFileWriter
{
  std::ofstream timestampFile;

  void init(std::string outputFolder)
  {
    std::stringstream filename;
    filename << outputFolder << "/timestampfile.txt";
    timestampFile.open(filename.str().c_str());
  }

  void writeStamp(int frame_count)
  {
    std::stringstream ssaux;
    ssaux << std::setfill('0') << std::setw(4) << frame_count << "_out.png";
    timestampFile << "file " << ssaux.str() << std::endl;
    timestampFile << "duration " << current_interval / 1e6 << std::endl;
  }

  void close()
  {
    timestampFile.close();
  }
};

int main(int argc, char *argv[])
{
  if (argc < 5)
  {
    printf("USAGE: VIDEO_INPUT_PATH MASK_OUTPUT_PATH START DURATION [COLOR_IMG_SUFFIX]");
    exit(1);
  }

  char *INPUT = argv[1];
  char *OUTPUT = argv[2];

  uint64_t timestamp_start_us = (uint64_t)std::stoi(argv[3]) * (uint64_t)1e6;
  printf("Starting at second %f\n", timestamp_start_us / 1e6);
  int duration = std::stoi(argv[4]);
  uint64_t timestamp_end_us = 0;
  if (duration > 0)
  {
    timestamp_end_us = (uint64_t)std::stoi(argv[4]) * (uint64_t)1e6 + timestamp_start_us;
    printf("Finishing at second %f\n", timestamp_end_us / 1e6);
  }

  bool write_color = false;
  char *color_img_suffix = "";
  if (argc >= 6)
  {
    write_color = true;
    color_img_suffix = argv[5];
  }

  SyncronizationFileWriter syncFileWriter;
  syncFileWriter.init(OUTPUT);

  k4a_playback_t playback_handle = NULL;
  VERIFY(k4a_playback_open(INPUT, &playback_handle), "Failed to open recording\n");

  uint64_t recording_length = k4a_playback_get_recording_length_usec(playback_handle);
  printf("Recording is %lld seconds long\n", recording_length / 1000000);

  k4a_calibration_t sensor_calibration;
  VERIFY(k4a_playback_get_calibration(playback_handle, &sensor_calibration),
         "Get camera calibration failed!");

  k4a_playback_set_color_conversion(playback_handle,
                                    K4A_IMAGE_FORMAT_COLOR_BGRA32);

  // Create the tracker
  k4abt_tracker_t tracker = NULL;
  k4abt_tracker_configuration_t tracker_config = K4ABT_TRACKER_CONFIG_DEFAULT;
  VERIFY(k4abt_tracker_create(&sensor_calibration, tracker_config, &tracker), "Body tracker initialization failed!");

  int frame_count = 0;
  char buffer[120];
  int color_image_width_pixels = sensor_calibration.color_camera_calibration.resolution_width;
  int color_image_height_pixels = sensor_calibration.color_camera_calibration.resolution_height;
  int depth_image_width_pixels = sensor_calibration.depth_camera_calibration.resolution_width;
  int depth_image_height_pixels = sensor_calibration.depth_camera_calibration.resolution_height;
  k4a_image_t mask_image = NULL;
  k4a_image_t transformed_mask_image = NULL;
  k4a_image_t transformed_depth_image = NULL;
  // transform depth camera into color camera geometry
  VERIFY(k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM8,
                          color_image_width_pixels,
                          color_image_height_pixels,
                          color_image_width_pixels * (int)sizeof(uint8_t),
                          &transformed_mask_image),
         "Could not create image for mask transformation!");

  VERIFY(k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16,
                          color_image_width_pixels,
                          color_image_height_pixels,
                          color_image_width_pixels * (int)sizeof(uint16_t),
                          &transformed_depth_image),
         "Could not create image for depth transformation!");
  k4a_transformation_t transformation = k4a_transformation_create(&sensor_calibration);
  Mat last_mask = cv::Mat::zeros(cv::Size(1920, 1080), CV_8U);
  bool writing = false;
  uint64_t initial_duration = 0;

  while (true)
  {
    frame_count++;

    // Get a capture
    k4a_capture_t sensor_capture;
    k4a_stream_result_t get_capture_result = k4a_playback_get_next_capture(playback_handle, &sensor_capture);
    if (get_capture_result == K4A_STREAM_RESULT_EOF)
    {
      break;
    }
    else if (get_capture_result != K4A_STREAM_RESULT_SUCCEEDED)
    {
      if (writing)
      {
        // If the frame is corrupt, we write last interval timestamp and last mask
        printf("Warning: Get capture returned error %d on frame %d\n", get_capture_result, frame_count);
        snprintf(buffer, 128, "%s/%04d_masksAK.png", OUTPUT, frame_count);
        cv::imwrite(buffer, last_mask);
        syncFileWriter.writeStamp(frame_count);
      }
      continue;
    }

    // if there is no color image, we just consider that frame didn't exist
    k4a_image_t color_image = k4a_capture_get_color_image(sensor_capture);
    if (color_image == 0)
    {
      printf("Warning: No color image %d on frame %d, dropped\n", get_capture_result, frame_count);
      frame_count--;
      k4a_capture_release(sensor_capture);
      continue;
    }

    // write color image
    if (writing && write_color)
    {
      Mat color = color_to_opencv(color_image);
      snprintf(buffer, 128, "%s/%04d_%s.png", OUTPUT, frame_count, color_img_suffix);
      cv::imwrite(buffer, color);
    }

    // get frame timestamp information
    uint64_t duration_us = k4a_image_get_device_timestamp_usec(color_image);
    k4a_image_release(color_image);
    if (initial_duration == 0)
    { // get the first duration to compare with the rest
      initial_duration = duration_us;
    }
    current_interval = duration_us - last_duration;
    last_duration = duration_us;
    // sometimes first timestamp is wrong, in that case we use default
    if (current_interval / 1e6 > 5)
    {
      current_interval = default_interval;
    }

    // check if we are in the interested piece of video
    if (!writing && (duration_us - initial_duration) >= timestamp_start_us)
    {
      writing = true; // start writing
      frame_count = 1;
    }
    else if (writing && duration > 0 && (duration_us - initial_duration) >= timestamp_end_us)
    {
      break; // done
    }

    // don't queue if we are to too far from the start
    if (!writing && (duration_us - initial_duration) < (timestamp_start_us - int(1e6)))
    {
      k4a_capture_release(sensor_capture);
      continue;
    }

    // queque capture
    k4a_wait_result_t queue_capture_result = k4abt_tracker_enqueue_capture(tracker, sensor_capture, K4A_WAIT_INFINITE);
    k4a_capture_release(sensor_capture);
    if (queue_capture_result == K4A_WAIT_RESULT_TIMEOUT)
    {
      // It should never hit timeout when K4A_WAIT_INFINITE is set.
      printf("Error! Add capture to tracker process queue timeout!\n");
      exit(1);
    }
    else if (queue_capture_result == K4A_WAIT_RESULT_FAILED)
    {
      if (writing)
      {
        // If for some reason (probably there is no depth information) we don't have a mask for
        //   this frame, we use the last one
        printf("Warning! Add capture to tracker process queue failed! Frame: %d\n", frame_count);

        snprintf(buffer, 128, "%s/%04d_masksAK.png", OUTPUT, frame_count);
        cv::imwrite(buffer, last_mask);
        syncFileWriter.writeStamp(frame_count);
      }
      continue;
    }

    // get capture for body tracker
    k4abt_frame_t body_frame = NULL;
    k4a_wait_result_t pop_frame_result = k4abt_tracker_pop_result(tracker, &body_frame, K4A_WAIT_INFINITE);
    if (pop_frame_result == K4A_WAIT_RESULT_TIMEOUT)
    {
      // It should never hit timeout when K4A_WAIT_INFINITE is set.
      printf("Error! Pop body frame result timeout!\n");
      exit(1);
    }
    else if (pop_frame_result != K4A_WAIT_RESULT_SUCCEEDED)
    {
      printf("Pop body frame result failed!\n");
      exit(1);
    }

    // go to next iteration if we are not writing
    if (!writing)
    {
      k4abt_frame_release(body_frame);
      continue;
    }

    // Process and write mask
    k4a_image_t body_index_map = k4abt_frame_get_body_index_map(body_frame);
    k4a_capture_t input_capture = k4abt_frame_get_capture(body_frame);
    k4a_image_t depth_image = k4a_capture_get_depth_image(input_capture);

    // transform body index into mask
    Mat depth_mask = index_to_opencv(body_index_map);
    cv::bitwise_not(depth_mask, depth_mask);

    VERIFY(k4a_image_create_from_buffer(
               K4A_IMAGE_FORMAT_CUSTOM8,
               depth_image_width_pixels,
               depth_image_height_pixels,
               depth_image_width_pixels * (int)sizeof(uint8_t),
               depth_mask.data,
               (size_t)(depth_mask.total() * depth_mask.elemSize()),
               NULL,
               NULL,
               &mask_image),
           "Could not create image from buffer!");

    VERIFY(k4a_transformation_depth_image_to_color_camera_custom(
               transformation,
               depth_image,
               mask_image,
               transformed_depth_image,
               transformed_mask_image,
               K4A_TRANSFORMATION_INTERPOLATION_TYPE_NEAREST,
               0),
           "Coud not transform depth image!");

    // Get mask, depth and color images
    Mat mask = index_to_opencv(transformed_mask_image);

    Mat rs_mask;
    cv::resize(mask, rs_mask, cv::Size(1920, 1080));
    snprintf(buffer, 128, "%s/%04d_masksAK.png", OUTPUT, frame_count);
    last_mask = rs_mask;
    cv::imwrite(buffer, rs_mask);
    syncFileWriter.writeStamp(frame_count);

    //imshow("BodyTracking", mask);
    //waitKey(1);

    k4a_image_release(mask_image);
    k4a_image_release(depth_image);
    k4a_image_release(body_index_map);
    k4a_capture_release(input_capture);

    k4abt_frame_release(body_frame);
  }

  printf("Finished body tracking processing!\n");

  // Clean up body tracker
  k4abt_tracker_shutdown(tracker);
  k4abt_tracker_destroy(tracker);

  k4a_playback_close(playback_handle);

  syncFileWriter.close();
  return 0;
}

static cv::Mat color_to_opencv(const k4a_image_t &im)
{
  cv::Mat cv_image_with_alpha(k4a_image_get_height_pixels(im), k4a_image_get_width_pixels(im), CV_8UC4, (void *)k4a_image_get_buffer(im));
  cv::Mat cv_image_no_alpha;
  cv::cvtColor(cv_image_with_alpha, cv_image_with_alpha, cv::COLOR_BGRA2BGR);
  return cv_image_with_alpha;
}

static cv::Mat depth_to_opencv(const k4a_image_t &im)
{
  return cv::Mat(k4a_image_get_height_pixels(im),
                 k4a_image_get_width_pixels(im),
                 CV_16U,
                 (void *)k4a_image_get_buffer(im),
                 static_cast<size_t>(k4a_image_get_stride_bytes(im)));
}

static cv::Mat index_to_opencv(const k4a_image_t &im)
{
  return cv::Mat(k4a_image_get_height_pixels(im),
                 k4a_image_get_width_pixels(im),
                 CV_8U,
                 (void *)k4a_image_get_buffer(im),
                 static_cast<size_t>(k4a_image_get_stride_bytes(im)));
}
