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

namespace ePS.Classes.WebSocketServerControllers
{
    public struct WSClient
    {
        public WebSocket WebSocket;
        public string machineID;
        public string clientID;
    }

    class RawPrinterDirect : WebSocketBehavior
    {
        private string _printerName;

        // static fields
        private static int _number = 0;
        private static Dictionary<string, WSClient> WSClientCollection;
        private RawPrinterHelper myPrinter;

        #region Construction
        // constructor with no arguments
        public RawPrinterDirect() : this(null) { }
        public RawPrinterDirect(string prefix) { }
        #endregion



        #region STATIC METHODS
        /// <summary>
        /// Add Connected WebSocket Client Object to Client Collection for further communication
        /// </summary>
        /// <param name="machineID">MachineID sent during connection, aquired using QueryString mid</param>
        /// <param name="SessionID">Session ID of Websocket Client</param>
        /// <param name="ws">WebSocket Object</param>
        protected static void AddClient(string machineID, string SessionID, WebSocket ws)
        {
            WSClient newESClient = new WSClient();
            newESClient.machineID = machineID;
            newESClient.clientID = SessionID;
            newESClient.WebSocket = ws;
            WSClientCollection.Add(machineID, newESClient);
            ServerController.LogDebug("New client Added. Client ID=" + machineID + ". Session ID=" + SessionID);
        }




        /// <summary>
        /// Return WebSocket Object used to communicate to client with machineID
        /// </summary>
        /// <param name="machineID">machineID of client used to search the WebSocket</param>
        /// <returns>WebSocket object if found using machineID, null otherwise</returns>
        protected static WebSocket GetClientWebSocket(string machineID)
        {
            if (WSClientCollection.ContainsKey(machineID))
            {
                return WSClientCollection[machineID].WebSocket;
            }
            else
            {
                return null;
            }
        }




        /// <summary>
        /// Return SessionID of client with machineID
        /// </summary>
        /// <param name="machineID"></param>
        /// <returns></returns>
        protected static string GetClientSessionID(string machineID)
        {
            if (WSClientCollection.ContainsKey(machineID))
            {
                return WSClientCollection[machineID].clientID;
            }
            else
            {
                return null;
            }
        }
        #endregion


        #region Private methods
        /// <summary>
        /// Get MachineID during connection set by QueryString mid
        /// </summary>
        /// <returns></returns>
        private string getPrinterName()
        {
            return Context.QueryString["mid"];
        }
        #endregion


        #region Events
        protected override void OnOpen()
        {
            _printerName = getPrinterName();
            this.Send("Embeded Print Server connected. Printer [" + _printerName + "]");
            ServerController.LogDebug("New RawPrinterDirect established to " + _printerName);
            myPrinter = new RawPrinterHelper();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            ServerController.LogDebug("Session closed. [" + _printerName + "]");
        }

        protected override void OnMessage(MessageEventArgs e)
        { 
            if (_printerName == null || _printerName == "")
            {
                this.Send("Failed. Printer is not specified.");
                return;
            }
        

            if (_printerName == "DEBUG")
            {
                string message;
                if (e.IsBinary)
                {
                    message = "RAW DATA";
                }
                else
                {
                    if (e.Data.Length <= 30) message = e.Data;
                    else message = e.Data.Substring(0, 30);
                }

                ServerController.LogInfo("Print Command Received: \n" + message);
                this.Send("Success");
                return;
            }


            bool opened = myPrinter.OpenPrint(_printerName, "Receipt");
            if (!opened)
            {
                ServerController.LogWarn("Can not open printer [" + _printerName + "]. Print failed.");
                this.Send("Failed");
                return;
            }


            // send data to printer
            if (e.IsBinary)
            {
                myPrinter.SendToPrinter(_printerName, e.RawData, e.RawData.Length);
                this.Send("Success");
                myPrinter.ClosePrint();
                ServerController.LogInfo("Print successfully. Printer [" + _printerName + "]");
            }
            else
            {
                ServerController.LogInfo("Can not send non binary data to printer [" + _printerName + "]. Print failed.");
                this.Send("Failed");
            }
            
        }


        #endregion

    }
}
