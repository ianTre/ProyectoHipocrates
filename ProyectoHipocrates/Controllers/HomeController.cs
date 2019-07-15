using Newtonsoft.Json;
using ProyectoHipocrates.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace ProyectoHipocrates.Controllers
{
    public class HomeController : Controller
    {
        GoogleSS spreed = new GoogleSS();
        Repositorio repo = new Repositorio();
        List<ProfesionalModel> lstNuevosProf;


        public ActionResult Index()
        {
            lstNuevosProf = spreed.ReadEntries();
            Session.Add("LstProfesionales", lstNuevosProf);
            
            Session.Add("indice", 0);

            if (lstNuevosProf.Count > 0)
            {
                return View(lstNuevosProf[0]);
            }
            else
            {
                ViewBag.Message = "No existen registros que mostrar.";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Index(ProfesionalModel profesional)
        {
            try
            {
                int row = 34;
                string errores;


                if (profesional.EsConsistente())
                {
                    if ((errores = ProfesionalExistente(profesional)).Length == 0)
                    {
                        repo.InsetarProfesional(profesional);

                        List<IList<object>> matrix = new List<IList<object>>();
                        IList<object> fila = new List<object>();
                        fila.Add("SI");
                        matrix.Add(fila);
                        spreed.WriteInGoogleSS(matrix, profesional.index);

                    }

                    else
                    {
                        ViewBag.Message = "Ya existia un profesional con ese numero de documento";
                    }
                }
                /* Agregar logica para que vuelva  a la pantalla y muestre el error en caso de que no sea consistente.
                else
                {

                }
                */
                int indice = (int)Session["indice"];
                Session["indice"] = ++indice;


                lstNuevosProf = (List<ProfesionalModel>)Session["LstProfesionales"];
                if (indice < lstNuevosProf.Count)
                {

                    return RedirectToAction("Iteracion", lstNuevosProf[indice]);
                }
                else
                {
                    return View("ReplicaCentral");
                    repo.EjecutarJob("BackUpJob");

                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        public ActionResult Iteracion(ProfesionalModel profesional)
        {
            return View("Index", profesional);
        }


        public ActionResult Replica()
        {
            try
            {


                repo.EjecutarJob("BackUpJob");
                Thread.Sleep(20000);
                repo.EjecutarJob("ReplicaSnapshotGeneral");

                Thread.Sleep(20000);
                lstNuevosProf = (List<ProfesionalModel>)Session["LstProfesionales"];
                AgregarEspecialidades(lstNuevosProf);
                ViewBag.Message = "Finalizo la replicacion y la carga de especialidad correctamente.";
                return View("Error");

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }

        }

        private void AgregarEspecialidades(List<ProfesionalModel> lstNuevosProf)
        {
            RepositorioMultiple repos = new RepositorioMultiple();
            foreach (ProfesionalModel profesional in lstNuevosProf)
            {
                try
                {
                    repos.AgregarPersonaProfesionalEspecialidad(profesional);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        

        

        private string ProfesionalExistente(ProfesionalModel nuevoProfesional) 
        {

            try
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
            catch (Exception ex)
            {

                throw ex;
            }
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