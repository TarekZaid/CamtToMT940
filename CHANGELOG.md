# Changelog

Alle nennenswerten Änderungen an diesem Projekt werden hier dokumentiert.
Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.1.0/),
Versionierung nach [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Geplant
- CSV → MT940 mit Bank-Templates
- PDF → MT940
- UI-Redesign

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