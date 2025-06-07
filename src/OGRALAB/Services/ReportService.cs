using OGRALAB.Data;
using OGRALAB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace OGRALAB.Services
{
    public class ReportService : IReportService
    {
        private readonly OgralabDbContext _context;
        public ReportService(OgralabDbContext context)
        {
            _context = context;
        }
        public async Task<List<PatientReportData>> GetPatientsForReportsAsync()
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.Doctor)
                    .Include(p => p.Entity)
                    .Include(p => p.TestResults)
                    .ThenInclude(tr => tr.Test)
                    .Where(p => p.TestResults.Any()) // فقط المرضى الذين لديهم فحوصات
                    .OrderByDescending(p => p.RegistrationDate)
                    .Select(p => new PatientReportData
                    {
                        PatientId = p.Id,
                        PatientCode = p.PatientCode,
                        FullName = p.FullName,
                        Age = p.Age,
                        Gender = p.Gender.ToString(),
                        RegistrationDate = p.RegistrationDate,
                        DoctorName = p.Doctor != null ? p.Doctor.Name : "غير محدد",
                        EntityName = p.Entity != null ? p.Entity.Name : "غير محدد"
                    })
                    .ToListAsync();
                return patients;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ReportService.GetPatientsForReportsAsync");
                throw;
            }
        }
        public async Task<PatientReportData> GetPatientReportDataAsync(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Doctor)
                    .Include(p => p.Entity)
                    .Include(p => p.TestResults)
                    .ThenInclude(tr => tr.Test)
                    .FirstOrDefaultAsync(p => p.Id == patientId);
                if (patient == null)
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                var reportData = new PatientReportData
                {
                    PatientId = patient.Id,
                    PatientCode = patient.PatientCode,
                    FullName = patient.FullName,
                    Age = patient.Age,
                    Gender = patient.Gender.ToString(),
                    RegistrationDate = patient.RegistrationDate,
                    DoctorName = patient.Doctor?.Name ?? "غير محدد",
                    EntityName = patient.Entity?.Name ?? "غير محدد"
                };
                // تحميل جميع الفحوصات المطلوبة للمريض
                var patientTestResults = patient.TestResults.ToList();
                reportData.AvailableTests = patientTestResults.Select(tr => new TestReportItem
                {
                    TestId = tr.TestId,
                    // تم التصحيح هنا: استخدام TestName بدلاً من Name
                    TestName = tr.Test.TestName,
                    TestResult = tr.Result ?? "",
                    ReferenceRange = tr.Test.ReferenceRange ?? "غير محدد",
                    Unit = tr.Test.Unit ?? "",
                    HasResult = !string.IsNullOrEmpty(tr.Result),
                    IsAbnormal = !string.IsNullOrEmpty(tr.Result) && IsResultAbnormal(tr.Result, tr.Test.ReferenceRange ?? ""),
                    IsSelected = false
                }).ToList();
                return reportData;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"ReportService.GetPatientReportDataAsync({patientId})");
                throw;
            }
        }
        public FlowDocument CreateReportDocument(PreparedReportData reportData)
        {
            try
            {
                var document = new FlowDocument();

                // إعدادات المستند الأساسية
                document.FontFamily = new FontFamily("Segoe UI");
                document.FontSize = 12;
                document.TextAlignment = TextAlignment.Right;
                document.FlowDirection = FlowDirection.RightToLeft;
                document.PagePadding = new Thickness(40);
                document.ColumnGap = 0;
                document.Background = Brushes.White;
                // 1. Header Section (مُعد للتخصيص في المرحلة 5)
                var headerSection = CreateHeaderSection(reportData.Template);
                document.Blocks.Add(headerSection);
                // 2. Patient Information Section
                var patientSection = CreatePatientInfoSection(reportData.Patient);
                document.Blocks.Add(patientSection);
                // 3. Tests Results Section
                var testsSection = CreateTestsResultsSection(reportData.SelectedTests, reportData.Template);
                document.Blocks.Add(testsSection);
                // 4. Footer Section
                var footerSection = CreateFooterSection(reportData.Patient, reportData.Template);
                document.Blocks.Add(footerSection);
                return document;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ReportService.CreateReportDocument");
                throw;
            }
        }
        private Section CreateHeaderSection(ReportTemplate template)
        {
            var headerSection = new Section();

            // عنوان المختبر (مُعد للتخصيص في المرحلة 5)
            var headerParagraph = new Paragraph();
            headerParagraph.TextAlignment = TextAlignment.Center;
            headerParagraph.FontSize = 18;
            headerParagraph.FontWeight = FontWeights.Bold;
            headerParagraph.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.HeaderColor));
            headerParagraph.Margin = new Thickness(0, 0, 0, 10);

            // اسم المختبر (افتراضي مؤقت)
            headerParagraph.Inlines.Add(new Run(template.LabName));

            // معلومات إضافية ستُضاف في المرحلة 5
            if (!string.IsNullOrEmpty(template.LabAddress))
            {
                headerParagraph.Inlines.Add(new LineBreak());
                headerParagraph.Inlines.Add(new Run(template.LabAddress) { FontSize = 12, FontWeight = FontWeights.Normal });
            }

            if (!string.IsNullOrEmpty(template.LabPhone))
            {
                headerParagraph.Inlines.Add(new LineBreak());
                headerParagraph.Inlines.Add(new Run($"هاتف: {template.LabPhone}") { FontSize = 12, FontWeight = FontWeights.Normal });
            }
            headerSection.Blocks.Add(headerParagraph);
            // عنوان التقرير
            var titleParagraph = new Paragraph();
            titleParagraph.TextAlignment = TextAlignment.Center;
            titleParagraph.FontSize = 16;
            titleParagraph.FontWeight = FontWeights.Bold;
            titleParagraph.Margin = new Thickness(0, 10, 0, 0);
            titleParagraph.Inlines.Add(new Run(template.ReportTitle));
            headerSection.Blocks.Add(titleParagraph);
            // خط فاصل
            var dividerParagraph = new Paragraph();
            dividerParagraph.TextAlignment = TextAlignment.Center;
            dividerParagraph.Margin = new Thickness(0, 10, 0, 20);
            dividerParagraph.Inlines.Add(new Run("═══════════════════════════════════════════════════════════════════"));
            headerSection.Blocks.Add(dividerParagraph);
            return headerSection;
        }
        private Section CreatePatientInfoSection(PatientReportData patientData)
        {
            var patientSection = new Section();

            var patientInfoTable = new Table();
            patientInfoTable.CellSpacing = 0;
            patientInfoTable.BorderBrush = Brushes.Black;
            patientInfoTable.BorderThickness = new Thickness(1);
            patientInfoTable.Margin = new Thickness(0, 0, 0, 20);
            // إعداد الأعمدة
            patientInfoTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            patientInfoTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            var patientTableRowGroup = new TableRowGroup();

            // صف 1: اسم المريض والتاريخ
            var row1 = new TableRow();
            var nameCell = new TableCell(new Paragraph(new Run($"المريض: {patientData.FullName}") { FontWeight = FontWeights.SemiBold }));
            var dateCell = new TableCell(new Paragraph(new Run($"التاريخ: {patientData.RegistrationDate:yyyy/MM/dd}") { FontWeight = FontWeights.SemiBold }));

            nameCell.Padding = new Thickness(10);
            dateCell.Padding = new Thickness(10);
            nameCell.BorderBrush = Brushes.Black;
            nameCell.BorderThickness = new Thickness(0, 0, 1, 1);
            dateCell.BorderBrush = Brushes.Black;
            dateCell.BorderThickness = new Thickness(0, 0, 0, 1);

            row1.Cells.Add(nameCell);
            row1.Cells.Add(dateCell);
            patientTableRowGroup.Rows.Add(row1);
            // صف 2: رقم المريض والعمر
            var row2 = new TableRow();
            var codeCell = new TableCell(new Paragraph(new Run($"الرقم: {patientData.PatientCode}") { FontWeight = FontWeights.SemiBold }));
            var ageCell = new TableCell(new Paragraph(new Run($"العمر: {patientData.Age} سنة") { FontWeight = FontWeights.SemiBold }));

            codeCell.Padding = new Thickness(10);
            ageCell.Padding = new Thickness(10);
            codeCell.BorderBrush = Brushes.Black;
            codeCell.BorderThickness = new Thickness(0, 0, 1, 1);
            ageCell.BorderBrush = Brushes.Black;
            ageCell.BorderThickness = new Thickness(0, 0, 0, 1);

            row2.Cells.Add(codeCell);
            row2.Cells.Add(ageCell);
            patientTableRowGroup.Rows.Add(row2);
            // صف 3: الجنس والطبيب
            var row3 = new TableRow();
            var genderCell = new TableCell(new Paragraph(new Run($"الجنس: {patientData.Gender}") { FontWeight = FontWeights.SemiBold }));
            var doctorCell = new TableCell(new Paragraph(new Run($"الطبيب: {patientData.DoctorName}") { FontWeight = FontWeights.SemiBold }));

            genderCell.Padding = new Thickness(10);
            doctorCell.Padding = new Thickness(10);
            genderCell.BorderBrush = Brushes.Black;
            genderCell.BorderThickness = new Thickness(0, 0, 1, 0);
            doctorCell.BorderBrush = Brushes.Black;
            doctorCell.BorderThickness = new Thickness(0, 0, 0, 0);

            row3.Cells.Add(genderCell);
            row3.Cells.Add(doctorCell);
            patientTableRowGroup.Rows.Add(row3);
            patientInfoTable.RowGroups.Add(patientTableRowGroup);
            patientSection.Blocks.Add(patientInfoTable);

            return patientSection;
        }
        private Section CreateTestsResultsSection(List<TestReportItem> selectedTests, ReportTemplate template)
        {
            var testsSection = new Section();

            // عنوان القسم
            var testsTitleParagraph = new Paragraph();
            testsTitleParagraph.FontSize = 14;
            testsTitleParagraph.FontWeight = FontWeights.Bold;
            testsTitleParagraph.Margin = new Thickness(0, 0, 0, 15);
            testsTitleParagraph.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.HeaderColor));
            testsTitleParagraph.Inlines.Add(new Run("🔬 الفحوصات والنتائج:"));
            testsSection.Blocks.Add(testsTitleParagraph);
            // جدول النتائج
            var resultsTable = new Table();
            resultsTable.CellSpacing = 0;
            resultsTable.BorderBrush = Brushes.Black;
            resultsTable.BorderThickness = new Thickness(1);
            resultsTable.Margin = new Thickness(0, 0, 0, 20);
            // إعداد الأعمدة
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(3, GridUnitType.Star) }); // اسم الفحص
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // النتيجة
            if (template.ShowReferenceRanges)
            {
                resultsTable.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // المدى المرجعي
            }
            var resultsRowGroup = new TableRowGroup();
            // صف العناوين
            var headerRow = new TableRow();
            headerRow.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));

            var testNameHeader = new TableCell(new Paragraph(new Run("اسم الفحص") { FontWeight = FontWeights.Bold }));
            var resultHeader = new TableCell(new Paragraph(new Run("النتيجة") { FontWeight = FontWeights.Bold }));

            testNameHeader.Padding = new Thickness(10);
            resultHeader.Padding = new Thickness(10);
            testNameHeader.BorderBrush = Brushes.Black;
            testNameHeader.BorderThickness = new Thickness(0, 0, 1, 1);
            resultHeader.BorderBrush = Brushes.Black;

            headerRow.Cells.Add(testNameHeader);
            headerRow.Cells.Add(resultHeader);

            if (template.ShowReferenceRanges)
            {
                var rangeHeader = new TableCell(new Paragraph(new Run("المدى المرجعي") { FontWeight = FontWeights.Bold }));
                rangeHeader.Padding = new Thickness(10);
                rangeHeader.BorderBrush = Brushes.Black;
                rangeHeader.BorderThickness = new Thickness(0, 0, 0, 1);
                resultHeader.BorderThickness = new Thickness(0, 0, 1, 1);
                headerRow.Cells.Add(rangeHeader);
            }
            else
            {
                resultHeader.BorderThickness = new Thickness(0, 0, 0, 1);
            }

            resultsRowGroup.Rows.Add(headerRow);
            // صفوف النتائج
            foreach (var test in selectedTests.Where(t => t.HasResult))
            {
                var resultRow = new TableRow();

                var testNameCell = new TableCell(new Paragraph(new Run(test.TestName)));

                var resultParagraph = new Paragraph();
                var resultRun = new Run(test.FormattedResult);

                // تمييز النتائج غير الطبيعية
                if (template.HighlightAbnormalResults && test.IsAbnormal)
                {
                    resultRun.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.AbnormalResultColor));
                    resultRun.FontWeight = FontWeights.Bold;
                    resultParagraph.Inlines.Add(resultRun);
                    resultParagraph.Inlines.Add(new Run(" ⚠️") { Foreground = Brushes.Orange });
                }
                else
                {
                    resultRun.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.NormalResultColor));
                    resultParagraph.Inlines.Add(resultRun);
                }

                var resultCell = new TableCell(resultParagraph);

                testNameCell.Padding = new Thickness(10);
                resultCell.Padding = new Thickness(10);
                testNameCell.BorderBrush = Brushes.Black;
                testNameCell.BorderThickness = new Thickness(0, 0, 1, 1);
                resultCell.BorderBrush = Brushes.Black;

                resultRow.Cells.Add(testNameCell);
                resultRow.Cells.Add(resultCell);

                if (template.ShowReferenceRanges)
                {
                    var rangeCell = new TableCell(new Paragraph(new Run(test.ReferenceRange)));
                    rangeCell.Padding = new Thickness(10);
                    rangeCell.BorderBrush = Brushes.Black;
                    rangeCell.BorderThickness = new Thickness(0, 0, 0, 1);
                    resultCell.BorderThickness = new Thickness(0, 0, 1, 1);
                    resultRow.Cells.Add(rangeCell);
                }
                else
                {
                    resultCell.BorderThickness = new Thickness(0, 0, 0, 1);
                }

                resultsRowGroup.Rows.Add(resultRow);
            }
            resultsTable.RowGroups.Add(resultsRowGroup);
            testsSection.Blocks.Add(resultsTable);
            return testsSection;
        }
        private Section CreateFooterSection(PatientReportData patientData, ReportTemplate template)
        {
            var footerSection = new Section();

            // خط فاصل
            var dividerParagraph = new Paragraph();
            dividerParagraph.TextAlignment = TextAlignment.Center;
            dividerParagraph.Margin = new Thickness(0, 30, 0, 20);
            dividerParagraph.Inlines.Add(new Run("═══════════════════════════════════════════════════════════════════"));
            footerSection.Blocks.Add(dividerParagraph);

            // جدول التوقيع
            var signatureTable = new Table();
            signatureTable.CellSpacing = 0;
            signatureTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            signatureTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            var signatureRowGroup = new TableRowGroup();

            // صف التوقيع والطبيب
            var signatureRow = new TableRow();
            var signatureCell = new TableCell(new Paragraph(new Run("التوقيع: ________________")));
            var doctorCell = new TableCell(new Paragraph(new Run($"الطبيب المسؤول: {patientData.DoctorName}")));

            signatureCell.TextAlignment = TextAlignment.Left;
            doctorCell.TextAlignment = TextAlignment.Right;

            signatureRow.Cells.Add(signatureCell);
            signatureRow.Cells.Add(doctorCell);
            signatureRowGroup.Rows.Add(signatureRow);

            // صف التاريخ والختم
            var dateRow = new TableRow();
            var dateCell = new TableCell(new Paragraph(new Run($"التاريخ: {DateTime.Now:yyyy/MM/dd}")));
            var stampCell = new TableCell(new Paragraph(new Run("الختم: [    ]")));

            dateCell.TextAlignment = TextAlignment.Left;
            stampCell.TextAlignment = TextAlignment.Right;

            dateRow.Cells.Add(dateCell);
            dateRow.Cells.Add(stampCell);
            signatureRowGroup.Rows.Add(dateRow);

            signatureTable.RowGroups.Add(signatureRowGroup);

            // تم التصحيح هنا: إضافة الجدول مباشرة إلى القسم
            footerSection.Blocks.Add(signatureTable);

            // نص تذييل مخصص (سيتم تخصيصه في المرحلة 5)
            if (!string.IsNullOrEmpty(template.FooterText))
            {
                var customFooterParagraph = new Paragraph();
                customFooterParagraph.TextAlignment = TextAlignment.Center;
                customFooterParagraph.FontSize = 10;
                customFooterParagraph.Margin = new Thickness(0, 10, 0, 0);
                customFooterParagraph.Inlines.Add(new Run(template.FooterText));
                footerSection.Blocks.Add(customFooterParagraph);
            }
            return footerSection;
        }
        public ReportTemplate GetDefaultReportTemplate()
        {
            return new ReportTemplate
            {
                LabName = "OGRALAB Medical Laboratory",
                LabAddress = "", // سيتم تخصيصه في المرحلة 5
                LabPhone = "", // سيتم تخصيصه في المرحلة 5
                LabEmail = "", // سيتم تخصيصه في المرحلة 5
                LabLicense = "", // سيتم تخصيصه في المرحلة 5
                LogoPath = "", // سيتم تخصيصه في المرحلة 5
                ReportTitle = "تقرير نتائج التحاليل الطبية",
                FooterText = "", // سيتم تخصيصه في المرحلة 5
                ShowLogo = false, // مُعطل مؤقتاً حتى المرحلة 5
                ShowLabInfo = true,
                ShowReferenceRanges = true,
                HighlightAbnormalResults = true,
                HeaderColor = "#2C5282",
                AbnormalResultColor = "#E53E3E",
                NormalResultColor = "#2D3748"
            };
        }
        public bool IsResultAbnormal(string result, string referenceRange)
        {
            if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(referenceRange))
                return false;
            try
            {
                // تحليل النتائج الرقمية
                if (double.TryParse(result, out double numericResult))
                {
                    // تحليل المدى المرجعي (مثال: "10-20")
                    if (referenceRange.Contains("-"))
                    {
                        var rangeParts = referenceRange.Split('-');
                        if (rangeParts.Length == 2 &&
                            double.TryParse(rangeParts[0].Trim(), out double min) &&
                            double.TryParse(rangeParts[1].Trim(), out double max))
                        {
                            return numericResult < min || numericResult > max;
                        }
                    }
                    // تحليل المدى المرجعي (مثال: "< 5")
                    else if (referenceRange.StartsWith("<"))
                    {
                        var maxValue = referenceRange.Substring(1).Trim();
                        if (double.TryParse(maxValue, out double max))
                        {
                            return numericResult >= max;
                        }
                    }
                    // تحليل المدى المرجعي (مثال: "> 10")
                    else if (referenceRange.StartsWith(">"))
                    {
                        var minValue = referenceRange.Substring(1).Trim();
                        if (double.TryParse(minValue, out double min))
                        {
                            return numericResult <= min;
                        }
                    }
                    // تحليل المدى المرجعي (مثال: "من 10 إلى 20")
                    else if (referenceRange.Contains("إلى") || referenceRange.Contains("الى"))
                    {
                        var rangeParts = referenceRange.Split(new[] { "إلى", "الى" }, StringSplitOptions.RemoveEmptyEntries);
                        if (rangeParts.Length == 2 &&
                            double.TryParse(rangeParts[0].Replace("من", "").Trim(), out double min) &&
                            double.TryParse(rangeParts[1].Trim(), out double max))
                        {
                            return numericResult < min || numericResult > max;
                        }
                    }
                }
                // تحليل النتائج النصية (مثال: "سالب/موجب")
                var resultLower = result.ToLower();
                var rangeLower = referenceRange.ToLower();

                if (rangeLower.Contains("سالب") || rangeLower.Contains("negative"))
                {
                    return resultLower.Contains("موجب") || resultLower.Contains("positive");
                }
                else if (rangeLower.Contains("موجب") || rangeLower.Contains("positive"))
                {
                    return resultLower.Contains("سالب") || resultLower.Contains("negative");
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"ReportService.IsResultAbnormal({result}, {referenceRange})");
                return false;
            }
        }
        public string FormatTestResult(TestReportItem test)
        {
            return test.FormattedResult;
        }
        public PreparedReportData PrepareReportData(PatientReportData patientData, List<TestReportItem> selectedTests)
        {
            return new PreparedReportData
            {
                Patient = patientData,
                SelectedTests = selectedTests.Where(t => t.IsSelected && t.HasResult).ToList(),
                Template = GetDefaultReportTemplate(),
                GeneratedOn = DateTime.Now,
                GeneratedBy = "OGRALAB System"
            };
        }
    }
}