using Newtonsoft.Json;
using ProyectoHipocrates.Models;
using ProyectoHipocrates.Helpers;
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
using System.Globalization;
using System.Text;

namespace ProyectoHipocrates.Controllers
{
    public class HomeController : Controller
    {
        GoogleSS spreed = new GoogleSS();
        Repositorio repo = new Repositorio();
        List<ProfesionalModel> lstNuevosProf;


        public ActionResult Index()
        {
            List<Especialidad> lstEsp = repo.ObtenerEspecialidades();
            lstNuevosProf = spreed.ReadEntries(lstEsp);


            Session.Add("LstProfesionales", lstNuevosProf);
            
            Session.Add("indice", 0);
            ViewBag.lstEsp = lstEsp.ToList().Select(x => new SelectListItem
                            {
                                Value = x.id.ToString(),
                                Text = x.nombre,
                                Selected = (x.nombre == lstNuevosProf .especialidad.nombre)
                            });
            // new SelectList (repo.ObtenerEspecialidades(), "codigo", "nombre", lstNuevosProf[0].especialidad.nombre);
            //ViewBag.especialidades = repo.ObtenerEspecialidades();


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

                string errores;

                if (profesional.EsConsistente() && ModelState.IsValid)
                {
                    if ((errores = ProfesionalExistente(profesional)).Length == 0)
                    {
                        repo.InsertarProfesional(profesional);

                        List<IList<object>> matrix = new List<IList<object>>();
                        IList<object> fila = new List<object>();
                        fila.Add("SI");
                        matrix.Add(fila);
                        spreed.WriteInGoogleSS(matrix, profesional.index);
                    }
                    else
                    {
                        this.AddToastMessage("", errores, ToastType.Error);
                        ViewBag.lstEsp =  repo.ObtenerEspecialidades().ToList();
                        return View("Index");
                    }
                }
                else
                {
                    this.AddToastMessage("", "Datos de Profesional Inconsistentes", ToastType.Error);
                    return View("Index");
                }

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
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }


        public ActionResult saltearProfesional()
        {

            List<IList<object>> matrix = new List<IList<object>>();
            IList<object> fila = new List<object>();
            int indice = (int)Session["indice"];
            Session["indice"] = ++indice;
            fila.Add("NO");
            matrix.Add(fila);
            spreed.WriteInGoogleSS(matrix, profesional.index);

            return RedirectToAction("Iteracion", lstNuevosProf[indice]);
        }


        public ActionResult Iteracion(ProfesionalModel profesional)
        {
            return View("Index", profesional);
        }


        public ActionResult Replica()
        {
            try
            {

                repo.EjecutarJob("ReplicaSnapshotGeneral");

                Thread.Sleep(60000);
                lstNuevosProf = (List<ProfesionalModel>)Session["LstProfesionales"];
                AgregarEspecialidades(lstNuevosProf);
                this.AddToastMessage("", "Finalizó la replicación y la carga de especialidad correctamente.", ToastType.Success);               
                return View("Index");

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
                    this.AddToastMessage("", "Profesional agregado con Especialidad", ToastType.Success);

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
                if (lstProfesionales.Exists(x => x.numeroDocumento == nuevoProfesional.numeroDocumento)) {
                    //this.AddToastMessage("", "Ya existe un profesional con el mismo número de documento", ToastType.Error);
                    return "Ya existe un profesional con el mismo número de documento";
                }                                 
                if (lstProfesionales.Exists(x => x.primerApellido.Trim().ToLower() == nuevoProfesional.primerApellido.Trim().ToLower() && x.primerNombre.ToLower().Trim() == nuevoProfesional.primerNombre.ToLower().Trim()))
                {
                    //this.AddToastMessage("", "Ya existe un profesional con el mismo nombre y el mismo apellido", ToastType.Error);
                    return "Ya existe un profesional con el mismo nombre y/o apellido";
                }

                if (lstProfesionales.Exists(x => x.matricula == nuevoProfesional.matricula))
                {
                    //this.AddToastMessage("", "Ya existe un profesional con el mismo numero de matricula", ToastType.Error);
                    return "Ya existe un profesional con el mismo número de matrícula";
                }

                if (normalizar(nuevoProfesional.apellidoSisa.ToLower()) == normalizar(nuevoProfesional.primerApellido.ToLower()) && (normalizar(nuevoProfesional.nombreSisa).Contains(normalizar(nuevoProfesional.primerNombre.ToLower().Trim())) ||  normalizar(nuevoProfesional.nombreSisa).Contains(normalizar(nuevoProfesional.otrosNombres.ToLower().Trim()))))
                    return "Datos Distintos de SISA"; 

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string normalizar(string palabra)
        {
            return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-9").GetBytes(palabra));
        }


        public ActionResult Contact()
        {
            return View("Contact");
        }

    }

    
}