using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using ExceptionHandlingAplicativos.ExceptionHandlers;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> cDimensions = new Dictionary<string, string>();
            try
            {
                //Ejecución en la capa de UI

                int x = 100;
                int z = 0;
                cDimensions.Add("Numero A", x.ToString());
                cDimensions.Add("Numero B", z.ToString());
                int w = x / z;

                //Navega a la capa de negocio
                CapaBusiness();
            }
            catch (Exception e)
            {
                //Agregar información de valor del contexto UI
                UserInterfaceExceptionHandler.HandleException(ref e, cDimensions);
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Programa Terminado");
            Console.ReadLine();
        }

        static void CapaBusiness()
        {
            Dictionary<string, string> cDimensions = new Dictionary<string, string>();
            try
            {
                //Ejecución capa de negocio
                string fileName = @"x:\demo.txt";
                cDimensions.Add("Archivo buscado", fileName);
                File.Open($"{fileName}", FileMode.Open);

                //Navega a capa de datos
                CapaData();
            }
            catch (Exception e)
            {
                //Agregar información de valor del contexto de negocio
                BusinessLogicExceptionHandler.HandleException(ref e, cDimensions);
                Console.WriteLine(e.Message);
            }
        }

        static void CapaData()
        {
            Dictionary<string, string> cDimensions = new Dictionary<string, string>();
            try
            {
                string strCN = "myConnectionString";
                cDimensions.Add("Cadena de conexión", strCN);
                SqlConnection cn = new SqlConnection("myCadenaConexion");
                cn.Open();
            }
            catch (Exception e)
            {
                //Agregar información de valor del contexto de datos
                DataAccessExceptionHandler.HandleException(ref e, cDimensions);
                Console.WriteLine(e.Message);
            }
        }
    }
}
