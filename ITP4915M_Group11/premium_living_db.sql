-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- 主機： 127.0.0.1
-- 產生時間： 2026-06-10 08:12:41
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

-- --------------------------------------------------------

--
-- 資料表結構 `complaint`
--

CREATE TABLE `complaint` (
  `ComplaintID` varchar(30) NOT NULL,
  `CustomerID` varchar(15) NOT NULL,
  `OrderID` varchar(20) DEFAULT NULL,
  `Date` datetime NOT NULL,
  `Status` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `complaint`
--

INSERT INTO `complaint` (`ComplaintID`, `CustomerID`, `OrderID`, `Date`, `Status`) VALUES
('COMP-20260601-101530', 'C001', 'SO2026-0001', '2026-06-01 10:15:30', 'Pending'),
('COMP-20260602-142211', 'C002', 'SO2026-0002', '2026-06-02 14:22:11', 'In Progress'),
('COMP-20260603-090544', 'C003', 'SO2026-0003', '2026-06-03 09:05:44', 'Resolved'),
('COMP-20260604-113000', 'C004', NULL, '2026-06-04 11:30:00', 'Closed'),
('COMP-20260604-124515', 'C005', 'SO2026-0006', '2026-06-04 12:45:15', 'In Progress');

-- --------------------------------------------------------

--
-- 資料表結構 `customer`
--

CREATE TABLE `customer` (
  `CustomerID` varchar(15) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Type` varchar(10) DEFAULT 'B2B',
  `Phone` varchar(20) DEFAULT NULL,
  `Address` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `customer`
--

INSERT INTO `customer` (`CustomerID`, `Name`, `Type`, `Phone`, `Address`) VALUES
('C001', 'Hong Kong Catering Group', 'B2B', '23456789', 'Flat B, 12/F, King Palace Plaza, Kwun Tong'),
('C002', 'Shatin Lifestyle Outlet', 'B2B', '26987412', 'Shop 302, Level 3, New Town Plaza, Shatin'),
('C003', 'Wanchai Modern Furniture Co.', 'B2B', '28341122', 'G/F, 88 Johnston Road, Wan Chai'),
('C004', 'Kowloon Hotel Group', 'B2B', '27348899', '18 Nathan Road, Tsim Sha Tsui'),
('C005', 'Central Design Studio', 'B2B', '25223344', 'Room 1004, 10/F, Duke Wellington House, Central'),
('C006', 'Mongkok Co-working Space', 'B2B', '31056677', 'Floor 21, Hollywood Plaza, Mong Kok'),
('C007', 'Tsuen Wan Property Management', 'B2B', '24189900', 'Block 3, Nina Tower, Tsuen Wan'),
('C008', 'Island East Cafe Chain', 'B2B', '25671122', 'Shop G12, Taikoo Shing, Quarry Bay'),
('C009', 'Apex Logistics Headquarters', 'B2B', '27405566', 'Cargo Terminal 4, Kwai Chung'),
('C010', 'Apex Education Centre', 'B2C', '26904455', '6/F, Foo Tan Industrial Building, Fo Tan');

-- --------------------------------------------------------

--
-- 資料表結構 `delivery_note`
--

CREATE TABLE `delivery_note` (
  `DeliveryNoteID` varchar(20) NOT NULL,
  `OrderID` varchar(20) DEFAULT NULL,
  `DeliveryDate` datetime DEFAULT NULL,
  `DeliveryAddress` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `delivery_note`
--

INSERT INTO `delivery_note` (`DeliveryNoteID`, `OrderID`, `DeliveryDate`, `DeliveryAddress`) VALUES
('DN-2026-001', 'SO2026-0001', '2026-05-29 11:00:00', 'Flat B, 12/F, King Palace Plaza, Kwun Tong'),
('DN-2026-002', 'SO2026-0002', '2026-06-01 14:00:00', 'Shop 302, Level 3, New Town Plaza, Shatin'),
('DN-2026-003', 'SO2026-0006', '2026-05-31 16:00:00', 'Room 1004, 10/F, Duke Wellington House, Central'),
('DN-2026-004', 'SO2026-0003', '2026-06-02 10:00:00', 'G/F, 88 Johnston Road, Wan Chai'),
('DN-2026-005', 'SO2026-0005', '2026-06-03 13:30:00', '18 Nathan Road, Tsim Sha Tsui'),
('DN-2026-006', 'SO2026-0004', '2026-06-01 10:00:00', 'Flat B, 12/F, King Palace Plaza, Kwun Tong'),
('DN-2026-007', 'SO2026-0007', '2026-06-02 14:00:00', 'Floor 21, Hollywood Plaza, Mong Kok'),
('DN-2026-008', 'SO2026-0008', '2026-06-01 15:30:00', 'Block 3, Nina Tower, Tsuen Wan'),
('DN-2026-009', 'SO2026-0009', '2026-06-01 16:20:00', 'Shop G12, Taikoo Shing, Quarry Bay'),
('DN-2026-010', 'SO2026-0010', '2026-06-02 11:15:00', 'Cargo Terminal 4, Kwai Chung'),
('DN-2026-011', 'SO2026-0014', '2026-06-10 13:51:17', 'Shop 302, Level 3, New Town Plaza, Shatin'),
('DN-2026-012', 'SO2026-0015', '2026-06-10 13:53:14', 'Flat B, 12/F, King Palace Plaza, Kwun Tong');

-- --------------------------------------------------------

--
-- 資料表結構 `goods_received_note`
--

CREATE TABLE `goods_received_note` (
  `GRN_ID` varchar(20) NOT NULL,
  `PO_ID` varchar(20) DEFAULT NULL,
  `StaffID` varchar(15) DEFAULT NULL,
  `ReceivedDate` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `goods_received_note`
--

INSERT INTO `goods_received_note` (`GRN_ID`, `PO_ID`, `StaffID`, `ReceivedDate`) VALUES
('GRN041130121100', 'PO202605001', 'S001', '2026-06-04 11:30:12'),
('GRN041135451101', 'PO202605004', 'STF0012', '2026-06-04 11:35:45'),
('GRN041142101102', 'PO202605009', 'STF0045', '2026-06-04 11:42:10'),
('GRN041148221103', 'PO202605012', 'STF0012', '2026-06-04 11:48:22'),
('GRN041151051104', 'PO202605015', 'STF0088', '2026-06-04 11:51:05'),
('GRN04115503', 'PO2026-0002', 'S001', '2026-06-04 11:55:34'),
('GRN04115537', 'PO2026-0003', 'S001', '2026-06-04 11:55:39'),
('GRN10122524', 'PO2026-0007', 'S003', '2026-06-10 12:25:59'),
('GRN10122600', 'PO2026-0008', 'S003', '2026-06-10 12:26:37'),
('GRN10122638', 'PO2026-0010', 'S003', '2026-06-10 12:26:41'),
('GRN10135139', 'PO2026-0006', 'S001', '2026-06-10 13:51:53');

-- --------------------------------------------------------

--
-- 資料表結構 `orders`
--

CREATE TABLE `orders` (
  `OrderID` varchar(20) NOT NULL,
  `CustomerID` varchar(15) DEFAULT NULL,
  `StaffID` varchar(15) DEFAULT NULL,
  `OrderDate` datetime DEFAULT current_timestamp(),
  `TotalAmount` decimal(10,2) DEFAULT 0.00,
  `Status` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `orders`
--

INSERT INTO `orders` (`OrderID`, `CustomerID`, `StaffID`, `OrderDate`, `TotalAmount`, `Status`) VALUES
('SO2026-0001', 'C001', 'S002', '2026-05-28 10:15:00', 1800.00, 'Delivered'),
('SO2026-0002', 'C002', 'S002', '2026-05-29 14:30:00', 2400.00, 'Dispatched'),
('SO2026-0003', 'C003', 'S001', '2026-05-30 09:00:00', 4800.00, 'Packed'),
('SO2026-0004', 'C001', 'S002', '2026-05-31 11:00:00', 900.00, 'Packed'),
('SO2026-0005', 'C004', 'S002', '2026-05-31 13:00:00', 8500.00, 'Ordered'),
('SO2026-0006', 'C005', 'S001', '2026-05-31 14:15:00', 2700.00, 'Delivered'),
('SO2026-0007', 'C006', 'S002', '2026-06-01 09:30:00', 5600.00, 'Ordered'),
('SO2026-0008', 'C007', 'S001', '2026-06-01 11:00:00', 4400.00, 'Dispatched'),
('SO2026-0009', 'C008', 'S002', '2026-06-01 13:45:00', 3800.00, 'Pending Delivery'),
('SO2026-0010', 'C009', 'S001', '2026-06-01 16:00:00', 7500.00, 'Packed'),
('SO2026-0011', 'C010', 'S001', '2026-06-10 11:37:32', 12000.00, 'Pending Delivery'),
('SO2026-0012', 'C002', 'S001', '2026-06-10 11:46:42', 1350.00, 'Pending Delivery'),
('SO2026-0013', 'C002', 'S001', '2026-06-10 12:10:56', 1350.00, 'Self-Pickup'),
('SO2026-0014', 'C002', 'S001', '2026-06-10 13:34:15', 2250.00, 'Dispatched'),
('SO2026-0015', 'C001', 'S001', '2026-06-10 13:53:04', 28000.00, 'Dispatched');

-- --------------------------------------------------------

--
-- 資料表結構 `order_lineitem`
--

CREATE TABLE `order_lineitem` (
  `OrderID` varchar(20) NOT NULL,
  `PartID` varchar(15) NOT NULL,
  `Quantity` int(11) NOT NULL,
  `UnitPrice` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `order_lineitem`
--

INSERT INTO `order_lineitem` (`OrderID`, `PartID`, `Quantity`, `UnitPrice`) VALUES
('SO2026-0001', 'P001', 4, 450.00),
('SO2026-0002', 'P002', 1, 2400.00),
('SO2026-0003', 'P003', 1, 4800.00),
('SO2026-0004', 'P001', 2, 450.00),
('SO2026-0005', 'P004', 10, 850.00),
('SO2026-0006', 'P005', 2, 1350.00),
('SO2026-0007', 'P006', 20, 280.00),
('SO2026-0008', 'P007', 4, 1100.00),
('SO2026-0009', 'P008', 4, 950.00),
('SO2026-0010', 'P010', 1, 7500.00),
('SO2026-0011', 'P002', 5, 2400.00),
('SO2026-0012', 'P001', 3, 450.00),
('SO2026-0013', 'P001', 3, 450.00),
('SO2026-0014', 'P001', 5, 450.00),
('SO2026-0015', 'P006', 100, 280.00);

-- --------------------------------------------------------

--
-- 資料表結構 `po_lineitem`
--

CREATE TABLE `po_lineitem` (
  `PO_ID` varchar(20) NOT NULL,
  `PartID` varchar(15) NOT NULL,
  `Quantity` int(11) DEFAULT NULL,
  `UnitPrice` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `po_lineitem`
--

INSERT INTO `po_lineitem` (`PO_ID`, `PartID`, `Quantity`, `UnitPrice`) VALUES
('PO-20260610-133231', 'P004', 100, 1000.00),
('PO-20260610-133252', 'P008', 40, 1000.00),
('PO-20260610-140732', 'P005', 20, 20.00),
('PO-20260610-140830', 'P006', 100, 50000000.00),
('PO2026-0001', 'P003', 10, 3500.00),
('PO2026-0002', 'P002', 15, 1800.00),
('PO2026-0003', 'P001', 100, 300.00),
('PO2026-0004', 'P004', 40, 600.00),
('PO2026-0005', 'P005', 25, 1000.00),
('PO2026-0006', 'P006', 100, 200.00),
('PO2026-0007', 'P008', 30, 700.00),
('PO2026-0008', 'P001', 50, 320.00),
('PO2026-0009', 'P007', 25, 800.00),
('PO2026-0010', 'P010', 5, 5500.00);

-- --------------------------------------------------------

--
-- 資料表結構 `product_part`
--

CREATE TABLE `product_part` (
  `PartID` varchar(15) NOT NULL,
  `PartName` varchar(100) NOT NULL,
  `StockLevel` int(11) DEFAULT 0,
  `ReorderLevel` int(11) DEFAULT 10,
  `DefaultPrice` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `product_part`
--

INSERT INTO `product_part` (`PartID`, `PartName`, `StockLevel`, `ReorderLevel`, `DefaultPrice`) VALUES
('P001', 'Premium Dining Chair', 259, 20, 450.00),
('P002', 'Extendable Dining Table', 45, 10, 2400.00),
('P003', 'Minimalist Lounge Sofa', 15, 5, 4800.00),
('P004', 'Office Ergonomic Chair', 85, 15, 850.00),
('P005', 'Solid Wood Bookshelf', 22, 8, 1350.00),
('P006', 'LED Desk Lamp', 150, 30, 280.00),
('P007', 'Glass Coffee Table', 40, 10, 1100.00),
('P008', 'Steel Filing Cabinet', 116, 12, 950.00),
('P009', 'Wooden Wardrobe', 18, 5, 3200.00),
('P010', 'Conference Room Table', 13, 2, 7500.00);

-- --------------------------------------------------------

--
-- 資料表結構 `purchase_order`
--

CREATE TABLE `purchase_order` (
  `PO_ID` varchar(20) NOT NULL,
  `SupplierID` varchar(15) DEFAULT NULL,
  `ReOrderCardID` varchar(20) DEFAULT NULL,
  `PODate` datetime DEFAULT NULL,
  `Status` varchar(20) DEFAULT NULL,
  `StaffID` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `purchase_order`
--

INSERT INTO `purchase_order` (`PO_ID`, `SupplierID`, `ReOrderCardID`, `PODate`, `Status`, `StaffID`) VALUES
('PO-20260610-133231', 'V001', 'RC011', '2026-06-10 13:32:51', 'Ordered', 'S001'),
('PO-20260610-133252', 'V002', 'RC008', '2026-06-10 13:33:08', 'Ordered', 'S001'),
('PO-20260610-140732', 'V001', 'RC002', '2026-06-10 14:08:06', 'Ordered', 'S001'),
('PO-20260610-140830', 'V002', 'RC006', '2026-06-10 14:08:48', 'Ordered', 'S001'),
('PO2026-0001', 'V001', 'RC001', '2026-05-11 11:00:00', 'Received', NULL),
('PO2026-0002', 'V003', NULL, '2026-05-15 15:00:00', 'Received', NULL),
('PO2026-0003', 'V002', NULL, '2026-05-18 09:30:00', 'Received', NULL),
('PO2026-0004', 'V005', NULL, '2026-05-22 13:45:00', 'Pending', NULL),
('PO2026-0005', 'V004', NULL, '2026-05-26 10:20:00', 'Received', NULL),
('PO2026-0006', 'V006', 'RC003', '2026-05-29 16:00:00', 'Received', NULL),
('PO2026-0007', 'V008', NULL, '2026-06-01 09:00:00', 'Received', NULL),
('PO2026-0008', 'V007', NULL, '2026-06-01 11:15:00', 'Received', NULL),
('PO2026-0009', 'V010', 'RC007', '2026-06-01 14:00:00', 'Received', NULL),
('PO2026-0010', 'V009', 'RC010', '2026-06-01 15:45:00', 'Received', NULL);

-- --------------------------------------------------------

--
-- 資料表結構 `reorder_card`
--

CREATE TABLE `reorder_card` (
  `ReOrderCardID` varchar(20) NOT NULL,
  `PartID` varchar(15) DEFAULT NULL,
  `TriggerDate` datetime DEFAULT NULL,
  `RequestedQty` int(11) DEFAULT NULL,
  `Status` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `reorder_card`
--

INSERT INTO `reorder_card` (`ReOrderCardID`, `PartID`, `TriggerDate`, `RequestedQty`, `Status`) VALUES
('RC001', 'P003', '2026-05-10 10:00:00', 10, 'Completed'),
('RC002', 'P005', '2026-05-25 16:30:00', 20, 'Approved'),
('RC003', 'P001', '2026-05-28 09:15:00', 50, 'Ordered'),
('RC004', 'P002', '2026-05-29 11:00:00', 15, 'Pending'),
('RC005', 'P004', '2026-05-30 14:00:00', 30, 'Cancelled'),
('RC006', 'P006', '2026-05-31 08:45:00', 100, 'Approved'),
('RC007', 'P007', '2026-06-01 10:20:00', 25, 'Ordered'),
('RC008', 'P008', '2026-06-01 11:30:00', 40, 'Approved'),
('RC009', 'P009', '2026-06-01 13:00:00', 12, 'Pending'),
('RC010', 'P010', '2026-06-01 15:15:00', 5, 'Ordered'),
('RC011', 'P004', '2026-06-10 13:11:30', 100, 'Approved');

-- --------------------------------------------------------

--
-- 資料表結構 `staff`
--

CREATE TABLE `staff` (
  `StaffID` varchar(15) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Password` varchar(255) NOT NULL DEFAULT 'password123',
  `Role` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `staff`
--

INSERT INTO `staff` (`StaffID`, `Name`, `Password`, `Role`) VALUES
('S001', 'PO', SHA2('123456',256), 'Manager'),
('S002', 'ada', SHA2('abcdef',256), 'Staff'),
('S003', 'PO PO', SHA2('123',256), 'Administrator');

-- --------------------------------------------------------

--
-- 資料表結構 `supplier`
--

CREATE TABLE `supplier` (
  `SupplierID` varchar(15) NOT NULL,
  `SupplierName` varchar(100) NOT NULL,
  `ContactInfo` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- 傾印資料表的資料 `supplier`
--

INSERT INTO `supplier` (`SupplierID`, `SupplierName`, `ContactInfo`) VALUES
('V001', 'Global Timber Supplies Ltd', 'info@globaltimber.com, Tel: 21113333'),
('V002', 'Comfort Foam & Fabric Factory', 'sales@comfortfoam.cn, Tel: 755-83334444'),
('V003', 'Steel & Glass Components Co.', 'order@steelglass.hk, Tel: 29998888'),
('V004', 'Eco-Friendly Coatings Ltd', 'contact@ecocoatings.com, Tel: 24445555'),
('V005', 'Modern Hardware Fittings', 'sales@modernhardware.hk, Tel: 26667777'),
('V006', 'Smart Lighting Industries', 'support@smartlighting.tw, Tel: 886-2-2555'),
('V007', 'Premium Leather Goods Corp', 'import@premiumleather.com, Tel: 23330099'),
('V008', 'Elite Office Systems Suppliers', 'b2b@eliteoffice.hk, Tel: 28881122'),
('V009', 'Pacific Logistics Imports', 'freight@pacificlog.com, Tel: 27553344'),
('V010', 'Oriental Veneer Milling Co.', 'milling@orientalveneer.cn, Tel: 757-2233');

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
-- 資料表索引 `delivery_note`
--
ALTER TABLE `delivery_note`
  ADD PRIMARY KEY (`DeliveryNoteID`),
  ADD UNIQUE KEY `unique_dn_order` (`OrderID`);

--
-- 資料表索引 `goods_received_note`
--
ALTER TABLE `goods_received_note`
  ADD PRIMARY KEY (`GRN_ID`),
  ADD KEY `PO_ID` (`PO_ID`),
  ADD KEY `StaffID` (`StaffID`);

--
-- 資料表索引 `orders`
--
ALTER TABLE `orders`
  ADD PRIMARY KEY (`OrderID`),
  ADD KEY `idx_customer` (`CustomerID`),
  ADD KEY `idx_staff` (`StaffID`);

--
-- 資料表索引 `order_lineitem`
--
ALTER TABLE `order_lineitem`
  ADD PRIMARY KEY (`OrderID`,`PartID`),
  ADD KEY `idx_ol_part` (`PartID`);

--
-- 資料表索引 `po_lineitem`
--
ALTER TABLE `po_lineitem`
  ADD PRIMARY KEY (`PO_ID`,`PartID`),
  ADD KEY `idx_pol_part` (`PartID`);

--
-- 資料表索引 `product_part`
--
ALTER TABLE `product_part`
  ADD PRIMARY KEY (`PartID`);

--
-- 資料表索引 `purchase_order`
--
ALTER TABLE `purchase_order`
  ADD PRIMARY KEY (`PO_ID`),
  ADD KEY `idx_po_supplier` (`SupplierID`),
  ADD KEY `idx_po_reorder` (`ReOrderCardID`);

--
-- 資料表索引 `reorder_card`
--
ALTER TABLE `reorder_card`
  ADD PRIMARY KEY (`ReOrderCardID`),
  ADD KEY `idx_rc_part` (`PartID`);

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
  ADD CONSTRAINT `complaint_ibfk_2` FOREIGN KEY (`OrderID`) REFERENCES `orders` (`OrderID`);

--
-- 資料表的限制式 `delivery_note`
--
ALTER TABLE `delivery_note`
  ADD CONSTRAINT `delivery_note_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `orders` (`OrderID`);

--
-- 資料表的限制式 `orders`
--
ALTER TABLE `orders`
  ADD CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`CustomerID`) REFERENCES `customer` (`CustomerID`),
  ADD CONSTRAINT `orders_ibfk_2` FOREIGN KEY (`StaffID`) REFERENCES `staff` (`StaffID`);

--
-- 資料表的限制式 `order_lineitem`
--
ALTER TABLE `order_lineitem`
  ADD CONSTRAINT `order_lineitem_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `orders` (`OrderID`),
  ADD CONSTRAINT `order_lineitem_ibfk_2` FOREIGN KEY (`PartID`) REFERENCES `product_part` (`PartID`);

--
-- 資料表的限制式 `po_lineitem`
--
ALTER TABLE `po_lineitem`
  ADD CONSTRAINT `po_lineitem_ibfk_1` FOREIGN KEY (`PO_ID`) REFERENCES `purchase_order` (`PO_ID`),
  ADD CONSTRAINT `po_lineitem_ibfk_2` FOREIGN KEY (`PartID`) REFERENCES `product_part` (`PartID`);

--
-- 資料表的限制式 `purchase_order`
--
ALTER TABLE `purchase_order`
  ADD CONSTRAINT `purchase_order_ibfk_1` FOREIGN KEY (`SupplierID`) REFERENCES `supplier` (`SupplierID`),
  ADD CONSTRAINT `purchase_order_ibfk_3` FOREIGN KEY (`ReOrderCardID`) REFERENCES `reorder_card` (`ReOrderCardID`);

--
-- 資料表的限制式 `reorder_card`
--
ALTER TABLE `reorder_card`
  ADD CONSTRAINT `reorder_card_ibfk_1` FOREIGN KEY (`PartID`) REFERENCES `product_part` (`PartID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
