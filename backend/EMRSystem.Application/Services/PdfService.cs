// PdfService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EMRSystem.Application.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateMedicalRecordPdfAsync(int recordId)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Prescriptions)
                    .ThenInclude(p => p.PrescriptionDetails)
                .Include(m => m.LabTests)
                .FirstOrDefaultAsync(m => m.Id == recordId);

            if (record == null)
                throw new Exception("Medical record not found");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, record));
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Trang ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                    page.Watermark(watermark =>
                    {
                        var user = GetCurrentUserFromContext(); // Lấy thông tin user hiện tại
                        watermark.Text(text =>
                        {
                            text.Span($"Accessed by: {user.FullName} ({user.Email}) on {DateTime.Now:yyyy-MM-dd HH:mm:ss} from IP: {GetCurrentIpAddress()}").FontSize(8).FontColor(Colors.Grey.Lighten2);
                            text.Rotate(-45);
                        });
                    });
                                    });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("BỆNH VIỆN XYZ").FontSize(16).Bold();
                    column.Item().Text("Địa chỉ: 123 Đường ABC, TP.HCM");
                    column.Item().Text("Điện thoại: (028) 1234 5678");
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("BỆNH ÁN ĐIỆN TỬ").FontSize(16).Bold();
                    column.Item().AlignRight().Text($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });
        }

        private void ComposeContent(IContainer container, MedicalRecord record)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(10);

                // Patient Information
                column.Item().Element(c => ComposePatientInfo(c, record));

                // Visit Information
                column.Item().Element(c => ComposeVisitInfo(c, record));

                // Vital Signs
                column.Item().Element(c => ComposeVitalSigns(c, record));

                // Diagnosis and Treatment
                column.Item().Element(c => ComposeDiagnosisTreatment(c, record));

                // Prescriptions
                if (record.Prescriptions?.Any() == true)
                {
                    column.Item().Element(c => ComposePrescriptions(c, record));
                }

                // Lab Tests
                if (record.LabTests?.Any() == true)
                {
                    column.Item().Element(c => ComposeLabTests(c, record));
                }

                // Signatures
                column.Item().PaddingTop(20).Element(ComposeSignatures);
            });
        }

        private void ComposePatientInfo(IContainer container, MedicalRecord record)
        {
            container.Column(column =>
            {
                column.Item().Text("THÔNG TIN BỆNH NHÂN").Bold().FontSize(13);
                column.Item().LineHorizontal(1);
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Họ tên: {record.Patient.FullName}");
                    row.RelativeItem().Text($"Ngày sinh: {record.Patient.DateOfBirth:dd/MM/yyyy}");
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Giới tính: {record.Patient.Gender}");
                    row.RelativeItem().Text($"SĐT: {record.Patient.PhoneNumber}");
                });
                column.Item().Text($"Địa chỉ: {record.Patient.Address}");
                if (!string.IsNullOrEmpty(record.Patient.Allergies))
                {
                    column.Item().Text($"Dị ứng: {record.Patient.Allergies}").FontColor(Colors.Red.Medium);
                }
            });
        }

        private void ComposeVisitInfo(IContainer container, MedicalRecord record)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text("THÔNG TIN KHÁM").Bold().FontSize(13);
                column.Item().LineHorizontal(1);
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Ngày khám: {record.VisitDate:dd/MM/yyyy HH:mm}");
                    row.RelativeItem().Text($"Bác sĩ: {record.Doctor.FullName}");
                });
                column.Item().Text($"Lý do khám: {record.ChiefComplaint}");
                if (!string.IsNullOrEmpty(record.PresentIllness))
                {
                    column.Item().Text($"Bệnh sử: {record.PresentIllness}");
                }
            });
        }

        private void ComposeVitalSigns(IContainer container, MedicalRecord record)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text("SINH HIỆU").Bold().FontSize(13);
                column.Item().LineHorizontal(1);
                column.Item().PaddingTop(5).Row(row =>
                {
                    if (record.Temperature.HasValue)
                        row.RelativeItem().Text($"Nhiệt độ: {record.Temperature}°C");
                    if (!string.IsNullOrEmpty(record.BloodPressure))
                        row.RelativeItem().Text($"Huyết áp: {record.BloodPressure} mmHg");
                    if (record.HeartRate.HasValue)
                        row.RelativeItem().Text($"Mạch: {record.HeartRate} bpm");
                });
                column.Item().Row(row =>
                {
                    if (record.Weight.HasValue)
                        row.RelativeItem().Text($"Cân nặng: {record.Weight} kg");
                    if (record.Height.HasValue)
                        row.RelativeItem().Text($"Chiều cao: {record.Height} cm");
                });
            });
        }

        private void ComposeDiagnosisTreatment(IContainer container, MedicalRecord record)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text("CHẨN ĐOÁN VÀ ĐIỀU TRỊ").Bold().FontSize(13);
                column.Item().LineHorizontal(1);
                
                if (!string.IsNullOrEmpty(record.PhysicalExamination))
                {
                    column.Item().PaddingTop(5).Text("Khám lâm sàng:").Bold();
                    column.Item().Text(record.PhysicalExamination);
                }

                if (!string.IsNullOrEmpty(record.Diagnosis))
                {
                    column.Item().PaddingTop(5).Text("Chẩn đoán:").Bold();
                    column.Item().Text(record.Diagnosis);
                }

                if (!string.IsNullOrEmpty(record.Treatment))
                {
                    column.Item().PaddingTop(5).Text("Điều trị:").Bold();
                    column.Item().Text(record.Treatment);
                }
            });
        }

        private void ComposePrescriptions(IContainer container, MedicalRecord record)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text("ĐƠN THUỐC").Bold().FontSize(13);
                column.Item().LineHorizontal(1);

                foreach (var prescription in record.Prescriptions)
                {
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("STT").Bold();
                            header.Cell().Text("Tên thuốc").Bold();
                            header.Cell().Text("Liều lượng").Bold();
                            header.Cell().Text("Cách dùng").Bold();
                            header.Cell().Text("Số ngày").Bold();
                        });

                        int index = 1;
                        foreach (var detail in prescription.PrescriptionDetails)
                        {
                            table.Cell().Text(index++.ToString());
                            table.Cell().Text(detail.MedicineName);
                            table.Cell().Text(detail.Dosage);
                            table.Cell().Text(detail.Frequency);
                            table.Cell().Text(detail.Duration.ToString());
                        }
                    });
                }
            });
        }

        private void ComposeLabTests(IContainer container, MedicalRecord record)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text("XÉT NGHIỆM").Bold().FontSize(13);
                column.Item().LineHorizontal(1);

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Tên xét nghiệm").Bold();
                        header.Cell().Text("Ngày chỉ định").Bold();
                        header.Cell().Text("Kết quả").Bold();
                        header.Cell().Text("Trạng thái").Bold();
                    });

                    foreach (var test in record.LabTests)
                    {
                        table.Cell().Text(test.TestName);
                        table.Cell().Text(test.OrderDate.ToString("dd/MM/yyyy"));
                        table.Cell().Text(test.Result ?? "Chưa có");
                        table.Cell().Text(test.Status);
                    }
                });
            });
        }

        private void ComposeSignatures(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignCenter().Text("Bệnh nhân/Người nhà").Bold();
                    column.Item().PaddingTop(50).AlignCenter().Text("(Ký, ghi rõ họ tên)");
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignCenter().Text("Bác sĩ điều trị").Bold();
                    column.Item().PaddingTop(50).AlignCenter().Text("(Ký, ghi rõ họ tên)");
                });
            });
        }

        public async Task<byte[]> GeneratePrescriptionPdfAsync(int prescriptionId)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Patient)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Doctor)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);

            if (prescription == null)
                throw new Exception("Prescription not found");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Header
                        column.Item().AlignCenter().Text("ĐơN THUỐC").FontSize(16).Bold();
                        column.Item().LineHorizontal(1);

                        // Patient info
                        column.Item().Text($"Họ tên: {prescription.MedicalRecord.Patient.FullName}");
                        column.Item().Text($"Tuổi: {DateTime.Now.Year - prescription.MedicalRecord.Patient.DateOfBirth.Year}");
                        column.Item().Text($"Ngày kê đơn: {prescription.PrescriptionDate:dd/MM/yyyy}");
                        column.Item().Text($"Bác sĩ: {prescription.MedicalRecord.Doctor.FullName}");

                        // Medicines
                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("STT").Bold();
                                header.Cell().Text("Tên thuốc - Cách dùng").Bold();
                                header.Cell().AlignRight().Text("Số lượng").Bold();
                            });

                            int index = 1;
                            foreach (var detail in prescription.PrescriptionDetails)
                            {
                                table.Cell().Text(index++.ToString());
                                table.Cell().Column(c =>
                                {
                                    c.Item().Text(detail.MedicineName).Bold();
                                    c.Item().Text($"{detail.Dosage} - {detail.Frequency}");
                                    if (!string.IsNullOrEmpty(detail.Instructions))
                                        c.Item().Text(detail.Instructions).FontSize(9).Italic();
                                });
                                table.Cell().AlignRight().Text($"{detail.Duration} ngày");
                            }
                        });

                        // Notes
                        if (!string.IsNullOrEmpty(prescription.Notes))
                        {
                            column.Item().PaddingTop(10).Text("Lưu ý:").Bold();
                            column.Item().Text(prescription.Notes);
                        }

                        // Signature
                        column.Item().PaddingTop(20).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}");
                            c.Item().Text("Bác sĩ kê đơn").Bold();
                            c.Item().PaddingTop(40).Text(prescription.MedicalRecord.Doctor.FullName);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}