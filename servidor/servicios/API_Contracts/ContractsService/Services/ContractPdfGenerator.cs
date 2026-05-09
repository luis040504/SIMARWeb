using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ContractsService.Models;

namespace ContractsService.Services;

/// <summary>
/// Genera el PDF del contrato de prestación de servicios replicando
/// el formato legal de la vista previa del frontend.
/// </summary>
public static class ContractPdfGenerator
{
    private static readonly string GrayBg = "#E6E6E6";
    private static readonly string Black = "#000000";
    private static readonly string White = "#FFFFFF";

    public static byte[] Generate(Contract contract, Quotation? quotation)
    {
        string clientName = quotation?.ClientName ?? $"Cliente #{contract.ClientId}";
        string clientRfc = quotation?.ClientRfc ?? "";
        string representative = quotation?.ContactName ?? "";

        var document = Document.Create(container =>
        {
            // --- PÁGINA PRINCIPAL ---
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(1.8f, Unit.Centimetre);
                page.MarginHorizontal(2.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    col.Spacing(0);

                    col.Item().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(s => s.Bold().FontSize(10));
                        t.Span("CONTRATO DE PRESTACIÓN DE SERVICIOS");
                    });
                    col.Item().PaddingTop(8);

                    BuildHeaderTable(col, contract, clientName, clientRfc, representative);

                    col.Item().PaddingTop(15).Background(Color.FromHex(Black))
                        .Padding(7).AlignCenter()
                        .Text(t =>
                        {
                            t.DefaultTextStyle(s => s.Bold().FontSize(10.5f).FontColor(Color.FromHex(White)));
                            t.Span("CLÁUSULAS");
                        });

                    col.Item().PaddingTop(10);
                    BuildClauses(col, contract, representative);
                });
            });

            // --- PÁGINA ANEXO (solo si hay extras) ---
            if (contract.Extras != null && contract.Extras.Count > 0)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginVertical(1.8f, Unit.Centimetre);
                    page.MarginHorizontal(2.2f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        col.Item().Background(Color.FromHex(Black))
                            .Padding(7).AlignCenter()
                            .Text(t =>
                            {
                                t.DefaultTextStyle(s => s.Bold().FontSize(10.5f).FontColor(Color.FromHex(White)));
                                t.Span("ANEXO ÚNICO: CATÁLOGO DE SERVICIOS EXTRAORDINARIOS");
                            });

                        col.Item().PaddingTop(10)
                            .DefaultTextStyle(s => s.FontSize(9.5f).LineHeight(1.45f))
                            .Text("De conformidad con la Cláusula Cuarta, en caso de requerir servicios adicionales a los descritos en las cláusulas operativas, se aplicarán los siguientes costos autorizados:");

                        col.Item().PaddingTop(10);
                        BuildExtrasTable(col, contract);

                        col.Item().PaddingTop(60);
                        BuildSignatureBlock(col, representative);
                    });
                });
            }
        });

        return document.GeneratePdf();
    }

    private static void BuildHeaderTable(ColumnDescriptor col, Contract contract, string clientName, string clientRfc, string representative)
    {
        col.Item().Border(1).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(15);
                c.RelativeColumn(12);
                c.RelativeColumn(10);
                c.RelativeColumn(15);
                c.RelativeColumn(13);
                c.RelativeColumn(13);
                c.RelativeColumn(10);
                c.RelativeColumn(12);
            });

            // Título
            CellText(table.Cell().ColumnSpan(8).Element(GrayCell), "CONTRATO DE PRESTACIÓN DE SERVICIOS", 9, bold: true);

            // Fecha
            CellText(table.Cell().Element(GrayLeftCell), "FECHA", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell), $"[{contract.CreatedAt:dd/MM/yyyy}]", 8.5f);

            // === EL PRESTADOR ===
            CellText(table.Cell().ColumnSpan(8).Element(GrayCell), "EL PRESTADOR", 8.5f, bold: true);

            CellText(table.Cell().Element(GrayLeftCell), "RAZÓN SOCIAL", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell), "SISTEMAS EN MANEJO Y ADMINISTRACION DE RESIDUOS S.A. DE C.V.", 8.5f, bold: true);

            // Constitutiva
            CellText(table.Cell().Element(GrayLeftCell), "CONSTITUTIVA NÚMERO", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "556", 8f);
            CellText(table.Cell().Element(GrayCell), "FECHA", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "23 de octubre de 2008", 7.5f);
            CellText(table.Cell().Element(GrayCell), "CORREDOR PÚBLICO:", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "Número 08, Plaza Veracruz.", 7.5f);
            CellText(table.Cell().Element(GrayCell), "FOLIO MERCANTIL", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "19779*11", 7.5f);

            // Apoderado
            CellText(table.Cell().Element(GrayLeftCell), "APODERADO", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(3).Element(CenterCell), "GUSTAVO CRUZ TORRES", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(2).Element(GrayCell), "IDENTIFICACIÓN DEL APODERADO LEGAL", 7f, bold: true);
            CellText(table.Cell().ColumnSpan(2).Element(CenterCell), "Credencial para votar con fotografía expedida por el INE.", 7f);

            // Instrumento
            CellText(table.Cell().Element(GrayLeftCell), "INSTRUMENTO NÚMERO", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "556", 8f);
            CellText(table.Cell().Element(GrayCell), "FECHA", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "23 de octubre de 2008", 7.5f);
            CellText(table.Cell().Element(GrayCell), "CORREDOR PÚBLICO:", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "Número 08, Plaza Veracruz.", 7.5f);
            CellText(table.Cell().Element(GrayCell), "FOLIO MERCANTIL", 7.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "19779*11", 7.5f);

            // Objeto Social Prestador
            CellText(table.Cell().Element(GrayLeftCell), "OBJETO SOCIAL", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell),
                "Realizar la recolección de residuos peligrosos y no peligrosos, sea al gobierno federal, estatal o municipal, se tratare de sus órganos centrales o descentralizados así como otra persona física o moral de nacionalidad mexicana o extranjera.", 8f);

            // Domicilio Prestador
            CellText(table.Cell().Element(GrayLeftCell), "DOMICILIO", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(5).Element(DataCell), "Calle Pípila, N.E. 126, José Cardel, Xalapa, Ver. C.P. :91030", 8f);
            CellText(table.Cell().Element(GrayCell), "R.F.C.", 8.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), "SMA0810239L9", 8.5f, bold: true);

            // Declaraciones Prestador
            CellText(table.Cell().Element(GrayLeftCell), "DECLARACIONES", 8.5f, bold: true);
            table.Cell().ColumnSpan(7).Element(DataCell).Column(c =>
            {
                DeclLine(c, "a.", "Es una sociedad anónima de capital variable...");
                DeclLine(c, "b.", "Su apoderado legal, cuenta con las facultades...");
                DeclLine(c, "c.", "Cuenta con los recursos humanos y materiales...");
                DeclLine(c, "d.", "Cuenta con los recursos económicos...");
                DeclLine(c, "e.", "Es su intención celebrar el presente contrato...");
            });

            // === EL CLIENTE ===
            CellText(table.Cell().ColumnSpan(8).Element(GrayCell), "\"EL CLIENTE\"", 8.5f, bold: true);

            CellText(table.Cell().Element(GrayLeftCell), "RAZÓN SOCIAL", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell), clientName, 8.5f, bold: true);

            CellText(table.Cell().Element(GrayLeftCell), "APODERADO LEGAL", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell), representative, 8.5f, bold: true);

            CellText(table.Cell().Element(GrayLeftCell), "OBJETO SOCIAL", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell), contract.ClientObjetoSocial ?? "", 8f);

            CellText(table.Cell().Element(GrayLeftCell), "DOMICILIO", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(5).Element(DataCell), "Pendiente de captura", 8f);
            CellText(table.Cell().Element(GrayCell), "R.F.C.", 8.5f, bold: true);
            CellText(table.Cell().Element(CenterCell), clientRfc, 8.5f, bold: true);

            CellText(table.Cell().Element(GrayLeftCell), "DECLARACIONES", 8.5f, bold: true);
            CellText(table.Cell().ColumnSpan(7).Element(DataCell), contract.ClientDeclaraciones ?? "", 7.5f);
        });
    }

    private static void BuildClauses(ColumnDescriptor col, Contract contract, string representative)
    {
        float bs = 9.5f;

        // --- PRIMERA ---
        ClauseTitle(col, "PRIMERA. - OBJETO Y ALCANCES OPERATIVOS.", bs);
        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("En virtud del presente instrumento, ");
            t.Span("EL PRESTADOR").Bold();
            t.Span(" se obliga a prestar a ");
            t.Span("EL CLIENTE").Bold();
            t.Span(" el servicio de recolección, transporte y disposición final de residuos, de conformidad con las especificaciones técnicas y operativas detalladas en la siguiente tabla de servicios:");
        });

        col.Item().PaddingTop(8);
        BuildServicesTable(col, contract);

        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("Las partes acuerdan que el primer servicio operativo de recolección se llevará a cabo el día ");
            t.Span(FormatDate(contract.FirstServiceDate)).Bold();
            t.Span(", sujeto a las condiciones de acceso y logística estipuladas.");
        });

        col.Item().PaddingTop(12);

        // --- SEGUNDA ---
        ClauseTitle(col, "SEGUNDA. - CONTRAPRESTACIÓN Y CONDICIONES DE PAGO.", bs);
        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("Como contraprestación por los servicios detallados en la Cláusula Primera, ");
            t.Span("EL CLIENTE").Bold();
            t.Span(" pagará a ");
            t.Span("EL PRESTADOR").Bold();
            t.Span(" la cantidad total de ");
            t.Span($"{contract.TotalBasePrice:C2} MXN").Bold();
            t.Span(" (más el Impuesto al Valor Agregado correspondiente).");
        });

        col.Item().PaddingTop(3).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f))
            .Text("Dicho monto será liquidado de conformidad con el siguiente desglose y calendario de pagos:");

        col.Item().PaddingTop(8);
        BuildPaymentsTable(col, contract);

        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("El pago se realizará mediante transferencia bancaria y ");
            t.Span("EL PRESTADOR").Bold();
            t.Span(" se obliga a expedir la factura fiscal correspondiente que ampare los servicios efectivamente prestados y aceptados a entera satisfacción de ");
            t.Span("EL CLIENTE").Bold();
            t.Span(".");
        });

        col.Item().PaddingTop(12);

        // --- TERCERA ---
        ClauseTitle(col, "TERCERA. - VIGENCIA.", bs);
        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("Las partes acuerdan que la vigencia del presente instrumento legal será de ");
            t.Span(contract.ContractDuration ?? "").Bold();
            t.Span(", y podrá ser renovado por mutuo acuerdo por escrito con antelación a su término. No obstante, en caso de rescisión, permanecerán vigentes las obligaciones en materia de confidencialidad y protección de datos.");
        });

        col.Item().PaddingTop(12);

        // --- CUARTA ---
        ClauseTitle(col, "CUARTA. - SERVICIOS EXTRAORDINARIOS.", bs);
        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("En caso de que ");
            t.Span("EL CLIENTE").Bold();
            t.Span(" solicite la prestación de servicios adicionales no contemplados en la Cláusula Primera, las partes se sujetarán a los costos estipulados en el ");
            t.Span("ANEXO ÚNICO").Bold();
            t.Span(" (Catálogo de Servicios Extraordinarios), el cual forma parte integral de este Contrato. En caso de no existir dicho anexo impreso en el presente documento, se entenderá que no aplican servicios extraordinarios autorizados.");
        });

        col.Item().PaddingTop(12);

        // --- QUINTA ---
        ClauseTitle(col, "QUINTA. - RESPONSABILIDAD LABORAL Y CONFIDENCIALIDAD.", bs);
        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("EL PRESTADOR").Bold();
            t.Span(" será el único responsable de sus empleados y del cumplimiento de la Ley Federal del Trabajo. Asimismo, ambas partes se obligan a mantener en estricto secreto la información compartida durante la vigencia de este Contrato y hasta por 10 años posteriores a su terminación.");
        });

        col.Item().PaddingTop(12);

        // --- SEXTA ---
        ClauseTitle(col, "SEXTA. - JURISDICCIÓN.", bs);
        col.Item().PaddingTop(5).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f))
            .Text("Para la interpretación y cumplimiento del presente Contrato, las partes se someten a la jurisdicción de los Tribunales Federales aplicables en los Estados Unidos Mexicanos, renunciando al fuero que pudiera corresponderles por razón de sus domicilios.");

        // --- Cierre ---
        col.Item().PaddingTop(25).DefaultTextStyle(s => s.FontSize(bs).LineHeight(1.45f)).Text(t =>
        {
            t.Span("Las partes manifiestan que, al elaborar el presente Contrato, su voluntad no se vio influenciada por algún vicio del consentimiento, por lo que, enteradas de su contenido, alcance y fuerza legal, lo suscriben por duplicado el día ");
            t.Span($"[{contract.CreatedAt:dd/MM/yyyy}]").Bold();
            t.Span(".");
        });

        // --- Firmas ---
        col.Item().PaddingTop(40);
        BuildSignatureBlock(col, representative);
    }

    private static void BuildServicesTable(ColumnDescriptor col, Contract contract)
    {
        col.Item().Border(1).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(15);
                c.RelativeColumn(10);
                c.RelativeColumn(15);
                c.RelativeColumn(12);
                c.RelativeColumn(24);
                c.RelativeColumn(24);
            });

            CellText(table.Cell().Element(GrayCell), "Tipo de Residuo", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Unidad", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Periodicidad", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Vehículos / Técnicos", 7, bold: true);
            CellText(table.Cell().Element(GrayCell), "Dirección de Recolección", 7.5f, bold: true);
            CellText(table.Cell().Element(GrayCell), "Almacén Destino", 7.5f, bold: true);

            if (contract.Services != null)
            {
                foreach (var s in contract.Services)
                {
                    CellText(table.Cell().Element(DataCell), s.WasteType, 8);
                    CellText(table.Cell().Element(DataCell), s.WasteUnit, 8);
                    CellText(table.Cell().Element(DataCell), s.Frequency, 8);
                    CellText(table.Cell().Element(CenterCell), $"{s.Vehicles} / {s.Technicians}", 8);
                    CellText(table.Cell().Element(DataCell), s.ServiceAddress, 8);
                    CellText(table.Cell().Element(DataCell), s.WarehouseAddress, 8);
                }
            }
        });
    }

    private static void BuildPaymentsTable(ColumnDescriptor col, Contract contract)
    {
        col.Item().PaddingHorizontal(40).Border(1).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(40);
                c.RelativeColumn(30);
                c.RelativeColumn(30);
            });

            CellText(table.Cell().Element(GrayCell), "Concepto / Desglose", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Monto (MXN)", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Fecha de Pago", 8, bold: true);

            if (contract.Payments != null)
            {
                foreach (var p in contract.Payments)
                {
                    CellText(table.Cell().Element(DataCell), p.Description, 8);
                    CellText(table.Cell().Element(CenterCell), $"${p.Amount:N2}", 8);
                    CellText(table.Cell().Element(CenterCell), p.PaymentDate.ToString("dd/MM/yyyy"), 8);
                }
            }
        });
    }

    private static void BuildExtrasTable(ColumnDescriptor col, Contract contract)
    {
        decimal totalExtras = 0;

        col.Item().Border(1).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(40);
                c.RelativeColumn(20);
                c.RelativeColumn(15);
                c.RelativeColumn(25);
            });

            CellText(table.Cell().Element(GrayCell), "Descripción", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Costo Unit.", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Cant.", 8, bold: true);
            CellText(table.Cell().Element(GrayCell), "Subtotal", 8, bold: true);

            if (contract.Extras != null)
            {
                foreach (var x in contract.Extras)
                {
                    var sub = x.UnitCost * x.Quantity;
                    totalExtras += sub;
                    CellText(table.Cell().Element(DataCell), x.Description, 8);
                    CellText(table.Cell().Element(CenterCell), $"${x.UnitCost:N2}", 8);
                    CellText(table.Cell().Element(CenterCell), x.Quantity.ToString(), 8);
                    CellText(table.Cell().Element(CenterCell), $"${sub:N2}", 8);
                }
            }

            CellText(table.Cell().ColumnSpan(3).Element(GrayCell), "TOTAL SERVICIOS EXTRAORDINARIOS:", 8, bold: true);
            CellText(table.Cell().Element(CenterCell), $"${totalExtras:N2}", 8, bold: true);
        });
    }

    private static void BuildSignatureBlock(ColumnDescriptor col, string representative)
    {
        col.Item().AlignCenter().Text(t =>
        {
            t.DefaultTextStyle(s => s.Bold().FontSize(10));
            t.Span("LAS PARTES");
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text(t => { t.DefaultTextStyle(s => s.Bold().FontSize(9)); t.Span("EL PRESTADOR"); });
                c.Item().PaddingTop(45).PaddingHorizontal(20).BorderBottom(1).BorderColor(Colors.Black);
                c.Item().PaddingTop(5).AlignCenter().Text(t => { t.DefaultTextStyle(s => s.Bold().FontSize(9)); t.Span("GUSTAVO CRUZ TORRES"); });
                c.Item().AlignCenter().Text(t => { t.DefaultTextStyle(s => s.FontSize(7)); t.Span("Apoderado Legal SIMAR S.A. de C.V."); });
            });

            row.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text(t => { t.DefaultTextStyle(s => s.Bold().FontSize(9)); t.Span("EL CLIENTE"); });
                c.Item().PaddingTop(45).PaddingHorizontal(20).BorderBottom(1).BorderColor(Colors.Black);
                c.Item().PaddingTop(5).AlignCenter().Text(t => { t.DefaultTextStyle(s => s.Bold().FontSize(9)); t.Span(representative); });
                c.Item().AlignCenter().Text(t => { t.DefaultTextStyle(s => s.FontSize(7)); t.Span("Apoderado Legal"); });
            });
        });
    }

    // --- HELPERS ---
    private static void CellText(IContainer container, string text, float fontSize, bool bold = false)
    {
        container.DefaultTextStyle(s =>
        {
            s = s.FontSize(fontSize);
            if (bold) s = s.Bold();
            return s;
        }).Text(text);
    }

    private static void ClauseTitle(ColumnDescriptor col, string title, float size)
    {
        col.Item().Text(t =>
        {
            t.DefaultTextStyle(s => s.Bold().FontSize(size));
            t.Span(title);
        });
    }

    private static void DeclLine(ColumnDescriptor c, string letter, string body)
    {
        c.Item().DefaultTextStyle(s => s.FontSize(7.5f)).Text(t =>
        {
            t.Span($"{letter} ").Bold();
            t.Span(body);
        });
    }

    private static IContainer GrayCell(IContainer container) =>
        container.Border(0.5f).Background(Color.FromHex(GrayBg))
            .Padding(4).AlignCenter().AlignMiddle();

    private static IContainer GrayLeftCell(IContainer container) =>
        container.Border(0.5f).Background(Color.FromHex(GrayBg))
            .Padding(4).AlignLeft().AlignMiddle();

    private static IContainer DataCell(IContainer container) =>
        container.Border(0.5f).Padding(4).AlignLeft().AlignMiddle();

    private static IContainer CenterCell(IContainer container) =>
        container.Border(0.5f).Padding(4).AlignCenter().AlignMiddle();

    private static string FormatDate(DateTime? date)
    {
        if (!date.HasValue) return "Pendiente";
        return date.Value.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-MX"));
    }
}
