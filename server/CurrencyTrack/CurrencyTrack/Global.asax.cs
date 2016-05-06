using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using PubNubMessaging.Core;// Exclusively for Pubnub
using Newtonsoft.Json;


namespace CurrencyTrack
{
    public class Global : System.Web.HttpApplication
    {

        const string key_curr = "";
        
        const string respChannel = "exchangedata";
        const string reqChannel  = "appRequestChannel";

        public static Pubnub _pubnub;
        
        public static string strDispDataError = "";// globally declaring strDispDataError for Error identification received from currencylayer site

        public static List<decimal> arrUSDEUR = new List<decimal>();// globally declaring list for USDEUR
        public static List<decimal> arrUSDAUD = new List<decimal>();// globally declaring list for USDAUD
        public static List<decimal> arrUSDCNY = new List<decimal>();// globally declaring list for USDCNY
        public static List<decimal> arrUSDINR = new List<decimal>();// globally declaring list for USDINR

       

        public static Dictionary<string, List<decimal>> ArrayDictMap = new Dictionary<string, List<decimal>>()
        {
            { "USDEUR", arrUSDEUR },
            { "USDAUD", arrUSDAUD },
            { "USDCNY", arrUSDCNY },
            { "USDINR", arrUSDINR },
        };


        public static int LastTimeStamp;


        

        void PNInit()
        {

            _pubnub = new Pubnub(System.Web.Configuration.WebConfigurationManager.AppSettings["PNPubKey"], System.Web.Configuration.WebConfigurationManager.AppSettings["PNSubKey"]);

            /* TEST DATA
            arrUSDEUR.Add(1.345);
            arrUSDEUR.Add(2.345);
            arrUSDEUR.Add(3.345);
            arrUSDEUR.Add(4.345);
            arrUSDEUR.Add(5.345);
            arrUSDEUR.Add(6.345);
            */

        }// End of Init()


        void Subscribe()
        {
            _pubnub.Subscribe<string>(
             reqChannel,
             DisplaySubscribeReturnMessage,
             DisplayConnectStatusMessage,
             DisplayErrorMessage);
        }//End of Subscribe

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations
        /// </summary>
        /// <param name="result"></param>
        void DisplaySubscribeReturnMessage(string result)
        {
            

            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                //Console.WriteLine("ReceivedMessageCallbackWhenSubscribed -> result = " + result);
                List<object> deserializedMessage = _pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object subscribedObject = (object)deserializedMessage[0];

                    AppRequest newReq = new AppRequest();
                    
                    //string curr = JsonConvert.DeserializeObject(subscribedObject);
                    if (subscribedObject != null)
                    {
                        JsonConvert.PopulateObject(subscribedObject.ToString(), newReq);

                        if (newReq.requestType == 1)
                        {
                            PublishTrend(newReq.name);
                        }
                        else
                        {
                            PublishCounter(newReq.name);
                        }

                    }
                }
            }
            
            
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

        /// <summary>
        /// Publish Trend data in response to app request = 1
        /// </summary>
        /// <param name="pCurr">Currency Name</param>
        void PublishTrend(string pCurr)
        {
            string trendJSON = string.Empty;
            string currKey = "USD" + pCurr;

            CurrencyTrendData currencyDataPublish = new CurrencyTrendData();

            currencyDataPublish.name = pCurr;
            currencyDataPublish.value = ArrayDictMap[currKey];
            currencyDataPublish.time = LastTimeStamp;

            trendJSON = JsonConvert.SerializeObject(currencyDataPublish); 
            
            _pubnub.Publish<string>(respChannel, trendJSON, DisplayReturnMessage, DisplayErrorMessage);


        }//End of PublishTrend


        /// <summary>
        /// Publish Latest Counter data in response to app request = 0
        /// </summary>
        /// <param name="pCurr">Currency Name</param>

        void PublishCounter(string pCurr)
        {
            string trendJSON = string.Empty;
            string currKey = "USD" + pCurr;

            int arrayCount = Global.ArrayDictMap[currKey].Count;

            if (arrayCount > 0)
            {
                
                CurrencyData currencyDataPublish = new CurrencyData();

                currencyDataPublish.name = pCurr;
                currencyDataPublish.value = ArrayDictMap[currKey][arrayCount - 1].ToString();
                currencyDataPublish.time = LastTimeStamp;

                try
                {

                    if (Global.ArrayDictMap[currKey][arrayCount - 1] > Global.ArrayDictMap[currKey][arrayCount - 2])
                    {
                        currencyDataPublish.direction = "+";
                    }
                    else
                    {
                        currencyDataPublish.direction = "-";
                    }

                    currencyDataPublish.magnitude = Math.Abs(Global.ArrayDictMap[currKey][arrayCount - 1] - Global.ArrayDictMap[currKey][arrayCount - 2]);

                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Can be ignored if list has one or less elements");

                    currencyDataPublish.direction = "+";
                    currencyDataPublish.magnitude = Global.ArrayDictMap[currKey][arrayCount - 1];
                }


                trendJSON = JsonConvert.SerializeObject(currencyDataPublish);

                _pubnub.Publish<string>(respChannel, trendJSON, DisplayReturnMessage, DisplayErrorMessage);


            }

        }//End of PublishCounter

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


        /// <summary>
        /// While the application starts, this method (Application_Start) will be called and JobScheduler.Start() will get initiated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        protected void Application_Start(object sender, EventArgs e)
        {
            JobScheduler.Start();// Run the "JobScheduler" class to start the code once the application will start
            PNInit();
            Subscribe();
        }// End Application_Start()

        protected void Session_Start(object sender, EventArgs e)
        {
        }// End Session_Start()

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }// End Application_BeginRequest()

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }// End Application_AuthenticateRequest()

        protected void Application_Error(object sender, EventArgs e)
        {
        }// End Application_Error()

        protected void Session_End(object sender, EventArgs e)
        {
        }// End Session_End()

        protected void Application_End(object sender, EventArgs e)
        {
        }// End Application_End()
    }
}