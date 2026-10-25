using System;
using System.Collections.Generic;

namespace CamtToMT940.Models
{
    public class BankStatement
    {
        public string Iban { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Currency { get; set; } = "EUR";
        public DateTime? OpeningDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public bool OpeningIsCredit { get; set; } = true;
        public DateTime? ClosingDate { get; set; }
        public decimal ClosingBalance { get; set; }
        public bool ClosingIsCredit { get; set; } = true;
        public List<BankTransaction> Transactions { get; set; } = new();
    }

    public class BankTransaction
    {
        public DateTime BookingDate { get; set; }
        public DateTime ValueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }   // true = Gutschrift, false = Lastschrift
        public string Currency { get; set; } = "EUR";

        public string? EndToEndId { get; set; }
        public string? AcctSvcrRef { get; set; }
        public string? MandateId { get; set; }
        public string? PurposeCode { get; set; }

        public string? DebtorName { get; set; }
        public string? DebtorIban { get; set; }
        public string? DebtorBic { get; set; }

        public string? CreditorName { get; set; }
        public string? CreditorIban { get; set; }
        public string? CreditorBic { get; set; }

        public string? RemittanceInfo { get; set; }
        public string? AdditionalInfo { get; set; }

        public string? BookingText { get; set; }
        public string? TransactionCode { get; set; }

        // --- Overrides, die der Mapper setzt (z.B. aus CSV-Template) ---

        /// <summary>
        /// Erzwingt einen bestimmten GVC für ?00 (z.B. "166").
        /// </summary>
        public string? GvcOverride { get; set; }

        /// <summary>
        /// Erzwingt einen bestimmten Buchungsschlüssel für ?10 (z.B. "931").
        /// </summary>
        public string? BookingKeyOverride { get; set; }

        // --- Hilfsmethoden ---

        public string? GetCounterpartyName()
            => IsCredit ? DebtorName : CreditorName;

        public string? GetCounterpartyIban()
            => IsCredit ? DebtorIban : CreditorIban;

        public string? GetCounterpartyBic()
            => IsCredit ? DebtorBic : CreditorBic;
    }
}