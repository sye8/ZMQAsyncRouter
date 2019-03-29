import random
import sys
import threading
import time

import zmq


# Worker Class
class Worker(threading.Thread):
    
    colors = ["red", "green", "blue", "yellow", "magenta"]
    
    def __init__(self, id):
        threading.Thread.__init__(self)
        self.id = ("worker_"+str(id))

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
            if msg.decode() == "Client Ready":
                socket.send_multipart([client, ("Worker Ready").encode()])
                print(self.id + " sent: Worker Ready")
            elif msg.decode() == "Client Stopping":
                break
            else:
                color = random.choice(self.colors)
                socket.send_multipart([client, (color.encode())])
                print(self.id + " sent: " + color)
            print("")
        
        # Clean Up
        print("Stopping " + str(self.id))
        socket.close()
        context.term()


# Main
if __name__ == "__main__":
    if len(sys.argv) == 1:
        print("Need to start worker with client UUID")
    else:
        worker = Worker(sys.argv[1])
        worker.start()
