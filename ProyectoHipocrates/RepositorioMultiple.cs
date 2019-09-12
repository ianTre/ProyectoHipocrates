using ProyectoHipocrates.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ProyectoHipocrates
{
    public class RepositorioMultiple
    {

        DBAcces dBAccess { get; set; }
        private List<ProfesionalModel> profesionales { get; set; }
        public List<Especialidad> Especialidades { get; set; }


        public RepositorioMultiple()
        {
            this.dBAccess = new DBAcces();

        }


        public List<Especialidad> ObtenerEspecialidades(string coneccion)
        {
            try
            {
                SqlConnection conn = dBAccess.GetConnection(coneccion);
                SqlCommand com = new SqlCommand("SP_OBTENER_ESPECIALIDADES", conn);
                List<Especialidad> test = new List<Especialidad>();
                com.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(com);
                DataTable dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                conn.Close();
                test = (from DataRow dr in dt.Rows
                        select new Especialidad()
                        {
                            id = Convert.ToInt32(dr["id"]),
                            codigo = Convert.ToString(dr["codigo"]),
                            nombre = Convert.ToString(dr["nombre"]),

                        }).ToList();
                return test;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<ProfesionalModel> ObtenerProfesionales(string coneccion)
        {
            try
            {
                SqlConnection conn = dBAccess.GetConnection();
                SqlCommand com = new SqlCommand("SP_OBTENER_PROFESIONALES", conn);
                List<ProfesionalModel> test = new List<ProfesionalModel>();
                com.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(com);
                DataTable dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                conn.Close();
                test = (from DataRow dr in dt.Rows
                        select new ProfesionalModel()
                        {
                            matricula = Convert.ToString(dr["matricula"]),
                            numeroDocumento = Convert.ToString(dr["numeroDocumento"]),
                            primerApellido = Convert.ToString(dr["primerApellido"]),
                            primerNombre = Convert.ToString(dr["primerNombre"]),
                            vigente = Convert.ToBoolean(dr["vigente"]),

                        }).ToList();
                return test;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public List<ProfesionalModel> ObtenerProfesionalesFull(string coneccion)
        {
            try
            {
                SqlConnection conn = dBAccess.GetConnection(coneccion);
                SqlCommand com = new SqlCommand("SP_OBTENER_PROFESIONALES", conn);
                List<ProfesionalModel> test = new List<ProfesionalModel>();
                com.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(com);
                DataTable dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                conn.Close();
                test = (from DataRow dr in dt.Rows
                        select new ProfesionalModel()
                        {
                            matricula = Convert.ToString(dr["matricula"]),
                            numeroDocumento = Convert.ToString(dr["numeroDocumento"]),
                            primerApellido = Convert.ToString(dr["primerApellido"]),
                            primerNombre = Convert.ToString(dr["primerNombre"]),
                            vigente = Convert.ToBoolean(dr["vigente"]),
                            id = Convert.ToInt32(dr["id"]),
                            idSexo = Convert.ToInt32(dr["idSexo"]),
                            otrosNombres = Convert.ToString(dr["otrosNombres"]),
                            telefono = Convert.ToString(dr["telefono"]),
                            email = Convert.ToString(dr["email"])
                        }).ToList();
                return test;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void AgregarPersonaProfesionalEspecialidad(ProfesionalModel profesional)
        {
            try
            {
                string conectionString = ObtenerBase(profesional.Establecimiento);
                if (String.IsNullOrEmpty(conectionString))
                {
                    throw new Exception("No se encuentra la base especificada");
                }
                if (!ExisteEspecialidadYProfesional(profesional, conectionString))
                {
                    throw new Exception("La especialidad o el profesional no existe en la base");
                }
                else
                {
                    InsetarEspecialidadProfesional(profesional, conectionString);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        internal bool ExisteEspecialidadYProfesional(ProfesionalModel profesional, string conectionString)
        {
            this.Especialidades = ObtenerEspecialidades(conectionString);
            this.profesionales = ObtenerProfesionalesFull(conectionString);
            bool returner = true;

            ProfesionalModel prof = this.profesionales.FirstOrDefault(x => x.numeroDocumento == profesional.numeroDocumento && x.primerApellido == profesional.primerApellido);

            if ( Object.Equals(prof,null))
            {
                returner = false;
            }

            if (!this.Especialidades.Contains(profesional.especialidad))
            {
                returner = false;
                
            }
            
            if(returner)
                profesional.id = prof.id;
                    
            return returner;

        }

        internal List<ProfesionalModel> GetProfesionales(string coneccion )
        {
            if (Object.Equals(null, this.profesionales))
                this.profesionales = ObtenerProfesionales(coneccion).Where(x => x.vigente == true).ToList();
            return profesionales;
        }


        public void InsetarEspecialidadProfesional(ProfesionalModel profesional, string conectionString)
        {
            try
            {
                SqlConnection conn = dBAccess.GetConnection(conectionString);
                SqlCommand com = new SqlCommand("dbo.SP_Insertar_Profesional_Especialidad", conn);
                com.CommandType = CommandType.StoredProcedure;
                com.CommandTimeout = 0;
                com.Parameters.Clear();
                com.Parameters.Add(new SqlParameter("idProfesional", profesional.id));
                com.Parameters.Add(new SqlParameter("idEspecialidad", profesional.especialidad.id));
                try
                {
                    conn.Open();
                    com.ExecuteScalar();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw ex;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string ObtenerBase(string establecimiento)
        {
            string nombreBase = string.Empty;
            switch (establecimiento)
            {
                case "Rebasa":
                    nombreBase = "RebasaConection";
                    break;
                case "Test":
                    nombreBase = "TestConection";
                    break;
                case "Tablada":
                    nombreBase = "TabladaConection";
                    break;
                case "Salud Mental":
                    nombreBase = "MentalConection";
                    break;
                case "Niños":
                    nombreBase = "NiñosConection";
                    break;
                case "Sakamoto":
                    nombreBase = "SakamotoConection";
                    break;
                case "Giovinazzo":
                    nombreBase = "GiovinazzoConection";
                    break;
                case "Cemefir":
                    nombreBase = "CemefirConection";
                    break;
                case "Eizaguirre":
                    nombreBase = "EizaguirreConection";
                    break;
                case "Policlinico":
                    nombreBase = "PoliclinicoConection";
                    break;
                default:
                    nombreBase = establecimiento+"Conection";
                    break;
            }

            return nombreBase;
        }
    }
}