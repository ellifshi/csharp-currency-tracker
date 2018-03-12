using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PubNubMessaging.Core;// Exclusively for Pubnub
using Newtonsoft.Json;// For JSON data parsing
using Quartz;
using System.Net;
using System.Collections;
using Newtonsoft.Json.Serialization;// For Schedule Task

namespace CurrencyTrack
{
    public class ScheduledTask : IJob
    {
     
        const string strCHANNELNAME = "exchangedata";
     //   1a142e188a7b4a43e404eee3bbf52378
     //   8eb5b8dcc5334a028e4ef3b746c97f3d
        // Active Access_key
        const string url = "http://apilayer.net/api/live?access_key=8eb5b8dcc5334a028e4ef3b746c97f3d&currencies=EUR,AUD,CNY,INR&source=USD&format=1"; // URL of currencylayer site

        // Not Active Access_key (only to check whether the code can handle Error thrown by Currencylayer site)
        // const string url = "http://apilayer.net/api/live?access_key=819f113d4e79e8d442d1a580d3c3bf80&currencies=EUR,AUD,CNY,INR&source=USD&format=1"; // URL of currencylayer site

        

        /// <summary>
        /// Response from CUrrency Layer is here broken into parts and filled up in respective Lists USDEUR, USDAUD, USDCNY, USDINR. 
        /// </summary>
        /// <param name="context">Job Scheduler Context</param>

        public void Execute(IJobExecutionContext context)
        {

            try
            {
                
                // Get data from source(i.e currencylayer) by calling "_download_serialized_json_data" function
                var data_from_currencylayer = _download_serialized_json_data<CurrencyRates>(url);

                Dictionary<string, decimal> objDictCurrencyQuote = data_from_currencylayer.quotes;

                string strCurrencySuccess = data_from_currencylayer.success;

                int strTimestamp = data_from_currencylayer.TimeStamp; // This is UNIX timestamp (last update time for all currencies)

                // Success message can be true or false
                if (strCurrencySuccess == "False")
                {
                    Dictionary<string, string> objDictCurrencyError = data_from_currencylayer.error;
                    Global.strDispDataError = objDictCurrencyError["info"].ToString();
                }//End if to check the success status of currencylayer
                else
                {

                    StoreAndPublishCurrencyData("USDEUR", objDictCurrencyQuote, strTimestamp);
                    StoreAndPublishCurrencyData("USDAUD", objDictCurrencyQuote, strTimestamp);
                    StoreAndPublishCurrencyData("USDCNY", objDictCurrencyQuote, strTimestamp);
                    StoreAndPublishCurrencyData("USDINR", objDictCurrencyQuote, strTimestamp);

                    Global.LastTimeStamp = strTimestamp;

                }
                
                
            }//End try
            catch(WebException we)
            {
                clsLogWriter.writeEx(we, this);
            }
            catch (Exception ex)
            {
                clsLogWriter.writeEx(ex, this); 
            }//End catch
        }// End Execute()

        /// <summary>
        /// Definition of _download_serialized_json_dat function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL when hit, fetches the json_data. 
        /// The json_data needs to be converted then to make it readable</param>
        /// <returns></returns>

        private static T _download_serialized_json_data<T>(string url) where T : new()
        {
            using (var w = new WebClient())
            {
                //var json_data = string.Empty;
                string json_data = "";

                // attempt to download JSON data as a string
                json_data = w.DownloadString(url);
                
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T(); // where T is CurrencyRates
            }// End using
        }// End download_serialized_json_data


        /// <summary>
        /// This function is called everytime a new currency value is received.
        /// It stores and publishes the new value to all clients. 
        /// </summary>
        /// <param name="currencyLabel">Currency Name</param>
        /// <param name="currencyObj">Currency Data received from CurrencyLayer.com API</param>
        /// <param name="TimeStamp">Latest TimeStamp of the new currency data</param>
        /// <returns></returns>

        void StoreAndPublishCurrencyData(string currencyLabel, Dictionary<string, decimal> currencyObj , int TimeStamp )
        {

            int arrayCount = 0;
            bool isSameValue = false;

            //Get the count of historical values stored
            arrayCount = Global.ArrayDictMap[currencyLabel].Count;// getting the length of an array(for USDEUR, USDAUD, USDCNY, USDINR)

            if (arrayCount >= 1)
            {
                isSameValue = Global.ArrayDictMap[currencyLabel][arrayCount - 1] == currencyObj[currencyLabel];
            }

            if (!isSameValue)
            {

                if (arrayCount >= 30)
                {
                    Global.ArrayDictMap[currencyLabel].RemoveAt(0);
                }

                
                CurrencyData currencyDataPublish = new CurrencyData();

                currencyDataPublish.name = currencyLabel.Substring(3);
                currencyDataPublish.value = currencyObj[currencyLabel].ToString();
                currencyDataPublish.time = TimeStamp;

                try {

                    if (currencyObj[currencyLabel] > Global.ArrayDictMap[currencyLabel][arrayCount - 1])
                    {
                        currencyDataPublish.direction = "+";
                    }
                    else
                    {
                        currencyDataPublish.direction = "-";
                    }

                    currencyDataPublish.magnitude = Math.Abs(currencyObj[currencyLabel] - Global.ArrayDictMap[currencyLabel][arrayCount - 1]);

                }
                catch(ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Can be ignored if list has one or less elements");

                    currencyDataPublish.direction = "+";
                    currencyDataPublish.magnitude = currencyObj[currencyLabel];
                }

                

                Global.ArrayDictMap[currencyLabel].Add(currencyObj[currencyLabel]);



                string json_Publish = JsonConvert.SerializeObject(currencyDataPublish);

                Global._pubnub.Publish<string>(strCHANNELNAME, json_Publish, DisplayReturnMessage, DisplayErrorMessage);
                

            }

        }

        /// <summary>
        /// Callback method for publish status messages
        /// </summary>
        /// <param name="result">Published message</param>

        public static void DisplayReturnMessage(string result)
        {
            Console.WriteLine("PUBLISH STATUS CALLBACK");
            Console.WriteLine(result);
        }// End of DisplayReturnMessage

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="pubnubError">Returns the error message if it fails to get back any result after hitting the URL</param>

        public static void DisplayErrorMessage(PubnubClientError pubnubError)
        {
            Console.WriteLine(pubnubError.StatusCode);
        }// End of DisplayErrorMessage




    }// End of Schedule_Task class

}