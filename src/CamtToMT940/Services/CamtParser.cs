using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class CamtParser
    {
        // Namespaces für camt.052 und camt.053
        private static readonly XNamespace Ns052 =
            "urn:iso:std:iso:20022:tech:xsd:camt.052.001.08";
        private static readonly XNamespace Ns053 =
            "urn:iso:std:iso:20022:tech:xsd:camt.053.001.08";

        public List<BankStatement> Parse(string filePath)
        {
            var doc = XDocument.Load(filePath);
            return ParseDocument(doc);
        }

        public List<BankStatement> ParseContent(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            return ParseDocument(doc);
        }

        private List<BankStatement> ParseDocument(XDocument doc)
        {
            var root = doc.Root;
            if (root == null)
                throw new InvalidDataException("XML-Dokument hat kein Wurzelelement.");

            XNamespace ns = root.Name.Namespace;
            var result = new List<BankStatement>();

            // camt.052: BkToCstmrAcctRpt / Rpt
            // camt.053: BkToCstmrStmt / Stmt
            var reports = root.Descendants(ns + "Rpt").ToList();
            if (reports.Count == 0)
                reports = root.Descendants(ns + "Stmt").ToList();

            foreach (var report in reports)
            {
                var statement = ParseStatement(report, ns);
                result.Add(statement);
            }

            return result;
        }

        private BankStatement ParseStatement(XElement report, XNamespace ns)
        {
            var stmt = new BankStatement();

            // Konto
            var acct = report.Element(ns + "Acct");
            if (acct != null)
            {
                stmt.Iban = acct.Element(ns + "Id")?
                    .Element(ns + "IBAN")?.Value ?? string.Empty;
                stmt.Currency = acct.Element(ns + "Ccy")?.Value ?? "EUR";
                stmt.OwnerName = acct.Element(ns + "Ownr")?
                    .Element(ns + "Nm")?.Value ?? string.Empty;
            }

            // Salden (OPBD = Opening, CLBD = Closing)
            foreach (var bal in report.Elements(ns + "Bal"))
            {
                var typeCode = bal.Element(ns + "Tp")?
                    .Element(ns + "CdOrPrtry")?
                    .Element(ns + "Cd")?.Value;

                var amountStr = bal.Element(ns + "Amt")?.Value ?? "0";
                var ccy = bal.Element(ns + "Amt")?.Attribute("Ccy")?.Value ?? "EUR";
                var cdtDbt = bal.Element(ns + "CdtDbtInd")?.Value ?? "CRDT";
                var dateStr = bal.Element(ns + "Dt")?.Element(ns + "Dt")?.Value;

                decimal.TryParse(amountStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var amount);
                DateTime.TryParse(dateStr, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date);

                if (typeCode == "OPBD")
                {
                    stmt.OpeningBalance = amount;
                    stmt.OpeningIsCredit = cdtDbt == "CRDT";
                    stmt.OpeningDate = date == default ? null : date;
                    if (!string.IsNullOrEmpty(ccy)) stmt.Currency = ccy;
                }
                else if (typeCode == "CLBD")
                {
                    stmt.ClosingBalance = amount;
                    stmt.ClosingIsCredit = cdtDbt == "CRDT";
                    stmt.ClosingDate = date == default ? null : date;
                }
            }

            // Buchungen
            foreach (var entry in report.Elements(ns + "Ntry"))
            {
                var tx = ParseEntry(entry, ns);
                if (tx != null)
                    stmt.Transactions.Add(tx);
            }

            return stmt;
        }

        private BankTransaction? ParseEntry(XElement entry, XNamespace ns)
        {
            // Status prüfen – nur gebuchte (BOOK) verwenden
            var status = entry.Element(ns + "Sts")?.Element(ns + "Cd")?.Value;
            if (status != "BOOK")
                return null;

            var tx = new BankTransaction();

            // Betrag + Richtung
            var amountStr = entry.Element(ns + "Amt")?.Value ?? "0";
            var ccy = entry.Element(ns + "Amt")?.Attribute("Ccy")?.Value ?? "EUR";
            var cdtDbt = entry.Element(ns + "CdtDbtInd")?.Value ?? "CRDT";

            decimal.TryParse(amountStr, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var amount);
            tx.Amount = amount;
            tx.Currency = ccy;
            tx.IsCredit = cdtDbt == "CRDT";

            // Datum
            var bookgDateStr = entry.Element(ns + "BookgDt")?
                .Element(ns + "Dt")?.Value;
            var valDateStr = entry.Element(ns + "ValDt")?
                .Element(ns + "Dt")?.Value;

            DateTime.TryParse(bookgDateStr, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var bookgDate);
            DateTime.TryParse(valDateStr, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var valDate);
            tx.BookingDate = bookgDate;
            tx.ValueDate = valDate == default ? bookgDate : valDate;

            // Bankinterne Referenz
            tx.AcctSvcrRef = entry.Element(ns + "AcctSvcrRef")?.Value;

            // Buchungscode
            var bkTxCd = entry.Element(ns + "BkTxCd");
            tx.TransactionCode = bkTxCd?
                .Element(ns + "Prtry")?
                .Element(ns + "Cd")?.Value;

            // Zusätzlicher Buchungstext
            tx.AdditionalInfo = entry.Element(ns + "AddtlNtryInf")?.Value;

            // Transaktionsdetails
            var txDtls = entry.Element(ns + "NtryDtls")?
                .Element(ns + "TxDtls");
            if (txDtls != null)
            {
                ParseTxDtls(txDtls, tx, ns);
            }

            return tx;
        }

        private void ParseTxDtls(XElement txDtls, BankTransaction tx, XNamespace ns)
        {
            // Referenzen
            var refs = txDtls.Element(ns + "Refs");
            if (refs != null)
            {
                tx.EndToEndId = refs.Element(ns + "EndToEndId")?.Value;
                tx.MandateId = refs.Element(ns + "MndtId")?.Value;
                tx.AcctSvcrRef ??= refs.Element(ns + "AcctSvcrRef")?.Value;
            }

            // Zweck
            tx.PurposeCode = txDtls.Element(ns + "Purp")?
                .Element(ns + "Cd")?.Value;

            // Verwendungszweck
            tx.RemittanceInfo = txDtls.Element(ns + "RmtInf")?
                .Element(ns + "Ustrd")?.Value;

            // Parteien
            var parties = txDtls.Element(ns + "RltdPties");
            if (parties != null)
            {
                var dbtr = parties.Element(ns + "Dbtr");
                if (dbtr != null)
                {
                    tx.DebtorName = dbtr.Element(ns + "Pty")?
                        .Element(ns + "Nm")?.Value;
                }
                var dbtrAcct = parties.Element(ns + "DbtrAcct");
                if (dbtrAcct != null)
                {
                    tx.DebtorIban = dbtrAcct.Element(ns + "Id")?
                        .Element(ns + "IBAN")?.Value;
                }

                var cdtr = parties.Element(ns + "Cdtr");
                if (cdtr != null)
                {
                    tx.CreditorName = cdtr.Element(ns + "Pty")?
                        .Element(ns + "Nm")?.Value;
                }
                var cdtrAcct = parties.Element(ns + "CdtrAcct");
                if (cdtrAcct != null)
                {
                    tx.CreditorIban = cdtrAcct.Element(ns + "Id")?
                        .Element(ns + "IBAN")?.Value;
                }
            }

            // Banken (BICs)
            var agents = txDtls.Element(ns + "RltdAgts");
            if (agents != null)
            {
                tx.DebtorBic = agents.Element(ns + "DbtrAgt")?
                    .Element(ns + "FinInstnId")?
                    .Element(ns + "BICFI")?.Value;
                tx.CreditorBic = agents.Element(ns + "CdtrAgt")?
                    .Element(ns + "FinInstnId")?
                    .Element(ns + "BICFI")?.Value;
            }
        }
    }
}