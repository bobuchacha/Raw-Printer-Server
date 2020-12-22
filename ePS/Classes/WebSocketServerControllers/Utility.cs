using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.IO.Ports;


namespace ePS.Classes.WebSocketServerControllers
{
    class Response
    {
        public Response()
        {
            this.timestamp = DateTime.Now.Millisecond;
        }
        public bool success { get; set; }
        public int timestamp { get; set; }
        public string message { get; set; }
        public object payload { get; set; }
        public string requestId { get; set; }
    }

    class Request
    {
        public string requestId { get; set; }
        public string requestName { get; set; }
        public object requestParams { get; set; }
    }

    class Utility : WebSocketBehavior
    {
        #region Events
        protected override void OnOpen()
        {

            this.Send("Print Server Utility");

        }
        protected override void OnClose(CloseEventArgs e)
        {

        }

        protected override void OnMessage(MessageEventArgs e)
        {

            // ================== get list of printers ====================
            if (e.Data == "printers")
            {
                string szReturn = "";
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    szReturn += printer + "\n";
                }
                this.Send(szReturn);
            }
        }


        #endregion
    }
}
