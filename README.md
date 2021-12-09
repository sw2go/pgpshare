# PGP Share

File sharing with end-to-end encryption

- .net core server
- html/js client
- end-to-end encryption with PGP (public/private key)
- crypto-key storage in IndexedDB
- crypto.subtle Password derivedKeys for passphrase en/decryption on client-side
- unlimited file size for up- and download (transfer in chunks)

Dependencies:
- openPGP.js
- StreamSaver.js

Requirements:
Firefox > Settings > Chronik > "anlegen" (if set to "niemals anlegen" IndexedDB is not working )
 
Deployment:
VisualStudio > Project > Publish to folder

Target-System:
Set permission for web-server process on wwwroot/data to 'full control'
When hosted in IIS remove WebDAVModule in web.config (to allow DELETE method)




