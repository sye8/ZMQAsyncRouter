using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using NetMQ;
using NetMQ.Sockets;

public class Client : MonoBehaviour
{
    // For async router comm
    private string colorStr = "";
    private bool dealerIsStarted = false;
    // For ping
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // For color changing
    public Renderer DraggableCube;

    // Get UNIX Timestamp in Milliseconds
    

    async void dealerListener()
    {
        AsyncIO.ForceDotNet.Force();

        var socket = new DealerSocket();
        socket.Options.Identity = Encoding.UTF8.GetBytes("client_" + System.Guid.NewGuid().ToString());
		socket.Connect("tcp://45.56.102.215:5580");

        try
        {
            var workerIsStarted = false;
            while (dealerIsStarted)
            {
                // Wait for worker
                while (!workerIsStarted)
                {
                    socket.SendFrame("Client Ready");
                    string m;
                    if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out m))
                    {
                        string[] parts = m.Split(' ');
                        Debug.Log("Delay: " + ((DateTimeOffset.UtcNow.ToUnixTimeSeconds()) - Convert.ToDouble(parts[0])));
                        m = string.Join(" ", parts.Skip(1));
                        if (string.Equals(m, "Worker Ready"))
                        {
                            Debug.Log("Worker Ready");
                            workerIsStarted = true;
                            socket.SendFrame("Xmas Lights!");
                        }
                    }
                }
                string msg;
                if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out msg))
                {
                    Debug.Log("Received: " + msg);
                    string[] parts = msg.Split(' ');
                    Debug.Log("Delay: " + ((DateTimeOffset.UtcNow.ToUnixTimeSeconds()) - Convert.ToDouble(parts[0])));
                    colorStr = parts[1];
                    socket.SendFrame("Will change color to " + msg);
                }
                await Task.Delay(500);
            }
        }
        finally
        {
            if (socket != null)
            {
				colorStr = "black";
                socket.SendFrame("Client Stopping");
                ((IDisposable)socket).Dispose();
            }
        }
    }

    void Start()
    {
        colorStr = "white";
        dealerIsStarted = true;
        Task task = new Task(async () => dealerListener());
        task.Start();
    }


    void Update()
    {
        Color newColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);   // white (default)
        // Parsing color from server code
		if (string.Equals ("red", colorStr)) {
			newColor = new Color (1.0f, 0f, 0f, 1.0f);  // red
		} else if (string.Equals ("green", colorStr)) {
			newColor = new Color (0f, 1.0f, 0f, 1.0f); // green
		} else if (string.Equals ("blue", colorStr)) {
			newColor = new Color (0f, 0f, 1.0f, 1.0f); // blue
		} else if (string.Equals ("yellow", colorStr)) {
			newColor = new Color (1.0f, 0.92f, 0.016f, 1.0f); // yellow
		} else if (string.Equals ("magenta", colorStr)) {
			newColor = new Color (1f, 0f, 1.0f, 1.0f); // mangeta
		} else if (string.Equals ("white", colorStr)){
			newColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);   // white
		} else if (string.Equals ("black", colorStr)) {
			newColor = new Color(0f, 0f, 0f, 1.0f);   // black
		} else if (string.Equals ("Worker Ready", colorStr)) {
			newColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);   // white (default)
		} else {
            Debug.Log("Color " + colorStr + " undefined. Using black.");
            newColor = new Color(0f, 0f, 0f, 1.0f);   // black
        }

        // Changing color
        DraggableCube = GetComponent<Renderer>();
        DraggableCube.material.color = newColor;

    }

	void OnDestroy(){
		dealerIsStarted = false;
	}
}
