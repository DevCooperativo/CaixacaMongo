// Confirmação antes de enviar formulários destrutivos: <form data-confirm="Mensagem">
document.addEventListener('submit', function (e) {
  var msg = e.target.getAttribute('data-confirm');
  if (msg && !window.confirm(msg)) {
    e.preventDefault();
  }
});
