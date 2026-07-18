-- =====================================================================
-- 1.9.0 - kompletni naplneni prazdne DB
-- MATERIAL beze zmeny + nova PRACE
-- =====================================================================

PRAGMA foreign_keys = ON;

-- =====================================================================
-- 1) TABULKY - MATERIAL
-- =====================================================================

CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Suppliers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Materials (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Unit TEXT NOT NULL,
    Price REAL NOT NULL DEFAULT 0,
    CategoryId INTEGER NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories (Id)
);

CREATE TABLE IF NOT EXISTS MaterialPrices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MaterialId INTEGER NOT NULL,
    SupplierId INTEGER NOT NULL,
    SupplierCode TEXT NOT NULL,
    SupplierName TEXT NOT NULL,
    Unit TEXT NOT NULL,
    Price REAL NOT NULL,
    Currency TEXT NOT NULL DEFAULT 'Kč',
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (MaterialId) REFERENCES Materials (Id),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers (Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MaterialPrices_SupplierId_SupplierCode
    ON MaterialPrices (SupplierId, SupplierCode);

CREATE INDEX IF NOT EXISTS IX_Materials_CategoryId
    ON Materials (CategoryId);

-- =====================================================================
-- 2) TABULKY - PRACE
-- =====================================================================

CREATE TABLE IF NOT EXISTS BaseMaterials (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(100) NOT NULL,
    BaseMaterialCoef REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS Positions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(100) NOT NULL,
    PositionCoef REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(100) NOT NULL,
    BasePrice REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS Specifications (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(10)
);

CREATE TABLE IF NOT EXISTS TaskSpecifications (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId INTEGER NOT NULL,
    SpecificationId INTEGER NOT NULL,
    FOREIGN KEY (TaskId) REFERENCES Tasks (Id),
    FOREIGN KEY (SpecificationId) REFERENCES Specifications (Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_TaskSpecifications_TaskId_SpecificationId
    ON TaskSpecifications (TaskId, SpecificationId);

-- =====================================================================
-- 3) DATA - MATERIAL
-- =====================================================================

INSERT INTO Categories (Name) VALUES
('Kabel'),
('Elektroinstalační materiál'),
('Spínače a zásuvky'),
('Skříně a rozvaděče'),
('Jističe'),
('Chrániče');

INSERT INTO Suppliers (Name) VALUES
('ELKOV'),
('EMAS');

INSERT INTO Materials (Name, Unit, Price, CategoryId) VALUES
('CYKY-J 3x1,5', 'm', 0, (SELECT Id FROM Categories WHERE Name='Kabel')),
('CYKY-J 3x2,5', 'm', 0, (SELECT Id FROM Categories WHERE Name='Kabel')),
('CYKY-J 5x6', 'm', 0, (SELECT Id FROM Categories WHERE Name='Kabel')),
('Chránička optického kabelu', 'm', 0, (SELECT Id FROM Categories WHERE Name='Kabel')),
('Krabice pod omítku', 'ks', 0, (SELECT Id FROM Categories WHERE Name='Elektroinstalační materiál')),
('Spínač/Přepínač', 'ks', 0, (SELECT Id FROM Categories WHERE Name='Spínače a zásuvky')),
('Zásuvka', 'ks', 0, (SELECT Id FROM Categories WHERE Name='Spínače a zásuvky')),
('Rozvaděč 4-modulový', 'ks', 0, (SELECT Id FROM Categories WHERE Name='Skříně a rozvaděče')),
('Jistič 1-fázový', 'ks', 0, (SELECT Id FROM Categories WHERE Name='Jističe')),
('Chránič proudový 2-pólový', 'ks', 0, (SELECT Id FROM Categories WHERE Name='Chrániče'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='CYKY-J 3x1,5'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '17018', 'Kabel CYKY-J 3x1,5 (C)', 'm', 23.22, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='CYKY-J 3x1,5'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELKAOS0802380', 'CYKY-J DCA 3 X 1,5', 'm', 20.45, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='CYKY-J 3x2,5'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '17022', 'Kabel CYKY-J 3x2,5 (C)', 'm', 36.86, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='CYKY-J 3x2,5'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELKAOS0804150', 'CYKY-J DCA 3 X 2,5', 'm', 33.59, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='CYKY-J 5x6'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '17062', 'Kabel CYKY-J 5x6 (C)', 'm', 151.73, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='CYKY-J 5x6'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELKASLJ560001', 'CYKY-J 5 X 6', 'm', 126.33, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Chránička optického kabelu'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '41996059', 'Chránička optického kabelu KOPOS HDPE 06040 AS100 VO 40mm oranžová', 'm', 81.17, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Chránička optického kabelu'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELTUOH0494261', '06040 AS100 CHRANICKA OPT. KABELU oranz', 'm', 68.52, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Krabice pod omítku'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '83097261', 'KOPOS KRABICE UNIVERZÁLNÍ KU 68-45_KA', 'ks', 9.33, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Krabice pod omítku'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELOSOS1342603', 'KU 68-45_KA KRABICE UNIVERZALNI SEDA', 'ks', 7.93, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Spínač/Přepínač'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '81493004', 'Spínač Schneider Electric Asfora řazení 6', 'ks', 77.85, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Spínač/Přepínač'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELPRSP0601677', 'EPH0400121 Přepínač střídavý', 'ks', 63.32, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Zásuvka'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '82696103', 'Zásuvka bezšroubová Sedna Design', 'ks', 110.62, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Zásuvka'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELOSOS0887627', 'SDD111012 Zásuvka bezšroubová', 'ks', 91.29, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Rozvaděč 4-modulový'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '34989217', 'SPACE ROZVADĚČ RA 4 NA OM.', 'ks', 223.5, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Rozvaděč 4-modulový'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELROBY0441152', 'RA 4 Rozvadec na povrch', 'ks', 192.4, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Jistič 1-fázový'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '89800020', 'ABB JISTIČ S201-B20', 'ks', 166.86, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Jistič 1-fázový'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELPRJI0706884', 'S(Z)201-B20', 'ks', 146.02, 'Kč', datetime('now'));

INSERT INTO MaterialPrices
(MaterialId, SupplierId, SupplierCode, SupplierName, Unit, Price, Currency, UpdatedAt) VALUES
((SELECT Id FROM Materials WHERE Name='Chránič proudový 2-pólový'), (SELECT Id FROM Suppliers WHERE Name='ELKOV'), '891901', 'ABB CHRÁNIČ FH202 AC-40/0,03', 'ks', 605.24, 'Kč', datetime('now')),
((SELECT Id FROM Materials WHERE Name='Chránič proudový 2-pólový'), (SELECT Id FROM Suppliers WHERE Name='EMAS'), 'ELOSOS0846527', 'FH202 A-40/0,03', 'ks', 742.15, 'Kč', datetime('now'));

-- =====================================================================
-- 4) DATA - PRACE
-- =====================================================================

INSERT INTO BaseMaterials (Name, BaseMaterialCoef) VALUES
('-', 1),
('Beton', 2),
('Cihla', 1.4),
('Dlažba', 1.2),
('Omítka - Sádra', 1),
('Tvárnice', 1.5),
('Železobeton', 2.5);

INSERT INTO Positions (Name, PositionCoef) VALUES
('Nízká (u podlahy)', 1),
('Strop', 1.1),
('Stěna', 1.2),
('Štafle', 1.5);

INSERT INTO Tasks (Name, BasePrice) VALUES
('Drážkování', 80),
('Osazení', 80),
('Vrtání otvor', 180),
('Vrtání průrazu', 60),
('Kabeláž', 35);

INSERT INTO Specifications (Name, Unit) VALUES
('El. krabice', 'ks'),
('Průchozí díra', 'ks'),
('Rozvaděč', 'ks'),
('Spára', 'm'),
('Vložení kabelu', 'm');

INSERT INTO TaskSpecifications (TaskId, SpecificationId)
SELECT t.Id, s.Id
FROM Tasks t
JOIN Specifications s ON (
    (t.Name='Drážkování' AND s.Name='Spára') OR
    (t.Name='Osazení' AND s.Name='El. krabice') OR
    (t.Name='Osazení' AND s.Name='Rozvaděč') OR
    (t.Name='Vrtání otvor' AND s.Name='Průchozí díra') OR
    (t.Name='Vrtání průrazu' AND s.Name='Průchozí díra') OR
    (t.Name='Kabeláž' AND s.Name='Vložení kabelu')
);

PRAGMA foreign_key_check;
