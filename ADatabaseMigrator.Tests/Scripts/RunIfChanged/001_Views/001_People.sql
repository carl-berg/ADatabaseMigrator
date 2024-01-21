CREATE OR ALTER VIEW People AS
    SELECT 
        Id, 
        Name, 
        DATEDIFF (year, BirthDate , GETDATE()) AS Age
    FROM Person