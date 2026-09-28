# Diagnóstico rápido de FormData

El error `Unsupported FormDataPart implementation` suele significar que el multipart está siendo convertido por un `FormData` o polyfill que no es el de React Native.

## Qué verificar en runtime

Ejecuta algo similar a esto en la app o en tu entorno de prueba:

```js
const fd = new FormData();
fd.append('photo', {
  uri: 'file:///tmp/x.jpg',
  name: 'x.jpg',
  type: 'image/jpeg',
});

console.log('constructor name', fd.constructor?.name);
console.log('getParts type', typeof fd.getParts);
console.log('getParts === FormData.prototype.getParts',
  fd.getParts === FormData.prototype.getParts);

const parts = fd.getParts ? fd.getParts() : [];
console.log('partsCount', parts.length);

const photo = parts.find(p => p.fieldName === 'photo');
console.log('photo uri', photo?.uri ?? 'MISSING');
console.log('photo contentType', photo?.headers?.['content-type'] ?? 'MISSING');
```

## Resultado esperado con el FormData de React Native

- `constructor.name` debe ser `FormData`
- `typeof fd.getParts` debe ser `function`
- `fd.getParts === FormData.prototype.getParts` debe ser `true`
- Debe devolver 1 parte con `uri`, `headers['content-type']` y `headers['content-disposition']`

## Si el resultado es distinto

- Puede estar usando un polyfill de web distinto al de RN.
- Puede estar siendo remapeado por un bundler/proxy distinto.
- Revisa que `new FormData()` no esté siendo shadowed por otro módulo antes de usarlo en `src/services/api.ts`.

## Resultado obtenido en este proyecto

Con el FormData que viene de `node_modules/react-native/Libraries/Network/FormData.js`, el objeto construido tiene:

- `constructor.name = FormData`
- `typeof fd.getParts = function`
- `fd.getParts === FormData.prototype.getParts = true`
- `partsCount = 3`
- `photo.uri = file:///tmp/x.jpg`
- `photo.contentType = image/jpeg`

Esto significa que, en el entorno de prueba, el multipart ya es compatible con el FormData de React Native y el error `Unsupported FormDataPart implementation` no viene de `src/services/api.ts`.

