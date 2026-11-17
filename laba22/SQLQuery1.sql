USE DB478;
GO
CREATE TABLE students (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    group_name NVARCHAR(20) NOT NULL,
    course INT NOT NULL,
    grade DECIMAL(3,2),
    created_at DATETIME DEFAULT GETDATE()
);