using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Store
{
    public partial class StoreWindow : Window
    {
        private int UserID { get; }
        internal Model StoreModel { get; private set; }

        public StoreWindow(int id)
        {
            InitializeComponent();
            StoreModel = Resources["StoreModel"] as Model;
            StoreModel.UserID = id;
            StoreModel.UpdateTotalValue();
            StoreModel.UpdateGoodsTable();
            StoreModel.UpdateBasketTable();
            UserID = id;
            goodsGrid.ItemsSource = StoreModel.Set.Tables["Goods"].DefaultView;
            basketGrid.ItemsSource = StoreModel.Set.Tables["Basket"].DefaultView;
            deliveryMethod.ItemsSource = StoreModel.Set.Tables["DeliveryMethod"].DefaultView;
            goodsGrid.ItemContainerGenerator.StatusChanged += 
                Goods_ItemContainerGenerator_StatusChanged;
            basketGrid.ItemContainerGenerator.StatusChanged += 
                Basket_ItemContainerGenerator_StatusChanged;
        }

        private void Basket_ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (goodsGrid.ItemContainerGenerator.Status ==
                System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                var containers = basketGrid.Items.Cast<object>().Select(
                    item => (FrameworkElement)basketGrid.ItemContainerGenerator.
                    ContainerFromItem(item));
                foreach (var container in containers)
                    if (container != null)
                        container.Loaded += Basket_Container_Loaded;
            }
        }

        private void Basket_Container_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (DataGridRow)sender;
            element.Loaded -= Goods_Container_Loaded;

            var contentPresenter = (ContentPresenter)basketGrid.Columns[^1].
                GetCellContent(element);
            var template = contentPresenter?.ContentTemplate;
            if (template != null)
            {
                var __id = (TextBox)template.FindName("__id", contentPresenter);
                var btn = (Button)template.FindName("__del", contentPresenter);
                if (btn?.CommandParameter is DelBasketParam basket)
                {
                    basket.ID = Convert.ToInt32(__id.Text);
                    basket.OwnerModel = StoreModel;
                }
            }
        }

        private void Goods_ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (goodsGrid.ItemContainerGenerator.Status ==
                System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                var containers = goodsGrid.Items.Cast<object>().Select(
                    item => (FrameworkElement)goodsGrid.ItemContainerGenerator.
                    ContainerFromItem(item));
                foreach (var container in containers)
                    container.Loaded += Goods_Container_Loaded;
            }
        }

        private void Goods_Container_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (DataGridRow)sender;
            element.Loaded -= Goods_Container_Loaded;

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

        private void GoodCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            using var connection = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                var contentPresenter = (ContentPresenter)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent((TextBox)sender));
                var template = contentPresenter?.ContentTemplate;
                if (template != null)
                {
                    var __count = (TextBox)template.FindName("__count", contentPresenter);
                    var __id = (TextBox)template.FindName("__id", contentPresenter);
                    if (int.TryParse(__count.Text, out var count) 
                        && int.TryParse(__id.Text, out var id)) 
                    {
                        new SqlCommand($"Update Basket Set GoodCount={count} " +
                            $"where ID={id}", connection).ExecuteNonQuery();
                    }
                }
                StoreModel.UpdateTotalValue();
            }
            catch
            {
                return;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        private void BasketGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (placeOrder.CommandParameter is PlaceOrderParam param)
            {
                param.SelectedBasketItems = ((DataGrid)sender).SelectedItems;
            }
        }

        private void DeliveryMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StoreModel.DeliveryMethod = (DataRowView)((ComboBox)sender).SelectedItem;
        }
    }
}
