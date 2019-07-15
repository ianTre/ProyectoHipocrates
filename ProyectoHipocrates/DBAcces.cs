using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace ProyectoHipocrates
{
    public class DBAcces
    {
        private SqlConnection con;

        public SqlConnection GetConnection()
        {
            //string sqlcon = "Data Source=localhost;" +
            //                "Initial Catalog=KLINICOS_CENTRAL;" +
            //                "persist security info = True;" +
            //                "Integrated Security=SSPI;";
            this.con = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefoultConection"].ConnectionString);
            return this.con;
        }


        public SqlConnection GetConnection(string nombreBase)
        {
            //string sqlcon = "Data Source=localhost;" +
            //                "Initial Catalog=KLINICOS_CENTRAL;" +
            //                "persist security info = True;" +
            //                "Integrated Security=SSPI;";
            this.con = new SqlConnection(WebConfigurationManager.ConnectionStrings[nombreBase].ConnectionString);
            return this.con;
        }
    }
}