using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;

namespace ScanAndSave.Models
{
    /// 
    /// Repræsenterer en opbevaringsgruppe (f.eks. Køleskab, Skuffe, Fryser).
    /// Hver gruppe kan have et billede og indeholder flere varer.
    ///
    public class StorageGroup
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// 
        /// Navn på gruppen, f.eks. "Køleskab", "Skuffe 1", "Fryser"
        /// 
        public string Name { get; set; } = string.Empty;

        /// 
        /// Sti til billedet af gruppen (taget med kameraet).
        /// Gemmes som filsti på enheden.
        /// 
        public string? ImagePath { get; set; }

        /// 
        /// Dato for oprettelse af gruppen
        /// 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}