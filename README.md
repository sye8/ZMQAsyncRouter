# ZMQ Async Router

A demo for ZMQ Asynchronous DEALER <-> ROUTER + ROUTER <-> DEALER that supports pairwise routing between arbitrary number of client/worker pairs

# Run

Run `router.py` first then start any number of `client.py`. `router.py` will start `worker.py` once a new client is connected to it. `client.py` can be stopped using `Ctrl-C` and will inform `router.py` that it will terminate so corresponding worker will stop.


