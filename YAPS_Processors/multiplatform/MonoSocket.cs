using System;
using System.Net.Sockets;
using System.Threading;

namespace System.Net.Sockets
{	
	public class MonoSocket : System.Net.Sockets.Socket
	{
		private double receiveTimeout = 100;
		private double internalCounter;
		private System.Threading.Thread timerThread;
		private object lockObject = new object();
		
		//private System.Timers.Timer receiveTimer = new System.Timers.Timer();
		
		public MonoSocket(AddressFamily family, SocketType type, ProtocolType ptype) : base(family, type, ptype)
		{
			timerThread = new Thread(new ThreadStart(StartTimer));
		}
		
		private void StartTimer()
		{
			internalCounter = receiveTimeout;
			while(internalCounter > 0)
			{
				Thread.Sleep(10);
				lock(lockObject)
				{
					internalCounter = internalCounter - 10; //this is a little bit inaccurate, because we do not know how log we wait int the lock()
				}
			}
			
			//call the method to close the socket
			TimerTimeout();			
		}
		
		public new double ReceiveTimeout
		{
			get {return receiveTimeout;}
			set
			{
				if (value < -1 || value > int.MaxValue)
					throw new ArgumentOutOfRangeException("value is too small or to big");
				else
				{
					lock(lockObject)
					{
						receiveTimeout = value;
						internalCounter = receiveTimeout;
					}
				}
			}
		}
		
		public new int Receive(byte[] buffer)
		{
			int receivedBytes = 0;
						
			receivedBytes = base.Receive(buffer);
			if (receivedBytes > 0)
			{
				lock(lockObject)
				{
					internalCounter = receiveTimeout;
				}
			}
			return receivedBytes;
		}
		
		private void TimerTimeout()
		{
			#if DEBUG
			Console.WriteLine(internalCounter);
			Console.WriteLine("timeout occured during receive");
			#endif
			Close() ; //don't if this works here. expecting an exception in the base.Receive() method above
		}
		
		public new void Close()
		{
			base.Close();
		}
	}	
}
