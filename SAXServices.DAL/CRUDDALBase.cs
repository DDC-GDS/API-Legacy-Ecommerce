using System.Data;
using System.Data.SqlClient;

namespace SAXServices.DAL
{
    public class CRUDDALBase
    {
        #region Variables Database
        protected SqlConnection oConexion;

        protected SqlTransaction oTran;
        #endregion

        #region Database Methods
        /// <summary>
        /// Establece una conexión con la base de datos
        /// </summary>
        /// <returns>True si la conexión se abrió con éxito. False en caso contrario.</returns>
        protected bool OpenDBConnection(string connectionString)
        {
            oConexion = new SqlConnection(connectionString);
            oConexion.Open();

            return oConexion.State == ConnectionState.Open;
        }

        /// <summary>
        /// Abre una transacción SQL
        /// </summary>
        protected void OpenDBTransaction()
        {
            if (oConexion.State == System.Data.ConnectionState.Open)
            {
                oTran = oConexion.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            }
        }

        /// <summary>
        /// Cierra una transacción SQL revirtiendo todos los cambios
        /// </summary>
        protected void RollBackDBTransaction()
        {
            if (oConexion.State != System.Data.ConnectionState.Closed && oTran != null)
            {
                oTran.Rollback();
                oTran = null;
            }
        }

        /// <summary>
        /// Cierra una transacción SQL confirmando todos los cambios
        /// </summary>
        protected void CommitDBTransaction()
        {
            if (oConexion.State != System.Data.ConnectionState.Closed && oTran != null)
            {
                oTran.Commit();
                oTran = null;
            }
        }

        /// <summary>
        /// Cierra la conexión a la base de datos
        /// </summary>
        protected void CloseDBConnection()
        {
            if (oTran != null) RollBackDBTransaction();
            if (oConexion.State == System.Data.ConnectionState.Open) oConexion.Close();
        }

        protected SqlParameter crearParametro(string nombre, DbType tipo, object valor, ParameterDirection parameterDirection =ParameterDirection.Input )
        {
            SqlParameter parametro = new SqlParameter();
            parametro.ParameterName = nombre;
            parametro.DbType = tipo;
            parametro.Value = valor;
            parametro.Direction = parameterDirection; 
            return parametro;
        }
        #endregion
    }
}
