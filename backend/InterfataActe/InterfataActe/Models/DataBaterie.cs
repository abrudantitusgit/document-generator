using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace InterfataActe.Models
{
    public class DataBaterie
    {
    [Key]
    public Guid Id { get; set; }
            // FK – nume ALINIAT cu navigation-ul
    public Guid ProiectFotovoltaiceId { get; set; }
    public string Model { get; set; } = string.Empty;

    public int NrBaterii { get; set; }

    public decimal PutereInstalata { get; set; }
    public decimal Capacitate {get;set;}
    public string Serie {get;set;}


    // navigation către părinte
    [JsonIgnore]
    public ProiectFotovoltaice ProiectFotovoltaice { get; set; } = null!;
    }
}
