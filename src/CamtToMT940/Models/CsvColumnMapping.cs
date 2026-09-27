using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamtToMT940.Models
{
    /// <summary>
    /// Spalten-Indizes (0-basiert) für jedes relevante Feld im CSV.
    /// Nicht jede Bank hat alle Felder – fehlende bleiben null.
    /// </summary>
    public class CsvColumnMapping
    {
        // Pflichtfelder
        public int? BookingDate { get; set; }
        public int? Amount { get; set; }

        // Datum & Währung
        public int? ValueDate { get; set; }
        public int? Currency { get; set; }

        // Betrag-Alternativen (Deutsche Bank)
        public int? Soll { get; set; }
        public int? Haben { get; set; }

        // Geschäftspartner
        public int? CounterpartyName { get; set; }     // Standard
        public int? CounterpartyName1 { get; set; }    // DKB: Zahlungspflichtige*r
        public int? CounterpartyName2 { get; set; }    // DKB: Zahlungsempfänger*in
        public int? CounterpartyIban { get; set; }
        public int? CounterpartyBic { get; set; }

        // Verwendungszweck
        public int? RemittanceInfo { get; set; }
        public int? BookingText { get; set; }
        public int? AdditionalInfo { get; set; }

        // Referenzen
        public int? CustomerRef { get; set; }          // Kundenreferenz
        public int? CreditorId { get; set; }           // Gläubiger-ID
        public int? MandateId { get; set; }            // Mandatsreferenz
        public int? EndToEndId { get; set; }           // EREF

        // Sonstiges
        public int? Status { get; set; }               // z.B. "Gebucht"
    }
}