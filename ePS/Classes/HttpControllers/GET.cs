using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace ePS.Classes.HttpControllers
{
    class GET
    {
        public static void Handle(object sender, HttpRequestEventArgs e)
        {
            switch (e.Request.Url.LocalPath.ToLower())
            {
                case "/get-printer-list":
                    ResponsePrinterList(e);
                    break;

                case "/get-version":
                    ResponsePrintServerVersion(e);
                    break;

                case "/get-features":
                    ResponseAppFeatures(e);
                    break;

                default:
                    e.Response.StatusCode = 404;
                    break;
            }
        }

        private static void ResponseAppFeatures(HttpRequestEventArgs e)
        {
            String features = "WebSocket\n" +
                "Http\n";


            e.Response.ContentType = "text/plain";
            Utility.ResponseWrite(e.Response, features);
        }

        private static void ResponsePrintServerVersion(HttpRequestEventArgs e)
        {
            e.Response.ContentType = "text/plain";
            Utility.ResponseWrite(e.Response, Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }


        /**
         * Responses printer list in plain text. separate by lines
         **/
        protected static void ResponsePrinterList(HttpRequestEventArgs e)
        {
            
            string szReturn = "";
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                szReturn += printer + "\n";
            }

            e.Response.ContentType = "text/plain";

            Utility.ResponseWrite(e.Response, szReturn);

        }
    }
}
