using CRUDtest.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HDS.Extension.HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Web;

namespace CRUDtest.Controllers
{
    public class CapaPlanController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CapaPlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var capaPlans = _context.CapaPlans.ToList();
            return View(capaPlans);
        }
     
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CapaPlan capaPlan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(capaPlan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(capaPlan);
        }

        public async Task<IActionResult> GenerateWord()
        {
            var capaPlans = _context.CapaPlans.ToList();
            //create File
            using (MemoryStream mem = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument
                    .Create(mem, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    Paragraph title = new Paragraph(new ParagraphProperties(new Justification()
                    {
                        Val = JustificationValues.Center
                    }));

                    Run titleRun = new Run(new RunProperties(new Bold()));
                    titleRun.AppendChild(new Text("KẾ HOẠCH KHẮC PHỤC, NGĂN NGỪA (CAPA)"));
                    title.AppendChild(titleRun);
                    body.AppendChild(title);

                    Table summary = new Table();
                    TableProperties summaryTblProperties = new TableProperties(
                       new TableBorders(
                           new TopBorder { Val = BorderValues.Single, Size = 12 },
                           new BottomBorder { Val = BorderValues.Single, Size = 12 },
                           new LeftBorder { Val = BorderValues.Single, Size = 12 },
                           new RightBorder { Val = BorderValues.Single, Size = 12 },
                           new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                           new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                       )
                   );
                    summary.AppendChild(summaryTblProperties);
                    TableRow summaryRow = new TableRow();
                    summaryRow.Append(
                       CreateTableCell("Số CAPA (STT/Năm): " + (capaPlans.FirstOrDefault()?.SoCAPA ?? ""), colspan: 4),
                        CreateTableCell("Đơn vị có sự PKH: " + (capaPlans.FirstOrDefault()?.DonViCoSuPKH ?? ""), colspan: 4),
                        CreateTableCell("Mã đơn vị: " + (capaPlans.FirstOrDefault()?.MaDonVi ?? ""), colspan: 4)

                    );
                    summary.Append(summaryRow);
                    body.AppendChild(summary);
                    //thêm bảng vào Document
                    Table table = new Table();

                    TableProperties tableProperties = new TableProperties(
                            new TableStyle { Val = "TableGrid" },
                            new TableBorders(
                                new TopBorder { Val = BorderValues.Single, Size = 6 },
                                new BottomBorder { Val = BorderValues.Single, Size = 6 },
                                new LeftBorder { Val = BorderValues.Single, Size = 6 },
                                new RightBorder { Val = BorderValues.Single, Size = 6 },
                                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                                new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                                )
                        );
                    table.AppendChild(tableProperties);


                    //phần đầu
                    TableRow headerRow = new TableRow();
                    headerRow.Append(
                       CreateTableCell("STT", true),
                        CreateTableCell("Số Phiếu CAR", true),
                        CreateTableCell("Ngày phát hành Phiếu", true),
                        CreateTableCell("Mô tả Sự KPH", true),
                        CreateTableCell("Phân tích nguyên nhân gốc", true),
                        CreateTableCell("Khắc phục", true),
                        CreateTableCell("Hành động ngăn ngừa", true),
                        CreateTableCell("Ngày hoàn tất dự kiến", true),
                        CreateTableCell("Xem xét của CPL/QAD/HSE", true),
                        CreateTableCell("Ngày xem xét", true));
                    table.AppendChild(headerRow);

                    //thêm dữ liệu vào cột

                    int index = 1;
                    foreach (var capa in capaPlans)
                    {
                        TableRow dataRow = new TableRow();
                        dataRow.Append(
                            CreateTableCell(index.ToString()),
                            CreateTableCell(capa.SoPhieuCAR),
                            CreateTableCell(capa.NgayPhatHanh.ToString("dd/MM/yyyy")),
                            CreateTableCell(capa.MoTaSuKPH),
                            CreateTableCell(capa.PhanTichNguyenNhanGoc),
                            CreateTableCell(capa.KhacPhuc),
                            CreateTableCell(capa.HanhDongNguaNgua),
                            CreateTableCell(capa.NgayHoanTatDuKien?.ToString("dd/MM/yyyy")),
                            CreateTableCell(capa.XemXetCPLQADHSE),
                            CreateTableCell(capa.NgayXemXet?.ToString("dd/MM/yyyy")));
                        table.AppendChild(dataRow);
                        index++;
                    }

                    TableRow footerRow = new TableRow();
                    footerRow.Append(CreateTableCell("Trưởng đơn vị", true));
                    table.Append(footerRow);

                    body.AppendChild(table);
                    Paragraph signature = new Paragraph(new Run(new Text("Người cập nhật")));
                    body.AppendChild(signature);
                }
                mem.Position = 0;
                return File(mem.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "CAPAPlan.docx");
            }
        }
        private TableCell CreateTableCell(string text, bool isHeader = false, int colspan = 1)
        {
            TableCell cell = new TableCell();
            TableCellProperties cellProperties = new TableCellProperties();

            // Cấu hình chiều rộng cố định cho mỗi ô
            if (!isHeader)
            {
                cellProperties.Append(new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = "2400" });
            }

            //  thuộc tính ghép cột nếu cần
            if (colspan > 1)
            {
                cellProperties.Append(new GridSpan { Val = colspan });
            }

            cell.Append(cellProperties);

            // Tạo và cấu hình đoạn văn bản trong ô
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            run.Append(new Text(text));

            // định dạng in đậm cho tiêu đề
            if (isHeader)
            {
                run.RunProperties = new RunProperties(new Bold());
                paragraph.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
            }

            paragraph.Append(run);
            cell.Append(paragraph);

            return cell;
        }
        
        [HttpPost]
        public IActionResult UploadWord(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(stream, true))
                {
                    Body body = wordDocument.MainDocumentPart.Document.Body;
                    string documentText = string.Join("\n", body.Elements<Paragraph>().Select(p => p.InnerText));

                    TempData["DocumentText"] = documentText;
                }
            }

            return RedirectToAction(nameof(EditWord));
        }
        public IActionResult EditWord()
        {
            var capaPlans = _context.CapaPlans.ToList();

            // Chuyển đổi nội dung Word thành HTML để hiển thị trên giao diện
            string documentHtml = GenerateWordContentAsHtml(capaPlans);

            // Kiểm tra nếu documentHtml có giá trị null hoặc rỗng, thì trả về thông báo lỗi cho người dùng
            if (string.IsNullOrEmpty(documentHtml))
            {
                ViewBag.ErrorMessage = "Không thể đọc nội dung từ file Word.";
                return View();
            }

            ViewBag.DocumentHtml = documentHtml;
            return View();
        }
        [HttpPost]
        public IActionResult SaveEditedWord(string editedHtml)
        {
            if (string.IsNullOrEmpty(editedHtml))
            {
                TempData["ErrorMessage"] = "Không có nội dung để lưu.";
                return RedirectToAction(nameof(EditWord));
            }
            using (MemoryStream mem = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Thêm tiêu đề như trong hàm GenerateWord
                    Paragraph title = new Paragraph(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
                    Run titleRun = new Run(new RunProperties(new Bold()));
                    titleRun.AppendChild(new Text("KẾ HOẠCH KHẮC PHỤC, NGĂN NGỪA (CAPA)"));
                    title.AppendChild(titleRun);
                    body.AppendChild(title);

                    // Thêm bảng tóm tắt đầu tiên giống như trong GenerateWord
                    Table summary = new Table();
                    TableProperties summaryTblProperties = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 12 },
                            new BottomBorder { Val = BorderValues.Single, Size = 12 },
                            new LeftBorder { Val = BorderValues.Single, Size = 12 },
                            new RightBorder { Val = BorderValues.Single, Size = 12 },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                        )
                    );
                    summary.AppendChild(summaryTblProperties);
                    TableRow summaryRow = new TableRow();
                    summaryRow.Append(
                        CreateTableCell("Số CAPA (STT/Năm): CAPA001", colspan: 4),
                        CreateTableCell("Đơn vị có sự PKH: Đơn vị A", colspan: 4),
                        CreateTableCell("Mã đơn vị: M01", colspan: 4)
                    );
                    summary.Append(summaryRow);
                    body.AppendChild(summary);

                    // Tạo bảng cho dữ liệu được chỉnh sửa
                    Table table = new Table();
                    TableProperties tableProperties = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 6 },
                            new BottomBorder { Val = BorderValues.Single, Size = 6 },
                            new LeftBorder { Val = BorderValues.Single, Size = 6 },
                            new RightBorder { Val = BorderValues.Single, Size = 6 },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                        )
                    );
                    table.AppendChild(tableProperties);
                   /* table.Append(CreateHeaderRow());*/  // Gọi phương thức tạo hàng tiêu đề tương tự như GenerateWord

                    // Thêm các hàng từ HTML đã chỉnh sửa
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(editedHtml);
                    HtmlNodeCollection rows = htmlDoc.DocumentNode.SelectNodes("//tr");
                    if (rows != null)
                    {
                        int index = 1;
                        foreach (var row in rows)
                        {
                            TableRow dataRow = new TableRow();
                            HtmlNodeCollection cells = row.SelectNodes("td");
                            if (cells != null)
                            {
                                dataRow.Append(CreateTableCell(index.ToString()));  // Thêm số thứ tự

                                foreach (var cell in cells)
                                {
                                    string cellText = HtmlEntity.DeEntitize(cell.InnerText.Trim());
                                    dataRow.Append(CreateTableCell(cellText));
                                }
                                table.AppendChild(dataRow);
                                index++;
                            }
                        }
                    }

                    body.AppendChild(table);

                    // Thêm chữ ký
                    Paragraph signature = new Paragraph(new Run(new Text("Người cập nhật")));
                    body.AppendChild(signature);
                }

                mem.Position = 0;
                return File(mem.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "CAPAPlans_Edited.docx");
            }
        }

     
        private string GenerateWordContentAsHtml(IEnumerable<CapaPlan> capaPlans)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Tạo nội dung với định dạng HTML
                    string htmlContent = "<h3 style='text-align:center;'><strong>KẾ HOẠCH KHẮC PHỤC, NGĂN NGỪA (CAPA)</strong></h3>";
                    htmlContent += "<table border='1' style='width:100%; border-collapse: collapse;'>";

                    htmlContent += "<tr>";
                    htmlContent += $"<td colspan='4'>Số CAPA (STT/Năm): {capaPlans.FirstOrDefault()?.SoCAPA}</td>";
                    htmlContent += $"<td colspan='4'>Đơn vị có sự PKH: {capaPlans.FirstOrDefault()?.DonViCoSuPKH}</td>";
                    htmlContent += $"<td colspan='4'>Mã đơn vị: {capaPlans.FirstOrDefault()?.MaDonVi}</td>";
                    htmlContent += "</tr>";

                    htmlContent += "<tr><th>STT</th><th>Số Phiếu CAR</th><th>Ngày phát hành Phiếu</th><th>Mô tả Sự KPH</th><th>Phân tích nguyên nhân gốc</th><th>Khắc phục</th><th>Hành động ngăn ngừa</th><th>Ngày hoàn tất dự kiến</th><th>Xem xét của CPL/QAD/HSE</th><th>Ngày xem xét</th></tr>";

                    int index = 1;
                    foreach (var capaPlan in capaPlans)
                    {
                        htmlContent += "<tr>";
                        htmlContent += $"<td>{index}</td>";
                        htmlContent += $"<td>{capaPlan.SoPhieuCAR}</td>";
                        htmlContent += $"<td>{capaPlan.NgayPhatHanh:dd/MM/yyyy}</td>";
                        htmlContent += $"<td>{capaPlan.MoTaSuKPH}</td>";
                        htmlContent += $"<td>{capaPlan.PhanTichNguyenNhanGoc}</td>";
                        htmlContent += $"<td>{capaPlan.KhacPhuc}</td>";
                        htmlContent += $"<td>{capaPlan.HanhDongNguaNgua}</td>";
                        htmlContent += $"<td>{capaPlan.NgayHoanTatDuKien:dd/MM/yyyy}</td>";
                        htmlContent += $"<td>{capaPlan.XemXetCPLQADHSE}</td>";
                        htmlContent += $"<td>{capaPlan.NgayXemXet:dd/MM/yyyy}</td>";
                        htmlContent += "</tr>";
                        index++;
                    }

                    htmlContent += "</table>";

                    return htmlContent;
                }
            }
        }

        /*private static string StripHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }*/


    }
}
