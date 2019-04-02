import subprocess
import threading
import time

import zmq


# ROUTER class
class Router(threading.Thread):
    def __init__(self):
        threading.Thread.__init__(self)

    def run(self):
        context = zmq.Context()
        # Init Front
        frontend = context.socket(zmq.ROUTER)
        frontend.bind("tcp://*:5580")
        # Init Back
        backend = context.socket(zmq.ROUTER)
        backend.bind("tcp://*:5581")
        
        # Initialize Pollers
        poller = zmq.Poller()
        poller.register(frontend, zmq.POLLIN)
        poller.register(backend, zmq.POLLIN)

        # Routing
        print("Router Started")
        while True:
            sockets = dict(poller.poll())
            if backend in sockets:
                workerID, clientID, msg = backend.recv_multipart()
                frontend.send_multipart([clientID, msg])
            if frontend in sockets:
                clientID, msg = frontend.recv_multipart()
                # Start Worker
                if msg.decode() == "Client Ready":
                    print(clientID.decode() + " is ready. Starting worker...")
                    subprocess.Popen(["python3", "worker.py", (clientID).decode().split('_')[1]])
                time.sleep(0.5)
                workerID = ("worker_" + (clientID).decode().split('_')[1]).encode()
                backend.send_multipart([workerID, clientID, msg])

        # Clean Up
        print("Closing router")
        front.close()
        back.close()
        context.term()


# Main
if __name__ == "__main__":
    router = Router()
    router.start()
