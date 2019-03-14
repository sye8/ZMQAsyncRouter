# ZMQ Async Router

A demo for ZMQ Asynchronous DEALER <-> ROUTER + ROUTER <-> DEALER that supports pairwise routing between arbitrary number of client/worker pairs

# Run

## Python Client

Run `router.py` first then start any number of `client.py`. `router.py` will start `worker.py` once a new client is connected to it. `client.py` can be stopped using `Ctrl-C` and will inform `router.py` that it will terminate so corresponding worker will stop.

## Unity Client

Run `router.py` first then play `demo` Unity scene. The light will switch on and off with a 1 second interval if the worker is working properly. Stop the scene will send a message to `router.py` to stop corresponding worker.

# Note

This is only compatible with Python 3

For `System.Threading.Tasks` to be accessible, set editor to .NET 4.6

`File -> Build Settings -> Player Settings... -> Other Settings -> Scripting Runtime Version -> Experimental (.NET 4.6 Equivalent)`
