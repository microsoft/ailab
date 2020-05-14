cmake_minimum_required(VERSION 3.0)

#Config
set(AKPATH "C:/Program Files/Azure Kinect SDK v1.4.0")
set(AKBTPATH "C:/Program Files/Azure Kinect Body Tracking SDK")
set(OpenCV_DIR "./3rdparty/opencv/build")

project(KinectMaskGenerator)

find_package(OpenCV REQUIRED)

# If the package has been found, several variables will
# be set, you can find the full list with descriptions
# in the OpenCVConfig.cmake file.
# Print some message showing some of them
message(STATUS "OpenCV library status:")
message(STATUS "    config: ${OpenCV_DIR}")
message(STATUS "    version: ${OpenCV_VERSION}")
message(STATUS "    libraries: ${OpenCV_LIBS}")
message(STATUS "    include path: ${OpenCV_INCLUDE_DIRS}")

set(SOURCE main.cpp)

add_executable(${PROJECT_NAME} ${SOURCE})

# Link your application with OpenCV libraries
target_link_libraries(${PROJECT_NAME} PRIVATE ${OpenCV_LIBS})

#K4A
find_library(AKLIB NAMES k4a HINTS ${AKPATH}/sdk/windows-desktop/amd64/release/lib)
find_library(AKLIBREC NAMES k4arecord HINTS ${AKPATH}/sdk/windows-desktop/amd64/release/lib)
target_include_directories(${PROJECT_NAME} PRIVATE ${AKPATH}/sdk/include)
target_link_libraries(${PROJECT_NAME} PRIVATE ${AKLIB})
target_link_libraries(${PROJECT_NAME} PRIVATE ${AKLIBREC})

#K4ABT
find_library(AKBTLIB NAMES k4abt HINTS ${AKBTPATH}/sdk/windows-desktop/amd64/release/lib)
target_include_directories(${PROJECT_NAME} PRIVATE ${AKBTPATH}/sdk/include)
target_link_libraries(${PROJECT_NAME} PRIVATE ${AKBTLIB})
