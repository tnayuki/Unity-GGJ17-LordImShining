using System;
using System.Net.Sockets;

public class OSCClient {
	private UdpClient udpClient;
	
	public OSCClient(string serverHostname, int serverPort) {
		udpClient = new UdpClient(serverHostname, serverPort);
	}
	
	// Currently, we implement only the needed methods
	public void SendSimpleMessage(string path, int i) {
		byte[] pathByte = System.Text.Encoding.UTF8.GetBytes(path);

		byte[] datagram = new byte[(pathByte.Length + 1 + 3) / 4 * 4 + 4 + 4];
		
		Buffer.BlockCopy(pathByte, 0, datagram, 0, pathByte.Length);
		
		int p = (pathByte.Length + 1 + 3) / 4 * 4;

		datagram[p++] = (byte)',';
		datagram[p++] = (byte)'i';
		p += 2;

		datagram[p++] = (byte)((uint)i >> 24);
		datagram[p++] = (byte)((uint)i >> 16);
		datagram[p++] = (byte)((uint)i >> 8);
		datagram[p++] = (byte)((uint)i);
		
		udpClient.Send(datagram, datagram.Length);
	}

	public void SendSimpleMessage(string path, int i1, int i2, int i3, int i4) {
		byte[] pathByte = System.Text.Encoding.UTF8.GetBytes(path);

		byte[] datagram = new byte[(pathByte.Length + 1 + 3) / 4 * 4 + 8 + 4 * 4];

		Buffer.BlockCopy(pathByte, 0, datagram, 0, pathByte.Length);

		int p = (pathByte.Length + 1 + 3) / 4 * 4;

		datagram[p++] = (byte)',';
		datagram[p++] = (byte)'i';
		datagram[p++] = (byte)'i';
		datagram[p++] = (byte)'i';
		datagram[p++] = (byte)'i';
		p += 3;

		datagram[p++] = (byte)((uint)i1 >> 24);
		datagram[p++] = (byte)((uint)i1 >> 16);
		datagram[p++] = (byte)((uint)i1 >> 8);
		datagram[p++] = (byte)((uint)i1);
		datagram[p++] = (byte)((uint)i2 >> 24);
		datagram[p++] = (byte)((uint)i2 >> 16);
		datagram[p++] = (byte)((uint)i2 >> 8);
		datagram[p++] = (byte)((uint)i2);
		datagram[p++] = (byte)((uint)i3 >> 24);
		datagram[p++] = (byte)((uint)i3 >> 16);
		datagram[p++] = (byte)((uint)i3 >> 8);
		datagram[p++] = (byte)((uint)i3);
		datagram[p++] = (byte)((uint)i4 >> 24);
		datagram[p++] = (byte)((uint)i4 >> 16);
		datagram[p++] = (byte)((uint)i4 >> 8);
		datagram[p++] = (byte)((uint)i4);

		udpClient.Send(datagram, datagram.Length);
	}
		
	public void SendSimpleMessage(string path, float f) {
		byte[] pathByte = System.Text.Encoding.UTF8.GetBytes(path);

		byte[] datagram = new byte[(pathByte.Length + 1 + 3) / 4 * 4 + 4 + 4];
		
		Buffer.BlockCopy(pathByte, 0, datagram, 0, pathByte.Length);
		
		int p = (pathByte.Length + 1 + 3) / 4 * 4;

		datagram[p++] = (byte)',';
		datagram[p++] = (byte)'f';
		p += 2;

		byte[] bytes = BitConverter.GetBytes(f);
	    if (BitConverter.IsLittleEndian) {
    	    Array.Reverse(bytes);
		}
		Buffer.BlockCopy(bytes, 0, datagram, p, 4);
		
		udpClient.Send(datagram, datagram.Length);
	}

	public void SendSimpleMessage(string path, byte[] b) {
		byte[] pathByte = System.Text.Encoding.UTF8.GetBytes(path);

		byte[] datagram = new byte[(pathByte.Length + 1 + 3) / 4 * 4 + 4 + 4 + (b.Length + 3) / 4 * 4];
		
		Buffer.BlockCopy(pathByte, 0, datagram, 0, pathByte.Length);
		
		int p = (pathByte.Length + 1 + 3) / 4 * 4;

		datagram[p++] = (byte)',';
		datagram[p++] = (byte)'b';
		p += 2;
		
		datagram[p++] = (byte)(b.Length >> 24);
		datagram[p++] = (byte)(b.Length >> 16);
		datagram[p++] = (byte)(b.Length >> 8);
		datagram[p++] = (byte)(b.Length);
		
		Buffer.BlockCopy(b, 0, datagram, p, b.Length);
		
		udpClient.Send(datagram, datagram.Length);
	}

}
