using System.ComponentModel.DataAnnotations;
namespace OGRALAB.Enums
{
    /// <summary>
    /// أنواع مقارنة العمر للمعدل الطبيعي
    /// يدعم أنواع مقارنة مختلفة لتحقيق مرونة كاملة
    /// </summary>
    public enum AgeOperator
    {
        /// <summary>
        /// نطاق عمري من قيمة إلى قيمة
        /// مثال: من 18 إلى 65 سنة
        /// </summary>
        [Display(Name = "نطاق (من - إلى)")]
        Range = 0,

        /// <summary>
        /// أكبر من العمر المحدد
        /// مثال: أكبر من 65 سنة
        /// </summary>
        [Display(Name = "أكبر من")]
        GreaterThan = 1,

        /// <summary>
        /// أقل من العمر المحدد
        /// مثال: أقل من 18 سنة
        /// </summary>
        [Display(Name = "أقل من")]
        LessThan = 2,

        /// <summary>
        /// يساوي العمر المحدد بالضبط
        /// مثال: 25 سنة
        /// </summary>
        [Display(Name = "يساوي")]
        Equals = 3,

        /// <summary>
        /// أكبر من أو يساوي العمر المحدد
        /// مثال: 18 سنة فأكثر
        /// </summary>
        [Display(Name = "أكبر من أو يساوي")]
        GreaterThanOrEqual = 4,

        /// <summary>
        /// أقل من أو يساوي العمر المحدد
        /// مثال: 17 سنة فأقل
        /// </summary>
        [Display(Name = "أقل من أو يساوي")]
        LessThanOrEqual = 5
    }
}