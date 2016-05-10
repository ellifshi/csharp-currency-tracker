# Realtime Currency Tracker Dashboard using PubNub and C#/.NET


## Introduction
A web based micro dashboard for displaying currency exchange rates using PubNub C# SDK and JS SDK. It displays the 
current currency conversion rate for EUR, AUD, CYN and INR against USD. The application source code is divided into 
a server part which is under [server](server/CurrencyTrack) location, and client part which is under [client](client) location.

##Setup and Build

### Server
Clone the repository and open the server source code in Visual Studio editor. The source code folder is a VS solution which can be
opened by choosing this [file](server/CurrencyTrack/CurrencyTrack.sln).

Before building the solution , make sure that you have

1. PubNub publish and subscribe keys. Update the [web.config](server/CurrencyTrack/CurrencyTrack/Web.config) file
with the publish and subscribe keys.

      Update the publish key [here](server/CurrencyTrack/CurrencyTrack/Web.config#L33)
      
      Update the subscribe key [here](server/CurrencyTrack/CurrencyTrack/Web.config#L34)

2. CurrencyLayer.com subscription. Update the following line in the source code with your currencylayer API keys
      https://github.com/shyampurk/currency-tracker-csharp/blob/master/server/CurrencyTrack/CurrencyTrack/ScheduledTask.cs#L21

      Update  the access_key URL parameter with your currencylayer.com subscription key 

Build the solution from within Visual Studio. You will get a message in the Output window as follows

      1>------ Build started: Project: CurrencyTrack, Configuration: Debug Any CPU ------
      1>  CurrencyTrack -> <YOUR_PATH>\CurrencyTrack\CurrencyTrack\bin\CurrencyTrack.dll
      ========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========

      where YOUR_PATH is the local path of the solution folder in your build system 



### Client

Update the javascript source code with the same set of PubNub keys that you have used at the server.


Update the publish key [here](client/js/index.js#L2)

Update the subscribe key [here](client/js/index.js#L3)


## Run the Demo

Step 1 - Run the server application either within Visual Studio or by uploading it to Azure cloud. 

Step 2 - Run the client app by opening the [main html file](client/main.html) in browser.

Allow the application to run for a few hours. You will see updated currency convertion values getting displayed in the client 
dashboard every couple of hours ( depending on the time and day of the week). Once the application is running for a few hours, 
you can also see the historical currency rate trends by clicking on the 'Trend' button against each currency in the dashboard. 



