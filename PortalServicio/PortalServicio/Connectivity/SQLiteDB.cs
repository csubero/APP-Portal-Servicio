using System;
using SQLite;
using System.IO;
using PortalServicio.Models;
using System.Threading.Tasks;


namespace PortalServicio
{
    public class SQLiteDB
    {
        public async Task<SQLiteAsyncConnection> GetConnection()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(documentsPath, "SPClocalfiles.db3");
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(path);
            try
            {
                await conn.CreateTablesAsync(CreateFlags.None,
             typeof(Incident),
             typeof(Client),
             typeof(Category),
             typeof(Coord),
             typeof(Currency),
             typeof(Subtype),
             typeof(IncidentTechnician),
             typeof(ServiceTicket),
             typeof(ServiceTicketTechnician),
             typeof(Product),
             typeof(SystemUser),
             typeof(ProductStorage),
             typeof(Technician),
             typeof(MaterialYRepuesto),
             typeof(Note),
             typeof(CDT),
             typeof(ProjectEquipment),
             typeof(ProjectMaterial),
             typeof(CrudTable),
             typeof(LogTable),
             typeof(EquipmentRequestOrder),
             typeof(MaterialRequestOrder),
             typeof(LineEquipmentRequestOrder),
             typeof(LineMaterialRequestOrder),
             typeof(CDTTicket),
             typeof(TechnicianRegistry),
             typeof(ExtraEquipmentRequest),
             typeof(Legalization),
             typeof(LegalizationItem),
             typeof(Company)
             //typeof(LegalizationCompany)
             );
            }
            catch (Exception)
            {
            }
            return conn;
        }
    }
}
