# CamtToMT940

Ein Windows-Tool zur Konvertierung von Bankauszügen ins MT940-Format für den DATEV-Import.

## Motivation

Banken stellen auf CAMT (ISO 20022) um, DATEV erwartet MT940. Dieses Tool schließt die Lücke.

## Funktionen

- **CAMT.052 / CAMT.053** (Version 08) → MT940
- **Drag & Drop** von Dateien und Ordnern
- **Copy & Paste** von Pfaden (Strg+V)
- **Stapelverarbeitung** ganzer Ordner
- **Getrennte Ein-/Ausgabelisten**
- **DATEV-konforme Ausgabe** (ISO-8859-1, `.txt`)
- **Ausgabeort**: gleicher Ordner wie Original

## Status

✅ **v0.1.1** – CAMT → MT940 funktioniert, in DATEV getestet

### Geplant
- [ ] CSV → MT940 mit Bank-Templates
- [ ] PDF → MT940 (mittelfristig)
- [ ] UI-Redesign

## Installation

### Für Anwender
1. Aktuelle Version unter [Releases](../../releases) herunterladen
2. `CamtToMT940.exe` per Doppelklick starten

**Voraussetzung**: Windows 10/11 – keine .NET-Installation nötig

**Hinweis**: Beim ersten Start kann Windows SmartScreen eine Warnung anzeigen. 
Klicke auf „Weitere Informationen" → „Trotzdem ausführen".

### Für Entwickler
```bash
git clone https://github.com/TarekZaid/CamtToMT940.git
```
CamtToMT940.sln in Visual Studio 2022 öffnen, F5.

**Voraussetzungen**: .NET 8 SDK, Visual Studio 2022

## Verwendung

1. Dateien per Drag & Drop, „Ordner auswählen" oder Strg+V hinzufügen
2. „Konvertieren" klicken
3. .txt-Dateien liegen neben den Originalen
4. In DATEV importieren

## Bekannte Einschränkungen

- Getestet mit CAMT.052/053 Version 08 (Volksbank, Bundesbank)
- Andere CAMT-Versionen (V02) möglicherweise nicht unterstützt
- CSV-Import noch nicht verfügbar

## Feedback

Fehlerberichte und Verbesserungsvorschläge bitte als Issue.

## Lizenz

MIT