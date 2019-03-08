import threading
import time

import zmq


# Client Class
class Client(threading.Thread):
    def __init__(self, id):
        threading.Thread.__init__(self)
        self.id = ("client-"+str(id))

    def run(self):
        context = zmq.Context()
        socket = context.socket(zmq.DEALER)
        socket.identity = self.id.encode("ascii")
        socket.connect("tcp://localhost:5580")
        print(self.id + " started")
        while True:
            # Send
            socket.send(("Hello from " + self.id).encode())
            # Receive
            msg = socket.recv_string()
            print(self.id + " received: " + msg)

        print("Stopping client " + str(self.id))
        socket.close()
        context.term()


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
        
        # Init Workers
        for i in range(3):
            worker = Worker(i)
            worker.start()

        # Routing
        while True:
            sockets = dict(poller.poll())
            if backend in sockets:
                workerID, clientID, msg = backend.recv_multipart()
                frontend.send_multipart([clientID, msg])
            if frontend in sockets:
                clientID, msg = frontend.recv_multipart()
                workerID = ("worker-" + (clientID).decode().split('-')[1]).encode()
                backend.send_multipart([workerID, clientID, msg])
            time.sleep(1)

        print("Closing router")
        front.close()
        back.close()
        context.term()


# Worker Class
class Worker(threading.Thread):
    def __init__(self, id):
        threading.Thread.__init__(self)
        self.id = ("worker-"+str(id))

    def run(self):
        # Init socket
        context = zmq.Context()
        socket = context.socket(zmq.DEALER)
        socket.identity = self.id.encode("ascii")
        socket.connect("tcp://localhost:5581")
        
        print(self.id + " started")
        while True:
            # Receive
            client, msg = socket.recv_multipart()
            print(self.id + " received: " + str(msg, 'utf-8'))
            # Send
            socket.send_multipart([client, ("World from " + self.id).encode()])
        
        print("Stopping worker " + str(self.id))
        socket.close()
        context.term()

# Main
if __name__ == "__main__":
    router = Router()
    router.start()
    time.sleep(1)
    for i in range(3):
        client = Client(i)
        client.start()

    router.join()
