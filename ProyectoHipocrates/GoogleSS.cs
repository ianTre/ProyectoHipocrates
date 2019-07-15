using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using ProyectoHipocrates.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace ProyectoHipocrates
{
    public class GoogleSS
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "MLM-Hipocrates";
        static readonly string SpreadsheetId = "1001t4b01_zRFTXnGQaLBz25_8ZbeV9-L_T08YAxUG6Y";
        static readonly string sheet = "Respuestasdeformulario";
        static SheetsService service;
        Repositorio repo = new Repositorio();

        public GoogleSS()
        {
            try
            {


                GoogleCredential credential;
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(Scopes);
                }

                // Create Google Sheets API service.
                service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public List<ProfesionalModel> ReadEntries()
        {
            try
            {
                List<ProfesionalModel> lista = new List<ProfesionalModel>();

                var range = $"{sheet}!A:M";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(SpreadsheetId, range);

                var response = request.Execute();
                IList<IList<object>> values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var lEs = repo.ObtenerEspecialidades();
                    int i = 0;
                    foreach (var row in values)
                    {
                        i++;
                        if (values[0] == row)
                            continue;
                        ProfesionalModel profesional = BindearProfesional(row, i, lEs);
                        if (!Object.Equals(null, profesional))
                            lista.Add(profesional);
                    }
                }
                else
                {
                    Console.WriteLine("No data found.");
                }
                return lista;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string WriteInGoogleSS(List<IList<object>> data, int writtingRange)
        {
            String range = "Respuestasdeformulario!A" + writtingRange.ToString() + ":B";
            string valueInputOption = "USER_ENTERED";

            // The new values to apply to the spreadsheet.
            List<Google.Apis.Sheets.v4.Data.ValueRange> updateData = new List<Google.Apis.Sheets.v4.Data.ValueRange>();
            var dataValueRange = new Google.Apis.Sheets.v4.Data.ValueRange();
            dataValueRange.Range = range;
            dataValueRange.Values = data;
            updateData.Add(dataValueRange);

            Google.Apis.Sheets.v4.Data.BatchUpdateValuesRequest requestBody = new Google.Apis.Sheets.v4.Data.BatchUpdateValuesRequest();
            requestBody.ValueInputOption = valueInputOption;
            requestBody.Data = updateData;

            var request = service.Spreadsheets.Values.BatchUpdate(requestBody, SpreadsheetId);

            Google.Apis.Sheets.v4.Data.BatchUpdateValuesResponse response = request.Execute();
            // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync(); // For async 

            return JsonConvert.SerializeObject(response);
        }

        private static ProfesionalModel BindearProfesional(IList<object> row, int index, List<Especialidad> lstEspecialidad)
        {
            try
            {


                const int CANTIDAD_CAMPOS = 12;
                ProfesionalModel model = new ProfesionalModel();

                String[] campos = new String[CANTIDAD_CAMPOS];

                for (int i = 0; i < CANTIDAD_CAMPOS; i++)
                {
                    campos[i] = (row.Count > i) ? row[i].ToString() : string.Empty;
                }
                model.index = index;
                string campoDone = campos[0].ToString();
                if (campoDone.Length > 0)
                    return null;
                string campoDni = campos[2].ToString();
                string campoApellido = campos[3].ToString();
                string campoNombre = campos[4].ToString();
                string campoTelefono = campos[5].ToString();
                string campoCorreo = campos[6].ToString();
                string campoMatricula = campos[7].ToString();
                string campoEspecialidad = campos[8].ToString();
                string campoEstablecimiento = campos[9].ToString();
                string campoSexo = campos[11].ToString();

                model.email = (string)campoCorreo;
                model.Establecimiento = campoEstablecimiento;
                if (String.IsNullOrEmpty(campoSexo))
                {
                    model.idSexo = 1;
                }
                else
                {
                    model.idSexo = campoSexo.ToLower().Trim().Equals("femenino") ? 1 : 2;
                }
                model.idTipoDocumento = 1;

                model.matricula = campoMatricula;
                model.numeroDocumento = campoDni;
                model.primerApellido = campoApellido;
                model.primerNombre = campoNombre;
                model.telefono = campoTelefono;
                model.vigente = true;
                model.CargarDatosSisa();
                model.nombreEspecialidad = campoEspecialidad;
                model.especialidad = MachearEspecialidad(model.nombreEspecialidad,lstEspecialidad);


                return model;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static Especialidad MachearEspecialidad(string especialidad, List<Especialidad> lstEspecialidad)
        {
            try
            {



                Especialidad entidad;
                especialidad = especialidad.Trim().ToLower();
                entidad = lstEspecialidad.Find(x => x.nombre.ToLower().Trim() == especialidad);
                if (Object.Equals(null, entidad))
                {
                    // no deberia ser posible que elijan especialidades que no existan 

                    throw new Exception("Se selecciono una especialidad inexistente en la base de datos");


                    //cambiar por : dejar vacio el combo box y seleccionar una especialidad en la vista.
                }
                return entidad;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}