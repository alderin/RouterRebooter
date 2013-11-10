using System;
using System.Net.Sockets;
using System.Threading;

namespace RouterRebooter
{
    internal class Program
    {
        private const int PORT = 23;
        private const int TIMEOUT = 2000;

        private static void Main(string[] args)
        {
            Program prog = new Program();
            if (args.Length == 3)
            {
                prog.InitConnection(args[0], args[1], args[2]);
            }
            else
            {
                Console.WriteLine("Tienes que pasar los argumentos: Direccion, Usuario y Contraseña.");
                Console.Read();
            }
        }

        private NetworkStream ns = null;

        private void InitConnection(string direccion, string user, string pass)
        {
            try
            {
                AutoResetEvent connectDone = new AutoResetEvent(false);
                TcpClient tcpClient = new TcpClient();

                //Comienza una solicitud asincrónica para una conexión a host remoto.
                tcpClient.BeginConnect(direccion, PORT, new AsyncCallback(delegate(IAsyncResult ar)
                {
                    tcpClient.EndConnect(ar); connectDone.Set();
                }), tcpClient);

                // si tarda mas del TIMEOUT entonces termina la connexion y para  el proceso
                if (!connectDone.WaitOne(TIMEOUT))
                {
                    Console.WriteLine("Network connection failed!");
                    return;
                }

                this.ns = tcpClient.GetStream();
                connectHostandReboot(user, pass);
                tcpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void connectHostandReboot(string user, string passwd)
        {
            // Conectando.....
            Byte[] output = new Byte[1024];
            String responseoutput = String.Empty;
            Byte[] cmd = System.Text.Encoding.ASCII.GetBytes("\n");
            //this.ns.Write(cmd, 0, cmd.Length);

            Thread.Sleep(1000);
            Int32 bytes = ns.Read(output, 0, output.Length);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
            Console.Write(responseoutput);
            if (responseoutput.EndsWith("Login: "))
            {
                cmd = System.Text.Encoding.ASCII.GetBytes(user + "\r");
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(cmd));
                ns.Write(cmd, 0, cmd.Length);
            }
            else
            {
                Console.WriteLine("Host Not Connected");
                return;
            }

            Thread.Sleep(1000);
            bytes = ns.Read(output, 0, output.Length);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
            Console.Write(responseoutput);

            if (responseoutput.EndsWith("Password: "))
            {
                cmd = System.Text.Encoding.ASCII.GetBytes(passwd + "\r");
                //  Console.WriteLine(System.Text.Encoding.ASCII.GetString(cmd));
                ns.Write(cmd, 0, cmd.Length);
            }
            else
            {
                Console.WriteLine(" Usario no valido");
                return;
            }

            Thread.Sleep(1000);
            bytes = ns.Read(output, 0, output.Length);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
            Console.Write(responseoutput);
            // controlamos el nombre de usuario que sea root

            if (responseoutput.EndsWith("> "))
            {
                // aqui podemos mandar el comando telnet que quieremos executar en el servidor.
                cmd = System.Text.Encoding.ASCII.GetBytes("reboot" + "\r");
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(cmd));
                ns.Write(cmd, 0, cmd.Length);
            }
            else
            {
                Console.WriteLine(" Contraseña no valida");
                return;
            }

            Thread.Sleep(1000);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
            Console.Write(responseoutput);
        }
    }
}