﻿namespace TradeBotyoupin898.APIData.Legacy
{
    public class LeaseOrder
    {
        public int Code { get; set; }

        public string Msg { get; set; }

        public LeaseOrderData Data { get; set; }
    }

    public class LeaseOrderData
    {
        public string ReturnOrderNo { get; set; }
    }
}
