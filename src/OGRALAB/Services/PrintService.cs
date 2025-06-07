using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
namespace OGRALAB.Services
{
    public class PrintService : IPrintService
    {
        public async Task<bool> PrintDocumentAsync(FlowDocument document)
        {
            try
            {
                var printDialog = CreatePrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    // تحضير المستند للطباعة
                    var printDocument = PrepareDocumentForPrint(document, printDialog);

                    // طباعة المستند
                    IDocumentPaginatorSource idpSource = printDocument;
                    await Task.Run(() => printDialog.PrintDocument(idpSource.DocumentPaginator, "OGRALAB Medical Report"));

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PrintService.PrintDocumentAsync");
                MessageBox.Show($"حدث خطأ أثناء الطباعة: {ex.Message}", "خطأ في الطباعة",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public PrintDialog CreatePrintDialog()
        {
            var printDialog = new PrintDialog();

            try
            {
                // إعداد خيارات الطباعة المحسنة
                printDialog.UserPageRangeEnabled = true;
                printDialog.PageRangeSelection = PageRangeSelection.AllPages;

                // تحديد الطابعة الافتراضية إن أمكن
                if (IsDefaultPrinterAvailable())
                {
                    using (var printServer = new LocalPrintServer())
                    {
                        printDialog.PrintQueue = printServer.DefaultPrintQueue;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PrintService.CreatePrintDialog");
            }

            return printDialog;
        }
        public bool IsDefaultPrinterAvailable()
        {
            try
            {
                using (var printServer = new LocalPrintServer())
                {
                    var defaultQueue = printServer.DefaultPrintQueue;
                    return defaultQueue != null && !defaultQueue.IsOffline && !defaultQueue.IsPaused;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PrintService.IsDefaultPrinterAvailable");
                return false;
            }
        }
        public List<string> GetAvailablePrinters()
        {
            try
            {
                using (var printServer = new LocalPrintServer())
                {
                    return printServer.GetPrintQueues()
                        .Where(pq => !pq.IsOffline && !pq.IsPaused)
                        .Select(pq => pq.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PrintService.GetAvailablePrinters");
                return new List<string>();
            }
        }
        public FlowDocument PrepareDocumentForPrint(FlowDocument originalDocument, PrintDialog printDialog)
        {
            try
            {
                // نسخ المستند لتجنب تعديل الأصل
                var printDocument = CloneDocument(originalDocument);

                // إعداد خصائص الطباعة
                printDocument.PageHeight = printDialog.PrintableAreaHeight;
                printDocument.PageWidth = printDialog.PrintableAreaWidth;
                printDocument.PagePadding = new Thickness(50); // هوامش مناسبة
                printDocument.ColumnGap = 0;
                printDocument.ColumnWidth = printDialog.PrintableAreaWidth - 100;

                // تحسين الخطوط للطباعة
                printDocument.FontSize = 11; // حجم أصغر قليلاً للطباعة

                return printDocument;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PrintService.PrepareDocumentForPrint");
                return originalDocument; // إرجاع المستند الأصلي في حالة الخطأ
            }
        }
        private FlowDocument CloneDocument(FlowDocument original)
        {
            try
            {
                // طريقة فعالة لنسخ FlowDocument
                using (var stream = new MemoryStream())
                {
                    var range = new TextRange(original.ContentStart, original.ContentEnd);
                    range.Save(stream, DataFormats.XamlPackage);

                    var copy = new FlowDocument();
                    var copyRange = new TextRange(copy.ContentStart, copy.ContentEnd);
                    stream.Position = 0;
                    copyRange.Load(stream, DataFormats.XamlPackage);

                    // نسخ الخصائص الأساسية
                    copy.FontFamily = original.FontFamily;
                    copy.FontSize = original.FontSize;
                    copy.FontWeight = original.FontWeight;
                    copy.TextAlignment = original.TextAlignment;
                    copy.FlowDirection = original.FlowDirection;
                    copy.Background = original.Background;
                    copy.Foreground = original.Foreground;

                    return copy;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PrintService.CloneDocument");
                return original; // في حالة فشل النسخ، إرجاع المستند الأصلي
            }
        }
    }
}