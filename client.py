import threading
import time
import uuid

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
        
        workerIsStarted = False
        print("Waiting for worker...")
        
        # Run Loop
        while True:
            # Wait for worker
            while not workerIsStarted:
                socket.send(("Client Ready").encode())
                msg = socket.recv_string()
                print(self.id + " received: " + msg)
                if msg == "Worker Ready":
                    workerIsStarted = True
                    break
                time.sleep(1)
            # Send
            socket.send(("Hello from " + self.id).encode())
            # Receive
            msg = socket.recv_string()
            print(self.id + " received: " + msg + "\n")
            time.sleep(1)

        # Clean Up
        print("Stopping client " + str(self.id))
        socket.close()
        context.term()


# Main
if __name__ == "__main__":
    client = Client(uuid.uuid4())
    client.start()
