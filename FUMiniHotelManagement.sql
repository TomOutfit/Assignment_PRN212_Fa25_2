-- FUMiniHotelManagement Database Script
-- Assignment 2: SQL-based Hotel Management System

USE master;
GO

-- Drop database if exists (để reset hoàn toàn)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'FUMiniHotelManagement')
BEGIN
    ALTER DATABASE FUMiniHotelManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FUMiniHotelManagement;
END
GO

-- Create database mới
CREATE DATABASE FUMiniHotelManagement;
GO

USE FUMiniHotelManagement;
GO

-- Tạo các bảng (Giữ nguyên cấu trúc)
-- RoomTypes, Customers, RoomInformation, Bookings
-- ... (Phần tạo bảng được giữ nguyên như script gốc) ...

-- Create RoomTypes table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RoomTypes' AND xtype='U')
BEGIN
    CREATE TABLE RoomTypes (
        RoomTypeID int IDENTITY(1,1) PRIMARY KEY,
        RoomTypeName nvarchar(50) NOT NULL,
        TypeDescription nvarchar(250),
        TypeNote nvarchar(250)
    );
END
GO

-- Create Customers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
BEGIN
    CREATE TABLE Customers (
        CustomerID int IDENTITY(1,1) PRIMARY KEY,
        CustomerFullName nvarchar(50) NOT NULL,
        Telephone nvarchar(12) NOT NULL,
        EmailAddress nvarchar(50) NOT NULL,
        CustomerBirthday datetime2 NOT NULL,
        CustomerStatus int NOT NULL DEFAULT 1, -- 1: Active, 2: Deleted
        Password nvarchar(50) NOT NULL
    );
END
GO

-- Create RoomInformation table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RoomInformation' AND xtype='U')
BEGIN
    CREATE TABLE RoomInformation (
        RoomID int IDENTITY(1,1) PRIMARY KEY,
        RoomNumber nvarchar(50) NOT NULL,
        RoomDescription nvarchar(220),
        RoomMaxCapacity int NOT NULL,
        RoomStatus int NOT NULL DEFAULT 1, -- 1: Active, 2: Deleted
        RoomPricePerDate decimal(18,2) NOT NULL,
        RoomTypeID int NOT NULL,
        FOREIGN KEY (RoomTypeID) REFERENCES RoomTypes(RoomTypeID)
    );
END
GO

-- Create Bookings table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Bookings' AND xtype='U')
BEGIN
    CREATE TABLE Bookings (
        BookingID int IDENTITY(1,1) PRIMARY KEY,
        CustomerID int NOT NULL,
        RoomID int NOT NULL,
        CheckInDate datetime2 NOT NULL,
        CheckOutDate datetime2 NOT NULL,
        TotalAmount decimal(18,2) NOT NULL,
        BookingStatus int NOT NULL DEFAULT 1, -- 1: Booked, 0: Not Booked
        CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
        Notes nvarchar(500),
        FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
        FOREIGN KEY (RoomID) REFERENCES RoomInformation(RoomID)
    );
END
GO

-- Insert sample data for RoomTypes (Giữ nguyên)
INSERT INTO RoomTypes (RoomTypeName, TypeDescription, TypeNote) VALUES
    ('Standard', 'Basic room with essential amenities', 'Single or double occupancy'),
    ('Deluxe', 'Spacious room with premium amenities', 'Up to 4 guests'),
    ('Suite', 'Luxury suite with separate living area', 'Up to 6 guests'),
    ('Presidential', 'Ultimate luxury with all amenities', 'Up to 8 guests'),
    ('Economy', 'Budget-friendly room with basic facilities', 'Single occupancy only'),
    ('Family', 'Large room perfect for families', 'Up to 5 guests'),
    ('Business', 'Room with work desk and business amenities', 'Up to 3 guests'),
    ('Honeymoon', 'Romantic room with special decorations', 'Couples only'),
    ('Executive', 'High-end room with executive services', 'Up to 4 guests'),
    ('Penthouse', 'Top-floor luxury with panoramic views', 'Up to 10 guests'),
    ('Garden View', 'Room overlooking hotel gardens', 'Up to 4 guests'),
    ('City View', 'Room with city skyline views', 'Up to 3 guests'),
    ('Poolside', 'Room with direct pool access', 'Up to 4 guests'),
    ('Corner Suite', 'Corner room with extra space', 'Up to 6 guests'),
    ('Accessible', 'Wheelchair accessible room', 'Up to 2 guests');
GO

-- Insert sample data for Customers (Giữ nguyên 47 khách hàng từ 1 đến 47)
INSERT INTO Customers (CustomerFullName, Telephone, EmailAddress, CustomerBirthday, CustomerStatus, Password) VALUES
    ('Admin User', '0123456789', 'admin@FUMiniHotelSystem.com', '1990-01-01', 1, '@@abc123@@'), -- ID 1 (Admin)
    ('John Doe', '0987654321', 'john.doe@email.com', '1985-05-15', 1, 'password123'), -- ID 2 (Customer)
    ('Jane Smith', '0555666777', 'jane.smith@email.com', '1992-08-20', 1, 'password456'),
    ('Mike Johnson', '0333444555', 'mike.johnson@email.com', '1988-12-10', 1, 'password789'),
    ('Sarah Wilson', '0111222333', 'sarah.wilson@email.com', '1995-03-22', 1, 'sarah123'),
    ('David Brown', '0222333444', 'david.brown@email.com', '1987-07-14', 1, 'david456'),
    ('Emily Davis', '0333444555', 'emily.davis@email.com', '1993-11-08', 1, 'emily789'),
    ('Michael Garcia', '0444555666', 'michael.garcia@email.com', '1989-09-30', 1, 'michael123'),
    ('Lisa Martinez', '0555666777', 'lisa.martinez@email.com', '1991-04-17', 1, 'lisa456'),
    ('Robert Anderson', '0666777888', 'robert.anderson@email.com', '1986-12-03', 1, 'robert789'),
    ('Jennifer Taylor', '0777888999', 'jennifer.taylor@email.com', '1994-06-25', 1, 'jennifer123'),
    ('William Thomas', '0888999000', 'william.thomas@email.com', '1983-02-11', 1, 'william456'),
    ('Amanda Jackson', '0999000111', 'amanda.jackson@email.com', '1996-10-19', 1, 'amanda789'),
    ('Christopher White', '1000111222', 'christopher.white@email.com', '1984-08-07', 1, 'chris123'),
    ('Michelle Harris', '1111222333', 'michelle.harris@email.com', '1990-01-15', 1, 'michelle456'),
    ('Daniel Martin', '1222333444', 'daniel.martin@email.com', '1988-05-28', 1, 'daniel789'),
    ('Ashley Thompson', '1333444555', 'ashley.thompson@email.com', '1992-12-12', 1, 'ashley123'),
    ('Matthew Garcia', '1444555666', 'matthew.garcia@email.com', '1987-03-05', 1, 'matthew456'),
    ('Jessica Martinez', '1555666777', 'jessica.martinez@email.com', '1995-09-18', 1, 'jessica789'),
    ('Andrew Robinson', '1666777888', 'andrew.robinson@email.com', '1989-11-21', 1, 'andrew123'),
    ('Stephanie Clark', '1777888999', 'stephanie.clark@email.com', '1993-07-04', 1, 'stephanie456'),
    ('Kevin Rodriguez', '1888999000', 'kevin.rodriguez@email.com', '1986-04-13', 1, 'kevin789'),
    ('Nicole Lewis', '1999000111', 'nicole.lewis@email.com', '1991-08-26', 1, 'nicole123'),
    ('Ryan Lee', '2000111222', 'ryan.lee@email.com', '1988-12-09', 1, 'ryan456'),
    ('Brittany Walker', '2111222333', 'brittany.walker@email.com', '1994-06-02', 1, 'brittany789'),
    ('Tyler Hall', '2222333444', 'tyler.hall@email.com', '1985-10-15', 1, 'tyler123'),
    ('Samantha Allen', '2333444555', 'samantha.allen@email.com', '1992-02-28', 1, 'samantha456'),
    ('Brandon Young', '2444555666', 'brandon.young@email.com', '1987-09-11', 1, 'brandon789'),
    ('Megan King', '2555666777', 'megan.king@email.com', '1990-05-24', 1, 'megan123'),
    ('Justin Wright', '2666777888', 'justin.wright@email.com', '1989-01-07', 1, 'justin456'),
    ('Rachel Lopez', '2777888999', 'rachel.lopez@email.com', '1993-11-20', 1, 'rachel789'),
    ('Jacob Hill', '2888999000', 'jacob.hill@email.com', '1986-08-03', 1, 'jacob123'),
    ('Lauren Scott', '2999000111', 'lauren.scott@email.com', '1991-04-16', 1, 'lauren456'),
    ('Nathan Green', '3000111222', 'nathan.green@email.com', '1988-12-29', 1, 'nathan789'),
    ('Kayla Adams', '3111222333', 'kayla.adams@email.com', '1995-07-12', 1, 'kayla123'),
    ('Zachary Baker', '3222333444', 'zachary.baker@email.com', '1984-03-25', 1, 'zachary456'),
    ('Hannah Nelson', '3333444555', 'hannah.nelson@email.com', '1992-10-08', 1, 'hannah789'),
    ('Caleb Carter', '3444555666', 'caleb.carter@email.com', '1987-06-21', 1, 'caleb123'),
    ('Olivia Mitchell', '3555666777', 'olivia.mitchell@email.com', '1994-01-14', 1, 'olivia456'),
    ('Ethan Perez', '3666777888', 'ethan.perez@email.com', '1989-09-27', 1, 'ethan789'),
    ('Ava Roberts', '3777888999', 'ava.roberts@email.com', '1991-05-10', 1, 'ava123'),
    ('Noah Turner', '3888999000', 'noah.turner@email.com', '1986-12-23', 1, 'noah456'),
    ('Isabella Phillips', '3999000111', 'isabella.phillips@email.com', '1993-08-06', 1, 'isabella789'),
    ('Liam Campbell', '4000111222', 'liam.campbell@email.com', '1988-04-19', 1, 'liam123'),
    ('Sophia Parker', '4111222333', 'sophia.parker@email.com', '1990-11-02', 1, 'sophia456'),
    ('Mason Evans', '4222333444', 'mason.evans@email.com', '1985-07-15', 1, 'mason789'),
    ('Emma Edwards', '4333444555', 'emma.edwards@email.com', '1992-02-28', 1, 'emma123');
GO

-- Insert sample data for RoomInformation (Giữ nguyên 90 phòng từ 1 đến 90)
INSERT INTO RoomInformation (RoomNumber, RoomDescription, RoomMaxCapacity, RoomStatus, RoomPricePerDate, RoomTypeID) VALUES
    ('101', 'Standard room with city view', 1, 1, 100.00, 1),
    ('102', 'Standard room with garden view', 1, 1, 100.00, 1),
    ('103', 'Standard room with mountain view', 1, 1, 110.00, 1),
    ('104', 'Standard room with lake view', 1, 1, 120.00, 1),
    ('105', 'Standard room with pool view', 1, 1, 130.00, 1),
    ('106', 'Standard room with courtyard view', 1, 1, 105.00, 1),
    ('107', 'Standard room with street view', 1, 1, 95.00, 1),
    ('108', 'Standard room with park view', 1, 1, 115.00, 1),
    ('109', 'Standard room with river view', 1, 1, 125.00, 1),
    ('110', 'Standard room with forest view', 1, 1, 135.00, 1),
    ('201', 'Deluxe room with balcony', 4, 1, 200.00, 2),
    ('202', 'Deluxe room with ocean view', 4, 1, 250.00, 2),
    ('203', 'Deluxe room with city skyline view', 4, 1, 220.00, 2),
    ('204', 'Deluxe room with garden terrace', 4, 1, 230.00, 2),
    ('205', 'Deluxe room with mountain panorama', 4, 1, 240.00, 2),
    ('206', 'Deluxe room with lakefront view', 4, 1, 260.00, 2),
    ('207', 'Deluxe room with pool access', 4, 1, 270.00, 2),
    ('208', 'Deluxe room with rooftop view', 4, 1, 280.00, 2),
    ('209', 'Deluxe room with sunset view', 4, 1, 290.00, 2),
    ('210', 'Deluxe room with sunrise view', 4, 1, 300.00, 2),
    ('301', 'Suite with living room', 6, 1, 400.00, 3),
    ('302', 'Suite with kitchenette', 6, 1, 450.00, 3),
    ('303', 'Suite with dining area', 6, 1, 500.00, 3),
    ('304', 'Suite with office space', 6, 1, 550.00, 3),
    ('305', 'Suite with private balcony', 6, 1, 600.00, 3),
    ('306', 'Suite with jacuzzi', 6, 1, 650.00, 3),
    ('307', 'Suite with fireplace', 6, 1, 700.00, 3),
    ('308', 'Suite with piano', 6, 1, 750.00, 3),
    ('309', 'Suite with library', 6, 1, 800.00, 3),
    ('310', 'Suite with game room', 6, 1, 850.00, 3),
    ('401', 'Presidential suite', 8, 1, 800.00, 4),
    ('402', 'Presidential suite with butler', 8, 1, 1000.00, 4),
    ('403', 'Presidential suite with chef', 8, 1, 1200.00, 4),
    ('404', 'Presidential suite with driver', 8, 1, 1400.00, 4),
    ('405', 'Presidential suite with spa', 8, 1, 1600.00, 4),
    ('501', 'Economy single room', 1, 1, 50.00, 5),
    ('502', 'Economy single room', 1, 1, 55.00, 5),
    ('503', 'Economy single room', 1, 1, 60.00, 5),
    ('504', 'Economy single room', 1, 1, 65.00, 5),
    ('505', 'Economy single room', 1, 1, 70.00, 5),
    ('601', 'Family room with bunk beds', 5, 1, 180.00, 6),
    ('602', 'Family room with twin beds', 5, 1, 190.00, 6),
    ('603', 'Family room with queen beds', 5, 1, 200.00, 6),
    ('604', 'Family room with king bed', 5, 1, 210.00, 6),
    ('605', 'Family room with sofa bed', 5, 1, 220.00, 6),
    ('701', 'Business room with desk', 3, 1, 150.00, 7),
    ('702', 'Business room with printer', 3, 1, 160.00, 7),
    ('703', 'Business room with fax', 3, 1, 170.00, 7),
    ('704', 'Business room with conference table', 3, 1, 180.00, 7),
    ('705', 'Business room with whiteboard', 3, 1, 190.00, 7),
    ('801', 'Honeymoon suite with roses', 1, 1, 300.00, 8),
    ('802', 'Honeymoon suite with champagne', 1, 1, 350.00, 8),
    ('803', 'Honeymoon suite with chocolates', 1, 1, 400.00, 8),
    ('804', 'Honeymoon suite with flowers', 1, 1, 450.00, 8),
    ('805', 'Honeymoon suite with candles', 1, 1, 500.00, 8),
    ('901', 'Executive room with lounge', 4, 1, 250.00, 9),
    ('902', 'Executive room with bar', 4, 1, 300.00, 9),
    ('903', 'Executive room with meeting space', 4, 1, 350.00, 9),
    ('904', 'Executive room with private entrance', 4, 1, 400.00, 9),
    ('905', 'Executive room with concierge', 4, 1, 450.00, 9),
    ('1001', 'Penthouse with 360 view', 10, 1, 2000.00, 10),
    ('1002', 'Penthouse with private elevator', 10, 1, 2500.00, 10),
    ('1003', 'Penthouse with helipad', 10, 1, 3000.00, 10),
    ('1004', 'Penthouse with infinity pool', 10, 1, 3500.00, 10),
    ('1005', 'Penthouse with rooftop garden', 10, 1, 4000.00, 10),
    ('1101', 'Garden view room', 4, 1, 120.00, 11),
    ('1102', 'Garden view room with balcony', 4, 1, 140.00, 11),
    ('1103', 'Garden view room with terrace', 4, 1, 160.00, 11),
    ('1104', 'Garden view room with patio', 4, 1, 180.00, 11),
    ('1105', 'Garden view room with gazebo', 4, 1, 200.00, 11),
    ('1201', 'City view room', 3, 1, 130.00, 12),
    ('1202', 'City view room with skyline', 3, 1, 150.00, 12),
    ('1203', 'City view room with landmarks', 3, 1, 170.00, 12),
    ('1204', 'City view room with bridges', 3, 1, 190.00, 12),
    ('1205', 'City view room with monuments', 3, 1, 210.00, 12),
    ('1301', 'Poolside room', 4, 1, 160.00, 13),
    ('1302', 'Poolside room with cabana', 4, 1, 180.00, 13),
    ('1303', 'Poolside room with lounge chairs', 4, 1, 200.00, 13),
    ('1304', 'Poolside room with umbrella', 4, 1, 220.00, 13),
    ('1305', 'Poolside room with bar access', 4, 1, 240.00, 13),
    ('1401', 'Corner suite with extra space', 6, 1, 350.00, 14),
    ('1402', 'Corner suite with wrap-around balcony', 6, 1, 400.00, 14),
    ('1403', 'Corner suite with panoramic windows', 6, 1, 450.00, 14),
    ('1404', 'Corner suite with private elevator', 6, 1, 500.00, 14),
    ('1405', 'Corner suite with butler service', 6, 1, 550.00, 14),
    ('1501', 'Accessible room with wide doors', 1, 1, 100.00, 15),
    ('1502', 'Accessible room with roll-in shower', 1, 1, 110.00, 15),
    ('1503', 'Accessible room with grab bars', 1, 1, 120.00, 15),
    ('1504', 'Accessible room with lowered fixtures', 1, 1, 130.00, 15),
    ('1505', 'Accessible room with emergency call', 1, 1, 140.00, 15);
GO

-- Insert sample data for Bookings (ĐÃ SỬA: CustomerID từ 2-47 và RoomID từ 1-90)
-- Sử dụng tổng cộng 90 giao dịch (tương ứng với 90 phòng) để đơn giản hóa.
-- CustomerID sẽ lặp lại sau ID 47, RoomID sẽ tăng từ 1 đến 90.

INSERT INTO Bookings (CustomerID, RoomID, CheckInDate, CheckOutDate, TotalAmount, BookingStatus, CreatedDate, Notes) VALUES
    -- 46 giao dịch đầu tiên (CustomerID từ 2-47, RoomID từ 1-46)
    (2, 1, '2024-01-15', '2024-01-17', 200.00, 1, '2024-01-10', 'Weekend getaway'),
    (3, 2, '2024-01-20', '2024-01-22', 200.00, 1, '2024-01-12', 'Anniversary celebration'),
    (4, 3, '2024-02-01', '2024-02-03', 220.00, 1, '2024-01-25', 'Business trip'),
    (5, 4, '2024-01-25', '2024-01-27', 240.00, 1, '2024-01-20', 'City break'),
    (6, 5, '2024-02-05', '2024-02-08', 390.00, 1, '2024-01-28', 'Pool view trip'),
    (7, 6, '2024-02-10', '2024-02-12', 210.00, 1, '2024-02-01', 'Luxury experience'),
    (8, 7, '2024-02-15', '2024-02-17', 190.00, 1, '2024-02-05', 'Business meeting'),
    (9, 8, '2024-02-20', '2024-02-22', 230.00, 1, '2024-02-10', 'Romantic dinner'),
    (10, 9, '2024-02-25', '2024-02-27', 250.00, 1, '2024-02-15', 'City tour'),
    (11, 10, '2024-03-01', '2024-03-03', 270.00, 1, '2024-02-20', 'Mountain hiking'),
    (12, 11, '2024-03-05', '2024-03-07', 400.00, 1, '2024-02-25', 'Executive retreat'),
    (13, 12, '2024-03-10', '2024-03-12', 500.00, 1, '2024-03-01', 'Ocean view trip'),
    (14, 13, '2024-03-15', '2024-03-17', 440.00, 1, '2024-03-05', 'Team building'),
    (15, 14, '2024-03-20', '2024-03-22', 460.00, 1, '2024-03-10', 'Product launch'),
    (16, 15, '2024-03-25', '2024-03-27', 480.00, 1, '2024-03-15', 'Conference'),
    (17, 16, '2024-04-01', '2024-04-03', 520.00, 1, '2024-03-20', 'Wedding anniversary'),
    (18, 17, '2024-04-05', '2024-04-07', 540.00, 1, '2024-03-25', 'Birthday celebration'),
    (19, 18, '2024-04-10', '2024-04-12', 560.00, 1, '2024-04-01', 'Graduation party'),
    (20, 19, '2024-04-15', '2024-04-17', 580.00, 1, '2024-04-05', 'Honeymoon'),
    (21, 20, '2024-04-20', '2024-04-22', 600.00, 1, '2024-04-10', 'Valentine special'),
    (22, 21, '2024-04-25', '2024-04-27', 800.00, 1, '2024-04-15', 'Easter holiday'),
    (23, 22, '2024-05-01', '2024-05-03', 900.00, 1, '2024-04-20', 'Labor day'),
    (24, 23, '2024-05-05', '2024-05-07', 1000.00, 1, '2024-04-25', 'Mother day'),
    (25, 24, '2024-05-10', '2024-05-12', 1100.00, 1, '2024-05-01', 'Father day'),
    (26, 25, '2024-05-15', '2024-05-17', 1200.00, 1, '2024-05-05', 'Summer vacation'),
    (27, 26, '2024-05-20', '2024-05-22', 1300.00, 1, '2024-05-10', 'Beach holiday'),
    (28, 27, '2024-05-25', '2024-05-27', 1400.00, 1, '2024-05-15', 'Mountain retreat'),
    (29, 28, '2024-06-01', '2024-06-03', 1500.00, 1, '2024-05-20', 'Cultural tour'),
    (30, 29, '2024-06-05', '2024-06-07', 1600.00, 1, '2024-05-25', 'Food festival'),
    (31, 30, '2024-06-10', '2024-06-12', 1700.00, 1, '2024-06-01', 'Music concert'),
    (32, 31, '2024-06-15', '2024-06-17', 1600.00, 1, '2024-06-05', 'Art exhibition'),
    (33, 32, '2024-06-20', '2024-06-22', 2000.00, 1, '2024-06-10', 'Theater show'),
    (34, 33, '2024-06-25', '2024-06-27', 2400.00, 1, '2024-06-15', 'Dance performance'),
    (35, 34, '2024-07-01', '2024-07-03', 2800.00, 1, '2024-06-20', 'Sports event'),
    (36, 35, '2024-07-05', '2024-07-07', 3200.00, 1, '2024-06-25', 'Gaming tournament'),
    (37, 36, '2024-07-10', '2024-07-12', 100.00, 1, '2024-07-01', 'Tech conference'),
    (38, 37, '2024-07-15', '2024-07-17', 110.00, 1, '2024-07-05', 'Startup pitch'),
    (39, 38, '2024-07-20', '2024-07-22', 120.00, 1, '2024-07-10', 'Investment meeting'),
    (40, 39, '2024-07-25', '2024-07-27', 130.00, 1, '2024-07-15', 'Board meeting'),
    (41, 40, '2024-08-01', '2024-08-03', 140.00, 1, '2024-07-20', 'Annual review'),
    (42, 41, '2024-08-05', '2024-08-07', 360.00, 1, '2024-07-25', 'Training workshop'),
    (43, 42, '2024-08-10', '2024-08-12', 380.00, 1, '2024-08-01', 'Seminar'),
    (44, 43, '2024-08-15', '2024-08-17', 400.00, 1, '2024-08-05', 'Workshop'),
    (45, 44, '2024-08-20', '2024-08-22', 420.00, 1, '2024-08-10', 'Masterclass'),
    (46, 45, '2024-08-25', '2024-08-27', 440.00, 1, '2024-08-15', 'Certification course'),
    (47, 46, '2024-09-01', '2024-09-03', 480.00, 1, '2024-08-20', 'Leadership summit'),
    
    -- 44 giao dịch tiếp theo (CustomerID lặp lại từ 2, RoomID từ 47-90)
    (2, 47, '2024-09-05', '2024-09-07', 500.00, 1, '2024-08-25', 'Innovation forum'),
    (3, 48, '2024-09-10', '2024-09-12', 600.00, 1, '2024-09-01', 'Research symposium'),
    (4, 49, '2024-09-15', '2024-09-17', 700.00, 1, '2024-09-05', 'Academic conference'),
    (5, 50, '2024-09-20', '2024-09-22', 800.00, 1, '2024-09-10', 'Scientific meeting'),
    (6, 51, '2024-09-25', '2024-09-27', 100.00, 1, '2024-09-15', 'Medical conference'),
    (7, 52, '2024-10-01', '2024-10-03', 110.00, 1, '2024-09-20', 'Health summit'),
    (8, 53, '2024-10-05', '2024-10-07', 120.00, 1, '2024-09-25', 'Wellness retreat'),
    (9, 54, '2024-10-10', '2024-10-12', 130.00, 1, '2024-10-01', 'Spa treatment'),
    (10, 55, '2024-10-15', '2024-10-17', 140.00, 1, '2024-10-05', 'Yoga retreat'),
    (11, 56, '2024-10-20', '2024-10-22', 150.00, 1, '2024-10-10', 'Meditation session'),
    (12, 57, '2024-10-25', '2024-10-27', 160.00, 1, '2024-10-15', 'Fitness bootcamp'),
    (13, 58, '2024-11-01', '2024-11-03', 170.00, 1, '2024-10-20', 'Marathon training'),
    (14, 59, '2024-11-05', '2024-11-07', 180.00, 1, '2024-10-25', 'Triathlon prep'),
    (15, 60, '2024-11-10', '2024-11-12', 190.00, 1, '2024-11-01', 'Swimming competition'),
    (16, 61, '2024-11-15', '2024-11-17', 200.00, 1, '2024-11-05', 'Tennis tournament'),
    (17, 62, '2024-11-20', '2024-11-22', 220.00, 1, '2024-11-10', 'Golf championship'),
    (18, 63, '2024-11-25', '2024-11-27', 240.00, 1, '2024-11-15', 'Skiing holiday'),
    (19, 64, '2024-12-01', '2024-12-03', 260.00, 1, '2024-11-20', 'Snowboarding trip'),
    (20, 65, '2024-12-05', '2024-12-07', 280.00, 1, '2024-11-25', 'Ice skating'),
    (21, 66, '2024-12-10', '2024-12-12', 300.00, 1, '2024-12-01', 'Christmas market'),
    (22, 67, '2024-12-15', '2024-12-17', 340.00, 1, '2024-12-05', 'New Year celebration'),
    (23, 68, '2024-12-20', '2024-12-22', 380.00, 1, '2024-12-10', 'Holiday party'),
    (24, 69, '2024-12-25', '2024-12-27', 420.00, 1, '2024-12-15', 'Family reunion'),
    (25, 70, '2024-12-30', '2025-01-01', 460.00, 1, '2024-12-20', 'New Year Eve'),
    (26, 71, '2025-01-05', '2025-01-07', 240.00, 1, '2024-12-25', 'Winter vacation'),
    (27, 72, '2025-01-10', '2025-01-12', 260.00, 1, '2024-12-30', 'Spring festival'),
    (28, 73, '2025-01-15', '2025-01-17', 340.00, 1, '2025-01-05', 'Lunar New Year'),
    (29, 74, '2025-01-20', '2025-01-22', 380.00, 1, '2025-01-10', 'Valentine week'),
    (30, 75, '2025-01-25', '2025-01-27', 420.00, 1, '2025-01-15', 'Romantic getaway'),
    (31, 76, '2025-02-01', '2025-02-03', 320.00, 1, '2025-01-20', 'Proposal planning'),
    (32, 77, '2025-02-05', '2025-02-07', 340.00, 1, '2025-01-25', 'Engagement party'),
    (33, 78, '2025-02-10', '2025-02-12', 420.00, 1, '2025-02-01', 'Wedding preparation'),
    (34, 79, '2025-02-15', '2025-02-17', 440.00, 1, '2025-02-05', 'Bridal shower'),
    (35, 80, '2025-02-20', '2025-02-22', 500.00, 1, '2025-02-10', 'Bachelor party'),
    (36, 81, '2025-02-25', '2025-02-27', 200.00, 1, '2025-02-15', 'Wedding ceremony'),
    (37, 82, '2025-03-01', '2025-03-03', 220.00, 1, '2025-02-20', 'Honeymoon trip'),
    (38, 83, '2025-03-05', '2025-03-07', 240.00, 1, '2025-02-25', 'Anniversary celebration'),
    (39, 84, '2025-03-10', '2025-03-12', 260.00, 1, '2025-03-01', 'Birthday party'),
    (40, 85, '2025-03-15', '2025-03-17', 280.00, 1, '2025-03-05', 'Graduation ceremony'),
    (41, 86, '2025-03-20', '2025-03-22', 300.00, 1, '2025-03-10', 'Prom night'),
    (42, 87, '2025-03-25', '2025-03-27', 320.00, 1, '2025-03-15', 'Dance recital'),
    (43, 88, '2025-04-01', '2025-04-03', 340.00, 1, '2025-03-20', 'Music recital'),
    (44, 89, '2025-04-05', '2025-04-07', 360.00, 1, '2025-03-25', 'Art showcase'),
    (45, 90, '2025-04-10', '2025-04-12', 380.00, 1, '2025-04-01', 'Photography session'); -- Giao dịch cuối cùng
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX IX_Customers_EmailAddress ON Customers(EmailAddress);
CREATE NONCLUSTERED INDEX IX_RoomInformation_RoomNumber ON RoomInformation(RoomNumber);
CREATE NONCLUSTERED INDEX IX_Bookings_CustomerID ON Bookings(CustomerID);
CREATE NONCLUSTERED INDEX IX_Bookings_RoomID ON Bookings(RoomID);
CREATE NONCLUSTERED INDEX IX_Bookings_CheckInDate ON Bookings(CheckInDate);
GO

PRINT 'FUMiniHotelManagement database created and populated successfully with ID constraints enforced!';