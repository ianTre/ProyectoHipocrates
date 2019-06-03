using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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

        [StringLength(50)]
        [Display(Name = "Teléfono")]
        [RegularExpression("^[0-9-\\s\\+]{8,20}$", ErrorMessage = "Formato de teléfono inválido")]
        public String telefono { get; set; }

        [Display(Name = "Observaciones de contacto")]
        public String contactoObservaciones { get; set; }

        [Display(Name = "E-mail")
            , DataType(DataType.EmailAddress)]
        [RegularExpression("^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`{}|~\\w])*)(?<=[0-9a-z])@))(?([)([(\\d{1,3}.){3}\\d{1,3}])|(([0-9a-z][-0-9a-z]*[0-9a-z]*.)+[a-z0-9][-a-z0-9]{0,22}[a-z0-9]))$", ErrorMessage = "Formato de email inválido")]
        public String email { get; set; }

        [StringLength(30)]
        [Display(Name = "Matricula")]
        public string matricula { get; set; }

        [Display(Name = "Vigente")]
        public bool vigente { get; set; }

        [NotMapped]
        [Display(Name = "Especialidad/es")]
        public virtual Int32[] idEspecialidad_array { get; set; }

        [NotMapped]
        public string nombresEspecialidades
        {
            get
            { return this.especialidades.Select(x => x.nombre).Aggregate((x, y) => string.Format("{0}, {1}", x, y)); }
        }

        public virtual List<Especialidad> especialidades { get; set; }

        public String usuarioCrea { get; set; }

        public DateTime fechaCrea { get; set; }

        public String usuarioModi { get; set; }

        public DateTime fechaModi { get; set; }

        public string  Establecimiento { get; set; }

        public int index { get; set; }



        internal void Mokear()
        {
            this.id = 1;
            this.contactoObservaciones = "Observaciones";
            this.email = "mail@falso.com.ar";
            this.fechaCrea = DateTime.Now;
            this.fechaModi = DateTime.Now;
            int[] array = { 1, 2, 3 };
            this.idEspecialidad_array = array;
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
        }

        internal bool EsConsistente()
        {
            
            bool correcto = true;
            this.email = String.IsNullOrEmpty(this.email) ? string.Empty : this.email;
            if (this.email.Length > 0 && (!this.email.Contains("@") || !this.email.Contains(".com")))
                correcto = false;
            if (this.matricula.Length < 1)
                correcto = false;
            if (this.numeroDocumento.Length < 1)
                correcto = false;
            int integer;
            if (!int.TryParse(this.numeroDocumento, out integer))
                correcto = false;

            if (this.primerApellido.Length < 1 || this.primerNombre.Length < 1)
                correcto = false;

            this.tipoTelefono = CaracteristicaTelefonica();
            this.idTipoDocumento = (integer > 10000000 && integer < 99000000) ? 1 : 4; // 1 = DNI , 4= Pasaporte
            this.idSexo = 1;
            this.contactoObservaciones = String.IsNullOrEmpty(this.contactoObservaciones) ? string.Empty : this.contactoObservaciones;
            this.telefono = String.IsNullOrEmpty(this.telefono) ? string.Empty : this.telefono;
            
            return correcto;
        }

        private string CaracteristicaTelefonica()
        {
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
}