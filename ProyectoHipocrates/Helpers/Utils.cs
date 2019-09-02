using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ProyectoHipocrates.Helpers
{
    public class Utils
    {
        public string quitarSoloAcentos(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return palabra;

            palabra = palabra.Replace('ñ','1').Replace('Ñ','2').Normalize(NormalizationForm.FormD);
            var chars = palabra.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC).Replace('1','ñ').Replace('2','Ñ'); 
          
        }
    }
}
