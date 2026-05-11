using System;
using System.Collections.Generic;
using System.Linq;
using ClienteWeb.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClienteWeb.Services
{
    public class InvoiceGeneratorService : IInvoiceGeneratorService
    {
        private const string SimarGreen = "#5aad31";
        private const string SimarDark = "#4a4a4a";

        public byte[] GenerateInvoicePdf(BillingResponse invoice)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(SimarDark));

                    page.Header().Element(header => ComposeHeader(header, invoice));
                    page.Content().Element(content => ComposeContent(content, invoice));
                    page.Footer().Element(footer => ComposeFooter(footer, invoice));
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container, BillingResponse invoice)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(SimarGreen);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("SIMAR S.A. de C.V.").Style(titleStyle);
                    column.Item().Text(text =>
                    {
                        text.Span("RFC: ").SemiBold();
                        text.Span(invoice.Issuer?.TaxId ?? "SIM120101XYZ");
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Régimen Fiscal: ").SemiBold();
                        text.Span(GetDisplayName_FiscalRegime(invoice.Issuer?.TaxRegime ?? "601"));
                    });
                });

                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text(invoice.Status == "Accepted" ? "FACTURA" : "PREFACTURA").FontSize(24).ExtraBold().FontColor(invoice.Status == "Accepted" ? SimarGreen : Colors.Grey.Medium);
                    column.Item().Text(text =>
                    {
                        text.Span("Folio: ").SemiBold();
                        text.Span(invoice.FiscalData?.InvoiceFolio ?? "PENDIENTE");
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Fecha: ").SemiBold();
                        text.Span(invoice.FiscalData?.IssueDate.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"));
                    });
                    if (!string.IsNullOrEmpty(invoice.FiscalData?.Uuid))
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("UUID: ").SemiBold().FontSize(8);
                            text.Span(invoice.FiscalData.Uuid).FontSize(8);
                        });
                    }
                });
            });
        }

        private void ComposeContent(IContainer container, BillingResponse invoice)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(20);

                // Information Section
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("RECEPTOR").SemiBold().FontColor(SimarGreen);
                        c.Item().BorderBottom(1).PaddingBottom(5).BorderColor(SimarGreen);
                        c.Item().PaddingTop(5).Text(invoice.Receiver?.Name).SemiBold();
                        c.Item().Text($"RFC: {invoice.Receiver?.TaxId}");
                        c.Item().Text($"C.P.: {invoice.Receiver?.PostalCode}");
                        c.Item().Text($"Régimen Fiscal: {GetDisplayName_FiscalRegime(invoice.Receiver?.FiscalRegime)}");
                        c.Item().Text($"Uso CFDI: {GetDisplayName_CfdiUsage(invoice.Receiver?.TaxUsage)}");
                    });

                    row.ConstantItem(50);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("DETALLES DE PAGO").SemiBold().FontColor(SimarGreen);
                        c.Item().BorderBottom(1).PaddingBottom(5).BorderColor(SimarGreen);
                        c.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Método de Pago: ").SemiBold();
                            text.Span(GetDisplayName_PaymentMethod(invoice.Financials?.PaymentMethod));
                        });
                        c.Item().Text(text =>
                        {
                            text.Span("Forma de Pago: ").SemiBold();
                            text.Span(GetDisplayName_PaymentForm(invoice.Financials?.PaymentForm));
                        });
                        c.Item().Text(text =>
                        {
                            text.Span("Moneda: ").SemiBold();
                            text.Span(invoice.Financials?.Currency ?? "MXN");
                        });
                    });
                });

                // Items Table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Descripción / Concepto");
                        header.Cell().Element(CellStyle).AlignRight().Text("Cant.");
                        header.Cell().Element(CellStyle).AlignRight().Text("P. Unitario");
                        header.Cell().Element(CellStyle).AlignRight().Text("Importe");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(SimarGreen)
                                            .Background(SimarGreen)
                                            .PaddingHorizontal(5);
                        }
                    });

                    foreach (var item in invoice.Items ?? new List<BillingItem>())
                    {
                        table.Cell().Element(ItemCellStyle).Text(item.Description);
                        table.Cell().Element(ItemCellStyle).AlignRight().Text(item.Quantity.ToString("N2"));
                        table.Cell().Element(ItemCellStyle).AlignRight().Text(item.UnitPrice.ToString("C"));
                        table.Cell().Element(ItemCellStyle).AlignRight().Text(item.Amount.ToString("C"));

                        static IContainer ItemCellStyle(IContainer container)
                        {
                            return container.BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .PaddingVertical(5)
                                            .PaddingHorizontal(5);
                        }
                    }
                });

                // Totals
                column.Item().AlignRight().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                    });

                    table.Cell().Text("Subtotal:").SemiBold();
                    table.Cell().AlignRight().Text(invoice.Financials?.Subtotal.ToString("C") ?? "$0.00");

                    table.Cell().Text("IVA (16%):").SemiBold();
                    table.Cell().AlignRight().Text(invoice.Financials?.TaxTotal.ToString("C") ?? "$0.00");

                    table.Cell().PaddingTop(5).Text("Total:").FontSize(14).SemiBold().FontColor(SimarGreen);
                    table.Cell().PaddingTop(5).AlignRight().Text(invoice.Financials?.Total.ToString("C") ?? "$0.00").FontSize(14).SemiBold().FontColor(SimarGreen);
                });

                // Status Message for Pre-invoices
                if (invoice.Status != "Accepted")
                {
                    column.Item().PaddingTop(30).Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("ESTE DOCUMENTO ES UNA PREFACTURA SIN VALIDEZ FISCAL").SemiBold().FontColor(Colors.Grey.Darken2).AlignCenter();
                        c.Item().Text("Debe ser aprobada por la administración de SIMAR para generar el CFDI correspondiente.").FontSize(8).AlignCenter();
                    });
                }
            });
        }

        private void ComposeFooter(IContainer container, BillingResponse invoice)
        {
            container.PaddingTop(20).Column(column =>
            {
                column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("SIMAR S.A. de C.V. | ").SemiBold();
                        text.Span("Gestión Integral de Residuos Especiales y Peligrosos");
                    });
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
                column.Item().AlignCenter().Text("www.simar.mx | contacto@simar.mx | Tel: (123) 456-7890").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }

        private string GetDisplayName_FiscalRegime(string code) => code switch
        {
            "601" => "601 - General Ley Personas Morales",
            "612" => "612 - Personas Físicas con Actividades",
            "626" => "626 - RESICO",
            _ => code ?? "N/A"
        };

        private string GetDisplayName_CfdiUsage(string code) => code switch
        {
            "G01" => "G01 - Adquisición mercancías",
            "G03" => "G03 - Gastos en general",
            "P01" => "P01 - Por definir",
            _ => code ?? "N/A"
        };

        private string GetDisplayName_PaymentForm(string code) => code switch
        {
            "01" => "01 - Efectivo",
            "02" => "02 - Cheque nominativo",
            "03" => "03 - Transferencia",
            "04" => "04 - Tarjeta Crédito",
            "99" => "99 - Por definir",
            _ => code ?? "N/A"
        };

        private string GetDisplayName_PaymentMethod(string code) => code switch
        {
            "PUE" => "PUE - Pago una exhibición",
            "PPD" => "PPD - Pago diferido",
            _ => code ?? "N/A"
        };
    }
}
