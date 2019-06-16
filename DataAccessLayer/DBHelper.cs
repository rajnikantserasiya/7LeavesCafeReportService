using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
//using Microsoft.Extensions.Configuration;

namespace Helpers
{
    public static class DBHelper
    {

        //private static string defaultConnectionString = "";

        private static string DefaultConnectionString { get; set; } = "";

        public static void SetDBConnectionString(string connectionString)
        {
            DefaultConnectionString = connectionString;
        }

        public static DataSet ExecuteProcedure(string PROC_NAME, List<SqlParameter> filters)
        {
            try
            {                
                DataSet a = new DataSet();                
                string query = "EXEC " + PROC_NAME;                

                a = Query(query, filters);
                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet ExecuteQuery(string query, List<SqlParameter> filters)
        {
            try
            {                
                DataSet a = new DataSet();               
                a = Query(query, filters);
                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int ExecuteNonQuery(string query, List<SqlParameter> filters)
        {
            try
            {                            
                return NonQuery(query, filters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static object ExecuteScalar(string query, List<SqlParameter> filters)
        {
            try
            {                           
                return Scalar(query, filters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Private Methods

        private static DataSet Query(String consulta, IList<SqlParameter> parametros)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlConnection connection = new SqlConnection(DefaultConnectionString);
                SqlCommand command = new SqlCommand();
                SqlDataAdapter da;
                try
                {
                    command.Connection = connection;
                    command.CommandText = consulta;
                    if (parametros != null)
                    {
                        command.Parameters.AddRange(parametros.ToArray());
                    }
                    da = new SqlDataAdapter(command);
                    da.Fill(ds);
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }
                return ds;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private static int NonQuery(string query, IList<SqlParameter> parametros)
        {
            try
            {
                DataSet dt = new DataSet();
                SqlConnection connection = new SqlConnection(DefaultConnectionString);
                SqlCommand command = new SqlCommand();

                try
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = query;
                    command.Parameters.AddRange(parametros.ToArray());
                    return command.ExecuteNonQuery();

                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static object Scalar(string query, List<SqlParameter> parametros)
        {
            try
            {
                DataSet dt = new DataSet();
                SqlConnection connection = new SqlConnection(DefaultConnectionString);
                SqlCommand command = new SqlCommand();

                try
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = query;
                    command.Parameters.AddRange(parametros.ToArray());
                    return command.ExecuteScalar();

                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }

}