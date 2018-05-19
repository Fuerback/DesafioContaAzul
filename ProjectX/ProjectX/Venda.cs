using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX
{
    class Venda
    {
        double n_payments;
        string saleId;
        DateTime timestampSale;
        string payment_method_sale;
        Int64 product_id;

        public Venda(double n_payments, string saleId, DateTime timestampSale,
            string payment_method_sale, Int64 product_id)
        {
            N_payments = n_payments;
            SaleId = saleId;
            TimestampSale = timestampSale;
            Payment_method_sale = payment_method_sale;
            Product_id = product_id;
        }

        public double N_payments { get => n_payments; set => n_payments = value; }
        public string SaleId { get => saleId; set => saleId = value; }
        public DateTime TimestampSale { get => timestampSale; set => timestampSale = value; }
        public string Payment_method_sale { get => payment_method_sale; set => payment_method_sale = value; }
        public Int64 Product_id { get => product_id; set => product_id = value; }
    }
}
