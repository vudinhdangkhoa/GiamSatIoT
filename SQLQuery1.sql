create database QuetDuLieu
go
use QuetDuLieu
go

CREATE TABLE Sensor (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SensorId VARCHAR(20) NOT NULL,
    Threshold FLOAT NOT NULL,
    Unit VARCHAR(10) NOT NULL,
    Description VARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1
);
ALTER TABLE Sensor
ADD CONSTRAINT UQ_Sensor_SensorId UNIQUE(SensorId);

go
CREATE TABLE SensorData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME NOT NULL,
    SensorId VARCHAR(20) NOT NULL,
    Value FLOAT NOT NULL,
    Unit VARCHAR(10) NOT NULL,
    IsAlert BIT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (SensorId) REFERENCES Sensor(SensorId)
);
go
INSERT INTO Sensor (SensorId, Threshold, Unit, Description, IsActive)
VALUES
('S1', 50.0, 'Celsius', 'Nhiet do phong server', 1),
('H2', 80.0, '%', 'Do am nha kho', 1),
('L3', 300.0, 'Lux', 'Anh sang van phong', 1),
('G4', 1000.0, 'ppm', 'Nong do khi gas', 1),
('P5', 1.5, 'bar', 'Ap suat duong ong chinh', 1);

go
INSERT INTO SensorData (Timestamp, SensorId, Value, Unit, IsAlert)
VALUES
('2025-07-05 08:00:00', 'S1', 45.7, 'Celsius', 0),
('2025-07-05 08:05:00', 'H2', 85.2, '%', 1),
('2025-07-05 08:10:00', 'L3', 299.5, 'Lux', 0),
('2025-07-05 08:15:00', 'G4', 1200.0, 'ppm', 1),
('2025-07-05 08:20:00', 'P5', 1.2, 'bar', 0);
SELECT * FROM Sensor;
SELECT * FROM SensorData;
delete SensorData
DELETE FROM Sensor WHERE SensorId IN ('L3', 'G4', 'P5');
DELETE FROM SensorData WHERE SensorId IN ('L3', 'G4', 'P5');
