using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Mail;

namespace Practica_4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void FuncionPing()
        /*Funcion Ping tiene como objetivo enviar un paquete a un host 
        para comprobar el estado de la comunicacion*/
        {
            string Direccion;//Variable que almacena el host a comprobar
            Direccion = TextBoxDireccion.Text;


            Ping Pings = new Ping();//Se crea una nueva instancia de Ping
            int timeout = 1200;//Constante que limita el tiempo de respuesta del host remoto.


            try {
                /*Se invoca el metodo Send de la instancia Ping, parametros: Hostname o IP, timeout
                en segunda instancia Status retorna la respuesta de echo, si esta es igual a IPStatus.Sucess*/
                if (Pings.Send(Direccion, timeout).Status == IPStatus.Success)
                {
                    //Se muestra una ventana confirmando al usuario que se ejecuto exitosamente.
                    MessageBox.Show("El comando Ping fue ejecutado exitosamente", "Confirmacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    //Si no informa que el tiempo de espera esta agotado.
                    MessageBox.Show("Tiempo de espera agotado", "Confirmacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception e)
            {
                var message = e.Message;
                if (e.InnerException != null)
                {
                    message = e.InnerException.Message;
                }
                if (e is PingException)
                {
                    var PingExceptionMessages = new Dictionary<string, string>()
                    {
                        { "No such host is known", "Se desconoce ese host!"}
                    };
                    if (PingExceptionMessages.ContainsKey(e.InnerException.Message))
                    {
                        message = PingExceptionMessages[e.InnerException.Message];
                    }
                    MessageBox.Show("Ping Error: " + Convert.ToString(message));
                }

                else if (e is SocketException)
                {
                    MessageBox.Show("Socket Error: " + Convert.ToString(message));
                }

                else
                {
                    MessageBox.Show(Convert.ToString(e));
                }

                CorreoElectronico(message);
            }
        }

        /*Funcion Tracert tiene como objetivo trazar la ruta de un paquete*/
        void Tracert()
        {

            TablaTracert.Rows.Clear();//Limpia las filas de la tabla tracert

            string host = textBoxTracert.Text;//Variable nombre o IP
            int Timeout = 120;//Tiempo maximo de espera.
            int MaxHops = 30;//Maximo de saltos.

            PingReply reply;//Informacion detallada sobre el resultado de la operacion
            Ping pinger = new Ping();
            PingOptions options = new PingOptions();//Opciones con las queremos enviar el paquete.
            options.Ttl = 1;//Time to live
            options.DontFragment = true;//El paque te no prodra ser fragmentado
            byte[] buffer = Encoding.ASCII.GetBytes("NTrace");



            try
            {
                /*Se repite el ping dentro de un bucle en el que va incrementando el TTL hasta llegar
                a su objetivo (reply.Status=IPStatus.Success) o hasta sueperar el numero de saltos permitidos*/
                do
                {
                    DateTime start = DateTime.Now;//Variable Star almacena la hora en la que se inicia.
                    reply = pinger.Send(host, Timeout, buffer, options);/*Almacenamos en al variable reply la informacion
                    retornada por el evento Send de la instancia de la clase ping*/

                    long Milisegundos = DateTime.Now.Subtract(start).Milliseconds;/*Now.Substract toma la diferencia de la variable star
                    al momento en que se ejecuta, se almacena en la variable Milisegundos*/

                    if (reply.Status == IPStatus.TtlExpired)/*Si la respuesta Status de retonor es IPStatus.TtlExpired
                    se considera que el paquete llego a ese salto*/
                    {
                        //Se imprime en la tabla un registro, con el TTL, IP, Tiempo
                        TablaTracert.Rows.Add(options.Ttl, reply.Address.ToString(), Milisegundos+" ms");
                    }
                    else if (reply.Status == IPStatus.Success)/*Si la respuesta Status de retonor es IPStatus.Success
                    el paquete llego a su destino, es decir su ultimo salto*/
                    {
                        TablaTracert.Rows.Add(options.Ttl, reply.Address.ToString(), Milisegundos+" ms");
                        //Se muestra una ventana confirmando al usuario que se ejecuto exitosamente.
                        MessageBox.Show("El comando Tracert fue ejecutado exitosamente", "Confirmacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        //Ante cualquier otro Status, se imprime en la tabla un registro, con el TTL, IP, Tiempo
                        TablaTracert.Rows.Add(options.Ttl, reply.Status.ToString(), "*");
                    }
                    options.Ttl++;//Se aumenta el TTL

                } while ((reply.Status != IPStatus.Success) && (options.Ttl < MaxHops));/*Mientras que  el IPStatus.Success
                o no se supere el maximo de saltos se ejecutara el bucle*/
            }
            /*
            catch (ArgumentNullException e)
            {
                MessageBox.Show(Convert.ToString(e));
            }*/
            catch (Exception e)
            {
                CorreoElectronico(Convert.ToString(e));
                MessageBox.Show(Convert.ToString(e));
            }
        }

        void CorreoElectronico(string Mensaje)
        {
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            var smtpPssword = txtPassword.Text.Length > 0 ? txtPassword.Text : "--------";
            var smtpUser = txtFrom.Text.Length > 0 ? txtFrom.Text : "susana.rivera.93@gmail.com";
            try {
                msg.To.Add(txtTo.Text.Length > 0 ? txtTo.Text : "cesar.f.laredo@gmail.com");//Destinatario
                msg.From = new MailAddress(smtpUser, "Nombre de Usuario", System.Text.Encoding.UTF8);//Emisor y nombre de usuario
                msg.Subject = "Ping";//Asunto del mensaje
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = Mensaje;//Mensaje a ser enviado
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(Convert.ToString(e));
            }

            try
            {
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPssword);
                //hotmail
                client.Port = 587; // Puerto de envio tanto de Hotmail como para Gmail
                client.Host = "smtp.live.com";// Protocolo Simple de Transferencia de Correo de (Hotmail)
                                              //
                client.EnableSsl = true;
                client.Send(msg);//Enviamos el mensaje
                MessageBox.Show("Mensaje Enviado Correctamente", "Correo C#", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Net.Mail.SmtpException e)
            {
                MessageBox.Show("Error en Enviar Mail: "+ e.Message);
            }
        }
        private void ButtonPing_Click(object sender, EventArgs e)
        {
            int intervalo = Convert.ToInt16(numericUpDownIntervalo.Value);//Se toma el valor intervalo
            int minutos = intervalo * 60 * 1000;//Conversion de minutos a milisegundos, valor manejado por el intervalo del timer.
            timer1.Interval = minutos;//Se le asigna el intervalo al timer1
            FuncionPing();
            timer1.Start();//Inicializa el timer1(Ping)/
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Se ejecuta la funcion Ping.
            FuncionPing();
        }

        private void buttonTracert_Click(object sender, EventArgs e)
        {
            int intervalo = Convert.ToInt16(numericUpDown1.Value);
            int minutos = intervalo * 60 * 1000;
            timer2.Interval = minutos;
            Tracert();
            timer2.Enabled = true;

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Tracert();//Se ejecuta la funcion Tracert
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();//Se detiene el timer1(Ping).
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer2.Stop();//Se detiene el timer2(Tracert).
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CorreoElectronico("Mensaje prueba");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
