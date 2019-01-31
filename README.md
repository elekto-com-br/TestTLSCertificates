# TestTLSCertificates

Pequeno programa para testar e examinar a causa de recusas de certificados pelo .Net.

## Para usar

TestTLSCertificates [url] [keep]

Se a `url` não for informada, será usado o que existir no .config em `/configuration/appSettings/add[@key='testUrl']/@value`.

Se o parâmetro `keep` for informado o conteúdo baixado será mantido no arquivo temporário criado.
