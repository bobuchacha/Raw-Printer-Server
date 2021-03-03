using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;


namespace ePS.Classes
{

    class TrayIcon
    {
        static NotifyIcon notifyIcon = new NotifyIcon();                                        // notification Icon object
        public static ContextMenuStrip contextMenu = new ContextMenuStrip();                   // context menu of notify icon
        public enum EventType { DoubleClick, Exit, BeforeExit, ShowMainWindow, About, ShowConnectedClientsWindow, Log }
        static event EventHandler OnExit;
        static event EventHandler OnBeforeExit;
        static event EventHandler OnDoubleClicked;
        static event EventHandler OnShowMainWindowClicked;
        static event EventHandler OnShowConnectedClientsClicked;
        static event EventHandler OnAboutClicked;
        static event EventHandler OnLogClicked;

        public static void InitializeTrayIcon()
        {
            // initialize static object
            //notifyIcon = new NotifyIcon();
            //contextMenu = new ContextMenuStrip();



            // set icon properties
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Visible = true;
            notifyIcon.Text = Application.ProductName;

            // set double click event
            notifyIcon.DoubleClick += OnDoubleClicked;

            // design context menu
           // contextMenu.Items.Add("Show &Main Window", null, OnShowMainWindowClicked);
            contextMenu.Items.Add("Show System Status", null, OnShowMainWindowClicked);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("E&xit", null, OnMenuExitClicked);
            notifyIcon.ContextMenuStrip = contextMenu;


            // Standard message loop to catch click-events on notify icon
            // Code after this method will be running only after Application.Exit()
            Application.Run();

            // call the on exit
            if (OnExit != null) OnExit(null, null);

            // exit the program
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            contextMenu.Dispose();

        }

        public static NotifyIcon getTrayIcon()
        {
            return notifyIcon;
        }

        private static void OnMenuExitClicked(object s, object e)
        {
            if (OnBeforeExit != null) OnBeforeExit(null, null);
            // this will end application and execute onExit event handler;
            Application.Exit();
        }


        /*
         * set event of the tray icon
         * all events must be set before initialize tray icon
         * */
        public static void AddEventHandler(EventType eventName, EventHandler handler)
        {
            switch (eventName)
            {
                case EventType.DoubleClick:
                    OnDoubleClicked += handler;
                    break;
                case EventType.Exit:
                    OnExit += handler;
                    break;
                case EventType.BeforeExit:
                    OnBeforeExit += handler;
                    break;
                case EventType.ShowMainWindow:
                    OnShowMainWindowClicked += handler;
                    break;
                case EventType.About:
                    OnAboutClicked += handler;
                    break;
                case EventType.ShowConnectedClientsWindow:
                    OnShowConnectedClientsClicked += handler;
                    break;
                case EventType.Log:
                    OnLogClicked += handler;
                    break;
            }

        }

        public static void NotifyUser(string title, string message, int duration = 3000, ToolTipIcon icon = ToolTipIcon.None)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(duration);
            notifyIcon.BalloonTipIcon = icon;
        }
    }
}
