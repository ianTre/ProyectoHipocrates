using ProyectoHipocrates.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace ProyectoHipocrates
{
    public class Repositorio
    {


        DBAcces dBAccess { get; set; }
        private List<ProfesionalModel> profesionales { get; set; }
        public List<ProfesionalModel> Especialidades { get; set; }

        public Repositorio()
        {
            this.dBAccess = new DBAcces();
        }

        public List<Especialidad> ObtenerEspecialidades()
        {
            try
            {
                SqlConnection conn = dBAccess.GetConnection();
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


        public void EjecutarJob(String nombreJob)
        {
            SqlConnection conn = dBAccess.GetConnection();
            SqlCommand com = new SqlCommand("msdb.dbo.sp_start_job", conn);
            com.CommandType = CommandType.StoredProcedure;
            com.CommandTimeout = 0;
            com.Parameters.Clear();
            com.Parameters.Add(new SqlParameter("job_name", nombreJob));
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


        public List<ProfesionalModel> ObtenerProfesionales()
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

        


        

        internal List<ProfesionalModel> GetProfesionales()
        {
            if (Object.Equals(null, this.profesionales))
                this.profesionales = ObtenerProfesionales().Where(x => x.vigente == true).ToList();
            return profesionales;
        }


        //public void InsetarEspecialidadProfesional(ProfesionalModel profesional , string conectionString)
        //{
        //    try
        //    {
        //        SqlConnection conn = dBAccess.GetConnection();
        //        SqlCommand com = new SqlCommand("dbo.SP_Insertar_Profesional_Especialidad", conn);
        //        com.CommandType = CommandType.StoredProcedure;
        //        com.CommandTimeout = 0;
        //        com.Parameters.Clear();
        //        com.Parameters.Add(new SqlParameter("idProfesional", profesional.id));
        //        com.Parameters.Add(new SqlParameter("idEspecialidad", profesional.especialidad));
        //        try
        //        {
        //            conn.Open();
        //            com.ExecuteScalar();
        //            conn.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //            conn.Close();
        //            throw ex;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}



        public void InsertarProfesional(ProfesionalModel profesional)
        {
            try
            {
                
                SqlConnection conn = dBAccess.GetConnection();
                SqlCommand com = new SqlCommand("dbo.SP_Insertar_Profesional", conn);
                com.CommandType = CommandType.StoredProcedure;
                com.CommandTimeout = 0;
                com.Parameters.Clear();
                com.Parameters.Add(new SqlParameter("contactoObsevaciones", profesional.contactoObservaciones));
                com.Parameters.Add(new SqlParameter("email", profesional.email));
                //com.Parameters.Add(new SqlParameter("DescripcionLarga", profesional.especialidades));
                com.Parameters.Add(new SqlParameter("fechaCrea", DateTime.Now));
                com.Parameters.Add(new SqlParameter("fechaModi", DateTime.Now));
                //com.Parameters.Add(new SqlParameter("DescripcionLarga", profesional.idEspecialidad_array));
                com.Parameters.Add(new SqlParameter("idSexo", profesional.idSexo));
                com.Parameters.Add(new SqlParameter("idTipoDocumento", profesional.idTipoDocumento));
                com.Parameters.Add(new SqlParameter("matricula", profesional.matricula));
                //com.Parameters.Add(new SqlParameter("DescripcionLarga", profesional.nombresEspecialidades));
                com.Parameters.Add(new SqlParameter("numeroDocumento", profesional.numeroDocumento));
                com.Parameters.Add(new SqlParameter("primerApellido", profesional.primerApellido));
                com.Parameters.Add(new SqlParameter("primerNombre", profesional.primerNombre));

                var parametro = new SqlParameter("telefono", SqlDbType.VarChar);
                parametro.Value = (object)profesional.telefono ?? DBNull.Value;
                com.Parameters.Add(parametro);

                com.Parameters.Add(new SqlParameter("tipoTelefono", profesional.tipoTelefono));
                com.Parameters.Add(new SqlParameter("usuarioCrea", "AutoGenerado"));
                com.Parameters.Add(new SqlParameter("usuarioModi", "AutoGenerado"));
                com.Parameters.Add(new SqlParameter("vigente", true));
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

                throw;
            }
        }

        

    }



}