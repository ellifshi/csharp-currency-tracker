using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;

namespace CurrencyTrack
{
    /// <summary>
    /// For Logging Purposes
    /// </summary>
    class clsLogWriter
    {
        ///// <summary>
        ///// Writes Exception information to Exception.txt
        ///// </summary>
        ///// <param name="ex">Exception Object</param>
        ///// <param name="objModuleRef">Reference to the module which caught the exception.<br>(By default send the "this" object)</br></param>
        public static void writeEx(Exception ex, Object objModuleRef)
        {
            try
            {
                // ThreadAbortException occurred when we use Response.End(), Response.Redirect().
                if (ex is System.Threading.ThreadAbortException)
                {
                    //System.Threading.Thread.ResetAbort();
                    return;
                }

                string strFilePath = HttpContext.Current.Server.MapPath("~/log/exception.txt");
                using (StreamWriter objStrmWrt = new StreamWriter(strFilePath, true))
                {
                    objStrmWrt.WriteLine("-----------------");
                    objStrmWrt.WriteLine(DateTime.Now.ToString());
                    objStrmWrt.WriteLine("Module : " + objModuleRef.ToString());
                    objStrmWrt.WriteLine("Exception : " + ex.ToString());
                    objStrmWrt.WriteLine("-----------------");
                }
            }
            catch { }
        }


        public static void writeExMsg(string ExceptionMsg, Object objModuleRef)
        {
            try
            {


                string strFilePath = HttpContext.Current.Server.MapPath("~/log/exception.txt");
                using (StreamWriter objStrmWrt = new StreamWriter(strFilePath, true))
                {
                    objStrmWrt.WriteLine("-----------------");
                    objStrmWrt.WriteLine(DateTime.Now.ToString());
                    objStrmWrt.WriteLine("Module : " + objModuleRef.ToString());
                    objStrmWrt.WriteLine("Exception : " + ExceptionMsg.ToString());
                    objStrmWrt.WriteLine("-----------------");
                }
            }
            catch { }
        }
        ///// <summary>
        ///// Writes Exception information to Exception.txt
        ///// </summary>
        ///// <param name="ex">Exception Object</param>
        ///// <param name="objModuleRef">Reference to the module which caught the exception.<br>(By default send the "this" object)</br></param>
        public static void writeSQLEx(string strSQLStmt, Exception ex, Object objModuleRef)
        {
            try
            {
                //FileInfo objFilInf = new FileInfo(Application.ExecutablePath);
                //string strFilePath = objFilInf.DirectoryName + "\\SQLException.txt";
                string strFilePath = HttpContext.Current.Server.MapPath("~/log/sqlexception.txt");
                using (StreamWriter objStrmWrt = new StreamWriter(strFilePath, true))
                {
                    objStrmWrt.WriteLine("*-----------------");
                    objStrmWrt.WriteLine(DateTime.Now.ToString());
                    objStrmWrt.WriteLine("Module : " + objModuleRef.ToString());
                    objStrmWrt.WriteLine("SQL : " + strSQLStmt);
                    objStrmWrt.WriteLine("Exception : " + ex.ToString());
                    objStrmWrt.WriteLine("*-----------------");
                }
            }
            catch { }
        }
    }

}