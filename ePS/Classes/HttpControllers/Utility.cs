using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Net;

namespace ePS.Classes.HttpControllers
{
    class Utility
    {
        public static void ResponseWrite(HttpListenerResponse response, string txt)
        {
            byte[] buffer = new byte[] { };
            buffer = Encoding.ASCII.GetBytes(txt);

            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();

            response.ContentLength64 = buffer.Length;

        }

        public static byte[] GetRequestPostDataAsBytes(HttpListenerRequest request)
        {
            int length = (int)request.ContentLength64;
            byte[] payload = new byte[length];
            int numRead = 0;
            while (numRead < length)
                numRead += request.InputStream.Read(payload, numRead, length - numRead);

            return payload;
        }

        public static string GetRequestPostDataAsString(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using (System.IO.Stream body = request.InputStream) // here we have data
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
