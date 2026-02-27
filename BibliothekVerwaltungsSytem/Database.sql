-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Erstellungszeit: 27. Feb 2026 um 11:45
-- Server-Version: 11.8.3-MariaDB-0+deb13u1 from Debian
-- PHP-Version: 8.4.16

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Datenbank: `pba3h24age`
--
CREATE DATABASE IF NOT EXISTS `pba3h24age` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;
USE `pba3h24age`;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `ausleihen`
--

CREATE TABLE `ausleihen` (
  `ausleihe_id` int(11) NOT NULL,
  `buch_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `ausgeliehen_am` date NOT NULL,
  `rueckgabe_bis` date NOT NULL,
  `zurueckgegeben_am` date DEFAULT NULL,
  `status` enum('aktiv','ueberfaellig','zurueckgegeben') DEFAULT 'aktiv',
  `bemerkungen` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Daten für Tabelle `ausleihen`
--

INSERT INTO `ausleihen` (`ausleihe_id`, `buch_id`, `user_id`, `ausgeliehen_am`, `rueckgabe_bis`, `zurueckgegeben_am`, `status`, `bemerkungen`) VALUES
(4, 4, 1, '2026-01-10', '2026-02-07', NULL, 'aktiv', NULL),
(5, 5, 1, '2026-01-15', '2026-02-12', NULL, 'aktiv', NULL),
(6, 6, 1, '2026-01-20', '2026-02-17', NULL, 'aktiv', NULL),
(7, 4, 9, '2026-02-27', '2026-03-27', NULL, 'aktiv', NULL),
(8, 5, 9, '2026-02-27', '2026-06-05', NULL, 'aktiv', NULL),
(9, 4, 1, '2026-02-27', '2026-03-27', NULL, 'aktiv', NULL);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `autoren`
--

CREATE TABLE `autoren` (
  `autor_id` int(11) NOT NULL,
  `vorname` varchar(50) DEFAULT NULL,
  `nachname` varchar(100) NOT NULL,
  `geburtsjahr` year(4) DEFAULT NULL,
  `nationalitaet` varchar(50) DEFAULT NULL,
  `biografie` text DEFAULT NULL,
  `erstellt_am` timestamp NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Daten für Tabelle `autoren`
--

INSERT INTO `autoren` (`autor_id`, `vorname`, `nachname`, `geburtsjahr`, `nationalitaet`, `biografie`, `erstellt_am`) VALUES
(11, 'J.K.', 'Rowling', '1965', 'Britisch', NULL, '2026-01-27 13:38:39'),
(12, 'J.K.', 'Rowling', '1965', 'Britisch', NULL, '2026-01-27 14:06:56'),
(13, 'George', 'Orwell', '1903', 'Britisch', NULL, '2026-01-27 14:06:56'),
(14, 'Isaac', 'Asimov', '1920', 'Amerikanisch', NULL, '2026-01-27 14:06:56');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `buecher`
--

CREATE TABLE `buecher` (
  `buch_id` int(11) NOT NULL,
  `titel` varchar(255) NOT NULL,
  `isbn` varchar(13) DEFAULT NULL,
  `autor_id` int(11) NOT NULL,
  `kategorie_id` int(11) DEFAULT NULL,
  `erscheinungsjahr` year(4) DEFAULT NULL,
  `verlag` varchar(100) DEFAULT NULL,
  `seitenzahl` int(11) DEFAULT NULL,
  `sprache` varchar(30) DEFAULT 'Deutsch',
  `anzahl_exemplare` int(11) DEFAULT 1,
  `verfuegbar` int(11) DEFAULT 1,
  `beschreibung` text DEFAULT NULL,
  `erstellt_am` timestamp NULL DEFAULT current_timestamp()
) ;

--
-- Daten für Tabelle `buecher`
--

INSERT INTO `buecher` (`buch_id`, `titel`, `isbn`, `autor_id`, `kategorie_id`, `erscheinungsjahr`, `verlag`, `seitenzahl`, `sprache`, `anzahl_exemplare`, `verfuegbar`, `beschreibung`, `erstellt_am`) VALUES
(4, 'Harry Potter und der Stein der Weisen', '9783551551672', 11, NULL, '1997', 'Carlsen', NULL, 'Deutsch', 3, 0, NULL, '2026-01-27 14:10:06'),
(5, '1984', '9783548234106', 12, 4, '1949', 'Ullstein', NULL, 'Deutsch', 2, 0, NULL, '2026-01-27 14:10:06'),
(6, 'Foundation', '9783453317420', 14, NULL, '1951', 'Heyne', NULL, 'Deutsch', 2, 1, NULL, '2026-01-27 14:10:06');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `kategorien`
--

CREATE TABLE `kategorien` (
  `kategorie_id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL,
  `beschreibung` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Daten für Tabelle `kategorien`
--

INSERT INTO `kategorien` (`kategorie_id`, `name`, `beschreibung`) VALUES
(1, 'Fantasy', 'Fantasy und magische Welten'),
(2, 'Krimi', 'Krimis und Thriller'),
(3, 'Science-Fiction', 'Zukunft und Technologie'),
(4, 'Roman', 'Allgemeine Romane'),
(5, 'Sachbuch', 'Wissensvermittlung');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `users`
--

CREATE TABLE `users` (
  `user_id` int(11) NOT NULL,
  `username` varchar(50) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `vorname` varchar(50) NOT NULL,
  `nachname` varchar(50) NOT NULL,
  `email` varchar(100) DEFAULT NULL,
  `rolle` enum('user','admin') DEFAULT 'user',
  `erstellt_am` timestamp NULL DEFAULT current_timestamp(),
  `aktiv` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Daten für Tabelle `users`
--

INSERT INTO `users` (`user_id`, `username`, `password_hash`, `vorname`, `nachname`, `email`, `rolle`, `erstellt_am`, `aktiv`) VALUES
(1, 'user', '$2a$12$nCicQuMACKKB92snZJb0AuO6t9JHly26S.2l92t/bgYh3cwnnVAA2', 'Max', 'Mustermann', 'max@example.com', 'user', '2026-01-27 13:33:04', 1),
(2, 'admin', '$2a$12$GUdAIMv9bUD5TPTGRD9iEecOi9ZCI.NeiM/TiBxS9n9PI3NELLZKu', 'Anna', 'Admin', 'anna@example.com', 'admin', '2026-01-27 13:33:04', 1),
(5, 'Gandon', '$2a$12$ibNuNfYNZ6EnjvBS8XNBJuH3peUTRBUdBWOrKBnwigCMWzTOOirDa', 'Tom', 'Sievert', 'gandon@gmail.com', 'admin', '2026-02-23 10:57:09', 1),
(8, 'GandonDerEchte', '$2a$12$XhblPnqMT3n69jEf1dEOAO98a5s51d6ALpeGgOnSS8qh9ykUnVide', 'Tom', 'Heinrich Sievert', 'gandon843@gmail.com', 'user', '2026-02-23 13:04:46', 1),
(9, 'test', '$2a$12$rWW/LvcaFIfpBbV2ynegY.IXsHygS2f0C3nmaTKNxcm647VJKYw2q', 'test', 'User', 'tes1234@gmail.com', 'user', '2026-02-27 11:09:11', 1);

--
-- Indizes der exportierten Tabellen
--

--
-- Indizes für die Tabelle `ausleihen`
--
ALTER TABLE `ausleihen`
  ADD PRIMARY KEY (`ausleihe_id`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_buch` (`buch_id`),
  ADD KEY `idx_status` (`status`),
  ADD KEY `idx_rueckgabe` (`rueckgabe_bis`);

--
-- Indizes für die Tabelle `autoren`
--
ALTER TABLE `autoren`
  ADD PRIMARY KEY (`autor_id`),
  ADD KEY `idx_nachname` (`nachname`);

--
-- Indizes für die Tabelle `buecher`
--
ALTER TABLE `buecher`
  ADD PRIMARY KEY (`buch_id`),
  ADD UNIQUE KEY `isbn` (`isbn`),
  ADD KEY `kategorie_id` (`kategorie_id`),
  ADD KEY `idx_titel` (`titel`),
  ADD KEY `idx_isbn` (`isbn`),
  ADD KEY `idx_autor` (`autor_id`);

--
-- Indizes für die Tabelle `kategorien`
--
ALTER TABLE `kategorien`
  ADD PRIMARY KEY (`kategorie_id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Indizes für die Tabelle `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`user_id`),
  ADD UNIQUE KEY `username` (`username`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `idx_username` (`username`),
  ADD KEY `idx_rolle` (`rolle`);

--
-- AUTO_INCREMENT für exportierte Tabellen
--

--
-- AUTO_INCREMENT für Tabelle `ausleihen`
--
ALTER TABLE `ausleihen`
  MODIFY `ausleihe_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT für Tabelle `autoren`
--
ALTER TABLE `autoren`
  MODIFY `autor_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT für Tabelle `buecher`
--
ALTER TABLE `buecher`
  MODIFY `buch_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT für Tabelle `kategorien`
--
ALTER TABLE `kategorien`
  MODIFY `kategorie_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT für Tabelle `users`
--
ALTER TABLE `users`
  MODIFY `user_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- Constraints der exportierten Tabellen
--

--
-- Constraints der Tabelle `ausleihen`
--
ALTER TABLE `ausleihen`
  ADD CONSTRAINT `ausleihen_ibfk_1` FOREIGN KEY (`buch_id`) REFERENCES `buecher` (`buch_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `ausleihen_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE;

--
-- Constraints der Tabelle `buecher`
--
ALTER TABLE `buecher`
  ADD CONSTRAINT `buecher_ibfk_1` FOREIGN KEY (`autor_id`) REFERENCES `autoren` (`autor_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `buecher_ibfk_2` FOREIGN KEY (`kategorie_id`) REFERENCES `kategorien` (`kategorie_id`) ON DELETE SET NULL;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
