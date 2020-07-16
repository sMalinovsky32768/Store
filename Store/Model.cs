using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;

namespace Store
{
    class Model : INotifyPropertyChanged
    {
        public event EventHandler UserIDChanged;

        private decimal totalValue;
        private int userID;
        private DataRowView deliveryMethod;

        public int UserID
        {
            get => userID;
            set
            {
                userID = value;
                UserIDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        internal bool IsAdmin { get; set; }
        internal DataRowView DeliveryMethod
        {
            get => deliveryMethod;
            set
            {
                deliveryMethod = value;
                OnPropertyChanged();
            }
        }
        internal DataSet Set { get; set; }
        public decimal TotalValue
        {
            get => totalValue;
            set
            {
                totalValue = value;
                OnPropertyChanged();
            }
        }
        public StoreCommand AddStoreCommand { get; } = new StoreCommand(
            obj =>
            {
                using var connection = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString);
                try
                {
                    connection.Open();
                    if (obj is Basket basket)
                    {
                        var basketID = Convert.ToInt32(new SqlCommand(
                        $"Select [dbo].[GetFromBasket]({basket.UserID}," +
                        $"{basket.GoodID})", connection).ExecuteScalar());
                        if (basketID != 0)
                        {
                            var count = Convert.ToInt32(new SqlCommand(
                                $"select top(1) GoodCount from Basket where ID={basketID}",
                                connection).ExecuteScalar());
                            new SqlCommand($"Update Basket set GoodCount={basket.GoodCount + count} " +
                                $"where ID={basketID}", connection).ExecuteNonQuery();
                        }
                        else
                        {
                            new SqlCommand($"Insert into Basket (UserID, GoodID, GoodCount) " +
                                $"values ({basket.UserID}, {basket.GoodID}, {basket.GoodCount})",
                                connection).ExecuteNonQuery();
                        }
                        basket.GoodCount = 0;
                        basket.OwnerModel.TotalValue = UpdateTotalValue(basket.UserID);
                        basket.OwnerModel.UpdateBasketTable();
                    }
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            },
            obj =>
            {
                if (obj is Basket basket)
                {

                    if (basket.UserID <= 0
                    || basket.GoodID <= 0
                    || basket.GoodCount <= 0)
                    {
                        return false;
                    }
                    return true;
                }
                else return false;
            });

        public StoreCommand ImportStoreCommand { get; } = new StoreCommand(
            obj =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "XML|*.xml"
                };
                if (dialog.ShowDialog() != true) return;
                using var connection = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString);
                try
                {
                    XElement element = XElement.Load(dialog.FileName);
                    var elementCollection = element.Descendants("Good");
                    var collection = elementCollection.Select(
                        item => new
                        {
                            Name = (string)item.Attribute("Name"),
                            Value = (decimal)item.Attribute("Value"),
                            Articul = (string)item.Attribute("Articul"),
                            Currency = (string)item.Attribute("Currency") ?? "RUB",
                            Producer = new
                            {
                                Name = (string)item.Element("Producer").Attribute("Name"),
                                Code = (string)item.Element("Producer").Attribute("Code"),
                            },
                            GoodType = new
                            {
                                Name = (string)item.Element("GoodType").Attribute("Name"),
                                Code = (string)item.Element("GoodType").Attribute("Code"),
                            },
                        });
                    connection.Open();
                    foreach (var item in collection)
                    {
                        var producerID = FindProducer(item.Producer.Name, item.Producer.Code);

                        if (producerID == 0)
                        {
                            new SqlCommand($"Insert into Producer (Name, Code) " +
                                $"values (N'{item.Producer.Name}', N'{item.Producer.Code}')",
                                connection).ExecuteNonQuery();
                            producerID = FindProducer(item.Producer.Name, item.Producer.Code);
                        }


                        int goodTypeID = FindGoodType(item.GoodType.Name, item.GoodType.Code);

                        if (goodTypeID == 0)
                        {
                            new SqlCommand($"Insert into GoodType (Name, Code) " +
                                $"values (N'{item.GoodType.Name}', N'{item.GoodType.Code}')",
                                connection).ExecuteNonQuery();
                            goodTypeID = FindGoodType(item.GoodType.Name, item.GoodType.Code);
                        }
                        decimal value = CBConverter.Convert(item.Value, item.Currency, "RUB");
                        var addGoodCommand = new SqlCommand($"Insert into Good " +
                            $"(Name, Articul, Value, ProducerID, GoodTypeID) values " +
                            $"(N'{item.Name}', N'{item.Articul}', {value}, " +
                            $"{producerID}, {goodTypeID})", connection);
                        addGoodCommand.ExecuteNonQuery();
                    }
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    if (obj is Model model)
                    {
                        model.UpdateGoodsTable();
                    }
                }
            },
            obj =>
            {
                if (obj is Model model)
                {
                    return model.IsAdmin;
                }
                return false;
            });

        public StoreCommand DelBasketStoreCommand { get; } = new StoreCommand(
            obj =>
            {
                using var connection = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString);
                try
                {
                    connection.Open();
                    if (obj is DelBasketParam basket)
                    {
                        if (basket.ID != 0)
                        {
                            new SqlCommand($"Delete from Basket where ID={basket.ID}",
                                connection).ExecuteNonQuery();
                        }
                        if (basket.OwnerModel != null)
                        {
                            basket.OwnerModel.UpdateBasketTable();
                            basket.OwnerModel.UpdateTotalValue();
                        }
                    }
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            });

        public StoreCommand PlaceOrderStoreCommand { get; } = new StoreCommand(
            obj =>
            {
                using var connection = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString);
                try
                {
                    connection.Open();
                    if (obj is PlaceOrderParam placeOrder)
                    {
                        foreach (DataRowView dataRowView in placeOrder.SelectedBasketItems)
                        {
                            var basketID = Convert.ToInt32(dataRowView?.Row?["ID"]);
                            var methodID = Convert.ToInt32(placeOrder?.OwnerModel?.DeliveryMethod?["ID"]);
                            var addOrder = new SqlCommand($"Insert into [Order] (BasketID, DeliveryMethodID) " +
                                $"values ({basketID}, {methodID})", connection);
                            addOrder.ExecuteNonQuery();
                        }
                        placeOrder.OwnerModel.UpdateBasketTable();
                    }
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            },
            obj =>
            {
                if (obj is PlaceOrderParam placeOrder)
                {
                    if (placeOrder.OwnerModel.DeliveryMethod != null)
                    {
                        return true;
                    }
                }
                return false;
            });

        public Model()
        {
            Set = new DataSet();
            UpdateGoodsTable();
            UpdateBasketTable();
            UpdateDeliveryMethodTable();
            UserIDChanged += Model_UserIDChanged;
        }

        private void Model_UserIDChanged(object sender, EventArgs e)
        {
            using var connection = new SqlConnection(ConfigurationManager.
                ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                IsAdmin = Convert.ToBoolean(new SqlCommand(
                    $"select top(1) IsAdmin from [User] where ID={UserID}",
                    connection).ExecuteScalar());
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        private static decimal UpdateTotalValue(int UserID)
        {
            using var connection = new SqlConnection(ConfigurationManager.
                ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                return Convert.ToDecimal(new SqlCommand(
                    $"Select [dbo].[GetBasketTotalValue]({UserID})",
                    connection).ExecuteScalar());
            }
            catch
            {
                return 0m;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        public void UpdateTotalValue()
        {
            TotalValue = UpdateTotalValue(UserID);
        }

        public void UpdateGoodsTable()
        {
            if (!Set.Tables.Contains("Goods"))
                Set.Tables.Add("Goods");
            Set.Tables["Goods"].Clear();
            new SqlDataAdapter("Select * from dbo.GetGoods()", ConfigurationManager.
                ConnectionStrings["DefaultConnection"].ConnectionString).Fill(Set, "Goods");
        }

        public void UpdateBasketTable()
        {
            if (!Set.Tables.Contains("Basket"))
                Set.Tables.Add("Basket");
            Set.Tables["Basket"].Clear();
            new SqlDataAdapter($"Select * from dbo.GetUserBasketList({UserID})", ConfigurationManager.
                ConnectionStrings["DefaultConnection"].ConnectionString).Fill(Set, "Basket");
        }

        private void UpdateDeliveryMethodTable()
        {
            if (!Set.Tables.Contains("DeliveryMethod"))
                Set.Tables.Add("DeliveryMethod");
            Set.Tables["DeliveryMethod"].Clear();
            new SqlDataAdapter($"Select * from DeliveryMethod", ConfigurationManager.
                ConnectionStrings["DefaultConnection"].ConnectionString).Fill(Set, "DeliveryMethod");
        }

        private static int FindProducer(string name, string code)
        {
            return FindGoodProperty(name, code);
        }

        private static int FindGoodType(string name, string code)
        {
            return FindGoodProperty(name, code);
        }

        private static int FindGoodProperty(string name, string code, [CallerMemberName] string func = "")
        {
            using var connection = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                var GoodTypeCommand = new SqlCommand($"select [dbo].[{func}] " +
                            $"(N'{name}', N'{code}')", connection);
                var obj = GoodTypeCommand.ExecuteScalar();
                return obj is DBNull ? 0 : Convert.ToInt32(obj);
            }
            catch
            {
                return 0;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
