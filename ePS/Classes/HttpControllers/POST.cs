using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace ePS.Classes.HttpControllers
{
    class POST
    {
        static RawPrinterHelper myPrinter = new RawPrinterHelper();

        public static void Handle(object sender, HttpRequestEventArgs e)
        {
            switch (e.Request.Url.LocalPath.ToLower())
            {
                case "/print":
                    ProceedPrintRequest(e);
                    break;

                default:
                    e.Response.StatusCode = 404;
                    break;
            }
        }

        private static void ProceedPrintRequest(HttpRequestEventArgs e)
        {
            var printer = e.Request.QueryString.Get("printer");
            var documentName = e.Request.QueryString.Get("title");

            bool printerInstalled = false;

            // check if user specified printer

            if (printer == null)
            {
                Utility.ResponseWrite(e.Response, "Printer not specified");
                e.Response.ContentType = "text/plain";
                e.Response.StatusCode = 400;
                return;
            }

            // check if printer installed

            foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
            {
                if (installedPrinter == printer)
                {
                    printerInstalled = true;
                    break;
                }
            }

            if (!printerInstalled)
            {
                Utility.ResponseWrite(e.Response, "Printer not installed: " + printer);
                e.Response.ContentType = "text/plain";
                e.Response.StatusCode = 400;
                return;
            }

            // perform printing

            bool opened = myPrinter.OpenPrint(printer, documentName != null ? documentName : "Receipt");

            // check if printer has opened for printing

            if (!opened)
            {
                ServerController.LogWarn("Can not open printer [" + printer + "]. Print failed.");
                Utility.ResponseWrite(e.Response, "Can not open printer [" + printer + "]. Print failed.");
                e.Response.ContentType = "text/plain";
                e.Response.StatusCode = 500;
                return;
            }

            if (!e.Request.HasEntityBody)
            {
                Utility.ResponseWrite(e.Response, "Invalid raw printing data.");
                e.Response.ContentType = "text/plain";
                e.Response.StatusCode = 400;
                return;
            }


            // get data and send to printer. then close print job

            byte[] data = Utility.GetRequestPostDataAsBytes(e.Request);
            myPrinter.SendToPrinter(printer, data , data.Length);
            myPrinter.ClosePrint();


            // response with success

            Utility.ResponseWrite(e.Response, "Print job has been sent to printer successfully!");
            e.Response.ContentType = "text/plain";
            e.Response.StatusCode = 200;
            e.Response.Close();

            return;
        }
    }
}
