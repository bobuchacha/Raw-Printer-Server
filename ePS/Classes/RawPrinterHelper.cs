using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ePS.Classes
{
    class RawPrinterHelper
    {
        // Structure and API declarations:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, ref IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In()][MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, ref Int32 dwWritten);

        private IntPtr hPrinter = new IntPtr(0);
        private DOCINFOA di = new DOCINFOA();
        private bool isPrinterOpen = false;

        public bool PrinterIsOpen()
        {
            return isPrinterOpen;
        }

        public bool OpenPrint(string szPrinterName, string szDocName = "Receipt")
        {
            if (!isPrinterOpen)
            {
                di.pDocName = szDocName;
                di.pDataType = "RAW";

                if (OpenPrinter(szPrinterName.Normalize(), ref hPrinter, IntPtr.Zero))
                {
                    // Start a document.
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        if (StartPagePrinter(hPrinter)) isPrinterOpen = true;
                    }
                }
            }

            return isPrinterOpen;
        }

        public void ClosePrint()
        {
            if (isPrinterOpen)
            {
                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);
                ClosePrinter(hPrinter);
                isPrinterOpen = false;
            }
        }

        public bool SendStringToPrinter(string szPrinterName, string szString)
        {
            if (isPrinterOpen)
            {
                IntPtr pBytes;
                Int32 dwCount;
                Int32 dwWritten = 0;

                dwCount = szString.Length;

                pBytes = Marshal.StringToCoTaskMemAnsi(szString);

                bool result = WritePrinter(hPrinter, pBytes, dwCount, ref dwWritten);

                Marshal.FreeCoTaskMem(pBytes);

                return result;
            }
            else
                return false;
        }

        public int SendToPrinter(string szPrinterName, byte[] arData, int length)
        {
            if (isPrinterOpen)
            {
                Int32 dwWritten = 0;
                IntPtr pBytes;

                pBytes = Marshal.UnsafeAddrOfPinnedArrayElement(arData, 0);


                WritePrinter(hPrinter, pBytes, length, ref dwWritten);
                return(dwWritten);
            }
            else
                return 0;
        }
    }
}
