using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanAndSave.Services
{
    /// Hjælpeklasse til validering og håndtering af stregkoder.
    /// EAN-13 er standard for dagligvarer i Danmark/Europa.

    public static class BarcodeHelper
    {
        /// Validerer om en stregkode er gyldig EAN-13 format.
        /// EAN-13 har præcis 13 cifre og en korrekt checksum.

        public static bool IsValidEAN13(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            if (barcode.Length != 13)
                return false;

            // Tjek at alle tegn er tal
            if (!barcode.All(char.IsDigit))
                return false;

            // Beregn og valider checksum (ciffer 13)
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = barcode[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (barcode[12] - '0');
        }


        /// Validerer EAN-8 stregkoder (kortere variant brugt på små produkter)

        public static bool IsValidEAN8(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            if (barcode.Length != 8)
                return false;

            if (!barcode.All(char.IsDigit))
                return false;

            int sum = 0;
            for (int i = 0; i < 7; i++)
            {
                int digit = barcode[i] - '0';
                sum += (i % 2 == 0) ? digit * 3 : digit;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (barcode[7] - '0');
        }


        /// Tjekker om stregkoden er en gyldig type (EAN-8 eller EAN-13)

        public static bool IsValidBarcode(string barcode)
        {
            return IsValidEAN13(barcode) || IsValidEAN8(barcode);
        }


        /// Forsøger at udlede landekode fra EAN-13 præfiks.
        /// De første 2-3 cifre angiver oprindelseslandet.

        public static string GetCountryFromEAN(string barcode)
        {
            if (barcode.Length < 3)
                return "Ukendt";

            string prefix = barcode[..3];
            int code = int.Parse(prefix);

            return code switch
            {
                >= 570 and <= 579 => "Danmark",
                >= 700 and <= 709 => "Norge",
                >= 730 and <= 739 => "Sverige",
                >= 640 and <= 649 => "Finland",
                >= 400 and <= 440 => "Tyskland",
                >= 300 and <= 379 => "Frankrig",
                >= 800 and <= 839 => "Italien",
                >= 840 and <= 849 => "Spanien",
                >= 000 and <= 139 => "USA/Canada",
                >= 450 and <= 459 or >= 490 and <= 499 => "Japan",
                >= 690 and <= 699 => "Kina",
                _ => "Ukendt"
            };
        }
    }
}
