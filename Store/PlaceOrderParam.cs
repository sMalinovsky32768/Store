using System.Collections;
using System.Windows;

namespace Store
{
    class PlaceOrderParam : DependencyObject
    {
        public static readonly DependencyProperty OwnerModelProperty =
            DependencyProperty.Register("OwnerModel", typeof(Model), typeof(PlaceOrderParam));
        public static readonly DependencyProperty SelectedBasketItemsProperty =
            DependencyProperty.Register("SelectedBasketItems", typeof(IList), typeof(PlaceOrderParam));

        public Model OwnerModel
        {
            get
            {
                return (Model)GetValue(OwnerModelProperty);
            }
            set
            {
                SetValue(OwnerModelProperty, value);
            }
        }
        public IList SelectedBasketItems
        {
            get
            {
                return (IList)GetValue(SelectedBasketItemsProperty);
            }
            set
            {
                SetValue(SelectedBasketItemsProperty, value);
            }
        }
    }
}
