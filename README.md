# PGP Share

File sharing with end-to-end encryption

- .net core server
- html/js client
- end-to-end encryption with PGP (public/private key)
- crypto-key storage in IndexedDB
- crypto.subtle Password derivedKeys for passphrase en/decryption on client-side
- unlimited file size for up- and download (transfer in chunks)
- change password support
- Browsing of files and keys via /help.html
- Deleting of files and keys via /help.html 

Dependencies:
- openPGP.js
- StreamSaver.js

Requirements:
Firefox > Settings > Chronik > "anlegen" (if set to "niemals anlegen" IndexedDB is not working )
 
Deployment:
VisualStudio > Project > Publish to folder

Hint:
Section in *.csproj to avoid publishing 'test-data' from localhost 

  <ItemGroup>
    <Content Update="wwwroot\data\files\**\*.*" CopyToPublishDirectory="Never" />
    <Content Update="wwwroot\data\keys\**\*.*" CopyToPublishDirectory="Never" />
  </ItemGroup>


Target-System:
Set permission for web-server process on wwwroot/data to 'full control'
When hosted in IIS remove WebDAVModule in web.config (to allow DELETE method)


Further reading, if you do not trust the server! (Thanks to Daniel Hugiens for guidance)

Presentation at FOSDEM: 
https://archive.fosdem.org/2020/schedule/event/dip_securing_protonmail/

Secure Remote Password: (used by Protonmail, 1Password) 
https://medium.com/swlh/what-is-secure-remote-password-srp-protocol-and-how-to-use-it-70e415b94a76
https://www.scottbrady91.com/pake/srp-in-csharp-and-dotnet-core                      

More words for online-research in the future:
merkle-tree, verifiable random function

merkle vs. verkle
https://vitalik.ca/general/2021/06/18/verkle.html
https://www.youtube.com/watch?v=RGJOQHzg3UQ (Dankrad Feist)