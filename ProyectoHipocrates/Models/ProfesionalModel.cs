using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Globalization;

namespace ProyectoHipocrates.Models
{
    public class ProfesionalModel
    {


        public int id { get; set; }

        [Required]
        [Display(Name = "Tipo de documento")]
        public Int32 idTipoDocumento { get; set; }        

        [Required]
        [StringLength(8)]
        [Display(Name = "Nro. de documento")]
        public String numeroDocumento { get; set; }

        [Required]
        [Display(Name = "Sexo")]
        public Int32 idSexo { get; set; }        

        [Required]
        [StringLength(100)]
        [Display(Name = "Primer apellido")]
        public String primerApellido { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Primer nombre")]
        public String primerNombre { get; set; }

        [StringLength(100)]
        [Display(Name = "Otros nombres")]
        public String otrosNombres { get; set; }

        [StringLength(3)]
        [Display(Name = "Tipo de teléfono")]
        public String tipoTelefono { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^[0-9-\\s\\+]{8,20}$", ErrorMessage = "Formato de teléfono inválido")]

        public String telefono { get; set; }

        [Display(Name = "Observaciones de contacto")]
        public String contactoObservaciones { get; set; }

        [Display(Name = "E-mail")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public String email { get; set; }

        [StringLength(30)]
        [Display(Name = "Matricula")]
        public string matricula { get; set; }

        [Display(Name = "Vigente")]
        public bool vigente { get; set; }
        
        public String usuarioCrea { get; set; }

        public DateTime fechaCrea { get; set; }

        public String usuarioModi { get; set; }

        public DateTime fechaModi { get; set; }

        public string  Establecimiento { get; set; }

        public int index { get; set; }

        public string nombreSisa { get; set; }

        public string apellidoSisa { get; set; }

        public string nombreEspecialidad { get; set; }

        public Especialidad especialidad { get; set; }

        internal void Mokear()
        {
            this.id = 1;
            this.contactoObservaciones = "Observaciones";
            this.email = "mail@falso.com.ar";
            this.fechaCrea = DateTime.Now;
            this.fechaModi = DateTime.Now;
            int[] array = { 1, 2, 3 };
            this.idSexo = 1;
            this.idTipoDocumento = 1;
            this.matricula = "5555";
            this.numeroDocumento = "38700222";
            this.otrosNombres = "pika";
            this.primerApellido = "Chu";
            this.primerNombre = "pika pi";
            this.telefono = "454646";
            this.tipoTelefono = "tel";
            this.usuarioCrea = "Yo";
            this.usuarioModi = "Yo";
            this.vigente = true;
            this.nombreSisa = this.primerNombre;
            this.apellidoSisa = this.primerApellido;
        }

        internal bool EsConsistente()
        {

            bool correcto = true;
            this.email = String.IsNullOrEmpty(this.email) ? string.Empty : this.email;
            if (this.email.Length > 0 && (!this.email.Contains("@") || !this.email.Contains(".com")))
                correcto = false;
            if (String.IsNullOrEmpty(this.matricula) || this.matricula.Length < 1)
                correcto = false;
            if (String.IsNullOrEmpty(this.numeroDocumento) || this.numeroDocumento.Length < 1)
                correcto = false;
            int integer;
            if (!int.TryParse(this.numeroDocumento, out integer))
                correcto = false;

            if (String.IsNullOrEmpty(this.primerApellido) || String.IsNullOrEmpty(this.primerNombre) || this.primerApellido.Length < 1 || this.primerNombre.Length < 1)
                correcto = false;
            if(String.IsNullOrEmpty(this.nombreEspecialidad))
                correcto = false;
            if (Object.Equals(this.especialidad, null))
                correcto = false;

            this.otrosNombres = String.IsNullOrEmpty(this.otrosNombres) ? string.Empty : this.otrosNombres;
            this.tipoTelefono = CaracteristicaTelefonica();
            this.idTipoDocumento = (integer > 10000000 && integer < 99000000) ? 1 : 4; // 1 = DNI , 4= Pasaporte
            this.contactoObservaciones = String.IsNullOrEmpty(this.contactoObservaciones) ? string.Empty : this.contactoObservaciones;
            this.telefono = String.IsNullOrEmpty(this.telefono) ? string.Empty : this.telefono;            

            return correcto;
        }

        public void CargarDatosSisa()
        {
            try
            {
            
                this.apellidoSisa = this.nombreSisa = String.Empty;
                if (String.IsNullOrEmpty(this.numeroDocumento))
                    return;
                XmlDocument response = Sisa.consultarSisa(this.numeroDocumento);
                if(Object.Equals(null,response))
                {
                    return;
                }
                else
                {
                    this.nombreSisa = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(response.DocumentElement.GetElementsByTagName("nombre")[0].FirstChild.Value.ToLower());
                    this.apellidoSisa = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(response.DocumentElement.GetElementsByTagName("apellido")[0].FirstChild.Value.ToLower());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private string CaracteristicaTelefonica()
        {
            if(String.IsNullOrEmpty(this.telefono))
            {
                return string.Empty;
            }
            var array = this.telefono.ToCharArray();
            int cantNum=0;
            foreach (char letra in array)
            {
                if(Char.IsDigit(letra))
                {
                    cantNum++;
                }
            }
            if (cantNum == 8)
                return "TEP";
            return "CEL";
        }



    }

    class Sisa
    {
        public static XmlDocument consultarSisa(String nroDoc)
        {
            string proxyUrl = ConfigurationManager.AppSettings["UrlProxy"];

            string usrSisa = ConfigurationManager.AppSettings["UsrSisa"];
            string passSisa = ConfigurationManager.AppSettings["PassSisa"];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://sisa.msal.gov.ar/sisa/services/rest/cmdb/obtener?nrodoc=" + nroDoc + "&usuario=" + usrSisa + "&clave=" + passSisa);
            request.Method = "GET";
            if (proxyUrl != "")
                request.Proxy = new WebProxy(proxyUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                return null;
            }
            string content = string.Empty;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    content = sr.ReadToEnd();
                }
            }
            var xml = new XmlDocument();
            xml.LoadXml(content);
            xml.RemoveChild(xml.FirstChild);
            return xml;
        }
    }
}