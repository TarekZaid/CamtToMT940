# Changelog

Alle nennenswerten Änderungen an diesem Projekt werden hier dokumentiert.
Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.1.0/),
Versionierung nach [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Geplant
- PDF → MT940

## [0.2.0] – 2026-11-06

### Hinzugefügt
- **CSV → MT940** mit Bank-Templates
- Templates für GLS Bank, ING, HypoVereinsbank, Deutsche Bank, DKB
  (Kontoauszug, Apple-Umsatzliste, Tab-getrennt), Sparkasse
- **Jahresfilter** für CSV-Dateien: ComboBox „Nur Jahr" (2020–2030),
  Filterung auf Buchungs- **und** Valuta-Datum, Ausgabedatei mit
  Jahres-Suffix (`_25.txt`)
- **Manuelle Angaben** im CSV-Tab: IBAN, BLZ, Kontonr., Anfangssaldo,
  Endsaldo (überschreiben Datei-Werte, wenn ausgefüllt)
- **Mod-97-IBAN-Berechnung** aus BLZ + Kontonummer (für Hypo)
- **Verwendungszweck-Aufteilung** auf `?23`–`?31` (mehrzeilig, DATEV-konform)
- **UI-Redesign**: CAMT-Tab mit großer Eingabeliste, CSV-Tab mit
  Eingabeliste links und manuellen Feldern rechts
- **`#if DEBUG`** um die Debug-Ausgabe (Release erzeugt keine `_debug.txt`)
- **Encoding-Erkennung** für UTF-8, UTF-16 (LE/BE), Windows-1252
- **Templates als EmbeddedResource**: alle JSON-Templates sind in die
  `.exe` eingebettet – kein separater `templates/`-Ordner mehr nötig

### Geändert
- `Mt940Writer`: Verwendungszweck wird auf mehrere `?xx`-Felder verteilt
  (max. 27 Zeichen pro Feld), damit DATEV den vollen Text anzeigt
- `CsvConversionService`: Endsaldo aus der zeitlich letzten Buchung
  (nicht aus der letzten CSV-Zeile) lesen
- `MainForm`: Ausgabelisten entfernt (Dateien landen im Zielordner),
  Erfolgszählung nur noch in der Statusleiste
- Auslieferung als einzelne `.exe` (Self-Contained, Single-File)

### Behoben
- HypoVereinsbank: IBAN fehlte (jetzt aus BLZ + Konto berechnet)
- Verwendungszweck wurde in DATEV nur einzeilig angezeigt

### Getestet mit
- CAMT.052/053.001.08 (Volksbank, Bundesbank)
- CSV: GLS, ING, Hypo (2024, 2026), Deutsche Bank, DKB, Sparkasse
- DATEV-Import erfolgreich für GLS, ING, Hypo (2024, 2026), Deutsche Bank, DKB

### Bekannt
- Hypo 2025: DATEV-Test mit Jahresfilter steht noch aus
- Sparkasse: DATEV-Test steht noch aus

## [0.1.1] – 2026-09-24

### Behoben
- Programm startet jetzt zuverlässig auf Windows 10 und 11
- Auslieferung als eigenständige `.exe` (Self-Contained, keine .NET-Installation nötig)
- Absturz beim Start auf Rechnern ohne lokalen `testdata`-Ordner behoben

### Entfernt
- Debug-Methode `TestParser()` aus `MainForm`

## [0.1.0] – 2026-09-22

### Hinzugefügt
- CAMT.052 und CAMT.053 (Version 08) Parser
- MT940-Writer mit DATEV-konformer `:86:`-Struktur
  (`?00`, `?10`, `?20`–`?29`, `?30`, `?31`, `?32`/`?33`)
- WinForms-UI mit Drag & Drop, Strg+V, Ordnerauswahl
- Getrennte Ein-/Ausgabeliste
- Kollisionsabfrage beim Überschreiben
- Ausgabe als `.txt` in ISO-8859-1

### Bekannt
- Nur CAMT V08 getestet