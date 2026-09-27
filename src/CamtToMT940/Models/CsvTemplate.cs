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

        /// <summary>0-basierte Zeilennummer mit dem Endsaldo (aus Header).</summary>
        public int? ClosingBalanceRow { get; set; }

        /// <summary>Spaltenindex des Endsaldos.</summary>
        public int? ClosingBalanceColumn { get; set; }

        /// <summary>0-basierte Zeilennummer mit dem Anfangssaldo (aus Header). Null = berechnen.</summary>
        public int? OpeningBalanceRow { get; set; }

        /// <summary>Spaltenindex des Anfangssaldos.</summary>
        public int? OpeningBalanceColumn { get; set; }

        public CsvColumnMapping Columns { get; set; } = new();
    }
}