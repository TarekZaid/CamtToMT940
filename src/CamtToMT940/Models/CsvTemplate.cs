namespace CamtToMT940.Models
{
    public class CsvTemplate
    {
        public string Name { get; set; } = "";
        public string? Encoding { get; set; }
        public string Delimiter { get; set; } = ";";
        public string QuoteChar { get; set; } = "\"";
        public int HeaderRows { get; set; } = 1;
        public string DateFormat { get; set; } = "dd.MM.yyyy";
        public string AmountFormat { get; set; } = "german";
        public string AmountSignSource { get; set; } = "signInAmount";
        public string DefaultGvc { get; set; } = "166";
        public string? DefaultBookingKey { get; set; }

        /// <summary>
        /// Buchungen aufsteigend nach Buchungsdatum sortieren.
        /// true = älteste zuerst (MT940-Konvention).
        /// </summary>
        public bool SortTransactionsAscending { get; set; } = true;

        // --- Salden und Konto-Info aus Kopfzeilen ---

        /// <summary>0-basierte Zeilennummer mit der eigenen IBAN (aus Header).</summary>
        public int? OwnIbanRow { get; set; }

        /// <summary>Spaltenindex der IBAN in dieser Zeile.</summary>
        public int? OwnIbanColumn { get; set; }

        /// <summary>
        /// Spaltenindex in den DATENZEILEN, der die eigene IBAN enthält
        /// Wird nur verwendet, wenn OwnIbanRow/OwnIbanColumn nicht gesetzt sind.
        /// </summary>
        public int? OwnIbanDataColumn { get; set; }

        /// <summary>
        /// Wenn true, wird die eigene IBAN aus dem Dateinamen extrahiert,
        /// falls sie weder im Header noch in den Datenzeilen gefunden wurde.
        /// </summary>
        public bool OwnIbanFromFileName { get; set; } = false;

        /// <summary>0-basierte Zeilennummer mit dem Endsaldo (aus Header).</summary>
        public int? ClosingBalanceRow { get; set; }

        /// <summary>Spaltenindex des Endsaldos.</summary>
        public int? ClosingBalanceColumn { get; set; }

        /// <summary> Spaltenindex in den DATENZEILEN, der den Saldo nach Buchung enthält </summary>
        public int? ClosingBalanceDataColumn { get; set; }

        /// <summary>
        /// Spaltenindex in den DATENZEILEN, der die eigene Kontonummer enthält.
        /// Wird nur verwendet, wenn weder OwnIbanRow/OwnIbanColumn
        /// noch OwnIbanDataColumn noch OwnIbanFromFileName eine IBAN liefern.
        /// </summary>
        public int? OwnAccountNumberDataColumn { get; set; }

        /// <summary>0-basierte Zeilennummer mit dem Anfangssaldo (aus Header). Null = berechnen.</summary>
        public int? OpeningBalanceRow { get; set; }

        /// <summary>Spaltenindex des Anfangssaldos.</summary>
        public int? OpeningBalanceColumn { get; set; }

        public CsvColumnMapping Columns { get; set; } = new();
    }
}