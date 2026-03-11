using ClosedXML.Excel;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Hospital.Services
{
    public class ReportService : IReportService
    {
        private readonly IHospitalInfoService _hospitals;
        private readonly IDoctorService _doctors;
        private readonly IRoomService _rooms;

        public ReportService(IHospitalInfoService h, IDoctorService d, IRoomService r)
        {
            _hospitals = h; _doctors = d; _rooms = r;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ── HOSPITALS PDF ─────────────────────────────────────────────────────

        public byte[] GenerateHospitalsPdf()
        {
            var items = _hospitals.GetAll(1, 1000).Items;
            return Document.Create(c => c.Page(page =>
            {
                page.Size(PageSizes.A4); page.Margin(30);
                page.Header().Text("Hospitals Report").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(cd => { cd.ConstantColumn(30); cd.RelativeColumn(3); cd.RelativeColumn(3); cd.RelativeColumn(2); });
                    t.Header(h => { foreach (var col in new[] { "#", "Name", "Address", "Phone" }) h.Cell().Background(Colors.Blue.Darken3).Padding(5).Text(col).FontColor(Colors.White).Bold(); });
                    int i = 1;
                    foreach (var item in items)
                    {
                        var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                        t.Cell().Background(bg).Padding(4).Text(i++.ToString());
                        t.Cell().Background(bg).Padding(4).Text(item.Name ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.Address ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.PhoneNumber ?? "");
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages(); });
            })).GeneratePdf();
        }

        // ── HOSPITALS EXCEL ───────────────────────────────────────────────────

        public byte[] GenerateHospitalsExcel()
        {
            var items = _hospitals.GetAll(1, 1000).Items;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Hospitals");
            WriteHeader(ws, new[] { "#", "Name", "Address", "Phone", "Email" }, "#0f4c81");
            int row = 3, i = 1;
            foreach (var item in items)
            {
                ws.Cell(row, 1).Value = i++;
                ws.Cell(row, 2).Value = item.Name ?? "";
                ws.Cell(row, 3).Value = item.Address ?? "";
                ws.Cell(row, 4).Value = item.PhoneNumber ?? "";
                ws.Cell(row, 5).Value = item.Email ?? "";
                if (row % 2 == 0) ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        // ── ROOMS PDF ─────────────────────────────────────────────────────────

        public byte[] GenerateRoomsPdf()
        {
            var items = _rooms.GetAll(1, 1000).Items;
            return Document.Create(c => c.Page(page =>
            {
                page.Size(PageSizes.A4); page.Margin(30);
                page.Header().Text("Rooms Report").FontSize(20).Bold().FontColor(Colors.Orange.Darken3);
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(cd => { cd.ConstantColumn(30); cd.RelativeColumn(2); cd.RelativeColumn(2); cd.RelativeColumn(2); cd.RelativeColumn(2); });
                    t.Header(h => { foreach (var col in new[] { "#", "Room No", "Type", "Hospital", "Status" }) h.Cell().Background(Colors.Orange.Darken3).Padding(5).Text(col).FontColor(Colors.White).Bold(); });
                    int i = 1;
                    foreach (var item in items)
                    {
                        var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                        t.Cell().Background(bg).Padding(4).Text(i++.ToString());
                        t.Cell().Background(bg).Padding(4).Text(item.RoomNumber ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.RoomType ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.HospitalName ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.IsAvailable ? "Available" : "Occupied");
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages(); });
            })).GeneratePdf();
        }

        // ── ROOMS EXCEL ───────────────────────────────────────────────────────

        public byte[] GenerateRoomsExcel()
        {
            var items = _rooms.GetAll(1, 1000).Items;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Rooms");
            WriteHeader(ws, new[] { "#", "Room No", "Type", "Hospital", "Status" }, "#c85a00");
            int row = 3, i = 1;
            foreach (var item in items)
            {
                ws.Cell(row, 1).Value = i++;
                ws.Cell(row, 2).Value = item.RoomNumber ?? "";
                ws.Cell(row, 3).Value = item.RoomType ?? "";
                ws.Cell(row, 4).Value = item.HospitalName ?? "";
                ws.Cell(row, 5).Value = item.IsAvailable ? "Available" : "Occupied";
                if (row % 2 == 0) ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        // ── DOCTORS PDF ───────────────────────────────────────────────────────

        public byte[] GenerateDoctorsPdf()
        {
            var items = _doctors.GetAll(1, 1000).Items;
            return Document.Create(c => c.Page(page =>
            {
                page.Size(PageSizes.A4); page.Margin(30);
                page.Header().Text("Doctors / Timings Report").FontSize(20).Bold().FontColor(Colors.Green.Darken3);
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(cd => { cd.ConstantColumn(30); cd.RelativeColumn(3); cd.RelativeColumn(3); cd.RelativeColumn(2); });
                    t.Header(h => { foreach (var col in new[] { "#", "Doctor", "Specialty", "Hospital" }) h.Cell().Background(Colors.Green.Darken3).Padding(5).Text(col).FontColor(Colors.White).Bold(); });
                    int i = 1;
                    foreach (var item in items)
                    {
                        var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                        t.Cell().Background(bg).Padding(4).Text(i++.ToString());
                        t.Cell().Background(bg).Padding(4).Text(item.DoctorName ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.Specialty ?? "");
                        t.Cell().Background(bg).Padding(4).Text(item.HospitalName ?? "");
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages(); });
            })).GeneratePdf();
        }

        // ── DOCTORS EXCEL ─────────────────────────────────────────────────────

        public byte[] GenerateDoctorsExcel()
        {
            var items = _doctors.GetAll(1, 1000).Items;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Doctors");
            WriteHeader(ws, new[] { "#", "Name", "Specialty", "Hospital", "Phone" }, "#1a7c4a");
            int row = 3, i = 1;
            foreach (var item in items)
            {
                ws.Cell(row, 1).Value = i++;
                ws.Cell(row, 2).Value = item.DoctorName ?? "";
                ws.Cell(row, 3).Value = item.Specialty ?? "";
                ws.Cell(row, 4).Value = item.HospitalName ?? "";
                ws.Cell(row, 5).Value = item.PhoneNumber ?? "";
                if (row % 2 == 0) ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        // ── HELPERS ───────────────────────────────────────────────────────────

        private static void WriteHeader(IXLWorksheet ws, string[] headers, string hexColor)
        {
            ws.Cell(1, 1).Value = ws.Name + " Report";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cell(2, c + 1).Value = headers[c];
                ws.Cell(2, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(hexColor);
                ws.Cell(2, c + 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(2, c + 1).Style.Font.Bold = true;
            }
        }

        private static byte[] ToBytes(XLWorkbook wb)
        {
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}