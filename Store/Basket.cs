using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Store
{
    class Basket : DependencyObject
    {
        public static readonly DependencyProperty UserIDProperty =
            DependencyProperty.Register("UserID", typeof(int), typeof(Basket));
        public static readonly DependencyProperty GoodIDProperty =
            DependencyProperty.Register("GoodID", typeof(int), typeof(Basket));
        public static readonly DependencyProperty GoodCountProperty =
            DependencyProperty.Register("GoodCount", typeof(int), typeof(Basket));

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
        public int GoodID
        {
            get
            {
                return (int)GetValue(GoodIDProperty);
            }
            set
            {
                SetValue(GoodIDProperty, value);
            }
        }
        public int GoodCount
        {
            get
            {
                return (int)GetValue(GoodCountProperty);
            }
            set
            {
                SetValue(GoodCountProperty, value);
            }
        }
        public Model OwnerModel { get; set; }
    }
}
