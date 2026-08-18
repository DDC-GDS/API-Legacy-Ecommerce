using System;
using System.Configuration;
using System.IO;
using System.Text;


namespace SAXServices.Web.Contracts
{
    internal  class Log
    {
        
        public void InicioServicio(string contenido)
        {
            try
            {
                // Ruta base del proyecto (carpeta bin/debug o bin/release)
                string rutaProyecto = AppDomain.CurrentDomain.BaseDirectory;

                // Combina la ruta del proyecto con el nombre del archivo
                string rutaCompleta = Path.Combine(rutaProyecto, ConfigurationManager.AppSettings["archivoLog"]);

                try
                {
                    using (FileStream archivo = new FileStream(rutaCompleta, FileMode.Append, FileAccess.Write))
                    using (StreamWriter escritor = new StreamWriter(archivo, Encoding.UTF8))
                    {
                       // escritor.WriteLine("");
                        string entrada = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INICIO servicio ****: " + contenido;
                        escritor.WriteLine(entrada);

                        Console.WriteLine("Eventos registrados correctamente.");
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error de IO: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error inesperado: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir el archivo: {ex.Message}");
            }
        }

        public void FinServicio(string contenido)
        {
            try
            {
                // Ruta base del proyecto (carpeta bin/debug o bin/release)
                string rutaProyecto = AppDomain.CurrentDomain.BaseDirectory;

                // Combina la ruta del proyecto con el nombre del archivo
                string rutaCompleta = Path.Combine(rutaProyecto, ConfigurationManager.AppSettings["archivoLog"]);

                try
                {
                    using (FileStream archivo = new FileStream(rutaCompleta, FileMode.Append, FileAccess.Write))
                    using (StreamWriter escritor = new StreamWriter(archivo, Encoding.UTF8))
                    {
                     //   escritor.WriteLine("");
                        string entrada = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FIN    servicio ****: " + contenido;
                        escritor.WriteLine(entrada);

                        Console.WriteLine("Eventos registrados correctamente.");
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error de IO: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error inesperado: " + ex.Message);
                }
            }         
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir el archivo: {ex.Message}");
            }
        }
    }
}


