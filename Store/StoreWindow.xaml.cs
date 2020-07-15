using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Store
{
    public partial class StoreWindow : Window
    {
        private int UserID { get; }
        private DataSet Set { get; set; }
        internal Model StoreModel { get; private set; }

        public StoreWindow(int id)
        {
            InitializeComponent();
            StoreModel = Resources["StoreModel"] as Model;
            StoreModel.UserID = id;
            StoreModel.UpdateTotalValue();
            UserID = id;
            Set = new DataSet();
            Set.Tables.Add("Goods");
            new SqlDataAdapter("Select * from GetGoods()", ConfigurationManager.
                ConnectionStrings["DefaultConnection"].ConnectionString).Fill(Set, "Goods");
            goodsGrid.ItemsSource = Set.Tables["Goods"].DefaultView;
            goodsGrid.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (goodsGrid.ItemContainerGenerator.Status ==
                System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                var containers = goodsGrid.Items.Cast<object>().Select(
                    item => (FrameworkElement)goodsGrid.ItemContainerGenerator.
                    ContainerFromItem(item));
                foreach (var container in containers)
                    container.Loaded += Container_Loaded;
            }
        }

        private void Container_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (DataGridRow)sender;
            element.Loaded -= Container_Loaded;

            var contentPresenter = (ContentPresenter)goodsGrid.Columns[^1].
                GetCellContent(element);
            var template = contentPresenter?.ContentTemplate;
            if (template != null)
            {
                var __id = (TextBox)template.FindName("__id", contentPresenter);
                var __count = (TextBox)template.FindName("__count", contentPresenter);
                var btn = (Button)template.FindName("__add", contentPresenter);
                var basket = (Basket)btn.CommandParameter;
                basket.GoodID = Convert.ToInt32(__id.Text);
                if (int.TryParse(__count.Text, out var i))
                    basket.GoodCount = i;
                basket.OwnerModel = StoreModel;
            }
        }

        private void Count_TextChanged(object sender, TextChangedEventArgs e)
        {
            var contentPresenter = (ContentPresenter)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent((TextBox)sender));
            var template = contentPresenter?.ContentTemplate;
            if (template != null)
            {
                var __count = (TextBox)template.FindName("__count", contentPresenter);
                var btn = (Button)template.FindName("__add", contentPresenter);
                var basket = (Basket)btn.CommandParameter;
                if (int.TryParse(__count.Text, out var i))
                    basket.GoodCount = i;
            }
        }
    }
}
