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
using System.Web.Script.Serialization;//JSON Deserialization

namespace CurrencyTrack
{
    public class ScheduledTask : IJob
    {
        const string key_curr = "";

        
        const string strCHANNELNAME = "exchangedata";
        const string strCHANNELNAME_Subscribe = "trendRequestChannel";
        const string strCHANNELNAMESubscribe = "trendRequestChannel";
        const string strPUBLISH_KEY = "pub-c-913ab39c-d613-44b3-8622-2e56b8f5ea6d";
        const string strSUBSCRIBE_KEY = "sub-c-8ad89b4e-a95e-11e5-a65d-02ee2ddab7fe";
        

        // Active Access_key
        const string url = "http://apilayer.net/api/live?access_key=1a142e188a7b4a43e404eee3bbf52378&currencies=EUR,AUD,CNY,INR&source=USD&format=1"; // URL of currencylayer site

        // Not Active Access_key (only to check whether the code can handle Error thrown by Currencylayer site)
        // const string url = "http://apilayer.net/api/live?access_key=819f113d4e79e8d442d1a580d3c3bf80&currencies=EUR,AUD,CNY,INR&source=USD&format=1"; // URL of currencylayer site

        

        /// <summary>
        /// json_data is here broken into parts and filled up in respective arrays USDEUR, USDAUD, USDCNY, USDINR. 
        /// The array elements are displayed later on inside strDispData
        /// </summary>
        /// <param name="context"></param>

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
        /// Method where Publish key & Subscribe keys are declared. It is the method from where the Pubnub channel is established.
        /// </summary>

            /*
        void Init1()
        {
            _pubnub = new Pubnub(strPUBLISH_KEY, strSUBSCRIBE_KEY);
        }// End of Init1()
        */
        

        void StoreAndPublishCurrencyData(string currencyLabel, Dictionary<string, decimal> currencyObj , int TimeStamp )
        {

            int arrayCount = 0;
            bool isSameValue = false;

            //Get the count of historical values stored
            arrayCount = Global.ArrayDictMap[currencyLabel].Count;// getting the length of an array(for USDEUR, USDAUD, USDCNY, USDINR)

            if (arrayCount >= 1)
            {
                isSameValue = Global.ArrayDictMap[currencyLabel][arrayCount - 1].ToString() == currencyObj[currencyLabel].ToString();
            }

            if (!isSameValue)
            {

                if (arrayCount >= 30)
                {
                    Global.ArrayDictMap[currencyLabel].RemoveAt(0);
                }

                Global.ArrayDictMap[currencyLabel].Add(currencyObj[currencyLabel]);

                CurrencyData currencyDataPublish = new CurrencyData();

                currencyDataPublish.requestType = 0;
                currencyDataPublish.name = currencyLabel.Substring(3);
                currencyDataPublish.value = currencyObj[currencyLabel].ToString();
                currencyDataPublish.time = TimeStamp;



                string json_Publish = JsonConvert.SerializeObject(currencyDataPublish);

                Global._pubnub.Publish<string>(strCHANNELNAME, json_Publish, DisplayReturnMessage, DisplayErrorMessage);
                

            }

        }

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations. Server returns currency data via this method.
        /// </summary>
        /// <param name="result">Parameter is Response</param>

        void DisplayReturnMessage(string result)
        {

            Console.WriteLine("PUBLISH STATUS CALLBACK");
            Console.WriteLine(result);

        }// End of DisplayReturnMessage

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="pubnubError">Returns the error message if it fails to get back any result after hitting the URL</param>

        void DisplayErrorMessage(PubnubClientError pubnubError)
        {
            Console.WriteLine(pubnubError.StatusCode);
        }// End of DisplayErrorMessage

        /*Subscribe from client end starts*/
        void Subscribe()
        {
            Global._pubnub.Subscribe<string>(
             strCHANNELNAME_Subscribe,
             DisplaySubscribeReturnMessage,
             DisplayConnectStatusMessage,
             DisplayErrorMessage);

            //Global.PublishTrend();
            //PublishTrend(); 
        }//End of Subscribe
        /*Subscribe from client end ends*/

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations
        /// </summary>
        /// <param name="result"></param>
        void DisplaySubscribeReturnMessage(string result)
        {
            var jss = new JavaScriptSerializer();

            var dict = jss.Deserialize<dynamic>(result);//[0]={"name":"EUR"},[1]=14582114931652548,[2]=trendRequestChannel

            var name_Json = dict[0];//{name:"EUR"}

            var curr_name = jss.Deserialize<dynamic>(name_Json);//{"name":"EUR"}

            var key_curr = curr_name["name"];//"EUR"

            //Console.WriteLine("SUBSCRIBE REGULAR CALLBACK:");
            //Console.WriteLine(result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = Global._pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object subscribedObject = (object)deserializedMessage[0];
                    if (subscribedObject != null)
                    {
                        //IF CUSTOM OBJECT IS EXCEPTED, YOU CAN CAST THIS OBJECT TO YOUR CUSTOM CLASS TYPE
                        string resultActualMessage = Global._pubnub.JsonPluggableLibrary.SerializeToJsonString(subscribedObject);
                    }//End if
                }//End if
            }//End if
        }// End of DisplaySubscribeReturnMessage

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        static void DisplayConnectStatusMessage(string result)
        {
            Console.WriteLine("ConnectStatus:");
            Console.WriteLine(result);
        }//End of DisplayConnectStatusMessage

        
    }// End of Schedule_Task class

}