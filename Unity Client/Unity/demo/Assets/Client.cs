using System;
using System.Collections;
using System.Text;
using System.Threading;

using NetMQ;
using NetMQ.Sockets;

using UnityEngine;


public class ParallelDealer{
	private readonly Thread dealer;
	private bool dealerStopped;
	private string sendMsg = "";

	public delegate void MessageDelegate(string obj);
	private readonly MessageDelegate messageDelegate;

	public ParallelDealer(MessageDelegate msgDelegate){
		dealer = new Thread(DealerWorker);
		messageDelegate = msgDelegate;
	}

	private void DealerWorker(){
		AsyncIO.ForceDotNet.Force();

		var socket = new DealerSocket();
		socket.Options.Identity = Encoding.UTF8.GetBytes("client_" + System.Guid.NewGuid().ToString());
		socket.Connect("tcp://localhost:5580");

		try{
			var workerStarted = false;
			while(!dealerStopped){
				// Wait for worker
				while(!workerStarted){
					socket.SendFrame("Client Ready");
					string m;
					if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out m)) {
						
						if(m == "Worker Ready"){
							workerStarted = true;
						}
					}
				}
				string msg;
				if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out msg)) {
					messageDelegate(msg);
				}
				if(!string.Equals(sendMsg, "")){
					socket.SendFrame(sendMsg);
					Debug.Log("Sent: " + sendMsg);
					sendMsg = "";
				}
				Thread.Sleep(1000);
			}
		}finally{
			if (socket != null) {
				socket.SendFrame("Client Stopping");
				((IDisposable)socket).Dispose();
			}
		}
	}

	public void Update(string msg){
		sendMsg = msg;
	}

	public void Start(){
		dealerStopped = false;
		dealer.Start();
	}

	public void Stop(){
		dealerStopped = true;

		dealer.Join();
	}
}

public class Client : MonoBehaviour {
	public ParallelDealer dealer;
	public Light light;
	public bool lightIsOn = false; 

	void MessageHandler(string msg){
		Debug.Log ("Received: " + msg);
		if(string.Equals(msg, "on")){
			lightIsOn = true;
			dealer.Update("Light is on");
		}else if(string.Equals(msg, "off")){
			lightIsOn = false;
			dealer.Update("Light is off");
		}else{
			return;
		}
	}

	// Use this for initialization
	void Start () {
		dealer = new ParallelDealer (MessageHandler);
		dealer.Start();
		dealer.Update ("Light is off");
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
		dealer.Stop();
	}
}
