using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;

namespace ScanAndSave.Models
{
    /// Repræsenterer en vare scannet med stregkode.
    /// Tilhører en StorageGroup (køleskab, skuffe osv.)

    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// Stregkoden (EAN-13) scannet fra varen
        public string Barcode { get; set; } = string.Empty;

        /// Navn på varen (f.eks. "Arla Letmælk 1L")
        
        public string Name { get; set; } = string.Empty;

        /// Kategori valgt af brugeren (f.eks. "Mejeri", "Kød", "Drikkevarer")
        public string Category { get; set; } = string.Empty;

        /// Antal af varen
        public int Quantity { get; set; } = 1;

        /// Udløbsdato (valgfrit – kan være null)
        public DateTime? ExpiryDate { get; set; }

        /// Dato varen blev tilføjet
        public DateTime AddedDate { get; set; } = DateTime.Now;

        /// Fremmednøgle til StorageGroup (hvilken gruppe varen tilhører)
        [Indexed]
        public int StorageGroupId { get; set; }

        /// Bemærkninger (valgfrit)
        public string? Notes { get; set; }
    }
}