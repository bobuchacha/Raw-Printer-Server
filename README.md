# Raw-Printer-Server
a simple server that let any application to sent data to printer via websocket

# How to use?
Use this application as a stand alone or embed it into your app and run.
You can specify the websocket port as follow:

```
ePS -p 8123 --p=8123
```

After the application start, to exit, just type in or send to its stdin the exit code

# Connect and print
Connect to the web server using the websocket address

```
ws://127.0.0.1:PORT?mid=PRINTER_NAME
```

to print, just send a binary data to your websocket.

# Using Utility Socket Route
This websocket server has an utility route that allow user to fetch information of the host machine such as installed printers. Check code for all functions.
