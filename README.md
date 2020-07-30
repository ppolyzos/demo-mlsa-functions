# Demo Functions

This is a demo for my presentation Stateful Workflows using Azure Durable Functions in http://summeringreece.studentambassadors.gr/.


# Setup 

To start using this project you can either use VS Code or Visual Studio / Jetbrain's Rider.

## Azure Storage Emulator

You can run the `ResizeImages` demo using the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) and having the following containers created:
* `images-to-resize` container where images that needs to be resized are uploaded
* `images-resized` container where images are being stored after `resize-image` function is executed and an image is being resized.


## VS Code

* [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) extension
* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash)

### Run

To run the functions you can run:

1. `dotnet restore` to restore dependencies
2. `func start --script-root ResizeImages` to run the ResizeImages demo.

## Visual Studio / Rider

Open the `demo-msp-functions.sln`, build the project and run the function project you are interested in.

