using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoHipocrates.Models
{
    public class Especialidad
    {
        
        public const String CONST_ESPECIALIDAD_MEDICA = "MED";
        public const String CONST_ESPECIALIDAD_PARAMEDICA = "PME";
        public const String CONST_ESPECIALIDAD_ODONTOLOGICA = "ODO";
        public const String CONST_ESPECIALIDAD_ENFERMERIA = "ENF";
        public const String CONST_ESPECIALIDAD_BIOQUIMICA = "BIO";
        public const String CONST_ESPECIALIDAD_FARMACIA = "FAR";

        [Key]
        public Int32 id { get; set; }

        public String codigo { get; set; }

        [Required]
        [StringLength(50)]
        public String nombre { get; set; }

        public Boolean vigente { get; set; }

        public String tipoEspecialidad { get; set; }

        
    }
}