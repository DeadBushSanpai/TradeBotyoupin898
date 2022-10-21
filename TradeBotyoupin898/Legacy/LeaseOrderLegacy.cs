namespace Legacy
{
    public class LeaseOrderLegacy
    {
        public int Code { get; set; }

        public string Msg { get; set; }

        public LeaseOrderDataLegacy Data { get; set; }
    }

    public class LeaseOrderDataLegacy
    {
        public string ReturnOrderNo { get; set; }
    }
}
