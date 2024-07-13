namespace AzureStorageEmulator.NET.Authorization.Blob.Shared_Access_Signature
{
    public class BlobSasSignatureComponents
    {
        public List<SasSignatureComponent> Components { get; set; } =
        [
            new SasSignatureComponent { ShortName = "sv", Name = "SignedVersion" },
            new SasSignatureComponent { ShortName = "sr", Name = "SignedResource" },
            new SasSignatureComponent { ShortName = "st", Name = "SignedStart" },
            new SasSignatureComponent { ShortName = "se", Name = "SignedExpiry" },
            new SasSignatureComponent { ShortName = "sp", Name = "SignedPermissions" },
            new SasSignatureComponent { ShortName = "sip", Name = "SignedIp" },
            new SasSignatureComponent { ShortName = "spr", Name = "SignedProtocol" },
            new SasSignatureComponent { ShortName = "skoid", Name = "SignedObjectId" },
            new SasSignatureComponent { ShortName = "sktid", Name = "SignedTenantId" },
            new SasSignatureComponent { ShortName = "skt", Name = "SignedKeyStartTime" },
            new SasSignatureComponent { ShortName = "ske", Name = "SignedKeyExpiryTime" },
            new SasSignatureComponent { ShortName = "skv", Name = "SignedKeyVersion" },
            new SasSignatureComponent { ShortName = "sks", Name = "SignedKeyService" },
            new SasSignatureComponent { ShortName = "saoid", Name = "SignedAuthorizedObjectId" },
            new SasSignatureComponent { ShortName = "suoid", Name = "SignedUnauthorizedObjectId" },
            new SasSignatureComponent { ShortName = "scid", Name = "SignedCorrelationId" },
            new SasSignatureComponent { ShortName = "sdd", Name = "SignedDirectoryDepth" },
            new SasSignatureComponent { ShortName = "ses", Name = "SignedEncryptionScope" },
            new SasSignatureComponent { ShortName = "sig", Name = "Signature" },
            new SasSignatureComponent { ShortName = "rscc", Name = "CacheControlHeader" },
            new SasSignatureComponent { ShortName = "rscd", Name = "ContentDispositionHeader" },
            new SasSignatureComponent { ShortName = "rsce", Name = "ContentEncodingHeader" },
            new SasSignatureComponent { ShortName = "rscl", Name = "ContentLanguageHeader" },
            new SasSignatureComponent { ShortName = "rsct", Name = "ContentTypeHeader" }
        ];
    }
}
