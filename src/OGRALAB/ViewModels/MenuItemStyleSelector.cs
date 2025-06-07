using System.Windows;
using System.Windows.Controls;

namespace OGRALAB.ViewModels
{
    public class MenuItemStyleSelector : StyleSelector
    {
        public Style? NormalStyle { get; set; }
        public Style? SelectedStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is NavigationItem navigationItem)
            {
                if (navigationItem.IsSelected)
                {
                    return SelectedStyle ?? base.SelectStyle(item, container);
                }
            }
            return NormalStyle ?? base.SelectStyle(item, container);
        }
    }
}