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
using System.Dynamic;

namespace ProyectoHipocrates.Controllers
{
    public class HomeController : Controller
    {
        GoogleSS spreed = new GoogleSS();
        Repositorio repo = new Repositorio();
        List<ProfesionalModel> lstNuevosProf;
        List<Especialidad> lstEsp;

        public ActionResult Index()
        {
            lstEsp = repo.ObtenerEspecialidades();
            lstNuevosProf = spreed.ReadEntries(lstEsp);

            //Cargar listados de profesionales y especialidades en Session para mantenerlos en memoria.
            Session.Add("LstProfesionales", lstNuevosProf);
            Session.Add("LstEspecialidades", lstEsp);

            return RedirectToAction("Nuevo");
        }


        public ActionResult Nuevo()
        {

            //Inicializo el indice.
            if (Session["indice"] == null)
            {
                Session.Add("indice", 0);
            }

            lstEsp = (List<Especialidad>)Session["LstEspecialidades"];
            lstNuevosProf = (List<ProfesionalModel>)Session["LstProfesionales"];

            //Comparo la especialidad del profesional con el listado de especialidades
            ViewBag.lstEsp = lstEsp.ToList().Select(x => new SelectListItem
            {
                Value = x.id.ToString(),
                Text = x.nombre,
                Selected = (x.nombre == lstNuevosProf[(int)Session["indice"]].especialidad.nombre)
            });

            //Analizo si ya terminó la inserción de profesionales, sino voy a poder replicar en Central
            if (lstNuevosProf.Count > 0 && (int)Session["indice"] < lstNuevosProf.Count)
            {
                return View(lstNuevosProf[(int)Session["indice"]]);
            }
            else
            {
                this.AddToastMessage("", "No existen más registros para agregar", ToastType.Error); 
                return View("ReplicaCentral");
            }            
        }




        [HttpPost]
        public ActionResult Nuevo(ProfesionalModel profesional)
        {
            try
            {
                /*Coincidir especialidad en el listado de Especialidades*/
                lstEsp = (List<Especialidad>)Session["lstEspecialidades"];
                
                profesional.especialidad = lstEsp.Where(x => x.nombre == profesional.nombreEspecialidad).ToList().FirstOrDefault();

                string errores;

                //Verificar consistencia de profesional, DNI, Nombre y Apellido, Matricula. 
                //Si los datos son erróneos, se vuelve al mismo profesional para arreglarlo.
                if (profesional.EsConsistente())
                {
                    //Si ya existe, se cancela la tupla y se avanza al siguiente profesional del listado.
                    if ((errores = ProfesionalExistente(profesional)).Length == 0)  {

                        //Primera mayúscula, luego minúscula.
                        profesional.primerApellido = priMayus(profesional.primerApellido);
                        profesional.primerNombre = priMayus(profesional.primerNombre);
                        profesional.otrosNombres = priMayus(profesional.otrosNombres);

                        /*Actualizar Listado Profesionales*/
                        ((List<ProfesionalModel>)Session["LstProfesionales"]).Where(x => x.index == profesional.index).ToList().FirstOrDefault().especialidad = profesional.especialidad;
                        ((List<ProfesionalModel>)Session["LstProfesionales"]).Where(x => x.index == profesional.index).ToList().FirstOrDefault().nombreEspecialidad = profesional.nombreEspecialidad;
                        ((List<ProfesionalModel>)Session["LstProfesionales"]).Where(x => x.index == profesional.index).ToList().FirstOrDefault().primerApellido = profesional.primerApellido;
                                                
                        //Se inserta en la base Central.
                        repo.InsertarProfesional(profesional);
                    }                 
                    
                    else
                    {
                        this.AddToastMessage("", errores, ToastType.Error);//salteo 
                       
                        return RedirectToAction("saltearProfesional", profesional.index);
                    }
                }
                else
                {
                    this.AddToastMessage("", "Datos de Profesional Inconsistentes", ToastType.Error);
                    return RedirectToAction("Nuevo");
                }

                return RedirectToAction("EscribirEnGoogle", new { indice = profesional.index, respuesta = "SI" });
                
            }
            catch (Exception ex)
            {                
                //En caso que ocurra un error externo.
                this.AddToastMessage("", ex.Message, ToastType.Error);                
                return RedirectToAction("Avanzar");
            }
        }

        public ActionResult saltearProfesional(int index)
        {
            this.AddToastMessage("", "Se ha Cancelado el profesional", ToastType.Error);           
            return RedirectToAction("EscribirEnGoogle", new { indice = index, respuesta = "NO"});

        }


        public ActionResult EscribirEnGoogle(int indice, string respuesta)
        {

            List<IList<object>> matrix = new List<IList<object>>();
            IList<object> fila = new List<object>();
            fila.Add(respuesta);
            matrix.Add(fila);
            spreed.WriteInGoogleSS(matrix, indice);
            return RedirectToAction("Avanzar");
        }

        public ActionResult Avanzar()
        {

            int indice = (int)Session["indice"];
            Session["indice"]=++indice;
            return RedirectToAction("Nuevo");                    
        }


        public ActionResult Replica()
        {
            try
            {
                //repo.ReplicarEnInstancias();
                //Thread.Sleep(60000);

                //Agregar para que al volver a entrar si Session["LstProfesionales"] está vacío, salga. Jaja, Saludos.
                
                lstNuevosProf = (List<ProfesionalModel>)Session["LstProfesionales"];
                if (lstNuevosProf.Count == 0)
                {
                    return View("ErrorFin");
                }
                AgregarEspecialidades(lstNuevosProf);
                this.AddToastMessage("", "Finalizó la replicación de profesionales y la carga de especialidad correctamente.", ToastType.Success);
                Session.Clear();
                return View("ErrorFin");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                this.AddToastMessage("", ex.Message, ToastType.Error);
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
                    this.AddToastMessage("", "Profesional "+ profesional.primerApellido+", "+profesional.primerNombre+" agregado con Especialidad "+profesional.nombreEspecialidad, ToastType.Success);
                }
                catch (Exception ex)
                {
                    throw new Exception (ex.Message);
                }
            }
        }

                
        private string ProfesionalExistente(ProfesionalModel nuevoProfesional) 
        {
            try
            {
                List<ProfesionalModel> lstProfesionales = repo.GetProfesionales();
                if (lstProfesionales.Exists(x => x.numeroDocumento == nuevoProfesional.numeroDocumento))
                {
                    return "Ya existe un profesional con el mismo número de documento";
                }
                if (lstProfesionales.Exists(x => x.primerApellido.Trim().ToLower() == nuevoProfesional.primerApellido.Trim().ToLower() && x.primerNombre.ToLower().Trim() == nuevoProfesional.primerNombre.ToLower().Trim()))
                {
                    return "Ya existe un profesional con el mismo nombre y/o apellido";
                }

                if (lstProfesionales.Exists(x => x.matricula == nuevoProfesional.matricula))
                {
                    return "Ya existe un profesional con el mismo número de matrícula";
                }
                if (!String.IsNullOrEmpty(nuevoProfesional.otrosNombres)) {
                    if (normalizar(nuevoProfesional.apellidoSisa.ToLower()) == normalizar(nuevoProfesional.primerApellido.ToLower()) && (normalizar(nuevoProfesional.nombreSisa).Contains(normalizar(nuevoProfesional.primerNombre.ToLower().Trim())) || normalizar(nuevoProfesional.nombreSisa).Contains(normalizar(nuevoProfesional.otrosNombres.ToLower().Trim()))))
                        return "Datos Distintos de SISA";
                }

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

        public string priMayus(string palabra)
        {
            return !string.IsNullOrEmpty(palabra) ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(palabra) : "";
        }
    }

    
}