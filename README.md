# RedirectServer

Small C# HTTP service that decodes XOR+Base64 short links and redirects users to PACS backend endpoints.
# Components

- controller/ShortLinkController.cs \- API controller exposing endpoints to resolve and redirect short links.
- service/ShortLinkService.cs \- Business logic that decodes input, builds query parts, and constructs final URIs or requests to PACS.
- client/IPacsClient & client/PacsClient.cs \- Typed HTTP client that POSTs to PACS EncryptPath and returns the token from JSON.
- Extensions/HttpClientExtensions.cs \- DI helper registering the PACS HttpClient with BaseUrl and timeout configuration.
- util/DecodeXorBase64.cs \- In-app decoder that reverses the XOR+Base64 encoding back to a Unicode string.
- fn_XorBase64_Encrypt.sql \- T‑SQL function that XORs NVARCHAR input with a repeating key and returns Base64 binary.

## Configuration
Edit `appsettings.json` and set the following under `PacsClient`:
- `BaseUrl` — base URL of the PACS backend (e.g. `https://pacq9.benhvienungbuou.vn/portal`)
- `EncryptPath` — endpoint path for encryption requests (e.g. `/CSPublicQueryService/CSPublicQueryService.svc/json/EncryptQSSecure?embed_cred=1`)
- `DefaultExpirationDays` — default expiration for generated values
Edit `launchSettings.json` to set environment variables:
- `KEY` — encryption key used in XOR+Base64 encoding/decoding (must match PACS backend)'
- `USERNAME` — username for PACS backend authentication
- `PASSWORD` — password for PACS backend authentication

## SQL encryption function

```
CREATE OR ALTER FUNCTION dbo.fn_XorBase64_Encrypt
(
@plainText NVARCHAR(MAX),
@key       NVARCHAR(100)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
IF @plainText IS NULL OR LEN(@plainText) = 0
RETURN N'';

    IF @key IS NULL OR LEN(@key) = 0
        SET @key = N'DefaultKey';

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
        SET @p = UNICODE(SUBSTRING(@plainText, @i, 1));
        SET @k = UNICODE(SUBSTRING(@key, ((@i - 1) % @keyLen) + 1, 1));
        SET @x = @p ^ @k;

        SET @result = @result + CAST(@x AS BINARY(2));
        SET @i += 1;
    END

    RETURN CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@result"))', 'NVARCHAR(MAX)');
END;
GO
```
