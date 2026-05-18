-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- 主機： 127.0.0.1
-- 產生時間： 2026-05-16 14:03:28
-- 伺服器版本： 10.4.32-MariaDB
-- PHP 版本： 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- 資料庫： `premium_living_db`
--
CREATE DATABASE IF NOT EXISTS `premium_living_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `premium_living_db`;

-- --------------------------------------------------------

--
-- 資料表結構 `complaint`
--

DROP TABLE IF EXISTS `complaint`;
CREATE TABLE `complaint` (
  `ComplaintID` varchar(20) NOT NULL,
  `CustomerID` varchar(15) DEFAULT NULL,
  `OrderID` varchar(20) DEFAULT NULL,
  `ComplaintDate` datetime DEFAULT NULL,
  `ResolutionStatus` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `customer`
--

DROP TABLE IF EXISTS `customer`;
CREATE TABLE `customer` (
  `CustomerID` varchar(15) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Address` text DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `deliverynote`
--

DROP TABLE IF EXISTS `deliverynote`;
CREATE TABLE `deliverynote` (
  `DeliveryNoteID` varchar(20) NOT NULL,
  `OrderID` varchar(20) DEFAULT NULL,
  `StaffID` varchar(15) DEFAULT NULL,
  `DeliveryDate` datetime DEFAULT NULL,
  `DeliveryAddress` varchar(255) DEFAULT NULL,
  `DeliveryStatus` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `goodsreceivednote_grn`
--

DROP TABLE IF EXISTS `goodsreceivednote_grn`;
CREATE TABLE `goodsreceivednote_grn` (
  `GRN_ID` varchar(20) NOT NULL,
  `PO_ID` varchar(20) DEFAULT NULL,
  `StaffID` varchar(15) DEFAULT NULL,
  `ReceivedDate` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `order_lineitem`
--

DROP TABLE IF EXISTS `order_lineitem`;
CREATE TABLE `order_lineitem` (
  `OrderID` varchar(20) NOT NULL,
  `PartID` varchar(15) NOT NULL,
  `Quantity` int(11) NOT NULL,
  `Subtotal` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `po_lineitem`
--

DROP TABLE IF EXISTS `po_lineitem`;
CREATE TABLE `po_lineitem` (
  `PO_ID` varchar(20) NOT NULL,
  `PartID` varchar(15) NOT NULL,
  `Quantity` int(11) DEFAULT NULL,
  `UnitPrice` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `product_part`
--

DROP TABLE IF EXISTS `product_part`;
CREATE TABLE `product_part` (
  `PartID` varchar(15) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Description` text DEFAULT NULL,
  `StockLevel` int(11) DEFAULT 0,
  `DefaultPrice` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `purchaseorder`
--

DROP TABLE IF EXISTS `purchaseorder`;
CREATE TABLE `purchaseorder` (
  `PO_ID` varchar(20) NOT NULL,
  `SupplierID` varchar(15) DEFAULT NULL,
  `StaffID` varchar(15) DEFAULT NULL,
  `ReOrderCardID` varchar(20) DEFAULT NULL,
  `PODate` datetime DEFAULT NULL,
  `Status` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `reordercard`
--

DROP TABLE IF EXISTS `reordercard`;
CREATE TABLE `reordercard` (
  `ReOrderCardID` varchar(20) NOT NULL,
  `PartID` varchar(15) DEFAULT NULL,
  `TriggerDate` datetime DEFAULT NULL,
  `RequestedQty` int(11) DEFAULT NULL,
  `Status` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `salesorder`
--

DROP TABLE IF EXISTS `salesorder`;
CREATE TABLE `salesorder` (
  `OrderID` varchar(20) NOT NULL,
  `CustomerID` varchar(15) DEFAULT NULL,
  `StaffID` varchar(15) DEFAULT NULL,
  `OrderDate` datetime DEFAULT current_timestamp(),
  `Status` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `staff`
--

DROP TABLE IF EXISTS `staff`;
CREATE TABLE `staff` (
  `StaffID` varchar(15) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Role` varchar(50) DEFAULT NULL,
  `Password` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `staff`
--

INSERT INTO `staff` (`StaffID`, `Name`, `Role`, `Password`) VALUES
('S001', 'PO', 'Manager', '1234'),
('S002', 'ada', 'Staff', 'password');

-- --------------------------------------------------------

--
-- 資料表結構 `supplier`
--

DROP TABLE IF EXISTS `supplier`;
CREATE TABLE `supplier` (
  `SupplierID` varchar(15) NOT NULL,
  `SupplierName` varchar(100) NOT NULL,
  `ContactInfo` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 已傾印資料表的索引
--

--
-- 資料表索引 `complaint`
--
ALTER TABLE `complaint`
  ADD PRIMARY KEY (`ComplaintID`),
  ADD KEY `CustomerID` (`CustomerID`),
  ADD KEY `OrderID` (`OrderID`);

--
-- 資料表索引 `customer`
--
ALTER TABLE `customer`
  ADD PRIMARY KEY (`CustomerID`);

--
-- 資料表索引 `deliverynote`
--
ALTER TABLE `deliverynote`
  ADD PRIMARY KEY (`DeliveryNoteID`),
  ADD KEY `OrderID` (`OrderID`),
  ADD KEY `StaffID` (`StaffID`);

--
-- 資料表索引 `goodsreceivednote_grn`
--
ALTER TABLE `goodsreceivednote_grn`
  ADD PRIMARY KEY (`GRN_ID`),
  ADD KEY `PO_ID` (`PO_ID`),
  ADD KEY `StaffID` (`StaffID`);

--
-- 資料表索引 `order_lineitem`
--
ALTER TABLE `order_lineitem`
  ADD PRIMARY KEY (`OrderID`,`PartID`),
  ADD KEY `PartID` (`PartID`);

--
-- 資料表索引 `po_lineitem`
--
ALTER TABLE `po_lineitem`
  ADD PRIMARY KEY (`PO_ID`,`PartID`),
  ADD KEY `PartID` (`PartID`);

--
-- 資料表索引 `product_part`
--
ALTER TABLE `product_part`
  ADD PRIMARY KEY (`PartID`);

--
-- 資料表索引 `purchaseorder`
--
ALTER TABLE `purchaseorder`
  ADD PRIMARY KEY (`PO_ID`),
  ADD KEY `SupplierID` (`SupplierID`),
  ADD KEY `StaffID` (`StaffID`),
  ADD KEY `ReOrderCardID` (`ReOrderCardID`);

--
-- 資料表索引 `reordercard`
--
ALTER TABLE `reordercard`
  ADD PRIMARY KEY (`ReOrderCardID`),
  ADD KEY `PartID` (`PartID`);

--
-- 資料表索引 `salesorder`
--
ALTER TABLE `salesorder`
  ADD PRIMARY KEY (`OrderID`),
  ADD KEY `CustomerID` (`CustomerID`),
  ADD KEY `StaffID` (`StaffID`);

--
-- 資料表索引 `staff`
--
ALTER TABLE `staff`
  ADD PRIMARY KEY (`StaffID`);

--
-- 資料表索引 `supplier`
--
ALTER TABLE `supplier`
  ADD PRIMARY KEY (`SupplierID`);

--
-- 已傾印資料表的限制式
--

--
-- 資料表的限制式 `complaint`
--
ALTER TABLE `complaint`
  ADD CONSTRAINT `complaint_ibfk_1` FOREIGN KEY (`CustomerID`) REFERENCES `customer` (`CustomerID`),
  ADD CONSTRAINT `complaint_ibfk_2` FOREIGN KEY (`OrderID`) REFERENCES `salesorder` (`OrderID`);

--
-- 資料表的限制式 `deliverynote`
--
ALTER TABLE `deliverynote`
  ADD CONSTRAINT `deliverynote_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `salesorder` (`OrderID`),
  ADD CONSTRAINT `deliverynote_ibfk_2` FOREIGN KEY (`StaffID`) REFERENCES `staff` (`StaffID`);

--
-- 資料表的限制式 `goodsreceivednote_grn`
--
ALTER TABLE `goodsreceivednote_grn`
  ADD CONSTRAINT `goodsreceivednote_grn_ibfk_1` FOREIGN KEY (`PO_ID`) REFERENCES `purchaseorder` (`PO_ID`),
  ADD CONSTRAINT `goodsreceivednote_grn_ibfk_2` FOREIGN KEY (`StaffID`) REFERENCES `staff` (`StaffID`);

--
-- 資料表的限制式 `order_lineitem`
--
ALTER TABLE `order_lineitem`
  ADD CONSTRAINT `order_lineitem_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `salesorder` (`OrderID`),
  ADD CONSTRAINT `order_lineitem_ibfk_2` FOREIGN KEY (`PartID`) REFERENCES `product_part` (`PartID`);

--
-- 資料表的限制式 `po_lineitem`
--
ALTER TABLE `po_lineitem`
  ADD CONSTRAINT `po_lineitem_ibfk_1` FOREIGN KEY (`PO_ID`) REFERENCES `purchaseorder` (`PO_ID`),
  ADD CONSTRAINT `po_lineitem_ibfk_2` FOREIGN KEY (`PartID`) REFERENCES `product_part` (`PartID`);

--
-- 資料表的限制式 `purchaseorder`
--
ALTER TABLE `purchaseorder`
  ADD CONSTRAINT `purchaseorder_ibfk_1` FOREIGN KEY (`SupplierID`) REFERENCES `supplier` (`SupplierID`),
  ADD CONSTRAINT `purchaseorder_ibfk_2` FOREIGN KEY (`StaffID`) REFERENCES `staff` (`StaffID`),
  ADD CONSTRAINT `purchaseorder_ibfk_3` FOREIGN KEY (`ReOrderCardID`) REFERENCES `reordercard` (`ReOrderCardID`);

--
-- 資料表的限制式 `reordercard`
--
ALTER TABLE `reordercard`
  ADD CONSTRAINT `reordercard_ibfk_1` FOREIGN KEY (`PartID`) REFERENCES `product_part` (`PartID`);

--
-- 資料表的限制式 `salesorder`
--
ALTER TABLE `salesorder`
  ADD CONSTRAINT `salesorder_ibfk_1` FOREIGN KEY (`CustomerID`) REFERENCES `customer` (`CustomerID`),
  ADD CONSTRAINT `salesorder_ibfk_2` FOREIGN KEY (`StaffID`) REFERENCES `staff` (`StaffID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
