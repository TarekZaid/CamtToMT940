# CamtToMT940

Ein Windows-Tool zur Konvertierung von Bankauszügen ins MT940-Format für den DATEV-Import.

## Motivation

Viele Banken stellen ihre Kontoauszüge auf das moderne CAMT-Format (ISO 20022) um. 
DATEV und viele Buchhaltungsprogramme erwarten jedoch weiterhin das klassische 
MT940-Format. Dieses Tool schließt die Lücke: Es liest CAMT-Dateien ein und 
erzeugt daraus DATEV-kompatible MT940-Dateien.

## Funktionen

- **CAMT.052 und CAMT.053** (Version 08) → MT940
- **Drag & Drop** von Dateien und Ordnern
- **Copy & Paste** von Pfaden aus dem Explorer (Strg+V)
- **Stapelverarbeitung** ganzer Ordner
- **Getrennte Ein-/Ausgabelisten** für bessere Übersicht
- **Ausgabe** als `.txt` im DATEV-kompatiblen MT940-Format (ISO-8859-1)
- **Ausgabeort** identisch mit dem Quellordner

## Status

🚧 **Phase 1 abgeschlossen**: CAMT → MT940

### Geplant (Phase 2)
- [ ] CSV → MT940 mit Bank-spezifischen Templates
- [ ] Template-Editor für eigene Bankformate
- [ ] Erweiterte Fehlerprotokollierung

## Installation

### Für Anwender
1. Aktuelle Version unter [Releases](../../releases) herunterladen
2. ZIP-Datei entpacken
3. `CamtToMT940.exe` starten

**Voraussetzung**: Windows 10 oder 11

### Für Entwickler
```bash
git clone https://github.com/TarekZaid/CamtToMT940.git
cd CamtToMT940
