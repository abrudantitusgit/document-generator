using System.Globalization;
using InterfataActe.Models;
using System.Collections.Generic;
using System.Linq;
namespace InterfataActe.Helpers
{

public static class ExportHelpers
{
    public static string BuildInvertor(List<DataInvertor> d)
    {
        var result = string.Join("",d.Select(x => $"{x.NrInvertoare} buc. invertor {x.Model}, putere {x.PutereInstalata} kW, serie {x.Serie}\n"));
        return result;
    }

        public static string BuildPanouri(List<DataPanou> d)
    {
        var result = string.Join("",d.Select(x => $"{x.NrPanouri} buc. panouri {x.Model}, putere {x.PutereInstalata} W, serii {x.Serie}\n"));
        return result;
    }

        public static string BuildBaterii(List<DataBaterie> d)
    {
        var result = string.Join("",d.Select(x => $"{x.NrBaterii} buc. baterie {x.Model}, putere {x.PutereInstalata} kW, capacitate {x.Capacitate} Ah, serie {x.Serie}\n"));
        return result;
    }

}
}