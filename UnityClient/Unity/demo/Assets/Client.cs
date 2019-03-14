using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Sockets;

using UnityEngine;


public class Client : MonoBehaviour {
	public Light light;
	public bool lightIsOn = false; 

	private bool dealerIsStarted = false;

	async void dealerListener(){
		var socket = new DealerSocket();
		socket.Options.Identity = Encoding.UTF8.GetBytes("client_" + System.Guid.NewGuid().ToString());
		socket.Connect("tcp://localhost:5580");

		try{
			var workerIsStarted = false;
			while(dealerIsStarted){
				// Wait for worker
				while(!workerIsStarted){
					socket.SendFrame("Client Ready");
					string m;
					if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out m)) {
						if(m == "Worker Ready"){
							Debug.Log("Worker Ready");
							workerIsStarted = true;
							socket.SendFrame("Light is off");
						}
					}
				}
				string msg;
				if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out msg)) {
					Debug.Log("Received: " + msg);
					if(msg == "on"){
						lightIsOn = true;
						socket.SendFrame("Light is on");
					}else if(msg == "off"){
						lightIsOn = false;
						socket.SendFrame("Light is off");
					}
				}
				await Task.Delay(500);
			}
		}finally{
			if (socket != null) {
				socket.SendFrame("Client Stopping");
				((IDisposable)socket).Dispose();
			}
		}
	}

	// Use this for initialization
	void Start () {
		lightIsOn = false;
		dealerIsStarted = true;
		Task task = new Task (async() => dealerListener ());
		task.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		if (lightIsOn) {
			light.enabled = true;
		} else {
			light.enabled = false;
		}
	}

	void OnDestroy(){
		dealerIsStarted = false;
	}
}
