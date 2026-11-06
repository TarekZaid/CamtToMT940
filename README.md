# CamtToMT940

Ein Windows-Tool zur Konvertierung von Bankauszügen ins MT940-Format für den DATEV-Import.

## Motivation

Banken stellen auf CAMT (ISO 20022) um, DATEV erwartet MT940. Dieses Tool schließt die Lücke.

## Funktionen

- **CAMT.052 / CAMT.053** (Version 08) → MT940
- **CSV** → MT940 mit Bank-Templates (GLS, ING, HypoVereinsbank, Deutsche Bank, DKB, Sparkasse)
- **Jahresfilter** für CSV-Dateien mit mehreren Jahren
- **Manuelle Angaben** (IBAN, BLZ, Kontonr., Anfangs-/Endsaldo) als Override
- **Drag & Drop** von Dateien und Ordnern
- **Copy & Paste** von Pfaden (Strg+V)
- **Stapelverarbeitung** ganzer Ordner
- **DATEV-konforme Ausgabe** (ISO-8859-1, `.txt`)
- **Ausgabeort**: gleicher Ordner wie Original
- **Eigenständige `.exe`** – keine Installation, keine separaten Ordner nötig

## Status

✅ **v0.2.0** – CAMT → MT940 und CSV → MT940 funktionieren, in DATEV getestet

### Getestete Banken

| Bank | Format | Status |
|---|---|---|
| Volksbank | CAMT.052.001.08 | ✅ DATEV |
| Deutsche Bundesbank | CAMT.053.001.08 | ✅ DATEV |
| GLS Bank | CSV | ✅ DATEV |
| ING | CSV | ✅ DATEV |
| HypoVereinsbank | CSV | ✅ DATEV (2024, 2026) |
| Deutsche Bank | CSV | ✅ DATEV |
| DKB | CSV | ✅ DATEV |
| Sparkasse | CSV | ⚠️ noch nicht in DATEV getestet |

### Geplant
- [ ] PDF → MT940 (mittelfristig)
- [ ] Sparkasse: DATEV-Test
- [ ] HypoVereinsbank 2025: DATEV-Test (Jahresfilter)

## Installation

### Für Anwender
1. Aktuelle Version unter [Releases](../../releases) herunterladen
2. `CamtToMT940.exe` per Doppelklick starten – keine Installation nötig
3. Falls Windows SmartScreen warnt: „Weitere Informationen" → „Trotzdem ausführen"

**Voraussetzung**: Windows 10/11 – keine .NET-Installation nötig

### Für Entwickler
```bash
git clone https://github.com/TarekZaid/CamtToMT940.git
```
'CamtToMT940.sln' in Visual Studio 2022 öffnen, F5.

**Voraussetzungen**: .NET 8 SDK, Visual Studio 2022

## Verwendung
CAMT → MT940

    1. CAMT-Dateien per Drag & Drop, „Ordner auswählen" oder Strg+V hinzufügen

    2. „Konvertieren" klicken

    3. '.txt'-Dateien liegen neben den Originalen

    4. In DATEV importieren

CSV → MT940

    1. CSV-Dateien im CSV-Tab hinzufügen

    2. Bank-Template auswählen

    3. Optional: IBAN, BLZ, Kontonr., Anfangs-/Endsaldo eintragen
    (nur nötig, wenn die Datei sie nicht enthält, z. B. Hypo)

    4. Optional: „Nur Jahr" wählen (z. B. 2025), um nur Buchungen aus
    diesem Jahr zu exportieren (Buchungs- und Valuta-Datum)

    5. „Konvertieren" klicken

## Bekannte Einschränkungen

    - CAMT nur Version 08 getestet

    - Sparkasse: DATEV-Test steht noch aus

    - HypoVereinsbank 2025: DATEV-Test mit Jahresfilter steht noch aus

    - PDF-Import noch nicht verfügbar

## Feedback

Fehlerberichte und Verbesserungsvorschläge bitte als Issue.
## Lizenz

MIT