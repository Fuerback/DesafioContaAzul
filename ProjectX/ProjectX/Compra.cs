using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX
{
    class Compra
    {
        double n_payments_purchase;
        DateTime timestamp_purchase;
        decimal price_purchase;
        string purchase_id;
        string payment_method_purchase;

        public Compra(double n_payments_purchase, DateTime timestamp_purchase, decimal price_purchase,
            string purchase_id, string payment_method_purchase)
        {
            N_payments_purchase = n_payments_purchase;
            Timestamp_purchase = timestamp_purchase;
            Price_purchase = price_purchase;
            Purchase_id = purchase_id;
            Payment_method_purchase = payment_method_purchase;
        }

        public double N_payments_purchase { get => n_payments_purchase; set => n_payments_purchase = value; }
        public DateTime Timestamp_purchase { get => timestamp_purchase; set => timestamp_purchase = value; }
        public decimal Price_purchase { get => price_purchase; set => price_purchase = value; }
        public string Purchase_id { get => purchase_id; set => purchase_id = value; }
        public string Payment_method_purchase { get => payment_method_purchase; set => payment_method_purchase = value; }
    }
}
