(function ($) {
    'use strict';

    $(document).ready(function () {
        // Inicializa a tabela de usu�rios com DataTables
        // Verifica se a fun��o DataTable existe (garante que datatables.min.js foi carregado)
        if ($.fn.DataTable) {
            $('#tabela-usuarios').DataTable({
                "pageLength": 10, // N�mero de linhas por p�gina
                "language": {
                    "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Portuguese-Brasil.json" // Tradu��o
                },
                "dom": '<"top"lf<"clear">>rt<"bottom"ip<"clear">>' // Layout dos elementos (filtro, pagina��o)
                // Para outras configura��es de DataTables, adicione aqui (ex: ordena��o inicial, colunas, etc.)
            });

            // Inicializa a tabela de cupons com DataTables (se existir na p�gina)
            if ($.fn.DataTable && $('#tabela-cupons').length) { // Usamos 'tabela-cupons' para o ID
                $('#tabela-cupons').DataTable({
                    "pageLength": 10,
                    "language": {
                        "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Portuguese-Brasil.json"
                    },
                    "dom": '<"top"lf<"clear">>rt<"bottom"ip<"clear">>'
                });
            }
            // Adicione aqui inicializa��es para outras tabelas DataTables em outras p�ginas, se houver.
        });
})(jQuery);