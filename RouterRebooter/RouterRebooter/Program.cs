using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
namespace RouterRebooter
{
    class Program
    {
        const string IPADDRESS = "192.168.1.1";
        const int PORT = 23;
        const int TIMEOUT = 2000;
        const string USER = "admin";
        const string PASSWORD = "caca";
        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.InitConnection();
            prog.connectHostandReboot(USER,PASSWORD);
        }

        NetworkStream ns = null;
        void InitConnection()
        {
         
            try
            {

                // Notifica que se ha producido un evento a un subproceso en espera.
                AutoResetEvent connectDone = new AutoResetEvent(false);

                // Proporciona conexiones de cliente para servicios de red TCP.
                TcpClient tcpClient = new TcpClient();

                //Comienza una solicitud asincrónica para una conexión a host remoto.
                tcpClient.BeginConnect(IPADDRESS, PORT, new AsyncCallback(delegate(IAsyncResult ar)
                {
                    tcpClient.EndConnect(ar); connectDone.Set();
                }), tcpClient);

                // si tarda mas del TIMEOUT entonces termina la connexion y para  el proceso
                if (!connectDone.WaitOne(TIMEOUT))
                {
                    Console.WriteLine("Network connection failed!");
                    return;
                }

                ns = tcpClient.GetStream();
                connectHostandReboot(USER, PASSWORD);
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
            ns.Write(cmd, 0, cmd.Length);

            Thread.Sleep(1000);
            Int32 bytes = ns.Read(output, 0, output.Length);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
            // para comprobar que estamos conectados podemos controlar si el servidor nos devuelve una respuesta con la palabra login
            Regex objToMatch = new Regex("login:");
            if (objToMatch.IsMatch(responseoutput))
            {
                cmd = System.Text.Encoding.ASCII.GetBytes(user + "\r");
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

            objToMatch = new Regex("Password");
            if (objToMatch.IsMatch(responseoutput))
            {
                cmd = System.Text.Encoding.ASCII.GetBytes(passwd + "\r");
                ns.Write(cmd, 0, cmd.Length);
            }
            else
            {
                Console.WriteLine(IPADDRESS + " User not valid");
                return;
            }

            Thread.Sleep(1000);
            bytes = ns.Read(output, 0, output.Length);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);

            // controlamos el nombre de usuario que sea root
            objToMatch = new Regex("root");
            if (objToMatch.IsMatch(responseoutput))
            {

                // aqui podemos mandar el comando telnet que quieremos executar en el servidor.
                cmd = System.Text.Encoding.ASCII.GetBytes("comando telenet" + "\r");
                ns.Write(cmd, 0, cmd.Length);
            }
            else
            {
                   Console.WriteLine(IPADDRESS + " Contraseña no valida");
                return;
            }

            Thread.Sleep(1000);
            responseoutput = System.Text.Encoding.ASCII.GetString(output, 0, bytes);
        }
    }
}
