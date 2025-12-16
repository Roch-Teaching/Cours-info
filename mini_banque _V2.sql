-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1
-- Généré le : mar. 16 déc. 2025 à 11:55
-- Version du serveur : 10.4.32-MariaDB
-- Version de PHP : 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de données : `mini_banque`
--

-- --------------------------------------------------------

--
-- Structure de la table `comptes`
--

CREATE TABLE `comptes` (
  `id_compte` int(11) NOT NULL,
  `id_client` int(11) NOT NULL,
  `solde` decimal(12,2) NOT NULL DEFAULT 0.00,
  `type` enum('risque','epargne','courant') NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Déchargement des données de la table `comptes`
--

INSERT INTO `comptes` (`id_compte`, `id_client`, `solde`, `type`) VALUES
(1, 1, 1200.00, ''),
(2, 1, 300.00, 'courant'),
(3, 3, 1200.00, ''),
(4, 3, 300.00, 'courant'),
(5, 1, 1200.00, ''),
(6, 1, 300.00, 'courant'),
(7, 3, 1200.00, ''),
(8, 3, 300.00, 'courant'),
(9, 3, 1200.00, ''),
(10, 3, 300.00, 'courant'),
(11, 3, 1200.00, ''),
(12, 3, 300.00, 'courant'),
(13, 3, 1200.00, ''),
(14, 3, 300.00, 'courant'),
(15, 3, 1200.00, ''),
(16, 3, 300.00, 'courant'),
(17, 3, 1200.00, ''),
(18, 3, 300.00, 'courant'),
(19, 3, 1200.00, ''),
(20, 3, 300.00, 'courant'),
(21, 3, 1200.00, ''),
(22, 3, 300.00, 'courant'),
(23, 3, 1200.00, ''),
(24, 3, 300.00, 'courant'),
(25, 3, 1200.00, ''),
(26, 3, 300.00, 'courant'),
(27, 3, 1200.00, ''),
(28, 3, 300.00, 'courant'),
(29, 3, 1200.00, ''),
(30, 3, 300.00, 'courant'),
(31, 3, 1236.00, 'epargne'),
(32, 3, 300.00, 'courant'),
(33, 3, 1236.00, 'epargne'),
(34, 3, 300.00, 'courant');

-- --------------------------------------------------------

--
-- Structure de la table `transaction`
--

CREATE TABLE `transaction` (
  `id_transaction` int(11) NOT NULL,
  `id_compte_source` int(11) NOT NULL,
  `typetransaction` varchar(50) NOT NULL,
  `montant` decimal(12,2) NOT NULL,
  `date_transaction` datetime NOT NULL DEFAULT current_timestamp(),
  `description` text DEFAULT NULL,
  `id_compte_destination` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Déchargement des données de la table `transaction`
--

INSERT INTO `transaction` (`id_transaction`, `id_compte_source`, `typetransaction`, `montant`, `date_transaction`, `description`, `id_compte_destination`) VALUES
(24, 15, 'virement', 300.00, '2025-12-16 10:58:56', 'Virement test', 16),
(25, 17, 'depot', 1000.00, '2025-12-16 11:10:53', 'Dépôt initial', NULL),
(26, 17, 'depot', 500.00, '2025-12-16 11:10:53', 'Dépôt test', NULL),
(27, 17, 'virement', 300.00, '2025-12-16 11:10:53', 'Virement test', 18),
(28, 19, 'depot', 1000.00, '2025-12-16 11:11:15', 'Dépôt initial', NULL),
(29, 19, 'depot', 500.00, '2025-12-16 11:11:16', 'Dépôt test', NULL),
(30, 19, 'virement', 300.00, '2025-12-16 11:11:16', 'Virement test', 20),
(31, 21, 'depot', 1000.00, '2025-12-16 11:15:55', 'Dépôt initial', NULL),
(32, 21, 'depot', 500.00, '2025-12-16 11:15:55', 'Dépôt test', NULL),
(33, 21, 'virement', 300.00, '2025-12-16 11:15:56', 'Virement test', 22),
(34, 23, 'depot', 1000.00, '2025-12-16 11:33:11', 'Dépôt initial', NULL),
(35, 23, 'depot', 500.00, '2025-12-16 11:33:11', 'Dépôt test', NULL),
(36, 23, 'virement', 300.00, '2025-12-16 11:33:12', 'Virement test', 24),
(37, 25, 'depot', 1000.00, '2025-12-16 11:34:14', 'Dépôt initial', NULL),
(38, 25, 'depot', 500.00, '2025-12-16 11:34:14', 'Dépôt test', NULL),
(39, 25, 'virement', 300.00, '2025-12-16 11:34:14', 'Virement test', 26),
(40, 27, 'depot', 1000.00, '2025-12-16 11:34:32', 'Dépôt initial', NULL),
(41, 27, 'depot', 500.00, '2025-12-16 11:34:33', 'Dépôt test', NULL),
(42, 27, 'virement', 300.00, '2025-12-16 11:34:33', 'Virement test', 28),
(43, 29, 'depot', 1000.00, '2025-12-16 11:36:20', 'Dépôt initial', NULL),
(44, 29, 'depot', 500.00, '2025-12-16 11:36:21', 'Dépôt test', NULL),
(45, 29, 'virement', 300.00, '2025-12-16 11:36:21', 'Virement test', 30),
(46, 31, 'depot', 1000.00, '2025-12-16 11:51:04', 'Dépôt initial', NULL),
(47, 31, 'depot', 500.00, '2025-12-16 11:51:05', 'Dépôt test', NULL),
(48, 31, 'virement', 300.00, '2025-12-16 11:51:05', 'Virement test', 32),
(49, 31, 'interets', 36.00, '2025-12-16 11:51:06', 'Intérêts annuels', NULL),
(50, 33, 'depot', 1000.00, '2025-12-16 11:54:01', 'Dépôt initial', NULL),
(51, 33, 'depot', 500.00, '2025-12-16 11:54:01', 'Dépôt test', NULL),
(52, 33, 'virement', 300.00, '2025-12-16 11:54:01', 'Virement test', 34),
(53, 33, 'interets', 36.00, '2025-12-16 11:54:01', 'Intérêts annuels', NULL);

-- --------------------------------------------------------

--
-- Structure de la table `users`
--

CREATE TABLE `users` (
  `id_user` int(11) NOT NULL,
  `nom` varchar(100) NOT NULL,
  `prenom` varchar(100) NOT NULL,
  `mail` varchar(150) NOT NULL,
  `username` varchar(100) NOT NULL,
  `password` varchar(255) NOT NULL,
  `role` enum('client','admin') NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Déchargement des données de la table `users`
--

INSERT INTO `users` (`id_user`, `nom`, `prenom`, `mail`, `username`, `password`, `role`) VALUES
(1, 'Dupont', 'Jean', 'jean.dupont@example.com', 'testuser', 'password123', 'client'),
(3, 'Admin', 'Super', 'admin@bank.com', 'admin', 'admin123', 'admin'),
(4, 'wail', 'dj', 'wail.dj@gmail.com', 'wail', 'wail123', 'client');

--
-- Index pour les tables déchargées
--

--
-- Index pour la table `comptes`
--
ALTER TABLE `comptes`
  ADD PRIMARY KEY (`id_compte`),
  ADD KEY `fk_compte_user` (`id_client`);

--
-- Index pour la table `transaction`
--
ALTER TABLE `transaction`
  ADD PRIMARY KEY (`id_transaction`),
  ADD KEY `fk_trans_source` (`id_compte_source`),
  ADD KEY `fk_trans_dest` (`id_compte_destination`);

--
-- Index pour la table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id_user`),
  ADD UNIQUE KEY `mail` (`mail`),
  ADD UNIQUE KEY `username` (`username`);

--
-- AUTO_INCREMENT pour les tables déchargées
--

--
-- AUTO_INCREMENT pour la table `comptes`
--
ALTER TABLE `comptes`
  MODIFY `id_compte` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=35;

--
-- AUTO_INCREMENT pour la table `transaction`
--
ALTER TABLE `transaction`
  MODIFY `id_transaction` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=54;

--
-- AUTO_INCREMENT pour la table `users`
--
ALTER TABLE `users`
  MODIFY `id_user` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- Contraintes pour les tables déchargées
--

--
-- Contraintes pour la table `comptes`
--
ALTER TABLE `comptes`
  ADD CONSTRAINT `fk_compte_user` FOREIGN KEY (`id_client`) REFERENCES `users` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Contraintes pour la table `transaction`
--
ALTER TABLE `transaction`
  ADD CONSTRAINT `fk_trans_dest` FOREIGN KEY (`id_compte_destination`) REFERENCES `comptes` (`id_compte`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_trans_source` FOREIGN KEY (`id_compte_source`) REFERENCES `comptes` (`id_compte`) ON DELETE CASCADE ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
