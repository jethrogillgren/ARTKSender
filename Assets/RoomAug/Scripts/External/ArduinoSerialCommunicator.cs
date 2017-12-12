/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.IO;

public class ArduinoSerialCommunicator : MonoBehaviour {
	[Space]

	/* The serial port where the Arduino is connected. */
	[Tooltip("The serial port where the Arduino is connected")]
	public string port = "/dev/cu.usbmodem1411";
	/* The baudrate of the serial port. */
	[Tooltip("The baudrate of the serial port")]
	public int baudrate = 9600;

	[Space]

	public Transform target;

	[Space]

	public bool poll = true;
	[Space]
	public bool toggleAccelerometer = false;// – Turn accelerometer readings on or off;
	public bool toggleGyro = false;// – Turn gyroscope readings on or off
	public bool toggleMagnetometer = false;// – Turn magnetometer readings on or off;
	public bool toggleQuaternions = false;// – Turn quaternion readings on or off (qw, qx, qy, and qz are printed after mag readings)
//	e – Turn Euler angle calculations (pitch, roll, yaw) on or off (printed after quaternions)
//	c – Switch to/from calculated values from/to raw readings
//	r – Adjust log rate in 10Hz increments between 1-100Hz (1, 10, 20, …, 100)
//	A – Adjust accelerometer full-scale range. Cycles between ± 2, 4, 8, and 16g.
//	G – Adjust gyroscope full-scale range. Cycles between ± 250, 500, 1000, 2000 dps.
//	s – Enable/disable SD card logging


//	[Tooltip("Read Updates?")]
//	public bool send
//	{
//		get {return m_send; }
//		set {
//			m_send = value;
//			WriteToArduino (" ");
//			Debug.LogWarning ( m_send ? "Recieving Updates" : "Stopping Updates" );
//		}
//	}

	private SerialPort stream;

	void Start()
	{
		Open();
//		WriteToArduino("PING");


//		StartCoroutine (
//			AsynchronousReadFromArduino (
//				(string s ) => OnIMU ( s ),     // Callback
//				() => OnIMUError(), // Error callback
//				10f                             // Timeout (seconds)
//			)
//		);
	}

	private void writeClear(ref bool b, string v) {
		if (b)
		{
			WriteToArduino ( v );
			b = false;
		}
	}

//	bool on = true;
//	int cnt = 0;
	void Update()
	{
		if (!stream.IsOpen)
			return;

		writeClear(ref toggleAccelerometer, "a");
		writeClear(ref toggleGyro, "g");
		writeClear(ref toggleMagnetometer, "m");
		writeClear(ref toggleQuaternions, "q");

		if (!poll)
			return;
		
//		StartCoroutine (
//			AsynchronousReadFromArduino (
//				(string s ) => Debug.Log ( s ),     // Callback
//				() => OnIMUError(), // Error callback
//				10f                             // Timeout (seconds)
//			)
//		);

		WriteToArduino ("#f");
		OnIMU( ReadFromArduino ( 10 ) );


//		cnt++;
//
//		if (cnt == 100)
//		{
//			cnt = 0;
//			Debug.LogError ( "Resume" );
//			on = true;
//		} else if ( cnt == 10 ) {
//			Debug.LogError ("Pause");
////			WriteToArduino (" ");
////			WriteToArduino ("q");
//
//			on = false;
//		} else if (on) {
//			Debug.Log( ReadFromArduino ( 10 ) );
//		}

//		if (!on)
//		{
//			on = true;
//
//		}
	}

	//Assumes Quaternion   1466662, 0.7000, -0.0505, 0.0592, 0.7099
	//Or v2 Euler   	#YPRAG=38.71,-25.54,10.99,109.13,44.75,228.13,-0.00,0.01,0.01
	private readonly string[] stringSeparators = new string[] {",", "="};
	public void OnIMU(string s)
	{
		Debug.Log ( s );

		//First Verison Quaternions
//		string[] parsed = s.Split ( stringSeparators, StringSplitOptions.None);
//		Debug.LogWarning ( float.Parse(parsed[1]) + "." +float.Parse(parsed[2]) + "." +float.Parse(parsed[3]) + "." +float.Parse(parsed[4]));
//		if (target)
//			target.rotation = Quaternion.Inverse(new Quaternion (float.Parse(parsed[1]),float.Parse(parsed[2]),float.Parse(parsed[3]),float.Parse(parsed[4]) ));

		//Second Version EUler
		string[] parsed = s.Split ( stringSeparators, StringSplitOptions.None);
		Debug.LogWarning ( float.Parse(parsed[1]) + "." +float.Parse(parsed[2]) + "." +float.Parse(parsed[3]) + "." +float.Parse(parsed[4]));
		if (target)
			target.localEulerAngles = new Vector3 ( float.Parse ( parsed [ 3 ] ), float.Parse ( parsed [ 2 ] ), float.Parse ( parsed [ 1 ] ) );
	}

	public void OnIMUError()
	{
		Debug.LogError ( "IMU Read Error!" );
//		on = false;
	}


	public void Open () {
		// Opens the serial port
		try {
			stream = new SerialPort(port, baudrate);
			stream.ReadTimeout = 50;
			stream.Open();
			//this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
		} catch(IOException e) {
			Debug.LogWarning ( "No Connection to Arduino: "+ e.Message);
		}
	}


	/* Commands that can be sent
	  (SPACE) – Pause/resume serial port printing
			a – Turn accelerometer readings on or off
			g – Turn gyroscope readings on or off
			m – Turn magnetometer readings on or off
			q – Turn quaternion readings on or off (qw, qx, qy, and qz are printed after mag readings)
			e – Turn Euler angle calculations (pitch, roll, yaw) on or off (printed after quaternions)
			c – Switch to/from calculated values from/to raw readings
			r – Adjust log rate in 10Hz increments between 1-100Hz (1, 10, 20, …, 100)
			A – Adjust accelerometer full-scale range. Cycles between ± 2, 4, 8, and 16g.
			G – Adjust gyroscope full-scale range. Cycles between ± 250, 500, 1000, 2000 dps.
			s – Enable/disable SD card logging

			p - Request a frame be sent
	*/
	public void WriteToArduino(string message)
	{
		// Send the request
		stream.WriteLine(message);
		stream.BaseStream.Flush();
	}

	public string ReadFromArduino(int timeout = 0)
	{
		stream.ReadTimeout = timeout;
		try
		{
			return stream.ReadLine();
		}
		catch (TimeoutException)
		{
			return null;
		}
	}


	public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
	{
		DateTime initialTime = DateTime.Now;
		DateTime nowTime;
		TimeSpan diff = default(TimeSpan);

		string dataString = null;

		do
		{
			// A single read attempt
			try
			{
				dataString = stream.ReadLine();
			}
			catch (TimeoutException)
			{
				dataString = null;
			}

			if (dataString != null)
			{
				callback(dataString);
				yield return null;
			} else
				yield return new WaitForSeconds(0.05f);

			nowTime = DateTime.Now;
			diff = nowTime - initialTime;

		} while (diff.Milliseconds < timeout);

		if (fail != null)
			fail();
		yield return null;
	}

	public void Close()
	{
		stream.Close();
	}
}