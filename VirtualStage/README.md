# Virtual Stage
Virtual Stage lab consists of two applications that allow you to record a lecture using an Azure Kinect device and process the video to remove the background.

## Speaker Recorder App
Speaker Recorder is a Windows application to record from one or two Azure Kinect devices and a PowerPoint file including audio from the default input device.

![Speaker Recorder App](./Images/Kinect1.jpg "Speaker Recorder app screenshot")

### Features
Speaker Recorder is a .Net Core 3 app built using WPF and UWP APIs, that offers the following features:
- Record from up to two Azure Kinect devices.
    - Using [Azure Kinect SDK](https://github.com/microsoft/Azure-Kinect-Sensor-SDK) custom built from pull request [#822](https://github.com/microsoft/Azure-Kinect-Sensor-SDK/pull/822).
- Record a PowerPoint slide show including the audio from the default input device.
    - Using the UWP GraphicsCaptureItem API and NAudio.
- Play all videos to review the result.
    - The player has been implemented using XAML Islands, to benefit from the performance of the UWP MediaPlayerElement.
- Upload the result to Azure Storage (you must configure the ConnectionString in the app.settings.json file).
    - The [Azure Storage Data Movement Library](https://github.com/Azure/azure-storage-net-data-movement) allows us to manage the upload of the recorded files due to their huge size.

### Environment Setup
To build the Speaker Recorder app you need:
- Windows 10 64 bits, version 1903, or a later release
- Windows 10 SDK, version 10.0.18362
- Visual Studio 2019 16.5.5

To run the application, the following prerequisites must be installed:
- Windows 10 64 bits, version 1903, or a later release
- .Net Core 3.1 Desktop Runtime https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.3-windows-x64-installer 
- Visual C++ Redistributable https://aka.ms/vs/16/release/VC_redist.x64.exe