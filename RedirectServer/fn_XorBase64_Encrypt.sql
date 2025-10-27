CREATE OR ALTER FUNCTION fn_XorBase64_Encrypt
(
    @plainText VARCHAR(MAX),
    @key VARCHAR(100)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    IF @plainText IS NULL OR LEN(@plainText) = 0 RETURN '';
    IF (@key IS NULL OR LEN(@key) = 0) SET @key = 'DefaultKey'; -- set default key value

    DECLARE 
        @i INT = 1,
        @lenPlain INT = LEN(@plainText),
        @keyLen INT = LEN(@key),
        @result VARBINARY(MAX) = 0x,
        @p INT,
        @k INT,
        @x INT;

    WHILE @i <= @lenPlain
    BEGIN
        SET @p = UNICODE(SUBSTRING(@plainText, @i, 1));                 -- Unicode code point (0..65535)
        SET @k = UNICODE(SUBSTRING(@key, ((@i - 1) % @keyLen) + 1, 1));
        SET @x = @p ^ @k;                                               -- bitwise XOR on integers

        -- represent result for this char as 2 bytes (NVARCHAR -> 2 bytes)
        SET @result = @result + CAST(@x AS BINARY(2));

        SET @i += 1;
    END;

    -- convert varbinary to base64 NVARCHAR
    RETURN CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@result"))','NVARCHAR(MAX)');
END;
GO

