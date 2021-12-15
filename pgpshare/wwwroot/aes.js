
function AES() {

  let aes = {};
  
  aes.generatePBKDF2 = async (password, passwordBits, keyParams) => {
  
	let pass = undefined;
	let saltValue = (keyParams && keyParams.salt) ? keyParams.salt : crypto.getRandomValues(new Uint8Array(32));
    let rounds = (keyParams && keyParams.iterations) ? keyParams.iterations : 500000;
    
    if (password) {
      pass = await crypto.subtle.importKey('raw', new TextEncoder().encode(password), {
        "name": "PBKDF2"
      }, false, ['deriveBits']);
    }
    
    if (passwordBits) {
      pass = await crypto.subtle.importKey('raw',new Uint8Array(passwordBits),{
        "name": "PBKDF2"
      }, false, ['deriveBits'])
    }
    
    return await crypto.subtle.deriveBits({
		"name": "PBKDF2",
		"salt": saltValue,
		"iterations": rounds,
		"hash": {
			"name": "SHA-256"
			}
		}, pass, 256);

	};
  
  aes.createGcmKey = async (bits, usage) => {
	  
	  let usageArray = (usage) ? usage : ['encrypt', 'decrypt'];
	  
      return await crypto.subtle.importKey('raw', bits, {
      "name": "AES-GCM"
    }, false, usageArray);
  }
  
  
  aes.gcmEncrypt = async (message, key, iv) => {
  
    let msg = new TextEncoder().encode(message);
	let ivValue = iv || crypto.getRandomValues(new Uint8Array(12));  	
    let enc = await crypto.subtle.encrypt({
      "name": "AES-GCM",
      "iv": ivValue,
	  "tagLength": 128
    }, key, msg);
    
    let ivHash = btoa(Array.from(new Uint8Array(ivValue)).map(val => {
      return String.fromCharCode(val)
    }).join(''));
    
    let encHash = btoa(Array.from(new Uint8Array(enc)).map(val => {
      return String.fromCharCode(val)
    }).join(''));
    
    return ivHash + '.' + encHash;    
  };  
  
  aes.gcmDecrypt = async (encrypted, key) => {
  
    let parts = encrypted.split('.');
        
    let iv = new Uint8Array(atob(parts[parts.length-2]).split('').map(val => {
      return val.charCodeAt(0);
    }));
    
    let enc = new Uint8Array(atob(parts[parts.length-1]).split('').map(val => {
      return val.charCodeAt(0);
    }));
        
    let dec = await crypto.subtle.decrypt({
      "name": "AES-GCM",
      "iv": iv,
	  "tagLength": 128
    }, key, enc);
    
    return (new TextDecoder().decode(dec));    
  };
  
  aes.encrypt = async (message, password, passwordBits, iterations) => {
  
    let rounds = iterations || 500000;

    let salt = crypto.getRandomValues(new Uint8Array(32));
    let bits = await aes.generatePBKDF2(password, passwordBits, { salt: salt, iterations: rounds });
	let key  = await aes.createGcmKey(bits,  ['encrypt']);
		
	let iv = crypto.getRandomValues(new Uint8Array(12));  	
    let enc = await aes.gcmEncrypt(message, key, iv); 
    
    let iterationsHash = btoa(rounds.toString());
    
    let saltHash = btoa(Array.from(new Uint8Array(salt)).map(val => {
      return String.fromCharCode(val)
    }).join(''));
        
    let encHash = btoa(Array.from(new Uint8Array(enc)).map(val => {
      return String.fromCharCode(val)
    }).join(''));
    
    return iterationsHash + '.' + saltHash + '.' + encHash;
    
  };
  
  aes.setKeyParams = (keyParams, encrypted) => {
	  
	  let iterationsHash = btoa(keyParams.iterations.toString());
	  
	  let saltHash = btoa(Array.from(new Uint8Array(keyParams.salt)).map(val => {
		return String.fromCharCode(val)
	  }).join(''));
	  
	  return iterationsHash + '.' + saltHash + '.' + encrypted;	  
  }
  
  aes.getKeyParams = (encrypted) => {
	  let parts = encrypted.split('.');
	  let iterations = parseInt(atob(parts[0]));
	  let salt = new Uint8Array(atob(parts[1]).split('').map(val => {
		return val.charCodeAt(0);
	  }));
	  return { iterations, salt };
  }

  aes.decrypt = async (encrypted, password, passwordBits) => {
  
    let parts = encrypted.split('.');
    let rounds = parseInt(atob(parts[0]));
    
    let salt = new Uint8Array(atob(parts[1]).split('').map(val => {
      return val.charCodeAt(0);
    }));
            
    let bits = await aes.generatePBKDF2(password, passwordBits, { salt: salt, iterations: rounds });
	let key  = await aes.createGcmKey(bits,  ['decrypt']);
	  
    return await aes.gcmDecrypt(encrypted, key);    
  };
  
  aes.generatePassphrase = () => {
    var buf = new Uint8Array(1024);
	window.crypto.getRandomValues(buf);
	return btoa(String.fromCharCode.apply(null, buf));
  };
  
  aes.generateKey = async () => {
	return window.crypto.subtle.generateKey(
      {
        name: "AES-GCM",
        length: 256,
      },
      false,
      ["encrypt", "decrypt"]
    )
  };
  
  aes.generateSha256 = async (message) => {

		// hash the message
		const hashBuffer = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(message));
		
		return btoa(Array.from(new Uint8Array(hashBuffer)).map(val => {
			console.log(val);
			return String.fromCharCode(val);
		}).join(''));
	}
  
  return aes;
}
