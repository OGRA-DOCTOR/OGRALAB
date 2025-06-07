using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
namespace OGRALAB.Services
{
    /// <summary>
    /// خدمة الطباعة المتقدمة
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// طباعة المستند مع إظهار مربع حوار الطباعة
        /// </summary>
        Task<bool> PrintDocumentAsync(FlowDocument document);

        /// <summary>
        /// إنشاء مربع حوار الطباعة مع الإعدادات المُحسنة
        /// </summary>
        PrintDialog CreatePrintDialog();

        /// <summary>
        /// فحص توفر طابعة افتراضية
        /// </summary>
        bool IsDefaultPrinterAvailable();

        /// <summary>
        /// جلب قائمة الطابعات المتاحة
        /// </summary>
        List<string> GetAvailablePrinters();

        /// <summary>
        /// تحضير المستند للطباعة (تحسين التخطيط)
        /// </summary>
        FlowDocument PrepareDocumentForPrint(FlowDocument originalDocument, PrintDialog printDialog);
    }
}