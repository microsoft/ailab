# Sketch2Code Documentation

## Description
Sketch2Code is a solution that uses AI to transform a handwritten user interface design from a picture to valid HTML markup code. 

## Process flow
The process of transformation of a handwritten image to HTML this solution implements is detailed as follows:
1.	The user uploads an image through the website.
2.	A custom vision model predicts what HTML elements are present in the image and their location.
3.	A handwritten text recognition service reads the text inside the predicted elements.
4.	A layout algorithm uses the spatial information from all the bounding boxes of the predicted elements to generate a grid structure that accommodates all.
5.	An HTML generation engine uses all these pieces of information to generate an HTML markup code reflecting the result.

## Architecture
The Sketch2Code solution uses the following elements:
-	A Microsoft Custom Vision Model: This model has been trained with images of different handwritten designs tagging the information of most common HTML elements like Buttons, TextBox and Image.
-	A Microsoft Computer Vision Service: To identify the text written into a design element a Computer Vision Service is used.
-	An Azure Blob Storage: All steps involved in the HTML generation process are stored, including original image, prediction results and layout grouping information. 
-	An Azure Function: Serves as the backend entry point that coordinates the generation process by interacting with all the services.
-	An Azure WebSite: User font-end to enable uploading a new design and see the generated HTML results.
This elements form the architecture as follows:
![Sketch2Code Architecture](https://github.com/Microsoft/ailab/blob/master/Sketch2Code/images/architecture.png)



## How to configure the solution

### Microsoft Custom Vision Model
The training set used to create the sample model used in the project is located in the Model folder. Each training image has a unique identifier that matches information contained in the dataset.json file. This file contains all the tag information used to train the sample model.
To create your own model you can use this dataset to start and using the Custom Vision API upload this dataset to your own project.
You can create your Custom Vision Project at https://customvision.ai
Once you have created your Custom Vision Project you need to annotate the Key and the Project Name to configure the Azure Function to call use the service.

### Computer Vision Service
A Microsoft Computer Vision Service is needed to perform handwritten character recognition.
To create this service go to your Azure Subscription and create your own service. Annotate the Endpoint and the Key to complete the configuration of the solution.

### Azure Blob Storage
An Azure Blob Storage is used to store all the intermediary steps of the process.
A new folder is created for each generation process with the following contents:
-	\slices: Contains the cropped images used for text prediction.
-	Original.png: image uploaded by the user.
-	results.json: results from the prediction process run against the original image.
-	groups.json: results from the layout algorithm containing the spatial distribution of predicted objects.

### Azure function
The code for the provided Azure Function is located in the Sketch2Code.Api folder. You can use this code to create your own function and must define the following configuration parameters:
-	AzureWebJobsStorage: Endpoint to the Azure Storage.
-	ComputerVisionDelay: Time in milliseconds to wait between calls to the computer vision service. Sample works with 120ms.
-	HandwrittenTextApiEndpoint: Endpoint to the Computer Vision Service.
-	HandwrittenTextSubscriptionKey: Key to access the Computer Vision Service.
-	ObjectDetectionIterationName: Name of the training iteration to use in the prediction model.
-	ObjectDetectionPredictionKey: Prediction Key to access the Custom Vision Service.
-	ObjectDetectionProjectName: Name of the custom vision project.
-	ObjectDetectionTrainingKey: Training key for the custom vision service.
-	Probability: Probability threshold to consider a successful object detection. Below this value predictions are not considered. Sample model works with 40. 

### Azure Website
The Sketch2Code.Web contains the code used to implement the front-end website. Two parameters must be configured:
-	Sketch2CodeAppFunctionEndPoint: Endpoint to the Azure Function
-	storageUrl: Url for the Azure Storage. 


