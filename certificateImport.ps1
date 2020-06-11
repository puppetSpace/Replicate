
param([String] $password)
$rootca = Import-PfxCertificate -FilePath 'PiRootCa.pfx' -CertStoreLocation Cert:\LocalMachine\Root -Confirm:$false -Password (ConvertTo-SecureString -AsPlainText $password -Force)
#$rootca = Get-ChildItem cert:\localmachine\root | Where-Object {$_.Thumbprint -like "F50C910808B23C815B435D552EC044843F573C2D"}
New-SelfSignedCertificate -DnsName $env:computername -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation "cert:\LocalMachine\My" -Subject "CN=$env:computername" -Signer $rootca