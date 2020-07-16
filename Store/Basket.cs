using System.Windows;

namespace Store
{
    class Basket : DependencyObject
    {
        public static readonly DependencyProperty UserIDProperty =
            DependencyProperty.Register("UserID", typeof(int), typeof(Basket));

        public int UserID
        {
            get
            {
                return (int)GetValue(UserIDProperty);
            }
            set
            {
                SetValue(UserIDProperty, value);
            }
        }
        public int GoodID { get; set; }
        public int GoodCount { get; set; }
        public Model OwnerModel { get; set; }
    }
}
