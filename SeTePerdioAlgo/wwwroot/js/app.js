const API = '/api/items';

const grid = document.getElementById('grid');
const vacio = document.getElementById('vacio');
const buscador = document.getElementById('buscador');
const tabs = document.querySelectorAll('.tab');
const btnPublicar = document.getElementById('btnPublicar');
const formPublicar = document.getElementById('formPublicar');
const previewFotos = document.getElementById('previewFotos');
const detalleContenido = document.getElementById('detalleContenido');

let tipoActivo = '';
let terminoBusqueda = '';
let itemsActuales = [];

function abrirModal(id) { document.getElementById(id).classList.remove('oculto'); }
function cerrarModal(id) { document.getElementById(id).classList.add('oculto'); }

document.querySelectorAll('[data-cerrar]').forEach(btn => {
  btn.addEventListener('click', () => cerrarModal(btn.dataset.cerrar));
});

btnPublicar.addEventListener('click', () => abrirModal('modalPublicar'));

tabs.forEach(tab => {
  tab.addEventListener('click', () => {
    tabs.forEach(t => t.classList.remove('activo'));
    tab.classList.add('activo');
    tipoActivo = tab.dataset.tipo;
    cargarItems();
  });
});

let debounce;
buscador.addEventListener('input', () => {
  clearTimeout(debounce);
  debounce = setTimeout(() => {
    terminoBusqueda = buscador.value.trim();
    cargarItems();
  }, 300);
});

formPublicar.querySelector('[name="fotos"]').addEventListener('change', mostrarPreview);

function mostrarPreview(e) {
  previewFotos.innerHTML = '';
  [...e.target.files].forEach(file => {
    const img = document.createElement('img');
    img.src = URL.createObjectURL(file);
    previewFotos.appendChild(img);
  });
}

formPublicar.addEventListener('submit', async (e) => {
  e.preventDefault();
  const formData = new FormData(formPublicar);
  const boton = formPublicar.querySelector('button[type="submit"]');
  boton.disabled = true;
  boton.textContent = 'Publicando...';

  try {
    const res = await fetch(API, { method: 'POST', body: formData });
    if (!res.ok) {
      const error = await res.json().catch(() => ({}));
      throw new Error(error.mensaje || 'Error al publicar');
    }
    formPublicar.reset();
    previewFotos.innerHTML = '';
    cerrarModal('modalPublicar');
    cargarItems();
  } catch (err) {
    alert(err.message || 'No se pudo publicar. Verifica que el backend esté corriendo.');
    console.error(err);
  } finally {
    boton.disabled = false;
    boton.textContent = 'Publicar';
  }
});

function tiempoRelativo(fechaISO) {
  const diffMs = Date.now() - new Date(fechaISO).getTime();
  const horas = Math.floor(diffMs / 3600000);
  if (horas < 1) return 'Hace unos minutos';
  if (horas < 24) return `Hace ${horas} h`;
  const dias = Math.floor(horas / 24);
  return `Hace ${dias} d`;
}

function tarjetaHTML(item) {
  const foto = item.fotos.find(f => f.esPrincipal) || item.fotos[0];
  const badgeClase = item.tipo === 'Perdido' ? 'badge-perdido' : 'badge-encontrado';
  return `
    <article class="card" data-id="${item.id}">
      <div class="card-imagen">
        ${foto ? `<img src="${foto.url}" alt="${item.titulo}" />` : '<div class="sin-foto">Sin foto</div>'}
        <span class="badge ${badgeClase}">${item.tipo}</span>
        ${item.estado === 'Resuelto' ? '<span class="badge badge-resuelto">Resuelto</span>' : ''}
      </div>
      <div class="card-info">
        <h3>${item.titulo}</h3>
        <p class="card-ubicacion">📍 ${item.ubicacion}</p>
        <p class="card-fecha">${tiempoRelativo(item.fecha)}</p>
      </div>
    </article>`;
}

async function cargarItems() {
  const params = new URLSearchParams();
  if (tipoActivo) params.set('tipo', tipoActivo);
  if (terminoBusqueda) params.set('q', terminoBusqueda);

  try {
    const res = await fetch(`${API}?${params.toString()}`);
    if (!res.ok) throw new Error('El backend respondió con un error');
    itemsActuales = await res.json();

    grid.innerHTML = itemsActuales.map(tarjetaHTML).join('');
    vacio.classList.toggle('oculto', itemsActuales.length > 0);
    vacio.textContent = 'Aún no hay publicaciones. ¡Sé el primero en publicar!';

    grid.querySelectorAll('.card').forEach(card => {
      card.addEventListener('click', () => {
        const item = itemsActuales.find(i => i.id === card.dataset.id);
        if (item) mostrarDetalle(item);
      });
    });
  } catch (err) {
    console.error(err);
    grid.innerHTML = '';
    vacio.textContent = 'No se pudo conectar con el backend. ¿Está corriendo con "dotnet run"?';
    vacio.classList.remove('oculto');
  }
}

function mostrarDetalle(item) {
  const fotosHTML = item.fotos.length
    ? `<div class="detalle-fotos">${item.fotos.map(f => `<img src="${f.url}" />`).join('')}</div>`
    : '<p class="sin-foto">Sin fotos</p>';

  detalleContenido.innerHTML = `
    <span class="badge ${item.tipo === 'Perdido' ? 'badge-perdido' : 'badge-encontrado'}">${item.tipo}</span>
    <h2>${item.titulo}</h2>
    ${fotosHTML}
    <p><strong>Descripción:</strong> ${item.descripcion}</p>
    <p><strong>Categoría:</strong> ${item.categoria}</p>
    <p><strong>Ubicación:</strong> ${item.ubicacion}</p>
    <p><strong>Publicado por:</strong> ${item.nombreContacto} — ${item.contacto}</p>
    <p><strong>Estado:</strong> ${item.estado}</p>
    <div class="detalle-acciones">
      ${item.estado === 'Activo'
        ? `<button class="btn-secundario" data-accion="resolver" data-id="${item.id}">Marcar como resuelto</button>`
        : ''}
      <button class="btn-peligro" data-accion="eliminar" data-id="${item.id}">Eliminar</button>
    </div>
  `;

  detalleContenido.querySelector('[data-accion="resolver"]')?.addEventListener('click', () => marcarResuelto(item.id));
  detalleContenido.querySelector('[data-accion="eliminar"]')?.addEventListener('click', () => eliminarItem(item.id));

  abrirModal('modalDetalle');
}

async function marcarResuelto(id) {
  await fetch(`${API}/${id}/estado`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ estado: 'Resuelto' })
  });
  cerrarModal('modalDetalle');
  cargarItems();
}

async function eliminarItem(id) {
  if (!confirm('¿Eliminar esta publicación?')) return;
  await fetch(`${API}/${id}`, { method: 'DELETE' });
  cerrarModal('modalDetalle');
  cargarItems();
}

cargarItems();
