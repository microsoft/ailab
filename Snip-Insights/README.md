# Snip Insights

An open source cross-platform AI tool for intelligent screen capture. Snip Insights revolutionizes the way users can generate insights from screen captures. The initial prototype of Snip Insights, built for Windows OS and released at Microsoft Build 2018 in May, was created by [Microsoft Garage interns](https://www.microsoft.com/en-us/garage/) based out of Vancouver, BC. Our team at [Microsoft AI Lab](https://ailab.microsoft.com/experiments/32e85f94-3fdd-4a4b-b1ca-9f4cdf47feb6) in collaboration with the Microsoft AI CTO team; took Snip Insights to the next level by giving the tool a new intuitive UX, cross-platform availability (MacOS, Linux, and Windows), and free download and usage under MSA license. Snip Insights leverages Microsoft Azure's Cognitive Services APIs to increase users' productivity by reducing the number of steps needed to gain intelligent insights. 

<p align="center">
  <img width="560" height="300" src="https://github.com/Microsoft/ailab/blob/master/Snip-Insights/Images/SnipInsights_Homepage-Carousel_580x326_V2b.gif">
</p>

## Supported Platforms: Windows, Mac OS and Linux

The Snip Insights app is available for three platforms:

- Universal Windows Platform (UWP)
- Mac OS
- Linux

## Xamarin.Forms GTK# App Mac OS and Linux (Snip Insights)

Xamarin.Forms enables you to build native UIs for iOS, Android, macOS, Linux, and Windows from a single, shared codebase. You can dive into app development with Xamarin.Forms by following our [free self-guided learning from Xamarin University](https://university.xamarin.com/classes/track/self-guided). 

Xamarin.Forms has preview support for GTK# apps. GTK# is a graphical user interface toolkit that links the GTK+ toolkit and a variety of GNOME libraries, allowing the development of fully native GNOME graphics apps using Mono and .NET. [Xamarin.Forms GTK#](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/platform/gtk?tabs=vswin)
 
## Installation

### **Windows**

1. Download the zip for Windows from [here](https://github.com/Microsoft/ailab/releases/tag/1.0.0.0)
2. Install the certificate (".cer" file) according to the instructions detailed on [**Install Certificate**](/Snip-Insights/docs/Windows_Install_Certificate.md) section.
3. Install Snip Insights by double clicking over the .appx package file.

### **Linux**

1. Install **Mono** by following the official steps depending on your Linux distribution: <http://www.mono-project.com/download/stable/#download-lin>
2. Install the .deb package from [here](https://github.com/Microsoft/ailab/releases/tag/1.0.0.0)
3. Launch the app from applications section. 

### **Mac OS**

1. Download and install **Mono** (Stable channel): https://www.mono-project.com/download/stable/#download-mac
   which includes GTK#, the UI toolkit on which Xamarin.Forms relies for this project
2. Install the .pkg as a normal macos application from [here](https://github.com/Microsoft/ailab/releases/tag/1.0.0.0)
3. SnipInsights app is available on Applications section on macos

## Architecture
<p align="center">
  <img width="560" height="300" src="https://github.com/Microsoft/ailab/blob/master/Snip-Insights/Images/SnipInsights_Content_Image.jpg">
</p>

## Requirements

* [Visual Studio 2017 version 15.8 or Visual Studio for Mac version 7.6.3](https://www.visualstudio.com/vs/)

## Using your own subscription

To add the keys to Snip Insights, a Microsoft Garage Project, start the application.  Once running, click/tap the **Settings** icon in the toolbar.  Scroll down until you find the "Cognitive Services, Enable AI assistance" toggle, and toggle it to the **On** position.  You should now see the Insight Service Keys section.

- Entity Search - Create new Entity Search Cognitive Service. Once created, you can display the keys.  Select one and paste into "Settings"
- Image Analysis - In Azure, create a **Computer Vision API ** Cognitive Service and use its key.
- Image Search - In Azure, create a **Bing Search v7 API** Cognitive Service and use its key.
- Text Recognition - You can use the same key as used in Image Analysis. Both Image Analysis and Text Recognition use Computer Vision API.
- Translator - Use the **Translator Text API** Cognitive Service.
- Content Moderator - Use the **Content Moderator API** Cognitive Service.

For the LUIS App ID and Key, you will need to create a Language Understanding application in the Language Understanding Portal ([https://www.luis.ai](https://www.luis.ai))
Use the following steps to create your LUIS App and retrieve an App ID

- Click on **Create new app** button.
- Provide an app name.  Leave Culture (English) and Description as defaults.
- Click **Done**
- In the left navigation pane, click **Entities**
- Click **Manage prebuild entities**
- Select **datetimeV2** and **email**
- Click **Done**
- Click the **Train** button at the top of the page
- Click the **Publish** tab.
- Click the **Publish to production slot** button
- At the bottom of the screen you will see a list with a Key String field.  Click the **Copy** button and paste that key value into the LUIS Key field in settings for Snip Insights
- Click the **Settings** tab (at the top)
- Copy the **Application ID** shown and paste into the LUIS App Id field in Settings for Snip Insights-  

You can now paste each key in the settings panel of the application.
Remember to Click the **Save** button after entering all the keys.

> **NOTE** For each key entered there is a corresponding Service Endpoint.  There are some default endpoints included (You can use as an example) but when you copy each key, also check and replace the Service Endpoint for each service you are using.  You will find the service endpoint for each Cognitive Service on the Overview Page.  Remember to Click the **Save** button after updating all the Service Endpoints.

Congratulations! You should now have a fully working application to get started. Have fun testing the project and thank you for your contribution! 
### Using the code:

For detailed instructions go to see [Using the code](docs/Using_The_Code.md).  
    

## Licenses

This project uses some third-party assets with a license that requires attribution:

- [Xam.Plugins.Settings](https://github.com/jamesmontemagno/SettingsPlugin): by James Montemagno
- [DynamicStackLayout](https://github.com/SuavePirate/DynamicStackLayout): by SuavePirate
- [Nerdbank.GitVersioning](https://github.com/aarnott/Nerdbank.GitVersioning): by Andrew Arnott 
- [Newtonsoft.Json](https://www.newtonsoft.com/json): by James Newton-King
- [Polly](https://github.com/App-vNext/Polly): by Michael Wolfenden
- [Refit](https://github.com/reactiveui/refit): by Paul Betts
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp): by Six Labors

## Code of Conduct

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
