$params = @{
  DnsName = "Pi Root Ca"
  KeyLength = 2048
  KeyAlgorithm = 'RSA'
  HashAlgorithm = 'SHA256'
  KeyExportPolicy = 'Exportable'
  NotAfter = (Get-Date).AddYears(5)
  CertStoreLocation = 'Cert:\LocalMachine\My'
  KeyUsage = 'CertSign','CRLSign' #fixes invalid cert error
}
$rootCA = New-SelfSignedCertificate @params
New-SelfSignedCertificate -DnsName $env:computername -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation "cert:\LocalMachine\My" -Subject "CN=$env:computername" -Signer $rootca

Export-Certificate -Cert $rootCA -FilePath "C:\rootCA.crt"
Import-Certificate -CertStoreLocation 'Cert:\LocalMachine\Root' -FilePath "C:\rootCA.crt"

Remove-Item "C:\rootCA.crt"
