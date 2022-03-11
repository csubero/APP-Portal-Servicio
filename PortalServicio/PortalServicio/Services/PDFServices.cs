using PortalAPI.Contracts;
using PortalServicio.Configuration;
using PortalServicio.Models;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PortalServicio.Services
{
    public static class PDFServices
    {
        public static async Task<Stream> CreateCDTTicketReport(CDTTicket ticket, CDT cdt/*, List<Note> photos, bool preview = true*/, Stream sign = null, string identification = null, string name = null/*, bool agree = false*/)
        {
            //Font configs
            PdfStandardFont logoHeader = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            PdfStandardFont disclaimerfont = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
            PdfStandardFont title = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
            PdfStandardFont subtitle = new PdfStandardFont(PdfFontFamily.TimesRoman, 10, PdfFontStyle.Bold);
            PdfFont subHeadingFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            PdfStandardFont normal = new PdfStandardFont(PdfFontFamily.TimesRoman, 10);
            PdfStandardFont notefont = new PdfStandardFont(PdfFontFamily.TimesRoman, 7);
            PdfSolidBrush gray = new PdfSolidBrush(new PdfColor(119, 119, 119));
            PdfSolidBrush blue = new PdfSolidBrush(new PdfColor(34, 83, 142));
            int nextLine = 2;
            int newPart = 5;
            int clientdatatab = 55;
            int caseDetailSize = 70;
            float ClientSecondColumnPos = 0.70f;

            PdfDocument document = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;

            #region LogoHeader
            RectangleF bounds = new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement docheader = new PdfPageTemplateElement(bounds);
            docheader.Graphics.DrawString("REPORTE-MINUTA DE CDT", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 10));
            docheader.Graphics.DrawString(" ", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 20));
            document.Template.Top = docheader;

            Stream imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("certified.jpg");
            PdfImage img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(g.ClientSize.Width - 200, 0, 35, 35));

            imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("doclogo.jpg");
            img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(0, 0, 250, 35));
            #endregion

            #region Footer
            int heightCostaRica = 80;
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 150));
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Página {0} de {1}", pageNumber, count);
            string notes = "1. ESTIMADO SR. CLIENTE: LE ROGAMOS LEER CON CUIDADO ESTE REPORTE Y ANTES DE FIRMARLO, COMPROBAR CON NUESTRO TECNICO QUE CADA UNO DE LOS SERVICIOS Y REPUESTOS FUERON EFECTUADOS E INSTALADOS RESPECTIVAMENTE.\n" +
                "2.LAS HORAS DE TRABAJO SE INICIAN DESDE QUE EL TECNICO SALE DE NUESTRAS OFICINAS HASTA QUE REGRESE DESPUES DE CONCLUIDO EL TRABAJO.\n" +
                "3.SOLO EL DUEÑO O PERSONAL AUTORIZADO POR EL MISMO PUEDEN FIRMAR ESTE REPORTE DE SERVICIO, NINGUN TRABAJO SE INICIARA HASTA QUE NO SE HALLA RECIBIDO LA RESPECTIVA ORDEN DE COMPRA O SOLICITUD DE SERVICIO";
            compositeField.Bounds = footer.Bounds;

            //Draw address image depending of the  country
            if (cdt.Client.Country.Equals("Guatemala"))
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            }
            if (cdt.Client.Country.Equals("Costa Rica"))
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            }
            //Draw the composite field in footer.
            compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            //Add the footer template at the bottom.
            footer.Graphics.DrawString(notes, notefont, PdfBrushes.Gray, new RectangleF(0, footer.Bounds.Height - heightCostaRica - 10 - 40, footer.Bounds.Width, 100));
            document.Template.Bottom = footer;

            #endregion

            #region ClientDetails
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 55, g.ClientSize.Width, 15));
            PdfTextElement element = new PdfTextElement("Boleta #: " + (string.IsNullOrEmpty(ticket.Number) ? " " : ticket.Number), subHeadingFont, PdfBrushes.White);
            PdfLayoutResult result = element.Draw(page, new PointF(10, 55));
            float leftstart = result.Bounds.Top;

            string CaseHeader = string.Format("CDT : {0}", cdt.Number);
            SizeF CaseHeaderSize = subHeadingFont.MeasureString(CaseHeader);
            g.DrawString(CaseHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CaseHeaderSize.Width / 2), result.Bounds.Y));

            string currentDate = String.Format("Fecha: {0}", ticket.Started.ToString("dd/MM/yyyy"));
            SizeF textSize = subHeadingFont.MeasureString(currentDate);
            g.DrawString(currentDate, subHeadingFont, PdfBrushes.White, new PointF(g.ClientSize.Width - textSize.Width - 10, result.Bounds.Y));

            element = new PdfTextElement("Cliente: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + newPart));
            element = new PdfTextElement((cdt.Client != null ? cdt.Client.Alias : " "), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Dirección: ", subtitle, blue); ;
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(cdt.Client != null ? string.IsNullOrEmpty(cdt.Client.Address) ? " " : cdt.Client.Address : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Teléfono: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(cdt.Client != null ? string.IsNullOrEmpty(cdt.Client.Phone) ? " " : cdt.Client.Phone : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //element = new PdfTextElement("Ejecutivo de Cuenta: ", subtitle, blue);
            //SizeF repheadersize = element.Font.MeasureString(element.Text);
            //result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - repheadersize.Width, result.Bounds.Y));
            //element = new PdfTextElement(string.IsNullOrEmpty(cdt.Representative) ? " " : incident.Representative, normal, PdfBrushes.Black);
            //result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("E-mail: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(cdt.Client != null ? string.IsNullOrEmpty(cdt.Client.Email) ? " " : cdt.Client.Email : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Sistema: ", subtitle, blue);
            SizeF sysheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - sysheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(cdt.System != null ? string.IsNullOrEmpty(cdt.System.Name) ? " " : cdt.System.Name : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //if (!String.IsNullOrEmpty(incident.Incidence)) //Just for BAC or any client that has Incidence filled
            //{
            //    element = new PdfTextElement("Incidente: ", subtitle, blue);
            //    result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            //    element = new PdfTextElement(incident.Incidence, normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            //}
            #endregion

            #region CaseDetail
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string ProblemHeader = "Datos del informe";
            SizeF CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));

            float templateheight = result.Bounds.Bottom + (nextLine * 4) + 15 + caseDetailSize;
            element = new PdfTextElement("El presente informe detalla las actividades realizadas el día de hoy, así como los acuerdos tomados en caso de que fuese necesario y así mismo las personas presentes al momento de realizar el presente documento.Seguidamente el detalle:", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(10, result.Bounds.Bottom + (nextLine * 4) + 15, g.ClientSize.Width - 20, caseDetailSize));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, templateheight), new PointF(g.ClientSize.Width, templateheight));

            element = new PdfTextElement("Fecha Solicitud: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, templateheight));
            element = new PdfTextElement(cdt.ProjectStartDate.ToString("dd/MM/yyyy"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Entrada: ", subtitle, blue);
            SizeF startheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - startheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(ticket.Started.ToString("dd/MM/yyyy hh:mm tt"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + 2.5)), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + (nextLine / 2))));

            TimeSpan ellapsed = DateTime.Now - ticket.Started;

            element = new PdfTextElement("Tiempo Total: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            string formatted = String.Format("{0} hora{2} y {1} minuto{2}", ellapsed.Hours, ellapsed.Minutes, ellapsed.Minutes == 1 ? "" : "s");
            element = new PdfTextElement(formatted, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            //element = new PdfTextElement(ticket.Technicians.Count >= 1 ? ticket.Technicians[0].Name : " ", normal, PdfBrushes.Black);
            //result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            // DateTime Now = preview ? DateTime.Now : ticket.Finished;
            //TimeSpan ellapsed = DateTime.Now - ticket.Started;

            element = new PdfTextElement("Salida: ", subtitle, blue);
            SizeF finishheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - finishheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(String.Format("{0}", ticket.Finished.ToString("dd/MM/yyyy hh:mm tt")), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

            //element = new PdfTextElement("Segundo Técnico: ", subtitle, blue);
            //result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            //element = new PdfTextElement(ticket.Technicians.Count > 1 ? ticket.Technicians[1] == null ? "" : ticket.Technicians[1].Name : " ", normal, PdfBrushes.Black);
            //result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //element = new PdfTextElement("Tiempo Total: ", subtitle, blue);
            //SizeF totalheadersize = element.Font.MeasureString(element.Text);
            //result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));

            //if (ticket.HadLunch)
            //    ellapsed = ellapsed.Subtract(new TimeSpan(1, 0, 0));//Subtract an hour.
            //string formatted = String.Format("{0} hora{2} y {1} minuto{2}", ellapsed.Hours, ellapsed.Minutes, ellapsed.Minutes == 1 ? "" : "s");
            //element = new PdfTextElement(formatted, normal, PdfBrushes.Black);
            //result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //if (ticket.Technicians.Count >= 3)
            //{
            //    g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));
            //    element = new PdfTextElement("Tercer Técnico: ", subtitle, blue);
            //    result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            //    element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[2].Name) ? " " : ticket.Technicians[2].Name, normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //    element = new PdfTextElement(ticket.Technicians.Count == 5 ? "Quinto Técnico: " : "Cuarto Técnico: ", subtitle, blue);
            //    totalheadersize = element.Font.MeasureString(element.Text);
            //    result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
            //    element = new PdfTextElement(ticket.Technicians.Count == 5 ? (ticket.Technicians[4] == null ? " " : string.IsNullOrEmpty(ticket.Technicians[4].Name) ? " " : ticket.Technicians[4].Name) : ticket.Technicians.Count == 4 ? (string.IsNullOrEmpty(ticket.Technicians[3].Name) ? " " : ticket.Technicians[3].Name) : " ", normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            //}
            //if (ticket.Technicians.Count == 5)
            //{
            //    g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

            //    element = new PdfTextElement("Cuarto Técnico: ", subtitle, blue);
            //    result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            //    element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[3].Name) ? " " : ticket.Technicians[3].Name, normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //    element = new PdfTextElement(" ", subtitle, blue);
            //    totalheadersize = element.Font.MeasureString(element.Text);
            //    result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
            //    element = new PdfTextElement(" ", normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            //}
            #endregion

            #region Workdone
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string WorkDoneHeader = "Trabajos Realizados";
            SizeF WorkDoneHeaderSize = subHeadingFont.MeasureString(WorkDoneHeader);
            g.DrawString(WorkDoneHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (WorkDoneHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));
            templateheight = result.Bounds.Bottom + (nextLine * 4) + 15;
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.Workdone) ? " " : ticket.Workdone, normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(10, templateheight), g.ClientSize.Width - 20, new PdfLayoutFormat());
            page = result.Page;
            #endregion       
            //g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, leftstart), new PointF(0, templateheight));
            //g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(g.ClientSize.Width, leftstart), new PointF(g.ClientSize.Width, templateheight));

            #region Agreements
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string Header = "Acuerdos";
            SizeF HeaderSize = subHeadingFont.MeasureString(Header);
            g.DrawString(Header, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (HeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));
            templateheight = result.Bounds.Bottom + (nextLine * 4) + 15;
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.Agreements) ? " " : ticket.Agreements, normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(10, templateheight), g.ClientSize.Width - 20, new PdfLayoutFormat());
            page = result.Page;
            #endregion       

            #region ProductList Normal
            PdfGrid grid = new PdfGrid();
            grid.Columns.Add(3);
            foreach (var techReg in ticket.TechniciansRegistered)
            {
                PdfGridRow arow = grid.Rows.Add();
                arow.Cells[0].Value = techReg.Technician.Name;
                arow.Cells[1].Value = techReg.Started.ToString("dd/MM/yyyy hh:mm tt");
                arow.Cells[2].Value = techReg.Finished.ToString("dd/MM/yyyy hh:mm tt");
            }
            PdfGridCellStyle cellStyle = new PdfGridCellStyle();
            cellStyle.Borders.All = PdfPens.White;
            grid.RepeatHeader = true;
            PdfGridRow header = grid.Headers.Add(1)[0];
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(34, 83, 142));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(34, 83, 142));
            headerStyle.TextBrush = PdfBrushes.White;
            headerStyle.Font = subtitle;
            string[] headers = new string[] { "Técnico", "Entrada", "Salida"};
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                header.Cells[i].Value = headers[i];
            }
            header.ApplyStyle(headerStyle);
            cellStyle.Borders.Bottom = new PdfPen(new PdfColor(217, 217, 217), 0.70f);
            cellStyle.Font = normal;
            cellStyle.TextBrush = PdfBrushes.Black;
            foreach (PdfGridRow row in grid.Rows)
            {
                row.ApplyStyle(cellStyle);
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    PdfGridCell cell = row.Cells[i];
                    if (i > 0)
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                    else
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
                }
            }

            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat
            {
                Layout = PdfLayoutType.Paginate,
            };
            int[] sizes = new int[] { 200, 150, 150 };
            for (int i = 0; i < grid.Columns.Count && i < sizes.Length; i++)
                grid.Columns[i].Width = sizes[i];
            PdfGridLayoutResult gridResult = grid.Draw(page, new RectangleF(new PointF(0, result.Bounds.Bottom + nextLine), new SizeF(g.ClientSize.Width, 500)), layoutFormat);
            #endregion

            g = gridResult.Page.Graphics;
            page = gridResult.Page;
            float y = gridResult.Bounds.Bottom;
            if (y + 105 > 600) //jump to next page
            {
                page = document.Pages.Add();
                g = page.Graphics;
                y = 0f;
            }

            #region SignNormal
            string clientsignheader = "Firma del Cliente";
            //string tech1signheader = "Técnico Asignado";
            if (sign != null)
            {
                PdfImage clientsign = PdfImage.FromStream(sign);
                g.DrawImage(clientsign, new RectangleF(200, y + 10, 150, 50));
                if(!string.IsNullOrEmpty(identification))
                    clientsignheader = identification;
            }
            g.DrawString(clientsignheader, subtitle, PdfBrushes.Black, new RectangleF(250 - (subtitle.MeasureString(clientsignheader).Width / 2), y + 75, 150, 15));
            if (!string.IsNullOrEmpty(name))
                g.DrawString(name, subtitle, PdfBrushes.Black, new RectangleF(250 - (subtitle.MeasureString(identification).Width / 2), y + 90, 150, 15));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(175, y + 75), new PointF(325, y + 75));

            //g.DrawString(ticket.Technicians[0].Name, normal, PdfBrushes.Black, new RectangleF(350 - (normal.MeasureString(ticket.Technicians[0].Name).Width / 2), y + 60, 200, 15));
            //g.DrawString(tech1signheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(tech1signheader).Width / 2), y + 75, 150, 15));
            //g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, y + 75), new PointF(425, y + 75));
            #endregion

            //if (cotizarProducts.Count > 0)
            //{
            //    //y += 105;
            //    // if (y > 500) //jump to next page
            //    //{
            //    page = document.Pages.Add();
            //    g = page.Graphics;
            //    y = 0f;
            //    //}

            //    #region ProductList Financial

            //    string Cotizacion = "Cotización de Productos";
            //    SizeF CotizacionSize = title.MeasureString(Cotizacion);
            //    g.DrawString(Cotizacion, title, new PdfSolidBrush(new PdfColor(34, 83, 142)), new PointF((g.ClientSize.Width / 2) - (CotizacionSize.Width / 2), y));

            //    grid = new PdfGrid();
            //    grid.Columns.Add(5);
            //    grid.RepeatHeader = true;
            //    header = grid.Headers.Add(1)[0];

            //    headers = new string[] { "Código", "Producto", "Cant", "Precio Unitario", "Subtotal" };
            //    for (int i = 0; i < header.Cells.Count; i++)
            //    {
            //        header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            //        header.Cells[i].Value = headers[i];
            //    }
            //    foreach (var product in cotizarProducts)
            //    {
            //        PdfGridRow arow = grid.Rows.Add();
            //        arow.Cells[0].Value = product.Item1;
            //        arow.Cells[1].Value = product.Item2;
            //        arow.Cells[2].Value = product.Item3;
            //        arow.Cells[3].Value = product.Item4;
            //        arow.Cells[4].Value = product.Item5;
            //    }
            //    header.ApplyStyle(headerStyle);
            //    foreach (PdfGridRow row in grid.Rows)
            //    {
            //        row.ApplyStyle(cellStyle);
            //        for (int i = 0; i < row.Cells.Count; i++)
            //        {
            //            PdfGridCell cell = row.Cells[i];
            //            if (i > 1)
            //                cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Middle);
            //            else
            //                cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
            //        }
            //    }

            //    layoutFormat = new PdfGridLayoutFormat
            //    {
            //        Layout = PdfLayoutType.Paginate
            //    };
            //    sizes = new int[] { 90, 200, 50, 80, 75 };
            //    for (int i = 0; i < grid.Columns.Count && i < sizes.Length; i++)
            //        grid.Columns[i].Width = sizes[i];

            //    gridResult = grid.Draw(page, new RectangleF(new PointF(0, y + 20), new SizeF(500, g.ClientSize.Height - 200)), layoutFormat);

            //    #endregion
            //    g = gridResult.Page.Graphics;
            //    page = gridResult.Page;
            //    y = gridResult.Bounds.Bottom;
            //    if (y > 395) //jump to next page
            //    {
            //        page = document.Pages.Add();
            //        g = page.Graphics;
            //        y = 0f;
            //    }
            //    g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(340, y + 15), new PointF(g.ClientSize.Width, y + 15));

            //    element = new PdfTextElement("Subtotal: ", subtitle, blue);
            //    SizeF subtotalsize = element.Font.MeasureString(element.Text);
            //    result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.85) - subtotalsize.Width, y + 15));
            //    string subtotalresult = net.ToString(ticket.MoneyCurrency.Symbol + "0.00");
            //    element = new PdfTextElement(subtotalresult, normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width - normal.MeasureString(subtotalresult).Width), result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //    element = new PdfTextElement("Impuestos: ", subtitle, blue);
            //    SizeF taxessize = element.Font.MeasureString(element.Text);
            //    result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.85) - taxessize.Width, result.Bounds.Bottom));
            //    string taxesresult = taxes.ToString(ticket.MoneyCurrency.Symbol + "0.00");
            //    element = new PdfTextElement(taxesresult, normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width - normal.MeasureString(taxesresult).Width), result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //    element = new PdfTextElement("Total: ", subtitle, blue);
            //    SizeF totalsize = element.Font.MeasureString(element.Text);
            //    result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.85) - totalsize.Width, result.Bounds.Bottom));
            //    string total = (net + taxes).ToString(ticket.MoneyCurrency.Symbol + "0.00");
            //    element = new PdfTextElement(total, normal, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width - normal.MeasureString(total).Width), result.Bounds.Y, g.ClientSize.Width / 2, 100));

            //    #region SignCotizacion
            //    clientsignheader = "Firma del cliente para cotización";
            //    tech1signheader = "Técnico Asignado";
            //    if (sign != null)
            //    {
            //        if (agree)
            //        {
            //            PdfImage clientsign = PdfImage.FromStream(sign);
            //            g.DrawImage(clientsign, new RectangleF(100, result.Bounds.Bottom + 10, 150, 50));
            //            clientsignheader = name;
            //        }
            //        else
            //            clientsignheader = "Sin aprobación del usuario final";
            //    }

            //    g.DrawString(clientsignheader, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(clientsignheader).Width / 2), result.Bounds.Bottom + 75, 150, 15));
            //    if (!preview && agree)
            //        g.DrawString(identification, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(identification).Width / 2), result.Bounds.Bottom + 90, 150, 15));
            //    g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, result.Bounds.Bottom + 75), new PointF(225, result.Bounds.Bottom + 75));

            //    g.DrawString(ticket.Technicians[0].Name, normal, PdfBrushes.Black, new RectangleF(350 - (normal.MeasureString(ticket.Technicians[0].Name).Width / 2), result.Bounds.Bottom + 60, 200, 15));
            //    g.DrawString(tech1signheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(tech1signheader).Width / 2), result.Bounds.Bottom + 75, 150, 15));
            //    g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, result.Bounds.Bottom + 75), new PointF(425, result.Bounds.Bottom + 75));
            //    #endregion

            //    y = result.Bounds.Bottom + 105;
            //    if (y + 40 > 500) //jump to next page
            //    {
            //        page = document.Pages.Add();
            //        g = page.Graphics;
            //        y = 0f;
            //    }
            //    string disclaimer = "Esta cotización no incluye mano de obra y materiales, únicamente productos, por lo que debe ser considerada como una cotización parcial.";
            //    g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, y, g.ClientSize.Width, 40));
            //    g.DrawRectangle(PdfBrushes.White, new RectangleF(1, y + 1, g.ClientSize.Width - 2, 38));
            //    element = new PdfTextElement(disclaimer.ToUpper(), disclaimerfont, PdfBrushes.Black);
            //    result = element.Draw(page, new RectangleF(10, y + 5, g.ClientSize.Width - 20, 30));
            //}

            //#region Footer
            //int heightCostaRica = 80;
            //PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 400));
            ////Create page number field.
            //PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            ////Create page count field.
            //PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            ////Add the fields in composite fields.
            //PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Página {0} de {1}", pageNumber, count);
            //string notes = "1. ESTIMADO SR. CLIENTE: LE ROGAMOS LEER CON CUIDADO ESTE REPORTE Y ANTES DE FIRMARLO, COMPROBAR CON NUESTRO TECNICO QUE CADA UNO DE LOS SERVICIOS Y REPUESTOS FUERON EFECTUADOS E INSTALADOS RESPECTIVAMENTE.\n" +
            //    "2.LAS HORAS DE TRABAJO SE INICIAN DESDE QUE EL TECNICO SALE DE NUESTRAS OFICINAS HASTA QUE REGRESE DESPUES DE CONCLUIDO EL TRABAJO.\n" +
            //    "3.SOLO EL DUEÑO O PERSONAL AUTORIZADO POR EL MISMO PUEDEN FIRMAR ESTE REPORTE DE SERVICIO, NINGUN TRABAJO SE INICIARA HASTA QUE NO SE HALLA RECIBIDO LA RESPECTIVA ORDEN DE COMPRA O SOLICITUD DE SERVICIO";
            //compositeField.Bounds = footer.Bounds;

            ////Draw address image depending of the  country
            //if (incident.Client.Country.Equals("Guatemala"))
            //{
            //    imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
            //    img = PdfImage.FromStream(imgStream);
            //    footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            //}
            //if (incident.Client.Country.Equals("Costa Rica"))
            //{
            //    imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
            //    img = PdfImage.FromStream(imgStream);
            //    footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            //}
            ////Draw the composite field in footer.
            //compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            ////Add the footer template at the bottom.
            //footer.Graphics.DrawString(notes, notefont, PdfBrushes.Gray, new RectangleF(0, footer.Bounds.Height - heightCostaRica - 10 - 40, footer.Bounds.Width, 100));
            //document.Template.Bottom = footer;

            //#endregion

            MemoryStream data = new MemoryStream();
            document.Save(data);
            document.Close();
            PdfDocument finaldoc = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            //Stream xrdoc;
            //PdfLoadedDocument loadedDocument1;
            //PdfLoadedDocument loadedDocument2;
            //if (ticket.Type != null && ticket.Type.Name.Equals("Rayos X"))
            //{
            //    xrdoc = await GenerateXRayReport(ticket, incident);
            //    loadedDocument1 = new PdfLoadedDocument(data);
            //    loadedDocument2 = new PdfLoadedDocument(xrdoc);
            //    PdfDocumentBase.Merge(finaldoc, loadedDocument1);
            //    PdfDocumentBase.Merge(finaldoc, loadedDocument2);

            //    data = new MemoryStream();
            //    finaldoc.Save(data);
            //    finaldoc.Close();
            //}
            //Add evidence photos to document.
            //#region Evidence Photos          
            //finaldoc = new PdfDocument
            //{
            //    Compression = PdfCompressionLevel.Best,
            //    EnableMemoryOptimization = true
            //};
            //xrdoc = await GeneratePhotoDocument(photos);
            //loadedDocument1 = new PdfLoadedDocument(data);
            //PdfDocumentBase.Merge(finaldoc, loadedDocument1);
            //if (photos.Count > 0)
            //{
            //    loadedDocument2 = new PdfLoadedDocument(xrdoc);
            //    PdfDocumentBase.Merge(finaldoc, loadedDocument2);
            //}
            //data = new MemoryStream();
            //finaldoc.Save(data);
            //finaldoc.Close();
            //#endregion
            try //Save to storage
            {
                DependencyService.Get<ISave>().SaveTextAsync("BoletaDeCDT.pdf", "application/pdf", data, false);
            }
            catch (Exception)
            {
                throw new FileNotFoundException("No se pudo acceder al archivo BoletaDeServicio.pdf");
            }
            return data;
        }

        public static async Task<Stream> CreateServiceTicketReport(ServiceTicket ticket, Incident incident, List<Note> photos, bool preview = true, Stream sign = null, string identification = null, string name = null)
        {
            //Font configs
            PdfStandardFont logoHeader = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            PdfStandardFont subtitle = new PdfStandardFont(PdfFontFamily.TimesRoman, 10, PdfFontStyle.Bold);
            PdfFont subHeadingFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            PdfStandardFont normal = new PdfStandardFont(PdfFontFamily.TimesRoman, 10);
            PdfStandardFont notefont = new PdfStandardFont(PdfFontFamily.TimesRoman, 7);
            PdfSolidBrush gray = new PdfSolidBrush(new PdfColor(119, 119, 119));
            PdfSolidBrush blue = new PdfSolidBrush(new PdfColor(34, 83, 142));
            int nextLine = 2;
            int newPart = 5;
            int clientdatatab = 55;
            int caseDetailSize = 100;
            int workDoneSize = 100;
            int maxWorkDoneSize = 400;
            float ClientSecondColumnPos = 0.70f;

            PdfDocument document = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;

            #region LogoHeader
            RectangleF bounds = new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement docheader = new PdfPageTemplateElement(bounds);
            docheader.Graphics.DrawString("REPORTE DE SERVICIO", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 10));
            docheader.Graphics.DrawString("RSE-01.02 V3", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 20));
            document.Template.Top = docheader;

            Stream imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("certified.jpg");
            PdfImage img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(g.ClientSize.Width - 200, 0, 35, 35));

            imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("doclogo.jpg");
            img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(0, 0, 250, 35));
            #endregion

            #region ClientDetails
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 55, g.ClientSize.Width, 15));
            PdfTextElement element = new PdfTextElement("Boleta #: " + ticket.TicketNumber, subHeadingFont, PdfBrushes.White);
            PdfLayoutResult result = element.Draw(page, new PointF(10, 55));
            float leftstart = result.Bounds.Top;

            string CaseHeader = "Caso #: " + incident.TicketNumber;
            SizeF CaseHeaderSize = subHeadingFont.MeasureString(CaseHeader);
            g.DrawString(CaseHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CaseHeaderSize.Width / 2), result.Bounds.Y));

            string currentDate = String.Format("Fecha: {0}/{1}/{2}", ticket.CreationDate.Day, ticket.CreationDate.Month, ticket.CreationDate.Year);
            SizeF textSize = subHeadingFont.MeasureString(currentDate);
            g.DrawString(currentDate, subHeadingFont, PdfBrushes.White, new PointF(g.ClientSize.Width - textSize.Width - 10, result.Bounds.Y));

            element = new PdfTextElement("Cliente: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + newPart));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Alias) ? " " : incident.Client.Alias, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Dirección: ", subtitle, blue); ;
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Address) ? " " : incident.Client.Address, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Teléfono: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Phone) ? " " : incident.Client.Phone, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Ejecutivo de Cuenta: ", subtitle, blue);
            SizeF repheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - repheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Representative) ? " " : incident.Representative, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("E-mail: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Email) ? " " : incident.Client.Email, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Sistema: ", subtitle, blue);
            SizeF sysheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - sysheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Type.Name) ? "Sin Tipo" : incident.Type.Name, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            if (!String.IsNullOrEmpty(incident.Incidence)) //Just for BAC or any client that has Incidence filled
            {
                element = new PdfTextElement("Incidente: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(incident.Incidence, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            #endregion

            #region CaseDetail
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string ProblemHeader = "Detalle de la Falla";
            SizeF CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));

            float templateheight = result.Bounds.Bottom + (nextLine * 4) + 15 + caseDetailSize;
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.Description) ? "Sin descripción." : ticket.Description, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(10, result.Bounds.Bottom + (nextLine * 4) + 15, g.ClientSize.Width - 20, caseDetailSize));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, templateheight), new PointF(g.ClientSize.Width, templateheight));

            element = new PdfTextElement("Fecha Solicitud: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, templateheight));
            element = new PdfTextElement(incident.CreatedOn.ToString("dd/MM/yyyy"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Entrada: ", subtitle, blue);
            SizeF startheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - startheadersize.Width, result.Bounds.Y)); 
            element = new PdfTextElement(ticket.Started.ToString("dd/MM/yyyy hh:mm tt"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + 2.5)), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + (nextLine / 2))));

            element = new PdfTextElement("Técnico Asignado: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[0].Name) ? " " : ticket.Technicians[0].Name, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            DateTime Now = preview ? DateTime.Now : ticket.Finished;
            TimeSpan ellapsed = Now - ticket.Started;

            element = new PdfTextElement("Salida: ", subtitle, blue);
            SizeF finishheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - finishheadersize.Width, result.Bounds.Y));
            if (preview)
                element = new PdfTextElement(String.Format("{0} (Aprox)", Now.ToString("dd/MM/yyyy hh:mm tt")), normal, PdfBrushes.Black);
            else
                element = new PdfTextElement(String.Format("{0}({1}{2})", ticket.Finished.ToString("dd/MM/yyyy hh:mm tt"), ticket.Code, Config.CalculateWorkedTime(ellapsed, ticket.HadLunch)), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

            element = new PdfTextElement("Segundo Técnico: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(ticket.Technicians.Count >= 2 ? string.IsNullOrEmpty(ticket.Technicians[1].Name) ? " " : ticket.Technicians[1].Name : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Tiempo Total: ", subtitle, blue);
            SizeF totalheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));

            if (ticket.HadLunch)
                ellapsed = ellapsed.Subtract(new TimeSpan(1, 0, 0));//Subtract an hour.
            string formatted = String.Format("{0} hora{2} y {1} minuto{2} {3}", ellapsed.Hours, ellapsed.Minutes, ellapsed.Minutes == 1 ? " " : "s", preview ? "(Aprox)" : "");
            element = new PdfTextElement(formatted, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            if (ticket.Technicians.Count >= 3)
            {
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));
                element = new PdfTextElement("Tercer Técnico: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[2].Name) ? " " : ticket.Technicians[2].Name, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

                element = new PdfTextElement(ticket.Technicians.Count == 5 ? "Quinto Técnico: " : "Cuarto Técnico: ", subtitle, blue);
                totalheadersize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
                element = new PdfTextElement(ticket.Technicians.Count == 5 ? (ticket.Technicians[4] == null ? " " : string.IsNullOrEmpty(ticket.Technicians[4].Name) ? " " : ticket.Technicians[4].Name) : ticket.Technicians.Count == 4? (string.IsNullOrEmpty(ticket.Technicians[3].Name) ? " " : ticket.Technicians[3].Name) : " ", normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            if (ticket.Technicians.Count >= 4)
            {
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

                element = new PdfTextElement("Cuarto Técnico: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[3].Name) ? " " : ticket.Technicians[3].Name, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

                element = new PdfTextElement(" ", subtitle, blue);
                totalheadersize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
                element = new PdfTextElement(" ", normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            #endregion

            #region Workdone
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string WorkDoneHeader = "Trabajos Realizados";
            SizeF WorkDoneHeaderSize = subHeadingFont.MeasureString(WorkDoneHeader);
            g.DrawString(WorkDoneHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (WorkDoneHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));

            templateheight = result.Bounds.Bottom + (nextLine * 4) + 15;
            string workdone = (string.IsNullOrEmpty(ticket.WorkDone)) ? "Sin descripción" : ticket.WorkDone;
            element = new PdfTextElement(workdone, normal, PdfBrushes.Black);
            //PdfLayoutFormat resultformat = new PdfLayoutFormat();
            //result = element.Draw(page, new PointF(10, templateheight), g.ClientSize.Width - 20,resultformat);
            result = element.Draw(page, new RectangleF(10, templateheight, g.ClientSize.Width - 20, maxWorkDoneSize));
            #endregion
            templateheight = result.Bounds.Bottom > (templateheight + workDoneSize) ? result.Bounds.Bottom + (nextLine) : (templateheight + workDoneSize) + (nextLine / 2);

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, leftstart), new PointF(0, templateheight));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(g.ClientSize.Width, leftstart), new PointF(g.ClientSize.Width, templateheight));

            #region ProductList
            PdfGrid grid = new PdfGrid();
            grid.Columns.Add(5);
            foreach (var product in ticket.ProductsUsed)
            {
                PdfGridRow arow = grid.Rows.Add();
                arow.Cells[0].Value = product.Product.Id;
                arow.Cells[1].Value = product.Product.Name;
                arow.Cells[2].Value = product.Count.ToString();
                arow.Cells[3].Value = product.Serials;
                arow.Cells[4].Value = product.Treatment.ToString();
            }
            PdfGridCellStyle cellStyle = new PdfGridCellStyle();
            cellStyle.Borders.All = PdfPens.White;
            grid.RepeatHeader = true;
            PdfGridRow header = grid.Headers.Add(1)[0];
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(34, 83, 142));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(34, 83, 142));
            headerStyle.TextBrush = PdfBrushes.White;
            headerStyle.Font = subtitle;
            string[] headers = new string[] { "Código", "Producto", "Cantidad", "Número de Serie", "Tratamiento" };
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                header.Cells[i].Value = headers[i];
            }
            header.ApplyStyle(headerStyle);
            cellStyle.Borders.Bottom = new PdfPen(new PdfColor(217, 217, 217), 0.70f);
            cellStyle.Font = normal;
            cellStyle.TextBrush = PdfBrushes.Black;
            foreach (PdfGridRow row in grid.Rows)
            {
                row.ApplyStyle(cellStyle);
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    PdfGridCell cell = row.Cells[i];
                    if (i > 1)
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                    else
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
                }
            }

            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat
            {
                Layout = PdfLayoutType.Paginate
            };
            int[] sizes = new int[] { 90, 200, 50, 80, 80 };
            for (int i = 0; i < grid.Columns.Count && i < sizes.Length; i++)
                grid.Columns[i].Width = sizes[i];
            PdfGridLayoutResult gridResult = grid.Draw(page, new RectangleF(new PointF(0, templateheight), new SizeF(g.ClientSize.Width, g.ClientSize.Height - 200)), layoutFormat);

            #endregion

            #region Footer
            int heightCostaRica = 80;
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 400));
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Pagina {0} de {1}", pageNumber, count);
            string notes = "1. ESTIMADO SR. CLIENTE: LE ROGAMOS LEER CON CUIDADO ESTE REPORTE Y ANTES DE FIRMARLO, COMPROBAR CON NUESTRO TECNICO QUE CADA UNO DE LOS SERVICIOS Y REPUESTOS FUERON EFECTUADOS E INSTALADOS RESPECTIVAMENTE.\n" +
                "2.LAS HORAS DE TRABAJO SE INICIAN DESDE QUE EL TECNICO SALE DE NUESTRAS OFICINAS HASTA QUE REGRESE DESPUES DE CONCLUIDO EL TRABAJO.\n" +
                "3.SOLO EL DUEÑO O PERSONAL AUTORIZADO POR EL MISMO PUEDEN FIRMAR ESTE REPORTE DE SERVICIO, NINGUN TRABAJO SE INICIARA HASTA QUE NO SE HALLA RECIBIDO LA RESPECTIVA ORDEN DE COMPRA O SOLICITUD DE SERVICIO";
            compositeField.Bounds = footer.Bounds;

            string clientsignheader = "Firma del Cliente";
            string tech1signheader = "Técnico Asignado";
            if (sign != null)
            {
                PdfImage clientsign = PdfImage.FromStream(sign);
                footer.Graphics.DrawImage(clientsign, new RectangleF(100, footer.Height - 160 - 50, 150, 50));
                clientsignheader = name;
            }
            footer.Graphics.DrawString(clientsignheader, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(clientsignheader).Width / 2), footer.Height - 155, 150, 15));
            if (!preview)
                footer.Graphics.DrawString(identification, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(identification).Width / 2), footer.Height - 145, 150, 15));
            footer.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, footer.Height - 155), new PointF(225, footer.Height - 155));

            footer.Graphics.DrawString(ticket.Technicians[0].Name, normal, PdfBrushes.Black, new RectangleF(350 - (normal.MeasureString(ticket.Technicians[0].Name).Width / 2), footer.Height - 165, 200, 15));
            footer.Graphics.DrawString(tech1signheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(tech1signheader).Width / 2), footer.Height - 155, 150, 15));
            footer.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, footer.Height - 155), new PointF(425, footer.Height - 155));

            //Draw address image depending of the  country
            if (incident.Client.Country?.Equals("Guatemala")??false)
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            }
            else
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            }
            //Draw the composite field in footer.
            compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            //Add the footer template at the bottom.
            footer.Graphics.DrawString(notes, notefont, PdfBrushes.Gray, new RectangleF(0, footer.Bounds.Height - heightCostaRica - 10 - 40, footer.Bounds.Width, 100));
            document.Template.Bottom = footer;

            #endregion         

            MemoryStream data = new MemoryStream();

            document.Save(data);

            document.Close();
            PdfDocument finaldoc = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            Stream xrdoc;
            PdfLoadedDocument loadedDocument1;
            PdfLoadedDocument loadedDocument2;
            if (ticket.Type != null && ticket.Type.Name.Equals("Rayos X"))
            {
                xrdoc = await GenerateXRayReport(ticket, incident);
                loadedDocument1 = new PdfLoadedDocument(data);
                loadedDocument2 = new PdfLoadedDocument(xrdoc);
                PdfDocumentBase.Merge(finaldoc, loadedDocument1);
                PdfDocumentBase.Merge(finaldoc, loadedDocument2);

                data = new MemoryStream();
                finaldoc.Save(data);
                finaldoc.Close();
            }
            //Add evidence photos to document.
            #region Evidence Photos
            finaldoc = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            xrdoc = await GeneratePhotoDocument(photos);
            loadedDocument1 = new PdfLoadedDocument(data);
            PdfDocumentBase.Merge(finaldoc, loadedDocument1);
            if (photos.Count > 0)
            {
                loadedDocument2 = new PdfLoadedDocument(xrdoc);
                PdfDocumentBase.Merge(finaldoc, loadedDocument2);
            }

            data = new MemoryStream();
            finaldoc.Save(data);
            finaldoc.Close();
            #endregion


            try //Save to storage
            {
                DependencyService.Get<ISave>().SaveTextAsync("BoletaDeServicio.pdf", "application/pdf", data, preview);               
            }
            catch (Exception)
            {
                throw new FileNotFoundException("No se pudo acceder al archivo BoletaDeServicio.pdf");
            }
            return data;
        }

        public static async Task<Stream> GeneratePhotoDocument(List<Note> Photos)
        {
            PdfFont subHeadingFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            SizeF photoSize = new SizeF(200, 300);
            string EvidenceLabel = "Evidencia del trabajo";
            PdfDocument document = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;

            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 0, g.ClientSize.Width, 15));
            PdfTextElement element = new PdfTextElement(EvidenceLabel, subHeadingFont, PdfBrushes.White);
            PdfLayoutResult result = element.Draw(page, new PointF((g.ClientSize.Width / 2) - (subHeadingFont.MeasureString(EvidenceLabel).Width / 2), 0));
            int lastPos = 20;
            bool first = true;
            foreach (Note photo in Photos)
            {
                Stream s = new MemoryStream(Convert.FromBase64String(photo.Content));
                PdfImage img = PdfImage.FromStream(s);
                if (lastPos + photoSize.Height < page.Size.Height)
                {
                    g.DrawImage(img, new PointF(first ? 40 : 50 + photoSize.Width, lastPos), photoSize);
                    if (!first)
                        lastPos = lastPos + (int)photoSize.Height + 5;
                    first = !first;
                }
                else
                {
                    page = document.Pages.Add();
                    g = page.Graphics;
                    g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 0, g.ClientSize.Width, 15));
                    element = new PdfTextElement(EvidenceLabel, subHeadingFont, PdfBrushes.White);
                    result = element.Draw(page, new PointF((g.ClientSize.Width / 2) - (subHeadingFont.MeasureString(EvidenceLabel).Width / 2), 0));
                    //g.DrawImage(img, new PointF(40, 20),photoSize);
                    lastPos = 20;
                }
            }
            MemoryStream data = new MemoryStream();

            document.Save(data);

            document.Close();
            return data;
        }

        public static async Task<Stream> GenerateXRayReport(ServiceTicket ticket, Incident incident)
        {
            //Font configs
            PdfStandardFont logoHeader = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            PdfStandardFont subtitle = new PdfStandardFont(PdfFontFamily.TimesRoman, 10, PdfFontStyle.Bold);
            PdfFont subHeadingFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            PdfStandardFont normal = new PdfStandardFont(PdfFontFamily.TimesRoman, 10);
            PdfStandardFont notefont = new PdfStandardFont(PdfFontFamily.TimesRoman, 7);
            PdfSolidBrush gray = new PdfSolidBrush(new PdfColor(119, 119, 119));
            PdfSolidBrush blue = new PdfSolidBrush(new PdfColor(34, 83, 142));
            int nextLine = 2;
            int newPart = 5;
            int clientdatatab = 55;
            float ClientSecondColumnPos = 0.70f;

            PdfDocument document = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;

            #region LogoHeader
            RectangleF bounds = new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement docheader = new PdfPageTemplateElement(bounds);
            docheader.Graphics.DrawString("Checklist Mant Prev", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 10));
            docheader.Graphics.DrawString("RSE- V1", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 20));
            document.Template.Top = docheader;

            Stream imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("certified.jpg");
            PdfImage img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(g.ClientSize.Width - 200, 0, 35, 35));

            imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("doclogo.jpg");
            img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(0, 0, 250, 35));
            #endregion

            #region ClientDetails
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, 55), new PointF(0, 625));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(g.ClientSize.Width, 55), new PointF(g.ClientSize.Width, 625));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, 625), new PointF(g.ClientSize.Width, 625));
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 55, g.ClientSize.Width, 15));
            PdfTextElement element = new PdfTextElement("Boleta #: " + ticket.TicketNumber, subHeadingFont, PdfBrushes.White);
            PdfLayoutResult result = element.Draw(page, new PointF(10, 55));
            float leftstart = result.Bounds.Top;

            string CaseHeader = "Caso #: " + incident.TicketNumber;
            SizeF CaseHeaderSize = subHeadingFont.MeasureString(CaseHeader);
            g.DrawString(CaseHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CaseHeaderSize.Width / 2), result.Bounds.Y));

            string currentDate = String.Format("Fecha: {0}/{1}/{2}", ticket.CreationDate.Day, ticket.CreationDate.Month, ticket.CreationDate.Year);
            SizeF textSize = subHeadingFont.MeasureString(currentDate);
            g.DrawString(currentDate, subHeadingFont, PdfBrushes.White, new PointF(g.ClientSize.Width - textSize.Width - 10, result.Bounds.Y));

            element = new PdfTextElement("Cliente: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + newPart));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Alias) ? " " : incident.Client.Alias, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Dirección: ", subtitle, blue); ;
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Address) ? " " : incident.Client.Address, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Teléfono: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Phone) ? " " : incident.Client.Phone, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Ejecutivo de Cuenta: ", subtitle, blue);
            SizeF repheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - repheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Representative) ? " " : incident.Representative, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("E-mail: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Email) ? " " : incident.Client.Email, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Sistema: ", subtitle, blue);
            SizeF sysheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((g.ClientSize.Width * ClientSecondColumnPos) - sysheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Type.Name) ? " " : incident.Type.Name, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(g.ClientSize.Width * ClientSecondColumnPos + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            if (!String.IsNullOrEmpty(incident.Incidence)) //Just for BAC or any client that has Incidence filled
            {
                element = new PdfTextElement("Incidente: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(incident.Incidence, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            #endregion

            #region General Info
            int leftstartfrom = 70;
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string ProblemHeader = "Datos Generales del Equipo";
            SizeF CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));

            element = new PdfTextElement("Modelo: ", subtitle, blue);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + (nextLine * 4) + 15));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXGenModel) ? " " : ticket.RXGenModel, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("El área se encuentra despejada: ", subtitle, blue);
            SizeF startheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - startheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(ticket.RXGenCleanArea ? "Si" : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + 2.5)), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + (nextLine / 2))));

            element = new PdfTextElement("No Serial: ", subtitle, blue);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXGenSerial) ? " " : ticket.RXGenSerial, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("No Visita Preventiva del año: ", subtitle, blue);
            SizeF finishheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - finishheadersize.Width, result.Bounds.Y));
            switch (ticket.RXGenVisitNumber)
            {
                case Types.SPCSERVTICKET_VISITNUMBER.Visita1:
                    element = new PdfTextElement("Primera", normal, PdfBrushes.Black);
                    break;
                case Types.SPCSERVTICKET_VISITNUMBER.Visita2:
                    element = new PdfTextElement("Segunda", normal, PdfBrushes.Black);
                    break;
                case Types.SPCSERVTICKET_VISITNUMBER.Visita3:
                    element = new PdfTextElement("Tercera", normal, PdfBrushes.Black);
                    break;
                case Types.SPCSERVTICKET_VISITNUMBER.Visita4:
                    element = new PdfTextElement("Cuarta", normal, PdfBrushes.Black);
                    break;
                default:
                    element = new PdfTextElement(" ", normal, PdfBrushes.Black);
                    break;
            }
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

            element = new PdfTextElement("Fabricación: ", subtitle, blue);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXGenCreationDate) ? " " : ticket.RXGenCreationDate, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Estado en el que se encuentra el equipo: ", subtitle, blue);
            SizeF totalheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
            switch (ticket.RXGenHWState)
            {
                case Types.SPCSERVTICKET_HWSTATE.Normal:
                    element = new PdfTextElement("Normal", normal, PdfBrushes.Black);
                    break;
                case Types.SPCSERVTICKET_HWSTATE.Falla:
                    element = new PdfTextElement("Fallando", normal, PdfBrushes.Black);
                    break;
                default:
                    element = new PdfTextElement(" ", normal, PdfBrushes.Black);
                    break;
            }
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            #endregion

            #region Checklist Detalles 
            leftstartfrom = 200;
            float ydetailstart = result.Bounds.Bottom + (nextLine * 2) + 15;
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            ProblemHeader = "Detalles";
            CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));
            float topOfDetails = result.Bounds.Bottom;

            #region Left Column 
            element = new PdfTextElement("Verificación estado cubiertas y etiquetas: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, topOfDetails + (nextLine * 4) + 15));

            imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("checkicon.jpg");
            img = PdfImage.FromStream(imgStream);
            if (ticket.RXMantCheckLabels)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Limpieza general del sistema Int y Ext: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckInOutSystem)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Limpieza de barras infrarrojas: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckIRFences)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación de elementos de control: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckControlElements)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación de relevos de motor: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckEngineControl)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Limpieza/calibración de banda transportadora: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckConveyorBelt)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación arrastre de motor: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckEngineTraction)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación de rodillos de carga y descarga: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckRollers)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación de paradas de emergencia: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckEmergencyStop)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación sistema interno Interlock: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckInterlock)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación monitor de corriente: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckVoltMonitor)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación de circuito de seguridad: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckSecurityCircuit)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación sistema de ventilación: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckConditioningSystem)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación y prueba del sistema operativo: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckOS)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación cono de emisión de RX: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckXRCone)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación señal de línea y módulo detector: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckLineAndDetectionModules)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación parámetros generales de config: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckConfiguration)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación y funcionamiento de teclado: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckKeyboard)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación estado y config de monitor: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckMonitorConfiguration)
                g.DrawImage(img, new RectangleF(leftstartfrom, result.Bounds.Y, 12, 12));

            float nextpart = result.Bounds.Bottom;
            float ydetailend = result.Bounds.Bottom + nextLine;
            #endregion

            #region Right Column
            float rightstartfrom = 240;
            int rightfourthstart = 450;
            int rightthirdstart = 360;
            int rightsecondstart = 320;
            element = new PdfTextElement("Tipo de monitor: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, topOfDetails + (nextLine * 4) + 15));
            string screentype = "Sin definir";
            switch (ticket.RXMantCheckScreenType)
            {
                case Types.SPCSERVTICKET_SCREENTYPE.CRT:
                    screentype = "CRT";
                    break;
                case Types.SPCSERVTICKET_SCREENTYPE.LCD:
                    screentype = "LED";
                    break;
                default:
                    break;
            }
            element = new PdfTextElement(screentype, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Inspección en doble sentido: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckTwoWayMode)
                g.DrawImage(img, new RectangleF(rightfourthstart, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Verificación de cubiertas de radiación: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            if (ticket.RXMantCheckRadiationIndicators)
                g.DrawImage(img, new RectangleF(rightfourthstart, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Estado de cortinas de plomo: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            string leadstate = "No definido";
            switch (ticket.RXMantLeadState)
            {
                case Types.SPCSERVTICKET_LEADSTATE.Ok:
                    leadstate = "Ok";
                    break;
                case Types.SPCSERVTICKET_LEADSTATE.Reg:
                    leadstate = "Regular";
                    break;
                case Types.SPCSERVTICKET_LEADSTATE.Mal:
                    leadstate = "Mal";
                    break;
                default:
                    break;
            }
            element = new PdfTextElement(leadstate, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            #region Voltages SubColumn 
            float voltcenterystart = result.Bounds.Bottom + nextLine;
            float voltcenterxstart = rightstartfrom - 5 + ((g.ClientSize.Width - rightstartfrom) / 2);
            ProblemHeader = "Verf Control Generador 1";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(rightstartfrom, result.Bounds.Bottom + nextLine, g.ClientSize.Width - rightstartfrom, CProblemHeaderSize.Height + 2));
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.White, new PointF(rightstartfrom + ((g.ClientSize.Width - rightstartfrom) / 4) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + 1));
            ProblemHeader = "Verf Control Generador 2";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.White, new PointF(rightthirdstart + ((g.ClientSize.Width - rightstartfrom) / 4) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + 1));
            float currenttop = result.Bounds.Bottom + nextLine + CProblemHeaderSize.Height + 2;
            ProblemHeader = "Rayos X Apagados";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(211, 211, 211)), new RectangleF(rightstartfrom, currenttop, g.ClientSize.Width - rightstartfrom, CProblemHeaderSize.Height + 2));
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(rightstartfrom + ((g.ClientSize.Width - rightstartfrom) / 4) - (CProblemHeaderSize.Width / 2), currenttop + 1));
            ProblemHeader = "Rayos X Apagados";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(rightthirdstart + ((g.ClientSize.Width - rightstartfrom) / 4) - (CProblemHeaderSize.Width / 2), currenttop + 1));
            currenttop = currenttop + CProblemHeaderSize.Height + 2;

            element = new PdfTextElement("Corriente Fil.: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, currenttop + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator1XROffVoltage) ? "- mA" : ticket.RXVoltGenerator1XROffVoltage + " mA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightsecondstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Corriente Fil.: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightthirdstart + 5, currenttop + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator2XROffVoltage) ? "- mA" : ticket.RXVoltGenerator2XROffVoltage + " mA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            currenttop = result.Bounds.Bottom + nextLine;
            ProblemHeader = "Rayos X Encendidos";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(211, 211, 211)), new RectangleF(rightstartfrom, currenttop, g.ClientSize.Width - rightstartfrom, CProblemHeaderSize.Height + 2));
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(rightstartfrom + ((g.ClientSize.Width - rightstartfrom) / 4) - (CProblemHeaderSize.Width / 2), currenttop + 1));
            ProblemHeader = "Rayos X Encendidos";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(rightthirdstart + ((g.ClientSize.Width - rightstartfrom) / 4) - (CProblemHeaderSize.Width / 2), currenttop + 1));
            currenttop = currenttop + CProblemHeaderSize.Height + 2;

            element = new PdfTextElement("Corriente Fil.: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, currenttop + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator1XROffVoltage) ? "- mA" : ticket.RXVoltGenerator1XROffVoltage + " mA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightsecondstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Corriente Fil.: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightthirdstart + 5, currenttop + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator2XROffVoltage) ? "- mA" : ticket.RXVoltGenerator2XROffVoltage + " mA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Corriente ánodo: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator1XROnAnodeVoltage) ? "- uA" : ticket.RXVoltGenerator1XROnAnodeVoltage + " uA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightsecondstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Corriente ánodo: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightthirdstart + 5, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator2XROnAnodeVoltage) ? "- uA" : ticket.RXVoltGenerator2XROnAnodeVoltage + " uA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Alto Voltaje: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator1XROnHighVoltage) ? "- kv3" : ticket.RXVoltGenerator1XROnHighVoltage + " kv3", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightsecondstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Alto Voltaje: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightthirdstart + 5, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGenerator2XROnHighVoltage) ? "- kv3" : ticket.RXVoltGenerator2XROnHighVoltage + " kv3", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(voltcenterxstart, voltcenterystart), new PointF(voltcenterxstart, result.Bounds.Bottom));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(rightstartfrom, result.Bounds.Bottom), new PointF(g.ClientSize.Width, result.Bounds.Bottom));
            #endregion

            element = new PdfTextElement("Voltaje de alimentación acometida: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltInVoltage) ? "- Vac" : ticket.RXVoltInVoltage + " Vac", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Voltaje neutro - Tierra acometida: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltGroundVoltage) ? "- Vac" : ticket.RXVoltGroundVoltage + " Vac", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Tiene UPS: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(ticket.RXVoltCheckHaveUPS ? "Si" : "No", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Capacidad de UPS: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltUPSCapacity) ? "- KVA" : ticket.RXVoltUPSCapacity + " KVA", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Voltaje de salida UPS: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltUPSInVoltage) ? "- Vac" : ticket.RXVoltUPSInVoltage + " Vac", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Voltaje neutro - Tierra de UPS: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXVoltUPSGroundVoltage) ? "- Vac" : ticket.RXVoltUPSGroundVoltage + " Vac", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Transformador de Aislamiento: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(rightstartfrom + 5, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(ticket.RXVoltCheckIsolationTransformator ? "Si" : "No", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightfourthstart, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(rightstartfrom, ydetailstart), new PointF(rightstartfrom, ydetailend));
            #endregion
            #endregion

            #region  Radiación
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, nextpart + (nextLine * 2), g.ClientSize.Width, 15));
            ProblemHeader = "Medición de radiación";
            CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), nextpart + (nextLine * 2)));
            topOfDetails = nextpart;

            ProblemHeader = "Equipo de Medición";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(211, 211, 211)), new RectangleF(0, nextpart + (nextLine * 2) + 15, g.ClientSize.Width, CProblemHeaderSize.Height + 2));
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF((g.ClientSize.Width / 5) - (CProblemHeaderSize.Width / 2), nextpart + (nextLine * 2) + 16));
            ProblemHeader = "Desempeño del equipo";
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), nextpart + (nextLine * 2) + 16));
            ProblemHeader = "Cumplimiento Nivel de radiación";
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(g.ClientSize.Width - (g.ClientSize.Width / 4) - (CProblemHeaderSize.Width / 2), nextpart + (nextLine * 2) + 16));

            element = new PdfTextElement("Marca: " + ((string.IsNullOrEmpty(ticket.RXRadHWTrademark)) ? "No definido" : ticket.RXRadHWTrademark), normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, nextpart + (nextLine * 2) + 16 + CProblemHeaderSize.Height));

            element = new PdfTextElement("Mod: " + ((string.IsNullOrEmpty(ticket.RXRadHWModel)) ? "No definido" : ticket.RXRadHWModel), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(result.Bounds.Right + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Salida del tunel: " + ((string.IsNullOrEmpty(ticket.RXRadTunnelRadOut)) ? "- uSv/h" : ticket.RXRadTunnelRadOut + "uSv/h"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom - 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Fecha Calibración: " + ticket.RXRadHWCalibrationDate.ToString("dd/MM/yyyy"), normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));

            element = new PdfTextElement("Ingreso del tunel: " + ((string.IsNullOrEmpty(ticket.RXRadTunnelRadIn)) ? "- uSv/h" : ticket.RXRadTunnelRadIn + "uSv/h"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom - 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            string radstate = "Desconocido";
            switch (ticket.RXRadRadiationState)
            {
                case Types.SPCSERVTICKET_RADSTATE.Cumple:
                    radstate = "Cumple";
                    break;
                case Types.SPCSERVTICKET_RADSTATE.NoCumple:
                    radstate = "No Cumple";
                    break;
                default:
                    break;
            }
            element = new PdfTextElement(radstate, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightthirdstart + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Fecha Venc Calib: " + ticket.RXRadHWCalibrationDueDate.ToString("dd/MM/yyyy"), normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));

            element = new PdfTextElement("Cost. oper. a 5 cm: " + ((string.IsNullOrEmpty(ticket.RXRadOperatorRad)) ? "- uSv/h" : ticket.RXRadOperatorRad + "uSv/h"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom - 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            #endregion

            #region Software
            int first = 5;
            int second = 75;
            int third = 175;
            int fourth = 230;
            int fifth = 290;
            int sixth = 340;
            int seventh = 420;

            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(leftstartfrom - 10, nextpart + 16), new PointF(leftstartfrom - 10, result.Bounds.Bottom + (nextLine * 2)));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(((g.ClientSize.Width / 3) * 2) - 3, nextpart + 16), new PointF(((g.ClientSize.Width / 3) * 2) - 3, result.Bounds.Bottom + (nextLine * 2)));
            ProblemHeader = "Funciones avanzadas de software opcionales";
            CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));
            topOfDetails = result.Bounds.Bottom + (nextLine * 2) + 15;

            ProblemHeader = "Dongle Serial Físico #: " + ((string.IsNullOrEmpty(ticket.RXSoftPhysicalDongleSerial)) ? " " : ticket.RXSoftPhysicalDongleSerial);
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(211, 211, 211)), new RectangleF(0, topOfDetails, g.ClientSize.Width, CProblemHeaderSize.Height + 2));
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(first, topOfDetails));
            ProblemHeader = "Dongle Serial Software ID: " + ((string.IsNullOrEmpty(ticket.RXSoftSoftwareDongleSerial)) ? " " : ticket.RXSoftSoftwareDongleSerial);
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(third, topOfDetails));
            ProblemHeader = "Tecnología";
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(seventh + 15, topOfDetails));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(third - 5, topOfDetails), new PointF(third - 5, 625));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(seventh - 5, topOfDetails), new PointF(seventh - 5, 625));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(fifth - 5, topOfDetails + 15), new PointF(fifth - 5, 625));
            topOfDetails = topOfDetails + (nextLine * 2) + 15;

            element = new PdfTextElement("SEN: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(first, topOfDetails));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveSEN), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(second, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("HI-TIP: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(first, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveHITIP), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(second, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("X-PLORE: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(first, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveXPLORE), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(second, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("HI-SPOT: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(third, topOfDetails));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveHISPOT), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(fourth, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("IMS: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(third, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveIMS), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(fourth, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("X-ACT: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(third, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveXACT), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(fourth, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("HDA: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(fifth, topOfDetails));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveHDA), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(sixth, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("X-PORT: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(fifth, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveXPORT), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(sixth, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("X-TRAIN: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(fifth, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(PossesionState(ticket.RXSoftHaveXTRAIN), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(sixth, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            string technology = " ";
            switch (ticket.RXSoftTechnology)
            {
                case Types.SPCSERVTICKET_TECHNOLOGY.HiTrax:
                    technology = "Hi Trax";
                    break;
                case Types.SPCSERVTICKET_TECHNOLOGY.SiProx:
                    technology = "Si Prox";
                    break;
                default:
                    break;
            }
            element = new PdfTextElement(technology, normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(seventh, topOfDetails));

            element = new PdfTextElement("Versión Software", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(seventh, result.Bounds.Bottom + nextLine));

            element = new PdfTextElement(string.IsNullOrEmpty(ticket.RXSoftSoftwareVersion) ? "Desconocida" : ticket.RXSoftSoftwareVersion, normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(seventh, result.Bounds.Bottom + nextLine));
            #endregion

            page = document.Pages.Add(); //next page
            g = page.Graphics;

            #region  Calibracion
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 0, g.ClientSize.Width, 15));
            ProblemHeader = "Pruebas de Calibración";
            CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), 0));
            topOfDetails = 15;
            nextpart = topOfDetails;

            ProblemHeader = "Test de Calibración";
            CProblemHeaderSize = subtitle.MeasureString(ProblemHeader);
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(211, 211, 211)), new RectangleF(0, topOfDetails, g.ClientSize.Width, CProblemHeaderSize.Height + 2));
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF((g.ClientSize.Width / 5) - (CProblemHeaderSize.Width / 2), topOfDetails));
            ProblemHeader = "Desempeño del equipo";
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), topOfDetails));
            ProblemHeader = "Cumplimiento Calibración";
            g.DrawString(ProblemHeader, subtitle, PdfBrushes.Black, new PointF(g.ClientSize.Width - (g.ClientSize.Width / 5) - (CProblemHeaderSize.Width / 2), topOfDetails));
            topOfDetails = topOfDetails + CProblemHeaderSize.Height + 2;

            element = new PdfTextElement("Cuña Escalonada: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, topOfDetails));
            if (ticket.RXCalType1)
                g.DrawImage(img, new RectangleF((g.ClientSize.Width / 4) + 10, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Penetración de acero: " + ((string.IsNullOrEmpty(ticket.RXCalSteelPenetration)) ? "- mm" : ticket.RXCalSteelPenetration + "mm"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom - 15, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Maletín de pruebas: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXCalType2)
                g.DrawImage(img, new RectangleF((g.ClientSize.Width / 4) + 10, result.Bounds.Y, 12, 12));

            radstate = "Desconocido";
            switch (ticket.RXCalCalibrationState)
            {
                case Types.SPCSERVTICKET_RADSTATE.Cumple:
                    radstate = "Cumple";
                    break;
                case Types.SPCSERVTICKET_RADSTATE.NoCumple:
                    radstate = "No Cumple";
                    break;
                default:
                    break;
            }
            element = new PdfTextElement(radstate, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(rightthirdstart + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Cuerpo Calib 1 y 2 (XACT): ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXCalType3)
                g.DrawImage(img, new RectangleF((g.ClientSize.Width / 4) + 10, result.Bounds.Y, 12, 12));

            element = new PdfTextElement("Resolución de Alambre: " + ((string.IsNullOrEmpty(ticket.RXCalWireResolution)) ? "- AWG" : ticket.RXCalWireResolution + "AWG"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(leftstartfrom - 15, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Calibración por software: ", normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(5, result.Bounds.Bottom + nextLine));
            if (ticket.RXCalType4)
                g.DrawImage(img, new RectangleF((g.ClientSize.Width / 4) + 10, result.Bounds.Y, 12, 12));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(leftstartfrom - 20, nextpart), new PointF(leftstartfrom - 20, result.Bounds.Bottom + (nextLine * 2)));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(((g.ClientSize.Width / 3) * 2) + 13, nextpart), new PointF(((g.ClientSize.Width / 3) * 2) + 13, result.Bounds.Bottom + (nextLine * 2)));
            #endregion

            #region Observaciones
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            ProblemHeader = "Observaciones y estado final del equipo";
            CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));
            topOfDetails = result.Bounds.Bottom + (nextLine * 2) + 15;

            element = new PdfTextElement(((string.IsNullOrEmpty(ticket.RXGenComments)) ? "Sin observaciones" : ticket.RXGenComments), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(first, topOfDetails, g.ClientSize.Width - 10, 150));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, 15), new PointF(0, topOfDetails + 150));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(g.ClientSize.Width, 15), new PointF(g.ClientSize.Width, topOfDetails + 150));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, topOfDetails + 150), new PointF(g.ClientSize.Width, topOfDetails + 150));
            #endregion

            #region Footer
            int heightCostaRica = 80;
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 400));
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Página {0} de {1}", pageNumber, count);
            compositeField.Bounds = footer.Bounds;

            //string clientsignheader = "Firma del Cliente";
            //string tech1signheader = "Técnico Asignado";
            //if (sign != null)
            //{
            //    PdfImage clientsign = PdfImage.FromStream(sign);
            //    footer.Graphics.DrawImage(clientsign, new RectangleF(100, footer.Height - 160 - 50, 150, 50));
            //    clientsignheader = name;
            //}
            //footer.Graphics.DrawString(clientsignheader, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(clientsignheader).Width / 2), footer.Height - 155, 150, 15));
            //if (!preview)
            //    footer.Graphics.DrawString(identification, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(identification).Width / 2), footer.Height - 145, 150, 15));
            //footer.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, footer.Height - 155), new PointF(225, footer.Height - 155));

            //footer.Graphics.DrawString(ticket.Technicians[0].Name, normal, PdfBrushes.Black, new RectangleF(350 - (normal.MeasureString(ticket.Technicians[0].Name).Width / 2), footer.Height - 165, 200, 15));
            //footer.Graphics.DrawString(tech1signheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(tech1signheader).Width / 2), footer.Height - 155, 150, 15));
            //footer.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, footer.Height - 155), new PointF(425, footer.Height - 155));

            //Draw address image depending of the  country
            if (incident.Client.Country?.Equals("Guatemala")??false)
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            }
            else
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            }
            //Draw the composite field in footer.
            compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            //Add the footer template at the bottom.
            //footer.Graphics.DrawString(notes, notefont, PdfBrushes.Gray, new RectangleF(0, footer.Bounds.Height - heightCostaRica - 10 - 40, footer.Bounds.Width, 100));
            document.Template.Bottom = footer;

            #endregion
            MemoryStream data = new MemoryStream();

            document.Save(data);

            document.Close();
            return data;
        }

        private static string PossesionState(Types.SPCSERVTICKET_POSSESIONSTATE state)
        {
            switch (state)
            {
                case Types.SPCSERVTICKET_POSSESIONSTATE.Undefined:
                    return "Desconocido";
                case Types.SPCSERVTICKET_POSSESIONSTATE.Enabled:
                    return "Tiene";
                case Types.SPCSERVTICKET_POSSESIONSTATE.Disabled:
                    return "Desactivado";
                case Types.SPCSERVTICKET_POSSESIONSTATE.NotHave:
                    return "No tiene";
                default:
                    return "No listado";
            }
        }

        public static async Task<Stream> CreateFinancialServiceTicketReport(ServiceTicket ticket, Incident incident, List<Note> photos, bool preview = true, Stream sign = null, string identification = null, string name = null, bool agree = false)
        {
            //Font configs
            PdfStandardFont logoHeader = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            PdfStandardFont disclaimerfont = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
            PdfStandardFont title = new PdfStandardFont(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
            PdfStandardFont subtitle = new PdfStandardFont(PdfFontFamily.TimesRoman, 10, PdfFontStyle.Bold);
            PdfFont subHeadingFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            PdfStandardFont normal = new PdfStandardFont(PdfFontFamily.TimesRoman, 10);
            PdfStandardFont notefont = new PdfStandardFont(PdfFontFamily.TimesRoman, 7);
            PdfSolidBrush gray = new PdfSolidBrush(new PdfColor(119, 119, 119));
            PdfSolidBrush blue = new PdfSolidBrush(new PdfColor(34, 83, 142));
            int nextLine = 2;
            int newPart = 5;
            int clientdatatab = 55;
            int caseDetailSize = 70;
            float ClientSecondColumnPos = 0.70f;

            PdfDocument document = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;

            #region LogoHeader
            RectangleF bounds = new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement docheader = new PdfPageTemplateElement(bounds);
            docheader.Graphics.DrawString("REPORTE DE SERVICIO", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 10));
            docheader.Graphics.DrawString("RSE-01.02 V1", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 20));
            document.Template.Top = docheader;

            Stream imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("certified.jpg");
            PdfImage img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(g.ClientSize.Width - 200, 0, 35, 35));

            imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("doclogo.jpg");
            img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(0, 0, 250, 35));
            #endregion

            #region Footer
            int heightCostaRica = 80;
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 150));
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Página {0} de {1}", pageNumber, count);
            string notes = "1. ESTIMADO SR. CLIENTE: LE ROGAMOS LEER CON CUIDADO ESTE REPORTE Y ANTES DE FIRMARLO, COMPROBAR CON NUESTRO TECNICO QUE CADA UNO DE LOS SERVICIOS Y REPUESTOS FUERON EFECTUADOS E INSTALADOS RESPECTIVAMENTE.\n" +
                "2.LAS HORAS DE TRABAJO SE INICIAN DESDE QUE EL TECNICO SALE DE NUESTRAS OFICINAS HASTA QUE REGRESE DESPUES DE CONCLUIDO EL TRABAJO.\n" +
                "3.SOLO EL DUEÑO O PERSONAL AUTORIZADO POR EL MISMO PUEDEN FIRMAR ESTE REPORTE DE SERVICIO, NINGUN TRABAJO SE INICIARA HASTA QUE NO SE HALLA RECIBIDO LA RESPECTIVA ORDEN DE COMPRA O SOLICITUD DE SERVICIO";
            compositeField.Bounds = footer.Bounds;

            //Draw address image depending of the  country
            if (incident.Client.Country?.Equals("Guatemala")??false)
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            }
            else
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            }
            //Draw the composite field in footer.
            compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            //Add the footer template at the bottom.
            footer.Graphics.DrawString(notes, notefont, PdfBrushes.Gray, new RectangleF(0, footer.Bounds.Height - heightCostaRica - 10 - 40, footer.Bounds.Width, 100));
            document.Template.Bottom = footer;

            #endregion

            #region ClientDetails
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 55, g.ClientSize.Width, 15));
            PdfTextElement element = new PdfTextElement("Boleta #: " + (string.IsNullOrEmpty(ticket.TicketNumber) ? " " : ticket.TicketNumber), subHeadingFont, PdfBrushes.White);
            PdfLayoutResult result = element.Draw(page, new PointF(10, 55));
            float leftstart = result.Bounds.Top;

            string CaseHeader = string.Format("Caso #: {0}", incident.TicketNumber);
            SizeF CaseHeaderSize = subHeadingFont.MeasureString(CaseHeader);
            g.DrawString(CaseHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CaseHeaderSize.Width / 2), result.Bounds.Y));

            string currentDate = String.Format("Fecha: {0}", ticket.CreationDate.ToString("dd/MM/yyyy"));
            SizeF textSize = subHeadingFont.MeasureString(currentDate);
            g.DrawString(currentDate, subHeadingFont, PdfBrushes.White, new PointF(g.ClientSize.Width - textSize.Width - 10, result.Bounds.Y));

            element = new PdfTextElement("Cliente: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + newPart));
            element = new PdfTextElement((incident.Client != null ? incident.Client.Alias : " "), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Dirección: ", subtitle, blue); ;
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(incident.Client != null ? string.IsNullOrEmpty(incident.Client.Address) ? " " : incident.Client.Address : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Teléfono: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(incident.Client != null ? string.IsNullOrEmpty(incident.Client.Phone) ? " " : incident.Client.Phone : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Ejecutivo de Cuenta: ", subtitle, blue);
            SizeF repheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - repheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Representative) ? " " : incident.Representative, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("E-mail: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(incident.Client != null ? string.IsNullOrEmpty(incident.Client.Email) ? " " : incident.Client.Email : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Sistema: ", subtitle, blue);
            SizeF sysheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * ClientSecondColumnPos) - sysheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(ticket.Type != null ? string.IsNullOrEmpty(ticket.Type.Name) ? " " : ticket.Type.Name : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * ClientSecondColumnPos) + 10, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            if (!String.IsNullOrEmpty(incident.Incidence)) //Just for BAC or any client that has Incidence filled
            {
                element = new PdfTextElement("Incidente: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(incident.Incidence, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            #endregion

            #region CaseDetail
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string ProblemHeader = "Detalle de la Falla";
            SizeF CProblemHeaderSize = subHeadingFont.MeasureString(ProblemHeader);
            g.DrawString(ProblemHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CProblemHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));

            float templateheight = result.Bounds.Bottom + (nextLine * 4) + 15 + caseDetailSize;
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.Description) ? " " : ticket.Description, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(10, result.Bounds.Bottom + (nextLine * 4) + 15, g.ClientSize.Width - 20, caseDetailSize));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, templateheight), new PointF(g.ClientSize.Width, templateheight));

            element = new PdfTextElement("Fecha Solicitud: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, templateheight));
            element = new PdfTextElement(incident.CreatedOn.ToString("dd/MM/yyyy"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Entrada: ", subtitle, blue);
            SizeF startheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - startheadersize.Width, result.Bounds.Y));
            element = new PdfTextElement(ticket.Started.ToString("dd/MM/yyyy hh:mm tt"), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + 2.5)), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + (nextLine / 2))));

            element = new PdfTextElement("Técnico Asignado: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(ticket.Technicians.Count >= 1 ? ticket.Technicians[0].Name : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            DateTime Now = preview ? DateTime.Now : ticket.Finished;
            TimeSpan ellapsed = Now - ticket.Started;

            element = new PdfTextElement("Salida: ", subtitle, blue);
            SizeF finishheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - finishheadersize.Width, result.Bounds.Y));
            if (preview)
                element = new PdfTextElement(String.Format("{0} (Aprox)", Now.ToString("dd/MM/yyyy hh:mm tt")), normal, PdfBrushes.Black);
            else
                element = new PdfTextElement(String.Format("{0}({1})", ticket.Finished.ToString("dd/MM/yyyy hh:mm tt"), ticket.Code), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

            element = new PdfTextElement("Segundo Técnico: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(ticket.Technicians.Count > 1 ? ticket.Technicians[1] == null ? "" : ticket.Technicians[1].Name : " ", normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Tiempo Total: ", subtitle, blue);
            SizeF totalheadersize = element.Font.MeasureString(element.Text);
            result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));

            if (ticket.HadLunch)
                ellapsed = ellapsed.Subtract(new TimeSpan(1, 0, 0));//Subtract an hour.
            string formatted = String.Format("{0} hora{2} y {1} minuto{2} {3}", ellapsed.Hours, ellapsed.Minutes, ellapsed.Minutes == 1 ? "" : "s", preview ? "(Aprox)" : "");
            element = new PdfTextElement(formatted, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            if (ticket.Technicians.Count >= 3)
            {
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));
                element = new PdfTextElement("Tercer Técnico: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[2].Name) ? " " : ticket.Technicians[2].Name, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

                element = new PdfTextElement(ticket.Technicians.Count == 5 ? "Quinto Técnico: " : "Cuarto Técnico: ", subtitle, blue);
                totalheadersize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
                element = new PdfTextElement(ticket.Technicians.Count == 5 ? (ticket.Technicians[4] == null ? " " : string.IsNullOrEmpty(ticket.Technicians[4].Name) ? " " : ticket.Technicians[4].Name) : ticket.Technicians.Count == 4 ? (string.IsNullOrEmpty(ticket.Technicians[3].Name) ? " " : ticket.Technicians[3].Name) : " ", normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            if (ticket.Technicians.Count == 5)
            {
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, (float)(result.Bounds.Bottom + (nextLine / 2))), new PointF(g.ClientSize.Width, (float)(result.Bounds.Bottom + 2.5)));

                element = new PdfTextElement("Cuarto Técnico: ", subtitle, blue);
                result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
                element = new PdfTextElement(string.IsNullOrEmpty(ticket.Technicians[3].Name) ? " " : ticket.Technicians[3].Name, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(100, result.Bounds.Y, g.ClientSize.Width / 2, 100));

                element = new PdfTextElement(" ", subtitle, blue);
                totalheadersize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.70) - totalheadersize.Width, result.Bounds.Y));
                element = new PdfTextElement(" ", normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width * 0.70) + 5, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            }
            #endregion

            #region Workdone
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, result.Bounds.Bottom + (nextLine * 2), g.ClientSize.Width, 15));
            string WorkDoneHeader = "Trabajos Realizados";
            SizeF WorkDoneHeaderSize = subHeadingFont.MeasureString(WorkDoneHeader);
            g.DrawString(WorkDoneHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (WorkDoneHeaderSize.Width / 2), result.Bounds.Bottom + (nextLine * 2)));
            templateheight = result.Bounds.Bottom + (nextLine * 4) + 15;
            element = new PdfTextElement(string.IsNullOrEmpty(ticket.WorkDone) ? " " : ticket.WorkDone, normal, PdfBrushes.Black);
            result = element.Draw(page, new PointF(10, templateheight), g.ClientSize.Width - 20,new PdfLayoutFormat());
            page = result.Page;
            #endregion       
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, leftstart), new PointF(0, templateheight));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(g.ClientSize.Width, leftstart), new PointF(g.ClientSize.Width, templateheight));

            #region ProductList Normal
            PdfGrid grid = new PdfGrid();
            grid.Columns.Add(5);
            List<Tuple<string, string, string, string, string>> cotizarProducts = new List<Tuple<string, string, string, string, string>>();
            double taxes = 0;
            double net = 0;
            foreach (var product in ticket.ProductsUsed)
            {
                if (product.Treatment.Equals(Types.SPCMATERIAL_TREATMENTOPTION.Cotizar))
                {
                    double subtotal = (product.Count * (double)product.UnitPrice);
                    net += subtotal;
                    if (incident.Client.DoesPayTaxes)
                        taxes += subtotal * Config.IVA;
                    Tuple<string, string, string, string, string> cotProduct =
                        new Tuple<string, string, string, string, string>
                        (product.Product.Id, product.Product.Name, product.Count.ToString(), product.UnitPrice == 0 ? "Sin precio registrado" : String.Format("{0}{1:0.00}", ticket.MoneyCurrency.Symbol, product.UnitPrice), product.UnitPrice == 0 ? "Excluído" : String.Format("{0}{1:0.00}", ticket.MoneyCurrency.Symbol, subtotal));
                    cotizarProducts.Add(cotProduct);
                }
                else
                {
                    PdfGridRow arow = grid.Rows.Add();
                    arow.Cells[0].Value = product.Product.Id;
                    arow.Cells[1].Value = product.Product.Name;
                    arow.Cells[2].Value = product.Count.ToString();
                    arow.Cells[3].Value = product.Serials;
                    arow.Cells[4].Value = product.Treatment.ToString();
                }
            }
            PdfGridCellStyle cellStyle = new PdfGridCellStyle();
            cellStyle.Borders.All = PdfPens.White;
            grid.RepeatHeader = true;
            PdfGridRow header = grid.Headers.Add(1)[0];
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(34, 83, 142));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(34, 83, 142));
            headerStyle.TextBrush = PdfBrushes.White;
            headerStyle.Font = subtitle;
            string[] headers = new string[] { "Código", "Producto", "Cantidad", "Número de Serie", "Tratamiento" };
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                header.Cells[i].Value = headers[i];
            }
            header.ApplyStyle(headerStyle);
            cellStyle.Borders.Bottom = new PdfPen(new PdfColor(217, 217, 217), 0.70f);
            cellStyle.Font = normal;
            cellStyle.TextBrush = PdfBrushes.Black;
            foreach (PdfGridRow row in grid.Rows)
            {
                row.ApplyStyle(cellStyle);
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    PdfGridCell cell = row.Cells[i];
                    if (i > 1)
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                    else
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
                }
            }

            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat
            {
                Layout = PdfLayoutType.Paginate,
            };
            int[] sizes = new int[] { 90, 200, 50, 80, 80 };
            for (int i = 0; i < grid.Columns.Count && i < sizes.Length; i++)
                grid.Columns[i].Width = sizes[i];
           
            PdfGridLayoutResult gridResult = grid.Draw(page, new RectangleF(new PointF(0, result.Bounds.Bottom + nextLine), new SizeF(g.ClientSize.Width, 300 )), layoutFormat);
            //PdfGridLayoutResult gridResult = grid.Draw(page, new RectangleF(new PointF(0, templateheight), new SizeF(g.ClientSize.Width, g.ClientSize.Height - 210)), layoutFormat);
            #endregion

            g = gridResult.Page.Graphics;
            page = gridResult.Page;
            float y = gridResult.Bounds.Bottom;
            if (y + 105 > 600) //jump to next page
            {
                page = document.Pages.Add();
                g = page.Graphics;
                y = 0f;
            }

            #region SignNormal
            string clientsignheader = "Firma del Cliente";
            string tech1signheader = "Técnico Asignado";
            if (sign != null)
            {
                PdfImage clientsign = PdfImage.FromStream(sign);
                g.DrawImage(clientsign, new RectangleF(100, y + 10, 150, 50));
                clientsignheader = name;
            }
            g.DrawString(clientsignheader, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(clientsignheader).Width / 2), y + 75, 150, 15));
            if (!preview)
                g.DrawString(identification, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(identification).Width / 2), y + 90, 150, 15));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, y + 75), new PointF(225, y + 75));

            g.DrawString(ticket.Technicians[0].Name, normal, PdfBrushes.Black, new RectangleF(350 - (normal.MeasureString(ticket.Technicians[0].Name).Width / 2), y + 60, 200, 15));
            g.DrawString(tech1signheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(tech1signheader).Width / 2), y + 75, 150, 15));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, y + 75), new PointF(425, y + 75));
            #endregion

            if (cotizarProducts.Count > 0)
            {
                //y += 105;
               // if (y > 500) //jump to next page
                //{
                    page = document.Pages.Add();
                    g = page.Graphics;
                    y = 0f;
                //}

                #region ProductList Financial

                string Cotizacion = "Cotización de Productos";
                SizeF CotizacionSize = title.MeasureString(Cotizacion);
                g.DrawString(Cotizacion, title, new PdfSolidBrush(new PdfColor(34, 83, 142)), new PointF((g.ClientSize.Width / 2) - (CotizacionSize.Width / 2), y));

                grid = new PdfGrid();
                grid.Columns.Add(5);
                grid.RepeatHeader = true;
                header = grid.Headers.Add(1)[0];

                headers = new string[] { "Código", "Producto", "Cant", "Precio Unitario", "Subtotal" };
                for (int i = 0; i < header.Cells.Count; i++)
                {
                    header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                    header.Cells[i].Value = headers[i];
                }
                foreach (var product in cotizarProducts)
                {
                    PdfGridRow arow = grid.Rows.Add();
                    arow.Cells[0].Value = product.Item1;
                    arow.Cells[1].Value = product.Item2;
                    arow.Cells[2].Value = product.Item3;
                    arow.Cells[3].Value = product.Item4;
                    arow.Cells[4].Value = product.Item5;
                }
                header.ApplyStyle(headerStyle);
                foreach (PdfGridRow row in grid.Rows)
                {
                    row.ApplyStyle(cellStyle);
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        PdfGridCell cell = row.Cells[i];
                        if (i > 1)
                            cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Middle);
                        else
                            cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
                    }
                }

                layoutFormat = new PdfGridLayoutFormat
                {
                    Layout = PdfLayoutType.Paginate
                };
                sizes = new int[] { 90, 200, 50, 80, 75 };
                for (int i = 0; i < grid.Columns.Count && i < sizes.Length; i++)
                    grid.Columns[i].Width = sizes[i];

                gridResult = grid.Draw(page, new RectangleF(new PointF(0, y + 20), new SizeF(500, g.ClientSize.Height - 200)), layoutFormat);

                #endregion
                g = gridResult.Page.Graphics;
                page = gridResult.Page;
                y = gridResult.Bounds.Bottom;
                if (y > 395) //jump to next page
                {
                    page = document.Pages.Add();
                    g = page.Graphics;
                    y = 0f;
                }
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(340, y + 15), new PointF(g.ClientSize.Width, y + 15));

                element = new PdfTextElement("Subtotal: ", subtitle, blue);
                SizeF subtotalsize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.85) - subtotalsize.Width, y + 15));
                string subtotalresult = net.ToString(ticket.MoneyCurrency.Symbol + "0.00");
                element = new PdfTextElement(subtotalresult, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width - normal.MeasureString(subtotalresult).Width), result.Bounds.Y, g.ClientSize.Width / 2, 100));

                element = new PdfTextElement("Impuestos: ", subtitle, blue);
                SizeF taxessize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.85) - taxessize.Width, result.Bounds.Bottom));
                string taxesresult = taxes.ToString(ticket.MoneyCurrency.Symbol + "0.00");
                element = new PdfTextElement(taxesresult, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width - normal.MeasureString(taxesresult).Width), result.Bounds.Y, g.ClientSize.Width / 2, 100));

                element = new PdfTextElement("Total: ", subtitle, blue);
                SizeF totalsize = element.Font.MeasureString(element.Text);
                result = element.Draw(page, new PointF((float)(g.ClientSize.Width * 0.85) - totalsize.Width, result.Bounds.Bottom));
                string total = (net + taxes).ToString(ticket.MoneyCurrency.Symbol + "0.00");
                element = new PdfTextElement(total, normal, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF((float)(g.ClientSize.Width - normal.MeasureString(total).Width), result.Bounds.Y, g.ClientSize.Width / 2, 100));

                #region SignCotizacion
                clientsignheader = "Firma del cliente para cotización";
                tech1signheader = "Técnico Asignado";
                if (sign != null)
                {
                    if (agree)
                    {
                        PdfImage clientsign = PdfImage.FromStream(sign);
                        g.DrawImage(clientsign, new RectangleF(100, result.Bounds.Bottom + 10, 150, 50));
                        clientsignheader = name;
                    }
                    else
                        clientsignheader = "Sin aprobación del usuario final";
                }

                g.DrawString(clientsignheader, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(clientsignheader).Width / 2), result.Bounds.Bottom + 75, 150, 15));
                if (!preview && agree)
                    g.DrawString(identification, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(identification).Width / 2), result.Bounds.Bottom + 90, 150, 15));
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, result.Bounds.Bottom + 75), new PointF(225, result.Bounds.Bottom + 75));

                g.DrawString(ticket.Technicians[0].Name, normal, PdfBrushes.Black, new RectangleF(350 - (normal.MeasureString(ticket.Technicians[0].Name).Width / 2), result.Bounds.Bottom + 60, 200, 15));
                g.DrawString(tech1signheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(tech1signheader).Width / 2), result.Bounds.Bottom + 75, 150, 15));
                g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, result.Bounds.Bottom + 75), new PointF(425, result.Bounds.Bottom + 75));
                #endregion

                y = result.Bounds.Bottom + 105;
                if (y + 40 > 500) //jump to next page
                {
                    page = document.Pages.Add();
                    g = page.Graphics;
                    y = 0f;
                }
                string disclaimer = "Esta cotización no incluye mano de obra y materiales, únicamente productos, por lo que debe ser considerada como una cotización parcial.";
                g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, y, g.ClientSize.Width, 40));
                g.DrawRectangle(PdfBrushes.White, new RectangleF(1, y + 1, g.ClientSize.Width - 2, 38));
                element = new PdfTextElement(disclaimer.ToUpper(), disclaimerfont, PdfBrushes.Black);
                result = element.Draw(page, new RectangleF(10, y + 5, g.ClientSize.Width - 20, 30));
            }

            //#region Footer
            //int heightCostaRica = 80;
            //PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 400));
            ////Create page number field.
            //PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            ////Create page count field.
            //PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            ////Add the fields in composite fields.
            //PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Página {0} de {1}", pageNumber, count);
            //string notes = "1. ESTIMADO SR. CLIENTE: LE ROGAMOS LEER CON CUIDADO ESTE REPORTE Y ANTES DE FIRMARLO, COMPROBAR CON NUESTRO TECNICO QUE CADA UNO DE LOS SERVICIOS Y REPUESTOS FUERON EFECTUADOS E INSTALADOS RESPECTIVAMENTE.\n" +
            //    "2.LAS HORAS DE TRABAJO SE INICIAN DESDE QUE EL TECNICO SALE DE NUESTRAS OFICINAS HASTA QUE REGRESE DESPUES DE CONCLUIDO EL TRABAJO.\n" +
            //    "3.SOLO EL DUEÑO O PERSONAL AUTORIZADO POR EL MISMO PUEDEN FIRMAR ESTE REPORTE DE SERVICIO, NINGUN TRABAJO SE INICIARA HASTA QUE NO SE HALLA RECIBIDO LA RESPECTIVA ORDEN DE COMPRA O SOLICITUD DE SERVICIO";
            //compositeField.Bounds = footer.Bounds;

            ////Draw address image depending of the  country
            //if (incident.Client.Country.Equals("Guatemala"))
            //{
            //    imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
            //    img = PdfImage.FromStream(imgStream);
            //    footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            //}
            //if (incident.Client.Country.Equals("Costa Rica"))
            //{
            //    imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
            //    img = PdfImage.FromStream(imgStream);
            //    footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            //}
            ////Draw the composite field in footer.
            //compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            ////Add the footer template at the bottom.
            //footer.Graphics.DrawString(notes, notefont, PdfBrushes.Gray, new RectangleF(0, footer.Bounds.Height - heightCostaRica - 10 - 40, footer.Bounds.Width, 100));
            //document.Template.Bottom = footer;

            //#endregion

            MemoryStream data = new MemoryStream();
            document.Save(data);
            document.Close();
            PdfDocument finaldoc = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            Stream xrdoc;
            PdfLoadedDocument loadedDocument1;
            PdfLoadedDocument loadedDocument2;
            if (ticket.Type != null && ticket.Type.Name.Equals("Rayos X"))
            {
                xrdoc = await GenerateXRayReport(ticket, incident);
                loadedDocument1 = new PdfLoadedDocument(data);
                loadedDocument2 = new PdfLoadedDocument(xrdoc);
                PdfDocumentBase.Merge(finaldoc, loadedDocument1);
                PdfDocumentBase.Merge(finaldoc, loadedDocument2);

                data = new MemoryStream();
                finaldoc.Save(data);
                finaldoc.Close();
            }
            //Add evidence photos to document.
            #region Evidence Photos          
            finaldoc = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            xrdoc = await GeneratePhotoDocument(photos);
            loadedDocument1 = new PdfLoadedDocument(data);
            PdfDocumentBase.Merge(finaldoc, loadedDocument1);
            if (photos.Count > 0)
            {
                loadedDocument2 = new PdfLoadedDocument(xrdoc);
                PdfDocumentBase.Merge(finaldoc, loadedDocument2);
            }
            data = new MemoryStream();
            finaldoc.Save(data);
            finaldoc.Close();
            #endregion
            try //Save to storage
            {
                DependencyService.Get<ISave>().SaveTextAsync("BoletaDeServicio.pdf", "application/pdf", data, preview);                
            }
            catch (Exception)
            {
                throw new FileNotFoundException("No se pudo acceder al archivo BoletaDeServicio.pdf");
            }
            return data;
        }

        public static async Task<Stream> CreateLegalizationReport(ServiceTicket ticket, Incident incident)
        {
            //Font configs
            PdfStandardFont logoHeader = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            PdfStandardFont subtitle = new PdfStandardFont(PdfFontFamily.TimesRoman, 10, PdfFontStyle.Bold);
            PdfFont subHeadingFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            PdfStandardFont normal = new PdfStandardFont(PdfFontFamily.TimesRoman, 10);
            PdfStandardFont notefont = new PdfStandardFont(PdfFontFamily.TimesRoman, 7);
            PdfSolidBrush gray = new PdfSolidBrush(new PdfColor(119, 119, 119));
            PdfSolidBrush blue = new PdfSolidBrush(new PdfColor(34, 83, 142));
            int nextLine = 2;
            int newPart = 5;
            int clientdatatab = 55;
            DateTime Now = DateTime.Now;

            PdfDocument document = new PdfDocument
            {
                Compression = PdfCompressionLevel.Best,
                EnableMemoryOptimization = true
            };
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;

            #region LogoHeader
            RectangleF bounds = new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 70);
            PdfPageTemplateElement docheader = new PdfPageTemplateElement(bounds);
            docheader.Graphics.DrawString("Legalización de equipo", logoHeader, PdfBrushes.Gray, new PointF(page.Graphics.ClientSize.Width - 150, 10));
            document.Template.Top = docheader;

            Stream imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("certified.jpg");
            PdfImage img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(g.ClientSize.Width - 200, 0, 35, 35));

            imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("doclogo.jpg");
            img = PdfImage.FromStream(imgStream);
            docheader.Graphics.DrawImage(img, new RectangleF(0, 0, 250, 35));
            #endregion

            #region ClientDetails
            g.DrawRectangle(new PdfSolidBrush(new PdfColor(34, 83, 142)), new RectangleF(0, 55, g.ClientSize.Width, 15));
            PdfTextElement element = new PdfTextElement("Boleta #: " + ticket.TicketNumber, subHeadingFont, PdfBrushes.White);
            PdfLayoutResult result = element.Draw(page, new PointF(10, 55));
            float leftstart = result.Bounds.Top;

            string CaseHeader = "Caso #: " + incident.TicketNumber;
            SizeF CaseHeaderSize = subHeadingFont.MeasureString(CaseHeader);
            g.DrawString(CaseHeader, subHeadingFont, PdfBrushes.White, new PointF((g.ClientSize.Width / 2) - (CaseHeaderSize.Width / 2), result.Bounds.Y));

            string currentDate = String.Format("Fecha: {0}/{1}/{2}", Now.Day, Now.Month, Now.Year);
            SizeF textSize = subHeadingFont.MeasureString(currentDate);
            g.DrawString(currentDate, subHeadingFont, PdfBrushes.White, new PointF(g.ClientSize.Width - textSize.Width - 10, result.Bounds.Y));

            element = new PdfTextElement("Cliente: ", subtitle, blue);
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + newPart));
            element = new PdfTextElement(string.IsNullOrEmpty(incident.Client.Alias) ? " " : incident.Client.Alias, normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement(String.Format("Técnico{0}: ", ticket.Technicians.Count>1 ? "s" : ""), subtitle, blue); ;
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(String.Format("{0}{1}", ticket.Technicians[0].Name, ticket.Technicians.Count>1 ? " y " + ticket.Technicians[1].Name : ""), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));

            element = new PdfTextElement("Bodega: ", subtitle, blue); ;
            result = element.Draw(page, new PointF(10, result.Bounds.Bottom + nextLine));
            element = new PdfTextElement(String.Format("{0} ", ticket.Technicians[0]?.ProductStorage?.Name), normal, PdfBrushes.Black);
            result = element.Draw(page, new RectangleF(clientdatatab, result.Bounds.Y, g.ClientSize.Width / 2, 100));
            #endregion

            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(0, leftstart), new PointF(0, result.Bounds.Bottom + newPart));
            g.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(g.ClientSize.Width, leftstart), new PointF(g.ClientSize.Width, result.Bounds.Bottom + newPart));

            #region ProductList
            PdfGrid grid = new PdfGrid();
            grid.Columns.Add(4);
            bool atLeastOne = false;
            foreach (var product in ticket.ProductsUsed)
            {
                if (product.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Facturar)
                {
                    PdfGridRow arow = grid.Rows.Add();
                    arow.Cells[0].Value = product.Product.Id;
                    arow.Cells[1].Value = product.Product.Name;
                    arow.Cells[2].Value = product.Count.ToString();
                    arow.Cells[3].Value = product.Serials;
                    atLeastOne = true;
                }
            }
            if (!atLeastOne)
                throw new Exception("No hay productos por facturar");
            PdfGridCellStyle cellStyle = new PdfGridCellStyle();
            cellStyle.Borders.All = PdfPens.White;
            PdfGridRow header = grid.Headers.Add(1)[0];
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(34, 83, 142));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(34, 83, 142));
            headerStyle.TextBrush = PdfBrushes.White;
            headerStyle.Font = subtitle;

            string[] headers = new string[] { "Código", "Producto", "Cantidad", "Número de Serie" };
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                header.Cells[i].Value = headers[i];
            }
            header.ApplyStyle(headerStyle);
            cellStyle.Borders.Bottom = new PdfPen(new PdfColor(217, 217, 217), 0.70f);
            cellStyle.Font = normal;
            cellStyle.TextBrush = PdfBrushes.Black;
            foreach (PdfGridRow row in grid.Rows)
            {
                row.ApplyStyle(cellStyle);
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    PdfGridCell cell = row.Cells[i];
                    if (i > 1)
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                    else
                        cell.StringFormat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
                }
            }
            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat
            {
                Layout = PdfLayoutType.Paginate
            };
            int[] sizes = new int[] { 90, 260, 50, 100 };
            for (int i = 0; i < grid.Columns.Count && i < sizes.Length; i++)
                grid.Columns[i].Width = sizes[i];
            PdfGridLayoutResult gridResult = grid.Draw(page, new RectangleF(new PointF(0, result.Bounds.Bottom + newPart), new SizeF(g.ClientSize.Width, g.ClientSize.Height - 250)), layoutFormat);

            #endregion

            #region Footer
            int heightCostaRica = 80;
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 400));
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(normal, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(normal, PdfBrushes.Black);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(normal, PdfBrushes.Black, "Página {0} de {1}", pageNumber, count)
            {
                Bounds = footer.Bounds
            };
            string Leadersignheader = "Firma Jefatura";
            footer.Graphics.DrawString(ticket.Technicians[0].Name, subtitle, PdfBrushes.Black, new RectangleF(150 - (subtitle.MeasureString(ticket.Technicians[0].Name).Width / 2), footer.Height - 155, 150, 15));
            footer.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, footer.Height - 155), new PointF(225, footer.Height - 155));

            footer.Graphics.DrawString(Leadersignheader, subtitle, PdfBrushes.Black, new RectangleF(350 - (subtitle.MeasureString(Leadersignheader).Width / 2), footer.Height - 155, 150, 15));
            footer.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(275, footer.Height - 155), new PointF(425, footer.Height - 155));

            //Draw address image depending of the  country
            if (incident.Client.Country?.Equals("Guatemala")?? false)
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootergt.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(footer.Bounds.Width / 2 - 150, footer.Bounds.Height - 50, 300, 35));
            }
            else
            {
                imgStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("docfootercr.jpg");
                img = PdfImage.FromStream(imgStream);
                footer.Graphics.DrawImage(img, new RectangleF(0, footer.Bounds.Height - heightCostaRica, footer.Bounds.Width, 65));
            }
            //Draw the composite field in footer.
            compositeField.Draw(footer.Graphics, new PointF(footer.Bounds.Width - normal.MeasureString(compositeField.Text).Width, footer.Bounds.Height - normal.MeasureString(compositeField.Text).Height));
            document.Template.Bottom = footer;

            #endregion

            MemoryStream data = new MemoryStream();

            document.Save(data);

            document.Close();
            try //Save to storage
            {
                DependencyService.Get<ISave>().SaveTextAsync("Legalizacion.pdf", "application/pdf", data, false);             
            }
            catch (Exception)
            {
                throw new FileNotFoundException("No se pudo acceder al archivo Legalización.pdf");
            }
            return data;
        }

        public static async Task<Stream> CreateContract(Contract contract, Stream sign = null)
        {
            #region Document Creation
            PdfDocument document = new PdfDocument();
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            PdfPage page = document.Pages.Add();
            PdfGraphics g = page.Graphics;
            #endregion
            #region Config&Variables
            int fontSize = 15;
            PdfStandardFont TitleBold = new PdfStandardFont(PdfFontFamily.TimesRoman, fontSize - 5, PdfFontStyle.Bold);
            PdfStandardFont Normal = new PdfStandardFont(PdfFontFamily.TimesRoman, fontSize - 5);
            string[] Months = { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Setiembre", "Octubre", "Noviembre", "Diciembre" };
            //float height = 10;
            float smallDivision = 2.5f;
            float bigDivision = 5f;
            DateTime Today = DateTime.Now;
            #endregion
            #region Contract Information
            string TitleMain = String.Format("CONTRATO DE SERVICIOS PARA LA INSTALACIÓN DE MATERIAL Y EQUIPO DE SEGURIDAD PROYECTO: {0}", contract.Cdt.Number);
            string ParagraphMain = String.Format("Entre nosotros, JOSEPH JAMRI portador cedula de residencia 420-0197320-004537, en representación de SPC TELECENTINEL SOCIEDAD ANONIMA, con facultades suficientes para el presente acto, por una parte, y el señor {0} portador de la cédula de identidad número {1}, vecino de {2}  con facultades suficientes para el presente acto, hemos acordado celebrar el presente contrato de SERVICIOS  PARA LA INSTALACIÓN DE TODO EL SISTEMA DE CABLE Y DUCTERIA DEL PROYECTO: {3}, el cual se regirá por las disposiciones contenidas en el Código de Comercio, la normativa vigente aplicable a la materia y las siguientes cláusulas: ", contract.Contractor.Name.ToUpper(), contract.Contractor.Identification, contract.Contractor.Address, contract.Cdt.Number);
            string TitleFirst = "PRIMERA: OBJETO DEL CONTRATO.";
            string ParagraphFirst1 = String.Format("SPC TELECENTINEL SOCIEDAD ANONIMA tiene como parte de su giro comercial la venta de equipo de seguridad electrónica. Por su parte el señor {0} se especializa en Instalación de este tipo de equipos. SPC TELECENTINEL SOCIEDAD ANONIMA únicamente se compromete a hacer entrega del material necesario para la realización del proyecto antes citado y el señor {0}  se compromete a realizar la instalación del referido material y asume cualquier responsabilidad que directa o indirectamente surja en razón de ésta, para la entrega final del trabajo el contratista debe cumplir con los siguientes puntos:", contract.Contractor.Name.ToUpper());
            string[] ListParagraphFirst = {
                "1.   Tuberías, ductos, canasta  y/o canaletas: Su instalación debe cumplir con los la norma del código eléctrico nacional",
                "2.   Cable: Se deben entregar la totalidad del cableado de acuerdo a norma para cableado estructurado, manteniendo la integridad de este.",
                "3.   Marcación de Cable: Todo el cable debe estar debidamente identificado en sus dos puntos",
                "4.   Anclajes: los anclajes de la tubería, ducto o canasta deben cumplir con la norma del Código eléctrico.",
                "5.   Diagramas o Planos: Debe entregar debidamente un diagrama o plano de las rutas de la tubería."
            };
            string ParagraphFirst2 = "CUALQUIER INCUMPLIMIENTO A LA NORMA EN LA INSTALACIÓN DE CABLE O TUBERÍA, ASÍ COMO CUALQUIER OBSERVACIÓN REALIZADA POR EL ENCARGADO DE PROYECTO DEBERÁ SER CORREGIDA SIN COSTO POR EL CONTRATISTA INCLUYENDO LOS MATERIALES NECESARIOS PARA SU ARREGLO.";
            string TitleSecond = "SEGUNDA: RESPONSABILIDAD DE LAS PARTES CONTRATANTES.";
            string ParagraphSecond1 = String.Format("SPC TELECENTINEL SOCIEDAD ANONIMA deberá suplir el material indicado en las mejores condiciones de acuerdo a su naturaleza y uso. El señor {0} deberá aportar los conocimientos técnicos e industriales y la mano de obra y/o personal necesario para la realización de dicha instalación. Una vez hecha la entrega física del material por parte de SPC TELECENTINEL SOCIEDAD ANONIMA, el señor {0} asumirá la responsabilidad absoluta por el transporte, manejo, uso, aplicación, pérdida y/o destrucción parcial o total de los materiales entregados.", contract.Contractor.Name.ToUpper());
            string ParagraphSecond2 = String.Format("El señor {0}  mantendrá el control absoluto sobre los empleados y/o personal que este último utilice para la realización de los servicios aquí indicados. Todos los empleados y/o personal que sean utilizados, contratados o requeridos por el señor {0} para la realización del servicio pactado estarán bajo la dependencia permanente y/o dirección inmediata o delegada de este último. El señor {0} será  el único responsable por el pago de sueldos, aguinaldos, vacaciones, seguros y responsabilidad contra riesgos laborales cargas sociales o cualesquiera otras obligaciones civiles, comerciales o laborales que puedan surgir entre sus empleados y/o personal en razón del presente contrato.", contract.Contractor.Name.ToUpper());
            string ParagraphSecond3 = String.Format("Así mismo, para efectos de nuestras políticas de seguridad, se detalla a continuación, cuál es el personal contratado por {0} y que está debidamente autorizado previa investigación realizada por SPC TELECENTINEL SOCIEDAD ANONIMA, para realizar los trabajos contratados:", contract.Contractor.Name.ToUpper());
            List<string> workers = new List<string>();
            foreach (ContractorWorker worker in contract.Workers)
                workers.Add(String.Format("{0}.   {1}, identificación {2}", workers.Count + 1, worker.Name, worker.Identification));
            string TitleThird = "TERCERA: PRECIO.";
            string ParagraphThird1 = String.Format("Por los servicios acordados en el presente contrato, SPC TELECENTINEL SOCIEDAD ANONIMA le pagará al señor {0}, la suma de {1}. De acuerdo a la siguiente forma de pago seleccionada:", contract.Contractor.Name, contract.AmountTotalFormatted);
            string Payment = contract.Payment.Description;
            string ParagraphThird2 = "El pago no se realizará sobre ningún trabajo que no esté asociado a una orden de compra.";
            string TitleFourth = "CUARTA: GARANTÍA DEL TRABAJO.";
            string ParagraphFourth1 = String.Format("La garantía del trabajo antes descrito es por un periodo de 24 meses después de recibido el mismo por parte del Departamento de Ingeniería y/o Departamento Técnico.  El Sr. {0} se comprometa a atender las averías resultantes por el trabajo realizado dentro del plazo de atención regidos de acuerdo a las necesidades del cliente y sus exigencias aceptando los tiempos de atención que el mismo requiera.", contract.Contractor.Name.ToUpper());
            string TitleFifth = "QUINTA: TIEMPO DE ENTREGA";
            string ParagraphFifth1 = String.Format("El señor {0} dará inicio a este proyecto el día {1} de {2} del {3} y terminara este proyecto el día {4} de {5} del {6} cualquier atraso no justificado del mismo generara un cobro del 2% diario del valor del contrato. El incumplimiento en esta cláusula, acarreará la terminación anticipada de su contrato sin responsabilidad contractual, sanciones penales, así como el obligado pago de daños y perjuicios,  por cuenta del señor {0} a favor de SPC TELECENTINEL S.A.", contract.Contractor.Name.ToUpper(), contract.StartDate.Day, Months[contract.StartDate.Month - 1], contract.StartDate.Year, contract.FinishDate.Day, Months[contract.FinishDate.Month - 1], contract.FinishDate.Year);
            string TitleSixth = "SEXTA: HORARIO.";
            string ParagraphSixth1 = "Los horarios a seguir son los establecidos de acuerdo a la solicitud del cliente. Cualquier requerimiento o extensión del mismo deberá ser solicitado al Departamento de ingeniería de SPC TELECENTINEL S.A.";
            string TitleSeventh = "SÉPTIMA: ACUERDO DE CONFIDENCIALIDAD";
            string ParagraphSeventh1 = String.Format("De conformidad con la Ley de Información No Divulgada No. 7975, publicada en la Gaceta Número 12 del 18 de enero del 2000, el señor {0} y todos sus empleados se comprometen a proteger toda la información no divulgada referente a los secretos comerciales, operacionales e industriales de SPC TELECENTINEL S.A., cédula jurídica número 3-101-201950, así como impedir que información legítimamente bajo su control sea divulgada a terceros, adquirida o utilizada sin consentimiento de SPC TELECENTINEL S.A. por terceros, de manera contraria a los usos comerciales honestos para lo cual se considerará que dicha información deberá contar con las siguientes características:", contract.Contractor.Name.ToUpper());
            string[] ListParagraphSeventh =
            {
                "1.   Sea secreta, en el sentido que no sea, como cuerpo ni en la configuración, operación y reunión precisas de sus componentes, generalmente conocida ni fácilmente accesible para las personas introducidas en los círculos donde normalmente se utiliza este tipo de información.",
                "2.   Tenga un valor comercial por su carácter de secreta.",
                "3.   La información no divulgada se refiere, en especial, a la naturaleza, las características o finalidades de los productos, clientes y los métodos o procesos de operación."
            };
            string ParagraphSeventh2 = String.Format("Para los efectos de esta cláusula, se definirán como formas contrarias a los usos comerciales honestos, entre otras, las prácticas de incumplimiento de contratos, el abuso de confianza, la instigación a la infracción y la adquisición de información no divulgada por terceros que hayan sabido. De esta forma, el señor {0} declara que no revelará, salvo requerida por ley, orden judicial, o por acuerdo escrito entre las partes, (a) cualquier hecho o contenido de discusiones o negociaciones entre las partes, en forma directa, oral o por escrito sobre los hechos y obligaciones presentes y/o futuros relativos a este contrato. b)  Cualquier información secreta o confidencial, concerniente a los productos, procedimientos, instrucciones operacionales, políticas de la empresa contratante y en general cualquier información relativa a SPC TELECENTINEL S.A. El incumplimiento de lo dispuesto por este acuerdo acarreará la terminación anticipada de su contrato sin responsabilidad contractual, sanciones penales, así como el obligado pago de daños y perjuicios,  por cuenta del señor {0} a favor de SPC TELECENTINEL S.A.", contract.Contractor.Name.ToUpper());
            string TitleEighth = "OCTAVA: ACUERDO DE PARTES.";
            string ParagraphEighth1 = String.Format("El presente contrato constituye la totalidad de los acuerdos a los que han llegado las partes, por lo que las mismas se obligan únicamente a lo aquí dispuesto. Leído el presente contrato por las partes y enteradas de su contenido y efectos legales, lo firman de común acuerdo y en fe cumplimiento, en dos tantos que se reputan como originales, uno para cada parte, en la ciudad de San José al ser las {0} horas del día {1} de {2} del {3}.", Today.TimeOfDay.Hours, Today.Day, Months[Today.Month - 1], Today.Year);
            contract = null;
            #endregion
            #region MainSection
            PdfTextElement element = new PdfTextElement(TitleMain, TitleBold, PdfBrushes.Black);
            float pageWidth = document.Pages[0].GetClientSize().Width;
            float width = (pageWidth / 5) * 3;
            PdfLayoutResult result = element.Draw(page, new RectangleF(pageWidth / 5, 0, width, fontSize * 3));
            element = new PdfTextElement(ParagraphMain, Normal, PdfBrushes.Black);
            #endregion
            #region First
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + (bigDivision * 2), pageWidth, fontSize * 7));
            element = new PdfTextElement(TitleFirst, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 1));
            element = new PdfTextElement(ParagraphFirst1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 7));
            foreach (string text in ListParagraphFirst)
            {
                element = new PdfTextElement(text, Normal, PdfBrushes.Black);
                result = element.Draw(result.Page, new RectangleF(14, result.Bounds.Bottom + smallDivision, pageWidth - 14, fontSize * 2));
            }
            element = new PdfTextElement(ParagraphFirst2, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 3));
            #endregion
            #region Second
            element = new PdfTextElement(TitleSecond, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 1));
            element = new PdfTextElement(ParagraphSecond1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 5));
            element = new PdfTextElement(ParagraphSecond2, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 7));
            element = new PdfTextElement(ParagraphSecond3, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 3));
            foreach (string worker in workers)
            {
                element = new PdfTextElement(worker, Normal, PdfBrushes.Black);
                result = element.Draw(result.Page, new RectangleF(14, result.Bounds.Bottom + smallDivision, pageWidth - 14, 30));
            }
            #endregion
            #region Third
            element = new PdfTextElement(TitleThird, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 1));
            element = new PdfTextElement(ParagraphThird1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 5));
            element = new PdfTextElement(Payment, TitleBold, PdfBrushes.Black);
            width = TitleBold.MeasureString(Payment).Width;
            result = element.Draw(result.Page, new RectangleF((pageWidth / 2) - (width / 2), result.Bounds.Bottom + bigDivision, width, fontSize * 2));
            element = new PdfTextElement(ParagraphThird2, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 5));
            #endregion
            #region Fourth
            element = new PdfTextElement(TitleFourth, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize));
            element = new PdfTextElement(ParagraphFourth1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 4));
            #endregion
            #region Fifth
            element = new PdfTextElement(TitleFifth, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize));
            element = new PdfTextElement(ParagraphFifth1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 6));
            #endregion
            #region Sixth
            element = new PdfTextElement(TitleSixth, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize));
            element = new PdfTextElement(ParagraphSixth1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 2));
            #endregion
            #region Seventh
            element = new PdfTextElement(TitleSeventh, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize));
            element = new PdfTextElement(ParagraphSeventh1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 6));
            foreach (string condition in ListParagraphSeventh)
            {
                element = new PdfTextElement(condition, Normal, PdfBrushes.Black);
                result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 4));
            }
            element = new PdfTextElement(ParagraphSeventh2, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + smallDivision, pageWidth, fontSize * 10));
            #endregion
            #region Eighth
            element = new PdfTextElement(TitleEighth, TitleBold, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize));
            element = new PdfTextElement(ParagraphEighth1, Normal, PdfBrushes.Black);
            result = element.Draw(result.Page, new RectangleF(0, result.Bounds.Bottom + bigDivision, pageWidth, fontSize * 3));
            #endregion
            #region Sign
            if (sign != null)
            {
                PdfImage clientsign = PdfImage.FromStream(sign);
                result.Page.Graphics.DrawImage(clientsign, new RectangleF(125, result.Bounds.Bottom + 50, 150, 50));
                result.Page.Graphics.DrawString("Firma Contratista", Normal, PdfBrushes.Black, new RectangleF(150 - (Normal.MeasureString("Firma Contratista").Width / 2), result.Bounds.Bottom + 100 + bigDivision, 150, 15));
                result.Page.Graphics.DrawLine(new PdfPen(new PdfColor(34, 83, 142), 0.70f), new PointF(75, result.Bounds.Bottom + 100), new PointF(225, result.Bounds.Bottom + 100));
            }
            #endregion
            #region Save&Open
            MemoryStream data = new MemoryStream();
            document.Save(data);
            document.Close();
            try //Save to storage
            {
                DependencyService.Get<ISave>().SaveTextAsync("Contrato.pdf", "application/pdf", data, sign == null);              
            }
            catch (Exception)
            {
                throw new FileNotFoundException("No se pudo acceder al archivo Contrato.pdf");
            }
            #endregion
            return data;
        }
    }
}
