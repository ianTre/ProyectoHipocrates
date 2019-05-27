using ProyectoHipocrates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace ProyectoHipocrates.Controllers
{
    public class HomeController : Controller
    {
        GoogleSS spreed = new GoogleSS();
        Repositorio repo = new Repositorio();
        public ActionResult Index()
        {
            
            List<ProfesionalModel>  lstNuevosProf= spreed.ReadEntries();
            var tes = repo.ObtenerEspecialidades();
            ProfesionalModel profesional = new ProfesionalModel();
            profesional.Mokear();
            return View(profesional);
        }

        [HttpPost]
        public ActionResult Index(ProfesionalModel profesional)
        {
            int row = 34;
            string errores;
            List<IList<object>> matrix = new List<IList<object>>();
            IList<object> fila = new List<object>();
            fila.Add("SI");
            matrix.Add(fila);
            spreed.WriteInGoogleSS(matrix, row);

            if (profesional.EsConsistente())
            { 
                if ((errores = ProfesionalExistente(profesional)).Length == 0)
                    repo.InsetarProfesional(profesional);
                else
                {
                    //Mostrar mensaje de error
                    return View(profesional);
                }
            }
            return View(profesional);
        }

        private string ProfesionalExistente(ProfesionalModel nuevoProfesional)
        {
            List<ProfesionalModel> lstProfesionales = repo.GetProfesionales();
            if (lstProfesionales.Exists(x => x.numeroDocumento == nuevoProfesional.numeroDocumento))
                return "Ya existe un profesional con el mismo numero de documento";
            if (lstProfesionales.Exists(x => x.primerApellido.Trim().ToLower() == nuevoProfesional.primerApellido.Trim().ToLower() && x.primerNombre.ToLower().Trim() == nuevoProfesional.primerNombre.ToLower().Trim()))
                return "Ya existe un profesional con el mismo nombre y el mismo apellido";
            if (lstProfesionales.Exists(x => x.matricula == nuevoProfesional.matricula))
                return "Ya existe un profesional con el mismo numero de matricula";
            return string.Empty;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}