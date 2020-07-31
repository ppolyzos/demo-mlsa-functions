# Demo Functions

![Stateful Wokflows using Azure Durable Functions](assets/img/durable-functions-demo.jpg)

This is a demo for my presentation Stateful Workflows using Azure Durable Functions in http://summeringreece.studentambassadors.gr/.


# Setup 

To start using this project you can either use VS Code or Visual Studio / Jetbrain's Rider.

## Azure Storage Emulator

You can run the `ResizeImages` demo using the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) and having the following containers created:
* `images-to-resize` container where images that needs to be resized are uploaded
* `images-resized` container where images are being stored after `resize-image` function is executed and an image is being resized.

For the `DurableHello` demo you can view execution history in `Tables\TestHubNameHistory` table:


![Azure Storage Emulator](/assets/img/azure-storage-emulator.png)

## VS Code

* [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) extension
* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash)

### Run

To run the functions you can run:

1. `dotnet restore` to restore dependencies
2. `func start --script-root ResizeImages` to run the ResizeImages demo.
3. `func start --script-root DurableHello` to run the DurableHello demo.

## Visual Studio / Rider

Open the `demo-msp-functions.sln`, build the project and run the function project you are interested in.


# Resources
## Postman Collection
You can run the durable hello demo APIs using the resources found in `assets/postman` folder:
* [Postman Collection](/assets/postman/durable-hello-demo.postman_collection.json): imports the API Urls used in the demo and
* [localhost environment](localhost.postman_environment.json) with environment variables to run demo locally.

When you deploy the demo to Azure you can change the `FUNCTIONS_URL` variable with the url from your newlly created Azure Function App.