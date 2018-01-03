using System;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace TestClient
{
    class Program
    {
        private static ushort port = 8080;

        private static void Shutdown()
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(IPAddress.Loopback, port);
                StreamWriter output = new StreamWriter(client.GetStream());
                StreamReader input = new StreamReader(client.GetStream());
                output.WriteLine("SHUTDOWN");
                output.Flush();
                Console.WriteLine(input.ReadLine());
                output.Close();
                client.Close();
            }
        }

        private static void Keys()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, port);
                    StreamWriter output = new StreamWriter(client.GetStream());
                    StreamReader input = new StreamReader(client.GetStream());
                    output.WriteLine("KEYS");
                    output.Flush();
                    string line;
                    while (!string.IsNullOrEmpty(line = input.ReadLine()))
                        Console.WriteLine(line);
                    output.Close();
                    client.Close();
                }
            }catch(Exception ex)
            {
                Console.WriteLine("KEYS instruction not executed");
                Console.WriteLine(ex.Message);
            }
            
        }

        private static void Set(string key, string value)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, port);

                    StreamWriter output = new StreamWriter(client.GetStream());
                    StreamReader input = new StreamReader(client.GetStream());

                    // Send request type line
                    output.WriteLine("SET {0} {1}", key, value);
                    output.Flush();
                    string line = input.ReadLine();
                    input.ReadLine();
                    if (line != "OK") throw new Exception("Invalid response format");

                    output.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SET instruction not executed");
                Console.WriteLine(ex.Message);
            }
        }

        private static string Get(string key)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, port);

                    StreamWriter output = new StreamWriter(client.GetStream());
                    StreamReader input = new StreamReader(client.GetStream());

                    // Send request type line
                    output.WriteLine("GET {0}", key);
                    output.Flush();
                    string line = input.ReadLine();
                    input.ReadLine();               

                    output.Close();
                    client.Close();
                    if (line == "(nil)")
					    return null;
                    if(line.StartsWith("\"") && line.EndsWith("\""))
                    {
                        return line.Substring(1, line.Length - 2);
                    }
                    throw new Exception("Invalid response format");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "GET instruction not executed";
            }
        }              


        static void Main(String[] args)
        {
			String execName = AppDomain.CurrentDomain.FriendlyName.Split('.')[0];
            // Checking command line arguments
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: {0} [<TCPPortNumber>]", execName);
                Environment.Exit(1);
            }
			if (args.Length == 1) {
            	if (!ushort.TryParse(args[0], out port))
            	{
                	Console.WriteLine("Usage: {0} [<TCPPortNumber>]", execName);
                	return;
            	}
			}
			Console.WriteLine("--client connects to the server using port {0}", port);
			string k1_value;
            Console.WriteLine("Get(\"k1\"): {0}", (k1_value = Get("k1")) == null ? "undef" : k1_value);
            Set("k1", "v1");
            Set("k1", "v2");
            Console.WriteLine("Get(\"k1\"): {0}", (k1_value = Get("k1"))== null ? "undef" : k1_value);
            Set("k2", "v3");
            Console.WriteLine("Get(\"k2\"): {0}", (k1_value = Get("k2")) == null ? "undef" : k1_value);
            Keys();
            Shutdown();
            Keys();
            Set("k2", "v4");
            Console.ReadKey();
        }
    }
}
