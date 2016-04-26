using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using PubNubMessaging.Core;// Exclusively for Pubnub
using Newtonsoft.Json;
using System.Web.Script.Serialization;//JSON Deserialization

namespace CurrencyTrack
{
    public class Global : System.Web.HttpApplication
    {

        const string key_curr = "";
        const string strCHANNELNAME = "exchangedata";
        const string strCHANNELNAME_Subscribe = "trendRequestChannel";
        public static Pubnub _pubnub;
        const string strPUBLISH_KEY = "pub-c-913ab39c-d613-44b3-8622-2e56b8f5ea6d";
        const string strSUBSCRIBE_KEY = "sub-c-8ad89b4e-a95e-11e5-a65d-02ee2ddab7fe";

        
        public static string strDispDataError = "";// globally declaring strDispDataError for Error identification received from currencylayer site

        public static ArrayList arrUSDEUR = new ArrayList();// globally declaring arraylist for USDEUR
        public static ArrayList arrUSDAUD = new ArrayList();// globally declaring arraylist for USDAUD
        public static ArrayList arrUSDCNY = new ArrayList();// globally declaring arraylist for USDCNY
        public static ArrayList arrUSDINR = new ArrayList();// globally declaring arraylist for USDINR

       

        public static Dictionary<string, ArrayList> ArrayDictMap = new Dictionary<string, ArrayList>()
        {
            { "USDEUR", arrUSDEUR },
            { "USDAUD", arrUSDAUD },
            { "USDCNY", arrUSDCNY },
            { "USDINR", arrUSDINR },
        };


        public static int LastTimeStamp;


        

        void PNInit()
        {
            _pubnub = new Pubnub(strPUBLISH_KEY, strSUBSCRIBE_KEY);

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
             strCHANNELNAME_Subscribe,
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
            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<dynamic>(result);//[0]={"name":"EUR"},[1]=14582114931652548,[2]=trendRequestChannel
            var name_Json = dict[0];//{name:"EUR"}
            var curr_name = jss.Deserialize<dynamic>(name_Json);//{"name":"EUR"}
            string key_curr = curr_name["name"];//"EUR"

            PublishTrend(key_curr);
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

        void PublishTrend(string pCurr)
        {
            string trendJSON = string.Empty;
            string currKey = "USD" + pCurr;

            CurrencyTrendData currencyDataPublish = new CurrencyTrendData();

            currencyDataPublish.requestType = 1;
            currencyDataPublish.name = pCurr;
            currencyDataPublish.value = ArrayDictMap[currKey];
            currencyDataPublish.time = LastTimeStamp;

            trendJSON = JsonConvert.SerializeObject(currencyDataPublish); 
            
            _pubnub.Publish<string>(strCHANNELNAME, trendJSON, DisplayReturnMessage, DisplayErrorMessage);


        }//End of PublishTrend

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