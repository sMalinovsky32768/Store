using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace Store
{
    class Model : DependencyObject, INotifyPropertyChanged
    {
        private decimal totalValue;

        public int UserID { get; set; }
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
                        $"{basket.GoodID})", connection).
                        ExecuteScalar());
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
                    }
                }
                catch
                {

                }
                finally
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
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
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }

        public void UpdateTotalValue()
        {
            TotalValue = Model.UpdateTotalValue(UserID);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
