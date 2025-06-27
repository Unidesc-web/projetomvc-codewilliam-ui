(function ($) {
    'use strict';

    $(document).ready(function () {
        // Inicializa a tabela de usuários com DataTables
        // Verifica se a função DataTable existe (garante que datatables.min.js foi carregado)
        if ($.fn.DataTable) {
            $('#tabela-usuarios').DataTable({
                "pageLength": 10, // Número de linhas por página
                "language": {
                    "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Portuguese-Brasil.json" // Tradução
                },
                "dom": '<"top"lf<"clear">>rt<"bottom"ip<"clear">>' // Layout dos elementos (filtro, paginação)
                // Para outras configurações de DataTables, adicione aqui (ex: ordenação inicial, colunas, etc.)
            });

            // Inicializa a tabela de cupons com DataTables (se existir na página)
            if ($.fn.DataTable && $('#tabela-cupons').length) { // Usamos 'tabela-cupons' para o ID
                $('#tabela-cupons').DataTable({
                    "pageLength": 10,
                    "language": {
                        "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Portuguese-Brasil.json"
                    },
                    "dom": '<"top"lf<"clear">>rt<"bottom"ip<"clear">>'
                });
            }
            // Adicione aqui inicializações para outras tabelas DataTables em outras páginas, se houver.
        });
})(jQuery);