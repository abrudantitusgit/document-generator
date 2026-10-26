using System;

namespace InterfataActe.DTO
{
    public class ContorRecord
    {
        public Guid Id { get; set; }
        public Guid ProiectID { get; set; }
        public long NrProiect { get; set; }
        public long Contor { get; set; }
        public string Tag { get; set; }
        public string Type { get; set; }
        public string TypeLabel { get; set; }
    }
}
